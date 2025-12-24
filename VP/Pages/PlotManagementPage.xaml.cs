using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Project.Pages
{
    /// <summary>
    /// Interaction logic for PlotManagementPage.xaml
    /// </summary>
    public partial class PlotManagementPage : Page
    {
        public class Plot
        {
            public string PlotNo { get; set; }
            public string ProjectName { get; set; }
            public string Size { get; set; }
            public string Price { get; set; }
            public string Status { get; set; }
            public string PlotType { get; set; }
            public string Description { get; set; }
        }

        private List<Plot> _plots;
        private List<string> _projects;

        public PlotManagementPage()
        {
            InitializeComponent();
            LoadProjects();
            LoadSampleData();
        }

        private void LoadProjects()
        {
            _projects = new List<string> { "Sunset Residency", "Green Valley", "City Center Plaza" };
            cmbProject.Items.Clear();
            cmbProject.ItemsSource = _projects;
            cmbStatus.ItemsSource = new List<string> { "Available", "Reserved", "Sold", "Booked" };
            cmbPlotType.ItemsSource = new List<string> { "Residential", "Commercial", "Mixed", "Agricultural" };
        }

        private void LoadSampleData()
        {
            _plots = new List<Plot>
            {
                new Plot { PlotNo = "P001", ProjectName = "Sunset Residency", Size = "1500", Price = "2500000", Status = "Available", PlotType = "Residential", Description = "Corner plot" },
                new Plot { PlotNo = "P002", ProjectName = "Sunset Residency", Size = "1800", Price = "3000000", Status = "Sold", PlotType = "Residential", Description = "Near main road" },
                new Plot { PlotNo = "P003", ProjectName = "Green Valley", Size = "2000", Price = "3500000", Status = "Available", PlotType = "Commercial", Description = "Prime location" },
                new Plot { PlotNo = "P004", ProjectName = "City Center Plaza", Size = "1200", Price = "2200000", Status = "Reserved", PlotType = "Mixed", Description = "Near shopping center" }
            };

            dgPlots.ItemsSource = _plots;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPlotNo.Text) || cmbProject.SelectedItem == null)
            {
                MessageBox.Show("Please enter Plot Number and select Project.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existingPlot = _plots.FirstOrDefault(p => p.PlotNo == txtPlotNo.Text);
            if (existingPlot != null)
            {
                // Update existing
                existingPlot.ProjectName = cmbProject.SelectedItem.ToString();
                existingPlot.Size = txtSize.Text;
                existingPlot.Price = txtPrice.Text;
                existingPlot.Status = cmbStatus.SelectedItem?.ToString() ?? "Available";
                existingPlot.PlotType = cmbPlotType.SelectedItem?.ToString() ?? "Residential";
                existingPlot.Description = txtDescription.Text;
                MessageBox.Show("Plot updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Add new
                var newPlot = new Plot
                {
                    PlotNo = txtPlotNo.Text,
                    ProjectName = cmbProject.SelectedItem.ToString(),
                    Size = txtSize.Text,
                    Price = txtPrice.Text,
                    Status = cmbStatus.SelectedItem?.ToString() ?? "Available",
                    PlotType = cmbPlotType.SelectedItem?.ToString() ?? "Residential",
                    Description = txtDescription.Text
                };
                _plots.Add(newPlot);
                MessageBox.Show("Plot added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            dgPlots.ItemsSource = null;
            dgPlots.ItemsSource = _plots;
            BtnClear_Click(sender, e);
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            txtPlotNo.Clear();
            cmbProject.SelectedIndex = -1;
            txtSize.Clear();
            txtPrice.Clear();
            cmbStatus.SelectedIndex = -1;
            cmbPlotType.SelectedIndex = -1;
            txtDescription.Clear();
            dgPlots.SelectedItem = null;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgPlots.SelectedItem is Plot selectedPlot)
            {
                var result = MessageBox.Show($"Are you sure you want to delete plot '{selectedPlot.PlotNo}'?", 
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    _plots.Remove(selectedPlot);
                    dgPlots.ItemsSource = null;
                    dgPlots.ItemsSource = _plots;
                    BtnClear_Click(sender, e);
                    MessageBox.Show("Plot deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select a plot to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DgPlots_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgPlots.SelectedItem is Plot selectedPlot)
            {
                txtPlotNo.Text = selectedPlot.PlotNo;
                cmbProject.SelectedItem = selectedPlot.ProjectName;
                txtSize.Text = selectedPlot.Size;
                txtPrice.Text = selectedPlot.Price;
                cmbStatus.SelectedItem = selectedPlot.Status;
                cmbPlotType.SelectedItem = selectedPlot.PlotType;
                txtDescription.Text = selectedPlot.Description;
            }
        }
    }
}



