using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Project.Pages
{
    public partial class CustomerStatementsPage : Page
    {
        private List<Customer> _customers = new();
        private List<StatementTransaction> _statementTransactions = new();
        
        public CustomerStatementsPage()
        {
            InitializeComponent();
            LoadSampleData();
            LoadCustomers();
            dpToDate.SelectedDate = DateTime.Now;
            dpFromDate.SelectedDate = DateTime.Now.AddDays(-30);
        }
        
        private void LoadSampleData()
        {
            _customers.Add(new Customer 
            { 
                CustomerId = "CUST001", 
                Name = "Ahmed Khan",
                Address = "123 Main Street, Karachi, Pakistan"
            });
            _customers.Add(new Customer 
            { 
                CustomerId = "CUST002", 
                Name = "Fatima Ali",
                Address = "456 Park Avenue, Lahore, Pakistan"
            });
            _customers.Add(new Customer 
            { 
                CustomerId = "CUST003", 
                Name = "Hassan Malik",
                Address = "789 Business District, Islamabad, Pakistan"
            });
            
            cmbCustomer.ItemsSource = _customers;
        }
        
        private void LoadCustomers()
        {
            // Already loaded in LoadSampleData
        }
        
        private void CmbCustomer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCustomer.SelectedItem != null)
            {
                GenerateStatement();
            }
        }
        
        private void CmbPeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPeriod.SelectedItem == null) return;
            
            var selectedItem = cmbPeriod.SelectedItem as ComboBoxItem;
            if (selectedItem == null || selectedItem.Content == null) return;
            
            var content = selectedItem.Content.ToString();
            if (content == null) return;
            
            if (content == "Custom Range")
            {
                if (pnlDateRange != null) pnlDateRange.Visibility = Visibility.Visible;
                if (pnlToDate != null) pnlToDate.Visibility = Visibility.Visible;
            }
            else
            {
                if (pnlDateRange != null) pnlDateRange.Visibility = Visibility.Collapsed;
                if (pnlToDate != null) pnlToDate.Visibility = Visibility.Collapsed;
                
                var days = content switch
                {
                    "Last 30 Days" => 30,
                    "Last 60 Days" => 60,
                    "Last 90 Days" => 90,
                    _ => 30
                };
                
                if (dpToDate != null) dpToDate.SelectedDate = DateTime.Now;
                if (dpFromDate != null) dpFromDate.SelectedDate = DateTime.Now.AddDays(-days);
            }
            
            if (cmbCustomer != null && cmbCustomer.SelectedItem != null)
            {
                GenerateStatement();
            }
        }
        
        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCustomer.SelectedItem == null)
            {
                MessageBox.Show("Please select a customer.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            GenerateStatement();
            btnExportPDF.IsEnabled = true;
            btnEmail.IsEnabled = true;
        }
        
        private void GenerateStatement()
        {
            if (cmbCustomer.SelectedItem == null) return;
            
            var customer = (Customer)cmbCustomer.SelectedItem;
            txtCustomerName.Text = customer.Name;
            txtCustomerAddress.Text = customer.Address;
            
            var fromDate = dpFromDate.SelectedDate ?? DateTime.Now.AddDays(-30);
            var toDate = dpToDate.SelectedDate ?? DateTime.Now;
            txtStatementPeriod.Text = $"{fromDate:dd/MM/yyyy} to {toDate:dd/MM/yyyy}";
            
            _statementTransactions.Clear();
            
            // Sample statement transactions
            decimal runningBalance = 50000.00m; // Opening balance
            
            _statementTransactions.Add(new StatementTransaction
            {
                TransactionDate = fromDate.AddDays(5),
                Description = "Plot booking payment",
                Reference = "INV-2024-001",
                Debit = 100000.00m,
                Credit = 0.00m,
                Balance = runningBalance += 100000.00m
            });
            
            _statementTransactions.Add(new StatementTransaction
            {
                TransactionDate = fromDate.AddDays(10),
                Description = "First installment payment",
                Reference = "PAY-2024-001",
                Debit = 0.00m,
                Credit = 50000.00m,
                Balance = runningBalance -= 50000.00m
            });
            
            _statementTransactions.Add(new StatementTransaction
            {
                TransactionDate = fromDate.AddDays(15),
                Description = "Additional services charge",
                Reference = "INV-2024-002",
                Debit = 25000.00m,
                Credit = 0.00m,
                Balance = runningBalance += 25000.00m
            });
            
            _statementTransactions.Add(new StatementTransaction
            {
                TransactionDate = fromDate.AddDays(20),
                Description = "Second installment payment",
                Reference = "PAY-2024-002",
                Debit = 0.00m,
                Credit = 75000.00m,
                Balance = runningBalance -= 75000.00m
            });
            
            txtOpeningBalance.Text = "50,000.00";
            txtClosingBalance.Text = runningBalance.ToString("N2");
            txtOutstanding.Text = runningBalance > 0 ? runningBalance.ToString("N2") : "0.00";
            
            dgStatement.ItemsSource = _statementTransactions.OrderBy(t => t.TransactionDate).ToList();
        }
        
        private void BtnExportPDF_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export to PDF functionality will be implemented here.\nThis will generate a PDF version of the statement.", 
                "Export to PDF", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void BtnEmail_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Email Statement functionality will be implemented here.\nThis will send the statement to the customer's email address.", 
                "Email Statement", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private class Customer
        {
            public string CustomerId { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
        }
        
        private class StatementTransaction
        {
            public DateTime TransactionDate { get; set; }
            public string Description { get; set; } = string.Empty;
            public string Reference { get; set; } = string.Empty;
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
            public decimal Balance { get; set; }
        }
    }
}
