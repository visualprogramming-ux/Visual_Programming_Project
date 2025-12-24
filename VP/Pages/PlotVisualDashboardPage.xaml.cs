using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Project.Pages
{
    /// <summary>
    /// Interaction logic for PlotVisualDashboardPage.xaml
    /// </summary>
    public partial class PlotVisualDashboardPage : Page
    {
        public class PlotItem
        {
            public string PlotNo { get; set; }
            public string Status { get; set; }
            public string Size { get; set; }
            public string ProjectName { get; set; }
            public Brush StatusColor
            {
                get
                {
                    return Status switch
                    {
                        "Available" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")),
                        "Reserved" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")),
                        "Sold" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")),
                        "Booked" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3")),
                        _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"))
                    };
                }
            }
            public Brush TextColor => Status == "Available" || Status == "Booked" ? Brushes.White : Brushes.Black;
        }

        private List<PlotItem> _allPlots;

        public PlotVisualDashboardPage()
        {
            InitializeComponent();
            LoadProjects();
            LoadSampleData();
            LoadPlots();
        }

        private void LoadProjects()
        {
            var projects = new List<string> { "All Projects", "Sunset Residency", "Green Valley", "City Center Plaza" };
            cmbProject.Items.Clear();
            cmbProject.ItemsSource = projects;
            cmbProject.SelectedIndex = 0;
            
            cmbStatusFilter.ItemsSource = new List<string> { "All Status", "Available", "Reserved", "Sold", "Booked" };
            cmbStatusFilter.SelectedIndex = 0;
        }

        private void LoadSampleData()
        {
            _allPlots = new List<PlotItem>
            {
                new PlotItem { PlotNo = "P001", Status = "Available", Size = "5", ProjectName = "Sunset Residency" },
                new PlotItem { PlotNo = "P002", Status = "Sold", Size = "7", ProjectName = "Sunset Residency" },
                new PlotItem { PlotNo = "P003", Status = "Available", Size = "10", ProjectName = "Green Valley" },
                new PlotItem { PlotNo = "P004", Status = "Reserved", Size = "3", ProjectName = "City Center Plaza" },
                new PlotItem { PlotNo = "P005", Status = "Available", Size = "8", ProjectName = "Sunset Residency" },
                new PlotItem { PlotNo = "P006", Status = "Booked", Size = "6", ProjectName = "Green Valley" },
                new PlotItem { PlotNo = "P007", Status = "Available", Size = "12", ProjectName = "City Center Plaza" },
                new PlotItem { PlotNo = "P008", Status = "Sold", Size = "15", ProjectName = "Sunset Residency" },
                new PlotItem { PlotNo = "P009", Status = "Available", Size = "4", ProjectName = "Green Valley" },
                new PlotItem { PlotNo = "P010", Status = "Reserved", Size = "9", ProjectName = "City Center Plaza" },
                new PlotItem { PlotNo = "P011", Status = "Available", Size = "20", ProjectName = "Sunset Residency" },
                new PlotItem { PlotNo = "P012", Status = "Booked", Size = "25", ProjectName = "Green Valley" },
                new PlotItem { PlotNo = "P013", Status = "Available", Size = "14", ProjectName = "City Center Plaza" },
                new PlotItem { PlotNo = "P014", Status = "Sold", Size = "30", ProjectName = "Sunset Residency" },
                new PlotItem { PlotNo = "P015", Status = "Available", Size = "18", ProjectName = "Green Valley" }
            };
        }

        private void LoadPlots()
        {
            var filteredPlots = _allPlots.AsEnumerable();

            if (cmbProject.SelectedItem != null && cmbProject.SelectedItem.ToString() != "All Projects")
            {
                filteredPlots = filteredPlots.Where(p => p.ProjectName == cmbProject.SelectedItem.ToString());
            }

            if (cmbStatusFilter.SelectedItem != null && cmbStatusFilter.SelectedItem.ToString() != "All Status")
            {
                filteredPlots = filteredPlots.Where(p => p.Status == cmbStatusFilter.SelectedItem.ToString());
            }

            plotGrid.ItemsSource = filteredPlots.ToList();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadPlots();
        }

        private void CmbProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && cmbProject.SelectedItem != null)
                LoadPlots();
        }

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && cmbStatusFilter.SelectedItem != null)
                LoadPlots();
        }
    }
}

