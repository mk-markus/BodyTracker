namespace BodyTracker.MVVM.Models
{
    /// <summary>
    /// This class contains the structure of the json file that represents the configuration parameters for the connection string.
    /// </summary>
    public class SQLConfigurationModel
    {
        /// <summary>
        /// Contains the IP Adress ot he SQL Server
        /// </summary>
        public string ServerIP { get; set; } = string.Empty;

        /// <summary>
        /// Contains the Port Number of the SQL Server --> Default Value 3306
        /// </summary>
        public string PortNumber { get; set; } = string.Empty;

        /// <summary>
        /// Contains the Database Name
        /// </summary>
        public string DatabaseName { get; set; } = string.Empty;

        /// <summary>
        /// Contains the User Name
        /// </summary>
        public string User { get; set; } = string.Empty;
        
        /// <summary>
        /// Contains the password! Store the password only in encrypted form for more security.
        /// </summary>
        public string PasswordEnc { get; set; } = string.Empty;
        
        ///// <summary>
        ///// Contains the Encraption Key for the password
        ///// </summary>
        //public string Key { get; set; } = string.Empty;

        ///// <summary>
        ///// Contains the Initialising vector
        ///// /// </summary>
        //public string IV { get; set; } = string.Empty;
    }
}
