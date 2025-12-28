using System.Windows;
using Microsoft.Win32;

namespace Project
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Set window size after window is loaded
            this.Loaded += MainWindow_Loaded;
            
            // Handle window state changes
            this.StateChanged += MainWindow_StateChanged;
            
            // Handle display settings changes (e.g., monitor changes, resolution changes)
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            
            // Clean up event handler when window closes
            this.Closed += MainWindow_Closed;
        }

        /// <summary>
        /// Sets the window size when it's first loaded
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeWindowSize();
        }

        /// <summary>
        /// Handles window state changes (Normal, Maximized, Minimized)
        /// </summary>
        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            // When window is restored from maximized, reset to work area size
            if (this.WindowState == WindowState.Normal)
            {
                InitializeWindowSize();
            }
        }

        /// <summary>
        /// Initializes the window size to use 100% of the screen excluding the taskbar
        /// </summary>
        private void InitializeWindowSize()
        {
            // Only set size if window is in Normal state (not maximized)
            if (this.WindowState == WindowState.Normal)
            {
                // Get the work area (screen minus taskbar)
                var workArea = SystemParameters.WorkArea;
                
                // Set window size to fill the work area
                this.Width = workArea.Width;
                this.Height = workArea.Height;
                
                // Position window at the top-left of the work area
                this.Left = workArea.Left;
                this.Top = workArea.Top;
            }
        }

        /// <summary>
        /// Handles display settings changes (resolution changes, monitor changes, etc.)
        /// </summary>
        private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
        {
            // Use Dispatcher to ensure UI updates happen on the UI thread
            this.Dispatcher.Invoke(() =>
            {
                // Only resize if window is in Normal state
                if (this.WindowState == WindowState.Normal)
                {
                    InitializeWindowSize();
                }
            });
        }

        /// <summary>
        /// Clean up event handler when window closes
        /// </summary>
        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                btnMaximize.Content = "□";
                btnMaximize.ToolTip = "Maximize";
                // Window size will be set automatically by StateChanged event
            }
            else
            {
                WindowState = WindowState.Maximized;
                btnMaximize.Content = "❐";
                btnMaximize.ToolTip = "Restore Down";
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}