using BodyTracker.Models;
using BodyTracker.Services;
using BodyTracker.State;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BodyTracker.ViewModels.Main
{
    public partial class DBEntryHeartRateViewModel : ObservableObject
    {
        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// This service acts as the primary data gateway for all persistence 
        /// operations initiated by the ViewModel.
        /// </summary>
        /// <remarks>
        /// Marked as <c>readonly</c> to ensure that the service reference remains 
        /// immutable throughout the lifetime of the ViewModel instance, preventing 
        /// accidental reassignment and ensuring architectural stability.
        /// </remarks>
        private readonly DatabaseService databaseService;

        #region Observiable Properties

        /// <summary>
        /// Gets or sets the collection of heart rate records currently displayed in the UI.
        /// Uses <see cref="ObservableCollection{T}"/> to automatically notify the UI 
        /// of additions, removals, or list clears.
        /// </summary>
        [ObservableProperty] private ObservableCollection<SamsungHeartRateModel> heartRateDatas = new();

        /// <summary>
        /// Gets or sets the currently selected heart rate record from the list.
        /// Nullable, as no record may be selected.
        /// </summary>
        [ObservableProperty] private SamsungHeartRateModel? selectedData;

        /// <summary>
        /// Gets or sets the name of the person currently being viewed.
        /// Used for display purposes in headers or titles.
        /// </summary>
        [ObservableProperty] private string userName = string.Empty;

        #endregion

        #region Relay Commans Async | Sync


        /// <summary>
        /// Gets the command responsible for handling the completion of a row edit operation.
        /// Triggers the necessary persistence updates when modifications are committed in the UI.
        /// </summary>
        public IAsyncRelayCommand CommandRowEditEnding { get; }

        /// <summary>
        /// Gets the command that initiates the deletion of the currently selected heart rate record.
        /// This operation removes the record from the database and updates the UI collection.
        /// </summary>
        /// <remarks>
        /// This command should typically check if <see cref="selectedData"/> is not null 
        /// before execution (via CanExecute logic).
        /// </remarks>
        public IAsyncRelayCommand CommandDelete { get; }

        /// <summary>
        /// Gets the command responsible for refreshing the heart rate history from the database.
        /// Triggers an asynchronous reload of the data collections.
        /// </summary>
        public IAsyncRelayCommand CommandRefresh { get; }

        #endregion

        /// <summary>
        /// Backing field for the general error message string.
        /// </summary>
        private string generalErrorMessage = "";

        /// <summary>
        /// Gets or sets the general error message, sending a database error message via the messenger when the value changes.
        /// </summary>
        public string GeneralErrorMessage
        {
            get => generalErrorMessage;
            set
            {
                generalErrorMessage = value;
                WeakReferenceMessenger.Default.Send(new DatabaseErrorMessage(generalErrorMessage));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DBEntryFoodIntakeViewModel"/> class.
        /// Sets up database access, initializes asynchronous commands, and retrieves 
        /// context information from the global application state.
        /// </summary>
        /// <param name="db">
        /// An injected instance of the <see cref="DatabaseService"/> used for 
        /// historical data retrieval and record deletion.
        /// </param>
        /// <remarks>
        /// The constructor links commands to their respective asynchronous implementations 
        /// and ensures that the <see cref="CommandDelete"/> is governed by selection-based 
        /// execution logic (<see cref="CanDelete"/>).
        /// </remarks>
        public DBEntryHeartRateViewModel(DatabaseService db)
        {
            databaseService = db;
            CommandDelete = new AsyncRelayCommand(DeleteSelectedAsync, CanDelete);
            CommandRefresh = new AsyncRelayCommand(ReloadAsync);
            CommandRowEditEnding = new AsyncRelayCommand<DataGridRowEditEndingEventArgs>(DataGrid_RowEditEnding);
            UserName = AppState.SelectedPersonName;
            _ = InitializeAsync();
        }

        /// <summary>
        /// Triggers the initial asynchronous loading sequence for the ViewModel.
        /// </summary>
        /// <returns>A task that represents the initialization process.</returns>
        /// <remarks>
        /// This method acts as a wrapper for <see cref="ReloadAsync"/>. By isolating the 
        /// initial load in this method, the ViewModel remains compatible with common 
        /// asynchronous initialization patterns in WPF/MVVM architectures.
        /// </remarks>
        public async Task InitializeAsync()
        {
            await ReloadAsync();
        }

        /// <summary>
        /// Synchronizes the local collection with the heart rate data stored in the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous reload operation.</returns>
        /// <remarks>
        /// The process involves retrieving the selected person's identifier and fetching their latest heart rate records.
        /// </remarks>
        private async Task ReloadAsync()
        {
            var pid = AppState.SelectedPersonId;

            await GetCurrentHeartRate(pid);
        }

        /// <summary>
        /// Asynchronously retrieves the current heart rate records for the specified person identifier from the database and updates the observable collection.
        /// </summary>
        /// <param name="pid">The unique person identifier used to query current heart rate entries.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task GetCurrentHeartRate(int pid)
        {
            var all = await databaseService.GetSamsungHeartRateSqlAsync(pid);

            HeartRateDatas.Clear();
            foreach (var m in all)
            {
                HeartRateDatas.Add(m);
            }
            OnPropertyChanged(nameof(HeartRateDatas));
        }

        /// <summary>
        /// Asynchronously retrieves the initial heart rate records for the specified person identifier from the database and updates the observable collection.
        /// </summary>
        /// <param name="pid">The unique person identifier used to query initial heart rate entries.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task GetInitialFoodIntakes(int pid)
        {
            return;
            //var all = await databaseService.GetSamsungFoodIntakeSqlAsync(pid);

            //InitialFoodIntakeDatas.Clear();
            //foreach (var m in all)
            //{
            //    InitialFoodIntakeDatas.Add(m);
            //}
            //OnPropertyChanged(nameof(InitialFoodIntakeDatas));
        }

        /// <summary>
        /// Evaluates whether the delete operation can currently be performed.
        /// </summary>
        /// <returns>
        /// <c>true</c> if a heart rate record is selected in the UI (<see cref="SelectedData"/> is not null); 
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method is used as the predicate for the <see cref="CommandDelete"/>. 
        /// In WPF, the command's associated UI element (e.g., a Button) will be 
        /// automatically enabled or disabled based on this return value.
        /// </remarks>
        private bool CanDelete()
        {
            return SelectedData != null;
        }

        /// <summary>
        /// Executed automatically by the Source Generator whenever the <see cref="SelectedData"/> property changes.
        /// </summary>
        /// <param name="value">The new selected data record (or null if deselected).</param>
        /// <remarks>
        /// This method ensures the UI remains responsive by forcing the <see cref="CommandDelete"/> 
        /// to re-evaluate its execution logic (<see cref="CanDelete"/>). 
        /// It utilizes a safe cast to <see cref="AsyncRelayCommand"/> to trigger the notification.
        /// </remarks>
        partial void OnSelectedDataChanged(SamsungHeartRateModel? value)
        {
            (CommandDelete as AsyncRelayCommand)?.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Asynchronously deletes the currently selected heart rate record from the database.
        /// It removes the entry based on its unique heart rate identifier.
        /// </summary>
        /// <returns>A task representing the asynchronous deletion and subsequent UI refresh.</returns>
        /// <remarks>
        /// This method performs a confirmation check via a message box before executing the deletion. 
        /// After the database operation is complete, it triggers <see cref="ReloadAsync"/> to 
        /// synchronize the UI collection with the updated database state.
        /// </remarks>
        private async Task DeleteSelectedAsync()
        {
            try
            {
                var result = MessageBox.Show("Are you sure you want to delete the selected data?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;
                if (SelectedData == null) return;
                if (SelectedData.HeartRateID.HasValue) await databaseService.GetHeartRateDeleteSqlAsync(SelectedData.HeartRateID.Value);
                await ReloadAsync();
            }
            catch (Exception ex)
            {
                GeneralErrorMessage = $"Delete error: {ex}";
            }

        }

        /// <summary>
        /// Asynchronously updates an existing heart rate record or inserts a new one into the database.
        /// This method acts as a wrapper for the data access layer, ensuring that all heart rate data 
        /// is persisted before triggering a full UI refresh.
        /// </summary>
        /// <param name="personId">The unique identifier of the person to whom the heart rate belongs.</param>
        /// <param name="row">The view model containing the heart rate data to be synchronized.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// The "Upsert" logic (Update or Insert) is determined by the presence of existing IDs 
        /// within the <paramref name="row"/>. Post-execution, <see cref="ReloadAsync"/> is invoked 
        /// to ensure the local collection remains consistent with the database state, 
        /// including any server-generated identifiers.
        /// </remarks>
        public async Task UpdateRowDataAsync(int personId, SamsungHeartRateModel row)
        {
            return;
        }

        /// <summary>
        /// Handles the <see cref="DataGrid.RowEditEnding"/> event to persist modified heart rate data to the database.
        /// This method ensures that only committed changes are processed.
        /// </summary>
        /// <param name="e">Event data containing the edit action and the row being edited.</param>
        private async Task DataGrid_RowEditEnding(DataGridRowEditEndingEventArgs e)
        {
            if (e == null) return;

            if (e.EditAction != DataGridEditAction.Commit) return;

            if (e.Row.Item is not SamsungHeartRateModel editedRow) return;

            int personId = AppState.SelectedPersonId;
            await UpdateRowDataAsync(personId, editedRow);
        }


        #region Disposal Pattern

        /// <summary>
        /// Tracks whether the object has been disposed to prevent double disposal.
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources, suppressing finalization.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of the Dispose pattern, releasing managed resources such as the database service and unregistering messenger listeners when disposing is true.
        /// </summary>
        /// <param name="disposing">A value indicating whether managed resources should be released.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                Debug.WriteLine($"{this.GetType().Name} Disposing managed resources {GetHashCode()}");

                try
                {
                    databaseService?.Dispose();
                    WeakReferenceMessenger.Default.UnregisterAll(this);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{this.GetType().Name} Disposal Error: {ex.Message}");
                }
            }

            disposed = true;
        }

        #endregion
    }
}
