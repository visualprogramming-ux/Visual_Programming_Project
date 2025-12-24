using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
            LoadSampleData();
            LoadBuyersAndPlots();
            dpStartDate.SelectedDate = DateTime.Now;
        }
        
        private void LoadBuyersAndPlots()
        {
            _buyers.Add(new Buyer { BuyerId = "BUY001", Name = "Ahmed Khan" });
            _buyers.Add(new Buyer { BuyerId = "BUY002", Name = "Fatima Ali" });
            _buyers.Add(new Buyer { BuyerId = "BUY003", Name = "Hassan Malik" });
            _buyers.Add(new Buyer { BuyerId = "BUY004", Name = "Zainab Ahmed" });
            
            _plots.Add(new Plot { PlotId = "PLT001", PlotNo = "P001" });
            _plots.Add(new Plot { PlotId = "PLT002", PlotNo = "P002" });
            _plots.Add(new Plot { PlotId = "PLT003", PlotNo = "P003" });
            _plots.Add(new Plot { PlotId = "PLT004", PlotNo = "P004" });
            _plots.Add(new Plot { PlotId = "PLT005", PlotNo = "P005" });
            
            cmbBuyer.ItemsSource = _buyers;
            cmbPlot.ItemsSource = _plots;
        }
        
        private void LoadSampleData()
        {
            _paymentPlans.Add(new PaymentPlan
            {
                PlanId = "PLN001",
                BuyerId = "BUY001",
                BuyerName = "Ahmed Khan",
                PlotId = "PLT001",
                PlotNo = "P001",
                PlanType = "Monthly",
                TotalAmount = 5000000.00m,
                DownPayment = 1000000.00m,
                InstallmentCount = 40,
                InstallmentAmount = 100000.00m,
                StartDate = DateTime.Now.AddMonths(-5),
                Status = "Active",
                OverdueReminder = true,
                UpcomingReminder = true
            });
            
            _paymentPlans.Add(new PaymentPlan
            {
                PlanId = "PLN002",
                BuyerId = "BUY002",
                BuyerName = "Fatima Ali",
                PlotId = "PLT002",
                PlotNo = "P002",
                PlanType = "Quarterly",
                TotalAmount = 7500000.00m,
                DownPayment = 1500000.00m,
                InstallmentCount = 8,
                InstallmentAmount = 750000.00m,
                StartDate = DateTime.Now.AddMonths(-2),
                Status = "Active",
                OverdueReminder = true,
                UpcomingReminder = true
            });
            
            _paymentPlans.Add(new PaymentPlan
            {
                PlanId = "PLN003",
                BuyerId = "BUY003",
                BuyerName = "Hassan Malik",
                PlotId = "PLT003",
                PlotNo = "P003",
                PlanType = "Bi-annual",
                TotalAmount = 10000000.00m,
                DownPayment = 2000000.00m,
                InstallmentCount = 4,
                InstallmentAmount = 2000000.00m,
                StartDate = DateTime.Now.AddMonths(-6),
                Status = "Active",
                OverdueReminder = true,
                UpcomingReminder = false
            });
            
            _filteredPlans = new List<PaymentPlan>(_paymentPlans);
            dgPaymentPlans.ItemsSource = _filteredPlans;
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
            var planType = ((ComboBoxItem)cmbPlanType.SelectedItem).Content.ToString() ?? "Monthly";
            var status = ((ComboBoxItem)cmbStatus.SelectedItem).Content.ToString() ?? "Active";
            
            if (_selectedPlan != null)
            {
                // Update existing plan
                _selectedPlan.BuyerId = buyer.BuyerId;
                _selectedPlan.BuyerName = buyer.Name;
                _selectedPlan.PlotId = plot.PlotId;
                _selectedPlan.PlotNo = plot.PlotNo;
                _selectedPlan.PlanType = planType;
                _selectedPlan.TotalAmount = totalAmount;
                _selectedPlan.DownPayment = downPayment;
                _selectedPlan.InstallmentCount = installments;
                _selectedPlan.InstallmentAmount = decimal.Parse(txtInstallmentAmount.Text);
                _selectedPlan.StartDate = dpStartDate.SelectedDate.Value;
                _selectedPlan.Status = status;
                _selectedPlan.OverdueReminder = chkOverdueReminder.IsChecked ?? false;
                _selectedPlan.UpcomingReminder = chkUpcomingReminder.IsChecked ?? false;
            }
            else
            {
                // Create new plan
                var newPlan = new PaymentPlan
                {
                    PlanId = "PLN" + (_paymentPlans.Count + 1).ToString("D3"),
                    BuyerId = buyer.BuyerId,
                    BuyerName = buyer.Name,
                    PlotId = plot.PlotId,
                    PlotNo = plot.PlotNo,
                    PlanType = planType,
                    TotalAmount = totalAmount,
                    DownPayment = downPayment,
                    InstallmentCount = installments,
                    InstallmentAmount = decimal.Parse(txtInstallmentAmount.Text),
                    StartDate = dpStartDate.SelectedDate.Value,
                    Status = status,
                    OverdueReminder = chkOverdueReminder.IsChecked ?? false,
                    UpcomingReminder = chkUpcomingReminder.IsChecked ?? false
                };
                
                _paymentPlans.Add(newPlan);
            }
            
            ApplyFilters();
            MessageBox.Show("Payment plan saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            BtnClear_Click(sender, e);
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
                _paymentPlans.Remove(_selectedPlan);
                ApplyFilters();
                BtnClear_Click(sender, e);
                MessageBox.Show("Payment plan deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
        
        private class PaymentPlan
        {
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