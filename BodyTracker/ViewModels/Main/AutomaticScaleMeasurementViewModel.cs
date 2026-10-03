using BF105;
using BF105.Bluetooth.Model;
using BodyTracker.Models.FullBodyMeasurement;
using BodyTracker.Resources.Languages;
using BodyTracker.Services;
using BodyTracker.Services.WorkoutLog;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace BodyTracker.ViewModels.Main
{
    /// <summary>
    /// Represents the view model for the automated scale measurement workflow, managing multi-phase 
    /// Bluetooth hardware communication, state-based UI steps, countdown timers, and cleanup routines.
    /// </summary>
    public partial class AutomaticScaleMeasurementViewModel : ObservableObject
    {
        #region Observable Properties

        /// <summary>
        /// Gets or sets the image source representing the current visual state of the measurement process.
        /// </summary>
        [ObservableProperty]
        private ImageSource? myImageSource;

        /// <summary>
        /// Gets or sets the resource path string pointing to the active instructional or status image.
        /// </summary>
        [ObservableProperty]
        private string pathImage;

        /// <summary>
        /// Gets or sets a value indicating whether the automated measurement sequence has completed successfully.
        /// </summary>
        [ObservableProperty]
        private bool measurementFinished;

        /// <summary>
        /// Gets or sets the header badge text string displayed in the scale measurement view.
        /// </summary>
        [ObservableProperty]
        private string txtScaleHeaderBadge;

        /// <summary>
        /// Gets or sets the main header title string representing the active scale operation phase.
        /// </summary>
        [ObservableProperty]
        private string txtScaleHeaderTitle;

        /// <summary>
        /// Gets or sets the introductory description text guiding the user through the current measurement step.
        /// </summary>
        [ObservableProperty]
        private string txtScaleIntro;

        /// <summary>
        /// Gets or sets the status message text indicating real-time feedback during hardware operations.
        /// </summary>
        [ObservableProperty]
        private string txtScaleStatusMessage;

        /// <summary>
        /// Gets or sets the formatted countdown text string displayed upon completion of the measurement sequence.
        /// </summary>
        [ObservableProperty]
        private string txtScaleCountdownText;

        /// <summary>
        /// Gets or sets the collection of step items with corresponding icons and instructions for the current phase.
        /// </summary>
        [ObservableProperty]
        private string currentScaleIconStepsList;

        /// <summary>
        /// Gets or sets a value indicating whether the scale measurement sequence has completed.
        /// </summary>
        [ObservableProperty]
        private bool isScaleMeasurementFinished = false;

        #endregion

        #region Relay Commands

        /// <summary>
        /// Gets the asynchronous command that confirms measurement results, disposes hardware resources, and closes the window.
        /// </summary>
        public IRelayCommand CommandConfirm { get; }

        /// <summary>
        /// Gets the asynchronous command that cancels the ongoing workflow, disposes resources, and closes the window.
        /// </summary>
        public IRelayCommand CommandCancel { get; }

        #endregion

        #region Members

        /// <summary>
        /// Stores the final aggregated body measurement model retrieved from the scale upon successful completion.
        /// </summary>
        public BodyMeasurementModel? Result;

        /// <summary>
        /// Holds a reference to the parent window shell hosting this view model.
        /// </summary>
        private Window _shell;

        /// <summary>
        /// Holds an instance of the Beurer BF105 Bluetooth scale wrapper utilized for hardware communication.
        /// </summary>
        private BF105BeurerWaage _scale;

        /// <summary>
        /// Holds an instance of the application settings service utilized for loading, saving, 
        /// and managing configuration parameters and user preferences.
        /// </summary>
        private AppSettingsService _appService = new AppSettingsService();

        /// <summary>
        /// Manages the countdown timer interval for automatically confirming and closing upon completion.
        /// </summary>
        private DispatcherTimer _countdownTimer;

        /// <summary>
        /// Tracks the remaining seconds left in the completion countdown timer.
        /// </summary>
        private int _countdownSecondsLeft = 10;

        /// <summary>
        /// 
        /// </summary>
        private BodyScanMode.ScanMode _bodyScanMode;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticScaleMeasurementViewModel"/> class, 
        /// sets up commands, loads configuration credentials to initialize the scale instance, 
        /// sets up the countdown timer, and kicks off the background measurement sequence.
        /// </summary>
        /// <param name="shell">The parent window shell instance.</param>
        /// <param name="bodyScanMode">The body scan mode enumeration value.</param>
        public AutomaticScaleMeasurementViewModel(Window shell, BodyScanMode.ScanMode bodyScanMode)
        {
            _shell = shell;

            _bodyScanMode = bodyScanMode;

            CommandConfirm = new AsyncRelayCommand(Confirm);
            CommandCancel = new AsyncRelayCommand(Cancel);

            var cfg = _appService.LoadConfigurationFile();
            int.TryParse(cfg.ScaleUser, out int user);
            int.TryParse(CryptoHelper.Unprotect(cfg.ScaleUserPin), out int pin);

            _scale = new BF105BeurerWaage(user, pin);

            _countdownTimer = new DispatcherTimer();
            _countdownTimer.Interval = TimeSpan.FromSeconds(1);
            _countdownTimer.Tick += CountdownTimer_Tick;

            _ = InitializeMeasurementSequenceAsync();
        }

        /// <summary>
        /// Asynchronously initializes the step item collection and kicks off the automated measurement pipeline.
        /// </summary>
        /// <returns>A task representing the asynchronous initialization workflow.</returns>
        private async Task InitializeMeasurementSequenceAsync()
        {
            MeasurementFinished = await StartMeasurement();
        }

        /// <summary>
        /// Orchestrates the multi-stage Bluetooth measurement pipeline, including device connection, 
        /// standard weight and composition acquisition, segmental analysis, and UI phase data updates.
        /// </summary>
        /// <returns>A task containing a boolean value indicating whether all measurement phases completed successfully.</returns>
        private async Task<bool> StartMeasurement()
        {

            bool _statusMeasurement = false;
            if (_scale == null) return false;

            LoadConnectionData();

            await _scale.ConnectToScaleAsync();

            if (!_scale.IsConnected) return false;


            switch (_bodyScanMode)
            {
                case BodyScanMode.ScanMode.BodyWeightAndCompositionScan:
                    LoadInitialMeasurementData();
                    await _scale.GetBodyCompositionAndWeightAsync(false);

                    _statusMeasurement = _scale.IsBodyMeasurementValid &&
                        _scale.IsBodyCompositionMeasurementValid;
                    break;

                case BodyScanMode.ScanMode.SegmentalBodyCompisitionScan:
                    
                    LoadSegmentMeasurementData();
                    
                    await _scale.GetSegmentalBodyCompostionAndWeightAsync(false);
                    
                    _statusMeasurement = _scale.IsBodyMeasurementValid &&
                        _scale.IsSegmentalBodyCompisitionMeasurementValid;
                    
                    break;

                case BodyScanMode.ScanMode.FullBodyScan:
                    
                    LoadInitialMeasurementData();
                    
                    await _scale.GetBodyCompositionAndWeightAsync(false);
                    _statusMeasurement = _scale.IsBodyMeasurementValid &&
                        _scale.IsBodyCompositionMeasurementValid;
                    
                    if(_statusMeasurement)
                    {
                        LoadSegmentMeasurementData();
                        await _scale.GetSegmentalBodyCompostionAndWeightAsync(false);
                        _statusMeasurement = _scale.IsBodyMeasurementValid &&
                            _scale.IsSegmentalBodyCompisitionMeasurementValid;
                    }

                    break;
            }

            if(_statusMeasurement)
            {
                LoadFinishedData();

                return true;
            }
            else return false;
        }

        /// <summary>
        /// Populates UI properties and icon step items corresponding to the initial device connection phase.
        /// </summary>
        private void LoadConnectionData()
        {
            TxtScaleHeaderBadge = UiStrings.TxtScaleConnectionBadge;
            TxtScaleHeaderTitle = UiStrings.TxtScaleConnectionTitle;
            TxtScaleIntro = UiStrings.TxtScaleConnectionIntro;
            TxtScaleStatusMessage = UiStrings.TxtScaleConnectionStatus;

            CurrentScaleIconStepsList = $"🔌 {UiStrings.TxtScaleConnText1}\n" +
                                        $"📡 {UiStrings.TxtScaleConnText2}\n" +
                                        $"⏳ {UiStrings.TxtScaleConnText3}";


        }

        /// <summary>
        /// Populates UI properties and icon step items corresponding to the initial body measurement phase.
        /// </summary>
        private void LoadInitialMeasurementData()
        {
            TxtScaleHeaderBadge = UiStrings.TxtScaleInitialBadge;
            TxtScaleHeaderTitle = UiStrings.TxtScaleInitialTitle;
            TxtScaleIntro = UiStrings.TxtScaleInitialIntro;
            TxtScaleStatusMessage = UiStrings.TxtScaleInitialStatus;

            CurrentScaleIconStepsList = $"👤 {UiStrings.TxtScaleInitText1}\n" +
                                        $"⚖️ {UiStrings.TxtScaleInitText2}\n" +
                                        $"🤲 {UiStrings.TxtScaleInitText3}";
        }

        /// <summary>
        /// Populates UI properties and icon step items corresponding to the advanced segmental analysis phase.
        /// </summary>
        private void LoadSegmentMeasurementData()
        {
            TxtScaleHeaderBadge = UiStrings.TxtScaleSegmentBadge;
            TxtScaleHeaderTitle = UiStrings.TxtScaleSegmentTitle;
            TxtScaleIntro = UiStrings.TxtScaleSegmentIntro;
            TxtScaleStatusMessage = UiStrings.TxtScaleSegmentStatus;

            CurrentScaleIconStepsList = $"⏻ {UiStrings.TxtScaleSegText1}\n" +
                                        $"⚡ {UiStrings.TxtScaleSegText2}\n" +
                                        $"📋 {UiStrings.TxtScaleSegText3}\n" +
                                        $"🧍 {UiStrings.TxtScaleSegText4}\n" +
                                        $"✋ {UiStrings.TxtScaleSegText5}";
        }

        /// <summary>
        /// Populates UI properties for the successful measurement completion phase and triggers the countdown timer.
        /// </summary>
        private void LoadFinishedData()
        {
            IsScaleMeasurementFinished = true;
            StartCountdown();
            TxtScaleHeaderBadge = UiStrings.TxtScaleFinishedBadge;
            TxtScaleHeaderTitle = UiStrings.TxtScaleFinishedTitle;
            TxtScaleIntro = UiStrings.TxtScaleFinishedDesc;
            TxtScaleStatusMessage = UiStrings.TxtScaleFinishedStatus;
        }

        /// <summary>
        /// Handles the dispatcher timer tick event by decrementing the countdown counter, 
        /// updating the status text, or invoking confirmation when time expires.
        /// </summary>
        /// <param name="sender">The source of the timer tick event.</param>
        /// <param name="e">An object containing event data.</param>
        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            _countdownSecondsLeft--;
            if (_countdownSecondsLeft > 0)
            {
                TxtScaleCountdownText = $"{UiStrings.TxtScaleCountdown} {_countdownSecondsLeft}s ...";
            }
            else
            {
                StopCountdown();

                _ = Confirm();
            }
        }

        /// <summary>
        /// Stops and unregisters the active countdown timer if it is initialized.
        /// </summary>
        private void StopCountdown()
        {
            if (_countdownTimer != null)
            {
                _countdownTimer.Stop();
                _countdownTimer.Tick -= CountdownTimer_Tick;
                _countdownTimer = null;
            }
        }

        /// <summary>
        /// Initializes and starts the countdown timer for automatic window confirmation.
        /// </summary>
        private void StartCountdown()
        {
            if (_countdownTimer == null)
            {
                _countdownTimer = new DispatcherTimer();
                _countdownTimer.Interval = TimeSpan.FromSeconds(1);
                _countdownTimer.Tick += CountdownTimer_Tick;
            }
            _countdownTimer.Start();
        }

        /// <summary>
        /// Confirms the successful measurement results by assigning the scale's data model to the result property, 
        /// setting the dialog outcome to true, disposing hardware resources, and closing the parent window.
        /// </summary>
        /// <returns>A task representing the asynchronous confirmation workflow.</returns>
        private async Task Confirm()
        {
            if (MeasurementFinished)
            {
                Debug.WriteLine("Confirm Start");
                Result = _scale.BodyMeasurement;
                _shell.DialogResult = true;
                await _scale.Dispose();
                await Task.Delay(2000);
                Debug.WriteLine("Confirm finished");

                _shell.Close();
            }
            else return;
        }

        /// <summary>
        /// Cancels the ongoing process, disposes scale resources, stops timers, and safely closes the parent window shell.
        /// </summary>
        /// <returns>A task representing the asynchronous cancellation and disposal operation.</returns>
        private async Task Cancel()
        {
            await Dispose();

            if (_shell != null)
            {
                _shell.Close();
            }
        }

        /// <summary>
        /// Asynchronously releases scale resources and stops any active countdown timers.
        /// </summary>
        /// <returns>A task representing the asynchronous disposal operation.</returns>
        public async Task Dispose()
        {
            await _scale.Dispose();
            StopCountdown();
        }
    }

    /// <summary>
    /// Represents an individual step item containing an icon and descriptive text for display during scale measurement phases.
    /// </summary>
    public class ScaleStepItem
    {
        /// <summary>
        /// Gets or sets the emoji or icon string representing the step item.
        /// </summary>
        public string StepIcon { get; set; }

        /// <summary>
        /// Gets or sets the descriptive instruction text explaining the step item.
        /// </summary>
        public string StepDescription { get; set; }
    }
}