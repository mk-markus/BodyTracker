using System;

namespace BF105.Bluetooth.Service
{
    /// <summary>
    /// Contains standard and proprietary BLE UUIDs and control point opcodes for smart scales and body composition devices.
    /// </summary>
    public static class Constants
    {
        #region BLE UUIDs

        /// <summary>
        /// The standard GATT Service defining the primary profile for weight scale devices (Weight Scale Service).
        /// </summary>
        public static readonly Guid WeightService =
            Guid.Parse("0000181d-0000-1000-8000-00805f9b34fb");

        /// <summary>
        /// The standard GATT Characteristic used to read individual weight measurement data packets from the scale.
        /// </summary>
        public static readonly Guid Weight =
            Guid.Parse("00002a9d-0000-1000-8000-00805f9b34fb");

        /// <summary>
        /// The standard GATT Service that exposes metrics related to body composition (e.g., body fat percentage, muscle mass).
        /// </summary>
        public static readonly Guid BodyCompositionService =
            Guid.Parse("0000181b-0000-1000-8000-00805f9b34fb");

        /// <summary>
        /// The specific GATT Characteristic used to transmit detailed body composition measurement values.
        /// </summary>
        public static readonly Guid BodyComposition =
            Guid.Parse("00002a9c-0000-1000-8000-00805f9b34fb");

        /// <summary>
        /// The specific GATT Characteristic used to transmit detailed segmental body composition measurement values.
        /// </summary>
        public static readonly Guid SegmentalBodyComposition =
            Guid.Parse("00000008-0000-1000-8000-00805f9b34fb");


        /// <summary>
        /// The specific GATT Characteristic used to transmit detailed segmental body composition measurement values.
        /// </summary>
        public static readonly Guid SegmentalBodyCompositionI =
            Guid.Parse("00000009-0000-1000-8000-00805f9b34fb");

        /// <summary>
        /// The specific GATT Characteristic used to transmit detailed segmental body composition measurement values.
        /// </summary>
        public static readonly Guid SegmentalBodyCompositionII =
            Guid.Parse("0000000A-0000-1000-8000-00805f9b34fb");


        /// <summary>
        /// The specific GATT Characteristic used to transmit detailed segmental body composition measurement values.
        /// </summary>
        public static readonly Guid SegmentalBodyCompositionIII =
            Guid.Parse("00000007-0000-1000-8000-00805f9b34fb");


        // 00000008-0000-1000-8000-00805f9b34fb
        // 00000009-0000-1000-8000-00805f9b34fb
        // 0000000A-0000-1000-8000-00805f9b34fb

        /// <summary>
        /// The standard User Data Service (UDS) used for managing user-specific profiles and configuration parameters on the device.
        /// </summary>
        public static readonly Guid UserDataService =
            Guid.Parse("0000181c-0000-1000-8000-00805f9b34fb");

        /// <summary>
        /// A write-enabled GATT Characteristic utilized to execute administrative user commands (e.g., registering new users, listing existing profiles).
        /// </summary>
        public static readonly Guid UserControlPoint =
            Guid.Parse("00002a9f-0000-1000-8000-00805f9b34fb");

        /// <summary>
        /// The standard _bF105Characteristics used to synchronize or read the real-time clock configuration of the peripheral device.
        /// </summary>
        public static readonly Guid CurrentTime =
            Guid.Parse("00002a2b-0000-1000-8000-00805f9b34fb");

        /// <summary>
        /// The standard _bF105Characteristics exposing the remaining battery charge percentage of the hardware device.
        /// </summary>
        public static readonly Guid Battery =
            Guid.Parse("00002a19-0000-1000-8000-00805f9b34fb");

        #endregion

        #region BEURER CUSTOM SERVICE

        /// <summary>
        /// Vendor-specific custom GATT Service root UUID implemented by Beurer devices for proprietary data exchange.
        /// </summary>
        public static readonly Guid BeurerService =
            Guid.Parse("0000ffff-0000-1000-8000-00805f9b34fb");

        /// <summary>
        /// Proprietary _bF105Characteristics to manage, query, or map stored user profiles on the scale.
        /// </summary>
        public static readonly Guid UserList =
            Guid.Parse("00000001-0000-1000-8000-00805f9b34fb");

        /// <summary>
        /// Characteristic used to transfer or retrieve user identification initials (typically a short string displayed on the hardware device).
        /// </summary>
        public static readonly Guid Initials =
            Guid.Parse("00000002-0000-1000-8000-00805f9b34fb");

        /// <summary>
        /// Characteristic dedicated to setting or reading the user's defined weight goal.
        /// </summary>
        public static readonly Guid TargetWeight =
            Guid.Parse("00000003-0000-1000-8000-00805f9b34fb");

        /// <summary>
        /// Characteristic representing the user's physical activity level configuration (often used for accurate calorie expenditure calculations).
        /// </summary>
        public static readonly Guid Activity =
            Guid.Parse("00000004-0000-1000-8000-00805f9b34fb");

        /// <summary>
        /// Proprietary trigger _bF105Characteristics used to initiate a fresh measurement cycle or command the device to capture data.
        /// </summary>
        public static readonly Guid TakeMeasurement =
            Guid.Parse("00000006-0000-1000-8000-00805f9b34fb");

        #endregion

        #region UDS CONTROL POINT

        /// <summary>
        /// Control point opcode instructing the device to append and register a new user profile.
        /// </summary>
        public const byte RegisterNewUser = 0x01;

        /// <summary>
        /// Control point opcode used to grant or verify user authorization and consent during profile interaction.
        /// </summary>
        public const byte AuthorizeUser = 0x02;

        /// <summary>
        /// Control point opcode requesting the device to return a collection of all registered user IDs.
        /// </summary>
        public const byte ListAllUsers = 0x04;

        /// <summary>
        /// Response opcode identifier indicating that the incoming packet contains a control point execution result.
        /// </summary>
        public const byte Response = 0x20;

        /// <summary>
        /// Status code indicating that the previously executed control point command completed successfully.
        /// </summary>
        public const byte ResultSuccess = 0x01;

        /// <summary>
        /// Error status code indicating that the requested control point operation could not be completed by the device.
        /// </summary>
        public const byte ResultOperationFailed = 0x04;

        /// <summary>
        /// Error status code indicating that the operation was rejected due to insufficient user authorization.
        /// </summary>
        public const byte ResultUserNotAuthorized = 0x05;

        #endregion



    }
}