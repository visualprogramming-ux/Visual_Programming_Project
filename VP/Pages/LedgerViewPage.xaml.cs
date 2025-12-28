using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project.Data;

namespace Project.Pages
{
    /// <summary>
    /// Interaction logic for LedgerViewPage.xaml
    /// </summary>
    public partial class LedgerViewPage : Page
    {
        public class CustomerInfo
        {
            public int PartyId { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        public class LedgerEntry
        {
            public DateTime Date { get; set; }
            public string Description { get; set; } = string.Empty;
            public string Reference { get; set; } = string.Empty;
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
            public decimal Balance { get; set; }
            public string Aging { get; set; } = string.Empty;
        }

        private List<CustomerInfo> _customers = new();
        private List<LedgerEntry> _ledgerEntries = new();

        public LedgerViewPage()
        {
            InitializeComponent();
            LoadCustomers();
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

                cmbSelectCustomer.ItemsSource = _customers;
                cmbSelectCustomer.DisplayMemberPath = "Name";
                cmbSelectCustomer.SelectedValuePath = "PartyId";

                if (_customers.Count > 0)
                {
                    cmbSelectCustomer.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customers: {ex.Message}\n\nPlease ensure the database is set up correctly.", 
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateLedgerView()
        {
            if (cmbSelectCustomer.SelectedItem is CustomerInfo selectedCustomer)
            {
                try
                {
                    // Load ledger entries from database
                    var dbEntries = LedgerDataAccess.GetLedgerEntriesByPartyId(selectedCustomer.PartyId);
                    _ledgerEntries = dbEntries.Select(e => new LedgerEntry
                    {
                        Date = e.Date,
                        Description = e.Description,
                        Reference = e.Reference,
                        Debit = e.Debit,
                        Credit = e.Credit,
                        Balance = e.Balance,
                        Aging = e.Aging
                    }).ToList();

                    dgLedger.ItemsSource = _ledgerEntries;

                    // Get summary from database
                    var summary = LedgerDataAccess.GetLedgerSummary(selectedCustomer.PartyId);
                    
                    txtRunningBalance.Text = summary.RunningBalance.ToString("N2");
                    txtOutstandingAmount.Text = summary.OutstandingAmount.ToString("N2");
                    txtTotalCredit.Text = summary.TotalCredit.ToString("N2");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading ledger: {ex.Message}", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    
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

