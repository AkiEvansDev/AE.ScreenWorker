using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows;

using Microsoft.Web.WebView2.Wpf;

using ScreenBase.Data.Base;

using ScreenWindows;

using Color = System.Drawing.Color;
using Image = System.Drawing.Image;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Size = System.Drawing.Size;

namespace ScreenWorkerWPF.Windows;

public partial class DisplayWindow : Window
{
    internal class DisplayWindowWorker : BaseExecutorWorker<DisplayWindow>
    {
        private readonly List<IAction> Data;
        private ISetupDisplayWindowAction Info;
        private Bitmap Bitmap;

        public DisplayWindowWorker(DisplayWindow window, WebView2 web, ScriptInfo scriptData, bool isDebug) : base(window, web, scriptData, isDebug)
        {
            Data = new List<IAction>();
        }

        protected override void OnStart(ScriptInfo scriptData, bool isDebug)
        {
            Executor.SetupDisplayWindow = OnSetup;
            Executor.AddDisplayVariable = OnAddData;
            Executor.AddDisplayImage = OnAddData;
            Executor.UpdateDisplay = Update;
            Executor.OnVariableChange += OnExecutorVariableChange;

            base.OnStart(scriptData, isDebug);
        }

        protected override void OnStop()
        {
            Executor.OnVariableChange -= OnExecutorVariableChange;

            base.OnStop();
        }

        private void OnSetup(ISetupDisplayWindowAction action)
        {
            Info = action;
            Bitmap = new Bitmap(action.Width, action.Height, PixelFormat.Format32bppArgb);

            Update();

            Application.Current.Dispatcher.Invoke(() =>
            {
                Window.Width = action.Width;
                Window.Height = action.Height;

                switch (action.DisplayWindowLocation)
                {
                    case WindowLocation.LeftTop:
                        Window.Left = action.Left;
                        Window.Top = action.Top;
                        break;
                    case WindowLocation.LeftBottom:
                        Window.Left = action.Left;
                        Window.Top = ScreenSize.Height - Window.Height - action.Top;
                        break;
                    case WindowLocation.RightTop:
                        Window.Left = ScreenSize.Width - Window.Width - action.Left;
                        Window.Top = action.Top;
                        break;
                    case WindowLocation.RightBottom:
                        Window.Left = ScreenSize.Width - Window.Width - action.Left;
                        Window.Top = ScreenSize.Height - Window.Height - action.Top;
                        break;
                    case WindowLocation.Center:
                        Window.Left = ScreenSize.Width / 2 - Window.Width / 2 + action.Left;
                        Window.Top = ScreenSize.Height / 2 - Window.Height / 2 + action.Top;
                        break;
                }
            });
        }

        private void OnAddData(IAction action)
        {
            Data.Add(action);
        }

        private void OnExecutorVariableChange(string name, object newValue)
        {
            if (Data
                .OfType<IAddDisplayVariableAction>()
                .Where(d => d.UpdateOnVariableChange)
                .Any(d => d.Variable == name || d.ColorVariable == name)
                )
            {
                Update();
            }
        }

        private void Update()
        {
            using var g = Graphics.FromImage(Bitmap);

            g.Clear(Color.Transparent);

            if (Info.Opacity > 0)
            {
                var color = Color.FromArgb(Info.Opacity, Info.ColorPoint.GetColor());
                using var path = RoundedRect(new Rectangle(0, 0, Info.Width, Info.Height), Info.Round);

                g.FillPath(new SolidBrush(color), path);
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            foreach (var data in Data)
            {
                if (data is IAddDisplayVariableAction vAction)
                {
                    var color = Executor.GetValue(vAction.ColorPoint.GetColor(), vAction.ColorVariable);
                    g.DrawString(
                        $"{vAction.Title}{Executor.GetValue("", vAction.Variable)}",
                        new Font(vAction.FontFamily, vAction.FontSize, (System.Drawing.FontStyle)vAction.FontStyle),
                        new SolidBrush(Color.FromArgb(vAction.Opacity, color)),
                        new PointF(vAction.Left, vAction.Top)
                    );
                }
                else if (data is IAddDisplayImageAction iAction)
                {
                    var bytes = Convert.FromBase64String(iAction.Image);
                    using var stream = new MemoryStream();

                    stream.Write(bytes, 0, bytes.Length);
                    stream.Position = 0;

                    g.DrawImage(Image.FromStream(stream), iAction.Left, iAction.Top, iAction.Width, iAction.Height);
                }
            }

            Window.Dispatcher.Invoke(() =>
            {
                Window.Opacity = 1;
                WindowsHelper.SelectBitmap(Handle, Bitmap, (int)Window.Left, (int)Window.Top);
            });
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            var size = new Size(diameter, diameter);
            var arc = new Rectangle(bounds.Location, size);
            var path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(arc, 180, 90);

            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }

    internal static DisplayWindowWorker Worker { get; private set; }

    public DisplayWindow(ScriptInfo scriptData, bool isDebug)
    {
        InitializeComponent();
        Worker = new DisplayWindowWorker(this, Web, scriptData, isDebug);
    }
}
