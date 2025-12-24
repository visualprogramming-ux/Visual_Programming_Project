using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Project.Pages
{
    /// <summary>
    /// Interaction logic for ProjectManagementPage.xaml
    /// </summary>
    public partial class ProjectManagementPage : Page
    {
        public class Project
        {
            public string ProjectId { get; set; }
            public string ProjectName { get; set; }
            public string Location { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }
        }

        private List<Project> _projects;

        public ProjectManagementPage()
        {
            InitializeComponent();
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            _projects = new List<Project>
            {
                new Project { ProjectId = "PRJ001", ProjectName = "Sunset Residency", Location = "Downtown", Description = "Luxury residential complex", Status = "Active" },
                new Project { ProjectId = "PRJ002", ProjectName = "Green Valley", Location = "Suburbs", Description = "Eco-friendly housing project", Status = "Active" },
                new Project { ProjectId = "PRJ003", ProjectName = "City Center Plaza", Location = "City Center", Description = "Commercial and residential mixed", Status = "Completed" }
            };

            dgProjects.ItemsSource = _projects;
            cmbStatus.ItemsSource = new List<string> { "Active", "In Progress", "Completed", "On Hold" };
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProjectId.Text) || string.IsNullOrWhiteSpace(txtProjectName.Text))
            {
                MessageBox.Show("Please enter Project ID and Project Name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existingProject = _projects.FirstOrDefault(p => p.ProjectId == txtProjectId.Text);
            if (existingProject != null)
            {
                // Update existing
                existingProject.ProjectName = txtProjectName.Text;
                existingProject.Location = txtLocation.Text;
                existingProject.Description = txtDescription.Text;
                existingProject.Status = cmbStatus.SelectedItem?.ToString() ?? "Active";
                MessageBox.Show("Project updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Add new
                var newProject = new Project
                {
                    ProjectId = txtProjectId.Text,
                    ProjectName = txtProjectName.Text,
                    Location = txtLocation.Text,
                    Description = txtDescription.Text,
                    Status = cmbStatus.SelectedItem?.ToString() ?? "Active"
                };
                _projects.Add(newProject);
                MessageBox.Show("Project added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            dgProjects.ItemsSource = null;
            dgProjects.ItemsSource = _projects;
            BtnClear_Click(sender, e);
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            txtProjectId.Clear();
            txtProjectName.Clear();
            txtLocation.Clear();
            txtDescription.Clear();
            cmbStatus.SelectedIndex = -1;
            dgProjects.SelectedItem = null;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgProjects.SelectedItem is Project selectedProject)
            {
                var result = MessageBox.Show($"Are you sure you want to delete project '{selectedProject.ProjectName}'?", 
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    _projects.Remove(selectedProject);
                    dgProjects.ItemsSource = null;
                    dgProjects.ItemsSource = _projects;
                    BtnClear_Click(sender, e);
                    MessageBox.Show("Project deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select a project to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DgProjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgProjects.SelectedItem is Project selectedProject)
            {
                txtProjectId.Text = selectedProject.ProjectId;
                txtProjectName.Text = selectedProject.ProjectName;
                txtLocation.Text = selectedProject.Location;
                txtDescription.Text = selectedProject.Description;
                cmbStatus.SelectedItem = selectedProject.Status;
            }
        }
    }
}



