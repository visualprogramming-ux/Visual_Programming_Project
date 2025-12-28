using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Project.Pages
{
    /// <summary>
    /// Interaction logic for WelcomePage.xaml
    /// </summary>
    public partial class WelcomePage : Page
    {
        private DispatcherTimer _timer;

        public WelcomePage()
        {
            InitializeComponent();
            
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(2);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _timer.Stop();
            NavigationService?.Navigate(new LoginPage());
        }
    }
}

