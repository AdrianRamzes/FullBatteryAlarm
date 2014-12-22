using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Reflection;
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
        private static readonly int _interval = 5 * 1000;// in milliseconds

        private static readonly string _batteryIcon_100 = "FullBatteryAlarm.Icons.100_percent-50.ico";
        private static readonly string _batteryIcon_90 = "FullBatteryAlarm.Icons.90_percent-50.ico";
        private static readonly string _batteryIcon_75 = "FullBatteryAlarm.Icons.75_percent-50.ico";
        private static readonly string _batteryIcon_50 = "FullBatteryAlarm.Icons.50_percent-50.ico";
        private static readonly string _batteryIcon_25 = "FullBatteryAlarm.Icons.25_percent-50.ico";
        private static readonly string _batteryIcon_10 = "FullBatteryAlarm.Icons.10_percent-50.ico";
        private static readonly string _batteryIcon_0 = "FullBatteryAlarm.Icons.0_percent-50.ico";


        System.Windows.Forms.NotifyIcon _notifyIcon;
        System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer();


        private bool _notified = false;

        public MainWindow()
        {
            InitializeComponent();

            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.Icon = GetIconFromResource(_batteryIcon_100);
            _notifyIcon.Visible = true;
            _notifyIcon.Click += SetWindowStateToNormal;
            _notifyIcon.DoubleClick += SetWindowStateToNormal;
            _notifyIcon.MouseDown += NotifyIcon_MouseDown;

            _timer.Interval = _interval;
            _timer.Tick += _timer_Tick;

            _timer.Start();

            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;

            CheckBatteryLevel();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            CheckBatteryLevel();
        }

        private void CheckBatteryLevel()
        {
            PowerStatus pw = SystemInformation.PowerStatus;

            v_TextBlock_Status.Text = string.Format("{0} - {1}", pw.PowerLineStatus, (pw.BatteryChargeStatus != 0) ? pw.BatteryChargeStatus.ToString() : "Not charging");

            var batteryLifePercent = (pw.BatteryLifePercent * 100).ToString();

            SetNotifyIcon(pw.BatteryLifePercent);

            if (pw.PowerLineStatus == System.Windows.Forms.PowerLineStatus.Online)
            {
                v_TextBlock_TimeRemaining.Text = string.Format("Online  ({0} %)", batteryLifePercent);

                if (pw.BatteryLifePercent >= 1 && !_notified)
                {
                    _notified = true;
                    System.Media.SystemSounds.Beep.Play();
                    System.Windows.MessageBox.Show("Battery is Full!");
                }
            }
            else
            {
                _notified = false;

                if (pw.BatteryLifeRemaining >= 0)
                {
                    var timeRemaining = TimeSpan.FromSeconds(pw.BatteryLifeRemaining);

                    v_TextBlock_TimeRemaining.Text = string.Format("{0} h {1} min  ({2} %)", timeRemaining.Hours, timeRemaining.Minutes, batteryLifePercent);
                }
                else
                {
                    v_TextBlock_TimeRemaining.Text = string.Format("Calculating...  ({0} %)", batteryLifePercent);

                }
            }

            _notifyIcon.Text = v_TextBlock_TimeRemaining.Text;
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

        private void SetNotifyIcon(float batteryLifePercent)
        {
            if (batteryLifePercent >= 1)
            {
                _notifyIcon.Icon = GetIconFromResource(_batteryIcon_100);
            }
            else if (batteryLifePercent >= 0.90)
            {
                _notifyIcon.Icon = GetIconFromResource(_batteryIcon_90);
            }
            else if (batteryLifePercent >= 0.75)
            {
                _notifyIcon.Icon = GetIconFromResource(_batteryIcon_75);
            }
            else if (batteryLifePercent >= 0.50)
            {
                _notifyIcon.Icon = GetIconFromResource(_batteryIcon_50);
            }
            else if (batteryLifePercent >= 0.25)
            {
                _notifyIcon.Icon = GetIconFromResource(_batteryIcon_25);
            }
            else if (batteryLifePercent >= 0.10)
            {
                _notifyIcon.Icon = GetIconFromResource(_batteryIcon_10);
            }
            else
            {
                _notifyIcon.Icon = GetIconFromResource(_batteryIcon_0);
            }
        }

        private Icon GetIconFromResource(string resourceName)
        {
            return new System.Drawing.Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName));
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        private void NotifyIcon_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
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
