using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BodyTracker.ViewModels;
using BodyTracker.Services;

namespace BodyTracker.Views
{
    public partial class DataEntryWindow : Window
    {
        private readonly NewDataEntryViewModel _vm;
        public DataEntryWindow(DatabaseService db)
        {
            InitializeComponent();
            _vm = new NewDataEntryViewModel(db);
            DataContext = _vm;
            Loaded += async (s,e) => await _vm.InitializeAsync();
        }
        private void OnSaved(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; this.Close();
        }
        private void OnClose(object sender, RoutedEventArgs e) => this.Close();
        private void SelectAllOnFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox tb) tb.SelectAll();
        }


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
