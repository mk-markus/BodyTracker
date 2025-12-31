using BodyTracker.MVVM.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace BodyTracker.Services
{
    /// <summary>
    /// Orchestrates all database interactions for the application. 
    /// It manages the connection lifecycle and utilizes a command provider to execute 
    /// operations against the MySQL database.
    /// </summary>
    public class DatabaseService
    {
        /// <summary>
        /// Provides the connection string used to establish communication with the MySQL database server.
       /// </summary>
        public string sConnectionString { get; private set; }

        /// <summary>
        /// Gets the provider for SQL command strings. 
        /// This property grants access to standardized SQL statements for CRUD operations (Create, Read, Update, Delete).
        /// </summary>
        public SqlCommandProvider DatabaseCommands { get; private set; } = new SqlCommandProvider();

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseService"/> class.
        /// Sets up the service with the required connection details to interact with the SQL server.
        /// </summary>
        /// <param name="sConnectionString">
        /// The full connection string, including server address, database name, and authentication credentials.
        /// </param>
        public DatabaseService(string sConnectionString) 
        { 
            this.sConnectionString = sConnectionString; 
        }


        /// <summary>
        /// Asynchronously establishes a connection to the SQL server and initializes the database schema.
        /// It ensures that all required tables (e.g., persons, metrics, dimensions) exist by executing 
        /// the corresponding 'CREATE TABLE IF NOT EXISTS' commands.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous initialization operation.</returns>
        /// <exception cref="MySqlException">Thrown when the connection fails or the SQL command execution encounters a database error.</exception>
        public async Task InitializeAsync()
        {
            var SqlServerConnection = await EtablishSqlServerConnection();
            
            await using var SqlCommand = new MySqlCommand(DatabaseCommands.CmdCreateTableIfNotExist(), SqlServerConnection);
            await SqlCommand.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Asynchronously retrieves all person records from the database.
        /// Iterates through the result set and maps each row to a <see cref="PersonModel"/>, 
        /// handling potential database null values for optional fields.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a <see cref="List{PersonModel}"/> containing all persons found in the database.
        /// </returns>
        /// <remarks>
        /// This method ensures that null values in the 'FirstName', 'LastName', or 'BirthDate' columns 
        /// are safely converted to their respective C# defaults (empty string or null).
        /// </remarks>
        public async Task<List<PersonModel>> GetPersonsAsync()
        {
            var list = new List<PersonModel>();
            await using var SqlServerConnection = await EtablishSqlServerConnection();
            await using var SqlCommand = new MySqlCommand(DatabaseCommands.CmdGetPerson(), SqlServerConnection);
            await using var SqlDataReader = await SqlCommand.ExecuteReaderAsync();
            while (await SqlDataReader.ReadAsync())
            {
                list.Add(new PersonModel
                {
                    PersonID = SqlDataReader.GetInt32(0),
                    PersonFirstName = SqlDataReader.IsDBNull(1) ? string.Empty : SqlDataReader.GetString(1),
                    PersonLastName = SqlDataReader.IsDBNull(2) ? string.Empty : SqlDataReader.GetString(2),
                    PersonBirthDate = SqlDataReader.IsDBNull(3) ? null : SqlDataReader.GetDateTime(3)
                });
            }
            return list;
        }

        /// <summary>
        /// Asynchronously retrieves the total number of records stored in the 'tbl_Personen' table.
        /// </summary>
        /// <remarks>
        /// This method utilizes <see cref="MySqlCommand.ExecuteScalarAsync"/> to efficiently fetch the count result. 
        /// It includes validation logic to ensure that 0 is returned if the database result is null or non-positive, 
        /// preventing potential conversion errors.
        /// </remarks>
        /// <returns>
        /// A task representing the asynchronous operation. 
        /// The task result contains the number of persons as an <see cref="int"/>. 
        /// Returns 0 if no records are found or if the result is null.
        /// </returns>
        public async Task<int> CountPersonAsync()
        {
            await using var SqlServerConnection = await EtablishSqlServerConnection();
            await using var SqlCommand = new MySqlCommand(DatabaseCommands.CmdCountPersonsInTable(), SqlServerConnection);
            var result = await SqlCommand.ExecuteScalarAsync();

            // Validation: If the result is null (DBNull), count is set to 0 to avoid exceptions
            int count = result != null ? Convert.ToInt32(result) : 0;

            // Return logic: Ensures the method never returns negative values, maintaining UI consistency
            return count > 0 ? count : 0;
        }

        // <summary>
        /// Asynchronously creates and opens a new connection to the MySQL database server.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains an open <see cref="MySqlConnection"/> object.
        /// </returns>
        /// <remarks>
        /// This method attempts to open the connection immediately. If the connection fails, 
        /// a <see cref="MessageBox"/> displays the error message. 
        /// Note: The caller is responsible for disposing of the returned connection.
        /// </remarks>
        public async Task<MySqlConnection> EtablishSqlServerConnection()
        {
            var SqlServerConnection = new MySqlConnection(sConnectionString);
            
            try
            {
                await SqlServerConnection.OpenAsync();

            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }

            return SqlServerConnection;
        }


        /// <summary>
        /// Asynchronously creates a new person record in the database and returns the newly generated unique identifier.
        /// </summary>
        /// <param name="FirstName">The first name of the person to be created.</param>
        /// <param name="LastName">The last name of the person to be created.</param>
        /// <param name="BirthDate">The date of birth of the person. If null, a DBNull value will be stored in the database.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the auto-incremented ID (primary key) of the newly inserted record.</returns>
        /// <returns></returns>
        public async Task<int> CreatePersonAsync(string FirstName, string LastName, DateTime? BirthDate)
        {
            var SqlServerConnection = await EtablishSqlServerConnection();
            await using var SqlCommand = new MySqlCommand(DatabaseCommands.CmdCreatePerson(), SqlServerConnection);
            SqlCommand.Parameters.AddWithValue("@v", FirstName);
            SqlCommand.Parameters.AddWithValue("@n", LastName);
            SqlCommand.Parameters.AddWithValue("@g", BirthDate.HasValue ? BirthDate.Value : (object)DBNull.Value);
            var idObj = await SqlCommand.ExecuteScalarAsync();
            return Convert.ToInt32(idObj);
        }

        /// <summary>
        /// Asynchronously retrieves the most recent body metric record for a specific person, 
        /// filtered by the provided date to ensure context-sensitive data retrieval.
        /// </summary>
        /// <param name="personId">The unique identifier of the person whose metrics are being retrieved.</param>
        /// <param name="today">The reference date used to filter or identify the relevant measurement period.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a <see cref="BodyMetricModel"/> if a record is found; otherwise, null.
        /// </returns>
        /// <remarks>
        /// This method handles nullable database fields for Weight, BMI, Fat, and Muscle percentages 
        /// by converting DBNull values to C# nullable types (float? or int?).
        /// </remarks>
        public async Task<BodyMetricModel?> GetLastBodyMetricAsync(int personId, DateTime today)
        {
            var SqlServerConnection = await EtablishSqlServerConnection();
            await using var SqlCommand = new MySqlCommand(DatabaseCommands.CmdGetLastPersonMetric(), SqlServerConnection);
            SqlCommand.Parameters.AddWithValue("@pid", personId);
            SqlCommand.Parameters.AddWithValue("@today", today.Date);
            await using var SqlDataReader = await SqlCommand.ExecuteReaderAsync();
            if (await SqlDataReader.ReadAsync())
            {
                return new BodyMetricModel
                {
                    MetricID = SqlDataReader.GetInt32(0),
                    PersonID = SqlDataReader.GetInt32(1),
                    MeasurementDate = SqlDataReader.GetDateTime(2),
                    BodyWeight = SqlDataReader.IsDBNull(3)?(float?)null:SqlDataReader.GetFloat(3),
                    BMI = SqlDataReader.IsDBNull(4)?(float?)null:SqlDataReader.GetFloat(4),
                    BodyFatPercentage = SqlDataReader.IsDBNull(5)?(float?)null:SqlDataReader.GetFloat(5),
                    BodyMusclePercentage = SqlDataReader.IsDBNull(6)?(float?)null:SqlDataReader.GetFloat(6),
                    BodyVisceralFat = SqlDataReader.IsDBNull(7)?(int?)null:SqlDataReader.GetInt32(7)
                };
            }
            return null;
        }

        /// <summary>
        /// Asynchronously retrieves the most recent physical body dimensions (e.g., chest, waist, hips) 
        /// for a specific person, filtered by a reference date.
        /// </summary>
        /// <param name="personId">The unique identifier of the person whose dimensions are being retrieved.</param>
        /// <param name="today">The reference date used to identify the relevant measurement record.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a <see cref="BodyDimensionsModel"/> if a record exists; otherwise, null.
        /// </returns>
        /// <remarks>
        /// This method safely handles optional circumference values by checking for <see cref="DBNull"/> 
        /// and mapping them to nullable float properties. It utilizes a self-contained connection 
        /// management to ensure resources are released immediately after execution.
        /// </remarks>
        public async Task<BodyDimensionsModel?> GetLastBodyDimensionsAsync(int personId, DateTime today)
        {
            var SqlServerConnection = await EtablishSqlServerConnection();
            await using var SqlCommand = new MySqlCommand(DatabaseCommands.CmdGetLastPersonDimension(), SqlServerConnection);

            SqlCommand.Parameters.AddWithValue("@pid", personId);
            SqlCommand.Parameters.AddWithValue("@today", today.Date);
            await using var rdr = await SqlCommand.ExecuteReaderAsync();
            
            if (await rdr.ReadAsync())
            {
                return new BodyDimensionsModel
                {
                    DimensionID = rdr.GetInt32(0),
                    PersonID = rdr.GetInt32(1),
                    MeasurementDate = rdr.GetDateTime(2),
                    ChestCircumference = rdr.IsDBNull(3)?(float?)null:rdr.GetFloat(3),
                    WaistCircumference = rdr.IsDBNull(4)?(float?)null:rdr.GetFloat(4),
                    HipsCircumference = rdr.IsDBNull(5)?(float?)null:rdr.GetFloat(5)
                };
            }
            return null;
        }

        /// <summary>
        /// Asynchronously inserts a new body metric record into the database.
        /// This includes physiological data such as weight, BMI, and body fat percentages.
        /// </summary>
        /// <param name="m">The <see cref="BodyMetricModel"/> containing the data to be stored.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InsertBodyMetricAsync(BodyMetricModel bodyMetricModel)
        {
            var SqlServerConnection = await EtablishSqlServerConnection();
            await using var SqlCommand = new MySqlCommand(DatabaseCommands.CmdInsertPersonMetric(), SqlServerConnection);

            SqlCommand.Parameters.AddWithValue("@pid", bodyMetricModel.PersonID);
            SqlCommand.Parameters.AddWithValue("@dt", bodyMetricModel.MeasurementDate);
            SqlCommand.Parameters.AddWithValue("@gw", (object?)bodyMetricModel.BodyWeight ?? DBNull.Value);
            SqlCommand.Parameters.AddWithValue("@bmi", (object?)bodyMetricModel.BMI ?? DBNull.Value);
            SqlCommand.Parameters.AddWithValue("@kf", (object?)bodyMetricModel.BodyFatPercentage ?? DBNull.Value);
            SqlCommand.Parameters.AddWithValue("@mm", (object?)bodyMetricModel.BodyMusclePercentage ?? DBNull.Value);
            SqlCommand.Parameters.AddWithValue("@vf", (object?)bodyMetricModel.BodyVisceralFat ?? DBNull.Value);
            
            await SqlCommand.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Asynchronously inserts a new body dimension record (circumferences) into the database.
        /// </summary>
        /// <param name="a">The <see cref="BodyDimensionsModel"/> containing the measurements.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InsertBodyDimensionAsync(BodyDimensionsModel bodyDemensionModel)
        {
            var SqlServerConnection = await EtablishSqlServerConnection();
            await using var SqlCommand = new MySqlCommand(DatabaseCommands.CmdInsertPersonDimension(), SqlServerConnection);

            SqlCommand.Parameters.AddWithValue("@pid", bodyDemensionModel.PersonID);
            SqlCommand.Parameters.AddWithValue("@dt", bodyDemensionModel.MeasurementDate);
            SqlCommand.Parameters.AddWithValue("@br", (object?)bodyDemensionModel.ChestCircumference ?? DBNull.Value);
            SqlCommand.Parameters.AddWithValue("@ba", (object?)bodyDemensionModel.WaistCircumference ?? DBNull.Value);
            SqlCommand.Parameters.AddWithValue("@hu", (object?)bodyDemensionModel.HipsCircumference ?? DBNull.Value);
            
            await SqlCommand.ExecuteNonQueryAsync();
        }


        /// <summary>
        /// Asynchronously retrieves a comprehensive list of all measurements for a specific person, 
        /// combining physiological metrics and physical dimensions into a single view model.
        /// </summary>
        /// <param name="personId">The unique identifier of the person whose history is being retrieved.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a <see cref="List{FullBodyMeasurementDatasViewModel}"/>.
        /// </returns>
        /// <remarks>
        /// This method performs a complex mapping of combined result sets. It handles cases where 
        /// either a metric or a dimension record might be missing for a specific date by using 
        /// nullable types for all measurement values.
        /// </remarks>
        public async Task<List<FullBodyMeasurementDatasViewModel>> GetBodyMeasurementAsync(int personId)
        {
            var list = new List<FullBodyMeasurementDatasViewModel>();
            
            var SqlServerConnection = await EtablishSqlServerConnection();
            await using var SqlCommand = new MySqlCommand(DatabaseCommands.CmdGetMeasurement(), SqlServerConnection);
            
            SqlCommand.Parameters.AddWithValue("@pid", personId);
            
            await using var SqlDataReader = await SqlCommand.ExecuteReaderAsync();
            
            while (await SqlDataReader.ReadAsync())
            {
                list.Add(new FullBodyMeasurementDatasViewModel
                {
                    MetricID = SqlDataReader.IsDBNull(0)?(int?)null:SqlDataReader.GetInt32(0),
                    DemensionID = SqlDataReader.IsDBNull(1)?(int?)null:SqlDataReader.GetInt32(1),
                    MeasurementDate = SqlDataReader.GetDateTime(2),
                    BodyWeight = SqlDataReader.IsDBNull(3)?(float?)null:SqlDataReader.GetFloat(3),
                    BMI = SqlDataReader.IsDBNull(4)?(float?)null:SqlDataReader.GetFloat(4),
                    BodyFatPercentage = SqlDataReader.IsDBNull(5)?(float?)null:SqlDataReader.GetFloat(5),
                    BodyMusclePercentage = SqlDataReader.IsDBNull(6)?(float?)null:SqlDataReader.GetFloat(6),
                    BodyVisceralFat = SqlDataReader.IsDBNull(7)?(int?)null:SqlDataReader.GetInt32(7),
                    ChestCircumference = SqlDataReader.IsDBNull(8)?(float?)null:SqlDataReader.GetFloat(8),
                    WaistCircumference = SqlDataReader.IsDBNull(9)?(float?)null:SqlDataReader.GetFloat(9),
                    HipsCircumference = SqlDataReader.IsDBNull(10)?(float?)null:SqlDataReader.GetFloat(10)
                });
            }

            return list;
        }

        /// <summary>
        /// Asynchronously deletes a specific metric record from the database using its unique identifier.
        /// </summary>
        /// <param name="bodyMetricId">The primary key (ID) of the metric record to be removed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteBodyMetricAsync(int bodyMetricId)
        {
            var SqlServerConnection = await EtablishSqlServerConnection();
            var SqlCommand = new MySqlCommand(DatabaseCommands.CmdDeletePersonMetric(), SqlServerConnection);
            SqlCommand.Parameters.AddWithValue("@id", bodyMetricId);
            await SqlCommand.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Asynchronously deletes a specific dimension record from the database using its unique identifier.
        /// </summary>
        /// <param name="bodyDimensionId">The primary key (ID) of the dimension record to be removed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteBodyDimensionAsync(int bodyDimensionId)
        {
            var SqlServerConnection = await EtablishSqlServerConnection();
            var SqlCommand = new MySqlCommand(DatabaseCommands.CmdDeletePersonDimension(), SqlServerConnection);
            SqlCommand.Parameters.AddWithValue("@id", bodyDimensionId);

            await SqlCommand.ExecuteNonQueryAsync();
        }
    }
}
