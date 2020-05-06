using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

namespace Analyzer
{
    /// <summary>
    /// Interaktionslogik für DeviceControl.xaml
    /// </summary>
    public partial class DeviceControl : UserControl
    {
        public DeviceControl()
        {
            InitializeComponent();
            RefAll();
        }

        public object MyDevice
        {
            get { return (object)GetValue(MyDeviceProperty); }
            set { SetValue(MyDeviceProperty, value); }
        }

        public static readonly DependencyProperty MyDeviceProperty =
            DependencyProperty.Register("MyDevice", typeof(object), typeof(DeviceControl), new FrameworkPropertyMetadata(new UdpDevice("Meta", "192.168.0.210", 4120, 32, 2),
                 FrameworkPropertyMetadataOptions.AffectsRender,
                   new PropertyChangedCallback(OnObjectChanged)));

        public int Smoothing
        {
            get
            {
                return DeviceItem.Smoothing;
            }
        }

        public UdpDevice DeviceItem { get { return (UdpDevice)MyDevice; } }

        private static void OnObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DeviceControl devC = d as DeviceControl;
            devC.RefAll();
        }

        public void RefAll()
        {
            if (DeviceItem.DeviceName.ToLower() == "meta") return;
            lblDetails.Content = Details;
            grpDevice.Header = DeviceName;
            spcDev.Smoothing = Smoothing;
            ckbSmoothing.IsChecked = DeviceItem.Smooth;
            if (!(bool)ckbSmoothing.IsChecked) spcDev.Smoothing = 0;
            FillDropDowns();
            sld.Value = DeviceItem.brightness;
            tbtnPower.IsChecked = (DeviceItem.power == 1);
        }

        public class ddElement
        {
            public PackIconFontAwesome p = new PackIconFontAwesome();
            public string name;
            public ddElement(string n, bool isStar)
            {
                name = n;
                
                if (isStar) p.Kind = PackIconFontAwesomeKind.StarRegular;
                p.UpdateLayout();
            }
        }

        public Grid getDdGrid(UdpDevice.Pattern p)
        {
            Grid d = new Grid();
            PackIconFontAwesome s = new PackIconFontAwesome(); s.Kind = PackIconFontAwesomeKind.StarRegular;
            s.VerticalAlignment = VerticalAlignment.Center;

            if (p.name.Contains("⋆"))
            {
                d.Children.Add(s);
                Label l = new Label(); l.Content = "\t" + p.name.Replace("⋆", "");
                d.Children.Add(l);
            }
            else
            {
                Label l = new Label(); l.Content = string.Format("{0}\t{1}", p.id, p.name);
                d.Children.Add(l);
            }
            return d;
        }

        public void FillDropDowns()
        {
            ddbVisualizers.Items.Clear();
            ddbPatterns.Items.Clear();
            foreach (UdpDevice.Pattern p in DeviceItem.VisualizationPatterns)
            {
                ddbVisualizers.Items.Add(string.Format("{0}\t{1}", p.id, p.name.Replace("⋆", "")));
                //ddbVisualizers.Items.Add(getDdGrid(p));
            }
            foreach (UdpDevice.Pattern p in DeviceItem.RegularPatterns)
            {
                ddbPatterns.Items.Add(string.Format("{0}\t{1}", p.id, p.name.Replace("⋆", "")));
                //ddbPatterns.Items.Add(getDdGrid(p));
            }
            foreach (UdpDevice.Pattern p in DeviceItem.TwinklePatterns)
            {
                ddbTwinkles.Items.Add(string.Format("{0}\t{1}", p.id, p.name.Replace("⋆", "")));
                //ddbTwinkles.Items.Add(getDdGrid(p));
            }
        }

        public string DeviceName
        {
            get { return DeviceItem.DeviceName; }
        }

        public string Details
        {
            get
            {
                string host = DeviceItem.Hostname;
                if (!String.IsNullOrEmpty(host)) return "Hostname: " + host + ", IP: " + DeviceItem.Ip;
                else return "IP: " + DeviceItem.Ip;
            }
        }

        private void DeviceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.Source == null || lblBrit == null) return;
            lblBrit.Content = ((Slider)e.Source).Value.ToString() + "%";
            DeviceItem.setBrightnessAsync((int)((Slider)e.Source).Value);
        }

        private void CkbEnable_Changed(object sender, RoutedEventArgs e)
        {
            if (ckbEnable.IsChecked == true)
            {
                DeviceItem.Start();
                spcDev.enable();
            }
            else
            {
                DeviceItem.Stop();
                spcDev.disable();
            }
        }

        private void CkbSmoothing_Changed(object sender, RoutedEventArgs e)
        {
            if (ckbSmoothing.IsChecked == true)
            {
                DeviceItem.Smooth = true;
                spcDev.Smoothing = DeviceItem.Smoothing;
            }
            else
            {
                DeviceItem.Smooth = false;
                spcDev.Smoothing = 0;
            }
        }

        private void BtnWeb_Click(object sender, RoutedEventArgs e)
        {
            
            System.Diagnostics.Process.Start("http://" + DeviceItem.Ip);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            //foreach (UdpDevice u in MyUtils.UdpDevices) u.Stop();
            MainWindow mwInstance = Window.GetWindow(this) as MainWindow;
            //DeviceItem.Stop();
            foreach (UdpDevice u in MyUtils.UdpDevices) u.Stop();
            ckbEnable.IsChecked = false;
            Window w = new EditDevice(DeviceItem.DeepCopy(), mwInstance);

            w.Show();
        }

        private void dropDrop_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem m = ((MenuItem)e.Source);
                string value = m.DataContext as string;
                int num = Convert.ToInt32(value.ToString().Substring(0, value.ToString().IndexOf('\t')));
                DeviceItem.setPatternAsync(num);
                if (m.Name == "MenuVisualizers")
                {
                    ckbEnable.IsChecked = true;
                    tbtnPower.IsChecked = true;
                }
                else
                {
                    ckbEnable.IsChecked = false;
                    tbtnPower.IsChecked = true;
                }
            }
            catch(Exception ex)
            {
                ;
            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (tbtnPower.IsChecked == true)
            {
                DeviceItem.setPowerAsync(1);
            }
            else DeviceItem.setPowerAsync(0);
        }

        private void sldSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            DeviceItem.setSpeedAsync((int)sldSpeed.Value);
        }

        private void ddb_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (DeviceItem.VisualizationPatterns.Count <= 0)
            {
                if(DeviceItem.getWebserverInfo()) FillDropDowns();
            }
        }

        private void btnOTA_Click(object sender, RoutedEventArgs e)
        {
            DeviceItem.modeOTA();
        }

        private void btnAlexa_Click(object sender, RoutedEventArgs e)
        {
            DeviceItem.modeAlexa();
        }

        private void btnReboot_Click(object sender, RoutedEventArgs e)
        {
            DeviceItem.modeReboot();
        }

        private void ddb_ContextMenuOpening(object sender, RoutedEventArgs e)
        {
            if(DeviceItem.RegularPatterns.Count <=0)
            {
                if (DeviceItem.getWebserverInfo()) RefreshDevice();
            }
        }

        private void RefreshDevice()
        {
            FillDropDowns();
            sld.Value = DeviceItem.brightness;
            if (DeviceItem.power == 1) tbtnPower.IsChecked = true; else tbtnPower.IsChecked = false;
        }
    }
}
