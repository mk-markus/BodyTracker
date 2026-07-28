using BodyTracker.MVVM.Views.Pages;
using BodyTracker.Services;
using System.Windows;

namespace BodyTracker
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// This service acts as the primary data gateway for all persistence 
        /// operations initiated by the ViewModel.
        /// </summary>
        private readonly DatabaseService _db;

        public MainWindow(DatabaseService db)
        {
            this.WindowState = WindowState.Maximized;

            InitializeComponent();

            _db = db;
            
            //var entry = new StartPage(this, databaseServerice);
            var entry = new StartPage(this, _db);

            MainFrame.Navigate(entry);
        }
    }
}
