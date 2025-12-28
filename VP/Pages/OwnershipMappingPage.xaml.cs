using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using Project.Data;

namespace Project.Pages
{
    /// <summary>
    /// Interaction logic for OwnershipMappingPage.xaml
    /// </summary>
    public partial class OwnershipMappingPage : Page
    {
        public class PlotInfo
        {
            public int PlotId { get; set; }
            public string PlotNo { get; set; } = string.Empty;
        }

        public class PartyInfo
        {
            public int PartyId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
        }

        public class OwnershipMapping
        {
            public int SaleId { get; set; }
            public string PlotNo { get; set; } = string.Empty;
            public string BuyerSellerName { get; set; } = string.Empty;
            public string OwnershipType { get; set; } = string.Empty;
            public DateTime Date { get; set; }
            public decimal Amount { get; set; }
            public string Notes { get; set; } = string.Empty;
        }

        private List<OwnershipMapping> _mappings = new();
        private List<PlotInfo> _plots = new();
        private List<PartyInfo> _parties = new();

        public OwnershipMappingPage()
        {
            InitializeComponent();
            try
            {
                LoadPlots();
                LoadParties();
                LoadDataFromDatabase();
                cmbOwnershipType.ItemsSource = new List<string> { "Buyer", "Seller", "Agent" };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}\n\nPlease ensure the database is set up correctly.", 
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPlots()
        {
            try
            {
                var plotList = PlotManagementDataAccess.GetAllPlots();
                _plots = plotList.Select(p => new PlotInfo
                {
                    PlotId = p.PlotId,
                    PlotNo = p.PlotNo
                }).ToList();

                if (cmbPlot != null)
                {
                    cmbPlot.ItemsSource = _plots;
                    // DisplayMemberPath not needed - ItemTemplate is defined in XAML
                    cmbPlot.SelectedValuePath = "PlotId";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading plots: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LoadParties()
        {
            try
            {
                // Load all parties (both buyers and sellers)
                var buyers = PartyDataAccess.GetAllBuyers();
                _parties = buyers.Select(b => new PartyInfo
                {
                    PartyId = int.Parse(b.BuyerId),
                    Name = b.Name,
                    Type = "Buyer"
                }).ToList();

                // Also load sellers and agents (Parties with Type='Seller' or Type='Agent')
                string query = @"
                    SELECT PartyId, Name, Type
                    FROM Parties
                    WHERE Type IN ('Seller', 'Agent') AND Status = 'Active'
                    ORDER BY Type, Name";

                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                connection.Open();

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int partyIdOrd = reader.GetOrdinal("PartyId");
                    int nameOrd = reader.GetOrdinal("Name");
                    int typeOrd = reader.GetOrdinal("Type");

                    _parties.Add(new PartyInfo
                    {
                        PartyId = reader.GetInt32(partyIdOrd),
                        Name = reader.IsDBNull(nameOrd) ? "" : reader.GetString(nameOrd),
                        Type = reader.IsDBNull(typeOrd) ? "" : reader.GetString(typeOrd)
                    });
                }

                if (cmbBuyerSeller != null)
                {
                    cmbBuyerSeller.ItemsSource = _parties;
                    // DisplayMemberPath not needed - ItemTemplate is defined in XAML
                    cmbBuyerSeller.SelectedValuePath = "PartyId";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading parties: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LoadDataFromDatabase()
        {
            try
            {
                var sales = SalesDataAccess.GetAllSales();
                _mappings = sales.Select(s => new OwnershipMapping
                {
                    SaleId = s.SaleId,
                    PlotNo = s.PlotNo,
                    BuyerSellerName = !string.IsNullOrEmpty(s.BuyerName) ? s.BuyerName : (!string.IsNullOrEmpty(s.SellerName) ? s.SellerName : ""),
                    OwnershipType = s.BuyerId.HasValue ? "Buyer" : (s.SellerId.HasValue ? (s.SellerType == "Agent" ? "Agent" : "Seller") : ""),
                    Date = s.SaleDate,
                    Amount = s.SalePrice,
                    Notes = s.Notes ?? ""
                }).ToList();

                if (dgOwnership != null)
                {
                    dgOwnership.ItemsSource = null;
                    dgOwnership.ItemsSource = _mappings;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading ownership mappings: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private OwnershipMapping? _selectedMapping;

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPlot.SelectedItem == null)
            {
                MessageBox.Show("Please select a Plot.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbBuyerSeller.SelectedItem == null)
            {
                MessageBox.Show("Please select a Buyer/Seller.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid amount.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dpDate.SelectedDate == null)
            {
                MessageBox.Show("Please select a date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var selectedPlot = (PlotInfo)cmbPlot.SelectedItem;
                var selectedParty = (PartyInfo)cmbBuyerSeller.SelectedItem;
                string ownershipType = cmbOwnershipType.SelectedItem?.ToString() ?? "Buyer";

                // Get ProjectId from Plot
                var plotInfo = PlotManagementDataAccess.GetAllPlots()
                    .FirstOrDefault(p => p.PlotId == selectedPlot.PlotId);
                
                if (plotInfo == null)
                {
                    MessageBox.Show("Plot not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Get ProjectId - we need to query it
                int projectId = GetProjectIdByPlotId(selectedPlot.PlotId);
                if (projectId == 0)
                {
                    MessageBox.Show("Could not find project for this plot.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int? buyerId = ownershipType == "Buyer" ? selectedParty.PartyId : null;
                // Store Agent in SellerId field (Sales table only has BuyerId and SellerId)
                int? sellerId = (ownershipType == "Seller" || ownershipType == "Agent") ? selectedParty.PartyId : null;

                // Get notes from text field
                string notes = txtNotes?.Text?.Trim() ?? "";

                if (_selectedMapping != null)
                {
                    // Update existing sale
                    SalesDataAccess.UpdateSale(
                        _selectedMapping.SaleId,
                        buyerId,
                        sellerId,
                        amount,
                        null, // DownPayment - can be added later if needed
                        dpDate.SelectedDate.Value,
                        "Active",
                        notes
                    );
                }
                else
                {
                    // Check if plot already has a sale
                    var existingSale = SalesDataAccess.GetSaleByPlotId(selectedPlot.PlotId);
                    if (existingSale != null)
                    {
                        // Update existing sale
                        SalesDataAccess.UpdateSale(
                            existingSale.SaleId,
                            buyerId,
                            sellerId,
                            amount,
                            null,
                            dpDate.SelectedDate.Value,
                            "Active",
                            notes
                        );
                    }
                    else
                    {
                        // Create new sale
                        SalesDataAccess.InsertSale(
                            projectId,
                            selectedPlot.PlotId,
                            buyerId,
                            sellerId,
                            amount,
                            null, // DownPayment
                            dpDate.SelectedDate.Value,
                            "Active",
                            notes
                        );
                    }
                }

                LoadDataFromDatabase();
                MessageBox.Show("Ownership mapping saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                BtnClear_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving ownership mapping: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetProjectIdByPlotId(int plotId)
        {
            string query = "SELECT ProjectId FROM Plots WHERE PlotId = @PlotId";

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PlotId", plotId);
                connection.Open();
                var result = command.ExecuteScalar();
                return result != null ? (int)result : 0;
            }
            catch
            {
                return 0;
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            _selectedMapping = null;
            if (cmbPlot != null) cmbPlot.SelectedIndex = -1;
            if (cmbBuyerSeller != null) cmbBuyerSeller.SelectedIndex = -1;
            if (cmbOwnershipType != null) cmbOwnershipType.SelectedIndex = -1;
            if (dpDate != null) dpDate.SelectedDate = DateTime.Now;
            if (txtAmount != null) txtAmount.Clear();
            if (txtNotes != null) txtNotes.Clear();
            if (dgOwnership != null) dgOwnership.SelectedItem = null;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedMapping == null)
            {
                MessageBox.Show("Please select an ownership mapping to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete the ownership mapping for plot '{_selectedMapping.PlotNo}'?", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    SalesDataAccess.DeleteSale(_selectedMapping.SaleId);
                    LoadDataFromDatabase();
                    BtnClear_Click(sender, e);
                    MessageBox.Show("Ownership mapping deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting ownership mapping: {ex.Message}", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DgOwnership_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgOwnership.SelectedItem is OwnershipMapping selectedMapping)
            {
                _selectedMapping = selectedMapping;
                
                var plot = _plots.FirstOrDefault(p => p.PlotNo == selectedMapping.PlotNo);
                if (plot != null)
                {
                    cmbPlot.SelectedItem = plot;
                }

                var party = _parties.FirstOrDefault(p => p.Name == selectedMapping.BuyerSellerName);
                if (party != null)
                {
                    cmbBuyerSeller.SelectedItem = party;
                }

                cmbOwnershipType.SelectedItem = selectedMapping.OwnershipType;
                dpDate.SelectedDate = selectedMapping.Date;
                txtAmount.Text = selectedMapping.Amount.ToString("N2");
                txtNotes.Text = selectedMapping.Notes;
            }
        }
    }
}

