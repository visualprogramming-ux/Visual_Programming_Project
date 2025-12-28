using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project.Data;

namespace Project.Pages
{
    public partial class Page48Page : Page
    {
        private List<PaymentPlan> _paymentPlans = new();
        private List<PaymentPlan> _filteredPlans = new();
        private List<Buyer> _buyers = new();
        private List<Plot> _plots = new();
        private PaymentPlan? _selectedPlan;
        
        public Page48Page()
        {
            InitializeComponent();
            try
            {
                LoadBuyersAndPlots();
                LoadDataFromDatabase();
                dpStartDate.SelectedDate = DateTime.Now;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}\n\nPlease ensure the database is set up correctly.", 
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void LoadBuyersAndPlots()
        {
            try
            {
                var buyerList = PartyDataAccess.GetAllBuyers();
                _buyers = buyerList.Select(b => new Buyer 
                { 
                    BuyerId = b.BuyerId, 
                    Name = b.Name 
                }).ToList();
                
                var plotList = PlotDataAccess.GetAllPlots();
                _plots = plotList.Select(p => new Plot 
                { 
                    PlotId = p.PlotId, 
                    PlotNo = p.PlotNo 
                }).ToList();
                
                if (cmbBuyer != null) cmbBuyer.ItemsSource = _buyers;
                if (cmbPlot != null) cmbPlot.ItemsSource = _plots;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading buyers and plots: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private void LoadDataFromDatabase()
        {
            try
            {
                _paymentPlans = PaymentPlanDataAccess.GetAllPaymentPlans();
                _filteredPlans = new List<PaymentPlan>(_paymentPlans);
                
                if (dgPaymentPlans != null)
                {
                    dgPaymentPlans.ItemsSource = null;
                    dgPaymentPlans.ItemsSource = _filteredPlans;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payment plans: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private void CmbPlanType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Don't calculate if the page is still initializing
            if (txtTotalAmount == null || txtDownPayment == null || 
                txtInstallments == null || txtInstallmentAmount == null)
            {
                return;
            }
            CalculateInstallmentAmount();
        }
        
        private void TxtTotalAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Don't calculate if controls aren't initialized
            if (txtTotalAmount == null || txtDownPayment == null || 
                txtInstallments == null || txtInstallmentAmount == null)
            {
                return;
            }
            CalculateInstallmentAmount();
        }
        
        private void TxtDownPayment_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Don't calculate if controls aren't initialized
            if (txtTotalAmount == null || txtDownPayment == null || 
                txtInstallments == null || txtInstallmentAmount == null)
            {
                return;
            }
            CalculateInstallmentAmount();
        }
        
        private void TxtInstallments_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Don't calculate if controls aren't initialized
            if (txtTotalAmount == null || txtDownPayment == null || 
                txtInstallments == null || txtInstallmentAmount == null)
            {
                return;
            }
            CalculateInstallmentAmount();
        }
        
        private void CalculateInstallmentAmount()
        {
            // Check if controls are initialized
            if (txtTotalAmount == null || txtDownPayment == null || 
                txtInstallments == null || txtInstallmentAmount == null)
            {
                return;
            }
            
            if (string.IsNullOrWhiteSpace(txtTotalAmount.Text) ||
                string.IsNullOrWhiteSpace(txtDownPayment.Text) ||
                string.IsNullOrWhiteSpace(txtInstallments.Text))
            {
                txtInstallmentAmount.Text = "";
                return;
            }
            
            if (decimal.TryParse(txtTotalAmount.Text, out decimal totalAmount) &&
                decimal.TryParse(txtDownPayment.Text, out decimal downPayment) &&
                int.TryParse(txtInstallments.Text, out int installments) &&
                installments > 0)
            {
                decimal remainingAmount = totalAmount - downPayment;
                decimal installmentAmount = remainingAmount / installments;
                txtInstallmentAmount.Text = installmentAmount.ToString("N2");
            }
            else
            {
                txtInstallmentAmount.Text = "";
            }
        }
        
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbBuyer.SelectedItem == null)
            {
                MessageBox.Show("Please select a buyer.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (cmbPlot.SelectedItem == null)
            {
                MessageBox.Show("Please select a plot.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(txtTotalAmount.Text) || !decimal.TryParse(txtTotalAmount.Text, out decimal totalAmount) || totalAmount <= 0)
            {
                MessageBox.Show("Please enter a valid total amount.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(txtDownPayment.Text) || !decimal.TryParse(txtDownPayment.Text, out decimal downPayment) || downPayment < 0)
            {
                MessageBox.Show("Please enter a valid down payment.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(txtInstallments.Text) || !int.TryParse(txtInstallments.Text, out int installments) || installments <= 0)
            {
                MessageBox.Show("Please enter a valid number of installments.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (dpStartDate.SelectedDate == null)
            {
                MessageBox.Show("Please select a start date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (downPayment >= totalAmount)
            {
                MessageBox.Show("Down payment cannot be greater than or equal to total amount.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var buyer = (Buyer)cmbBuyer.SelectedItem;
            var plot = (Plot)cmbPlot.SelectedItem;
            var planType = ((ComboBoxItem)cmbPlanType.SelectedItem)?.Content?.ToString() ?? "Monthly";
            var status = ((ComboBoxItem)cmbStatus.SelectedItem)?.Content?.ToString() ?? "Active";
            
            try
            {
                int buyerIdInt = int.Parse(buyer.BuyerId);
                int plotIdInt = int.Parse(plot.PlotId);
                decimal installmentAmount = decimal.Parse(txtInstallmentAmount.Text);

                // Get Reminder settings (planType and status are already declared above)
                bool overdueReminder = chkOverdueReminder.IsChecked ?? true;
                bool upcomingReminder = chkUpcomingReminder.IsChecked ?? true;
                
                // Update Sale status if needed
                if (_selectedPlan != null)
                {
                    // Update existing plan
                    PaymentPlanDataAccess.UpdatePaymentPlan(
                        _selectedPlan.PaymentPlanId,
                        totalAmount,
                        downPayment,
                        installments,
                        installmentAmount,
                        dpStartDate.SelectedDate.Value,
                        planType,
                        status,
                        overdueReminder,
                        upcomingReminder
                    );
                    
                    // Update Sale status
                    UpdateSaleStatus(buyerIdInt, plotIdInt, status);
                }
                else
                {
                    // Create new plan - first get or create a Sale
                    int saleId = PaymentPlanDataAccess.GetOrCreateSale(
                        buyerIdInt,
                        plotIdInt,
                        totalAmount,
                        downPayment
                    );

                    // Insert new payment plan
                    int paymentPlanId = PaymentPlanDataAccess.InsertPaymentPlan(
                        saleId,
                        totalAmount,
                        downPayment,
                        installments,
                        installmentAmount,
                        dpStartDate.SelectedDate.Value,
                        planType,
                        status,
                        overdueReminder,
                        upcomingReminder
                    );
                    
                    // Update Sale status
                    UpdateSaleStatus(buyerIdInt, plotIdInt, status);
                }
                
                // Reload data from database
                LoadDataFromDatabase();
                MessageBox.Show("Payment plan saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                BtnClear_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving payment plan: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            cmbBuyer.SelectedItem = null;
            cmbPlot.SelectedItem = null;
            cmbPlanType.SelectedIndex = 0;
            txtTotalAmount.Clear();
            txtDownPayment.Clear();
            txtInstallments.Clear();
            txtInstallmentAmount.Clear();
            dpStartDate.SelectedDate = DateTime.Now;
            cmbStatus.SelectedIndex = 0;
            chkOverdueReminder.IsChecked = true;
            chkUpcomingReminder.IsChecked = true;
            _selectedPlan = null;
        }
        
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPlan == null)
            {
                MessageBox.Show("Please select a payment plan to delete.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var result = MessageBox.Show($"Are you sure you want to delete payment plan {_selectedPlan.PlanId}?", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    PaymentPlanDataAccess.DeletePaymentPlan(_selectedPlan.PaymentPlanId);
                    LoadDataFromDatabase();
                    BtnClear_Click(sender, e);
                    MessageBox.Show("Payment plan deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting payment plan: {ex.Message}", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void DgPaymentPlans_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgPaymentPlans.SelectedItem is PaymentPlan plan)
            {
                _selectedPlan = plan;
                
                var buyer = _buyers.FirstOrDefault(b => b.BuyerId == plan.BuyerId);
                var plot = _plots.FirstOrDefault(p => p.PlotId == plan.PlotId);
                
                if (buyer != null) cmbBuyer.SelectedItem = buyer;
                if (plot != null) cmbPlot.SelectedItem = plot;
                
                var planTypeItem = cmbPlanType.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == plan.PlanType);
                if (planTypeItem != null) cmbPlanType.SelectedItem = planTypeItem;
                
                txtTotalAmount.Text = plan.TotalAmount.ToString("N2");
                txtDownPayment.Text = plan.DownPayment.ToString("N2");
                txtInstallments.Text = plan.InstallmentCount.ToString();
                txtInstallmentAmount.Text = plan.InstallmentAmount.ToString("N2");
                dpStartDate.SelectedDate = plan.StartDate;
                
                var statusItem = cmbStatus.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == plan.Status);
                if (statusItem != null) cmbStatus.SelectedItem = statusItem;
                
                chkOverdueReminder.IsChecked = plan.OverdueReminder;
                chkUpcomingReminder.IsChecked = plan.UpcomingReminder;
            }
        }
        
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Don't apply filters if controls aren't initialized
            if (txtSearch == null || cmbFilterStatus == null || dgPaymentPlans == null)
            {
                return;
            }
            ApplyFilters();
        }
        
        private void CmbFilterStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Don't apply filters if controls aren't initialized
            if (txtSearch == null || cmbFilterStatus == null || dgPaymentPlans == null)
            {
                return;
            }
            ApplyFilters();
        }
        
        private void ApplyFilters()
        {
            // Check if controls are initialized
            if (txtSearch == null || cmbFilterStatus == null || dgPaymentPlans == null)
            {
                return;
            }
            
            _filteredPlans.Clear();
            var query = _paymentPlans.AsEnumerable();
            
            // Search filter
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                var searchText = txtSearch.Text.ToLower();
                query = query.Where(p => 
                    p.BuyerName.ToLower().Contains(searchText) ||
                    p.PlotNo.ToLower().Contains(searchText) ||
                    p.PlanId.ToLower().Contains(searchText));
            }
            
            // Status filter
            if (cmbFilterStatus.SelectedItem is ComboBoxItem statusItem && statusItem.Content.ToString() != "All Status")
            {
                query = query.Where(p => p.Status == statusItem.Content.ToString());
            }
            
            _filteredPlans.AddRange(query);
            dgPaymentPlans.ItemsSource = null;
            dgPaymentPlans.ItemsSource = _filteredPlans;
        }
        
        private void BtnExportPDF_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export to PDF functionality will be implemented here.", 
                "Export to PDF", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export to Excel functionality will be implemented here.", 
                "Export to Excel", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void UpdateSaleStatus(int buyerId, int plotId, string status)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                connection.Open();
                
                string query = @"
                    UPDATE Sales 
                    SET Status = @Status 
                    WHERE BuyerId = @BuyerId AND PlotId = @PlotId";
                
                using var command = new Microsoft.Data.SqlClient.SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Status", status);
                command.Parameters.AddWithValue("@BuyerId", buyerId);
                command.Parameters.AddWithValue("@PlotId", plotId);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // Log error but don't fail the save operation
                System.Diagnostics.Debug.WriteLine($"Error updating sale status: {ex.Message}");
            }
        }
        
        private class Buyer
        {
            public string BuyerId { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
        }
        
        private class Plot
        {
            public string PlotId { get; set; } = string.Empty;
            public string PlotNo { get; set; } = string.Empty;
        }
        
        public class PaymentPlan
        {
            public int PaymentPlanId { get; set; }
            public string PlanId { get; set; } = string.Empty;
            public string BuyerId { get; set; } = string.Empty;
            public string BuyerName { get; set; } = string.Empty;
            public string PlotId { get; set; } = string.Empty;
            public string PlotNo { get; set; } = string.Empty;
            public string PlanType { get; set; } = string.Empty;
            public decimal TotalAmount { get; set; }
            public decimal DownPayment { get; set; }
            public int InstallmentCount { get; set; }
            public decimal InstallmentAmount { get; set; }
            public DateTime StartDate { get; set; }
            public string Status { get; set; } = string.Empty;
            public bool OverdueReminder { get; set; }
            public bool UpcomingReminder { get; set; }
        }
    }
}