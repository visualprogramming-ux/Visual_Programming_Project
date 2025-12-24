using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Project.Pages
{
    public partial class ReceivablesAgingReportPage : Page
    {
        private List<Customer> _customers = new();
        private List<AgingReportItem> _agingReport = new();
        
        public ReceivablesAgingReportPage()
        {
            InitializeComponent();
            LoadSampleData();
            dpAsOfDate.SelectedDate = DateTime.Now;
            GenerateReport();
        }
        
        private void LoadSampleData()
        {
            _customers.Add(new Customer { CustomerId = "CUST001", Name = "Ahmed Khan" });
            _customers.Add(new Customer { CustomerId = "CUST002", Name = "Fatima Ali" });
            _customers.Add(new Customer { CustomerId = "CUST003", Name = "Hassan Malik" });
            _customers.Add(new Customer { CustomerId = "CUST004", Name = "Zainab Ahmed" });
            
            var allCustomersItem = new ComboBoxItem { Content = "All Customers", IsSelected = true };
            cmbCustomer.Items.Add(allCustomersItem);
            foreach (var customer in _customers)
            {
                cmbCustomer.Items.Add(customer);
            }
        }
        
        private void DpAsOfDate_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (dpAsOfDate.SelectedDate.HasValue)
            {
                GenerateReport();
            }
        }
        
        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            GenerateReport();
        }
        
        private void GenerateReport()
        {
            _agingReport.Clear();
            var asOfDate = dpAsOfDate.SelectedDate ?? DateTime.Now;
            
            // Sample aging data
            _agingReport.Add(new AgingReportItem
            {
                CustomerId = "CUST001",
                CustomerName = "Ahmed Khan",
                TotalOutstanding = 250000.00m,
                Current = 100000.00m,
                Days31to60 = 75000.00m,
                Days61to90 = 50000.00m,
                Over90 = 25000.00m,
                OldestInvoiceDate = asOfDate.AddDays(-95)
            });
            
            _agingReport.Add(new AgingReportItem
            {
                CustomerId = "CUST002",
                CustomerName = "Fatima Ali",
                TotalOutstanding = 180000.00m,
                Current = 120000.00m,
                Days31to60 = 40000.00m,
                Days61to90 = 20000.00m,
                Over90 = 0.00m,
                OldestInvoiceDate = asOfDate.AddDays(-75)
            });
            
            _agingReport.Add(new AgingReportItem
            {
                CustomerId = "CUST003",
                CustomerName = "Hassan Malik",
                TotalOutstanding = 320000.00m,
                Current = 80000.00m,
                Days31to60 = 60000.00m,
                Days61to90 = 90000.00m,
                Over90 = 90000.00m,
                OldestInvoiceDate = asOfDate.AddDays(-120)
            });
            
            UpdateSummaryCards();
            dgAgingReport.ItemsSource = _agingReport;
        }
        
        private void UpdateSummaryCards()
        {
            var current = _agingReport.Sum(r => r.Current);
            var days31to60 = _agingReport.Sum(r => r.Days31to60);
            var days61to90 = _agingReport.Sum(r => r.Days61to90);
            var over90 = _agingReport.Sum(r => r.Over90);
            
            txtCurrent.Text = current.ToString("N2");
            txtDays31to60.Text = days31to60.ToString("N2");
            txtDays61to90.Text = days61to90.ToString("N2");
            txtOver90.Text = over90.ToString("N2");
        }
        
        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export to Excel functionality will be implemented here.", 
                "Export to Excel", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void BtnExportPDF_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export to PDF functionality will be implemented here.", 
                "Export to PDF", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private class Customer
        {
            public string CustomerId { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
        }
        
        private class AgingReportItem
        {
            public string CustomerId { get; set; } = string.Empty;
            public string CustomerName { get; set; } = string.Empty;
            public decimal TotalOutstanding { get; set; }
            public decimal Current { get; set; }
            public decimal Days31to60 { get; set; }
            public decimal Days61to90 { get; set; }
            public decimal Over90 { get; set; }
            public DateTime OldestInvoiceDate { get; set; }
        }
    }
}
