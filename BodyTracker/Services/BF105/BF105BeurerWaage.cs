using ABI.Windows.ApplicationModel.Activation;
using BF105.Bluetooth.Model;
using BF105.Bluetooth.Service;
using Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace BF105
{
    public class BF105BeurerWaage
    {
        /// <summary>
        /// Stores the cached Bluetooth physical address (MAC address as a 64-bit unsigned integer) 
        /// of the BF105 scale device, enabling direct reconnection without requiring a full device discovery scan.
        /// </summary>
        private ulong? _bf105BluetoothAddress;

        private string _loggingFileName;

        /// <summary>
        /// Stores the zero-based index of the currently active user profile interacting with the scale.
        /// </summary>
        private int _userIndex;

        /// <summary>
        /// Stores the unique numerical identification code associated with the active user.
        /// </summary>
        private int _userPin;

        /// <summary>
        /// Holds a list of registered user profiles retrieved from or synchronized with the scale device.
        /// </summary>
        private List<UserListModel>? _userList;

        /// <summary>
        /// Gets the list of registered user models retrieved from the scale. 
        /// Returns the cached user list collection if initialized; otherwise, returns an empty list instance.
        /// </summary>
        public List<UserListModel> UserList
        {
            get => _userList ?? new List<UserListModel>();
        }

        /// <summary>
        /// Specifies the constant model identifier string used to validate the target scale hardware (BF105).
        /// </summary>
        private const string _scaleModelName = "BF105";

        /// <summary>
        /// Encapsulates the specific Bluetooth GATT characteristics, UUIDs, and parsing logic for the BF105 scale model.
        /// </summary>
        private BF105Characteristic _bF105Characteristics = new BF105Characteristic();

        /// <summary>
        /// Manages the Bluetooth LE advertisement watcher responsible for scanning, 
        /// filtering, and discovering nearby peripheral devices asynchronously.
        /// </summary>
        private BluetoothLEAdvertisementWatcher? _bleWatcher;

        /// <summary>
        /// Represents a remote Bluetooth Low Energy device, providing access to its 
        /// connection state, properties, and GATT services.
        /// </summary>
        private BluetoothLEDevice? _bluetoothLeDevice;

        /// <summary>
        /// Stores the latest raw or processed body composition data received from the scale.
        /// </summary>
        private BodyCompositionMeasurementModel? _bodyCompositionMeasurementResult;

        /// <summary>
        /// Stores the latest raw or processed body weight data received from the scale.
        /// </summary>
        private BodyWeightMeasurementModel? _bodyWeightMeasurementResult;

        /// <summary>
        /// Stores the latest raw or processed segmetnal body composition data received from the scale.
        /// </summary>
        private SegmentalBodyCompositionMeasurementModel? _segmentalBodyCompositionMeasurementResult;

        /// <summary>
        /// Gets a consolidated data transfer model combining current body composition and weight measurement.s.
        /// </summary>
        public BodyMeasurementModel BodyMeasurement
        {
            get
            {
                return new BodyMeasurementModel()
                {
                    BodyCompositionMeasurement = _bodyCompositionMeasurementResult,
                    BodyWeightMeasurement = _bodyWeightMeasurementResult,
                    SegmentalBodyCompositionMeasurement = _segmentalBodyCompositionMeasurementResult
                };
            }
        }

        /// <summary>
        /// Manages the asynchronous completion state for the device connection and initialization sequence.
        /// </summary>
        private TaskCompletionSource<bool>? _isConnectToScaleValidationCompleted;

        /// <summary>
        /// Gets a value indicating whether the connection process to the scale has successfully finished.
        /// Returns false if the task is null or not yet completed.
        /// </summary>
        public bool IsConnectedToScale
        {
            get => (_isConnectToScaleValidationCompleted?.Task.IsCompletedSuccessfully == true) ? _isConnectToScaleValidationCompleted.Task.Result : false;
        }

        /// <summary>
        /// Manages the asynchronous validation state for standard body weight measurement data.
        /// </summary>
        private TaskCompletionSource<bool>? _bodyMeasurementValidationCompleted;

        /// <summary>
        /// Gets a value indicating whether the standard body weight measurement data has been successfully validated and completed.
        /// Returns false if the underlying task is null or has not completed successfully.
        /// </summary>
        public bool IsBodyMeasurementValid
        {
            get => (_bodyMeasurementValidationCompleted?.Task.IsCompletedSuccessfully == true) ?
                _bodyMeasurementValidationCompleted.Task.Result : false;
        }

        /// <summary>
        /// Manages the asynchronous validation state for advanced body composition measurement data.
        /// </summary>
        private TaskCompletionSource<bool>? _bodyCompositionMeasurementValidationCompleted;

        /// <summary>
        /// Gets a value indicating whether the advanced body composition measurement data has been successfully validated and completed.
        /// Returns false if the underlying task is null or has not completed successfully.
        /// </summary>
        public bool IsBodyCompositionMeasurementValid
        {
            get => (_bodyCompositionMeasurementValidationCompleted?.Task.IsCompletedSuccessfully == true) ?
                _bodyCompositionMeasurementValidationCompleted.Task.Result : false;
        }

        /// <summary>
        /// Manages the asynchronous validation state for advanced segmental body composition measurement data.
        /// </summary>
        private TaskCompletionSource<bool>? _segmentalBodyCompositionMeasurementValidationCompleted;

        /// <summary>
        /// Gets a value indicating whether the segmental body composition measurement data has been successfully validated and completed.
        /// Returns false if the underlying task is null or has not finished execution.
        /// </summary>
        public bool IsSegmentalBodyCompisitionMeasurementValid
        {
            get => (_segmentalBodyCompositionMeasurementValidationCompleted?.Task.IsCompleted == true) ?
                _segmentalBodyCompositionMeasurementValidationCompleted.Task.Result : false;
        }

        /// <summary>
        /// Manages the asynchronous completion and validation state for the user list retrieval process.
        /// </summary>
        private TaskCompletionSource<bool>? _userListValidationCompleted;

        /// <summary>
        /// Gets a value indicating whether the user list synchronization has completed successfully and is valid.
        /// </summary>
        public bool IsUserListValid
        {
            get => (_userListValidationCompleted?.Task.IsCompletedSuccessfully == true) ?
                _userListValidationCompleted.Task.Result : false;
        }

        /// <summary>
        /// Indicates whether the background advertisement watcher is currently active 
        /// and scanning for target devices.
        /// </summary>
        private bool _isScanning = false;

        /// <summary>
        /// Tracks whether the required user data access consent or pairing authorization 
        /// has been successfully transmitted to the peripheral device.
        /// </summary>
        private bool _authorizationSent = false;

        /// <summary>
        /// Indicates whether a device measurement. sequence has been explicitly 
        /// initiated or requested by the application workflow.
        /// </summary>
        private bool _measurementRequested = false;

        /// <summary>
        /// Indicates whether a request to fetch the user list from the scale has been initiated.
        /// </summary>
        private bool _userListRequested = false;

        /// <summary>
        /// Stores the internal physical connection state of the Bluetooth peripheral.
        /// </summary>
        private bool _connectionStatus = false;

        /// <summary>
        /// Gets or sets the current connection status of the scale device within the application context.
        /// </summary>
        public bool IsConnected
        {
            get { return _connectionStatus; }
            set { _connectionStatus = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BF105BeurerWaage"/> class with the specified user profile parameters.
        /// </summary>
        /// <param name="userIndex">The zero-based index identifying the user profile on the scale.</param>
        /// <param name="userPin">The unique numerical authorization code associated with the user.</param>
        public BF105BeurerWaage(int userIndex, int userPin, string loggingFileName = "BLE.log")
        {
            _userIndex = userIndex;
            _userPin = userPin;
            _loggingFileName = loggingFileName;

        }

        /// <summary>
        /// Asynchronously establishes a connection to the BF105 scale device. 
        /// Verifies if an active, authorized connection already exists to bypass redundant connection attempts. 
        /// Otherwise, initializes a task completion source, Starts a Bluetooth discovery scan, and awaits 
        /// the connection outcome while logging success or failure states.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing a boolean value indicating whether the connection was successfully established.</returns>
        /// <exception cref="Exception">Thrown if an unexpected error occurs while attempting to establish the connection.</exception>
        public async Task<bool> ConnectToScaleAsync()
        {
            if (_bluetoothLeDevice != null &&
                _bluetoothLeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected &&
                _authorizationSent)
            {
                return true;
            }

            _isConnectToScaleValidationCompleted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            try
            {
                await StartBluetoothScan();

                if (await _isConnectToScaleValidationCompleted.Task) Logger.Log("[INFO] connection to scale etablished", _loggingFileName);
                else Logger.Log("[FAILURE] connection to scale couldnt etablished", _loggingFileName);

                return _isConnectToScaleValidationCompleted.Task.Result;
            }
            catch (Exception ex)
            {
                Logger.Log($"[EXCEPTION] During etablishing conneciton to scale: {ex.Message}", _loggingFileName);
                throw new Exception($"[EXCEPTION] During etablishing conneciton to scale: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Asynchronously performs a standard body composition and weight measurement sequence. 
        /// Initializes tracking task completion sources, subscribes to standard composition metrics, 
        /// conditionally triggers a hardware measurement request based on the provided parameter, 
        /// and awaits validation tasks for both weight and body composition data.
        /// </summary>
        /// <param name="autoRequest">A boolean value indicating whether to automatically trigger the measurement request command after subscription.</param>
        /// <returns>A task representing the asynchronous operation, containing a boolean value indicating success status for both measurements.</returns>
        /// <exception cref="Exception">Thrown if an unexpected error occurs during the measurement process.</exception>
        public async Task<bool> GetBodyCompositionAndWeightAsync(bool autoRequest)
        {
            try
            {
                _bodyMeasurementValidationCompleted =
                       new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                _bodyCompositionMeasurementValidationCompleted =
                    new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                _measurementRequested = false;

                Logger.Log($"[INFO] Start body compostion and body weight measurement.", _loggingFileName);

                if (!await SubscribeBodyCompositionAsync(false))
                {
                    Logger.Log("[FAILURE] Subscribe Body Composition", _loggingFileName);
                    return false;
                }

                _measurementRequested = false;

                if (autoRequest) await RequestMeasurementAsync();

                bool measurementOK = await _bodyMeasurementValidationCompleted.Task;
                bool compositionOK = await _bodyCompositionMeasurementValidationCompleted.Task;

                Logger.Log("[INFO] Finished body cposition and body weight measurement.", _loggingFileName);

                return measurementOK && compositionOK;
            }
            catch (Exception ex)
            {
                Logger.Log("[EXCEPTION] During body cposition and body weight measurement.", _loggingFileName);
                throw new Exception("[EXCEPTION] During body cposition and body weight measurement.", ex);
            }
        }

        /// <summary>
        /// Asynchronously executes a segmental body composition and weight measurement sequence. 
        /// Configures the BLE characteristics for segmental mode, initializes task completion sources, 
        /// conditionally triggers the hardware measurement request, and awaits validation tasks for 
        /// both standard weight and segmental body composition data.
        /// </summary>
        /// <param name="autoRequest">A boolean value determining whether to automatically trigger the measurement request command.</param>
        /// <returns>A task containing a boolean value representing the collective success of the weight and segmental composition measurements.</returns>
        /// <exception cref="Exception">Thrown if an unhandled error occurs during the segmental measurement routine.</exception>
        public async Task<bool> GetSegmentalBodyCompostionAndWeightAsync(bool autoRequest)
        {
            try
            {
                _segmentalBodyCompositionMeasurementValidationCompleted =
                   new TaskCompletionSource<bool>(
                       TaskCreationOptions.RunContinuationsAsynchronously);

                _bodyMeasurementValidationCompleted =
                    new TaskCompletionSource<bool>(
                        TaskCreationOptions.RunContinuationsAsynchronously);

                _measurementRequested = false;

                Logger.Log("[INFO] Start segmental body composition and body weight measurement.", _loggingFileName);

                await SubscribeBodyCompositionAsync(true);

                _measurementRequested = false;

                if (autoRequest) await RequestMeasurementAsync();

                await _bodyMeasurementValidationCompleted.Task;
                await _segmentalBodyCompositionMeasurementValidationCompleted.Task;

                bool measuremntOK = await _bodyMeasurementValidationCompleted.Task;

                bool segmetnalBodyCompositionOK = await _segmentalBodyCompositionMeasurementValidationCompleted.Task;

                Logger.Log("[INFO] Finished segmental body composition and body weight measurement.", _loggingFileName);

                return measuremntOK && segmetnalBodyCompositionOK;
            }
            catch (Exception ex)
            {
                Logger.Log("[EXCEPTION] During segmental body composition and body weight measurement.", _loggingFileName);
                throw new Exception("[EXCEPTION] segmental body composition and body weight measurement.", ex);
            }
        }

        /// <summary>
        /// Asynchronously fetches and validates the list of registered user profiles synchronized from the scale.
        /// </summary>
        /// <returns>A task that represents the asynchronous user list retrieval operation.</returns>
        /// <exception cref="Exception">Thrown if the device is disconnected, the user lacks authorization, or an error occurs During retrieval.</exception>
        public async Task GetUsertListAsync()
        {
            if (_userListRequested) return;

            if (_bluetoothLeDevice == null ||
                _bluetoothLeDevice.ConnectionStatus != BluetoothConnectionStatus.Connected)
            {
                Logger.Log("[EXCEPTION] The BF105 is not connected.", _loggingFileName);
                throw new Exception("[EXCEPTION] The BF105 is not connected.");
            }

            if (!_authorizationSent)
            {
                Logger.Log("[EXCEPTION] The user has not yet been authorized.", _loggingFileName);
                throw new Exception("[EXCEPTION]  user has not yet been authorized.");
            }

            _userListValidationCompleted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);


            try
            {
                await RequestUserListAsync();

                bool usersValid = await _userListValidationCompleted.Task;
                if (usersValid)
                {
                    Logger.Log("[INFO] user list is valid", _loggingFileName);
                }
                else
                {
                    Logger.Log("[FAILURE] user list is not valid", _loggingFileName);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[EXCEPTION] getting user list: {ex.Message}", _loggingFileName);
                throw new Exception($"[EXCEPTION] getting user list: {ex.Message}", ex);
            }

        }

        /// <summary>
        /// Asynchronously reestablishes the connection to the scale by utilizing a cached Bluetooth address if available, 
        /// or falling back to a general device scan to discover and connect to the hardware.
        /// </summary>
        /// <returns>A task that represents the asynchronous reconnection operation.</returns>
        /// <exception cref="Exception">Thrown if an error occurs During the direct connection attempt or the Bluetooth scan initialization.</exception>
        public async Task ReconnectToScaleAsync()
        {
            try
            {
                if (_bf105BluetoothAddress.HasValue)
                {
                    _isConnectToScaleValidationCompleted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    Logger.Log("[RECONNECT] Connecting directly via known Bluetooth address..", _loggingFileName);
                    await ConnectToDeviceAsync(_bf105BluetoothAddress.Value);
                }
                else
                {
                    await StartBluetoothScan();
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[EXCEPTION] During etablishing conneciton to scale: {ex.Message}", _loggingFileName);
                throw new Exception($"[EXCEPTION] During etablishing conneciton to scale: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Initializes and Starts an active Bluetooth Low Energy advertisement watcher to discover the target peripheral device.
        /// </summary>
        /// <exception cref="Exception">Thrown if the advertisement watcher fails to initialize or Start correctly.</exception>
        private async Task StartBluetoothScan()
        {
            try
            {

                if (_isScanning) return;

                // Close existing connecitons
                await DisconnectExistingDeviceAsync();

                _isScanning = true;

                _bleWatcher = new BluetoothLEAdvertisementWatcher();

                // actice scan
                _bleWatcher.ScanningMode = BluetoothLEScanningMode.Active;

                _bleWatcher.Received += Watcher_Received;

                _bleWatcher.Start();
                Logger.Log("[BLE] Scan Started.", _loggingFileName);
            }
            catch (Exception ex)
            {
                _isScanning = false;
                Logger.Log($"[EXCEPTION] Starting the BLE scan: {ex.Message} | HResult={ex.HResult:X}", _loggingFileName);
                throw new Exception($"[EXCEPTION] Starting the BLE scan: {ex.Message} | HResult={ex.HResult:X}", ex);
            }
        }

        /// <summary>
        /// Stops the active Bluetooth Low Energy advertisement watcher and unregisters event handlers.
        /// </summary>
        /// <exception cref="Exception">Thrown if an error occurs while stopping the scan watcher.</exception>
        private void StopBluetoothScan()
        {
            try
            {
                _isScanning = false;

                if (_bleWatcher != null)
                {
                    _bleWatcher.Received -= Watcher_Received;

                    if (_bleWatcher.Status == BluetoothLEAdvertisementWatcherStatus.Started)
                    {
                        _bleWatcher.Stop();
                    }

                    Logger.Log("[BLE] can stopped.", _loggingFileName);
                    _bleWatcher = null;
                }

            }
            catch (Exception ex)
            {
                Logger.Log($"[EXCEPTION] stopping the scan: {ex.Message}", _loggingFileName);
                throw new Exception($"[EXCEPTION] stopping the scan: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Asynchronously establishes a connection to a remote Bluetooth Low Energy device using its unique hardware address, 
        /// registers connection state change handlers, and triggers GATT service initialization.
        /// </summary>
        /// <param name="bluetoothAddress">The unique 64-bit integer address of the target Bluetooth LE device.</param>
        /// <returns>A task representing the asynchronous device connection operation.</returns>
        /// <exception cref="Exception">Thrown if unauthorized access occurs or if connection setup fails.</exception>
        private async Task ConnectToDeviceAsync(ulong bluetoothAddress)
        {
            try
            {
                if (_bluetoothLeDevice != null &&
                    _bluetoothLeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected &&
                    _bluetoothLeDevice.BluetoothAddress == bluetoothAddress)
                {
                    Logger.Log("[INFO] connection to scale already etablished", _loggingFileName);

                    await SendAuthorizeUserAsync(_userIndex, _userPin);

                    return;
                }

                _bluetoothLeDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(bluetoothAddress);

                _bluetoothLeDevice.ConnectionStatusChanged += BluetoothLeDevice_ConnectionStatusChanged;

                await InitializeGattAsync(_bluetoothLeDevice);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Log($"[UnauthorizedAccessException]: {ex.Message} | HResult: {ex.HResult:X}", _loggingFileName);
                throw new Exception($"[UnauthorizedAccessException]: {ex.Message} | HResult: {ex.HResult:X}", ex);
            }
            catch (Exception ex)
            {
                Logger.Log($"[EXCEPTION] connection: {ex.Message} | HResult: {ex.HResult:X}", _loggingFileName);
                throw new Exception($"[EXCEPTION] connection: {ex.Message} | HResult: {ex.HResult:X}", ex);
            }
        }

        /// <summary>
        /// Asynchronously initializes the GATT profile for the connected scale, requests device access permissions, 
        /// discovers required services and characteristics, sets up subscriptions, and triggers user authorization.
        /// </summary>
        /// <param name="device">The connected remote Bluetooth Low Energy device instance.</param>
        /// <returns>A task representing the asynchronous GATT initialization process.</returns>
        /// <exception cref="Exception">Thrown if a critical error occurs During GATT discovery or characteristic binding.</exception>
        private async Task InitializeGattAsync(BluetoothLEDevice device)
        {
            try
            {
                var accessStatus = await device.RequestAccessAsync();

                if (accessStatus != Windows.Devices.Enumeration.DeviceAccessStatus.Allowed)
                {
                    Logger.Log($"Bluetooth access not allowed: {accessStatus}", _loggingFileName);
                    return;
                }

                var serviceResult = await device.GetGattServicesAsync(BluetoothCacheMode.Uncached);

                if (serviceResult.Status != GattCommunicationStatus.Success)
                {
                    Logger.Log($"[GATT] services could not be read: {serviceResult.Status}", _loggingFileName);
                    return;
                }

                foreach (var service in serviceResult.Services)
                {
                    var characteristicResult = await service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);

                    if (characteristicResult.Status != GattCommunicationStatus.Success)
                    {
                        continue;
                    }

                    _bF105Characteristics.IdentifyCharacteristic(characteristicResult.Characteristics);

                }

                if (!await SubscribeDefaultCharacteristics())
                {
                    throw new Exception("It was not possible to subscribe to the standard feature.");
                }

                if (_userPin >= 0) await SendAuthorizeUserAsync(_userIndex, _userPin);


            }
            catch (Exception ex)
            {
                Logger.Log($"[EXCEPTION] During GATT initialization: {ex.Message} | HResult: {ex.HResult}:X", _loggingFileName);
                throw new Exception($"[EXCEPTION] During GATT initialization: {ex.Message} | HResult: {ex.HResult}:X", ex);
            }
        }

        /// <summary>
        /// Subscribes to notifications or indications for the specified GATT characteristic asynchronously.
        /// </summary>
        /// <param name="characteristic">The GATT characteristic to subscribe to.</param>
        /// <returns>true if the subscription is successful; otherwise, false.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when access to the characteristic is denied.</exception>
        /// <exception cref="Exception">Thrown when an error occurs During the subscription process.</exception>
        private async Task<bool> SubscribeCharacteristicAsync(GattCharacteristic characteristic)
        {
            if (characteristic == null)
            {
                return false;
            }

            var properties = characteristic.CharacteristicProperties;

            GattClientCharacteristicConfigurationDescriptorValue descriptorValue;

            if (properties.HasFlag(GattCharacteristicProperties.Indicate))
            {
                descriptorValue = GattClientCharacteristicConfigurationDescriptorValue.Indicate;
            }
            else if (properties.HasFlag(GattCharacteristicProperties.Notify))
            {
                descriptorValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;
            }
            else
            {
                Logger.Log($"No Notify/Indicate function: {characteristic.Uuid}", _loggingFileName);
                return false;
            }

            try
            {
                characteristic.ValueChanged -= UniversalCharacteristic_ValueChanged;


                characteristic.ValueChanged += UniversalCharacteristic_ValueChanged;

                var status = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(descriptorValue);

                Logger.Log($"Subscribe Status: {status} | {characteristic.Uuid}", _loggingFileName);

                return status == GattCommunicationStatus.Success ? true : false;

            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Log($"[UnauthorizedAccessException] Subscribe at {characteristic.Uuid}: {ex.Message} | HResult={ex.HResult:X}", _loggingFileName);
                throw new UnauthorizedAccessException($"[UnauthorizedAccessException] Subscribe at {characteristic.Uuid}: {ex.Message} | HResult={ex.HResult:X}", ex);
            }
            catch (Exception ex)
            {
                Logger.Log($"Subscribe [EXCEPTION] at {characteristic.Uuid}: {ex.Message} | HResult={ex.HResult:X}", _loggingFileName);
                throw new Exception($"Subscribe [EXCEPTION] at {characteristic.Uuid}: {ex.Message} | HResult={ex.HResult:X}", ex);
            }
        }

        /// <summary>
        /// Unsubscribes from notifications for the specified GATT characteristic asynchronously.
        /// </summary>
        /// <param name="characteristic">The GATT characteristic to unsubscribe from.</param>
        /// <returns>True if unsubscription succeeds; otherwise, false.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when access to the characteristic is denied.</exception>
        /// <exception cref="Exception">Thrown when an error occurs During the unsubscription process.</exception>
        private async Task<bool> UnsubscribeCharacteristicAsync(GattCharacteristic characteristic)
        {
            if (characteristic == null) return false;

            try
            {
                var status = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                GattClientCharacteristicConfigurationDescriptorValue.None);

                Logger.Log($"Unsubscribe Status: {status} | {characteristic.Uuid}", _loggingFileName);

                characteristic.ValueChanged -= UniversalCharacteristic_ValueChanged;

                return status == GattCommunicationStatus.Success ? true : false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Log($"[UnauthorizedAccessException] Unsubscribe at {characteristic.Uuid}: {ex.Message} | HResult={ex.HResult:X}", _loggingFileName);
                throw new UnauthorizedAccessException($"[UnauthorizedAccessException] Unsubscribe at {characteristic.Uuid}: {ex.Message} | HResult={ex.HResult:X}", ex);
            }
            catch (Exception ex)
            {
                Logger.Log($"Unsubscribe [EXCEPTION] at {characteristic.Uuid}: {ex.Message} | HResult={ex.HResult:X}", _loggingFileName);
                throw new Exception($"Unsubscribe [EXCEPTION] at {characteristic.Uuid}: {ex.Message} | HResult={ex.HResult:X}", ex);
            }
        }

        /// <summary>
        /// Subscribes to the default Bluetooth Low Energy characteristics required for the BF105 device.
        /// </summary>
        /// <remarks>Logs the availability of relevant characteristics before attempting to
        /// subscribe.</remarks>
        /// <returns>true if all characteristic subscriptions succeed; otherwise, false.</returns>
        private async Task<bool> SubscribeDefaultCharacteristics()
        {
            // Log if necessary characters are available
            Logger.Log($"BF105 relevant characteristics:\n" +
                $"[2A9D] Weight: {(_bF105Characteristics.WeightCharacteristic != null ? "YES" : "NO")}\n" +
                $"[2A9F] User Control Point: {(_bF105Characteristics.UserControlPointCharacteristic != null ? "YES" : "NO")}\n" +
                $"[0001] User List: {(_bF105Characteristics.UserListCharacteristic != null ? "YES" : "NO")}\n" +
                $"[0006] Take measurement.: {(_bF105Characteristics.TakeMeasurementCharacteristic != null ? "YES" : "NO")}\n", _loggingFileName);

            if (_bF105Characteristics.UserControlPointCharacteristic == null ||
                _bF105Characteristics.TakeMeasurementCharacteristic == null ||
                _bF105Characteristics.WeightCharacteristic == null ||
                _bF105Characteristics.UserListCharacteristic == null ||
                _bF105Characteristics.BodyCompositionCharacteristic == null ||
                _bF105Characteristics.SegmentalBodyCompositionCharacteristic == null)

            {
                Logger.Log("One or some of the necessary characteristics are empty.", _loggingFileName);
                return false;
            }

            var weightSubOK = await SubscribeCharacteristicAsync(_bF105Characteristics.WeightCharacteristic);

            var userControlSubOK = await SubscribeCharacteristicAsync(_bF105Characteristics.UserControlPointCharacteristic);

            var userListSubOK = await SubscribeCharacteristicAsync(_bF105Characteristics.UserListCharacteristic);

            var takeMeasSubOK = await SubscribeCharacteristicAsync(_bF105Characteristics.TakeMeasurementCharacteristic);

            return weightSubOK && userControlSubOK && userListSubOK && takeMeasSubOK;
        }

        /// <summary>
        /// Unsubscribes from the default BLE characteristics for weight, user control point, user list, and take
        /// measurement.
        /// </summary>
        /// <remarks>Logs the status of the relevant characteristics before attempting to
        /// unsubscribe.</remarks>
        /// <returns>true if all unsubscribe operations succeed; otherwise, false.</returns>
        private async Task<bool> UnsubscribeDefaultCharacteristics()
        {
            var weightSubOK = await UnsubscribeCharacteristicAsync(_bF105Characteristics.WeightCharacteristic);

            var userControlSubOK = await UnsubscribeCharacteristicAsync(_bF105Characteristics.UserControlPointCharacteristic);

            var userListSubOK = await UnsubscribeCharacteristicAsync(_bF105Characteristics.UserListCharacteristic);

            var takeMeasSubOK = await UnsubscribeCharacteristicAsync(_bF105Characteristics.TakeMeasurementCharacteristic);

            return weightSubOK && userControlSubOK && userListSubOK && takeMeasSubOK;
        }

        /// <summary>
        /// Subscribes to body composition or segmental body composition notifications based on the specified mode.
        /// </summary>
        /// <param name="segmental">True to subscribe to segmental body composition notifications; false to subscribe to standard body composition
        /// notifications.</param>
        /// <returns>True if the subscription was successful; otherwise, false.</returns>
        private async Task<bool> SubscribeBodyCompositionAsync(bool segmental)
        {
            // Log if necessary characters are available
            Logger.Log($"BF105 relevant characteristics:\n" +
                $"[2A9C] Body Composition: {(_bF105Characteristics.BodyCompositionCharacteristic != null ? "YES" : "NO")}\n" +
                $"[0008] Segmental Body Composition: {(_bF105Characteristics.SegmentalBodyCompositionCharacteristic != null ? "YES" : "NO")}\n", _loggingFileName);

            if (_bF105Characteristics.BodyCompositionCharacteristic == null ||
                _bF105Characteristics.SegmentalBodyCompositionCharacteristic == null)
            {
                Logger.Log("One or some of the necessary Body Compistion or Segmental Body Composition are empty.", _loggingFileName);
                return false;
            }

            if (segmental)
            {
                var bodyCompoUnsubOK = await UnsubscribeCharacteristicAsync(_bF105Characteristics.BodyCompositionCharacteristic);

                var bodySegmentCompoSubOK = await SubscribeCharacteristicAsync(_bF105Characteristics.SegmentalBodyCompositionCharacteristic);

                Logger.Log("[MODE] Body Composition [2A9C] OFF, Body Segmental Composition [0008] ON", _loggingFileName);

                return bodyCompoUnsubOK && bodySegmentCompoSubOK;
            }
            else
            {
                var bodySegmentCompoUnsubOK = await UnsubscribeCharacteristicAsync(_bF105Characteristics.SegmentalBodyCompositionCharacteristic);

                var bodyCompoSubOK = await SubscribeCharacteristicAsync(_bF105Characteristics.BodyCompositionCharacteristic);

                Logger.Log("[MODE] Body Composition [2A9C] ON, Body Segmental Composition [0008] OFF", _loggingFileName);

                return bodyCompoSubOK && bodySegmentCompoUnsubOK;
            }
        }

        /// <summary>
        /// Asynchronously unsubscribes from both the standard body composition Bluetooth characteristic 
        /// and the segmental body composition characteristic to halt incoming measurement notifications.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing a boolean value indicating whether both unsubscription requests succeeded.</returns>
        private async Task<bool> UnsubscribeBodyCompostion()
        {
            return await UnsubscribeCharacteristicAsync(_bF105Characteristics.BodyCompositionCharacteristic) &&
                   await UnsubscribeCharacteristicAsync(_bF105Characteristics.SegmentalBodyCompositionCharacteristic);
        }

        /// <summary>
        /// Asynchronously sends a command payload to request and retrieve the registered user list from the scale device.
        /// </summary>
        /// <returns>A task representing the asynchronous user list request operation.</returns>
        private async Task RequestUserListAsync()
        {
            if (_bF105Characteristics.UserListCharacteristic == null)
            {
                Logger.Log("[0001] User list does not exist.", _loggingFileName);
                return;
            }
            await WriteCharacteristicAsync(_bF105Characteristics.UserListCharacteristic, new byte[] { 0x00 });
        }

        /// <summary>
        /// Validates user profile parameters and asynchronously sends an authorization command packet 
        /// to the scale's User Control Point characteristic to authenticate the active user.
        /// </summary>
        /// <param name="userIndex">The zero-based index of the user profile.</param>
        /// <param name="authorizationCode">The numerical consent or PIN code associated with the user.</param>
        /// <returns>A task representing the asynchronous authorization request operation.</returns>
        private async Task SendAuthorizeUserAsync(int userIndex, int authorizationCode)
        {
            if (_bF105Characteristics.UserControlPointCharacteristic == null)
            {
                Logger.Log("[2A9F] User Control Point does not exist.", _loggingFileName);
                return;
            }

            if (userIndex < 0 || userIndex > 255)
            {
                Logger.Log($"[2A9F] Invalid user index: {userIndex}", _loggingFileName);
                return;
            }

            if (authorizationCode < 0 || authorizationCode > 9999)
            {
                Logger.Log($"[2A9F] Invalid consent code: {authorizationCode}", _loggingFileName);
                return;
            }


            byte[] payload =
            {
            Constants.AuthorizeUser,

            (byte)userIndex,

            (byte)(authorizationCode & 0xFF),

            (byte)(authorizationCode >> 8 & 0xFF)
        };

            string hex = BitConverter.ToString(payload);

            _authorizationSent = true;

            var status = await WriteCharacteristicAsync(_bF105Characteristics.UserControlPointCharacteristic, payload);
        }

        /// <summary>
        /// Verifies characteristic availability and asynchronously triggers a new body measurement. sequence on the scale device.
        /// </summary>
        /// <returns>A task representing the asynchronous measurement. request operation.</returns>
        private async Task RequestMeasurementAsync()
        {
            if (_bF105Characteristics.TakeMeasurementCharacteristic == null)
            {
                Logger.Log("[0006] Take measurement. is not available.", _loggingFileName);
                return;
            }

            if (_measurementRequested)
            {
                Logger.Log("[0006] The measurement. has already been requested.", _loggingFileName);
                return;
            }

            byte[] command = { 0x00 };



            await WriteCharacteristicAsync(_bF105Characteristics.TakeMeasurementCharacteristic, command);
            _measurementRequested = true;

        }

        /// <summary>
        /// Writes the specified data to a GattCharacteristic asynchronously.
        /// </summary>
        /// <param name="characteristic">The GattCharacteristic to write to.</param>
        /// <param name="data">The data to write as a byte array.</param>
        /// <returns>A task that represents the asynchronous write operation. The task result contains the status of the write
        /// operation.</returns>
        /// <exception cref="Exception">Thrown when an error occurs During the write operation.</exception>
        private async Task<GattCommunicationStatus> WriteCharacteristicAsync(GattCharacteristic characteristic, byte[] data)
        {
            if (characteristic == null) return GattCommunicationStatus.Unreachable;

            try
            {
                string hex = BitConverter.ToString(data);

                using (var writer = new DataWriter())
                {
                    writer.WriteBytes(data);

                    GattWriteOption writeOption;

                    if (characteristic.CharacteristicProperties
                        .HasFlag(GattCharacteristicProperties.Write))
                    {
                        writeOption = GattWriteOption.WriteWithResponse;
                    }
                    else if (characteristic.CharacteristicProperties
                             .HasFlag(GattCharacteristicProperties.WriteWithoutResponse))
                    {
                        writeOption = GattWriteOption.WriteWithoutResponse;
                    }
                    else
                    {
                        Logger.Log($"Characteristic {characteristic.ToString()} is not writable.", _loggingFileName);
                        return GattCommunicationStatus.Unreachable;
                    }

                    return await characteristic.WriteValueAsync(writer.DetachBuffer(), writeOption); ;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[EXCEPTION] Writting: {ex.Message} / HResult={ex.HResult:X}", _loggingFileName);
                throw new Exception($"[EXCEPTION] Writting: {ex.Message} / HResult={ex.HResult:X}", ex);
            }
        }

        /// <summary>
        /// Handles the arrival of Bluetooth LE advertisement packets During the scan, filtering devices by name 
        /// and initiating a connection if the target scale is identified.
        /// </summary>
        /// <param name="sender">The advertisement watcher that triggered the event.</param>
        /// <param name="args">Event data containing the advertisement payload, local name, and Bluetooth address.</param>
        /// <exception cref="Exception">Thrown if an unexpected error occurs During advertisement processing or scanning.</exception>
        private async void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            try
            {
                string localName = args.Advertisement.LocalName;

                bool isKnownDevice = _bf105BluetoothAddress.HasValue &&
                                        args.BluetoothAddress == _bf105BluetoothAddress.Value;

                bool isNamedDevice = string.Equals(localName, _scaleModelName, StringComparison.OrdinalIgnoreCase);

                if (!isKnownDevice && !isNamedDevice)
                {
                    Logger.Log($"[INFO] Scale Model {_scaleModelName} not found.", _loggingFileName);
                    await Task.Delay(200);
                    return;
                }

                _bf105BluetoothAddress = args.BluetoothAddress;

                StopBluetoothScan();

                await ConnectToDeviceAsync(args.BluetoothAddress);
            }
            catch (Exception ex)
            {
                Logger.Log($"[EXCEPTION] During BLE scan: {ex.Message}", _loggingFileName);
                throw new Exception($"[EXCEPTION] During BLE scan: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Handles changes in the physical connection status of the Bluetooth LE peripheral device 
        /// and updates the internal connection state tracker.
        /// </summary>
        /// <param name="sender">The Bluetooth LE device whose connection status changed.</param>
        /// <param name="args">The event arguments associated with the status change.</param>
        private void BluetoothLeDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            if (sender.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
            {
                Logger.Log("[BLE] disconected", _loggingFileName);
                IsConnected = false;
            }
            else
            {
                Logger.Log("[BLE] connected", _loggingFileName);
                IsConnected = true;
            }
        }

        /// <summary>
        /// Universal event handler triggered when a subscribed GATT characteristic value changes, 
        /// reading the raw data buffer and routing it to the appropriate data parser based on the characteristic UUID.
        /// </summary>
        /// <param name="sender">The GATT characteristic whose value has changed.</param>
        /// <param name="args">Event data containing the updated characteristic value buffer.</param>
        /// <exception cref="Exception">Thrown if an error occurs while parsing or routing the received data payload.</exception>
        private void UniversalCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            try
            {
                using (DataReader reader = DataReader.FromBuffer(args.CharacteristicValue))
                {
                    uint length = reader.UnconsumedBufferLength;

                    if (length == 0)
                    {
                        Logger.Log("[BLE] Emtpy Package", _loggingFileName);
                        return;
                    }

                    byte[] data = new byte[length];

                    reader.ReadBytes(data);

                    if (sender.Uuid == Constants.Weight)
                    {
                        _bodyWeightMeasurementResult = ParseWeightMeasurement(data);
                    }

                    if (sender.Uuid == Constants.BodyComposition)
                    {
                        _bodyCompositionMeasurementResult = ParseBodyCompositionMeasurement(data);
                    }

                    if (sender.Uuid == Constants.UserControlPoint)
                    {
                        ParseUserControlPoint(data);
                    }

                    if (sender.Uuid == Constants.UserList)
                    {
                        var user = ParseUserList(data);
                        if (user != null && _userList != null) { _userList.Add(user); }
                        else if (user != null && _userList == null)
                        {
                            _userList = new List<UserListModel>();
                            _userList.Add(user);
                        }
                    }

                    if (sender.Uuid == Constants.TakeMeasurement)
                    {
                        ParseMeasurementStatus(data);
                    }

                    if (sender.Uuid == Constants.SegmentalBodyComposition)
                    {
                        _segmentalBodyCompositionMeasurementResult = ParseSegmentalBodyCompositionMeasurement(data);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[EXCEPTION] ValueChanged {ex.Message} | HResult={ex.HResult}", _loggingFileName);
                throw new Exception($"[EXCEPTION] ValueChanged {ex.Message} | HResult={ex.HResult}", ex);
            }
        }

        /// <summary>
        /// Processes the response result code received from the User Control Point characteristic after attempting user authorization, 
        /// updating connection/task states or logging appropriate error messages.
        /// </summary>
        /// <param name="data">The raw byte array payload of the response.</param>
        /// <param name="result">The result status byte returned by the scale.</param>
        private void HandleAuthorizeUserResponse(byte[] data, byte result)
        {
            switch (result)
            {
                case Constants.ResultSuccess:

                    Logger.Log("Wait for measurement. Start", _loggingFileName);
                    _isConnectToScaleValidationCompleted.SetResult(true);
                    break;


                case Constants.ResultUserNotAuthorized:

                    Logger.Log("[2A9F] CONSENT REJECTED! The user index or consent code is likely incorrect.", _loggingFileName);
                    break;

                case Constants.ResultOperationFailed:
                    Logger.Log("[2A9F] CONSENT operation failed! CONSENT was rejected by the BF105.", _loggingFileName);
                    break;

                default:
                    Logger.Log($"[2A9F] CONSENT unkown [EXCEPTION] code: {result:X2}", _loggingFileName);
                    break;
            }
        }

        /// <summary>
        /// Handles the response status of a new user registration command, extracting and logging the assigned user index on success.
        /// </summary>
        /// <param name="data">The raw byte array payload of the registration response.</param>
        /// <param name="result">The result status byte indicating success or failure of the registration.</param>
        private void HandleRegisterNewUserResponse(byte[] data, byte result)
        {
            if (result == Constants.ResultSuccess)
            {
                if (data.Length >= 4)
                {
                    int newUserIndex = data[3];
                    Logger.Log($"[2A9F] new user index: {newUserIndex}", _loggingFileName);
                }
            }
            else
            {
                Logger.Log($"[2A9F] REGISTER_NEW_USER failed: 0x{result:X2}", _loggingFileName);
            }
        }

        /// <summary>
        /// Parses incoming response data from the User Control Point (UCP) characteristic, 
        /// validating response opcodes and dispatching execution to specific handlers like user registration or authorization.
        /// </summary>
        /// <param name="data">The raw byte array response received from the scale.</param>
        private void ParseUserControlPoint(byte[] data)
        {
            if (data == null || data.Length == 0) return;

            string hex = BitConverter.ToString(data);

            if (data.Length < 3)
            {
                Logger.Log("[2A9F] Answer to short!", _loggingFileName);
                return;
            }


            byte opcode = data[0];

            if (opcode != Constants.Response)
            {
                Logger.Log($"[2A9F] No UCP Answer. Opcode: {opcode}", _loggingFileName);
                return;
            }

            byte requestedOperation = data[1];

            byte result = data[2];

            // REGISTER_NEW_USER
            if (requestedOperation == Constants.RegisterNewUser)
            {
                HandleRegisterNewUserResponse(data, result);
                return;
            }

            // Authorization
            if (requestedOperation == Constants.AuthorizeUser)
            {
                HandleAuthorizeUserResponse(data, result);
                return;
            }

            // LIST_ALL_USERS
            if (requestedOperation == Constants.ListAllUsers)
            {
                return;
            }
        }

        /// <summary>
        /// Parses incoming raw data packets from the User List characteristic to construct user profile models 
        /// or manage synchronization completion states.
        /// </summary>
        /// <param name="data">The raw byte array payload received from the user list characteristic.</param>
        /// <returns>A populated <see cref="UserListModel"/> instance if a valid record is read; otherwise, null.</returns>
        private UserListModel ParseUserList(byte[] data)
        {
            if (data == null || data.Length == 0) return null;

            byte status = data[0];

            // Status 0x00 = Benutzer-Datensatz
            var user = new UserListModel();

            if (status == 0x00)
            {
                if (data.Length < 12)
                {
                    Logger.Log($"[0001] User packet too short: {data.Length} bytes", _loggingFileName);
                    return new UserListModel();
                }

                int userIndex = data[1];

                byte[] initialsBytes =
                {
                data[2],
                data[3],
                data[4]
            };

                string initials = Encoding.ASCII.GetString(initialsBytes)
                    .Replace("\0", "")
                    .Trim();

                // 5 = Reserved
                // 6/7 = Year
                // 8   = Month
                // 9   = Day
                // 10  = Size
                // 11  = Gender
                // 12  = Activity
                // The complete packet may be larger
                // depending on the device.
                if (data.Length >= 13)
                {
                    int year = ReadUInt16LE(data, 5);

                    int month = data[7];

                    int day = data[8];

                    int height = data[9];

                    int gender = data[10];

                    int activity = data[11];

                    user.Index = userIndex;
                    user.Initials = initials;
                    user.Activity = activity;
                    user.BirthDate = new DateTime(year, month, day);
                    user.Heigth = height;
                    user.Gender = gender;
                }
                else
                {
                    user.Index = userIndex;
                    user.Initials = initials;
                }

                return user;
            }

            // Status 0x01 = Finished User List
            if (status == 0x01)
            {
                Logger.Log("[0001] User list fully received.", _loggingFileName);
                _userListValidationCompleted.SetResult(true);
                return null;
            }

            // Status 0x02 = no User
            else if (status == 0x02)
            {
                Logger.Log("[0001] No users stored on the BF105.", _loggingFileName);
                return null;
            }

            else
            {
                Logger.Log("Unknown Status 0x{status:X2}", _loggingFileName);
                return null;
            }
        }

        /// <summary>
        /// Logs the raw status data packet received from the measurement. initiation characteristic.
        /// </summary>
        /// <param name="data">The raw byte array status payload.</param>
        private void ParseMeasurementStatus(byte[] data)
        {
            if (data == null || data.Length == 0) return;
            string hex = BitConverter.ToString(data);
            Logger.Log($"Parse Measurement Status: {hex}", _loggingFileName);
        }

        /// <summary>
        /// Parses the raw byte array data from the Weight characteristic (UUID 0x2A9D), extracting flags, 
        /// weight values (converting units if imperial), timestamps, user association, BMI, and height.
        /// </summary>
        /// <param name="data">The raw byte array payload received from the weight characteristic.</param>
        /// <returns>A populated <see cref="BodyWeightMeasurementModel"/> instance, or null if the packet is too short.</returns>
        private BodyWeightMeasurementModel ParseWeightMeasurement(byte[] data)
        {
            if (data == null || data.Length < 3)
            {
                Logger.Log("[2A9D] package too short", _loggingFileName);
                return null;
            }

            if (_bodyMeasurementValidationCompleted == null) return null;

            var usereMeasurement = new BodyWeightMeasurementModel();

            int offset = 0;

            byte flags = data[offset++];

            bool isImperial = (flags & 0x01) != 0;

            bool timestampPresent = (flags & 0x02) != 0;

            bool userPresent = (flags & 0x04) != 0;

            bool bmiHeightPresent = (flags & 0x08) != 0;

            ushort rawWeight = ReadUInt16LE(data, offset);

            offset += 2;

            double weightKg;

            if (!isImperial)
            {
                // kg ==> Resolution = 0.005 kg
                weightKg = rawWeight * 0.005;
            }
            else
            {
                // lb ==> Resolution = 0.01 lb
                double weightLb = rawWeight * 0.01;

                weightKg = weightLb * 0.45359237;
            }

            usereMeasurement.Weight = weightKg;

            if (timestampPresent) usereMeasurement.TimeStamp = ParseTimeStamp(data, offset);

            int scaleUserIndex = -1;

            if (userPresent)
            {
                if (data.Length > offset)
                {
                    scaleUserIndex = data[offset++];

                    usereMeasurement.UserIndex = scaleUserIndex;
                }
            }

            if (bmiHeightPresent)
            {
                if (data.Length >= offset + 4)
                {
                    ushort rawBmi = ReadUInt16LE(data, offset);

                    offset += 2;


                    ushort rawHeight = ReadUInt16LE(data, offset);

                    offset += 2;
                    usereMeasurement.BMI = rawBmi * 0.1;

                    usereMeasurement.Height = rawHeight * 0.001;
                }
            }

            _bodyMeasurementValidationCompleted.TrySetResult(true);

            return usereMeasurement;
        }

        /// <summary>
        /// Parses the raw byte array payload from the Body Composition characteristic, evaluating bitwise flags 
        /// to dynamically extract metrics such as body fat percentage, BMR, muscle mass, water percentage, impedance, and timestamps.
        /// </summary>
        /// <param name="data">The raw byte array payload received from the body composition characteristic.</param>
        /// <returns>A populated <see cref="BodyCompositionMeasurementModel"/> instance, or an empty model if parsing fails or data is too short.</returns>
        private BodyCompositionMeasurementModel ParseBodyCompositionMeasurement(byte[] data)
        {
            if (data == null || data.Length < 4) return new BodyCompositionMeasurementModel();

            if (_bodyCompositionMeasurementValidationCompleted == null) return null;

            int offset = 0;

            var bodyMeasurements = new BodyCompositionMeasurementModel();

            ushort flags = ReadUInt16LE(data, offset);

            offset += 2;

            bool isImperial = (flags & 0x0001) != 0;

            bool timestampPresent = (flags & 0x0002) != 0;

            bool userPresent = (flags & 0x0004) != 0;

            bool bmrPresent = (flags & 0x0008) != 0;

            bool musclePctPresent = (flags & 0x0010) != 0;

            bool muscleMassPresent = (flags & 0x0020) != 0;

            bool fatFreeMassPresent = (flags & 0x0040) != 0;

            bool softLeanPresent = (flags & 0x0080) != 0;

            bool waterPresent = (flags & 0x0100) != 0;

            bool impedancePresent = (flags & 0x0200) != 0;

            bool weightPresent = (flags & 0x0400) != 0;

            bool heightPresent = (flags & 0x0800) != 0;

            bool multiPacket = (flags & 0x1000) != 0;

            if (data.Length < offset + 2) return new BodyCompositionMeasurementModel();

            ushort rawBodyFat = ReadUInt16LE(data, offset);

            offset += 2;

            bodyMeasurements.BodyFatPercentage = rawBodyFat * 0.1f;

            if (timestampPresent) bodyMeasurements.TimeStamp = ParseTimeStamp(data, offset);

            if (userPresent)
            {
                if (data.Length > offset)
                {
                    int userIndex = data[offset++];

                    bodyMeasurements.UserIndex = userIndex.ToString();
                }
            }

            if (bmrPresent)
            {
                if (!TryReadUInt16LE(
                        data,
                        ref offset,
                        out ushort rawBmr))
                {
                    return new BodyCompositionMeasurementModel();
                }

                // openScale treats this value
                // as a standardized raw BMR value. 
                //
                // For the diagnosis, we output both
                // the raw value and an approximation.
                float bmrKcal = rawBmr / 4.1868f;

                bodyMeasurements.BodyBMR = bmrKcal;
            }

            if (musclePctPresent)
            {
                if (!TryReadUInt16LE(
                        data,
                        ref offset,
                        out ushort rawMuscle))
                {
                    return new BodyCompositionMeasurementModel();
                }

                bodyMeasurements.BodyMusclePercentage = rawMuscle * 0.1f;
            }

            if (muscleMassPresent)
            {
                if (!TryReadUInt16LE(
                        data,
                        ref offset,
                        out ushort rawMuscleMass))
                {
                    return new BodyCompositionMeasurementModel();
                }

                bodyMeasurements.BodyMuscleMassKg = ConvertMassToKg(rawMuscleMass, isImperial);
            }

            if (fatFreeMassPresent)
            {
                if (!TryReadUInt16LE(
                        data,
                        ref offset,
                        out ushort rawFfm))
                {
                    return new BodyCompositionMeasurementModel();
                }

                bodyMeasurements.BodyFFM = ConvertMassToKg(rawFfm, isImperial);
            }

            if (softLeanPresent)
            {
                if (!TryReadUInt16LE(
                        data,
                        ref offset,
                        out ushort rawSoftLean))
                {
                    return new BodyCompositionMeasurementModel();
                }

                bodyMeasurements.BodySoftLeanMass = ConvertMassToKg(rawSoftLean, isImperial);
            }

            if (waterPresent)
            {
                if (!TryReadUInt16LE(
                        data,
                        ref offset,
                        out ushort rawWater))
                {
                    return new BodyCompositionMeasurementModel();
                }

                // For the BF105, this value
                // needs to be verified more precisely in practice. 
                //
                // We therefore log both
                // the raw value and the scaled representation.

                var bodyWaterInKg = rawWater * 0.005f;
                if (bodyWaterInKg > 0 &&
                    (_bodyWeightMeasurementResult?.Weight ?? 0.0) > 0)
                {
                    bodyMeasurements.BodyWaterPercentage = ((float)(bodyWaterInKg / _bodyWeightMeasurementResult.Weight)) * 100.0f;
                }
                else bodyMeasurements.BodyWaterPercentage = 0.0f;
            }

            if (impedancePresent)
            {
                if (!TryReadUInt16LE(
                        data,
                        ref offset,
                        out ushort rawImpedance))
                {
                    return new BodyCompositionMeasurementModel();
                }


                bodyMeasurements.BodyImpedanz = rawImpedance * 0.1f;

            }

            if (weightPresent)
            {
                if (!TryReadUInt16LE(
                        data,
                        ref offset,
                        out ushort rawWeight))
                {
                    return new BodyCompositionMeasurementModel();
                }

                bodyMeasurements.BodyWeight = ConvertMassToKg(rawWeight, isImperial);
            }

            if (heightPresent)
            {
                if (!TryReadUInt16LE(
                        data,
                        ref offset,
                        out ushort rawHeight))
                {
                    return new BodyCompositionMeasurementModel();
                }

                if (isImperial)
                {
                    // Diagnostic Value
                    bodyMeasurements.PersonHeight = rawHeight * 0.001f;
                }
                else
                {
                    bodyMeasurements.PersonHeight = rawHeight * 0.001f;
                }
            }

            if (multiPacket) bodyMeasurements.MoreMeasurementsAvailable = true;
            else bodyMeasurements.MoreMeasurementsAvailable = false;

            _bodyCompositionMeasurementValidationCompleted.TrySetResult(true);

            return bodyMeasurements;
        }

        /// <summary>
        /// Parses a raw byte array to extract segmental body composition measurement.s, 
        /// including upper and lower body fat and muscle percentages.
        /// </summary>
        /// <param name="data">The raw byte array containing the measurement. packet (minimum 9 bytes).</param>
        /// <returns>A populated SegmentalBodyCompositionMeasurementModel instance, or null if the input data is invalid.</returns>
        private SegmentalBodyCompositionMeasurementModel ParseSegmentalBodyCompositionMeasurement(byte[] data)
        {
            if (data == null || data.Length < 9)
            {
                return null;
            }

            if (_segmentalBodyCompositionMeasurementValidationCompleted == null) return null;

            var result = new SegmentalBodyCompositionMeasurementModel();

            // Byte 0:
            // Paketkennung / Status
            byte packetType = data[0];

            ushort upperFatRaw = ReadUInt16LE(data, 1);
            result.UpperBodyFatPercentage = upperFatRaw * 0.1f;

            ushort lowerFatRaw = ReadUInt16LE(data, 3);
            result.LowerBodyFatPercentage = lowerFatRaw * 0.1f;

            ushort upperMuscleRaw = ReadUInt16LE(data, 5);
            result.UpperBodyMusclePercentage = upperMuscleRaw * 0.1f;

            ushort lowerMuscleRaw = ReadUInt16LE(data, 7);
            result.LowerBodyMusclePercentage = lowerMuscleRaw * 0.1f;

            _segmentalBodyCompositionMeasurementValidationCompleted.TrySetResult(true);

            return result;
        }

        /// <summary>
        /// Parses a standard Bluetooth LE timestamp structure (7 bytes) Starting from the specified byte offset.
        /// </summary>
        /// <param name="data">The raw byte array containing the timestamp data.</param>
        /// <param name="offset">The Starting byte index for the timestamp structure.</param>
        /// <returns>A <see cref="DateTime"/> instance if parsing succeeds; otherwise, default (DateTime.MinValue).</returns>
        private DateTime ParseTimeStamp(byte[] data, int offset)
        {

            if (data.Length >= offset + 7)
            {
                int year = ReadUInt16LE(data, offset);

                offset += 2;

                int month = data[offset++];

                int day = data[offset++];

                int hour = data[offset++];

                int minute = data[offset++];

                int second = data[offset++];

                return new DateTime(year, month, day, hour, minute, second);
            }

            return default;

        }

        /// <summary>
        /// Converts a raw mass value (ushort) into kilograms based on the unit system flag (metric vs. imperial).
        /// </summary>
        /// <param name="rawValue">The raw integer mass value read from the characteristic buffer.</param>
        /// <param name="imperial">A flag indicating whether the value is in imperial units (pounds).</param>
        /// <returns>The calculated mass value in kilograms.</returns>
        private float ConvertMassToKg(ushort rawValue, bool imperial)
        {
            if (!imperial)
            {
                // kg, Auflösung 0.005 kg
                return rawValue * 0.005f;
            }


            // lb, Auflösung 0.01 lb
            float pounds = rawValue * 0.01f;

            return pounds * 0.45359237f;
        }

        /// <summary>
        /// Reads a 16-bit unsigned integer in Little-Endian format from the specified byte array at the given offset.
        /// </summary>
        /// <param name="data">The source byte array.</param>
        /// <param name="offset">The byte position to read from.</param>
        /// <returns>The 16-bit unsigned integer value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the provided data array is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the offset is out of bounds.</exception>
        private static ushort ReadUInt16LE(byte[] data, int offset)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }


            if (offset < 0 || offset + 1 >= data.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }


            return (ushort)(data[offset] | data[offset + 1] << 8);
        }

        /// <summary>
        /// Safely attempts to read a 16-bit unsigned integer in Little-Endian format, advancing the offset reference upon success.
        /// </summary>
        /// <param name="data">The source byte array.</param>
        /// <param name="offset">A reference to the current byte offset, which is incremented by 2 if successful.</param>
        /// <param name="value">The resulting 16-bit unsigned integer value if successful.</param>
        /// <returns>True if the read operation succeeded; otherwise, false.</returns>
        private static bool TryReadUInt16LE(byte[] data, ref int offset, out ushort value)
        {
            value = 0;

            if (data == null || offset < 0 || offset + 1 >= data.Length)
            {
                return false;
            }


            value = ReadUInt16LE(data, offset);

            offset += 2;

            return true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources, 
        /// specifically triggering device disconnection.
        /// </summary>
        public async Task Dispose()
        {
            await DisconnectExistingDeviceAsync();
        }

        /// <summary>
        /// Disconnects the current Bluetooth device, stops device discovery, resets related characteristics and state flags,
        /// and handles cleanup asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous disconnect operation.</returns>
        private async Task<bool> DisconnectExistingDeviceAsync()
        {
            try
            {
                await UnsubscribeBodyCompostion();
                await UnsubscribeDefaultCharacteristics();

                if (_bleWatcher != null)
                {
                    if (_bleWatcher.Status == BluetoothLEAdvertisementWatcherStatus.Started)
                    {
                        _bleWatcher.Stop();
                    }
                    _bleWatcher.Received -= Watcher_Received;
                    _bleWatcher = null;
                }

                if (_bluetoothLeDevice != null)
                {
                    _bluetoothLeDevice.ConnectionStatusChanged -= BluetoothLeDevice_ConnectionStatusChanged;

                    _bluetoothLeDevice?.Dispose();
                    _bluetoothLeDevice = null;
                }

                _bF105Characteristics = new BF105Characteristic();

                _authorizationSent = false;
                _measurementRequested = false;
                _userListRequested = false;
                _isScanning = false;

                GC.Collect();
                GC.WaitForPendingFinalizers();

                Logger.Log("[BLE] Device disconnection routine completed successfully.", _loggingFileName);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"[EXCEPTION] During disconnect: {ex.Message}", _loggingFileName);
                return false;
            }
        }
    }
}