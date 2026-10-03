using BF105.Bluetooth.Service;
using System;
using System.Collections.Generic;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BF105.Bluetooth.Service
{
    /// <summary>
    /// Manages and maps the standard and custom GATT characteristics for the Beurer BF105 device, 
    /// providing structured access points for weight, body composition, and device control.
    /// </summary>
    public class BF105Characteristic
    {
        /// <summary>
        /// Stores the mapping dictionary that associates device-specific GATT UUIDs with their respective assignment delegates.
        /// </summary>
        private readonly Dictionary<Guid, Action<GattCharacteristic>> _characteristicMappers;

        /// <summary>
        /// Exposes the standard GATT characteristic for reading or subscribing to 
        /// real-time weight measurement values.
        /// </summary>
        private GattCharacteristic? _weightCharacteristic;

        /// <summary>
        /// Gets the GATT characteristic instance responsible for handling real-time weight data streams.
        /// </summary>
        public GattCharacteristic? WeightCharacteristic
        {
            get => _weightCharacteristic;
            private set => _weightCharacteristic = value;
        }

        /// <summary>
        /// Exposes the GATT characteristic used to retrieve detailed body composition 
        /// metrics (e.g., body fat percentage, muscle mass, water weight).
        /// </summary>
        private GattCharacteristic? _bodyCompositionCharacteristic;

        /// <summary>
        /// Gets the GATT characteristic instance used for retrieving body composition metrics.
        /// </summary>
        public GattCharacteristic? BodyCompositionCharacteristic
        {
            get => _bodyCompositionCharacteristic;
            private set => _bodyCompositionCharacteristic = value;
        }

        /// <summary>
        /// Exposes the GATT characteristic utilized for user-specific control operations, 
        /// such as selecting or authenticating user profiles on the scale.
        /// </summary>
        private GattCharacteristic? _userControlPointCharacteristic;

        /// <summary>
        /// Gets the GATT characteristic instance for executing user-specific profile control operations.
        /// </summary>
        public GattCharacteristic? UserControlPointCharacteristic
        {
            get => _userControlPointCharacteristic;
            private set => _userControlPointCharacteristic = value;
        }

        /// <summary>
        /// Custom GATT characteristic providing access to the device's stored user list 
        /// for profile synchronization and management.
        /// </summary>
        private GattCharacteristic? _userListCharacteristic;

        /// <summary>
        /// Gets the custom GATT characteristic instance for synchronizing the device's user directory.
        /// </summary>
        public GattCharacteristic? UserListCharacteristic
        {
            get => _userListCharacteristic;
            private set => _userListCharacteristic = value;
        }

        /// <summary>
        /// Custom GATT characteristic used to trigger a remote measurement command 
        /// directly from the host application.
        /// </summary>
        private GattCharacteristic? _takeMeasurementCharacteristic;

        /// <summary>
        /// Gets the custom GATT characteristic instance used to remotely trigger hardware measurement cycles.
        /// </summary>
        public GattCharacteristic? TakeMeasurementCharacteristic
        {
            get => _takeMeasurementCharacteristic;
            private set => _takeMeasurementCharacteristic = value;
        }

        /// <summary>
        /// Custom GATT characteristic responsible for handling user activity tracking data 
        /// and lifestyle parameters.
        /// </summary>
        private GattCharacteristic? _activityCharacteristic;

        /// <summary>
        /// Gets the custom GATT characteristic instance managing lifestyle and activity level configurations.
        /// </summary>
        public GattCharacteristic? ActivityCharacteristic
        {
            get => _activityCharacteristic;
            private set => _activityCharacteristic = value;
        }

        /// <summary>
        /// Custom GATT characteristic used to read or write user initials (short identifiers) 
        /// associated with specific scale memory slots.
        /// </summary>
        private GattCharacteristic? _initialsCharacteristic;

        /// <summary>
        /// Gets the custom GATT characteristic instance for handling user initials and short identifiers.
        /// </summary>
        public GattCharacteristic? InitialsCharacteristic
        {
            get => _initialsCharacteristic;
            private set => _initialsCharacteristic = value;
        }

        /// <summary>
        /// Custom GATT characteristic enabling the configuration and retrieval of a 
        /// user's defined target weight goal.
        /// </summary>
        private GattCharacteristic? _targetWeightCharacteristic;

        /// <summary>
        /// Gets the custom GATT characteristic instance for setting and querying user weight goals.
        /// </summary>
        public GattCharacteristic? TargetWeightCharacteristic
        {
            get => _targetWeightCharacteristic;
            private set => _targetWeightCharacteristic = value;
        }

        /// <summary>
        /// Custom GATT characteristic enabling the configuration and retrieval of a 
        /// user's defined target weight goal.
        /// </summary>
        private GattCharacteristic? _segmentalBodyCompositionCharacteristic;

        /// <summary>
        /// Gets the custom GATT characteristic instance for setting and querying user weight goals.
        /// </summary>
        public GattCharacteristic? SegmentalBodyCompositionCharacteristic
        {
            get => _segmentalBodyCompositionCharacteristic;
            private set => _segmentalBodyCompositionCharacteristic = value;
        }

        /// <summary>
        /// Custom GATT characteristic enabling the configuration and retrieval of a 
        /// user's defined target weight goal.
        /// </summary>
        private GattCharacteristic? _segmentalBodyCompositionCharacteristicI;

        /// <summary>
        /// Gets the custom GATT characteristic instance for setting and querying user weight goals.
        /// </summary>
        public GattCharacteristic? SegmentalBodyCompositionCharacteristicI
        {
            get => _segmentalBodyCompositionCharacteristicI;
            private set => _segmentalBodyCompositionCharacteristicI = value;
        }

        /// <summary>
        /// Custom GATT characteristic enabling the configuration and retrieval of a 
        /// user's defined target weight goal.
        /// </summary>
        private GattCharacteristic? _segmentalBodyCompositionCharacteristicII;

        /// <summary>
        /// Gets the custom GATT characteristic instance for setting and querying user weight goals.
        /// </summary>
        public GattCharacteristic? SegmentalBodyCompositionCharacteristicII
        {
            get => _segmentalBodyCompositionCharacteristicII;
            private set => _segmentalBodyCompositionCharacteristicII = value;
        }

        /// <summary>
        /// Custom GATT characteristic enabling the configuration and retrieval of a 
        /// user's defined target weight goal.
        /// </summary>
        private GattCharacteristic? _segmentalBodyCompositionCharacteristicIII;

        /// <summary>
        /// Gets the custom GATT characteristic instance for setting and querying user weight goals.
        /// </summary>
        public GattCharacteristic? SegmentalBodyCompositionCharacteristicIII
        {
            get => _segmentalBodyCompositionCharacteristicIII;
            private set => _segmentalBodyCompositionCharacteristicIII = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BF105Characteristic"/> class by configuring 
        /// the internal UUID-to-property mapping dictionary.
        /// </summary>
        public BF105Characteristic()
        {
            // Initializes the lookup dictionary for O(1) characteristic resolution
            _characteristicMappers = new Dictionary<Guid, Action<GattCharacteristic>>
            {
                { Constants.Weight, c => WeightCharacteristic = c },
                { Constants.BodyComposition, c => BodyCompositionCharacteristic = c },
                { Constants.UserControlPoint, c => UserControlPointCharacteristic = c },
                { Constants.UserList, c => UserListCharacteristic = c },
                { Constants.TakeMeasurement, c => TakeMeasurementCharacteristic = c },
                { Constants.Activity, c => ActivityCharacteristic = c },
                { Constants.Initials, c => InitialsCharacteristic = c },
                { Constants.TargetWeight, c => TargetWeightCharacteristic = c },
                { Constants.SegmentalBodyComposition, c => SegmentalBodyCompositionCharacteristic = c },
                { Constants.SegmentalBodyCompositionI, c => SegmentalBodyCompositionCharacteristicI = c },
                { Constants.SegmentalBodyCompositionII, c => SegmentalBodyCompositionCharacteristicII = c },
                { Constants.SegmentalBodyCompositionIII, c => SegmentalBodyCompositionCharacteristicIII = c }
            };
        }

        /// <summary>
        /// Evaluates a collection of discovered GATT characteristics and maps them to their respective 
        /// properties using an optimized dictionary lookup.
        /// </summary>
        /// <param name="characteristics">A read-only list of discovered GATT characteristics from the BLE device.</param>
        public void IdentifyCharacteristic(IReadOnlyList<GattCharacteristic> characteristics)
        {
            foreach (var characteristic in characteristics)
            {
                // Check if the UUID is registered in the mapper dictionary and execute the assignment delegate directly
                if (_characteristicMappers.TryGetValue(characteristic.Uuid, out var assignAction))
                {
                    assignAction(characteristic);
                }
            }
        }
    }
}