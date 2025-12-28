using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Project.Data;

namespace Project.Pages
{
    /// <summary>
    /// Interaction logic for PlotManagementPage.xaml
    /// </summary>
    public partial class PlotManagementPage : Page
    {
        public class Plot
        {
            public int PlotIdDb { get; set; }
            public string PlotNo { get; set; } = string.Empty;
            public string ProjectName { get; set; } = string.Empty;
            public decimal Size { get; set; }
            public decimal Price { get; set; }
            public string Status { get; set; } = string.Empty;
            public int ProjectIdDb { get; set; }
            public int? OwnerId { get; set; }
            public string OwnerName { get; set; } = string.Empty;
        }

        private List<Plot> _plots = new();
        private List<ProjectInfo> _projectList = new();
        private List<PartyInfo> _ownerList = new();

        public class PartyInfo
        {
            public int PartyId { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        public class ProjectInfo
        {
            public int ProjectId { get; set; }
            public string ProjectName { get; set; } = string.Empty;
        }

        public PlotManagementPage()
        {
            InitializeComponent();
            try
            {
                LoadProjects();
                LoadOwners();
                LoadDataFromDatabase();
                cmbStatus.ItemsSource = new List<string> { "Available", "Reserved", "Sold", "Booked" };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}\n\nPlease ensure the database is set up correctly.", 
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadOwners()
        {
            try
            {
                // Load all parties with Type='Buyer' as potential owners
                // Note: Party Type is a role (Buyer/Seller/Agent/Partner)
                // Owner is a relationship (Plots.OwnerId), not a Party Type
                var buyers = PartyDataAccess.GetAllBuyers();
                _ownerList = buyers.Select(b => new PartyInfo
                {
                    PartyId = int.Parse(b.BuyerId),
                    Name = b.Name
                }).ToList();

                // Add "None" option for plots without owners
                _ownerList.Insert(0, new PartyInfo { PartyId = 0, Name = "(No Owner)" });

                if (cmbOwner != null)
                {
                    cmbOwner.ItemsSource = _ownerList;
                    cmbOwner.DisplayMemberPath = "Name";
                    cmbOwner.SelectedValuePath = "PartyId";
                }
            }
            catch
            {
                // Silently fail - OwnerId column might not exist yet
                _ownerList = new List<PartyInfo> { new PartyInfo { PartyId = 0, Name = "(No Owner)" } };
                if (cmbOwner != null)
                {
                    cmbOwner.ItemsSource = _ownerList;
                    cmbOwner.DisplayMemberPath = "Name";
                    cmbOwner.SelectedValuePath = "PartyId";
                }
            }
        }

        private void LoadProjects()
        {
            try
            {
                var projects = ProjectDataAccess.GetAllProjects();
                _projectList = projects.Select(p => new ProjectInfo
                {
                    ProjectId = int.Parse(p.ProjectId),
                    ProjectName = p.ProjectName
                }).ToList();

                if (cmbProject != null)
                {
                    cmbProject.ItemsSource = _projectList;
                    cmbProject.DisplayMemberPath = "ProjectName";
                    cmbProject.SelectedValuePath = "ProjectId";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading projects: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LoadDataFromDatabase()
        {
            try
            {
                var plotList = PlotManagementDataAccess.GetAllPlots();
                _plots = plotList.Select(p => new Plot
                {
                    PlotIdDb = p.PlotId,
                    PlotNo = p.PlotNo,
                    ProjectName = p.ProjectName,
                    Size = p.SizeMarla,
                    Price = p.Price,
                    Status = p.Status,
                    ProjectIdDb = p.ProjectId,
                    OwnerId = p.OwnerId,
                    OwnerName = p.OwnerName
                }).ToList();

                if (dgPlots != null)
                {
                    dgPlots.ItemsSource = null;
                    dgPlots.ItemsSource = _plots;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading plots: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private Plot? _selectedPlot;

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPlotNo.Text))
            {
                MessageBox.Show("Please enter Plot Number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbProject.SelectedItem == null)
            {
                MessageBox.Show("Please select a Project.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtSize.Text, out decimal sizeMarla) || sizeMarla <= 0)
            {
                MessageBox.Show("Please enter a valid size in marla.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Please enter a valid price.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var selectedProject = (ProjectInfo)cmbProject.SelectedItem;
                string status = cmbStatus.SelectedItem?.ToString() ?? "Available";
                
                // Get owner ID (0 means no owner)
                int? ownerId = null;
                if (cmbOwner?.SelectedItem is PartyInfo selectedOwner && selectedOwner.PartyId > 0)
                {
                    ownerId = selectedOwner.PartyId;
                }

                if (_selectedPlot != null)
                {
                    // Update existing
                    PlotManagementDataAccess.UpdatePlot(
                        _selectedPlot.PlotIdDb,
                        selectedProject.ProjectId,
                        txtPlotNo.Text.Trim(),
                        sizeMarla,
                        price,
                        status,
                        ownerId
                    );
                }
                else
                {
                    // Create new
                    int plotId = PlotManagementDataAccess.InsertPlot(
                        selectedProject.ProjectId,
                        txtPlotNo.Text.Trim(),
                        sizeMarla,
                        price,
                        status,
                        ownerId
                    );
                }

                LoadDataFromDatabase();
                MessageBox.Show("Plot saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                BtnClear_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving plot: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            _selectedPlot = null;
            txtPlotNo.Clear();
            if (cmbProject != null) cmbProject.SelectedIndex = -1;
            txtSize.Clear();
            txtPrice.Clear();
            if (cmbStatus != null) cmbStatus.SelectedIndex = -1;
            if (cmbOwner != null) cmbOwner.SelectedIndex = 0; // Select "No Owner"
            if (dgPlots != null) dgPlots.SelectedItem = null;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPlot == null)
            {
                MessageBox.Show("Please select a plot to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete plot '{_selectedPlot.PlotNo}'?", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    PlotManagementDataAccess.DeletePlot(_selectedPlot.PlotIdDb);
                    LoadDataFromDatabase();
                    BtnClear_Click(sender, e);
                    MessageBox.Show("Plot deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting plot: {ex.Message}", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DgPlots_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgPlots.SelectedItem is Plot selectedPlot)
            {
                _selectedPlot = selectedPlot;
                txtPlotNo.Text = selectedPlot.PlotNo;
                
                var project = _projectList.FirstOrDefault(p => p.ProjectId == selectedPlot.ProjectIdDb);
                if (project != null)
                {
                    cmbProject.SelectedItem = project;
                }
                
                txtSize.Text = selectedPlot.Size.ToString("N2");
                txtPrice.Text = selectedPlot.Price.ToString("N2");
                
                // Find and select the status in the combobox
                if (cmbStatus.ItemsSource is List<string> statusList)
                {
                    var statusIndex = statusList.IndexOf(selectedPlot.Status);
                    if (statusIndex >= 0)
                    {
                        cmbStatus.SelectedIndex = statusIndex;
                    }
                    else
                    {
                        cmbStatus.SelectedIndex = -1;
                    }
                }
                
                // Set owner
                if (selectedPlot.OwnerId.HasValue && cmbOwner != null)
                {
                    var owner = _ownerList.FirstOrDefault(o => o.PartyId == selectedPlot.OwnerId.Value);
                    if (owner != null)
                    {
                        cmbOwner.SelectedItem = owner;
                    }
                    else
                    {
                        cmbOwner.SelectedIndex = 0; // Select "No Owner"
                    }
                }
                else if (cmbOwner != null)
                {
                    cmbOwner.SelectedIndex = 0; // Select "No Owner"
                }
            }
        }

        private void BtnAddOwner_Click(object sender, RoutedEventArgs e)
        {
            // Show a dialog to add new owner
            var dialog = new AddOwnerDialog
            {
                Owner = Application.Current.MainWindow
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Insert new party
                    int newPartyId = PartyManagementDataAccess.InsertParty(
                        dialog.OwnerName,
                        "Buyer", // Type = Buyer
                        dialog.CNIC,
                        dialog.ContactPhone,
                        dialog.ContactEmail,
                        dialog.Address
                    );

                    // Reload owners list
                    LoadOwners();
                    
                    // Select the newly added owner
                    if (cmbOwner != null)
                    {
                        var newOwner = _ownerList.FirstOrDefault(o => o.PartyId == newPartyId);
                        if (newOwner != null)
                        {
                            cmbOwner.SelectedItem = newOwner;
                        }
                    }

                    MessageBox.Show("Owner added successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding owner: {ex.Message}", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    // Simple dialog for adding new owner
    public class AddOwnerDialog : Window
    {
        public string OwnerName { get; private set; } = string.Empty;
        public string? CNIC { get; private set; }
        public string? ContactPhone { get; private set; }
        public string? ContactEmail { get; private set; }
        public string? Address { get; private set; }

        private TextBox txtName = new();
        private TextBox txtCNIC = new();
        private TextBox txtPhone = new();
        private TextBox txtEmail = new();
        private TextBox txtAddress = new();

        public AddOwnerDialog()
        {
            Title = "Add New Owner";
            Width = 520;
            Height = 550;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            var mainPanel = new StackPanel { Margin = new Thickness(20) };
            
            // Info text
            var infoText = new TextBlock
            {
                Text = "Data will be saved to the Parties table in the database.",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                Margin = new Thickness(0, 0, 0, 15),
                TextWrapping = TextWrapping.Wrap
            };
            mainPanel.Children.Add(infoText);

            var stackPanel = new StackPanel();

            // Name (required)
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = "Owner Name *", 
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            });
            txtName.Margin = new Thickness(0, 0, 0, 15);
            txtName.Height = 30;
            stackPanel.Children.Add(txtName);

            // CNIC
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = "CNIC", 
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            });
            txtCNIC.Margin = new Thickness(0, 0, 0, 15);
            txtCNIC.Height = 30;
            stackPanel.Children.Add(txtCNIC);

            // Contact Phone
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = "Contact Phone", 
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            });
            txtPhone.Margin = new Thickness(0, 0, 0, 15);
            txtPhone.Height = 30;
            stackPanel.Children.Add(txtPhone);

            // Contact Email
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = "Contact Email", 
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            });
            txtEmail.Margin = new Thickness(0, 0, 0, 15);
            txtEmail.Height = 30;
            stackPanel.Children.Add(txtEmail);

            // Address
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = "Address", 
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            });
            txtAddress.Margin = new Thickness(0, 0, 0, 20);
            txtAddress.Height = 60;
            txtAddress.TextWrapping = TextWrapping.Wrap;
            txtAddress.AcceptsReturn = true;
            stackPanel.Children.Add(txtAddress);
            
            mainPanel.Children.Add(stackPanel);

            // Buttons
            var buttonPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var btnOK = new Button 
            { 
                Content = "Save", 
                Width = 100, 
                Height = 35,
                Margin = new Thickness(0, 0, 10, 0),
                FontSize = 14,
                FontWeight = FontWeights.Medium
            };
            btnOK.Click += BtnOK_Click;

            var btnCancel = new Button 
            { 
                Content = "Cancel", 
                Width = 100, 
                Height = 35,
                FontSize = 14
            };
            btnCancel.Click += (s, e) => DialogResult = false;

            buttonPanel.Children.Add(btnOK);
            buttonPanel.Children.Add(btnCancel);
            mainPanel.Children.Add(buttonPanel);

            Content = mainPanel;
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter Owner Name.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            OwnerName = txtName.Text.Trim();
            CNIC = string.IsNullOrWhiteSpace(txtCNIC.Text) ? null : txtCNIC.Text.Trim();
            ContactPhone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim();
            ContactEmail = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim();
            Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim();

            DialogResult = true;
        }
    }
}



