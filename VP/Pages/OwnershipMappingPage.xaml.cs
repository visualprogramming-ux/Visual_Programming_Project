using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Project.Pages
{
    /// <summary>
    /// Interaction logic for OwnershipMappingPage.xaml
    /// </summary>
    public partial class OwnershipMappingPage : Page
    {
        public class Plot
        {
            public string PlotNo { get; set; }
        }

        public class BuyerSeller
        {
            public string Name { get; set; }
            public string Contact { get; set; }
        }

        public class OwnershipMapping
        {
            public string PlotNo { get; set; }
            public string BuyerSellerName { get; set; }
            public string OwnershipType { get; set; }
            public DateTime Date { get; set; }
            public string Amount { get; set; }
            public string Notes { get; set; }
        }

        private List<OwnershipMapping> _mappings;
        private List<Plot> _plots;
        private List<BuyerSeller> _buyerSellers;

        public OwnershipMappingPage()
        {
            InitializeComponent();
            LoadPlots();
            LoadSellers();
            LoadSampleData();
        }

        private void LoadPlots()
        {
            _plots = new List<Plot>
            {
                new Plot { PlotNo = "P001" },
                new Plot { PlotNo = "P002" },
                new Plot { PlotNo = "P003" },
                new Plot { PlotNo = "P004" },
                new Plot { PlotNo = "P005" }
            };
            cmbPlot.Items.Clear();
            cmbPlot.ItemsSource = _plots;
        }

        private void LoadSellers()
        {
            _buyerSellers = new List<BuyerSeller>
            {
                new BuyerSeller { Name = "John Smith", Contact = "123-456-7890" },
                new BuyerSeller { Name = "Sarah Johnson", Contact = "234-567-8901" },
                new BuyerSeller { Name = "Michael Brown", Contact = "345-678-9012" },
                new BuyerSeller { Name = "Emily Davis", Contact = "456-789-0123" },
                new BuyerSeller { Name = "David Wilson", Contact = "567-890-1234" }
            };
            cmbBuyerSeller.Items.Clear();
            cmbBuyerSeller.ItemsSource = _buyerSellers;
            
            cmbOwnershipType.ItemsSource = new List<string> { "Buyer", "Seller", "Agent", "Investor" };
        }

        private void LoadSampleData()
        {
            _mappings = new List<OwnershipMapping>
            {
                new OwnershipMapping { PlotNo = "P001", BuyerSellerName = "John Smith", OwnershipType = "Buyer", Date = DateTime.Now.AddDays(-30), Amount = "2500000", Notes = "Initial purchase" },
                new OwnershipMapping { PlotNo = "P002", BuyerSellerName = "Sarah Johnson", OwnershipType = "Buyer", Date = DateTime.Now.AddDays(-20), Amount = "3000000", Notes = "Full payment" },
                new OwnershipMapping { PlotNo = "P003", BuyerSellerName = "Michael Brown", OwnershipType = "Seller", Date = DateTime.Now.AddDays(-10), Amount = "3500000", Notes = "Plot sold" }
            };

            dgOwnership.ItemsSource = _mappings;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPlot.SelectedItem == null || cmbBuyerSeller.SelectedItem == null)
            {
                MessageBox.Show("Please select Plot and Buyer/Seller.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedPlot = cmbPlot.SelectedItem as Plot;
            var selectedBuyerSeller = cmbBuyerSeller.SelectedItem as BuyerSeller;

            var existingMapping = _mappings.FirstOrDefault(m => m.PlotNo == selectedPlot.PlotNo);
            if (existingMapping != null)
            {
                // Update existing
                existingMapping.BuyerSellerName = selectedBuyerSeller.Name;
                existingMapping.OwnershipType = cmbOwnershipType.SelectedItem?.ToString() ?? "Buyer";
                existingMapping.Date = dpDate.SelectedDate ?? DateTime.Now;
                existingMapping.Amount = txtAmount.Text;
                existingMapping.Notes = txtNotes.Text;
                MessageBox.Show("Ownership mapping updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Add new
                var newMapping = new OwnershipMapping
                {
                    PlotNo = selectedPlot.PlotNo,
                    BuyerSellerName = selectedBuyerSeller.Name,
                    OwnershipType = cmbOwnershipType.SelectedItem?.ToString() ?? "Buyer",
                    Date = dpDate.SelectedDate ?? DateTime.Now,
                    Amount = txtAmount.Text,
                    Notes = txtNotes.Text
                };
                _mappings.Add(newMapping);
                MessageBox.Show("Ownership mapping added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            dgOwnership.ItemsSource = null;
            dgOwnership.ItemsSource = _mappings;
            BtnClear_Click(sender, e);
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            cmbPlot.SelectedIndex = -1;
            cmbBuyerSeller.SelectedIndex = -1;
            cmbOwnershipType.SelectedIndex = -1;
            if (dpDate != null)
                dpDate.SelectedDate = DateTime.Now;
            txtAmount.Clear();
            txtNotes.Clear();
            dgOwnership.SelectedItem = null;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgOwnership.SelectedItem is OwnershipMapping selectedMapping)
            {
                var result = MessageBox.Show($"Are you sure you want to delete the ownership mapping for plot '{selectedMapping.PlotNo}'?", 
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    _mappings.Remove(selectedMapping);
                    dgOwnership.ItemsSource = null;
                    dgOwnership.ItemsSource = _mappings;
                    BtnClear_Click(sender, e);
                    MessageBox.Show("Ownership mapping deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select an ownership mapping to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DgOwnership_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgOwnership.SelectedItem is OwnershipMapping selectedMapping)
            {
                cmbPlot.SelectedItem = _plots.FirstOrDefault(p => p.PlotNo == selectedMapping.PlotNo);
                cmbBuyerSeller.SelectedItem = _buyerSellers.FirstOrDefault(b => b.Name == selectedMapping.BuyerSellerName);
                cmbOwnershipType.SelectedItem = selectedMapping.OwnershipType;
                dpDate.SelectedDate = selectedMapping.Date;
                txtAmount.Text = selectedMapping.Amount;
                txtNotes.Text = selectedMapping.Notes;
            }
        }
    }
}

