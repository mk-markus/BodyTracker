using BodyTracker.Models;
using BodyTracker.ViewModels;
using BodyTracker.Views.windows;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace BodyTracker.Views.Pages
{
    /// <summary>
    /// Represents the launcher home page within the application, managing user selection, initialization, visibility changes, and data grid interactions.
    /// </summary>
    public partial class LauncherHomePage : Page
    {
        /// <summary>
        /// Represents the parent launcher window instance hosting this page.
        /// </summary>
        private readonly LauncherWindow launcherWindow;

        /// <summary>
        /// Represents the view model that handles the business logic, commands, and data binding for the launcher home page.
        /// </summary>
        private readonly LauncherDashboardViewModel launcherHomePageViewModel;

        /// <summary>
        /// Initializes a new instance of the LauncherHomePage class, configuring component UI elements, establishing window references, instantiating the view model, and registering event handlers for unloading and visibility changes.
        /// </summary>
        /// <param name="shell">The parent launcher window instance hosting the application navigation.</param>
        public LauncherHomePage(LauncherWindow shell)
        {
            InitializeComponent();
            launcherWindow = shell;
            launcherHomePageViewModel = new LauncherDashboardViewModel(launcherWindow);
            DataContext = launcherHomePageViewModel;

            this.Unloaded += (s, e) => launcherHomePageViewModel.Dispose();

            this.IsVisibleChanged += LauncherHomePage_IsVisibleChanged;
        }

        /// <summary>
        /// Handles the mouse double-click event on the person grid, executing the command to load the body tracker tool for the selected person.
        /// </summary>
        /// <param name="sender">The source of the event, typically the grid control.</param>
        /// <param name="e">An instance containing mouse button event data.</param>
        private void PersonGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            launcherHomePageViewModel.CommandLoadBodyTrackerToolAsync.Execute(null);
        }

        /// <summary>
        /// Handles the visibility changed event for the page, asynchronously initializing the view model and configuring default ascending sorting by PersonID on the data collection view when the page becomes visible.
        /// </summary>
        /// <param name="sender">The source of the event, typically the page.</param>
        /// <param name="e">An instance containing dependency property change event data.</param>
        private async void LauncherHomePage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue) return; // e.NewValue is true when the page becomes visible

            try
            {
                await launcherHomePageViewModel.InitializeAsync();

                if (launcherHomePageViewModel?.Persons != null)
                {
                    ICollectionView view = CollectionViewSource.GetDefaultView(launcherHomePageViewModel.Persons);
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
    }
}
