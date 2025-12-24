using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Project.Pages
{
    public partial class QuickEntryPage : Page
    {
        private List<Customer> _customers = new();
        private List<Transaction> _transactions = new();
        
        public QuickEntryPage()
        {
            InitializeComponent();
            LoadSampleData();
            LoadCustomers();
            dpTransactionDate.SelectedDate = DateTime.Now;
        }
        
        private void LoadSampleData()
        {
            // Sample transactions
            _transactions.Add(new Transaction
            {
                TransactionId = "TXN001",
                CustomerId = "CUST001",
                CustomerName = "Ahmed Khan",
                TransactionType = "Debit",
                Amount = 50000.00m,
                TransactionDate = DateTime.Now.AddDays(-5),
                Reference = "INV-2024-001",
                Description = "Plot booking payment"
            });
            
            _transactions.Add(new Transaction
            {
                TransactionId = "TXN002",
                CustomerId = "CUST001",
                CustomerName = "Ahmed Khan",
                TransactionType = "Credit",
                Amount = 25000.00m,
                TransactionDate = DateTime.Now.AddDays(-3),
                Reference = "PAY-2024-001",
                Description = "First installment payment"
            });
            
            _transactions.Add(new Transaction
            {
                TransactionId = "TXN003",
                CustomerId = "CUST002",
                CustomerName = "Fatima Ali",
                TransactionType = "Debit",
                Amount = 75000.00m,
                TransactionDate = DateTime.Now.AddDays(-2),
                Reference = "INV-2024-002",
                Description = "Plot purchase advance"
            });
            
            dgTransactions.ItemsSource = _transactions.OrderByDescending(t => t.TransactionDate).ToList();
        }
        
        private void LoadCustomers()
        {
            _customers.Add(new Customer { CustomerId = "CUST001", Name = "Ahmed Khan" });
            _customers.Add(new Customer { CustomerId = "CUST002", Name = "Fatima Ali" });
            _customers.Add(new Customer { CustomerId = "CUST003", Name = "Hassan Malik" });
            _customers.Add(new Customer { CustomerId = "CUST004", Name = "Zainab Ahmed" });
            
            cmbCustomer.ItemsSource = _customers;
        }
        
        private void CmbTransactionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Update UI based on transaction type if needed
        }
        
        private void DpTransactionDate_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            // Handle date selection if needed
        }
        
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCustomer.SelectedItem == null)
            {
                MessageBox.Show("Please select a customer.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(txtAmount.Text) || !decimal.TryParse(txtAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid amount.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (dpTransactionDate.SelectedDate == null)
            {
                MessageBox.Show("Please select a transaction date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var customer = (Customer)cmbCustomer.SelectedItem;
            var transactionType = ((ComboBoxItem)cmbTransactionType.SelectedItem).Content.ToString();
            
            var transaction = new Transaction
            {
                TransactionId = "TXN" + (_transactions.Count + 1).ToString("D3"),
                CustomerId = customer.CustomerId,
                CustomerName = customer.Name,
                TransactionType = transactionType ?? "Debit",
                Amount = amount,
                TransactionDate = dpTransactionDate.SelectedDate.Value,
                Reference = txtReference.Text,
                Description = txtDescription.Text
            };
            
            _transactions.Insert(0, transaction);
            dgTransactions.ItemsSource = _transactions.OrderByDescending(t => t.TransactionDate).ToList();
            
            MessageBox.Show("Transaction saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            BtnClear_Click(sender, e);
        }
        
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            cmbCustomer.SelectedItem = null;
            cmbTransactionType.SelectedIndex = 0;
            txtAmount.Clear();
            dpTransactionDate.SelectedDate = DateTime.Now;
            txtReference.Clear();
            txtDescription.Clear();
        }
        
        private void DgTransactions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle selection if needed
        }
        
        private class Customer
        {
            public string CustomerId { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
        }
        
        private class Transaction
        {
            public string TransactionId { get; set; } = string.Empty;
            public string CustomerId { get; set; } = string.Empty;
            public string CustomerName { get; set; } = string.Empty;
            public string TransactionType { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public DateTime TransactionDate { get; set; }
            public string Reference { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }
    }
}
