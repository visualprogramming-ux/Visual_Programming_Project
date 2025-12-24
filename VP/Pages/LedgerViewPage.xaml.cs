using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Project.Pages
{
    /// <summary>
    /// Interaction logic for LedgerViewPage.xaml
    /// </summary>
    public partial class LedgerViewPage : Page
    {
        public class CustomerLedger
        {
            public string CustomerId { get; set; }
            public string Name { get; set; }
            public string CNICNTN { get; set; }
            public string Contact { get; set; }
            public string Address { get; set; }
            public string Type { get; set; }
            public string Status { get; set; }
        }

        public class LedgerEntry
        {
            public DateTime Date { get; set; }
            public string Description { get; set; }
            public string Reference { get; set; }
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
            public decimal Balance { get; set; }
            public string Aging { get; set; }
        }

        private List<CustomerLedger> _customers;
        private Dictionary<string, List<LedgerEntry>> _ledgerEntries;

        public LedgerViewPage()
        {
            InitializeComponent();
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            _customers = new List<CustomerLedger>
            {
                new CustomerLedger 
                { 
                    CustomerId = "CUST001",
                    Name = "John Smith", 
                    CNICNTN = "12345-1234567-1",
                    Contact = "+1234567890",
                    Address = "123 Main Street, City",
                    Type = "Buyer",
                    Status = "Active"
                },
                new CustomerLedger 
                { 
                    CustomerId = "CUST002",
                    Name = "Sarah Johnson", 
                    CNICNTN = "23456-2345678-2",
                    Contact = "+1234567891",
                    Address = "456 Oak Avenue, City",
                    Type = "Seller",
                    Status = "Active"
                },
                new CustomerLedger 
                { 
                    CustomerId = "CUST003",
                    Name = "Michael Brown", 
                    CNICNTN = "34567-3456789-3",
                    Contact = "+1234567892",
                    Address = "789 Pine Road, City",
                    Type = "Agent",
                    Status = "Active"
                },
                new CustomerLedger 
                { 
                    CustomerId = "CUST004",
                    Name = "Emily Davis", 
                    CNICNTN = "45678-4567890-4",
                    Contact = "+1234567893",
                    Address = "321 Elm Street, City",
                    Type = "Buyer",
                    Status = "Inactive"
                }
            };

            _ledgerEntries = new Dictionary<string, List<LedgerEntry>>();
            
            // Sample ledger entries for CUST001
            _ledgerEntries["CUST001"] = new List<LedgerEntry>
            {
                new LedgerEntry { Date = DateTime.Now.AddDays(-60), Description = "Initial Payment", Reference = "INV001", Debit = 0, Credit = 500000, Balance = 500000, Aging = "60 days" },
                new LedgerEntry { Date = DateTime.Now.AddDays(-45), Description = "Plot Purchase", Reference = "PLT001", Debit = 750000, Credit = 0, Balance = -250000, Aging = "45 days" },
                new LedgerEntry { Date = DateTime.Now.AddDays(-30), Description = "Installment Payment", Reference = "INV002", Debit = 0, Credit = 100000, Balance = -150000, Aging = "30 days" },
                new LedgerEntry { Date = DateTime.Now.AddDays(-15), Description = "Installment Payment", Reference = "INV003", Debit = 0, Credit = 50000, Balance = -100000, Aging = "15 days" }
            };

            // Sample ledger entries for CUST002
            _ledgerEntries["CUST002"] = new List<LedgerEntry>
            {
                new LedgerEntry { Date = DateTime.Now.AddDays(-90), Description = "Plot Sale", Reference = "SALE001", Debit = 0, Credit = 600000, Balance = 600000, Aging = "90 days" },
                new LedgerEntry { Date = DateTime.Now.AddDays(-75), Description = "Commission Payment", Reference = "COM001", Debit = 30000, Credit = 0, Balance = 570000, Aging = "75 days" },
                new LedgerEntry { Date = DateTime.Now.AddDays(-45), Description = "Tax Deduction", Reference = "TAX001", Debit = 5000, Credit = 0, Balance = 565000, Aging = "45 days" }
            };

            cmbSelectCustomer.ItemsSource = _customers;
            if (_customers.Count > 0)
            {
                cmbSelectCustomer.SelectedIndex = 0;
            }

            UpdateLedgerView();
        }

        private void UpdateLedgerView()
        {
            if (cmbSelectCustomer.SelectedItem is CustomerLedger selectedCustomer)
            {
                if (_ledgerEntries.ContainsKey(selectedCustomer.CustomerId))
                {
                    var entries = _ledgerEntries[selectedCustomer.CustomerId];
                    dgLedger.ItemsSource = entries;
                    
                    // Calculate running balance (last entry's balance)
                    decimal runningBalance = entries.Count > 0 ? entries.Last().Balance : 0;
                    txtRunningBalance.Text = runningBalance.ToString("N2");
                    
                    // Calculate outstanding amount (negative balance)
                    decimal outstandingAmount = runningBalance < 0 ? Math.Abs(runningBalance) : 0;
                    txtOutstandingAmount.Text = outstandingAmount.ToString("N2");
                    
                    // Calculate total credit
                    decimal totalCredit = entries.Sum(e => e.Credit);
                    txtTotalCredit.Text = totalCredit.ToString("N2");
                }
                else
                {
                    dgLedger.ItemsSource = null;
                    txtRunningBalance.Text = "0.00";
                    txtOutstandingAmount.Text = "0.00";
                    txtTotalCredit.Text = "0.00";
                }
            }
            else
            {
                dgLedger.ItemsSource = null;
                txtRunningBalance.Text = "0.00";
                txtOutstandingAmount.Text = "0.00";
                txtTotalCredit.Text = "0.00";
            }
        }

        private void CmbSelectCustomer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateLedgerView();
        }

        private void DgLedger_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ledger entries are read-only, no action needed
        }

        private void BtnLinkToPlots_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Link to Plots/Projects functionality would be implemented here.\nThis would open a dialog to link the selected customer to specific plots or projects.", 
                "Link to Plots/Projects", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

