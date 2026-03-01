using BodyTracker.Services;
using BodyTracker.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BodyTracker.MVVM.Views
{
    /// <summary>
    /// Interaktionslogik für NewEntryPage.xaml
    /// </summary>
    public partial class NewEntryPage : Page
    {

        /// <summary>
        /// A private, read-only reference to the <see cref="DataEntryViewModel"/>.
        /// This instance acts as the primary logic controller for the entry view, 
        /// managing the temporary state of input fields and coordinating the validation 
        /// before data is committed to the persistence layer.
        /// </summary>
        /// <remarks>
        /// The ViewModel provides the necessary properties for Two-Way-Binding within the 
        /// <see cref="NewEntryPage"/>, ensuring that user input is immediately captured and processed.
        /// </remarks>
        private readonly DataEntryViewModel dataEntryViewModel;
        
        /// <summary>
        /// 
        /// </summary>
        private readonly MainWindow _shell;

        /// <summary>
        /// A private, read-only reference to the <see cref="MainWindow"/> instance.
        /// This field facilitates communication with the application's "Shell," enabling the 
        /// current page to trigger navigation requests and access global UI state or status indicators.
        /// </summary>
        /// <remarks>
        /// By holding a reference to the main window, the page can participate in the 
        /// application's overall navigation lifecycle, such as switching back to the measurement history 
        /// after a successful data entry.
        /// </remarks>
        public event Action SwitchToMeasurements;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewEntryPage"/> class.
        /// Configures the view-model, establishes the data context, and registers 
        /// an asynchronous initialization routine upon the page's loaded event.
        /// </summary>
        /// <param name="shell">The main application window instance, used for navigation and shell-level control.</param>
        /// <param name="db">The database service providing the necessary data access layer for entries.</param>
        /// <remarks>
        /// This constructor utilizes the <see cref="FrameworkElement.Loaded"/> event to invoke 
        /// <see cref="DataEntryViewModel.InitializeAsync"/>, ensuring that all necessary 
        /// background data (e.g., lookup tables or user state) is ready before user interaction begins.
        /// </remarks>
        public NewEntryPage(MainWindow shell, DatabaseService db)
        {
            InitializeComponent();
            dataEntryViewModel = new DataEntryViewModel(db);
            DataContext = dataEntryViewModel;
            _shell = shell;
            Loaded += async (s, e) => await dataEntryViewModel.InitializeAsync();
        }

        /// <summary>
        /// Handles the save completion event. Invokes the <see cref="SwitchToMeasurements"/> 
        /// action to notify the shell that the data entry process is finished 
        /// and a navigation change to the measurement overview is required.
        /// </summary>
        /// <param name="sender">The source of the event, typically the Save <see cref="Button"/>.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// This method does not perform the database operation itself; instead, it triggers the 
        /// UI transition. The actual data persistence is expected to be handled by the 
        /// ViewModel or an upstream command before this event is raised.
        /// </remarks>
        private async void OnSaved(object sender, RoutedEventArgs e)
        {
            if (dataEntryViewModel != null)
            {
                string[] values = { dataEntryViewModel.Bmi.ToString(), dataEntryViewModel.BodyWeight.ToString(),
                                    dataEntryViewModel.BodyFatPercentage.ToString(), dataEntryViewModel.BodyMusclePercentage.ToString(),
                                    dataEntryViewModel.BodyVisceralFat.ToString(), dataEntryViewModel.ChestCircumference.ToString(),
                                    dataEntryViewModel.WaistCircumference.ToString(), dataEntryViewModel.HipsCircumference.ToString(),
                                    dataEntryViewModel.FatTongs.ToString()};

                if(dataEntryViewModel.CheckBodyValuesValid(values))
                {
                    await dataEntryViewModel.SaveAsync();
                    SwitchToMeasurements?.Invoke();
                }
               else
                {
                    MessageBox.Show("The values are not valid. Check if some value boxes has an red frame and correct the value", "Waring",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }



            }
        }

        /// <summary>
        /// Handles the <see cref="UIElement.GotKeyboardFocus"/> event for input fields.
        /// Automatically selects the entire text within a <see cref="TextBox"/> to facilitate 
        /// immediate overwriting or editing by the user.
        /// </summary>
        /// <param name="sender">The source of the event, expected to be a <see cref="TextBox"/>.</param>
        /// <param name="e">The <see cref="KeyboardFocusChangedEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// This UX enhancement reduces the number of required clicks or keystrokes for the user, 
        /// as it eliminates the need to manually clear the field before entering a new value. 
        /// It is particularly useful in data-heavy forms like measurement entries.
        /// </remarks>
        private void SelectAllOnFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox tb) tb.SelectAll();
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
