using System;

namespace ScreenBase;

[Flags]
public enum KeyFlags
{
    Key0 = 0x30,
    Key1 = 0x31,
    Key2 = 0x32,
    Key3 = 0x33,
    Key4 = 0x34,
    Key5 = 0x35,
    Key6 = 0x36,
    Key7 = 0x37,
    Key8 = 0x38,
    Key9 = 0x39,
    KeyA = 0x41,
    KeyB = 0x42,
    KeyC = 0x43,
    KeyD = 0x44,
    KeyE = 0x45,
    KeyF = 0x46,
    KeyG = 0x47,
    KeyH = 0x48,
    KeyI = 0x49,
    KeyJ = 0x4A,
    KeyK = 0x4B,
    KeyL = 0x4C,
    KeyM = 0x4D,
    KeyN = 0x4E,
    KeyO = 0x4F,
    KeyP = 0x50,
    KeyQ = 0x51,
    KeyR = 0x52,
    KeyS = 0x53,
    KeyT = 0x54,
    KeyU = 0x55,
    KeyV = 0x56,
    KeyW = 0x57,
    KeyX = 0x58,
    KeyY = 0x59,
    KeyZ = 0x5A,
    KeySpace = 0x20,
    KeyEnter = 0x0D,
    KeyDelete = 0x2E,
    KeyBackspace = 0x08,
    KeyTab = 0x09,
    KeyCapsLock = 0x14,
    KeyLeftAlt = 0xA4,
    KeyLeftShift = 0xA0,
    KeyRightAlt = 0xA5,
    KeyRightShift = 0xA1,
    KeyArrowDown = 0x28,
    KeyArrowUp = 0x26,
    KeyArrowLeft = 0x25,
    KeyArrowRight = 0x27,
    KeyEsc = 0x1B,
}

[Flags]
public enum MouseEventFlags
{
    LeftDown = 0x00000002,
    LeftUp = 0x00000004,
    MiddleDown = 0x00000020,
    MiddleUp = 0x00000040,
    Move = 0x00000001,
    Absolute = 0x00008000,
    RightDown = 0x00000008,
    RightUp = 0x00000010
}

public enum MouseEventType
{
    Left,
    Middle,
    Right
}
