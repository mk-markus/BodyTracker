using BodyTracker.MVVM.ViewModels;
using System.Windows;

namespace BodyTracker.MVVM.Views
{
    public partial class LoginWindow : Window
    {
        /// <summary>
        /// The view model for the login window.
        /// </summary>
        private readonly LoginViewModel loginViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginWindow"/> class.
        /// </summary>
        public LoginWindow()
        {
            InitializeComponent();
            loginViewModel = new LoginViewModel();
            this.DataContext = loginViewModel;
        }

        /// <summary>
        /// Handles the password box password changed event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is LoginViewModel lvm && sender is System.Windows.Controls.PasswordBox pb)
            {
                lvm.SqlPassword = pb.Password;
            }
        }

        /// <summary>
        /// Handles the login button click event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            await loginViewModel.LoginCommand.ExecuteAsync(null);
            this.DialogResult = true;

        }

        /// <summary>
        /// Handles the cancel button click event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}