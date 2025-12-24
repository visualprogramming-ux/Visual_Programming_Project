using System;
using System.Text;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using Project;

namespace Project.Pages
{
    public partial class LoginPage : Page
    {
        private readonly string _connectionString =
            "Server=DESKTOP-44MO1B2\\SQLEXPRESS;" +
            "Database=RealEstateDB;" +
            "Trusted_Connection=True;" +
            "TrustServerCertificate=True;";

        public LoginPage()
        {
            InitializeComponent();

            Loaded += LoginPage_Loaded;
            txtPassword.KeyDown += TxtPassword_KeyDown;
        }

        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            txtUsername.Focus();
        }

        private void BtnForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Please contact your system administrator to reset your password.",
                "Forgot Password",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnLogin_Click(this, new RoutedEventArgs());
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            if (email == "" || password == "")
            {
                ShowError("Please enter email and password.");
                return;
            }

            if (CheckLogin(email, password))
            {
                txtErrorMessage.Visibility = Visibility.Collapsed;
                NavigationService.Navigate(new MainPage());
            }
            else
            {
                ShowError("Incorrect email or password.");
                txtPassword.Clear();
            }
        }

        private void ShowError(string message)
        {
            txtErrorMessage.Text = message;
            txtErrorMessage.Visibility = Visibility.Visible;
        }


        private bool CheckLogin(string email, string password)
        {
            try
            {
                string hashedPassword = HashPassword(password);
                
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(
                    "SELECT COUNT(1) FROM Users WHERE Email = @Email AND PasswordHash = @PasswordHash AND IsActive = 1", 
                    conn);

                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);

                conn.Open();
                return (int)cmd.ExecuteScalar() == 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private string HashPassword(string password)
        {
            using SHA256 sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
