using BodyTracker.Services;
using BodyTracker.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using System.Timers;

namespace BodyTracker.MVVM.Views
{
    /// <summary>
    /// Interaktionslogik für NewEntryPage.xaml
    /// </summary>
    public partial class NewEntryPage : Page
    {
        private readonly DataEntryViewModel _vm;
        private readonly MainWindow _shell;

        public event Action SwitchToMeasurements;


        public NewEntryPage(MainWindow shell, DatabaseService db)
        {
            InitializeComponent();
            _vm = new DataEntryViewModel(db);
            DataContext = _vm;
            _shell = shell;
            Loaded += async (s, e) => await _vm.InitializeAsync();
           
        }
        private void OnSaved(object sender, RoutedEventArgs e)
        {
           SwitchToMeasurements?.Invoke();
        }
        //private void OnClose(object sender, RoutedEventArgs e) => this.Close();

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
