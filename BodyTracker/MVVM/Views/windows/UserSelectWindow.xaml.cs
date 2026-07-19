using BodyTracker.MVVM.Models;
using BodyTracker.MVVM.Views;
using BodyTracker.Services;
using BodyTracker.State;
using BodyTracker.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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

        /// <summary>
        /// Represents the persisted SQL configuration settings loaded from the configuration file.
        /// </summary>
        private SQLConfigurationModel sqlConfigurationModel;


        /// <summary>
        /// Initializes a new instance of the <see cref="UserSelectWindow"/> class.
        /// Loads the server configuration and sets up the data context if credentials are available.
        /// </summary>
        public UserSelectWindow()
        {
            InitializeComponent();
            configurationService = new ConfigrationService();
            sqlConfigurationModel = configurationService.LoadConfigurationFile();


            if (!string.IsNullOrEmpty(sqlConfigurationModel.User) &&
                 !string.IsNullOrEmpty(sqlConfigurationModel.PasswordEnc) &&
                 !string.IsNullOrEmpty(sqlConfigurationModel.DatabaseName))
            {
                InitializeComponentsUserSelectedWindow();
                Loaded += UserSelectWindow_Loaded;
                this.IsVisibleChanged += UserSelectPage_IsVisibleChanged;
            }
        }

        /// <summary>
        /// Creates a fresh DatabaseService and ViewModel based on the persisted configuration
        /// and binds the ViewModel to the window.
        /// </summary>
        private void InitializeComponentsUserSelectedWindow()
        {
            var connectionString = configurationService.BuildConnectionString();

            databaseService = new DatabaseService(connectionString);
            userSelectViewModel = new UserSelectViewModel(databaseService);
            userSelectViewModel.UpdateConnectionInfo(sqlConfigurationModel);
            DataContext = userSelectViewModel;
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
                if (databaseService != null && userSelectViewModel != null)
                {
                    int count = await databaseService.GetPersonCountAsync();

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
        /// Handles the RowEditEnding event for the PersonsGrid DataGrid.
        /// Commits changes to the database when a row edit is completed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data providing details about the row being edited.</param>
        private void PersonsGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;

            if (e.Row.Item is not PersonModel editedRow) return;

            Dispatcher.BeginInvoke(new Action(async () =>
            {
                int personId = editedRow.PersonID;

                if (DataContext is UserSelectViewModel vm && e.Row.Item != null)
                {
                    // Typ der Person ggf. anpassen, siehe Kommentar in ViewModel
                    await userSelectViewModel.UpdatePersonAsync(personId, editedRow);
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        /// <summary>
        /// Handles the click event for creating a new login.
        /// Opens the LoginWindow and initializes the user selection window upon successful login.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data providing details about the click.</param>
        private async void btnNewLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            var result = loginWindow.ShowDialog();

            if (result == true)
            {
                sqlConfigurationModel = configurationService.LoadConfigurationFile();

                InitializeComponentsUserSelectedWindow();

                if (userSelectViewModel == null)
                    return;

                bool success = await userSelectViewModel.InitializeAsync();

                if (success && userSelectViewModel.Persons.Count == 0)
                {
                    MessageBox.Show(
                        "No users were found. Please create a new profile.",
                        "Information",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show(
                    "The login was aborted and no new connection was established.",
                    "Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Handles the click event for creating a new person.
        /// Opens the NewPersonWindow and initializes the user selection window upon successful creation.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data providing details about the click.</param>
        private async void btnNewPerson_Click(object sender, RoutedEventArgs e)
        {
            NewPersonWindow newPersonWindow = new NewPersonWindow(databaseService);
            var result = newPersonWindow.ShowDialog();

            if (result == true)
            {
                if (userSelectViewModel == null)
                    return;

                bool success = await userSelectViewModel.InitializeAsync();

                if (success && userSelectViewModel.Persons.Count == 0)
                {
                    MessageBox.Show(
                        "No users were found. Please create a new profile.",
                        "Information",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show(
                    "The login was aborted and no new connection was established.",
                    "Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Handles the click event for loading user data.
        /// Validates the selection state and performs the transition to the <see cref="MainWindow"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data providing details about the click.</param>
        private void btnLoadUserdata_Click(object sender, RoutedEventArgs e)
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
        /// Automatically refreshes the measurements whenever the page becomes visible.
        /// This ensures data consistency when switching between different tabs/pages.
        /// </summary>
        private async void UserSelectPage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        { 
            if (!(bool)e.NewValue) return; // e.NewValue is true when the page becomes visible

            try
            {
                await userSelectViewModel.InitializeAsync();

               
                if (userSelectViewModel?.Persons != null)
                {
                    ICollectionView view = CollectionViewSource.GetDefaultView(userSelectViewModel.Persons);
                    view.SortDescriptions.Clear();
                    view.SortDescriptions.Add(new SortDescription(nameof(PersonModel.PersonID), ListSortDirection.Ascending));
                    view.Refresh();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Refrehing Errror: {ex.Message}");
            }
            
        }

        private void PersonGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Debug.WriteLine("Double click on PersonGrid detected.");
            btnLoadUserdata_Click(sender, e);
        }
    }
}
