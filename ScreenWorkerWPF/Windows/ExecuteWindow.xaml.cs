using System.Windows;
using System.Windows.Media;

using AE.Core;

using Microsoft.Web.WebView2.Wpf;

using ScreenBase.Data.Base;

using ScreenWorkerWPF.Common;

namespace ScreenWorkerWPF.Windows;

public partial class ExecuteWindow : Window
{
    internal class ExecuteWindowWorker : BaseExecutorWorker<ExecuteWindow>
    {
        public ExecuteWindowWorker(ExecuteWindow window, WebView2 web, ScriptInfo scriptData, bool isDebug) : base(window, web, scriptData, isDebug) { }

        protected override void OnStart(ScriptInfo scriptData, bool isDebug)
        {
            var marginLeft = App.CurrentSettings.ExecuteWindowMarginLeft;
            var marginTop = App.CurrentSettings.ExecuteWindowMarginTop;
            switch (App.CurrentSettings.ExecuteWindowLocation)
            {
                case WindowLocation.LeftTop:
                    Window.Left = marginLeft;
                    Window.Top = marginTop;
                    break;
                case WindowLocation.LeftBottom:
                    Window.Left = marginLeft;
                    Window.Top = ScreenSize.Height - Window.Height - marginTop;
                    break;
                case WindowLocation.RightTop:
                    Window.Left = ScreenSize.Width - Window.Width - marginLeft;
                    Window.Top = marginTop;
                    break;
                case WindowLocation.RightBottom:
                    Window.Left = ScreenSize.Width - Window.Width - marginLeft;
                    Window.Top = ScreenSize.Height - Window.Height - marginTop;
                    break;
                case WindowLocation.Center:
                    Window.Left = ScreenSize.Width / 2 - Window.Width / 2 + marginLeft;
                    Window.Top = ScreenSize.Height / 2 - Window.Height / 2 + marginTop;
                    break;
            }

            var color = Color.FromArgb(
                App.CurrentSettings.ExecuteWindowColor.A,
                App.CurrentSettings.ExecuteWindowColor.R,
                App.CurrentSettings.ExecuteWindowColor.G,
                App.CurrentSettings.ExecuteWindowColor.B
            );
            Window.Background = new SolidColorBrush(color);

            Window.Info.Text = $"Press Ctrl+Alt+{App.CurrentSettings.StopKey.Name()[3..]} to stop";

            base.OnStart(scriptData, isDebug);
        }

        protected override void OnMessage(string message, bool needDisplay)
        {
            base.OnMessage(message, needDisplay);

            if (needDisplay)
                Application.Current.Dispatcher.Invoke(() =>
                {
                    FormattedTextBlockBehavior.SetFormattedText(Window.Display, message.Trim());
                });
        }
    }

    internal static ExecuteWindowWorker Worker { get; private set; }

    public ExecuteWindow(ScriptInfo scriptData, bool isDebug)
    {
        InitializeComponent();
        Worker = new ExecuteWindowWorker(this, Web, scriptData, isDebug);
    }
}
