using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using BodyTracker.Models;
using System.Windows;

namespace BodyTracker.Services
{
    public class DatabaseService
    {
        /// <summary>
        /// Contains the current connection string
        /// </summary>
        public string sConnectionString { get; private set; }

        /// <summary>
        /// Represents the available database commands to insert, delete and read table values.
        /// </summary>
        public SqlCommandProvider DatabaseCommands { get; private set; } = new SqlCommandProvider();

        /// <summary>
        /// Databass Service Contructor
        /// </summary>
        /// <param name="sConnectionString">Connection String for SQL Server Connection</param>
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
            var SqlServerConnection = await EtablishSqlServerConnection();
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
            await using var SqlServerConnection = new MySqlConnection(sConnectionString);
            
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
            await using var conn = new MySqlConnection(sConnectionString);
            await conn.OpenAsync();
            var sql = DatabaseCommands.CmdGetLastPersonDimension();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@pid", personId);
            cmd.Parameters.AddWithValue("@today", today.Date);
            await using var rdr = await cmd.ExecuteReaderAsync();
            if (await rdr.ReadAsync())
            {
                return new BodyDimensionsModel
                {
                    DimensionID = rdr.GetInt32(0),
                    PersonID = rdr.GetInt32(1),
                    MeasurementDate = rdr.GetDateTime(2),
                    Chestcircumference = rdr.IsDBNull(3)?(float?)null:rdr.GetFloat(3),
                    WaistCircumference = rdr.IsDBNull(4)?(float?)null:rdr.GetFloat(4),
                    HipsCircumference = rdr.IsDBNull(5)?(float?)null:rdr.GetFloat(5)
                };
            }
            return null;
        }

        public async Task InsertMetrikAsync(BodyMetricModel m)
        {
            await using var conn = new MySqlConnection(sConnectionString);
            await conn.OpenAsync();
            var sql = DatabaseCommands.CmdInsertPersonMetric();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@pid", m.PersonID);
            cmd.Parameters.AddWithValue("@dt", m.MeasurementDate);
            cmd.Parameters.AddWithValue("@gw", (object?)m.BodyWeight ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@bmi", (object?)m.BMI ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@kf", (object?)m.BodyFatPercentage ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@mm", (object?)m.BodyMusclePercentage ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@vf", (object?)m.BodyVisceralFat ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task InsertAbmessungAsync(BodyDimensionsModel a)
        {
            await using var conn = new MySqlConnection(sConnectionString);
            await conn.OpenAsync();
            var sql = DatabaseCommands.CmdInsertPersonDimension();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@pid", a.PersonID);
            cmd.Parameters.AddWithValue("@dt", a.MeasurementDate);
            cmd.Parameters.AddWithValue("@br", (object?)a.Chestcircumference ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ba", (object?)a.WaistCircumference ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@hu", (object?)a.HipsCircumference ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<FullBodyMeasurementDatasViewModel>> GetMessungenAsync(int personId)
        {
            var list = new List<FullBodyMeasurementDatasViewModel>();
            await using var conn = new MySqlConnection(sConnectionString);
            await conn.OpenAsync();
            var sql = DatabaseCommands.CmdGetMeasurement();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@pid", personId);
            await using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new FullBodyMeasurementDatasViewModel
                {
                    MetricID = rdr.IsDBNull(0)?(int?)null:rdr.GetInt32(0),
                    DemensionID = rdr.IsDBNull(1)?(int?)null:rdr.GetInt32(1),
                    MeasurementDate = rdr.GetDateTime(2),
                    GewichtKg = rdr.IsDBNull(3)?(float?)null:rdr.GetFloat(3),
                    Bmi = rdr.IsDBNull(4)?(float?)null:rdr.GetFloat(4),
                    BodyFatPercentage = rdr.IsDBNull(5)?(float?)null:rdr.GetFloat(5),
                    BodyMusclePercentage = rdr.IsDBNull(6)?(float?)null:rdr.GetFloat(6),
                    BodyVisceralFat = rdr.IsDBNull(7)?(int?)null:rdr.GetInt32(7),
                    Chestcircumference = rdr.IsDBNull(8)?(float?)null:rdr.GetFloat(8),
                    WaistCircumference = rdr.IsDBNull(9)?(float?)null:rdr.GetFloat(9),
                    HipsCircumference = rdr.IsDBNull(10)?(float?)null:rdr.GetFloat(10)
                });
            }
            return list;
        }

        public async Task DeleteMetrikAsync(int metrikId)
        {
            await using var conn = new MySqlConnection(sConnectionString);
            await conn.OpenAsync();
            var cmd = new MySqlCommand(DatabaseCommands.CmdDeletePersonMetric(), conn);
            cmd.Parameters.AddWithValue("@id", metrikId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAbmessungAsync(int abmessungId)
        {
            await using var conn = new MySqlConnection(sConnectionString);
            await conn.OpenAsync();
            var cmd = new MySqlCommand(DatabaseCommands.CmdDeletePersonDimension(), conn);
            cmd.Parameters.AddWithValue("@id", abmessungId);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
