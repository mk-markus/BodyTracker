using BodyTracker.MVVM.ViewModels;
using BodyTracker.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BodyTracker.MVVM.Views
{
    /// <summary>
    /// Interaktionslogik für NewPersonWindow.xaml
    /// </summary>
    public partial class NewPersonWindow : Window
    {
        /// <summary>
        /// Represents the ViewModel associated with the NewPersonWindow, responsible for handling the logic and data binding for creating a new person entry in the application.
        /// </summary>
        private NewPersonViewModel newPersonviewModel;

        /// <summary>
        /// Represents the constructor for the <see cref="NewPersonWindow"/> class, which initializes the ViewModel with a reference to the <see cref="DatabaseService"/> and sets up the command for creating a new person.
        /// </summary>
        /// <param name="db">The <see cref="DatabaseService"/> instance used for data operations.</param>
        public NewPersonWindow(DatabaseService db)
        {
            InitializeComponent();

            newPersonviewModel = new NewPersonViewModel(db);

            this.DataContext = newPersonviewModel;

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

        /// <summary>
        /// Closes the current window when the cancel button is clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Executes the save command for the new user and closes the dialog upon success.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private async void btnSaveNewUser_Click(object sender, RoutedEventArgs e)
        {
            await newPersonviewModel.NewUserCommand.ExecuteAsync(null);
            this.DialogResult = true;
        }
    }
}
