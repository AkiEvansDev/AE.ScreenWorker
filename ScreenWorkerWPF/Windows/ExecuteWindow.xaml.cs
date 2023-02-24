using System.Windows;

using AE.Core;

using ScreenBase.Data.Base;

using ScreenWorkerWPF.Common;

namespace ScreenWorkerWPF.Windows;

public partial class ExecuteWindow : Window
{
    internal class ExecuteWindowWorker : BaseExecutorWorker<ExecuteWindow>
    {
        public ExecuteWindowWorker(ExecuteWindow window, ScriptInfo scriptData, bool isDebug) : base(window, scriptData, isDebug) { }

        protected override void OnStart(ScriptInfo scriptData, bool isDebug)
        {
            var margin = App.CurrentSettings.ExecuteWindowMargin;
            switch (App.CurrentSettings.ExecuteWindowLocation)
            {
                case WindowLocation.LeftTop:
                    Window.Left = margin;
                    Window.Top = margin;
                    break;
                case WindowLocation.LeftBottom:
                    Window.Left = margin;
                    Window.Top = ScreenSize.Height - Window.Height - margin;
                    break;
                case WindowLocation.RightTop:
                    Window.Left = ScreenSize.Width - Window.Width - margin;
                    Window.Top = margin;
                    break;
                case WindowLocation.RightBottom:
                    Window.Left = ScreenSize.Width - Window.Width - margin;
                    Window.Top = ScreenSize.Height - Window.Height - margin;
                    break;
                case WindowLocation.Center:
                    Window.Left = ScreenSize.Width / 2 - Window.Width / 2 + margin;
                    Window.Top = ScreenSize.Height / 2 - Window.Height / 2 + margin;
                    break;
            }

            Window.Info.Text = $"Press Ctrl+Alt+{App.CurrentSettings.StopKey.Name().Substring(3)} to stop";

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
        Worker = new ExecuteWindowWorker(this, scriptData, isDebug);
    }
}
