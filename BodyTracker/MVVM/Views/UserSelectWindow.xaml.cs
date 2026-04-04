using BodyTracker.Services;
using BodyTracker.State;
using BodyTracker.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace BodyTracker.Views
{
    /// <summary>
    /// Represents a window that allows users to select or configure database connection settings and user profiles.
    /// Provides functionality for validating input, persisting credentials, and initializing the application's data
    /// context based on user selection.
    /// </summary>
    /// <remarks>This window is typically used at application startup to establish a connection to the
    /// database and select a user profile before proceeding to the main application interface. It supports data binding
    /// and input validation for connection parameters, and integrates with configuration and database services to
    /// manage persistence and retrieval of user and connection data. The class implements INotifyPropertyChanged to
    /// support property change notifications for data binding scenarios.</remarks>
    public partial class UserSelectWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// <remarks>This event is typically raised by classes that implement the INotifyPropertyChanged
        /// interface to notify clients, such as data-binding frameworks, that a property value has changed.</remarks>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event to notify listeners that a property value has changed.
        /// </summary>
        /// <remarks>Call this method in the setter of a property to notify subscribers that the
        /// property's value has changed. This is commonly used to implement the INotifyPropertyChanged interface in
        /// data-binding scenarios.</remarks>
        /// <param name="propertyName">The name of the property that changed. This value is optional and is automatically provided when called from
        /// a property setter.</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// Service responsible for managing application configuration settings, 
        /// including loading and saving database connection strings from the configuration file.
        /// </summary>
        private ConfigrationService configurationService;

        /// <summary>
        /// A private reference to the <see cref="DatabaseService"/>.
        /// This service acts as the primary data gateway for all persistence 
        /// operations initiated by the ViewModel.
        /// </summary>
        private DatabaseService? databaseService;

        /// <summary>
        /// Provides access to the underlying data store for person-related operations.
        /// This service handles low-level database communication and entity retrieval.
        /// </summary>
        private UserSelectViewModel? userSelectViewModel;


        private bool _isIpValid = true;
        public bool IsIpValid
        {
            get => _isIpValid;
            set
            {
                _isIpValid = value;
                OnPropertyChanged();
            }
        }

        private bool _isPortValid = true;
        public bool IsPortValid
        {
            get => _isPortValid;
            set
            {
                _isPortValid = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="UserSelectWindow"/> class.
        /// Loads the server configuration and sets up the data context if credentials are available.
        /// </summary>
        public UserSelectWindow()
        {
            InitializeComponent();
           
            //configurationService = new ConfigrationService(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
            configurationService = new ConfigrationService();

            var cfg = configurationService.LoadCinfigurationFile();
            
            if (!string.IsNullOrEmpty(cfg.User)) DbUserBox.Text = cfg.User;
            if (!string.IsNullOrEmpty(cfg.DatabaseName)) DbDatenbank.Text = cfg.DatabaseName;
            if (!string.IsNullOrEmpty(cfg.ServerIP)) DbServer.Text = cfg.ServerIP ;
            if (!string.IsNullOrEmpty(cfg.User)) pbPasswordBox.Password = "****";
            if (cfg.PortNumber != 0) DbPort.Text = cfg.PortNumber.ToString();

            if (!string.IsNullOrEmpty(cfg.User) && !string.IsNullOrEmpty(cfg.PasswordEnc) && !string.IsNullOrEmpty(cfg.DatabaseName))
            {
                var cs = configurationService.BuildConnectionString();
                
                databaseService = new DatabaseService(cs);
                
                userSelectViewModel = new UserSelectViewModel(databaseService);

                // Update the UI's DataContext to the new ViewModel
                DataContext = userSelectViewModel;

                Loaded += UserSelectWindow_Loaded;
            }
        }

        /// <summary>
        /// Handles the Window's Loaded event to perform asynchronous initialization.
        /// Validates the presence of user records and triggers data loading or user notification.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data providing details about the element being loaded.</param>
        private async void UserSelectWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var expression = BindingOperations.GetBindingExpression(DbServer, TextBox.TextProperty);
                expression?.ValidateWithoutUpdate();

                if (databaseService != null && userSelectViewModel != null)
                {
                    int count = await databaseService.CountPersonAsync();

                    if (count > 0 && userSelectViewModel != null)
                    {
                        await userSelectViewModel.InitializeAsync();
                    }
                    else
                    {
                        MessageBox.Show(
                            "No users were found. Please create a new profile.",
                            "Information",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection Error: {ex.Message}", "Error");
            }
        }

        /// <summary>
        /// Persists the entered database credentials, reconstructs the connection string, 
        /// and re-initializes the application's data context.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data regarding the click operation.</param>
        private async void OnSaveCredentials(object sender, RoutedEventArgs e)
        {
            configurationService.SaveCredentials(DbUserBox.Text.Trim(), pbPasswordBox.Password, DbServer.Text, Convert.ToInt16(DbPort.Text.Trim()), DbDatenbank.Text);
            var connectionString = configurationService.BuildConnectionString();
            
            databaseService = new DatabaseService(connectionString);
            
            userSelectViewModel = new UserSelectViewModel(databaseService);
            
            userSelectViewModel = await EtablisSQLConnection(userSelectViewModel);

            // Update the UI's DataContext to the new ViewModel
            this.DataContext = userSelectViewModel;

            MessageBox.Show("Connection saved!", 
                            "Information", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);
          
        }

        /// <summary>
        /// Attempts to establish a connection to the SQL server by initializing the provided ViewModel.
        /// If the connection or initialization fails, an error message is displayed to the user.
        /// </summary>
        /// <param name="userSelectViewModel">The ViewModel instance to be initialized with database data.</param>
        /// <returns>Returns the initialized <see cref="UserSelectViewModel"/> instance.</returns>
        public async Task<UserSelectViewModel> EtablisSQLConnection(UserSelectViewModel userSelectViewModel)
        {
            try
            {
                await userSelectViewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"The following error occurred: {ex.Message}", 
                                "Failure",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }

            return userSelectViewModel;
        }

        /// <summary>
        /// Monitors changes in the <see cref="PasswordBox"/> and updates the availability of the save command.
        /// Validates that the input is neither empty nor the default placeholder string.
        /// </summary>
        /// <param name="sender">The source of the event, typically the PasswordBox.</param>
        /// <param name="e">Event data regarding the password change.</param>
        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            string password = pbPasswordBox.Password;

            if (!string.IsNullOrEmpty(password) && password != "****")
            {
                btnSaveConnection.IsEnabled = true;
            }
            else
            {
                btnSaveConnection.IsEnabled = false;
            }
        }

        /// <summary>
        /// Handles the click event for loading user data.
        /// Validates the selection state and performs the transition to the <see cref="MainWindow"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data providing details about the click.</param>
        private void LoadUserdata_Click(object sender, RoutedEventArgs e)
        {
            if (userSelectViewModel == null)
            {
                MessageBox.Show("Please save the connection first.",
                                "Information",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }
            
            userSelectViewModel.ConfirmCommand.Execute(null);
            
            if (AppState.SelectedPersonId <= 0)
            {
                MessageBox.Show("No user has been selected. Please select a user!",
                                "Information",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }
            

            var main = new MainWindow(databaseService!);
            

            Application.Current.MainWindow = main;

            main.Show();
            
            this.Close();
        }

        /// <summary>
        /// Handles the TextChanged event for the database server input field, validating the entered IP address.
        /// </summary>
        /// <remarks>Updates the IsIpValid property based on whether the current text represents a valid
        /// IP address. This method is intended to be used as an event handler for text input validation
        /// scenarios.</remarks>
        /// <param name="sender">The source of the event, typically the database server text input control.</param>
        /// <param name="e">The event data associated with the text change.</param>
        private void DbServer_TextChanged(object sender, TextChangedEventArgs e)
        {

            var service = new ConfigrationService();
            var result = service.checkIPAdressOK(DbServer.Text);
            this.IsIpValid = result.Item1;
        }

        /// <summary>
        /// Handles the TextChanged event for the database port input field, updating the port validity state based on
        /// the entered value.
        /// </summary>
        /// <remarks>This method updates the IsPortValid property to indicate whether the current text
        /// represents a valid integer port number. This can be used to enable or disable related UI elements or to
        /// provide user feedback.</remarks>
        /// <param name="sender">The source of the event, typically the database port text input control.</param>
        /// <param name="e">The event data associated with the text change.</param>
        private void DbPort_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (!int.TryParse(DbPort.Text, out var port)) IsPortValid = false;
            else IsPortValid = true;
        }

        /// <summary>
        /// Intercepts text input to ensure consistent decimal formatting. 
        /// Automatically replaces a period (".") with a comma (",") in real-time.
        /// </summary>
        /// <param name="sender">The source of the event, typically a <see cref="TextBox"/> configured for numeric input.</param>
        /// <param name="e">The <see cref="TextCompositionEventArgs"/> containing the input text to be processed.</param>
        /// <remarks>
        /// This method enhances user experience by allowing the use of the numeric keypad's period key 
        /// while maintaining compatibility with culture-specific decimal parsing (e.g., German "de-DE").
        /// It manually manipulates the <see cref="TextBox.Text"/> and manages the <see cref="TextBox.SelectionStart"/> 
        /// to ensure the cursor position remains intuitive after the replacement.
        /// </remarks
        private void Decimal_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".")
            {
                if (sender is TextBox tb)
                {
                    e.Handled = true;
                    var selStart = tb.SelectionStart;
                    tb.Text = tb.Text.Insert(selStart, ",");
                    tb.SelectionStart = selStart + 1;
                }
            }
        }


    }
}
