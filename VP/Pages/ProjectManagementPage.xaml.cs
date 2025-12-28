using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project.Data;

namespace Project.Pages
{
    /// <summary>
    /// Interaction logic for ProjectManagementPage.xaml
    /// </summary>
    public partial class ProjectManagementPage : Page
    {
        public class Project
        {
            public int ProjectIdDb { get; set; }
            public string ProjectId { get; set; } = string.Empty;
            public string ProjectName { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
        }

        private List<Project> _projects = new();

        public ProjectManagementPage()
        {
            InitializeComponent();
            try
            {
                LoadDataFromDatabase();
                cmbStatus.ItemsSource = new List<string> { "Active", "In Progress", "Completed", "On Hold" };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading projects: {ex.Message}\n\nPlease ensure the database is set up correctly.", 
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDataFromDatabase()
        {
            try
            {
                var projectList = ProjectDataAccess.GetAllProjects();
                _projects = projectList.Select(p => new Project
                {
                    ProjectIdDb = int.Parse(p.ProjectId),
                    ProjectId = "PRJ" + p.ProjectId.PadLeft(3, '0'),
                    ProjectName = p.ProjectName,
                    Location = p.Location,
                    Status = p.Status
                }).ToList();

                if (dgProjects != null)
                {
                    dgProjects.ItemsSource = null;
                    dgProjects.ItemsSource = _projects;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading projects: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProjectName.Text))
            {
                MessageBox.Show("Please enter Project Name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string location = txtLocation.Text;
                string status = cmbStatus.SelectedItem?.ToString() ?? "Active";

                if (_selectedProject != null)
                {
                    // Update existing
                    ProjectDataAccess.UpdateProject(
                        _selectedProject.ProjectIdDb,
                        txtProjectName.Text,
                        location,
                        status
                    );
                }
                else
                {
                    // Add new
                    int projectId = ProjectDataAccess.InsertProject(
                        txtProjectName.Text,
                        location,
                        status
                    );
                }

                LoadDataFromDatabase();
                MessageBox.Show("Project saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                BtnClear_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving project: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Project? _selectedProject;

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            _selectedProject = null;
            txtProjectId.Clear();
            txtProjectName.Clear();
            txtLocation.Clear();
            cmbStatus.SelectedIndex = -1;
            if (dgProjects != null)
            {
                dgProjects.SelectedItem = null;
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProject == null)
            {
                MessageBox.Show("Please select a project to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete project '{_selectedProject.ProjectName}'?", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    ProjectDataAccess.DeleteProject(_selectedProject.ProjectIdDb);
                    LoadDataFromDatabase();
                    BtnClear_Click(sender, e);
                    MessageBox.Show("Project deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    // Display the error message - it should already be user-friendly from DeleteProject
                    string errorMessage = ex.Message;
                    
                    // Clean up any duplicate prefixes
                    while (errorMessage.Contains("Error deleting project: Error deleting project:"))
                    {
                        errorMessage = errorMessage.Replace("Error deleting project: Error deleting project:", "Error deleting project:");
                    }
                    
                    // If message doesn't start with our custom message, it's a raw error - make it user-friendly
                    if (!errorMessage.Contains("Cannot delete project") && 
                        !errorMessage.Contains("An error occurred"))
                    {
                        // Extract the meaningful part if it's a SQL error
                        if (errorMessage.Contains("REFERENCE constraint"))
                        {
                            errorMessage = "Cannot delete project. This project has related records (plots, sales, etc.) that must be deleted first.";
                        }
                    }
                    
                    MessageBox.Show(errorMessage, 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DgProjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgProjects.SelectedItem is Project selectedProject)
            {
                _selectedProject = selectedProject;
                txtProjectId.Text = selectedProject.ProjectId;
                txtProjectName.Text = selectedProject.ProjectName;
                txtLocation.Text = selectedProject.Location;
                
                // Find and select the status in the combobox
                if (cmbStatus.ItemsSource is List<string> statusList)
                {
                    var statusIndex = statusList.IndexOf(selectedProject.Status);
                    if (statusIndex >= 0)
                    {
                        cmbStatus.SelectedIndex = statusIndex;
                    }
                    else
                    {
                        cmbStatus.SelectedIndex = -1;
                    }
                }
            }
            else
            {
                // Clear selection
                _selectedProject = null;
            }
        }
    }
}



