using System;
using System.Windows;
using System.Windows.Controls;
using Project.Pages;

namespace Project
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            
            // Load default page
            ContentFrame.Navigate(new ProjectManagementPage());
        }
        
        private void BtnProjectManagement_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new ProjectManagementPage());
        }
        
        private void BtnPlotManagement_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new PlotManagementPage());
        }
        
        private void BtnPlotVisualDashboard_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new PlotVisualDashboardPage());
        }
        
        private void BtnOwnershipMapping_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new OwnershipMappingPage());
        }
        
        private void BtnModule48_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContentFrame.Navigate(new Page48Page());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error navigating to Installments & Payment Plans page: {ex.Message}\n\n{ex.StackTrace}", 
                    "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void BtnModule410_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContentFrame.Navigate(new CustomerLedgerReceivablesManagementPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error navigating to Customer Ledger page: {ex.Message}\n\n{ex.StackTrace}", 
                    "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnLedgerView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContentFrame.Navigate(new LedgerViewPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error navigating to Ledger View page: {ex.Message}\n\n{ex.StackTrace}", 
                    "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnQuickEntry_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContentFrame.Navigate(new QuickEntryPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error navigating to Quick Entry page: {ex.Message}\n\n{ex.StackTrace}", 
                    "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAgingReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContentFrame.Navigate(new ReceivablesAgingReportPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error navigating to Aging Report page: {ex.Message}\n\n{ex.StackTrace}", 
                    "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCustomerStatements_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContentFrame.Navigate(new CustomerStatementsPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error navigating to Customer Statements page: {ex.Message}\n\n{ex.StackTrace}", 
                    "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
    }
}

