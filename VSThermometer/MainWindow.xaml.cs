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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace VSThermometer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// inspiration https://github.com/VernierST/GoTweet/blob/master/GoDeviceConnect/Program.cs
    /// </summary>
    public partial class MainWindow : Window
    {
        GoIOThermometer _thermometer;
        const int interval = 100; //milliseconds

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                _thermometer = new();
                _thermometer.StartMeasurement();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }

            DispatcherTimer timer = new();
            timer.Interval = TimeSpan.FromMilliseconds(interval);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            //AllVals.Text = string.Join("\n", _thermometer.GetAllMeasurements());

            double? temperature = _thermometer.GetMeasurement();
            if (temperature != null)
            {
                Display.Text = $"{temperature:0.0}  °C";
            }
        }
    }
}
