using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project.Data;

namespace Project.Pages
{
    /// <summary>
    /// Interaction logic for CustomerLedgerReceivablesManagementPage.xaml
    /// </summary>
    public partial class CustomerLedgerReceivablesManagementPage : Page
    {
        public class CustomerLedger
        {
            public int PartyId { get; set; }
            public string? Name { get; set; }
            public string? CNICNTN { get; set; }
            public string? Contact { get; set; }
            public string? Address { get; set; }
            public string? Type { get; set; }
            public string? Status { get; set; }
        }

        private List<CustomerLedger> _customers = new List<CustomerLedger>();
        private List<CustomerLedger> _filteredCustomers = new List<CustomerLedger>();
        private CustomerLedger? _selectedCustomer;

        public CustomerLedgerReceivablesManagementPage()
        {
            InitializeComponent();
            InitializeComboBoxes();
            LoadDataFromDatabase();
        }

        private void InitializeComboBoxes()
        {
            cmbType.ItemsSource = new List<string> { "Buyer", "Seller", "Agent" };
            cmbStatus.ItemsSource = new List<string> { "Active", "Inactive" };
            
            cmbFilterType.Items.Add(new ComboBoxItem { Content = "All Types", IsSelected = true });
            cmbFilterType.Items.Add(new ComboBoxItem { Content = "Buyer" });
            cmbFilterType.Items.Add(new ComboBoxItem { Content = "Seller" });
            cmbFilterType.Items.Add(new ComboBoxItem { Content = "Agent" });
            
            cmbFilterStatus.Items.Add(new ComboBoxItem { Content = "All Status", IsSelected = true });
            cmbFilterStatus.Items.Add(new ComboBoxItem { Content = "Active" });
            cmbFilterStatus.Items.Add(new ComboBoxItem { Content = "Inactive" });
        }

        private void LoadDataFromDatabase()
        {
            try
            {
                var parties = PartyDataAccess.GetAllParties();
                _customers = parties.Select(p => new CustomerLedger
                {
                    PartyId = p.PartyId,
                    Name = p.Name ?? "",
                    CNICNTN = p.CNIC ?? "",
                    Contact = p.ContactPhone ?? "",
                    Address = p.Address ?? "",
                    Type = p.Type ?? "",
                    Status = p.Status ?? "Active"
                }).ToList();

                _filteredCustomers = new List<CustomerLedger>(_customers);
                dgCustomers.ItemsSource = null;
                dgCustomers.ItemsSource = _filteredCustomers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customers: {ex.Message}\n\nPlease ensure the database is set up correctly.", 
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter Name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string name = txtName.Text.Trim();
                string cnic = txtCNICNTN.Text?.Trim() ?? "";
                string contact = txtContact.Text?.Trim() ?? "";
                string address = txtAddress.Text?.Trim() ?? "";
                string type = cmbType.SelectedItem?.ToString() ?? "Buyer";
                string status = cmbStatus.SelectedItem?.ToString() ?? "Active";

                if (_selectedCustomer != null)
                {
                    // Update existing
                    PartyManagementDataAccess.UpdateParty(
                        _selectedCustomer.PartyId,
                        name,
                        type,
                        string.IsNullOrWhiteSpace(cnic) ? null : cnic,
                        string.IsNullOrWhiteSpace(contact) ? null : contact,
                        null, // ContactEmail - not in UI
                        string.IsNullOrWhiteSpace(address) ? null : address,
                        status
                    );
                    MessageBox.Show("Customer updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Check if CNIC already exists
                    var existingParty = _customers.FirstOrDefault(c => c.CNICNTN == cnic && !string.IsNullOrWhiteSpace(cnic));
                    if (existingParty != null)
                    {
                        MessageBox.Show("A customer with this CNIC/NTN already exists. Please select it to update.", 
                            "Duplicate CNIC", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Add new
                    int partyId = PartyManagementDataAccess.InsertParty(
                        name,
                        type,
                        string.IsNullOrWhiteSpace(cnic) ? null : cnic,
                        string.IsNullOrWhiteSpace(contact) ? null : contact,
                        null, // ContactEmail - not in UI
                        string.IsNullOrWhiteSpace(address) ? null : address,
                        status
                    );
                    MessageBox.Show("Customer added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                LoadDataFromDatabase();
                BtnClear_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving customer: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            _selectedCustomer = null;
            txtName.Clear();
            txtCNICNTN.Clear();
            txtContact.Clear();
            txtAddress.Clear();
            cmbType.SelectedIndex = -1;
            cmbStatus.SelectedIndex = -1;
            if (dgCustomers != null) dgCustomers.SelectedItem = null;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCustomer == null)
            {
                MessageBox.Show("Please select a customer to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete customer '{_selectedCustomer.Name}'?", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    PartyManagementDataAccess.DeleteParty(_selectedCustomer.PartyId);
                    LoadDataFromDatabase();
                    BtnClear_Click(sender, e);
                    MessageBox.Show("Customer deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting customer: {ex.Message}", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DgCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgCustomers.SelectedItem is CustomerLedger selectedCustomer)
            {
                _selectedCustomer = selectedCustomer;
                txtName.Text = selectedCustomer.Name ?? "";
                txtCNICNTN.Text = selectedCustomer.CNICNTN ?? "";
                txtContact.Text = selectedCustomer.Contact ?? "";
                txtAddress.Text = selectedCustomer.Address ?? "";
                
                // Set combobox selections - find the item in the ItemsSource
                if (cmbType.ItemsSource is List<string> typeList)
                {
                    var selectedType = selectedCustomer.Type ?? "";
                    cmbType.SelectedItem = typeList.FirstOrDefault(t => t == selectedType);
                }
                
                if (cmbStatus.ItemsSource is List<string> statusList)
                {
                    var selectedStatus = selectedCustomer.Status ?? "";
                    cmbStatus.SelectedItem = statusList.FirstOrDefault(s => s == selectedStatus);
                }
            }
            else
            {
                _selectedCustomer = null;
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
            // Guard against null or empty customers list (prevents crash during initialization)
            if (_customers == null || _customers.Count == 0)
            {
                _filteredCustomers = new List<CustomerLedger>();
                if (dgCustomers != null)
                {
                    dgCustomers.ItemsSource = null;
                    dgCustomers.ItemsSource = _filteredCustomers;
                }
                return;
            }

            _filteredCustomers = _customers.Where(c =>
            {
                bool matchesSearch = string.IsNullOrWhiteSpace(txtSearch.Text) ||
                                    (c.Name != null && c.Name.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)) ||
                                    (c.CNICNTN != null && c.CNICNTN.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)) ||
                                    (c.Contact != null && c.Contact.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase));
                
                bool matchesType = cmbFilterType.SelectedItem is ComboBoxItem typeItem &&
                                  (typeItem.Content.ToString() == "All Types" || (c.Type ?? "") == typeItem.Content.ToString());
                
                bool matchesStatus = cmbFilterStatus.SelectedItem is ComboBoxItem statusItem &&
                                    (statusItem.Content.ToString() == "All Status" || (c.Status ?? "") == statusItem.Content.ToString());
                
                return matchesSearch && matchesType && matchesStatus;
            }).ToList();

            // Update customer data grid with filtered results
            if (dgCustomers != null)
            {
                dgCustomers.ItemsSource = null;
                dgCustomers.ItemsSource = _filteredCustomers;
            }
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
