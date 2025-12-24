using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Project.Pages
{
    /// <summary>
    /// Interaction logic for CustomerLedgerReceivablesManagementPage.xaml
    /// </summary>
    public partial class CustomerLedgerReceivablesManagementPage : Page
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

        private List<CustomerLedger> _customers;
        private List<CustomerLedger> _filteredCustomers;

        public CustomerLedgerReceivablesManagementPage()
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


            _filteredCustomers = new List<CustomerLedger>(_customers);
            
            cmbType.ItemsSource = new List<string> { "Buyer", "Seller", "Agent" };
            cmbStatus.ItemsSource = new List<string> { "Active", "Inactive" };
            
            cmbFilterType.Items.Add(new ComboBoxItem { Content = "All Types", IsSelected = true });
            cmbFilterType.Items.Add(new ComboBoxItem { Content = "Buyer" });
            cmbFilterType.Items.Add(new ComboBoxItem { Content = "Seller" });
            cmbFilterType.Items.Add(new ComboBoxItem { Content = "Agent" });
            
            cmbFilterStatus.Items.Add(new ComboBoxItem { Content = "All Status", IsSelected = true });
            cmbFilterStatus.Items.Add(new ComboBoxItem { Content = "Active" });
            cmbFilterStatus.Items.Add(new ComboBoxItem { Content = "Inactive" });

            dgCustomers.ItemsSource = _customers;
            ApplyFilters();
        }


        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter Name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string customerId = txtCNICNTN.Text?.Substring(0, Math.Min(8, txtCNICNTN.Text.Length)) ?? Guid.NewGuid().ToString().Substring(0, 8);
            
            var existingCustomer = _customers.FirstOrDefault(c => c.CNICNTN == txtCNICNTN.Text);
            if (existingCustomer != null)
            {
                // Update existing
                existingCustomer.Name = txtName.Text;
                existingCustomer.Contact = txtContact.Text;
                existingCustomer.Address = txtAddress.Text;
                existingCustomer.Type = cmbType.SelectedItem?.ToString() ?? "Buyer";
                existingCustomer.Status = cmbStatus.SelectedItem?.ToString() ?? "Active";
                MessageBox.Show("Customer updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Add new
                var newCustomer = new CustomerLedger
                {
                    CustomerId = "CUST" + (_customers.Count + 1).ToString("D3"),
                    Name = txtName.Text,
                    CNICNTN = txtCNICNTN.Text,
                    Contact = txtContact.Text,
                    Address = txtAddress.Text,
                    Type = cmbType.SelectedItem?.ToString() ?? "Buyer",
                    Status = cmbStatus.SelectedItem?.ToString() ?? "Active"
                };
                _customers.Add(newCustomer);
                MessageBox.Show("Customer added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            dgCustomers.ItemsSource = null;
            dgCustomers.ItemsSource = _customers;
            ApplyFilters();
            BtnClear_Click(sender, e);
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            txtName.Clear();
            txtCNICNTN.Clear();
            txtContact.Clear();
            txtAddress.Clear();
            cmbType.SelectedIndex = -1;
            cmbStatus.SelectedIndex = -1;
            dgCustomers.SelectedItem = null;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtCNICNTN.Text))
            {
                var selectedCustomer = _customers.FirstOrDefault(c => c.CNICNTN == txtCNICNTN.Text);
                if (selectedCustomer != null)
                {
                    var result = MessageBox.Show($"Are you sure you want to delete customer '{selectedCustomer.Name}'?", 
                        "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        _customers.Remove(selectedCustomer);
                        dgCustomers.ItemsSource = null;
                        dgCustomers.ItemsSource = _customers;
                        ApplyFilters();
                        BtnClear_Click(sender, e);
                        MessageBox.Show("Customer deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a customer to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select a customer to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DgCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgCustomers.SelectedItem is CustomerLedger selectedCustomer)
            {
                txtName.Text = selectedCustomer.Name;
                txtCNICNTN.Text = selectedCustomer.CNICNTN;
                txtContact.Text = selectedCustomer.Contact;
                txtAddress.Text = selectedCustomer.Address;
                cmbType.SelectedItem = selectedCustomer.Type;
                cmbStatus.SelectedItem = selectedCustomer.Status;
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CmbFilterType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CmbFilterStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            _filteredCustomers = _customers.Where(c =>
            {
                bool matchesSearch = string.IsNullOrWhiteSpace(txtSearch.Text) ||
                                    c.Name.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase) ||
                                    c.CNICNTN.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase) ||
                                    c.Contact.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase);
                
                bool matchesType = cmbFilterType.SelectedItem is ComboBoxItem typeItem &&
                                  (typeItem.Content.ToString() == "All Types" || c.Type == typeItem.Content.ToString());
                
                bool matchesStatus = cmbFilterStatus.SelectedItem is ComboBoxItem statusItem &&
                                    (statusItem.Content.ToString() == "All Status" || c.Status == statusItem.Content.ToString());
                
                return matchesSearch && matchesType && matchesStatus;
            }).ToList();

            // Update customer data grid with filtered results
            dgCustomers.ItemsSource = null;
            dgCustomers.ItemsSource = _filteredCustomers;
        }

        private void BtnExportPDF_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("PDF export functionality would be implemented here.\nThis would require a PDF library like iTextSharp or PdfSharp.", 
                "Export PDF", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Excel export functionality would be implemented here.\nThis would require a library like EPPlus or ClosedXML.", 
                "Export Excel", MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}
