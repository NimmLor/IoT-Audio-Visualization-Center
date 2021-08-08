using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Un4seen.Bass;
using Un4seen.BassWasapi;

namespace Analyzer
{
    public partial class MainWindow : MetroWindow
    {
        private string loadedAudioDevice = "";
        private bool audioDeviceLoaded = false;
        private bool deviceListInitialized = false;

        public MainWindow()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(MyUtils.OnProcessExit);
            MyUtils.SetDefaultTheme();

            InitializeComponent();
            InitDevices();
            cboDevices.SelectedIndex = 0;
            Load();
            RefreshDeviceList();
            //ThreadPool.QueueUserWorkItem(new WaitCallback(delegate (object state) { LoadAudioDevice(); }), null);
            //ThreadPool.QueueUserWorkItem(new WaitCallback(delegate (object state) { LoadAudioDevice(); }), null);
        }

        public void LoadAudioDevice()
        {
            while (!deviceListInitialized || String.IsNullOrEmpty(loadedAudioDevice)) if(loadedAudioDevice == null)return;
            cboDevices.Dispatcher.Invoke(() =>
            {
                if (cboDevices.Items.Contains(loadedAudioDevice as object))
            {
                cboDevices.SelectedIndex = cboDevices.Items.IndexOf(loadedAudioDevice as object);
                MyUtils.SwitchDeviceFromString(loadedAudioDevice);
                restartSourceSpectrum();
                audioDeviceLoaded = true;
            }
            });
        }

        public void RefreshDeviceList()
        {
            icDevices.ItemsSource = null;
            foreach (UdpDevice u in MyUtils.UdpDevices) u.getWebserverInfo();
            icDevices.ItemsSource = MyUtils.UdpDevices;
            RefreshDevicesBox();
        }

        private void InitDevices()
        {
            List<string> toAdd = new List<string>();
            for (int i = 0; i < BassWasapi.BASS_WASAPI_GetDeviceCount(); i++)
                {
                    var device = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);
                    if (device.IsEnabled && (device.IsLoopback || device.IsInput)) /*Add Audio Inputs to Device List*/
                    {
                    toAdd.Add(string.Format("{0} - {1}", i, device.name));
                    }
                }
            cboDevices.Dispatcher.Invoke(() =>
            {
                cboDevices.Items.Clear();
                foreach(string s in toAdd)cboDevices.Items.Add(s);
            });
            //cboDevices.SelectedIndex = 0;
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, false);
            if (!Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero)) MessageBox.Show("Error while initializing the sound device");
            deviceListInitialized = true;
        }

        private void CboDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                spcSource.wucd.Stop();
            }
            catch
            {
            }
            string s = (sender as ComboBox).SelectedItem as string;
            MyUtils.SwitchDeviceFromString(s);
            StartWpfUserControls();
            //StartWpfUserControls();
        }

        public void restartSourceSpectrum()
        {
            try
            {
                spcSource.wucd.Stop();

            }
            catch { }
            try
            {
                spcSource.wucd.Start();
            }
            catch { }
        }

        public void StartWpfUserControls()
        {
            spcSource.wucd.Stop();
            spcSource.wucd.Start();
            foreach (Spectrum y in MyUtils.FindVisualChildren<Spectrum>(Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive)))
            {
                y.wucd.Stop();
                y.wucd.Start();
            }
        }


        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshDevicesBox();
        }

        public void RefreshDevicesBox()
        {
            foreach (DeviceControl y in MyUtils.FindVisualChildren<DeviceControl>(grpDevices)) y.RefAll();
        }

        public async void AddDeviceDialogAsync()
        {
            string devicename = "";
            string ip = "";
            int port = 0;
            int lines = 32;
            int smoothing = 0;

            var x = new MetroDialogSettings();
            x.AffirmativeButtonText = "Next";
            x.NegativeButtonText = "Cancel";
            devicename = await this.ShowInputAsync("New Device", "How should the device be called?", x);
            if (String.IsNullOrEmpty(devicename)) return;

            var exist = MyUtils.UdpDevices.Find(o => o.DeviceName == devicename);
            if (exist != null)
            {
                await this.ShowMessageAsync("Error!", "A device with that name already exists!");
                return;
            }

            x.DefaultText = "192.168.0.";
            ip = await this.ShowInputAsync("New Device", "Whats the IP-Address of your device?", x);
            while (!MyUtils.ValidateIp(ip))
            {
                await this.ShowMessageAsync("Error!", "The IP-Address that was entered is invalid!");
                ip = await this.ShowInputAsync("New Device", "Whats the IP-Address of your device?", x);
                if (String.IsNullOrEmpty(ip)) return;
            }
            var controller = await this.ShowProgressAsync("Please wait...", "Trying to reach the device");
            controller.SetIndeterminate();
            Thread.Sleep(500);
            bool success = MyUtils.IpReachable(ip);
            await controller.CloseAsync();
            if (!success)
            {
                x.AffirmativeButtonText = "Yes";
                var cont = await this.ShowMessageAsync("The device could not be reached!", "Continue anyway?", MessageDialogStyle.AffirmativeAndNegative, x);
                if (cont == MessageDialogResult.Negative) return;
            }

            string porttext = "";
            x.DefaultText = "4210";
            porttext = await this.ShowInputAsync("New Device", "On what UDP-Port should the data be sent?", x);
            if (!int.TryParse(porttext, out port)) port = -1;
            while (port <= 0)
            {
                porttext = await this.ShowInputAsync("Invalid Number!", "On what UDP-Port should the data be sent?", x);
                if (String.IsNullOrEmpty(porttext)) return;
                if (!int.TryParse(porttext, out port)) port = -1;
            }
            x.DefaultText = "32";
            string linestext = "";

            if (!success)
            {
                linestext = await this.ShowInputAsync("New Device", "How many lines of data should be sent to the device?", x);
                if (!int.TryParse(linestext, out lines)) lines = -1;
                while ((lines <= 0 || lines > 1023))
                {
                    linestext = await this.ShowInputAsync("Invalid Number!", "How many lines of data should be sent to the device?", x);
                    if (String.IsNullOrEmpty(linestext)) return;
                    if (!int.TryParse(linestext, out lines)) lines = -1;
                }
            }


            x.DefaultText = "2";
            string smtext = "";

            smtext = await this.ShowInputAsync("New Device", "How much should the spectrum be smoothed?", x);
            if (!int.TryParse(smtext, out smoothing)) smoothing = -1;
            while ((smoothing <= 0 || smoothing > 100))
            {
                smtext = await this.ShowInputAsync("Invalid Number!", "How much should the spectrum be smoothed?", x);
                if (String.IsNullOrEmpty(smtext)) return;
                if (!int.TryParse(smtext, out smoothing)) smoothing = -1;
            }

            x.AffirmativeButtonText = "Add Device";
            x.NegativeButtonText = "Cancel Device Creation";
            var res = await this.ShowMessageAsync("Confirm", "Name: " + devicename + "\nIP-Address: " + ip + "\nLines: " + lines, MessageDialogStyle.AffirmativeAndNegative, x);
            if (res == MessageDialogResult.Negative) return;

            try
            {
                UdpDevice newDev = new UdpDevice(devicename, ip, port, lines, smoothing);
                newDev.Smooth = true;
                MyUtils.UdpDevices.Add(newDev);
                Save();
                RefreshDeviceList();
                await this.ShowMessageAsync("Success!", "Device created successfully!");
            }
            catch
            {
                await this.ShowInputAsync("Error", "Something went wrong while creating the new device!");
            }


        }

        public void Save(string dev = "")
        {
            SaveObject s = new SaveObject(MyUtils.UdpDevices);
            if (String.IsNullOrEmpty(dev)) dev = MyUtils.audioDevice;
            s.audioDevice = dev;
            MyUtils.SaveToProperties(s);
        }

        private void Load()
        {
            try
            {
                SaveObject s = MyUtils.RetrieveSettings();
                if (s != null)
                {
                    if (s.udps.Count != 0) MyUtils.UdpDevices.Clear();
                    foreach (UdpDevice u in s.udps)
                    {
                        if (MyUtils.ValidateIp(u.Ip))
                        {
                            var ad = new UdpDevice(u.DeviceName, u.Ip, u.Port, u.Lines, u.Smoothing);
                            //ad.Smoothing = 0;
                            MyUtils.UdpDevices.Add(ad);
                        }
                    }
                    loadedAudioDevice = s.audioDevice;
                    Save(loadedAudioDevice);
                }
            }
            catch (Exception ex)
            {
                this.ShowMessageAsync("Error", "Loading devices failed!\n\nError message:\n" + ex.Message, MessageDialogStyle.Affirmative);
                MyUtils.UdpDevices.Clear();
            }
        }

        private void btnOTA_Click(object sender, RoutedEventArgs e)
        {
            foreach (UdpDevice u in MyUtils.UdpDevices) u.modeOTA();
        }

        private void btnAlexa_Click(object sender, RoutedEventArgs e)
        {
            foreach (UdpDevice u in MyUtils.UdpDevices) u.modeAlexa();
        }

        private void btnReboot_Click(object sender, RoutedEventArgs e)
        {
            foreach (UdpDevice u in MyUtils.UdpDevices) u.modeReboot();
        }

        private void sldSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            foreach (UdpDevice u in MyUtils.UdpDevices) u.setSpeedAsync((int)sldSpeed.Value);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddDeviceDialogAsync();
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            //Environment.Exit(0);
            //((App)Application.Current).ExitApplication();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            spcSource.TotalWidth = (int)MainWindow_Window.ActualWidth - 30;
        }

        private void BtnRefresh_Click_1(object sender, RoutedEventArgs e)
        {
            RefreshDeviceList();
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            Window s = new Settings();
            s.Show();
#else
            MessageBox.Show("Work in progress...");
#endif
        }

        private void BtnNetworkscanner_Click(object sender, RoutedEventArgs e)
        {
            Window scan = new Networkscanner();
            scan.Show();
        }

        private void sldSourceScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MyUtils.sourceFactor = sldSourceScale.Value / 100.0;
        }
    }
}
