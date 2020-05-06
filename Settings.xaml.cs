using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Analyzer
{
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void DeviceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.Source == null || lblSmoothing == null) return;
            lblSmoothing.Content = ((Slider)e.Source).Value.ToString();
        }
    }
}
