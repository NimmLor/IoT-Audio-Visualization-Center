using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Un4seen.BassWasapi;

namespace Analyzer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private bool _isExit;

        protected override void OnStartup(StartupEventArgs e)
        {
            
            base.OnStartup(e);
            
            MainWindow = new MainWindow();
            MainWindow.Closing += MainWindow_Closing;

            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.DoubleClick += (s, args) => ShowMainWindow();
            _notifyIcon.Icon = Analyzer.Properties.Resources.AppIcon;
            _notifyIcon.Visible = true;

            CreateContextMenu2();
            ShowMainWindow();
        }

        /*
        private void CreateContextMenu()
        {
            _notifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Dashboard").Click += (s, e) => ShowMainWindow();
            _notifyIcon.ContextMenuStrip.Items.Add("Enable All").Click += (s, e) => MyUtils.EnableAll();
            _notifyIcon.ContextMenuStrip.Items.Add("Disable All").Click += (s, e) => MyUtils.DisableAll();

            ToolStrip ts = new ToolStrip();
            ToolStripDropDownButton tsddb = new ToolStripDropDownButton("device x");
            ts.Items.Add(tsddb);
            //tsddb.DropDown = _notifyIcon.ContextMenuStrip;
            

            _notifyIcon.ContextMenuStrip.Items.Add("Exit").Click += (s, e) => ExitApplication();
        }
        */

        private void CreateContextMenu2()
        {
            System.Windows.Forms.ContextMenu m = new System.Windows.Forms.ContextMenu();
            m.MenuItems.Add("Dashboard").Click += (s, e) => ShowMainWindow();
            m.MenuItems.Add("Enable All").Click += (s, e) => MyUtils.EnableAll();
            m.MenuItems.Add("Disable All").Click += (s, e) => MyUtils.DisableAll();
            List<MenuItem> mItems = new List<MenuItem>();
            for (int i = 0; i < BassWasapi.BASS_WASAPI_GetDeviceCount(); i++)
            {
                var device = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);
                if (device.IsEnabled && device.IsLoopback)
                {
                    var x = new MenuItem(string.Format("{0} - {1}", i, device.name), AudioSwitching);
                    mItems.Add(x);
                }
            }
            MenuItem mi = new MenuItem("Audio Device",mItems.ToArray());
            
            m.MenuItems.Add(mi);
            //tsddb.DropDown = _notifyIcon.ContextMenuStrip;


            m.MenuItems.Add("Exit").Click += (s, e) => ExitApplication();
            _notifyIcon.ContextMenu = m;
        }

        public void ExitApplication()
        {
            _isExit = true;
            MainWindow.Close();
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

        private void ShowMainWindow()
        {
            if (MainWindow.IsVisible)
            {
                if (MainWindow.WindowState == WindowState.Minimized)
                {
                    MainWindow.WindowState = WindowState.Normal;
                }
                MainWindow.Activate();
            }
            else
            {
                MainWindow.Show();
            }
        }

        private void AudioSwitching(object sender, EventArgs e)
        {
            var element = sender as MenuItem;
            MyUtils.SwitchDeviceFromString(element.Text);
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!_isExit)
            {
                e.Cancel = true;
                MainWindow.Hide(); // A hidden window can be shown again, a closed one not
            }
        }
    }
}