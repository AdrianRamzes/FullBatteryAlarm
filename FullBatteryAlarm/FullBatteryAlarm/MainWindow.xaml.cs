using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FullBatteryAlarm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Forms.NotifyIcon _notifyIcon;
        System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer();

        private bool _notified = false;

        public MainWindow()
        {
            InitializeComponent();

            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.Icon = new System.Drawing.Icon("Main.ico");
            _notifyIcon.Visible = true;
            _notifyIcon.Click += SetWindowStateToNormal;
            _notifyIcon.DoubleClick += SetWindowStateToNormal;
            _notifyIcon.MouseDown += NotifyIcon_MouseDown;

            _timer.Interval = 5 * 1000;
            _timer.Tick += CheckBatteryLevel;

            _timer.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void CheckBatteryLevel(object sender, EventArgs e)
        {
            PowerStatus pw = SystemInformation.PowerStatus;

            v_TextBlock_State.Text = string.Format("{0} - {1}", pw.PowerLineStatus, (pw.BatteryChargeStatus != 0) ? pw.BatteryChargeStatus.ToString() : "Not charging");

            if (pw.PowerLineStatus == System.Windows.Forms.PowerLineStatus.Online)
            {
                v_TextBlock_TimeRemaining.Text = "Online";

                if (pw.BatteryLifePercent >= 1 && !_notified)
                {
                    System.Windows.MessageBox.Show("Battery is Full!");

                    _notified = true;
                }
            }
            else
            {
                _notified = false;

                if (pw.BatteryLifeRemaining >= 0)
                {
                    var timeRemaining = TimeSpan.FromSeconds(pw.BatteryLifeRemaining);

                    v_TextBlock_TimeRemaining.Text = string.Format("{0} h {1} min", timeRemaining.Hours, timeRemaining.Minutes);
                }
                else
                {
                    v_TextBlock_TimeRemaining.Text = "Calculating...";
                }
            }
        }

        private void SetWindowStateToNormal(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Normal)
            {
                this.WindowState = WindowState.Minimized;
            }
            else
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        void NotifyIcon_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                var menu = this.FindResource("NotifierContextMenu") as System.Windows.Controls.ContextMenu;
                menu.IsOpen = true;
            }
        }

        protected void Menu_Exit(object sender, RoutedEventArgs e)
        {
            _notifyIcon.Visible = false;
            this.Close();
            System.Windows.Application.Current.Shutdown();
        }
    }
}
