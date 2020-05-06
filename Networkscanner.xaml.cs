using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Analyzer
{
    public partial class Networkscanner : MetroWindow
    {
        DispatcherTimer timer;
        int ScanCount;
        int btnState = 0;
        List<ManualResetEvent> doneEvents = new List<ManualResetEvent>();

        public Networkscanner()
        {
            InitializeComponent();
            dgDevices.ItemsSource = MyUtils.NetworkDevices;
            //ThreadPool.SetMaxThreads(2, 2);
        }



        public bool InputCheck(string a, string b)
        {
            bool success = true;
            int frm = 0;
            int end = 0;
            try
            {
                if (a.Substring(0, a.LastIndexOf('.')) != b.Substring(0, a.LastIndexOf('.')))
                    success = false;

                if (a.Count(f => f == '.') != b.Count(f => f == '.'))
                    success = false;

                for (int i = 0; i <= 1; i++)
                {
                    string input;
                    IPAddress address;
                    if (i == 0) input = a; else input = b;
                    if (IPAddress.TryParse(input, out address))
                    {
                        switch (address.AddressFamily)
                        {
                            case System.Net.Sockets.AddressFamily.InterNetwork:

                                break;
                            case System.Net.Sockets.AddressFamily.InterNetworkV6:
                                success = false;
                                MessageBox.Show("Unfortunatly IPv6 is not supported.");
                                break;
                            default:
                                success = false;
                                break;
                        }
                    }
                }
                frm = int.Parse(a.Substring(a.LastIndexOf('.') + 1));
                end = int.Parse(b.Substring(b.LastIndexOf('.') + 1));
                if (end < frm) success = false;
            }
            catch (Exception ex)
            {
                success = false;
            }
            return success;
        }

        public void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(20);
            timer.Tick += timer_Tick;
            timer.Start();
        }
        void timer_Tick(object sender, EventArgs e)
        {
            dgDevices.ItemsSource = null;
            dgDevices.ItemsSource = MyUtils.NetworkDevices;
            bool isDone = true;
            int removedDevs = 0;
            try
            {
                foreach (MyUtils.NetworkDevice test in MyUtils.NetworkDevices)
                {
                    if (test.ping < 0) isDone = false;
                    else if (test.allResponse == null) isDone = false;
                    if (isDone == false) break;
                }
                removedDevs += RemoveEmptyDevices();
            }
            catch (Exception ex)
            {
                isDone = false;
            }
            if (ScanCount == null) ScanCount = 1000;
            // button Text:
            string dots = "";
            for (int i = 0; i < btnState; i++) dots += '.';
            btnState++; if (btnState > 4) btnState = 0;
            btnScan.Dispatcher.Invoke(() =>
            {
                btnScan.Content = "Scanning" + dots;
                btnScan.IsEnabled = false;
            });
            if (isDone && (MyUtils.NetworkDevices.Count + removedDevs) >= ScanCount)
            {
                timer.Stop();
                btnScan.Dispatcher.Invoke(() =>
                {
                    btnScan.Content = "Start Networkscan";
                    btnScan.IsEnabled = true;
                });
                btnCancel.Dispatcher.Invoke(() =>
                {
                    btnCancel.IsEnabled = false;
                });
            }
        }
        public List<MyUtils.NetworkDevice> FindDevs(string ipFrom, string ipTo, DataGrid dg, Button btn) // 
        {
            List<MyUtils.NetworkDevice> dev = new List<MyUtils.NetworkDevice>();


            int lastF = ipFrom.LastIndexOf(".");
            int lastT = ipTo.LastIndexOf(".");
            string frm = ipFrom.Substring(lastF + 1);
            string tto = ipTo.Substring(lastT + 1);

            btn.Dispatcher.Invoke(() =>
            {
                btn.Content = "Scanning";
            });
            for (int i = int.Parse(frm); i <= int.Parse(tto); i++)
            {
                try
                {
                    string address = ipTo.Substring(0, lastT + 1);
                    System.Diagnostics.Debug.WriteLine(ipTo.Substring(0, lastT + 1) + i);

                    doneEvents.Add(new ManualResetEvent(false));
                    MyUtils.NetworkDevice nd = new MyUtils.NetworkDevice(address + i, doneEvents[doneEvents.Count - 1]);

                    ThreadPool.QueueUserWorkItem(nd.ThreadPoolCallback);


                    dev.Add(nd);
                }
                catch (SocketException ex)
                {

                }
                catch (Exception ex)
                {

                }
            }
            return dev;
        }

        public static int RemoveEmptyDevices(bool removeUnscanned = false)
        {
            List<MyUtils.NetworkDevice> toRemove = new List<MyUtils.NetworkDevice>();

            foreach (MyUtils.NetworkDevice n in MyUtils.NetworkDevices)
            {
                if (n.ping == -1) toRemove.Add(n);
                else if (removeUnscanned && n.ping < 0) toRemove.Add(n);
            }
            int ret = toRemove.Count;
            foreach (MyUtils.NetworkDevice r in toRemove)
            {
                MyUtils.NetworkDevices.Remove(r);
            }
            return ret;
        }

        // Buttons
        private void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            if (!InputCheck(IPstart.Text, IPend.Text))
            {
                MessageBox.Show("Invalid input");
                return;
            }
            dgDevices.ItemsSource = MyUtils.NetworkDevices;
            btnScan.IsEnabled = false;
            btnCancel.IsEnabled = true;
            StartTimer();
            try
            {
                MyUtils.NetworkDevices = FindDevs(IPstart.Text, IPend.Text, dgDevices, btnScan);
            }
            catch (Exception ex) { MessageBox.Show("An error occured while scanning!\n\n\nError message: " + ex.Message); }
        }

        private void BtnRef_Click(object sender, RoutedEventArgs e)
        {
            dgDevices.ItemsSource = null;
            dgDevices.ItemsSource = MyUtils.NetworkDevices;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            foreach (ManualResetEvent mre in doneEvents)
            {
                mre.Set();
            }
            RemoveEmptyDevices(true);
            timer.Stop();
            dgDevices.ItemsSource = null;
            dgDevices.ItemsSource = MyUtils.NetworkDevices;
            btnScan.IsEnabled = true;
            btnScan.Content = "Start Networkscan";
            btnCancel.IsEnabled = false;
        }

        private void BtnOpenWeb_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://" + (sender as Button).CommandParameter.ToString());
        }

        private void btnAddDevice_Click(object sender, RoutedEventArgs e)
        {
            AddDeviceDialogAsync((sender as Button).CommandParameter.ToString());
        }

        public async void AddDeviceDialogAsync(string ipaddr)
        {
            string devicename = "";
            string ip = ipaddr;
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
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(MainWindow))
                    {
                        (window as MainWindow).Save();
                        (window as MainWindow).RefreshDeviceList();
                    }
                }
                await this.ShowMessageAsync("Success!", "Device created successfully!");
            }
            catch
            {
                await this.ShowInputAsync("Error", "Something went wrong while creating the new device!");
            }
        }
    }
}
