using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ScreenWindows;

public class GlobalKeyboardHookEventArgs : HandledEventArgs
{
    public GlobalKeyboardHook.KeyboardState KeyboardState { get; private set; }
    public GlobalKeyboardHook.LowLevelKeyboardInputEvent KeyboardData { get; private set; }

    public GlobalKeyboardHookEventArgs(GlobalKeyboardHook.LowLevelKeyboardInputEvent keyboardData, GlobalKeyboardHook.KeyboardState keyboardState)
    {
        KeyboardData = keyboardData;
        KeyboardState = keyboardState;
    }
}

public class GlobalKeyboardHook : IDisposable
{
    public static readonly GlobalKeyboardHook Current;

    static GlobalKeyboardHook()
    {
        Current = new GlobalKeyboardHook();
    }

    public event EventHandler<GlobalKeyboardHookEventArgs> KeyboardPressed;

    private IntPtr windowsHookHandle;
    private IntPtr user32LibraryHandle;
    private HookProc hookProc;

    #region User32

    delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    public const int WH_KEYBOARD_LL = 13;
    public const int VkSnapshot = 0x2c;
    private const int KfAltdown = 0x2000;
    public const int LlkhfAltdown = (KfAltdown >> 8);

    [DllImport("kernel32.dll")]
    private static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    private static extern bool FreeLibrary(IntPtr hModule);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool UnhookWindowsHookEx(IntPtr hHook);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr CallNextHookEx(IntPtr hHook, int code, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    public struct LowLevelKeyboardInputEvent
    {
        public int VirtualCode;
        public int HardwareScanCode;
        public int Flags;
        public int TimeStamp;
        public IntPtr AdditionalInformation;
    }

    public enum KeyboardState
    {
        KeyDown = 0x0100,
        KeyUp = 0x0101,
        SysKeyDown = 0x0104,
        SysKeyUp = 0x0105
    }

    public IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        bool fEatKeyStroke = false;

        var wparamTyped = wParam.ToInt32();
        if (Enum.IsDefined(typeof(KeyboardState), wparamTyped))
        {
            object o = Marshal.PtrToStructure(lParam, typeof(LowLevelKeyboardInputEvent));
            LowLevelKeyboardInputEvent p = (LowLevelKeyboardInputEvent)o;

            var eventArguments = new GlobalKeyboardHookEventArgs(p, (KeyboardState)wparamTyped);

            EventHandler<GlobalKeyboardHookEventArgs> handler = KeyboardPressed;
            handler?.Invoke(this, eventArguments);

            fEatKeyStroke = eventArguments.Handled;
        }

        return fEatKeyStroke ? (IntPtr)1 : CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
    }

    #endregion

    public GlobalKeyboardHook()
    {
        windowsHookHandle = IntPtr.Zero;
        user32LibraryHandle = IntPtr.Zero;
        hookProc = LowLevelKeyboardProc;

        user32LibraryHandle = LoadLibrary("user32.dll");
        if (user32LibraryHandle == IntPtr.Zero)
        {
            int errorCode = Marshal.GetLastWin32Error();
            throw new Win32Exception(errorCode, $"Failed to load library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
        }

        windowsHookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, user32LibraryHandle, 0);
        if (windowsHookHandle == IntPtr.Zero)
        {
            int errorCode = Marshal.GetLastWin32Error();
            throw new Win32Exception(errorCode, $"Failed to adjust keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (windowsHookHandle != IntPtr.Zero)
            {
                if (!UnhookWindowsHookEx(windowsHookHandle))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode, $"Failed to remove keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                }

                windowsHookHandle = IntPtr.Zero;
                hookProc -= LowLevelKeyboardProc;
            }
        }

        if (user32LibraryHandle != IntPtr.Zero)
        {
            if (!FreeLibrary(user32LibraryHandle))
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode, $"Failed to unload library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
            }

            user32LibraryHandle = IntPtr.Zero;
        }
    }

    ~GlobalKeyboardHook()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

