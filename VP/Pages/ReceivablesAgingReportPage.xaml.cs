using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project.Data;

namespace Project.Pages
{
    public partial class ReceivablesAgingReportPage : Page
    {
        private List<CustomerInfo> _customers = new();
        private List<AgingReportItem> _agingReport = new();
        
        public ReceivablesAgingReportPage()
        {
            InitializeComponent();
            LoadCustomers();
            dpAsOfDate.SelectedDate = DateTime.Now;
            GenerateReport();
        }
        
        private void LoadCustomers()
        {
            try
            {
                var parties = PartyDataAccess.GetAllParties();
                _customers = parties.Select(p => new CustomerInfo
                {
                    PartyId = p.PartyId,
                    Name = p.Name ?? ""
                }).ToList();

                var allCustomersItem = new ComboBoxItem { Content = "All Customers", IsSelected = true, Tag = null };
                cmbCustomer.Items.Add(allCustomersItem);
                
                foreach (var customer in _customers)
                {
                    var item = new ComboBoxItem 
                    { 
                        Content = customer.Name, 
                        Tag = customer.PartyId 
                    };
                    cmbCustomer.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customers: {ex.Message}\n\nPlease ensure the database is set up correctly.", 
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private class CustomerInfo
        {
            public int PartyId { get; set; }
            public string Name { get; set; } = string.Empty;
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
            try
            {
                _agingReport.Clear();
                var asOfDate = dpAsOfDate.SelectedDate ?? DateTime.Now;
                
                // Get selected customer (if any)
                int? selectedPartyId = null;
                if (cmbCustomer.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
                {
                    selectedPartyId = (int)selectedItem.Tag;
                }
                
                // Load aging report from database
                var dbReport = LedgerDataAccess.GetReceivablesAgingReport(asOfDate, selectedPartyId);
                
                _agingReport = dbReport.Select(r => new AgingReportItem
                {
                    PartyId = r.PartyId,
                    CustomerName = r.CustomerName,
                    TotalOutstanding = r.TotalOutstanding,
                    Current = r.Current,
                    Days31to60 = r.Days31to60,
                    Days61to90 = r.Days61to90,
                    Over90 = r.Over90,
                    OldestInvoiceDate = r.OldestInvoiceDate
                }).ToList();
                
                UpdateSummaryCards();
                dgAgingReport.ItemsSource = _agingReport;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
        
        private class AgingReportItem
        {
            public int PartyId { get; set; }
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
