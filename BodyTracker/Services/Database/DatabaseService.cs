using BodyTracker.Models;
using BodyTracker.Models.Chart;
using BodyTracker.Models.SamsungHealth;
using BodyTracker.Services.SamsungHealth;
using BodyTracker.Services.SamsungHelath;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;


namespace BodyTracker.Services
{
    /// <summary>
    /// Orchestrates all database interactions for the application. 
    /// It manages the sqlServerConnection lifecycle and utilizes a command provider to execute 
    /// operations against the MySQL database.
    /// </summary>
    public partial class DatabaseService : ObservableObject, IDisposable
    {
        #region Members
        /// <summary>
        /// Provides the sqlServerConnection string used to establish communication with the MySQL database server.
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Gets the provider for SQL command strings. 
        /// This property grants access to standardized SQL statements for CRUD operations (Create, Read, Update, Delete).
        /// </summary>
        public SqlCommandProvider DatabaseCommands { get; private set; } = new SqlCommandProvider();

        /// <summary>
        /// Gets or sets a value indicating whether the server sqlServerConnection is established successfully.
        /// </summary>
        private bool isConnected = false;

        /// <summary>
        /// Gets or sets a value indicating whether the database server sqlServerConnection status is operational, sending a database sqlServerConnection state message via the messenger when the value changes.
        /// </summary>
        public bool IsConnected
        {
            get => isConnected;
            set
            {
                isConnected = value;
                WeakReferenceMessenger.Default.Send(new DatabaseConnectionStateMessage(isConnected));
            }
        }

        /// <summary>
        /// Backing field for the SQL server failure message string.
        /// </summary>
        private string errorMessage = "";

        /// <summary>
        /// Gets or sets the SQL server failure message, sending a database error message via the messenger when the value changes.
        /// </summary>
        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                errorMessage = value;
                WeakReferenceMessenger.Default.Send(new DatabaseErrorMessage(errorMessage));
            }
        }

        /// <summary>
        /// Represents a cancellation token source used to safely signal and stop the background sqlServerConnection monitoring task.
        /// </summary>
        private readonly CancellationTokenSource connectionMonitorCts = new();

        /// <summary>
        /// Represents the background task responsible for periodically checking and maintaining the database server sqlServerConnection status.
        /// </summary>
        private Task? connectionMonitorTask;

        /// <summary>
        /// Represents a volatile flag indicating whether a database reconnection attempt is currently in progress to prevent concurrent execution.
        /// </summary>
        private volatile bool reconnectInProgress = false;

        /// <summary>
        /// Represents the time interval used by the background monitor to periodically check the database sqlServerConnection health.
        /// </summary>
        private readonly TimeSpan monitorInterval = TimeSpan.FromSeconds(10);

        public string DatabaseName { private set; get; } = string.Empty;

        public string DatabaseUser { private set; get; } = string.Empty;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseService"/> class.
        /// Sets up the service with the required sqlServerConnection details to interact with the SQL server.
        /// </summary>
        /// <param name="sConnectionString">
        /// The full sqlServerConnection string, including server address, database name, and authentication credentials.
        /// </param>
        public DatabaseService(string sConnectionString)
        {
            this.ConnectionString = sConnectionString;
            connectionMonitorTask = Task.Run(() => MonitorConnectionAsync(connectionMonitorCts.Token));
            Debug.WriteLine($"DatabaseService Created {GetHashCode()}");
        }

        #endregion

        #region Disposal Pattern

        // <summary>
        /// Tracks whether the object has been disposed to prevent double disposal.
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Releases all resources used by the <see cref="DatabaseService"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DatabaseService"/> class and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// true to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Debug.WriteLine($"DatabaseService Disposing managed resources {GetHashCode()}");

                try
                {
                    connectionMonitorCts.Cancel();

                    if (connectionMonitorTask != null)
                    {
                        // Wait for the monitor task to complete (max 2 seconds)
                        if (!connectionMonitorTask.Wait(2000))
                        {
                            Debug.WriteLine("DatabaseService: Monitor task did not complete within timeout");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error during sqlServerConnection monitor shutdown: {ex.Message}");
                    Debug.WriteLine($"DatabaseService Disposal Error: {ex.Message}");
                }
                finally
                {
                    // Always dispose the CancellationTokenSource
                    connectionMonitorCts?.Dispose();
                }
            }

            disposed = true;
        }

        #endregion

        #region Default Database Functions

        /// <summary>
        /// Asynchronously establishes a sqlServerConnection to the SQL server and initializes the database schema.
        /// It ensures that all required tables (e.g., persons, metrics, dimensions) exist by executing 
        /// the corresponding 'CREATE TABLE IF NOT EXISTS' commands.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous initialization operation.</returns>
        /// <exception cref="MySqlException">Thrown when the sqlServerConnection fails or the SQL command execution encounters a database error.</exception>
        public async Task InitializeDatabaseAsync()
        {
            await using var SqlServerConnection = await OpenConnectionAsync();

            await using var SqlCommand = new MySqlCommand(DatabaseCommands.CmdCreateTableIfNotExist(), SqlServerConnection);
            await SqlCommand.ExecuteNonQueryAsync();
            ErrorMessage = string.Empty;
        }

        /// <summary>
        /// Updates the database sqlServerConnection string with a new value and asynchronously triggers a sqlServerConnection check and reconnection attempt.
        /// </summary>
        /// <param name="newConnectionString">The new sqlServerConnection string to be applied.</param>
        public void UpdateConnectionString(string newConnectionString)
        {
            ConnectionString = newConnectionString;

            _ = EnsureConnectionAsync();
        }

        /// <summary>
        /// Asynchronously checks the server sqlServerConnection state and attempts to re-establish a sqlServerConnection to the SQL server if not already connected or reconnecting.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task EnsureConnectionAsync()
        {
            if (reconnectInProgress) return;

            try
            {
                reconnectInProgress = true;
                await OpenConnectionAsync();
                Debug.WriteLine("isConnected Status: " + IsConnected);
                if (!IsConnected) return;

            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error during sqlServerConnection check/reconnect: {ex.Message}";
            }
            finally
            {
                reconnectInProgress = false;
            }
        }


        /// <summary>
        /// Asynchronously runs a continuous background loop that periodically checks and attempts to re-establish the SQL server sqlServerConnection until cancellation is requested.
        /// </summary>
        /// <param name="token">A cancellation token used to stop the monitoring loop.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task MonitorConnectionAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await EnsureConnectionAsync();
                    await Task.Delay(monitorInterval, token);
                }
            }
            catch (TaskCanceledException)
            {
                ErrorMessage = "Connection monitoring task was canceled.";
            }
        }

        // <summary>
        /// Asynchronously creates and opens a new sqlServerConnection to the MySQL database server.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains an open <see cref="MySqlConnection"/> object.
        /// </returns>
        /// <remarks>
        /// This method attempts to open the sqlServerConnection immediately. If the sqlServerConnection fails, 
        /// a <see cref="MessageBox"/> displays the error message. 
        /// Note: The caller is responsible for disposing of the returned sqlServerConnection.
        /// </remarks>
        public async Task<MySqlConnection> OpenConnectionAsync()
        {
            var sqlServerConnection = new MySqlConnection(ConnectionString);
            bool previousState = IsConnected; // Vorherigen Status merken

            try
            {
                // Verbindung einmalig asynchron öffnen
                await sqlServerConnection.OpenAsync();

                // Metadaten auf der geöffneten Verbindung auslesen
                DatabaseName = sqlServerConnection.Database;

                using (var cmd = sqlServerConnection.CreateCommand())
                {
                    cmd.CommandText = "SELECT CURRENT_USER();";
                    DatabaseUser = cmd.ExecuteScalar()?.ToString();
                }

                IsConnected = sqlServerConnection.State == ConnectionState.Open;
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {

                ErrorMessage = $"Database sqlServerConnection error: {ex.Message}";
                IsConnected = false;

                sqlServerConnection.Dispose();
            }

            if (previousState != IsConnected)
            {
                WeakReferenceMessenger.Default.Send(new DatabaseConnectionStateMessage(IsConnected));
            }

            return sqlServerConnection;
        }

        /// <summary>
        /// Updates the details of an existing person in the database.
        /// </summary>
        /// <param name="PersonId">The identifier of the person to update.</param>
        /// <param name="firstName">The new first name of the person.</param>
        /// <param name="lastName">The new last name of the person.</param>
        /// <param name="birthDate">The new birth date of the person.</param>
        /// <param name="height">The new height of the person.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GetPersonUpdateSqlAsync(int PersonId, string firstName, string lastName, DateTime? birthDate, float height)
        {
            await using var conn = await OpenConnectionAsync();
            if (!IsConnected) return;

            await using var tx = await conn.BeginTransactionAsync();

            try
            {
                await using var cmdUpdatePerson = new MySqlCommand(DatabaseCommands.GetPersonUpdateSql(), conn, (MySqlTransaction)tx);
                cmdUpdatePerson.Parameters.AddWithValue("@pid", PersonId);
                cmdUpdatePerson.Parameters.AddWithValue("@v", firstName);
                cmdUpdatePerson.Parameters.AddWithValue("@n", lastName);
                cmdUpdatePerson.Parameters.AddWithValue("@g", birthDate.HasValue ? birthDate.Value : (object)DBNull.Value);
                cmdUpdatePerson.Parameters.AddWithValue("@k", height);
                await cmdUpdatePerson.ExecuteNonQueryAsync();
                await tx.CommitAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Update Person Async: {ex.Message}";
                await tx.CommitAsync();
            }
        }


        /// <summary>
        /// Asynchronously creates a new person record in the database and returns the newly generated unique identifier.
        /// </summary>
        /// <param name="FirstName">The first name of the person to be created.</param>
        /// <param name="LastName">The last name of the person to be created.</param>
        /// <param name="BirthDate">The date of birth of the person. If null, a DBNull value will be stored in the database.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the auto-incremented ID (primary key) of the newly inserted record.</returns>
        /// <returns></returns>
        public async Task<int> GetPersonCreateSqlAsync(string FirstName, string LastName, DateTime? BirthDate, double Height)
        {
            var SqlServerConnection = await OpenConnectionAsync();

            try
            {
                if (IsConnected)
                {
                    await using var SqlCommand = new MySqlCommand(DatabaseCommands.GetNewPersonCreateSql(), SqlServerConnection);
                    SqlCommand.Parameters.AddWithValue("@v", FirstName);
                    SqlCommand.Parameters.AddWithValue("@n", LastName);
                    SqlCommand.Parameters.AddWithValue("@g", BirthDate.HasValue ? BirthDate.Value : (object)DBNull.Value);
                    SqlCommand.Parameters.AddWithValue("@k", Height);
                    var idObj = await SqlCommand.ExecuteScalarAsync();
                    return Convert.ToInt32(idObj);

                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error creating person: {ex.Message}";

            }

            return 0;
        }


        #endregion

        #region Body Measurements

        /// <summary>
        /// Determines whether a bodyMeasurement exists for a specified person on a given date.
        /// </summary>
        /// <param name="dateToCheck">The date to check for an existing bodyMeasurement.</param>
        /// <param name="PersonId">The identifier of the person whose measurements are being checked.</param>
        /// <returns>true if a bodyMeasurement exists for the specified date and person; otherwise, false.</returns>
        public async Task<bool> GetMeasurementExistsSqlAsync(DateTime dateToCheck, int PersonId)
        {
            var SqlServerConnection = await OpenConnectionAsync();
            try
            {
                if (IsConnected)
                {
                    // Wir nutzen DATE(@TargetDate), um nur den Kalendertag zu vergleichen
                    // Die SQL-Abfrage sollte lauten: 
                    // "SELECT 1 FROM measurements WHERE DATE(MeasurementDate) = DATE(@TargetDate) LIMIT 1"
                    await using var SqlCommand = new MySqlCommand(DatabaseCommands.GetPersonMeasurementsExistSql(), SqlServerConnection);

                    // Wir definieren den exakten Tag ohne Uhrzeit
                    DateTime startDate = dateToCheck;
                    DateTime endDate = startDate.AddDays(1);

                    SqlCommand.Parameters.AddWithValue("@pid", PersonId);
                    SqlCommand.Parameters.AddWithValue("@start", startDate);
                    SqlCommand.Parameters.AddWithValue("@end", endDate);

                    var result = await SqlCommand.ExecuteScalarAsync();

                    // Konvertierung des Ergebnisses. Da COUNT genutzt wird, ist result nie null.
                    if (result != null && Convert.ToInt64(result) > 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Checking Measurement Existing: {ex.Message}";

            }

            return false;
        }

        /// <summary>
        /// Asynchronously inserts a new body metric record into the database.
        /// This includes physiological data such as weight, BMI, and body fat percentages.
        /// </summary>
        /// <param name="m">The <see cref="BodyMetricModel"/> containing the data to be stored.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InsertBodyMetricSqlAsync(BodyMetricModel bodyMetric)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();
            if (!IsConnected) return;
            try
            {
                TimeSpan timeWithoutMilliseconds = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                DateTime finalDate = bodyMetric.MeasurementDate.Date.Add(timeWithoutMilliseconds);

                await using var SqlCommand = new MySqlCommand(DatabaseCommands.GetPersonMetricInsertSql(), sqlServerConnection);

                SqlCommand.Parameters.AddWithValue("@pid", bodyMetric.PersonID);
                SqlCommand.Parameters.Add("@dt", (DbType)SqlDbType.DateTime2).Value = finalDate;
                SqlCommand.Parameters.AddWithValue("@gw", (object?)bodyMetric.BodyWeight ?? 0.0f);
                SqlCommand.Parameters.AddWithValue("@bmi", (object?)bodyMetric.BMI ?? 0.0f);
                SqlCommand.Parameters.AddWithValue("@kf", (object?)bodyMetric.BodyFatPercentage ?? 0.0f);
                SqlCommand.Parameters.AddWithValue("@kfo", (object?)bodyMetric.BodyFatPercentageTop ?? 0.0f);
                SqlCommand.Parameters.AddWithValue("@kfu", (object?)bodyMetric.BodyFatPercentageBottom ?? 0.0f);
                SqlCommand.Parameters.AddWithValue("@mm", (object?)bodyMetric.BodyMusclePercentage ?? 0.0f);
                SqlCommand.Parameters.AddWithValue("@mmo", (object?)bodyMetric.BodyMusclePercentageTop ?? 0.0f);
                SqlCommand.Parameters.AddWithValue("@mmu", (object?)bodyMetric.BodyMusclePercentageBottom ?? 0.0f);
                SqlCommand.Parameters.AddWithValue("@kw", (object?)bodyMetric.BodyWaterPercentage ?? 0.0f);
                SqlCommand.Parameters.AddWithValue("@kk", (object?)bodyMetric.BodyBoneMass ?? 0.0f);
                SqlCommand.Parameters.AddWithValue("@vf", (object?)bodyMetric.BodyVisceralFat ?? 0.0f);

                await SqlCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error insert Body Metric: {ex.Message}";

            }
        }

        /// <summary>
        /// Asynchronously inserts a new body dimension record (circumferences) into the database.
        /// </summary>
        /// <param name="a">The <see cref="BodyDimensionsModel"/> containing the measurements.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InsertBodyDimensionSqlAsync(BodyDimensionsModel bodyDemension)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();
            if (!IsConnected) return;

            try
            {
                TimeSpan timeWithoutMilliseconds = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                DateTime finalDate = bodyDemension.MeasurementDate.Date.Add(timeWithoutMilliseconds);

                await using var SqlCommand = new MySqlCommand(DatabaseCommands.GetPersonDimensionInsertSql(), sqlServerConnection);

                SqlCommand.Parameters.AddWithValue("@pid", bodyDemension.PersonID);
                SqlCommand.Parameters.Add("@dt", (DbType)SqlDbType.DateTime2).Value = finalDate;
                SqlCommand.Parameters.AddWithValue("@br", (object?)bodyDemension.ChestCircumference ?? 0.0f);
                SqlCommand.Parameters.AddWithValue("@ba", (object?)bodyDemension.WaistCircumference ?? 0.0f);
                SqlCommand.Parameters.AddWithValue("@hu", (object?)bodyDemension.HipsCircumference ?? 0.0f);
                SqlCommand.Parameters.AddWithValue("@fzbreast", (object?)bodyDemension.FatTongBreastCrease ?? 0.0f);
                SqlCommand.Parameters.AddWithValue("@fzarmpit", (object?)bodyDemension.FatTongArmpitCrease ?? 0.0f);
                SqlCommand.Parameters.AddWithValue("@fzabdomen", (object?)bodyDemension.FatTongAbdominalCrease ?? 0.0f);
                SqlCommand.Parameters.AddWithValue("@fzhip", (object?)bodyDemension.FatTongHipCrease ?? 0.0f);
                SqlCommand.Parameters.AddWithValue("@fzthigh", (object?)bodyDemension.FatTongThighCrease ?? 0.0f);
                SqlCommand.Parameters.AddWithValue("@fzback", (object?)bodyDemension.FatTongBackCrease ?? 0.0f);
                SqlCommand.Parameters.AddWithValue("@fztricep", (object?)bodyDemension.FatTongTricepsCrease ?? 0.0f);
                await SqlCommand.ExecuteNonQueryAsync();

            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Insert Body Dimension: {ex.Message}";
            }
        }

        /// <summary>
        /// Asynchronously retrieves the most recent body metric record for a specific person, 
        /// filtered by the provided date to ensure context-sensitive data retrieval.
        /// </summary>
        /// <param name="personId">The unique identifier of the person whose metrics are being retrieved.</param>
        /// <param name="today">The reference date used to filter or identify the relevant bodyMeasurement period.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a <see cref="BodyMetricModel"/> if a record is found; otherwise, null.
        /// </returns>
        /// <remarks>
        /// This method handles nullable database fields for Weight, BMI, Fat, and Muscle percentages 
        /// by converting DBNull values to C# nullable types (float? or int?).
        /// </remarks>
        public async Task<BodyMetricModel?> GetLatestBodyMetricAsync(int personId, DateTime today)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return new BodyMetricModel();

            try
            {
                await using var SqlCommand = new MySqlCommand(DatabaseCommands.GetLastPersonMetricsSql(), sqlServerConnection);
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
                        BodyWeight = SqlDataReader.IsDBNull(3) ? (float?)null : SqlDataReader.GetFloat(3),
                        BMI = SqlDataReader.IsDBNull(4) ? (float?)null : SqlDataReader.GetFloat(4),
                        BodyFatPercentage = SqlDataReader.IsDBNull(5) ? (float?)null : SqlDataReader.GetFloat(5),
                        BodyFatPercentageTop = SqlDataReader.IsDBNull(6) ? (float?)null : SqlDataReader.GetFloat(6),
                        BodyFatPercentageBottom = SqlDataReader.IsDBNull(7) ? (float?)null : SqlDataReader.GetFloat(7),
                        BodyMusclePercentage = SqlDataReader.IsDBNull(8) ? (float?)null : SqlDataReader.GetFloat(8),
                        BodyMusclePercentageTop = SqlDataReader.IsDBNull(9) ? (float?)null : SqlDataReader.GetFloat(9),
                        BodyMusclePercentageBottom = SqlDataReader.IsDBNull(10) ? (float?)null : SqlDataReader.GetFloat(10),
                        BodyWaterPercentage = SqlDataReader.IsDBNull(12) ? (float?)null : SqlDataReader.GetFloat(12),
                        BodyBoneMass = SqlDataReader.IsDBNull(11) ? (float?)null : SqlDataReader.GetFloat(11),
                        BodyVisceralFat = SqlDataReader.IsDBNull(13) ? (int?)null : SqlDataReader.GetInt32(13)
                    };
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Get Last Body Metrics: {ex.Message}";
            }

            return new BodyMetricModel();
        }

        /// <summary>
        /// Asynchronously retrieves the most recent physical body dimensions (e.g., chest, waist, hips) 
        /// for a specific person, filtered by a reference date.
        /// </summary>
        /// <param name="personId">The unique identifier of the person whose dimensions are being retrieved.</param>
        /// <param name="today">The reference date used to identify the relevant bodyMeasurement record.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a <see cref="BodyDimensionsModel"/> if a record exists; otherwise, null.
        /// </returns>
        /// <remarks>
        /// This method safely handles optional circumference values by checking for <see cref="DBNull"/> 
        /// and mapping them to nullable float properties. It utilizes a self-contained sqlServerConnection 
        /// management to ensure resources are released immediately after execution.
        /// </remarks>
        public async Task<BodyDimensionsModel?> GetLatestBodyDimensionsAsync(int personId, DateTime today)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return new BodyDimensionsModel();

            try
            {
                await using var SqlCommand = new MySqlCommand(DatabaseCommands.GetLastPersonDimensionsSql(), sqlServerConnection);

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
                        ChestCircumference = rdr.IsDBNull(3) ? (float?)null : rdr.GetFloat(3),
                        WaistCircumference = rdr.IsDBNull(4) ? (float?)null : rdr.GetFloat(4),
                        HipsCircumference = rdr.IsDBNull(5) ? (float?)null : rdr.GetFloat(5),
                        FatTongBreastCrease = rdr.IsDBNull(6) ? (float?)null : rdr.GetFloat(6),
                        FatTongArmpitCrease = rdr.IsDBNull(7) ? (float?)null : rdr.GetFloat(7),
                        FatTongAbdominalCrease = rdr.IsDBNull(8) ? (float?)null : rdr.GetFloat(8),
                        FatTongHipCrease = rdr.IsDBNull(9) ? (float?)null : rdr.GetFloat(9),
                        FatTongThighCrease = rdr.IsDBNull(10) ? (float?)null : rdr.GetFloat(10),
                        FatTongBackCrease = rdr.IsDBNull(11) ? (float?)null : rdr.GetFloat(11),
                        FatTongTricepsCrease = rdr.IsDBNull(12) ? (float?)null : rdr.GetFloat(12)
                    };
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Get Body Demensions: {ex.Message}";
            }

            return new BodyDimensionsModel();
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
        /// nullable types for all bodyMeasurement values.
        /// </remarks>
        public async Task<List<FullBodyMeasurementDatasModel>> GetBodyMeasurementAsync(int personId, string sqlCommand)
        {
            var list = new List<FullBodyMeasurementDatasModel>();

            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return list;

            try
            {
                await using var SqlCommand = new MySqlCommand(sqlCommand, sqlServerConnection);

                SqlCommand.Parameters.AddWithValue("@pid", personId);

                await using var SqlDataReader = await SqlCommand.ExecuteReaderAsync();

                while (await SqlDataReader.ReadAsync())
                {
                    list.Add(new FullBodyMeasurementDatasModel
                    {
                        MetricID = SqlDataReader.IsDBNull(0) ? (int?)null : SqlDataReader.GetInt32(0),
                        DemensionID = SqlDataReader.IsDBNull(1) ? (int?)null : SqlDataReader.GetInt32(1),
                        MeasurementDate = SqlDataReader.GetDateTime(2),
                        BodyWeight = SqlDataReader.IsDBNull(3) ? (float?)null : SqlDataReader.GetFloat(3),
                        BMI = SqlDataReader.IsDBNull(4) ? (float?)null : SqlDataReader.GetFloat(4),
                        BodyFatPercentage = SqlDataReader.IsDBNull(5) ? (float?)null : SqlDataReader.GetFloat(5),
                        BodyFatPercentageTop = SqlDataReader.IsDBNull(6) ? (float?)null : SqlDataReader.GetFloat(6),
                        BodyFatPercentageBottom = SqlDataReader.IsDBNull(7) ? (float?)null : SqlDataReader.GetFloat(7),
                        BodyMusclePercentage = SqlDataReader.IsDBNull(8) ? (float?)null : SqlDataReader.GetFloat(8),
                        BodyMusclePercentageTop = SqlDataReader.IsDBNull(9) ? (float?)null : SqlDataReader.GetFloat(9),
                        BodyMusclePercentageBottom = SqlDataReader.IsDBNull(10) ? (float?)null : SqlDataReader.GetFloat(10),
                        BodyWaterPercentage = SqlDataReader.IsDBNull(12) ? (float?)null : SqlDataReader.GetFloat(12),
                        BodyBoneMass = SqlDataReader.IsDBNull(11) ? (float?)null : SqlDataReader.GetFloat(11),
                        BodyVisceralFat = SqlDataReader.IsDBNull(13) ? (int?)null : SqlDataReader.GetInt32(13),
                        ChestCircumference = SqlDataReader.IsDBNull(14) ? (float?)null : SqlDataReader.GetFloat(14),
                        WaistCircumference = SqlDataReader.IsDBNull(15) ? (float?)null : SqlDataReader.GetFloat(15),
                        HipsCircumference = SqlDataReader.IsDBNull(16) ? (float?)null : SqlDataReader.GetFloat(16),
                        FatTongBreastCrease = SqlDataReader.IsDBNull(17) ? (float?)null : SqlDataReader.GetFloat(17),
                        FatTongArmpitCrease = SqlDataReader.IsDBNull(18) ? (float?)null : SqlDataReader.GetFloat(18),
                        FatTongAbdominalCrease = SqlDataReader.IsDBNull(19) ? (float?)null : SqlDataReader.GetFloat(19),
                        FatTongHipCrease = SqlDataReader.IsDBNull(20) ? (float?)null : SqlDataReader.GetFloat(20),
                        FatTongThighCrease = SqlDataReader.IsDBNull(21) ? (float?)null : SqlDataReader.GetFloat(21),
                        FatTongBackCrease = SqlDataReader.IsDBNull(22) ? (float?)null : SqlDataReader.GetFloat(22),
                        FatTongTricepsCrease = SqlDataReader.IsDBNull(23) ? (float?)null : SqlDataReader.GetFloat(23)
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Get Body Measurement Async: {ex.Message}";
            }

            return list;
        }

        /// <summary>
        /// Asynchronously deletes a specific metric record from the database using its unique identifier.
        /// </summary>
        /// <param name="bodyMetricId">The primary key (ID) of the metric record to be removed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GetBodyMetricDeleteSqlAsync(int bodyMetricId)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return;

            try
            {
                var sqlCommand = new MySqlCommand(DatabaseCommands.GetPersonMetricDeleteSql(), sqlServerConnection);
                sqlCommand.Parameters.AddWithValue("@id", bodyMetricId);
                await sqlCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Delete Body Metric Async: {ex.Message}";
            }
        }

        /// <summary>
        /// Asynchronously deletes a specific dimension record from the database using its unique identifier.
        /// </summary>
        /// <param name="bodyDimensionId">The primary key (ID) of the dimension record to be removed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GetBodyDimensionDeleteSqlAsync(int bodyDimensionId)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();
            if (!IsConnected) return;

            try
            {
                var sqlCommand = new MySqlCommand(DatabaseCommands.GetPersonDimensionDeleteSql(), sqlServerConnection);
                sqlCommand.Parameters.AddWithValue("@id", bodyDimensionId);

                await sqlCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Delete Body Dimension Async: {ex.Message}";
            }
        }

        /// <summary>
        /// Performs an atomic upsert operation for both metric and dimension datasets within a single database transaction.
        /// </summary>
        /// <param name="personId">The unique identifier of the target user.</param>
        /// <param name="metricId">Optional ID for existing metric records; if null, an INSERT is performed.</param>
        /// <param name="dimensionId">Optional ID for existing dimension records; if null, an INSERT is performed.</param>
        /// <param name="measurementDate">The effective timestamp for the bodyMeasurement record.</param>
        /// <param name="bodyWeight">Measured body weight in kilograms.</param>
        /// <param name="bmi">Calculated Body Mass Index.</param>
        /// <param name="fat">Body fat percentage.</param>
        /// <param name="muscle">Muscle mass percentage.</param>
        /// <param name="visceralFat">Visceral fat level indicator.</param>
        /// <param name="chest">Chest circumference in centimeters.</param>
        /// <param name="waist">Waist circumference in centimeters.</param>
        /// <param name="hips">Hip circumference in centimeters.</param>
        /// <param name="fattongs">Skinfold bodyMeasurement (caliper) value.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous database operation.</returns>
        /// <exception cref="MySqlException">Thrown if the transaction fails or the sqlServerConnection is interrupted.</exception>
        public async Task GetMeasurementUpdateSqlAsync(
            int personId,
            int? metricId, int? dimensionId,
            DateTime measurementDate,
            float? bodyWeight, float? bmi, float? fat, float? fato, float? fatu, float? muscle, float? muscleo, float? muscleu, float? bodyw, float? bodyb, int? visceralFat,
            float? chest, float? waist, float? hips, float? fatTongBreastCrease, float? fatTongArmpitCrease, float? fatTongAbdominalCrease,
            float? fatTongHipCrease, float? fatTongThighCrease, float? fatTongBackCrease, float? fatTongTricepsCrease)
        {
            await using var conn = await OpenConnectionAsync();

            if (!IsConnected) return;

            await using var tx = await conn.BeginTransactionAsync();
            try
            {
                TimeSpan timeWithoutMilliseconds = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                DateTime finalDate = measurementDate.Date.Add(timeWithoutMilliseconds);

                if (metricId.HasValue)
                {
                    await using var cmdUpdateMetric = new MySqlCommand(DatabaseCommands.GetPersonMetricUpdateSql(), conn, (MySqlTransaction)tx);
                    cmdUpdateMetric.Parameters.AddWithValue("@mid", metricId.Value);
                    cmdUpdateMetric.Parameters.AddWithValue("@gw", (object?)bodyWeight ?? DBNull.Value);
                    cmdUpdateMetric.Parameters.AddWithValue("@bmi", (object?)bmi ?? DBNull.Value);
                    cmdUpdateMetric.Parameters.AddWithValue("@kf", (object?)fat ?? DBNull.Value);
                    cmdUpdateMetric.Parameters.AddWithValue("@kfo", (object?)fato ?? DBNull.Value);
                    cmdUpdateMetric.Parameters.AddWithValue("@kfu", (object?)fatu ?? DBNull.Value);
                    cmdUpdateMetric.Parameters.AddWithValue("@mm", (object?)muscle ?? DBNull.Value);
                    cmdUpdateMetric.Parameters.AddWithValue("@mmo", (object?)muscleo ?? DBNull.Value);
                    cmdUpdateMetric.Parameters.AddWithValue("@mmu", (object?)muscleu ?? DBNull.Value);
                    cmdUpdateMetric.Parameters.AddWithValue("@kw", (object?)bodyw ?? DBNull.Value);
                    cmdUpdateMetric.Parameters.AddWithValue("@kk", (object?)bodyb ?? DBNull.Value);
                    cmdUpdateMetric.Parameters.AddWithValue("@vf", (object?)visceralFat ?? DBNull.Value);
                    await cmdUpdateMetric.ExecuteNonQueryAsync();
                }
                //else
                //{
                //    await using var cmdInsertMetric = new MySqlCommand(DatabaseCommands.GetPersonMetricInsertSql(), conn, (MySqlTransaction)tx);
                //    cmdInsertMetric.Parameters.Add("@dt", (DbType)SqlDbType.DateTime2).Value = finalDate;
                //    cmdInsertMetric.Parameters.AddWithValue("@mid", metricId.Value);
                //    cmdInsertMetric.Parameters.AddWithValue("@gw", (object?)bodyWeight ?? DBNull.Value);
                //    cmdInsertMetric.Parameters.AddWithValue("@bmi", (object?)bmi ?? DBNull.Value);
                //    cmdInsertMetric.Parameters.AddWithValue("@kf", (object?)fat ?? DBNull.Value);
                //    cmdInsertMetric.Parameters.AddWithValue("@kfo", (object?)fato ?? DBNull.Value);
                //    cmdInsertMetric.Parameters.AddWithValue("@kfu", (object?)fatu ?? DBNull.Value);
                //    cmdInsertMetric.Parameters.AddWithValue("@mm", (object?)muscle ?? DBNull.Value);
                //    cmdInsertMetric.Parameters.AddWithValue("@mmo", (object?)muscleo ?? DBNull.Value);
                //    cmdInsertMetric.Parameters.AddWithValue("@mmu", (object?)muscleu ?? DBNull.Value);
                //    cmdInsertMetric.Parameters.AddWithValue("@kw", (object?)bodyw ?? DBNull.Value);
                //    cmdInsertMetric.Parameters.AddWithValue("@kk", (object?)bodyb ?? DBNull.Value);
                //    cmdInsertMetric.Parameters.AddWithValue("@vf", (object?)visceralFat ?? DBNull.Value);
                //    await cmdInsertMetric.ExecuteNonQueryAsync();
                //}

                if (dimensionId.HasValue)
                {
                    await using var cmdUpdateDim = new MySqlCommand(DatabaseCommands.GetPersonDimensionUpdateSql(), conn, (MySqlTransaction)tx);
                    cmdUpdateDim.Parameters.AddWithValue("@aid", dimensionId.Value);
                    cmdUpdateDim.Parameters.AddWithValue("@br", (object?)chest ?? DBNull.Value);
                    cmdUpdateDim.Parameters.AddWithValue("@ba", (object?)waist ?? DBNull.Value);
                    cmdUpdateDim.Parameters.AddWithValue("@hu", (object?)hips ?? DBNull.Value);
                    cmdUpdateDim.Parameters.AddWithValue("@fzbreast", (object?)fatTongBreastCrease ?? DBNull.Value);
                    cmdUpdateDim.Parameters.AddWithValue("@fzarmpit", (object?)fatTongArmpitCrease ?? DBNull.Value);
                    cmdUpdateDim.Parameters.AddWithValue("@fzabdomen", (object?)fatTongAbdominalCrease ?? DBNull.Value);
                    cmdUpdateDim.Parameters.AddWithValue("@fzhip", (object?)fatTongHipCrease ?? DBNull.Value);
                    cmdUpdateDim.Parameters.AddWithValue("@fzthigh", (object?)fatTongThighCrease ?? DBNull.Value);
                    cmdUpdateDim.Parameters.AddWithValue("@fzback", (object?)fatTongBackCrease ?? DBNull.Value);
                    cmdUpdateDim.Parameters.AddWithValue("@fztricep", (object?)fatTongTricepsCrease ?? DBNull.Value);
                    await cmdUpdateDim.ExecuteNonQueryAsync();
                }
                //else
                //{
                //    await using var cmdInsertDim = new MySqlCommand(DatabaseCommands.GetPersonDimensionInsertSql(), conn, (MySqlTransaction)tx);
                //    cmdInsertDim.Parameters.AddWithValue("@pid", personId);
                //    cmdInsertDim.Parameters.Add("@dt", (DbType)SqlDbType.DateTime2).Value = finalDate;
                //    cmdInsertDim.Parameters.AddWithValue("@br", (object?)chest ?? DBNull.Value);
                //    cmdInsertDim.Parameters.AddWithValue("@ba", (object?)waist ?? DBNull.Value);
                //    cmdInsertDim.Parameters.AddWithValue("@hu", (object?)hips ?? DBNull.Value);
                //    cmdInsertDim.Parameters.AddWithValue("@fzbrust", (object?)fatTongBreastCrease ?? DBNull.Value);
                //    cmdInsertDim.Parameters.AddWithValue("@fzarmpit", (object?)fatTongArmpitCrease ?? DBNull.Value);
                //    cmdInsertDim.Parameters.AddWithValue("@fzabdomen", (object?)fatTongAbdominalCrease ?? DBNull.Value);
                //    cmdInsertDim.Parameters.AddWithValue("@fzhip", (object?)fatTongHipCrease ?? DBNull.Value);
                //    cmdInsertDim.Parameters.AddWithValue("@fzthigh", (object?)fatTongThighCrease ?? DBNull.Value);
                //    cmdInsertDim.Parameters.AddWithValue("@fzback", (object?)fatTongBackCrease ?? DBNull.Value);
                //    cmdInsertDim.Parameters.AddWithValue("@fztriceps", (object?)fatTongTricepsCrease ?? DBNull.Value);
                //    await cmdInsertDim.ExecuteNonQueryAsync();
                //}

                await tx.CommitAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Update Measurement Async: {ex.Message}";
                await tx.RollbackAsync();
            }
        }


        #endregion

        #region Samsung Health Food Intake

        /// <summary>
        /// Inserts a collection of Samsung Health food intake records for a specific person into the database.
        /// Utilizes a database transaction to ensure data integrity; performs a rollback in case of failure.
        /// </summary>
        /// <param name="PersonId">The unique identifier of the person to whom the data belongs.</param>
        /// <param name="foodIntakeList">A collection of <see cref="SamsungFoodIntakeModel"/> objects containing the nutritional data to be stored.</param>
        /// <param name="progress">An optional progress indicator that reports the percentage of the insertion process.</param>
        /// <returns>Returns 'true' if the operation completes successfully.</returns>
        public async Task<bool> InsertFoodIntakeSqlAsync(int PersonId, ICollection<SamsungFoodIntakeModel> foodIntakeList, IProgress<double>? progress = null)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();
            if (!IsConnected) return false;
            using var tx = await sqlServerConnection.BeginTransactionAsync();

            try
            {

                int total = foodIntakeList.Count;
                int current = 0;

                foreach (var foodIntake in foodIntakeList)
                {
                    current++;

                    await using var sqlCommand = new MySqlCommand(DatabaseCommands.GetSamsungFoodIntakeUpsertSql(),
                                                                sqlServerConnection,
                                                                (MySqlTransaction)tx);

                    sqlCommand.Parameters.AddWithValue("@PersonID_FK", PersonId);
                    sqlCommand.Parameters.AddWithValue("@CreateShVer", foodIntake.CreateShVer);
                    sqlCommand.Parameters.AddWithValue("@StartTime", foodIntake.StartTime);
                    sqlCommand.Parameters.AddWithValue("@Amount", foodIntake.Amount);
                    sqlCommand.Parameters.AddWithValue("@Custom", (object?)foodIntake.Custom ?? string.Empty);
                    sqlCommand.Parameters.AddWithValue("@ModifyShVer", foodIntake.ModifyShVer);
                    sqlCommand.Parameters.AddWithValue("@UpdateTime", foodIntake.UpdateTime);
                    sqlCommand.Parameters.AddWithValue("@CreateTime", foodIntake.CreateTime);
                    sqlCommand.Parameters.AddWithValue("@MealType", foodIntake.MealType);
                    sqlCommand.Parameters.AddWithValue("@ClientDataId", (object?)foodIntake.ClientDataId ?? string.Empty);
                    sqlCommand.Parameters.AddWithValue("@Name", foodIntake.Name);
                    sqlCommand.Parameters.AddWithValue("@Unit", foodIntake.Unit);
                    sqlCommand.Parameters.AddWithValue("@ClientDataVer", (object?)foodIntake.ClientDataVer ?? string.Empty);
                    sqlCommand.Parameters.AddWithValue("@Calorie", foodIntake.Calorie);
                    sqlCommand.Parameters.AddWithValue("@TimeOffset", foodIntake.TimeOffset);
                    sqlCommand.Parameters.AddWithValue("@DeviceUuid", foodIntake.DeviceUuid);
                    sqlCommand.Parameters.AddWithValue("@Comment", (object?)foodIntake.Comment ?? string.Empty);
                    sqlCommand.Parameters.AddWithValue("@PkgName", foodIntake.PkgName);
                    sqlCommand.Parameters.AddWithValue("@DataUuid", foodIntake.DataUuid.ToString());
                    sqlCommand.Parameters.AddWithValue("@FoodInfoId", (object?)foodIntake.FoodInfoId ?? string.Empty);

                    await sqlCommand.ExecuteNonQueryAsync();

                    progress?.Report((double)current / total * 100);
                }

                await tx.CommitAsync();

                progress?.Report(100);

                return true;


            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error inserting Samsung Health food intake: {ex.Message}";
                await tx.RollbackAsync();
            }
            return false;
        }

        /// <summary>
        /// Asynchronously synchronizes a collection of Samsung Health food intake records for a specific person into the database, 
        /// performing bulk insertions for new entries and transactional updates for existing records based on unique data identifiers and update timestamps.
        /// </summary>
        /// <param name="PersonId">The unique identifier of the person to whom the data belongs.</param>
        /// <param name="foodIntakeList">A collection of <see cref="SamsungFoodIntakeModel"/> objects containing the nutritional data to be stored.</param>
        /// <param name="progress">An optional progress indicator that reports the percentage of the insertion process.</param>
        /// <returns>Returns 'true' if the operation completes successfully.</returns>
        public async Task<bool> UpsertFoodIntakeSqlAsync(int personId, ICollection<SamsungFoodIntakeModel> foodIntakeList,
                                                  IProgress<(double value, string prgressText)>? progress = null)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return false;

            try
            {

                var existingEntries = new Dictionary<string, DateTime?>();

                // Get the data_uuid and update_time from the exisiting food intake in the databasse tbl_FoodIntake 
                await using var cmd = new MySqlCommand(DatabaseCommands.GetFoodIntakeExistenceCheckSql(),
                                                        sqlServerConnection);

                cmd.Parameters.AddWithValue("@PersonId", personId);

                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        existingEntries.Add(reader.GetString("data_uuid"),
                                            reader.IsDBNull("update_at")
                                                ? null
                                                : reader.GetDateTime("update_at"));
                    }
                }
                var inserts = new List<SamsungFoodIntakeModel>();
                var updates = new List<SamsungFoodIntakeModel>();

                // Compare the Exercise List with the existitng values in the database tbl_FoodIntake.
                // If the value exist then we update the entries. Is the value new then we insert
                // it in the table with bulk inserting.
                foreach (var item in foodIntakeList)
                {
                    if (string.IsNullOrWhiteSpace(item.DataUuid)) continue;

                    if (!existingEntries.TryGetValue(item.DataUuid, out var dbUpdateTime))
                    {
                        inserts.Add(item);
                        continue;
                    }

                    if (item.UpdateTime.HasValue && (!dbUpdateTime.HasValue || item.UpdateTime > dbUpdateTime))
                    {
                        updates.Add(item);
                    }
                }

                if (inserts.Count > 0)
                {
                    var table = SamsungFoodIntakeDataTable.CreateFoodIntakeDataTable(personId, inserts);

                    var bulkCopy = new MySqlBulkCopy(sqlServerConnection)
                    {
                        DestinationTableName = "tbl_FoodIntake",
                        NotifyAfter = 1000
                    };

                    bulkCopy.MySqlRowsCopied += (sender, e) =>
                    {
                        double percentage = (double)e.RowsCopied / inserts.Count * 100;

                        progress?.Report((percentage, $"Inserted: "));
                    };



                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(0, "person_id"));

                    #region ... rest of bulk inserting

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(1, "create_sync_version"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(2, "start_time"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(3, "amount_current"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(4, "amount_initial"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(5, "custom_text"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(6, "modify_sync_version"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(7, "update_at"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(8, "create_at"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(9, "meal_type"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(10, "client_data_id"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(11, "food_name"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(12, "unit_id"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(13, "client_data_version"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(14, "calories_current"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(15, "calories_initial"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(16, "time_offset"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(17, "device_uuid"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(18, "comment_text"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(19, "package_name"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(20, "data_uuid"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(21, "food_info_id"));

                    #endregion

                    await bulkCopy.WriteToServerAsync(table);
                }


                int current = 0;
                await using var transaction = await sqlServerConnection.BeginTransactionAsync();

                int total = updates.Count();

                // Update the exisiting datas in tbl_HeartRate
                foreach (var item in updates)
                {
                    await using var command = new MySqlCommand(DatabaseCommands.GetSamsungHealthFoodIntakeUpdateSql(),
                                                            sqlServerConnection, (MySqlTransaction)transaction);

                    command.Parameters.AddWithValue("@PersonID_FK", personId);

                    #region ... rest of AddWithValue

                    command.Parameters.AddWithValue("@CreateShVer", (object?)item.CreateShVer ?? DBNull.Value);
                    command.Parameters.AddWithValue("@StartTime", (object?)item.StartTime ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Amount", (object?)item.Amount ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Custom", (object?)item.Custom ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ModifyShVer", (object?)item.ModifyShVer ?? DBNull.Value);
                    command.Parameters.AddWithValue("@UpdateTime", (object?)item.UpdateTime ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CreateTime", (object?)item.CreateTime ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MealType", (object?)item.MealType ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ClientDataId", (object?)item.ClientDataId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Name", (object?)item.Name ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Unit", (object?)item.Unit ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ClientDataVer", (object?)item.ClientDataVer ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Calorie", (object?)item.Calorie ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TimeOffset", (object?)item.TimeOffset ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DeviceUuid", (object?)item.DeviceUuid ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Comment", (object?)item.Comment ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PkgName", (object?)item.PkgName ?? DBNull.Value);
                    command.Parameters.AddWithValue("@FoodInfoId", (object?)item.FoodInfoId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DataUuid", item.DataUuid);

                    #endregion

                    await command.ExecuteNonQueryAsync();

                    current++;

                    if (current % 100 == 0)
                    {
                        double percentage = (double)current / total * 100;

                        progress?.Report((percentage, "Updated: "));
                    }
                }

                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                //await transaction.RollbackAsync();
                ErrorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Asynchronously retrieves and returns Samsung food intake records for a specified person.
        /// </summary>
        /// <remarks>Establishes a database connection, executes the food intake command retrieved from <see cref="DatabaseCommands.GetSamsungFoodIntakeSql"/> with the given person identifier parameter, and reads the result set into a list of <see cref="SamsungFoodIntakeModel"/> instances.</remarks>
        /// <param name="personId">The unique identifier of the person whose food intake records are being requested.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of <see cref="SamsungFoodIntakeModel"/> objects.</returns>
        public async Task<List<SamsungFoodIntakeModel>> GetSamsungFoodIntakeSqlAsync(int personId)
        {
            var result = new List<SamsungFoodIntakeModel>();

            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected)
                return result;

            try
            {
                var sqlCommand = new MySqlCommand(
                    DatabaseCommands.GetSamsungFoodIntakeSql(),
                    sqlServerConnection);

                sqlCommand.Parameters.AddWithValue("@PersonID", personId);

                await using var reader = await sqlCommand.ExecuteReaderAsync();
                result.Clear();
                while (await reader.ReadAsync())
                {
                    result.Add(new SamsungFoodIntakeModel
                    {
                        FoodIntakeID = reader.GetInt32("food_intake_id"),
                        CreateShVer = reader.GetInt64("create_sync_version"),
                        StartTime = reader.GetDateTime("start_time"),
                        Amount = reader.GetDouble("amount_current"),
                        Custom = reader.IsDBNull("custom_text")
                            ? null
                            : reader.GetString("custom_text"),
                        ModifyShVer = reader.GetInt64("modify_sync_version"),
                        UpdateTime = reader.IsDBNull("update_at")
                            ? null
                            : reader.GetDateTime("update_at"),
                        CreateTime = reader.GetDateTime("create_at"),
                        MealType = reader.GetInt32("meal_type"),
                        ClientDataId = reader.GetString("client_data_id"),
                        Name = reader.GetString("food_name"),
                        Unit = reader.GetInt64("unit_id"),
                        ClientDataVer = reader.IsDBNull("client_data_version")
                            ? null
                            : reader.GetString("client_data_version"),
                        Calorie = reader.GetDouble("calories_current"),
                        TimeOffset = reader.GetString("time_offset"),
                        DeviceUuid = reader.GetString("device_uuid"),
                        Comment = reader.IsDBNull("comment_text")
                            ? null
                            : reader.GetString("comment_text"),
                        PkgName = reader.GetString("package_name"),
                        DataUuid = reader.IsDBNull("data_uuid")
                            ? null
                            : reader.GetString("data_uuid"),
                        FoodInfoId = reader.IsDBNull("food_info_id")
                            ? null
                            : reader.GetString("food_info_id")
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Get Food Intake Async: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Asynchronously deletes a specific food intake record from the database using its unique identifier.
        /// </summary>
        /// <param name="foodIntakeID">The primary key (ID) of the food intake record to be removed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GetFoodIntakeDeleteSqlAsync(int foodIntakeID)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return;

            try
            {
                var sqlCommand = new MySqlCommand(DatabaseCommands.GetFoodIntakeDeleteSql(), sqlServerConnection);
                sqlCommand.Parameters.AddWithValue("@id", foodIntakeID);
                await sqlCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Delete Food Intake Async: {ex.Message}";
            }
        }

        #endregion

        #region Samsung Health Exercise

        /// <summary>
        /// Inserts a collection of Samsung Health exercise records for a specific person into the database.
        /// Uses an upsert strategy based on data_uuid.
        /// </summary>
        /// <param name="PersonId">Person identifier.</param>
        /// <param name="exerciseList">Exercise records to save.</param>
        /// <param name="progress">Optional progress reporting.</param>
        /// <returns>True if successful; otherwise false.</returns>
        public async Task<bool> InsertExerciseSqlAsync(int PersonId,
                                                  ICollection<SamsungExerciseModel> exerciseList,
                                                  IProgress<double>? progress = null)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();
            if (!IsConnected) return false;

            using var tx = await sqlServerConnection.BeginTransactionAsync();

            try
            {
                int total = exerciseList.Count;
                int current = 0;

                foreach (var exercise in exerciseList)
                {
                    current++;

                    await using var sqlCommand = new MySqlCommand(
                        DatabaseCommands.GetSamsungExerciseUpsertSql(),
                        sqlServerConnection,
                        (MySqlTransaction)tx);

                    sqlCommand.Parameters.AddWithValue("@person_id", PersonId);

                    sqlCommand.Parameters.AddWithValue("@LiveDataInternal", (object?)exercise.LiveDataInternal ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@MissionValue", (object?)exercise.MissionValue ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@RaceTarget", (object?)exercise.RaceTarget ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@SubsetData", (object?)exercise.SubsetData ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@StartLongitude", (object?)exercise.StartLongitude ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@RoutineDataUuid", (object?)exercise.RoutineDataUuid ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@TotalCalorie", (object?)exercise.TotalCalorie ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@CompletionStatus", (object?)exercise.CompletionStatus ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@PaceInfoId", (object?)exercise.PaceInfoId ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@ActivityType", (object?)exercise.ActivityType ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@PaceLiveData", (object?)exercise.PaceLiveData ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@SensingStatus", (object?)exercise.SensingStatus ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@SourceType", (object?)exercise.SourceType ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@MissionType", (object?)exercise.MissionType ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@Ftp", (object?)exercise.Ftp ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@TrackingStatus", (object?)exercise.TrackingStatus ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@ProgramId", (object?)exercise.ProgramId ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@Title", (object?)exercise.Title ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@RewardStatus", (object?)exercise.RewardStatus ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@HeartRateSampleCount", (object?)exercise.HeartRateSampleCount ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@StartLatitude", (object?)exercise.StartLatitude ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@MissionExtraValue", (object?)exercise.MissionExtraValue ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@ProgramScheduleId", (object?)exercise.ProgramScheduleId ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@HeartRateDeviceUuid", (object?)exercise.HeartRateDeviceUuid ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@LocationDataInternal", (object?)exercise.LocationDataInternal ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@CustomId", (object?)exercise.CustomId ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@AdditionalInternal", (object?)exercise.AdditionalInternal ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@Duration", (object?)exercise.Duration ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@Additional", (object?)exercise.Additional ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@CreateShVer", (object?)exercise.CreateShVer ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@MeanCaloricBurnRate", (object?)exercise.MeanCaloricBurnRate ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@LocationData", (object?)exercise.LocationData ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@StartTime", (object?)exercise.StartTime ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@ExerciseType", (object?)exercise.ExerciseType ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@Custom", (object?)exercise.Custom ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@MaxAltitude", (object?)exercise.MaxAltitude ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@InclineDistance", (object?)exercise.InclineDistance ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@MeanHeartRate", (object?)exercise.MeanHeartRate ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@CountType", (object?)exercise.CountType ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@MeanRpm", (object?)exercise.MeanRpm ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@MinAltitude", (object?)exercise.MinAltitude ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@ModifyShVer", (object?)exercise.ModifyShVer ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@MaxHeartRate", (object?)exercise.MaxHeartRate ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@UpdateTime", (object?)exercise.UpdateTime ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@CreateTime", (object?)exercise.CreateTime ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@ClientDataId", (object?)exercise.ClientDataId ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@MaxPower", (object?)exercise.MaxPower ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@MaxSpeed", (object?)exercise.MaxSpeed ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@MeanCadence", (object?)exercise.MeanCadence ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@MinHeartRate", (object?)exercise.MinHeartRate ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@ClientDataVer", (object?)exercise.ClientDataVer ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@StepCount", (object?)exercise.Count ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@Distance", (object?)exercise.Distance ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@MaxCaloricBurnRate", (object?)exercise.MaxCaloricBurnRate ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@Calorie", (object?)exercise.Calorie ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@MaxCadence", (object?)exercise.MaxCadence ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@DeclineDistance", (object?)exercise.DeclineDistance ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@Vo2Max", (object?)exercise.Vo2Max ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@TimeOffset", (object?)exercise.TimeOffset ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@DeviceUuid", (object?)exercise.DeviceUuid ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@MaxRpm", (object?)exercise.MaxRpm ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@Comment", (object?)exercise.Comment ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@LiveData", (object?)exercise.LiveData ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@MeanPower", (object?)exercise.MeanPower ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@MeanSpeed", (object?)exercise.MeanSpeed ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@PkgName", (object?)exercise.PkgName ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@AltitudeGain", (object?)exercise.AltitudeGain ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@AltitudeLoss", (object?)exercise.AltitudeLoss ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@ExerciseCustomType", (object?)exercise.ExerciseCustomType ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@AuxiliaryDevices", (object?)exercise.AuxiliaryDevices ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@EndTime", (object?)exercise.EndTime ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@DataUuid", (object?)exercise.DataUuid ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@SweatLoss", (object?)exercise.SweatLoss ?? DBNull.Value);


                    await sqlCommand.ExecuteNonQueryAsync();

                    progress?.Report((double)current / total * 100);

                }

                await tx.CommitAsync();

                progress?.Report(100);

                return true;


            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error inserting Samsung Exercises: {ex.Message}";
                await tx.RollbackAsync();
            }
            return false;
        }

        /// <summary>
        /// Asynchronously synchronizes a collection of Samsung Health exercise records for a specific person into the database, 
        /// performing bulk insertions for new entries and transactional updates for existing records based on unique data identifiers and update timestamps.
        /// </summary>
        /// <param name="personId">The unique identifier of the person associated with the exercise records.</param>
        /// <param name="exerciseList">The collection of Samsung exercise records to synchronize.</param>
        /// <param name="progress">An optional progress reporter for tracking completion percentages and status messages.</param>
        /// <returns>True if successful; otherwise false.</returns>
        public async Task<bool> UpsertExerciseSqlAsync(int personId, ICollection<SamsungExerciseModel> exerciseList,
                                                  IProgress<(double value, string prgressText)>? progress = null)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return false;

            try
            {

                var existingEntries = new Dictionary<string, DateTime?>();

                // Get the data_uuid and update_time from the exisiting exercise in the databasse tbl_Exercise 
                await using var cmd = new MySqlCommand(DatabaseCommands.GetSamsungExerciseExistenceCheckSql(),
                                                        sqlServerConnection);

                cmd.Parameters.AddWithValue("@PersonId", personId);

                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        existingEntries.Add(
                            reader.GetString("data_uuid"),
                            reader.IsDBNull("update_at")
                                ? null
                                : reader.GetDateTime("update_at"));
                    }
                }
                var inserts = new List<SamsungExerciseModel>();
                var updates = new List<SamsungExerciseModel>();

                // Compare the Exercise List with the existitng values in the database tbl_Exercise.
                // If the value exist then we updarte the entrie. Is the value new then we insert
                // it in the table with bulk inserting.
                foreach (var item in exerciseList)
                {
                    if (string.IsNullOrWhiteSpace(item.DataUuid)) continue;

                    if (!existingEntries.TryGetValue(item.DataUuid, out var dbUpdateTime))
                    {
                        inserts.Add(item);
                        continue;
                    }

                    if (item.UpdateTime.HasValue && (!dbUpdateTime.HasValue || item.UpdateTime > dbUpdateTime))
                    {
                        updates.Add(item);
                    }
                }

                if (inserts.Count > 0)
                {
                    var table = SamsungExerciseDataTable.CreateExerciseDataTable(personId, inserts);

                    var bulkCopy = new MySqlBulkCopy(sqlServerConnection)
                    {
                        DestinationTableName = "tbl_Exercise",
                        NotifyAfter = 1000
                    };

                    bulkCopy.MySqlRowsCopied += (sender, e) =>
                    {
                        double percentage = (double)e.RowsCopied / inserts.Count * 100;

                        progress?.Report((percentage, $"Inserted: "));
                    };



                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(0, "person_id"));

                    #region ... rest of bulk copy 

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(1, "live_data_internal"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(2, "mission_value"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(3, "race_target"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(4, "subset_data"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(5, "start_longitude"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(6, "routine_data_uuid"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(7, "total_calorie"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(8, "completion_status"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(9, "pace_info_id"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(10, "activity_type"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(11, "pace_live_data"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(12, "sensing_status"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(13, "source_type"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(14, "mission_type"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(15, "ftp"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(16, "tracking_status"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(17, "program_id"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(18, "title"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(19, "reward_status"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(20, "heart_rate_sample_count"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(21, "start_latitude"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(22, "mission_extra_value"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(23, "program_schedule_id"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(24, "heart_rate_device_uuid"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(25, "location_data_internal"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(26, "custom_id"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(27, "additional_internal"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(28, "duration"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(29, "additional"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(30, "create_sync_version"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(31, "mean_caloric_burn_rate"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(32, "location_data"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(33, "start_time"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(34, "exercise_type"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(35, "custom_text"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(36, "max_altitude"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(37, "incline_distance"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(38, "mean_heart_rate"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(39, "count_type"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(40, "mean_rpm"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(41, "min_altitude"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(42, "modify_sync_version"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(43, "max_heart_rate"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(44, "update_at"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(45, "create_at"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(46, "client_data_id"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(47, "max_power"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(48, "max_speed"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(49, "mean_cadence"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(50, "min_heart_rate"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(51, "client_data_version"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(52, "count_value"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(53, "distance"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(54, "max_caloric_burn_rate"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(55, "calorie"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(56, "max_cadence"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(57, "decline_distance"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(58, "vo2_max"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(59, "time_offset"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(60, "device_uuid"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(61, "max_rpm"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(62, "comment_text"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(63, "live_data"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(64, "mean_power"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(65, "mean_speed"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(66, "package_name"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(67, "altitude_gain"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(68, "altitude_loss"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(69, "exercise_custom_type"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(70, "auxiliary_devices"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(71, "end_time"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(72, "data_uuid"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(73, "sweat_loss"));

                    #endregion

                    await bulkCopy.WriteToServerAsync(table);
                }


                int current = 0;
                await using var transaction = await sqlServerConnection.BeginTransactionAsync();

                int total = updates.Count();

                // Update the exisiting datas in tbl_HeartRate
                foreach (var item in updates)
                {
                    await using var command = new MySqlCommand(DatabaseCommands.GetExerciseUpdateSql(),
                                                            sqlServerConnection, (MySqlTransaction)transaction);

                    command.Parameters.AddWithValue("@LiveDataInternal", (object?)item.LiveDataInternal ?? DBNull.Value);

                    #region ... rest of AddWithValue

                    command.Parameters.AddWithValue("@MissionValue", (object?)item.MissionValue ?? DBNull.Value);
                    command.Parameters.AddWithValue("@RaceTarget", (object?)item.RaceTarget ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SubsetData", (object?)item.SubsetData ?? DBNull.Value);
                    command.Parameters.AddWithValue("@StartLongitude", (object?)item.StartLongitude ?? DBNull.Value);
                    command.Parameters.AddWithValue("@RoutineDataUuid", (object?)item.RoutineDataUuid ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TotalCalorie", (object?)item.TotalCalorie ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CompletionStatus", (object?)item.CompletionStatus ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PaceInfoId", (object?)item.PaceInfoId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ActivityType", (object?)item.ActivityType ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PaceLiveData", (object?)item.PaceLiveData ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SensingStatus", (object?)item.SensingStatus ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SourceType", (object?)item.SourceType ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MissionType", (object?)item.MissionType ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Ftp", (object?)item.Ftp ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TrackingStatus", (object?)item.TrackingStatus ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ProgramId", (object?)item.ProgramId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Title", (object?)item.Title ?? DBNull.Value);
                    command.Parameters.AddWithValue("@RewardStatus", (object?)item.RewardStatus ?? DBNull.Value);
                    command.Parameters.AddWithValue("@HeartRateSampleCount", (object?)item.HeartRateSampleCount ?? DBNull.Value);
                    command.Parameters.AddWithValue("@StartLatitude", (object?)item.StartLatitude ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MissionExtraValue", (object?)item.MissionExtraValue ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ProgramScheduleId", (object?)item.ProgramScheduleId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@HeartRateDeviceUuid", (object?)item.HeartRateDeviceUuid ?? DBNull.Value);
                    command.Parameters.AddWithValue("@LocationDataInternal", (object?)item.LocationDataInternal ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CustomId", (object?)item.CustomId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AdditionalInternal", (object?)item.AdditionalInternal ?? DBNull.Value);

                    command.Parameters.AddWithValue("@Duration", (object?)item.Duration ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Additional", (object?)item.Additional ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CreateShVer", (object?)item.CreateShVer ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MeanCaloricBurnRate", (object?)item.MeanCaloricBurnRate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@LocationData", (object?)item.LocationData ?? DBNull.Value);
                    command.Parameters.AddWithValue("@StartTime", (object?)item.StartTime ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ExerciseType", (object?)item.ExerciseType ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Custom", (object?)item.Custom ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MaxAltitude", (object?)item.MaxAltitude ?? DBNull.Value);
                    command.Parameters.AddWithValue("@InclineDistance", (object?)item.InclineDistance ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MeanHeartRate", (object?)item.MeanHeartRate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CountType", (object?)item.CountType ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MeanRpm", (object?)item.MeanRpm ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MinAltitude", (object?)item.MinAltitude ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ModifyShVer", (object?)item.ModifyShVer ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MaxHeartRate", (object?)item.MaxHeartRate ?? DBNull.Value);

                    command.Parameters.AddWithValue("@UpdateTime", (object?)item.UpdateTime ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CreateTime", (object?)item.CreateTime ?? DBNull.Value);

                    command.Parameters.AddWithValue("@ClientDataId", (object?)item.ClientDataId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MaxPower", (object?)item.MaxPower ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MaxSpeed", (object?)item.MaxSpeed ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MeanCadence", (object?)item.MeanCadence ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MinHeartRate", (object?)item.MinHeartRate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ClientDataVer", (object?)item.ClientDataVer ?? DBNull.Value);
                    command.Parameters.AddWithValue("@StepCount", (object?)item.Count ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Distance", (object?)item.Distance ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MaxCaloricBurnRate", (object?)item.MaxCaloricBurnRate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Calorie", (object?)item.Calorie ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MaxCadence", (object?)item.MaxCadence ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DeclineDistance", (object?)item.DeclineDistance ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Vo2Max", (object?)item.Vo2Max ?? DBNull.Value);

                    command.Parameters.AddWithValue("@TimeOffset", (object?)item.TimeOffset ?? DBNull.Value);

                    command.Parameters.AddWithValue("@DeviceUuid", (object?)item.DeviceUuid ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MaxRpm", (object?)item.MaxRpm ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Comment", (object?)item.Comment ?? DBNull.Value);
                    command.Parameters.AddWithValue("@LiveData", (object?)item.LiveData ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MeanPower", (object?)item.MeanPower ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MeanSpeed", (object?)item.MeanSpeed ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PkgName", (object?)item.PkgName ?? DBNull.Value);

                    command.Parameters.AddWithValue("@AltitudeGain", (object?)item.AltitudeGain ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AltitudeLoss", (object?)item.AltitudeLoss ?? DBNull.Value);

                    command.Parameters.AddWithValue("@ExerciseCustomType", (object?)item.ExerciseCustomType ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AuxiliaryDevices", (object?)item.AuxiliaryDevices ?? DBNull.Value);

                    command.Parameters.AddWithValue("@EndTime", (object?)item.EndTime ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DataUuid", item.DataUuid);
                    command.Parameters.AddWithValue("@SweatLoss", (object?)item.SweatLoss ?? DBNull.Value);

                    #endregion

                    await command.ExecuteNonQueryAsync();

                    current++;

                    if (current % 100 == 0)
                    {
                        double percentage = (double)current / total * 100;

                        progress?.Report((percentage, "Updated: "));
                    }
                }

                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                //await transaction.RollbackAsync();
                ErrorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Asynchronously retrieves and returns daily step trend chart records for a specified person.
        /// </summary>
        /// <remarks>Establishes a database connection, executes the step trend command retrieved from <see cref="DatabaseCommands.GetSamsungStepTrendSpecificSql"/> with the given person identifier parameter, and reads the result set into a list of <see cref="SamsungStepTrendDashboardModel"/> instances.</remarks>
        /// <param name="personId">The unique identifier of the person whose step trends are being requested.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of <see cref="SamsungStepTrendDashboardModel"/> objects.</returns>
        public async Task<List<SamsungExerciseDashboardModel>> GetExerciseDashboardSqlAsync(int personId)
        {
            var result = new List<SamsungExerciseDashboardModel>();

            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected)
                return result;

            try
            {
                await using var sqlCommand =
                    new MySqlCommand(
                        DatabaseCommands.GetSamsungExerciseDashboardSql(),
                        sqlServerConnection);

                sqlCommand.Parameters.AddWithValue("@PersonID", personId);

                await using var reader =
                    await sqlCommand.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(new SamsungExerciseDashboardModel
                    {
                        StartTime = reader.GetDateTime("start_time"),

                        Title = reader.IsDBNull("title")
                            ? null
                            : reader.GetString("title"),

                        ExerciseType = reader.IsDBNull("exercise_type")
                            ? null
                            : reader.GetInt32("exercise_type"),

                        Duration = reader.IsDBNull("duration")
                            ? null
                            : reader.GetInt64("duration"),

                        Distance = reader.IsDBNull("distance")
                            ? null
                            : reader.GetDouble("distance"),

                        Calorie = reader.IsDBNull("calorie")
                            ? null
                            : reader.GetDouble("calorie"),

                        MeanHeartRate = reader.IsDBNull("mean_heart_rate")
                            ? null
                            : reader.GetDouble("mean_heart_rate"),

                        MaxHeartRate = reader.IsDBNull("max_heart_rate")
                            ? null
                            : reader.GetDouble("max_heart_rate"),

                        MinHeartRate = reader.IsDBNull("min_heart_rate")
                            ? null
                            : reader.GetDouble("min_heart_rate"),

                        MeanSpeed = reader.IsDBNull("mean_speed")
                            ? null
                            : reader.GetDouble("mean_speed")
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Get Exercise Async: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves and returns Samsung step trend records for a specified person.
        /// </summary>
        /// <remarks>Establishes a database connection, executes the step trend command retrieved from <see cref="DatabaseCommands.GetSamsungStepTrendSql"/> with the given person identifier parameter, and reads the result set into a list of <see cref="SamsungStepTrendModel"/> instances.</remarks>
        /// <param name="personId">The unique identifier of the person whose step trends are being requested.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of <see cref="SamsungStepTrendModel"/> objects.</returns>
        public async Task<List<SamsungExerciseModel>> GetSamsungExerciseAsync(int personId)
        {
            var result = new List<SamsungExerciseModel>();

            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected)
                return result;

            try
            {
                var sqlCommand = new MySqlCommand(
                    DatabaseCommands.GetSamsungExerciseSql(),
                    sqlServerConnection);

                sqlCommand.Parameters.AddWithValue("@PersonID", personId);

                await using var reader = await sqlCommand.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(new SamsungExerciseModel
                    {
                        ExerciseID = reader.IsDBNull("exercise_id") ? null : reader.GetInt32("exercise_id"),
                        LiveDataInternal = reader.IsDBNull("live_data_internal") ? null : reader.GetString("live_data_internal"),
                        MissionValue = reader.IsDBNull("mission_value") ? null : reader.GetString("mission_value"),
                        RaceTarget = reader.IsDBNull("race_target") ? null : reader.GetString("race_target"),
                        SubsetData = reader.IsDBNull("subset_data") ? null : reader.GetString("subset_data"),
                        StartLongitude = reader.IsDBNull("start_longitude") ? null : reader.GetDouble("start_longitude"),
                        RoutineDataUuid = reader.IsDBNull("routine_data_uuid") ? null : reader.GetString("routine_data_uuid"),
                        TotalCalorie = reader.IsDBNull("total_calorie") ? null : reader.GetDouble("total_calorie"),
                        CompletionStatus = reader.IsDBNull("completion_status") ? null : reader.GetInt32("completion_status"),
                        PaceInfoId = reader.IsDBNull("pace_info_id") ? null : reader.GetInt64("pace_info_id"),
                        ActivityType = reader.IsDBNull("activity_type") ? null : reader.GetInt32("activity_type"),
                        PaceLiveData = reader.IsDBNull("pace_live_data") ? null : reader.GetString("pace_live_data"),
                        SensingStatus = reader.IsDBNull("sensing_status") ? null : reader.GetString("sensing_status"),
                        SourceType = reader.IsDBNull("source_type") ? null : reader.GetInt32("source_type"),
                        MissionType = reader.IsDBNull("mission_type") ? null : reader.GetInt32("mission_type"),
                        Ftp = reader.IsDBNull("ftp") ? null : reader.GetDouble("ftp"),
                        TrackingStatus = reader.IsDBNull("tracking_status") ? null : reader.GetInt32("tracking_status"),
                        ProgramId = reader.IsDBNull("program_id") ? null : reader.GetInt64("program_id"),
                        Title = reader.IsDBNull("title") ? null : reader.GetString("title"),
                        RewardStatus = reader.IsDBNull("reward_status") ? null : reader.GetInt32("reward_status"),
                        HeartRateSampleCount = reader.IsDBNull("heart_rate_sample_count") ? null : reader.GetInt32("heart_rate_sample_count"),
                        StartLatitude = reader.IsDBNull("start_latitude") ? null : reader.GetDouble("start_latitude"),
                        MissionExtraValue = reader.IsDBNull("mission_extra_value") ? null : reader.GetString("mission_extra_value"),
                        ProgramScheduleId = reader.IsDBNull("program_schedule_id") ? null : reader.GetInt64("program_schedule_id"),
                        HeartRateDeviceUuid = reader.IsDBNull("heart_rate_device_uuid") ? null : reader.GetString("heart_rate_device_uuid"),
                        LocationDataInternal = reader.IsDBNull("location_data_internal") ? null : reader.GetString("location_data_internal"),
                        CustomId = reader.IsDBNull("custom_id") ? null : reader.GetString("custom_id"),
                        AdditionalInternal = reader.IsDBNull("additional_internal") ? null : reader.GetString("additional_internal"),

                        Duration = reader.IsDBNull("duration") ? null : reader.GetInt64("duration"),
                        Additional = reader.IsDBNull("additional") ? null : reader.GetString("additional"),
                        CreateShVer = reader.IsDBNull("create_sync_version") ? null : reader.GetString("create_sync_version"),
                        MeanCaloricBurnRate = reader.IsDBNull("mean_caloric_burn_rate") ? null : reader.GetDouble("mean_caloric_burn_rate"),
                        LocationData = reader.IsDBNull("location_data") ? null : reader.GetString("location_data"),
                        StartTime = reader.IsDBNull("start_time") ? null : reader.GetDateTime("start_time"),
                        ExerciseType = reader.IsDBNull("exercise_type") ? null : reader.GetInt32("exercise_type"),
                        Custom = reader.IsDBNull("custom_text") ? null : reader.GetString("custom_text"),
                        MaxAltitude = reader.IsDBNull("max_altitude") ? null : reader.GetDouble("max_altitude"),
                        InclineDistance = reader.IsDBNull("incline_distance") ? null : reader.GetDouble("incline_distance"),
                        MeanHeartRate = reader.IsDBNull("mean_heart_rate") ? null : reader.GetDouble("mean_heart_rate"),
                        CountType = reader.IsDBNull("count_type") ? null : reader.GetInt32("count_type"),
                        MeanRpm = reader.IsDBNull("mean_rpm") ? null : reader.GetDouble("mean_rpm"),
                        MinAltitude = reader.IsDBNull("min_altitude") ? null : reader.GetDouble("min_altitude"),
                        ModifyShVer = reader.IsDBNull("modify_sync_version") ? null : reader.GetString("modify_sync_version"),
                        MaxHeartRate = reader.IsDBNull("max_heart_rate") ? null : reader.GetDouble("max_heart_rate"),

                        UpdateTime = reader.IsDBNull("update_at") ? null : reader.GetDateTime("update_at"),
                        CreateTime = reader.IsDBNull("create_at") ? null : reader.GetDateTime("create_at"),

                        ClientDataId = reader.IsDBNull("client_data_id") ? null : reader.GetString("client_data_id"),
                        MaxPower = reader.IsDBNull("max_power") ? null : reader.GetDouble("max_power"),
                        MaxSpeed = reader.IsDBNull("max_speed") ? null : reader.GetDouble("max_speed"),
                        MeanCadence = reader.IsDBNull("mean_cadence") ? null : reader.GetDouble("mean_cadence"),
                        MinHeartRate = reader.IsDBNull("min_heart_rate") ? null : reader.GetDouble("min_heart_rate"),
                        ClientDataVer = reader.IsDBNull("client_data_version") ? null : reader.GetString("client_data_version"),
                        Count = reader.IsDBNull("count_value") ? null : reader.GetInt32("count_value"),
                        Distance = reader.IsDBNull("distance") ? null : reader.GetDouble("distance"),
                        MaxCaloricBurnRate = reader.IsDBNull("max_caloric_burn_rate") ? null : reader.GetDouble("max_caloric_burn_rate"),
                        Calorie = reader.IsDBNull("calorie") ? null : reader.GetDouble("calorie"),
                        MaxCadence = reader.IsDBNull("max_cadence") ? null : reader.GetDouble("max_cadence"),
                        DeclineDistance = reader.IsDBNull("decline_distance") ? null : reader.GetDouble("decline_distance"),
                        Vo2Max = reader.IsDBNull("vo2_max") ? null : reader.GetDouble("vo2_max"),
                        TimeOffset = reader.IsDBNull("time_offset") ? null : reader.GetString("time_offset"),
                        DeviceUuid = reader.IsDBNull("device_uuid") ? null : reader.GetString("device_uuid"),
                        MaxRpm = reader.IsDBNull("max_rpm") ? null : reader.GetDouble("max_rpm"),
                        Comment = reader.IsDBNull("comment_text") ? null : reader.GetString("comment_text"),
                        LiveData = reader.IsDBNull("live_data") ? null : reader.GetString("live_data"),
                        MeanPower = reader.IsDBNull("mean_power") ? null : reader.GetDouble("mean_power"),
                        MeanSpeed = reader.IsDBNull("mean_speed") ? null : reader.GetDouble("mean_speed"),
                        PkgName = reader.IsDBNull("package_name") ? null : reader.GetString("package_name"),
                        AltitudeGain = reader.IsDBNull("altitude_gain") ? null : reader.GetDouble("altitude_gain"),
                        AltitudeLoss = reader.IsDBNull("altitude_loss") ? null : reader.GetDouble("altitude_loss"),
                        ExerciseCustomType = reader.IsDBNull("exercise_custom_type") ? null : reader.GetInt32("exercise_custom_type"),
                        AuxiliaryDevices = reader.IsDBNull("auxiliary_devices") ? null : reader.GetString("auxiliary_devices"),
                        EndTime = reader.IsDBNull("end_time") ? null : reader.GetDateTime("end_time"),
                        DataUuid = reader.IsDBNull("data_uuid") ? null : reader.GetString("data_uuid"),
                        SweatLoss = reader.IsDBNull("sweat_loss") ? null : reader.GetDouble("sweat_loss")
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Get Samsung Exercise Async: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Asynchronously deletes a specific exercise record from the database using its unique identifier.
        /// </summary>
        /// <param name="exerciseID">The primary key (ID) of the exercise record to be removed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GetExerciseDeleteSqlAsync(int exerciseID)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return;

            try
            {
                var sqlCommand = new MySqlCommand(DatabaseCommands.GetSamsungExerciseDeleteSql(), sqlServerConnection);
                sqlCommand.Parameters.AddWithValue("@id", exerciseID);
                await sqlCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Delete Exercise Async: {ex.Message}";
            }
        }

        #endregion

        #region Samsung Health Heart Rate


        /// <summary>
        /// Inserts a collection of Samsung Health heart rate records for a specific person.
        /// Uses a transaction to ensure data consistency.
        /// </summary>
        /// <param name="personId">The unique identifier of the person.</param>
        /// <param name="heartRates">The heart rate records to store.</param>
        /// <param name="progress">Optional progress reporter.</param>
        /// <returns>True if the operation succeeds; otherwise false.</returns>
        public async Task<bool> InsertHeartRatesSqlAsync(int personId,
                                                    ICollection<SamsungHeartRateModel> heartRates,
                                                    IProgress<double>? progress = null)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected)
                return false;

            await using var tx =
                await sqlServerConnection.BeginTransactionAsync();

            try
            {
                int total = heartRates.Count;
                int current = 0;

                await using var cmd = new MySqlCommand(
                    DatabaseCommands.GetSamsungHeartRateUpsertSql(),
                    sqlServerConnection,
                    (MySqlTransaction)tx);

                cmd.Parameters.Add("@PersonId", MySqlDbType.Int64);
                cmd.Parameters.Add("@Source", MySqlDbType.VarChar);
                cmd.Parameters.Add("@TagId", MySqlDbType.VarChar);
                cmd.Parameters.Add("@CreateShVer", MySqlDbType.VarChar);

                cmd.Parameters.Add("@StartTime", MySqlDbType.DateTime);
                cmd.Parameters.Add("@EndTime", MySqlDbType.DateTime);
                cmd.Parameters.Add("@UpdateTime", MySqlDbType.DateTime);
                cmd.Parameters.Add("@CreateTime", MySqlDbType.DateTime);

                cmd.Parameters.Add("@TimeOffset", MySqlDbType.VarChar);
                cmd.Parameters.Add("@Custom", MySqlDbType.LongText);
                cmd.Parameters.Add("@BinningData", MySqlDbType.LongText);
                cmd.Parameters.Add("@ModifyShVer", MySqlDbType.VarChar);

                cmd.Parameters.Add("@ClientDataId", MySqlDbType.VarChar);

                cmd.Parameters.Add("@HeartRate", MySqlDbType.Double);
                cmd.Parameters.Add("@HeartRateMax", MySqlDbType.Double);
                cmd.Parameters.Add("@HeartRateMin", MySqlDbType.Double);

                cmd.Parameters.Add("@HeartBeatCount", MySqlDbType.Int32);

                cmd.Parameters.Add("@ClientDataVer", MySqlDbType.VarChar);

                cmd.Parameters.Add("@Comment", MySqlDbType.LongText);
                cmd.Parameters.Add("@PackageName", MySqlDbType.VarChar);

                cmd.Parameters.Add("@DeviceUuid", MySqlDbType.VarChar);
                cmd.Parameters.Add("@DataUuid", MySqlDbType.VarChar);

                await cmd.PrepareAsync();

                foreach (var heartRate in heartRates)
                {
                    current++;

                    cmd.Parameters["@PersonId"].Value = personId;
                    cmd.Parameters["@Source"].Value =
                        (object?)heartRate.Source ?? DBNull.Value;

                    cmd.Parameters["@TagId"].Value =
                        (object?)heartRate.TagId ?? DBNull.Value;

                    cmd.Parameters["@CreateShVer"].Value =
                        (object?)heartRate.CreateShVer ?? DBNull.Value;

                    cmd.Parameters["@StartTime"].Value =
                        (object?)heartRate.StartTime ?? DBNull.Value;

                    cmd.Parameters["@EndTime"].Value =
                        (object?)heartRate.EndTime ?? DBNull.Value;

                    cmd.Parameters["@UpdateTime"].Value =
                        (object?)heartRate.UpdateTime ?? DBNull.Value;

                    cmd.Parameters["@CreateTime"].Value =
                        (object?)heartRate.CreateTime ?? DBNull.Value;

                    cmd.Parameters["@TimeOffset"].Value =
                        (object?)heartRate.TimeOffset ?? DBNull.Value;

                    cmd.Parameters["@Custom"].Value =
                        (object?)heartRate.Custom ?? DBNull.Value;

                    cmd.Parameters["@BinningData"].Value =
                        (object?)heartRate.BinningData ?? DBNull.Value;

                    cmd.Parameters["@ModifyShVer"].Value =
                        (object?)heartRate.ModifyShVer ?? DBNull.Value;

                    cmd.Parameters["@ClientDataId"].Value =
                        (object?)heartRate.ClientDataId ?? DBNull.Value;

                    cmd.Parameters["@HeartRate"].Value =
                        (object?)heartRate.HeartRate ?? DBNull.Value;

                    cmd.Parameters["@HeartRateMax"].Value =
                        (object?)heartRate.HeartRateMax ?? DBNull.Value;

                    cmd.Parameters["@HeartRateMin"].Value =
                        (object?)heartRate.HeartRateMin ?? DBNull.Value;

                    cmd.Parameters["@HeartBeatCount"].Value =
                        (object?)heartRate.HeartBeatCount ?? DBNull.Value;

                    cmd.Parameters["@ClientDataVer"].Value =
                        (object?)heartRate.ClientDataVer ?? DBNull.Value;

                    cmd.Parameters["@Comment"].Value =
                        (object?)heartRate.Comment ?? DBNull.Value;

                    cmd.Parameters["@PackageName"].Value =
                        (object?)heartRate.PackageName ?? DBNull.Value;

                    cmd.Parameters["@DeviceUuid"].Value =
                        (object?)heartRate.DeviceUuid ?? DBNull.Value;

                    cmd.Parameters["@DataUuid"].Value =
                        (object?)heartRate.DataUuid ?? DBNull.Value;

                    await cmd.ExecuteNonQueryAsync();

                    if (current % 100 == 0)
                    {
                        progress?.Report(
                            (double)current / total * 100);
                    }
                }

                await tx.CommitAsync();

                progress?.Report(100);

                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Asynchronously synchronizes a collection of Samsung Health Heart Rate records for a specific person into the database, 
        /// performing bulk insertions for new entries and transactional updates for existing records based on unique data identifiers and update timestamps.
        /// </summary>
        /// <param name="personId">The unique identifier of the person.</param>
        /// <param name="heartRates">The heart rate records to store.</param>
        /// <param name="progress">Optional progress reporter.</param>
        /// <returns>True if the operation succeeds; otherwise false.</returns>
        public async Task<bool> UpsertHeartRatesSqlAsync(int personId, ICollection<SamsungHeartRateModel> heartRates,
                                                    IProgress<(double value, string prgressText)>? progress = null)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return false;

            try
            {

                var existingEntries = new Dictionary<string, DateTime?>();

                // Get the data_uuid and update_time from the exisiting heart rates in the databasse tbl_HeartRate 
                await using var cmd = new MySqlCommand(DatabaseCommands.GetHeartRateExistenceCheckSql(),
                                                        sqlServerConnection);

                cmd.Parameters.AddWithValue("@PersonId", personId);

                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        existingEntries.Add(
                            reader.GetString("data_uuid"),
                            reader.IsDBNull("update_time")
                                ? null
                                : reader.GetDateTime("update_time"));
                    }
                }
                var inserts = new List<SamsungHeartRateModel>();
                var updates = new List<SamsungHeartRateModel>();

                // Compare the heartRates List with the existitng values in the database tbl_HeartRate. If the value exist then we updarte the entrie.
                // Is the value new then we insert
                // it in the table with bulk inserting.
                foreach (var item in heartRates)
                {
                    if (string.IsNullOrWhiteSpace(item.DataUuid)) continue;

                    if (!existingEntries.TryGetValue(item.DataUuid, out var dbUpdateTime))
                    {
                        inserts.Add(item);
                        continue;
                    }

                    if (item.UpdateTime.HasValue && (!dbUpdateTime.HasValue || item.UpdateTime > dbUpdateTime))
                    {
                        updates.Add(item);
                    }
                }

                if (inserts.Count > 0)
                {
                    var table = SamsungHeartRateDataTable.CreateHeartRateDataTable(personId, inserts);

                    var bulkCopy = new MySqlBulkCopy(sqlServerConnection)
                    {
                        DestinationTableName = "tbl_HeartRate",
                        NotifyAfter = 1000
                    };

                    bulkCopy.MySqlRowsCopied += (sender, e) =>
                    {
                        double percentage = (double)e.RowsCopied / inserts.Count * 100;

                        progress?.Report((percentage,
                            $"Inserted: "));
                    };

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(0, "person_id"));
                    #region ... rest of bulk copy

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(1, "source"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(2, "tag_id"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(3, "createShVer"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(4, "start_time"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(5, "end_time"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(6, "update_time"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(7, "create_time"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(8, "time_offset"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(9, "custom"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(10, "binning_data"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(11, "modify_shver"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(12, "client_data_id"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(13, "heart_rate"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(14, "heart_rate_max"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(15, "heart_rate_min"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(16, "heart_beat_count"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(17, "client_dataver"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(18, "comment"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(19, "package_name"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(20, "device_uuid"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(21, "data_uuid"));

                    #endregion

                    await bulkCopy.WriteToServerAsync(table);
                }


                int current = 0;
                await using var transaction = await sqlServerConnection.BeginTransactionAsync();
                int total = updates.Count();
                // Update the exisiting datas in tbl_HeartRate
                foreach (var item in updates)
                {
                    await using var command = new MySqlCommand(DatabaseCommands.GetHeartRateUpdateSql(),
                                                            sqlServerConnection, (MySqlTransaction)transaction);

                    command.Parameters.AddWithValue("@Source", (object?)item.Source ?? DBNull.Value);
                    #region ... rest of add with value

                    command.Parameters.AddWithValue("@TagId", (object?)item.TagId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CreateShVer", (object?)item.CreateShVer ?? DBNull.Value);
                    command.Parameters.AddWithValue("@StartTime", (object?)item.StartTime ?? DBNull.Value);
                    command.Parameters.AddWithValue("@EndTime", (object?)item.EndTime ?? DBNull.Value);
                    command.Parameters.AddWithValue("@UpdateTime", (object?)item.UpdateTime ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CreateTime", (object?)item.CreateTime ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TimeOffset", (object?)item.TimeOffset ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Custom", (object?)item.Custom ?? DBNull.Value);
                    command.Parameters.AddWithValue("@BinningData", (object?)item.BinningData ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ModifyShVer", (object?)item.ModifyShVer ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ClientDataId", (object?)item.ClientDataId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@HeartRate", (object?)item.HeartRate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@HeartRateMax", (object?)item.HeartRateMax ?? DBNull.Value);
                    command.Parameters.AddWithValue("@HeartRateMin", (object?)item.HeartRateMin ?? DBNull.Value);
                    command.Parameters.AddWithValue("@HeartBeatCount", (object?)item.HeartBeatCount ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ClientDataVer", (object?)item.ClientDataVer ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Comment", (object?)item.Comment ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PackageName", (object?)item.PackageName ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DeviceUuid", (object?)item.DeviceUuid ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DataUuid", item.DataUuid);

                    #endregion

                    await command.ExecuteNonQueryAsync();

                    current++;

                    if (current % 100 == 0)
                    {
                        double percentage = (double)current / total * 100;

                        progress?.Report((percentage, "Updated: "));
                    }
                }

                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                //await transaction.RollbackAsync();
                ErrorMessage = ex.Message;
                return false;
            }
        }


        /// <summary>
        /// Asynchronously retrieves a list of Samsung Health heart rate records for the specified person from the database.
        /// </summary>
        /// <param name="personId">The unique identifier of the person whose heart rate data is being fetched.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of <see cref="SamsungHeartRateModel"/> instances.</returns>
        public async Task<List<SamsungHeartRateModel>> GetSamsungHeartRateSqlAsync(int personId)
        {
            var result = new List<SamsungHeartRateModel>();

            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected)
                return result;

            try
            {
                var sqlCommand = new MySqlCommand(
                    DatabaseCommands.GetSamsungHeartRateSql(),
                    sqlServerConnection);

                sqlCommand.Parameters.AddWithValue("@PersonID", personId);

                await using var reader = await sqlCommand.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(new SamsungHeartRateModel
                    {
                        HeartRateID = reader.GetInt32("heart_rate_id"),
                        Source = reader.IsDBNull("source") ? null : reader.GetString("source"),
                        TagId = reader.IsDBNull("tag_id") ? null : reader.GetInt32("tag_id"),

                        CreateShVer = reader.IsDBNull("createShVer")
                            ? null
                            : reader.GetInt32("createShVer"),

                        StartTime = reader.IsDBNull("start_time")
                            ? null
                            : reader.GetDateTime("start_time"),

                        EndTime = reader.IsDBNull("end_time")
                            ? null
                            : reader.GetDateTime("end_time"),

                        UpdateTime = reader.IsDBNull("update_time")
                            ? null
                            : reader.GetDateTime("update_time"),

                        CreateTime = reader.IsDBNull("create_time")
                            ? null
                            : reader.GetDateTime("create_time"),

                        TimeOffset = reader.IsDBNull("time_offset")
                            ? null
                            : reader.GetString("time_offset"),

                        Custom = reader.IsDBNull("custom")
                            ? null
                            : reader.GetString("custom"),

                        BinningData = reader.IsDBNull("binning_data")
                            ? null
                            : reader.GetString("binning_data"),

                        ModifyShVer = reader.IsDBNull("modify_shver")
                            ? null
                            : reader.GetInt32("modify_shver"),

                        ClientDataId = reader.IsDBNull("client_data_id")
                            ? null
                            : reader.GetString("client_data_id"),

                        HeartRate = reader.IsDBNull("heart_rate")
                            ? null
                            : reader.GetDouble("heart_rate"),

                        HeartRateMax = reader.IsDBNull("heart_rate_max")
                            ? null
                            : reader.GetDouble("heart_rate_max"),

                        HeartRateMin = reader.IsDBNull("heart_rate_min")
                            ? null
                            : reader.GetDouble("heart_rate_min"),

                        HeartBeatCount = reader.IsDBNull("heart_beat_count")
                            ? null
                            : reader.GetInt32("heart_beat_count"),

                        ClientDataVer = reader.IsDBNull("client_dataver")
                            ? null
                            : reader.GetInt32("client_dataver"),

                        DeviceUuid = reader.IsDBNull("device_uuid")
                            ? null
                            : reader.GetString("device_uuid"),

                        Comment = reader.IsDBNull("comment")
                            ? null
                            : reader.GetString("comment"),

                        PackageName = reader.IsDBNull("package_name")
                            ? null
                            : reader.GetString("package_name"),

                        DataUuid = reader.IsDBNull("data_uuid")
                            ? null
                            : reader.GetString("data_uuid")
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Get Samsung Heart Rate Async: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves and returns daily step trend chart records for a specified person.
        /// </summary>
        /// <remarks>Establishes a database connection, executes the step trend command retrieved from <see cref="DatabaseCommands.GetSamsungStepTrendSpecificSql"/> with the given person identifier parameter, and reads the result set into a list of <see cref="SamsungStepTrendDashboardModel"/> instances.</remarks>
        /// <param name="personId">The unique identifier of the person whose step trends are being requested.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of <see cref="SamsungStepTrendDashboardModel"/> objects.</returns>
        public async Task<List<SamsungHeartRateDashboardModel>> GetHeartRateDashboardSqlAsync(int personId)
        {
            var result = new List<SamsungHeartRateDashboardModel>();

            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected)
                return result;

            try
            {
                await using var sqlCommand =
                    new MySqlCommand(
                        DatabaseCommands.GetSamsungHeartRateDashboardSql(),
                        sqlServerConnection);

                sqlCommand.Parameters.AddWithValue("@PersonID", personId);

                await using var reader =
                    await sqlCommand.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(new SamsungHeartRateDashboardModel
                    {
                        CreateTime = reader.GetDateTime("create_time"),

                        HeartRate = reader.IsDBNull("heart_rate")
                            ? null
                            : reader.GetFloat("heart_rate"),

                        MaxHeartRate = reader.IsDBNull("heart_rate_max")
                            ? null
                            : reader.GetFloat("heart_rate_max"),

                        MinHeartRate = reader.IsDBNull("heart_rate_min")
                            ? null
                            : reader.GetFloat("heart_rate_min"),
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Get Exercise Async: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Asynchronously deletes a specific heart rate record from the database using its unique data identifier.
        /// </summary>
        /// <param name="heartRateDataUuid">The unique data identifier (UUID) of the heart rate record to be removed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GetHeartRateDeleteSqlAsync(int heartRateID)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return;

            try
            {
                var sqlCommand = new MySqlCommand(DatabaseCommands.GetSamsungHeartRateDeleteSql(), sqlServerConnection);
                sqlCommand.Parameters.AddWithValue("@id", heartRateID);
                await sqlCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Delete Heart Rate Async: {ex.Message}";
            }
        }

        #endregion

        #region Samsug Health Step Trend

        /// <summary>
        /// Inserts a collection of daily step trend records for a specific person into the database.
        /// Utilizes a database transaction to ensure data integrity; performs a rollback in case of failure.
        /// </summary>
        /// <param name="PersonId">The unique identifier of the person to whom the data belongs.</param>
        /// <param name="stepDailyTrendList">A collection of <see cref="SamsungStepTrendModel"/> objects containing the step trend data to be stored.</param>
        /// <param name="progress">An optional progress indicator that reports the percentage of the insertion process.</param>
        /// <returns>Returns 'true' if the operation completes successfully.</returns>
        public async Task<bool> InsertStepTrendSqlAsync(int PersonId, IEnumerable<SamsungStepTrendModel> stepDailyTrendList, IProgress<double>? progress = null)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return false;

            await using var tx = await sqlServerConnection.BeginTransactionAsync();

            try
            {
                var trends = stepDailyTrendList.ToList();

                int total = trends.Count;
                int current = 0;

                foreach (var trend in trends)
                {
                    await using var sqlCommand = new MySqlCommand(DatabaseCommands.GetSamsungStepTrendUpsertSql(),
                                                                sqlServerConnection,
                                                                (MySqlTransaction)tx);

                    sqlCommand.Parameters.AddWithValue("@PersonID_FK", PersonId);
                    sqlCommand.Parameters.AddWithValue("@BinningData", trend.BinningData);
                    sqlCommand.Parameters.AddWithValue("@UpdateTime", trend.UpdateTime);
                    sqlCommand.Parameters.AddWithValue("@CreateTime", trend.CreateTime);
                    sqlCommand.Parameters.AddWithValue("@SourcePkgName", trend.SourcePkgName);
                    sqlCommand.Parameters.AddWithValue("@SourceType", trend.SourceType);
                    sqlCommand.Parameters.AddWithValue("@StepCount", trend.Count);
                    sqlCommand.Parameters.AddWithValue("@Speed", trend.Speed);
                    sqlCommand.Parameters.AddWithValue("@Distance", trend.Distance);
                    sqlCommand.Parameters.AddWithValue("@Calorie", trend.Calorie);
                    sqlCommand.Parameters.AddWithValue("@DeviceUuid", trend.DeviceUuid);
                    sqlCommand.Parameters.AddWithValue("@PkgName", trend.PkgName);
                    sqlCommand.Parameters.AddWithValue("@DataUuid", trend.DataUuid.ToString());
                    sqlCommand.Parameters.AddWithValue("@DayTime", trend.DayTime);

                    await sqlCommand.ExecuteNonQueryAsync();

                    current++;

                    progress?.Report((double)current / total * 100);
                }

                await tx.CommitAsync();

                progress?.Report(100);

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error inserting Samsung Health Daily Steps Trend: {ex.Message}";
                await tx.RollbackAsync();
            }

            return false;
        }

        /// <summary>
        /// Asynchronously synchronizes a collection of Samsung Health Step Trend records for a specific person into the database, 
        /// performing bulk insertions for new entries and transactional updates for existing records based on unique data identifiers and update timestamps.
        /// </summary>
        /// <param name="PersonId">The unique identifier of the person to whom the data belongs.</param>
        /// <param name="stepDailyTrendList">A collection of <see cref="SamsungStepTrendModel"/> objects containing the step trend data to be stored.</param>
        /// <param name="progress">An optional progress indicator that reports the percentage of the insertion process.</param>
        /// <returns>Returns 'true' if the operation completes successfully.</returns>
        public async Task<bool> UpsertStepTrendSqlAsync(int personId, ICollection<SamsungStepTrendModel> stepDailyTrendList,
                                                    IProgress<(double value, string prgressText)>? progress = null)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return false;

            try
            {

                var existingEntries = new Dictionary<string, DateTime?>();

                // Get the data_uuid and update_time from the exisiting heart rates in the databasse tbl_HeartRate 
                await using var cmd = new MySqlCommand(DatabaseCommands.GetSamsungStepTrendExistenceCheckSql(),
                                                        sqlServerConnection);

                cmd.Parameters.AddWithValue("@PersonId", personId);

                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        existingEntries.Add(
                            reader.GetString("data_uuid"),
                            reader.IsDBNull("update_at")
                                ? null
                                : reader.GetDateTime("update_at"));
                    }
                }
                var inserts = new List<SamsungStepTrendModel>();
                var updates = new List<SamsungStepTrendModel>();

                // Compare the stepTrendList List with the existitng values in the database tbl_StepTrend. If the value exist then we update the entrie.
                // Is the value new then we insert
                // it in the table with bulk inserting.
                foreach (var item in stepDailyTrendList)
                {
                    var uuid = item.DataUuid?.Trim();
                    if (string.IsNullOrWhiteSpace(item.DataUuid)) continue;

                    if (!existingEntries.TryGetValue(uuid, out var dbUpdateTime))
                    {
                        inserts.Add(item);
                        continue;
                    }

                    if (item.UpdateTime.HasValue && (!dbUpdateTime.HasValue || item.UpdateTime > dbUpdateTime))
                    {
                        updates.Add(item);
                    }
                }

                if (inserts.Count > 0)
                {
                    var table = SamsungHealthStepDailyTrendDataTable.CreateStepDailyTrendDataTable(personId, inserts);

                    var bulkCopy = new MySqlBulkCopy(sqlServerConnection)
                    {
                        DestinationTableName = "tbl_StepTrend",
                        NotifyAfter = 1000
                    };

                    bulkCopy.MySqlRowsCopied += (sender, e) =>
                    {
                        double percentage = (double)e.RowsCopied / inserts.Count * 100;

                        progress?.Report((percentage,
                            $"Inserted: "));
                    };

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(0, "person_id"));
                    #region ... rest of bulk copy

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(1, "binning_data"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(2, "update_at"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(3, "create_at"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(4, "source_package"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(5, "source_type"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(6, "step_count_current"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(7, "step_count_initial"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(8, "speed_current"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(9, "speed_initial"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(10, "distance_current"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(11, "distance_initial"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(12, "calories_current"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(13, "calories_initial"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(14, "device_uuid"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(15, "package_name"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(16, "data_uuid"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(17, "record_date"));

                    #endregion

                    await bulkCopy.WriteToServerAsync(table);
                }


                int current = 0;
                await using var transaction = await sqlServerConnection.BeginTransactionAsync();
                int total = updates.Count();
                // Update the exisiting datas in tbl_HeartRate
                foreach (var item in updates)
                {
                    await using var command = new MySqlCommand(DatabaseCommands.GetSamsungStepTrendUpdateSql(),
                                                            sqlServerConnection, (MySqlTransaction)transaction);

                    command.Parameters.AddWithValue("@PersonID_FK", personId);
                    #region ... rest of add with value

                    command.Parameters.AddWithValue("@BinningData", (object?)item.BinningData ?? DBNull.Value);
                    command.Parameters.AddWithValue("@UpdateTime", (object?)item.UpdateTime ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CreateTime", (object?)item.CreateTime ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SourcePkgName", (object?)item.SourcePkgName ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SourceType", (object?)item.SourceType ?? DBNull.Value);

                    command.Parameters.AddWithValue("@StepCount", (object?)item.Count ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Speed", (object?)item.Speed ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Distance", (object?)item.Distance ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Calorie", (object?)item.Calorie ?? DBNull.Value);

                    command.Parameters.AddWithValue("@DeviceUuid", (object?)item.DeviceUuid ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PkgName", (object?)item.PkgName ?? DBNull.Value);

                    command.Parameters.AddWithValue("@DataUuid", item.DataUuid);

                    #endregion

                    await command.ExecuteNonQueryAsync();

                    current++;

                    if (current % 100 == 0)
                    {
                        double percentage = (double)current / total * 100;

                        progress?.Report((percentage, "Updated: "));
                    }
                }

                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                //await transaction.RollbackAsync();
                ErrorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Asynchronously retrieves and returns Samsung step trend records for a specified person.
        /// </summary>
        /// <remarks>Establishes a database connection, executes the step trend command retrieved from <see cref="DatabaseCommands.GetSamsungStepTrendSql"/> with the given person identifier parameter, and reads the result set into a list of <see cref="SamsungStepTrendModel"/> instances.</remarks>
        /// <param name="personId">The unique identifier of the person whose step trends are being requested.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of <see cref="SamsungStepTrendModel"/> objects.</returns>
        public async Task<List<SamsungStepTrendModel>> GetStepTrendSqlAsync(int personId)
        {
            var result = new List<SamsungStepTrendModel>();

            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected)
                return result;

            try
            {
                var sqlCommand = new MySqlCommand(
                    DatabaseCommands.GetSamsungStepTrendSql(),
                    sqlServerConnection);

                sqlCommand.Parameters.AddWithValue("@PersonID", personId);

                await using var reader = await sqlCommand.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(new SamsungStepTrendModel
                    {
                        StepTrendID = reader.IsDBNull("step_trend_id")
                            ? null
                            : reader.GetInt32("step_trend_id"),

                        BinningData = reader.IsDBNull("binning_data")
                            ? string.Empty
                            : reader.GetString("binning_data"),

                        UpdateTime = reader.IsDBNull("update_at")
                            ? null
                            : reader.GetDateTime("update_at"),

                        CreateTime = reader.GetDateTime("create_at"),

                        SourcePkgName = reader.IsDBNull("source_package")
                            ? string.Empty
                            : reader.GetString("source_package"),

                        SourceType = reader.GetInt32("source_type"),

                        Count = reader.GetInt32("step_count_current"),

                        Speed = reader.GetDouble("speed_current"),

                        Distance = reader.GetDouble("distance_current"),

                        Calorie = reader.GetDouble("calories_current"),

                        DeviceUuid = reader.IsDBNull("device_uuid")
                            ? string.Empty
                            : reader.GetString("device_uuid"),

                        PkgName = reader.IsDBNull("package_name")
                            ? string.Empty
                            : reader.GetString("package_name"),

                        DataUuid = reader.IsDBNull("data_uuid")
                            ? null
                            : reader.GetString("data_uuid"),

                        DayTime = reader.GetDateTime("record_date")
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Get Step Trend Async: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves and returns daily step trend chart records for a specified person.
        /// </summary>
        /// <remarks>Establishes a database connection, executes the step trend command retrieved from <see cref="DatabaseCommands.GetSamsungStepTrendSpecificSql"/> with the given person identifier parameter, and reads the result set into a list of <see cref="SamsungStepTrendDashboardModel"/> instances.</remarks>
        /// <param name="personId">The unique identifier of the person whose step trends are being requested.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of <see cref="SamsungStepTrendDashboardModel"/> objects.</returns>
        public async Task<List<SamsungStepTrendDashboardModel>> GetStepTrendDashboardSqlAsync(int personId)
        {
            var result = new List<SamsungStepTrendDashboardModel>();

            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return result;

            try
            {
                var SqlCommand = new MySqlCommand(DatabaseCommands.GetSamsungStepTrendSpecificSql(), sqlServerConnection);

                SqlCommand.Parameters.AddWithValue("@PersonID", personId);

                await using var reader = await SqlCommand.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(new SamsungStepTrendDashboardModel
                    {
                        CreateTime = reader.GetDateTime("create_at"),
                        SourceType = reader.GetInt32("source_type"),
                        StepCount = reader.GetInt32("step_count_current"),
                        Distance = reader.GetDouble("distance_current"),
                        Calorie = reader.GetDouble("calories_current")
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Get Step Daily Trend Async: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Asynchronously deletes a specific step trend record from the database using its unique identifier.
        /// </summary>
        /// <param name="stepTrendID">The primary key (ID) of the step trend record to be removed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GetStepTrendDeleteSqlAsync(int stepTrendID)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return;

            try
            {
                var sqlCommand = new MySqlCommand(DatabaseCommands.GetSamsungStepTrendDeleteSql(), sqlServerConnection);
                sqlCommand.Parameters.AddWithValue("@id", stepTrendID);
                await sqlCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Delete Step Trend Async: {ex.Message}";
            }
        }

        #endregion

        #region Samsung Health Oxygen Saturation

        /// <summary>
        /// Inserts a collection of daily step trend records for a specific person into the database.
        /// Utilizes a database transaction to ensure data integrity; performs a rollback in case of failure.
        /// </summary>
        /// <param name="PersonId">The unique identifier of the person to whom the data belongs.</param>
        /// <param name="stepDailyTrendList">A collection of <see cref="SamsungOxygenSaturationModel"/> objects containing the step trend data to be stored.</param>
        /// <param name="progress">An optional progress indicator that reports the percentage of the insertion process.</param>
        /// <returns>Returns 'true' if the operation completes successfully.</returns>
        public async Task<bool> InsertSpO2SqlAsync(int PersonId, IEnumerable<SamsungOxygenSaturationModel> oxygenSaturation,
                                                         IProgress<(double value, string prgressText)>? progress = null)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return false;

            await using var tx = await sqlServerConnection.BeginTransactionAsync();

            try
            {
                var trends = oxygenSaturation.ToList();

                int total = trends.Count;
                int current = 0;

                foreach (var trend in trends)
                {
                    await using var sqlCommand = new MySqlCommand(DatabaseCommands.GetSamsungOxygenSaturationUpsertSql(),
                                                                sqlServerConnection,
                                                                (MySqlTransaction)tx);

                    sqlCommand.Parameters.AddWithValue("@PersonId", PersonId);

                    sqlCommand.Parameters.AddWithValue("@IntegratedId", (object?)trend.IntegratedId ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@ClientDataId", (object?)trend.ClientDataId ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@TagId", (object?)trend.TagId ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@StartTime", (object?)trend.StartTime ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@EndTime", (object?)trend.EndTime ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@UpdateTime", (object?)trend.UpdateTime ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@CreateTime", (object?)trend.CreateTime ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@TimeOffset", (object?)trend.TimeOffset ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@Custom", (object?)trend.Custom ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@Spo2", (object?)trend.SpO2 ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@Spo2Max", (object?)trend.SpO2Max ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@Spo2Min", (object?)trend.SpO2Min ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@LowSpo2Duration", (object?)trend.LowSpO2Duration ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@CoverageRate", (object?)trend.SpO2CoverageRate ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@HeartRate", (object?)trend.HeartRate ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@Comment", (object?)trend.Comment ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@DataUuid", (object?)trend.DataUuid ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@DeviceUuid", (object?)trend.DeviceUuid ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@PackageName", (object?)trend.PackageName ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@CreateShVer", (object?)trend.CreateShVer ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@ModifyShVer", (object?)trend.ModifyShVer ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@ClientDataVer", (object?)trend.ClientDataVer ?? DBNull.Value);

                    sqlCommand.Parameters.AddWithValue("@Source", (object?)trend.Source ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@BinningData", (object?)trend.Binning ?? DBNull.Value);

                    await sqlCommand.ExecuteNonQueryAsync();

                    current++;

                    double percentage = (double)current / total * 100;

                    progress?.Report((percentage, "Uploaded: "));

                }

                await tx.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error inserting Samsung Datas: {ex.Message}";
                await tx.RollbackAsync();
            }

            return false;
        }

        /// <summary>
        /// Asynchronously synchronizes a collection of Samsung Health Step Trend records for a specific person into the database, 
        /// performing bulk insertions for new entries and transactional updates for existing records based on unique data identifiers and update timestamps.
        /// </summary>
        /// <param name="PersonId">The unique identifier of the person to whom the data belongs.</param>
        /// <param name="stepDailyTrendList">A collection of <see cref="SamsungStepTrendModel"/> objects containing the step trend data to be stored.</param>
        /// <param name="progress">An optional progress indicator that reports the percentage of the insertion process.</param>
        /// <returns>Returns 'true' if the operation completes successfully.</returns>
        public async Task<bool> UpsertSpO2SqlAsync(int personId, ICollection<SamsungOxygenSaturationModel> oxygenSaturation,
                                                    IProgress<(double value, string prgressText)>? progress = null)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return false;

            try
            {

                var existingEntries = new Dictionary<string, DateTime?>();

                // Get the data_uuid and update_time or update_at from the exisiting datas in the databasse table 
                await using var cmd = new MySqlCommand(DatabaseCommands.GetSamsungOxygenSaturationExistenceCheckSql(),
                                                        sqlServerConnection);

                cmd.Parameters.AddWithValue("@PersonId", personId);

                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        existingEntries.Add(
                            reader.GetString("data_uuid"),
                            reader.IsDBNull("update_time")
                                ? null
                                : reader.GetDateTime("update_time"));
                    }
                }
                var inserts = new List<SamsungOxygenSaturationModel>();
                var updates = new List<SamsungOxygenSaturationModel>();

                // Compare the current List with the existitng values in the database.
                // If the value exist then we update the entrie.
                // Is the value new then we insert it in the table with bulk inserting.
                foreach (var item in oxygenSaturation)
                {
                    var uuid = item.DataUuid?.Trim();
                    if (string.IsNullOrWhiteSpace(item.DataUuid)) continue;

                    if (!existingEntries.TryGetValue(uuid, out var dbUpdateTime))
                    {
                        inserts.Add(item);
                        continue;
                    }

                    if (item.UpdateTime.HasValue && (!dbUpdateTime.HasValue || item.UpdateTime > dbUpdateTime))
                    {
                        updates.Add(item);
                    }
                }

                if (inserts.Count > 0)
                {
                    var table = SamsungHealthOxygenDataTable.CreateOxygenDataTable(personId, inserts);

                    var bulkCopy = new MySqlBulkCopy(sqlServerConnection)
                    {
                        DestinationTableName = "tbl_OxygenSaturation",
                        NotifyAfter = 10
                    };

                    bulkCopy.MySqlRowsCopied += (sender, e) =>
                    {
                        double percentage = (double)e.RowsCopied / inserts.Count * 100;

                        progress?.Report((percentage, $"Inserted: "));
                    };

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(0, "person_id"));
                    #region ... rest of bulk copy

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(1, "integrated_id"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(2, "client_data_id"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(3, "tag_id"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(4, "start_time"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(5, "end_time"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(6, "update_time"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(7, "create_time"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(8, "time_offset"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(9, "custom"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(10, "spo2"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(11, "spo2_max"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(12, "spo2_min"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(13, "low_spo2duration"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(14, "coverage_rate"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(15, "heart_rate"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(16, "comment"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(17, "data_uuid"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(18, "device_uuid"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(19, "package_name"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(20, "createShVer"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(21, "modify_shver"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(22, "client_dataver"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(23, "source"));

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(24, "binning_data"));

                    #endregion

                    await bulkCopy.WriteToServerAsync(table);
                }


                int current = 0;
                await using var transaction = await sqlServerConnection.BeginTransactionAsync();
                int total = updates.Count();

                // Update the exisiting datas in database table
                foreach (var item in updates)
                {
                    await using var command = new MySqlCommand(DatabaseCommands.GetSamsungOxygenSaturationUpdateSql(),
                                                            sqlServerConnection, (MySqlTransaction)transaction);

                    command.Parameters.AddWithValue("@PersonId", personId);

                    #region ... rest of add with value

                    command.Parameters.AddWithValue("@IntegratedId", (object?)item.IntegratedId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ClientDataId", (object?)item.ClientDataId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TagId", (object?)item.TagId ?? DBNull.Value);

                    command.Parameters.AddWithValue("@StartTime", (object?)item.StartTime ?? DBNull.Value);
                    command.Parameters.AddWithValue("@EndTime", (object?)item.EndTime ?? DBNull.Value);
                    command.Parameters.AddWithValue("@UpdateTime", (object?)item.UpdateTime ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CreateTime", (object?)item.CreateTime ?? DBNull.Value);

                    command.Parameters.AddWithValue("@TimeOffset", (object?)item.TimeOffset ?? DBNull.Value);

                    command.Parameters.AddWithValue("@Custom", (object?)item.Custom ?? DBNull.Value);

                    command.Parameters.AddWithValue("@Spo2", (object?)item.SpO2 ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Spo2Max", (object?)item.SpO2Max ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Spo2Min", (object?)item.SpO2Min ?? DBNull.Value);

                    command.Parameters.AddWithValue("@LowSpo2Duration", (object?)item.LowSpO2Duration ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CoverageRate", (object?)item.SpO2CoverageRate ?? DBNull.Value);

                    command.Parameters.AddWithValue("@HeartRate", (object?)item.HeartRate ?? DBNull.Value);

                    command.Parameters.AddWithValue("@Comment", (object?)item.Comment ?? DBNull.Value);

                    command.Parameters.AddWithValue("@DataUuid", (object?)item.DataUuid ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DeviceUuid", (object?)item.DeviceUuid ?? DBNull.Value);

                    command.Parameters.AddWithValue("@PackageName", (object?)item.PackageName ?? DBNull.Value);

                    command.Parameters.AddWithValue("@CreateShVer", (object?)item.CreateShVer ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ModifyShVer", (object?)item.ModifyShVer ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ClientDataVer", (object?)item.ClientDataVer ?? DBNull.Value);

                    command.Parameters.AddWithValue("@Source", (object?)item.Source ?? DBNull.Value);
                    command.Parameters.AddWithValue("@BinningData", (object?)item.Binning ?? DBNull.Value);

                    #endregion

                    await command.ExecuteNonQueryAsync();

                    current++;

                    if (current % 100 == 0)
                    {
                        double percentage = (double)current / total * 100;

                        progress?.Report((percentage, "Updated: "));
                    }
                }

                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                //await transaction.RollbackAsync();
                ErrorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Asynchronously retrieves a list of Samsung Health oxygen saturation records for the specified person.
        /// </summary>
        /// <param name="personId">The unique identifier of the person.</param>
        /// <returns>A list of <see cref="SamsungOxygenSaturationModel"/>.</returns>
        public async Task<List<SamsungOxygenSaturationModel>> GetSpO2SqlAsync(int personId)
        {
            var result = new List<SamsungOxygenSaturationModel>();

            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected)
                return result;

            try
            {
                var sqlCommand = new MySqlCommand(
                    DatabaseCommands.GetSamsungOxygenSaturationSql(),
                    sqlServerConnection);

                sqlCommand.Parameters.AddWithValue("@PersonID", personId);

                await using var reader = await sqlCommand.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(new SamsungOxygenSaturationModel
                    {
                        OxygenSaturationID = reader.GetInt32("oxygen_saturation_id"),

                        IntegratedId = reader.IsDBNull("integrated_id")
                            ? null
                            : reader.GetString("integrated_id"),

                        ClientDataId = reader.IsDBNull("client_data_id")
                            ? null
                            : reader.GetString("client_data_id"),

                        TagId = reader.IsDBNull("tag_id")
                            ? null
                            : reader.GetInt32("tag_id"),

                        StartTime = reader.IsDBNull("start_time")
                            ? null
                            : reader.GetDateTime("start_time"),

                        EndTime = reader.IsDBNull("end_time")
                            ? null
                            : reader.GetDateTime("end_time"),

                        UpdateTime = reader.IsDBNull("update_time")
                            ? null
                            : reader.GetDateTime("update_time"),

                        CreateTime = reader.IsDBNull("create_time")
                            ? null
                            : reader.GetDateTime("create_time"),

                        TimeOffset = reader.IsDBNull("time_offset")
                            ? null
                            : reader.GetString("time_offset"),

                        Custom = reader.IsDBNull("custom")
                            ? null
                            : reader.GetString("custom"),

                        SpO2 = reader.IsDBNull("spo2")
                            ? null
                            : reader.GetDouble("spo2"),

                        SpO2Max = reader.IsDBNull("spo2_max")
                            ? null
                            : reader.GetDouble("spo2_max"),

                        SpO2Min = reader.IsDBNull("spo2_min")
                            ? null
                            : reader.GetDouble("spo2_min"),

                        LowSpO2Duration = reader.IsDBNull("low_spo2duration")
                            ? null
                            : reader.GetInt32("low_spo2duration"),

                        SpO2CoverageRate = reader.IsDBNull("coverage_rate")
                            ? null
                            : Convert.ToDouble(reader.GetValue("coverage_rate")),

                        HeartRate = reader.IsDBNull("heart_rate")
                            ? null
                            : reader.GetDouble("heart_rate"),

                        Comment = reader.IsDBNull("comment")
                            ? null
                            : reader.GetString("comment"),

                        DataUuid = reader.IsDBNull("data_uuid")
                            ? null
                            : reader.GetString("data_uuid"),

                        DeviceUuid = reader.IsDBNull("device_uuid")
                            ? null
                            : reader.GetString("device_uuid"),

                        PackageName = reader.IsDBNull("package_name")
                            ? null
                            : reader.GetString("package_name"),

                        CreateShVer = reader.IsDBNull("createShVer")
                            ? null
                            : reader.GetInt32("createShVer"),

                        ModifyShVer = reader.IsDBNull("modify_shver")
                            ? null
                            : reader.GetInt32("modify_shver"),

                        ClientDataVer = reader.IsDBNull("client_dataver")
                            ? null
                            : reader.GetInt32("client_dataver"),

                        Source = reader.IsDBNull("source")
                            ? null
                            : reader.GetString("source"),

                        Binning = reader.IsDBNull("binning_data")
                            ? null
                            : reader.GetString("binning_data")
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Get Samsung Oxygen Saturation Async: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves and returns oxygen saturation dashboard records for a specified person.
        /// </summary>
        /// <remarks>Establishes a database connection, executes the oxygen saturation command retrieved from <see cref="DatabaseCommands.GetSamsungOxygenSaturationDashboardSql"/> with the given person identifier parameter, and reads the result set into a list of <see cref="SamsungOxygenSaturationDashboardModel"/> instances.</remarks>
        /// <param name="personId">The unique identifier of the person whose oxygen saturation data is being requested.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of <see cref="SamsungOxygenSaturationDashboardModel"/> objects.</returns>
        public async Task<List<SamsungOxygenSaturationDashboardModel>> GetSpO2DashboardSqlAsync(int personId)
        {
            var result = new List<SamsungOxygenSaturationDashboardModel>();

            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected)
                return result;

            try
            {
                await using var sqlCommand =
                    new MySqlCommand(
                        DatabaseCommands.GetSamsungOxygenSaturationDashboardSql(),
                        sqlServerConnection);

                sqlCommand.Parameters.AddWithValue("@PersonID", personId);

                await using var reader =
                    await sqlCommand.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(new SamsungOxygenSaturationDashboardModel
                    {
                        StartTime = reader.GetDateTime("start_time"),

                        SpO2 = reader.IsDBNull("spo2")
                            ? null
                            : reader.GetDouble("spo2"),

                        MaxSpO2 = reader.IsDBNull("spo2_max")
                            ? null
                            : reader.GetDouble("spo2_max"),

                        MinSpO2 = reader.IsDBNull("spo2_min")
                            ? null
                            : reader.GetDouble("spo2_min"),

                        LowSpO2Duration = reader.IsDBNull("low_spo2duration")
                            ? null
                            : reader.GetInt32("low_spo2duration"),

                        CoverageRate = reader.IsDBNull("coverage_rate")
                            ? null
                            : reader.GetInt32("coverage_rate"),
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Get SpO2 Async: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Asynchronously deletes a specific heart rate record from the database using its unique data identifier.
        /// </summary>
        /// <param name="heartRateDataUuid">The unique data identifier (UUID) of the heart rate record to be removed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GetSpO2DeleteSqlAsync(int oxygenSaturationID)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return;

            try
            {
                var sqlCommand = new MySqlCommand(DatabaseCommands.GetOxygenSaturationDeleteSql(), sqlServerConnection);
                sqlCommand.Parameters.AddWithValue("@id", oxygenSaturationID);
                await sqlCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Delete SpO2 Async: {ex.Message}";
            }
        }

        #endregion


        #region Workout Log

        /// <summary>
        /// Inserts Heavy App workout data into the database.
        /// Existing records are skipped based on DataUuid.
        /// </summary>
        /// <param name="personId">ID of the selected person.</param>
        /// <param name="wokoutDatas">Heavy App workout data.</param>
        /// <param name="progress">Optional progress reporting.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> UpsertWorkoutLogAsync(int personId,
            IEnumerable<HeavyAppCSVModel> wokoutDatas, IProgress<(double value, string prgressText)>? progress = null)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return false;
       

            try
            {

                var dbEntries = new Dictionary<string,
                    (
                        int WorkoutId,
                        string Hash,
                        DateTime StartTime
                    )>();
                var csvEntries = new Dictionary<string, HeavyAppCSVModel>();

                // Create a dictionary for the CSV file using the data GUID
                foreach (var workout in wokoutDatas)
                {
                    csvEntries[workout.DataUuid] = workout;
                }

                await using var cmd = new MySqlCommand(DatabaseCommands.GetWorkoutLogUuidSql(),
                                                        sqlServerConnection);

                cmd.Parameters.AddWithValue("@PersonId", personId);

                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var id = reader.GetInt32("exercise_log_id");
                        var hash = reader.GetString("uuid");
                        var start = reader.GetDateTime("start_time");

                        dbEntries[hash] = (id, hash, start);
                    }
                }

                var insertList = new List<HeavyAppCSVModel>();
                var updateList = new List<(int WorkoutId, string Hash, HeavyAppCSVModel Workout)>();
                var deleteList = new List<int>();


                // Retrieve entries for updating and inserting
                foreach (var csvItem in csvEntries)
                {
                    var hash = csvItem.Key;
                    var workout = csvItem.Value;

                    if (dbEntries.ContainsKey(hash)) continue;
                    
                    var sameStartDate = dbEntries.Values
                        .FirstOrDefault(x => x.StartTime == workout.StartTime);

                    if (sameStartDate.WorkoutId > 0 && !dbEntries.ContainsKey(hash))
                    {
                        updateList.Add(
                        (
                            sameStartDate.WorkoutId,
                            hash,
                            workout
                        ));
                    }


                    else
                    {
                        insertList.Add(workout);
                    }
                }

                // Retrieve entries for deleting
                foreach (var dbEntry in dbEntries.Values)
                {
                    if (!csvEntries.ContainsKey(dbEntry.Hash))
                    {
                        deleteList.Add(dbEntry.WorkoutId);
                    }
                }


                var workouts = wokoutDatas.ToList();

                int total = insertList.Count + updateList.Count + deleteList.Count;
                int current = 0;
                double percentage = 0;
                
                await using var tx = await sqlServerConnection.BeginTransactionAsync();

                foreach (var workout in insertList)
                {
                    await using var sqlCommand = new MySqlCommand(
                        DatabaseCommands.GetWorkoutLogInsertSql(),
                        sqlServerConnection,
                        (MySqlTransaction)tx);

                    sqlCommand.Parameters.AddWithValue("@PersonID_FK", personId);
                    sqlCommand.Parameters.AddWithValue("@Uuid", workout.DataUuid);

                    sqlCommand.Parameters.AddWithValue("@Title", workout.Title);
                    sqlCommand.Parameters.AddWithValue("@StartTime", workout.StartTime);
                    sqlCommand.Parameters.AddWithValue("@EndTime", workout.EndTime);
                    sqlCommand.Parameters.AddWithValue("@Description", (object?)workout.Description ?? string.Empty);
                    sqlCommand.Parameters.AddWithValue("@ExerciseTitle", workout.ExerciseTitle);
                    sqlCommand.Parameters.AddWithValue("@SupersetId", (object?)workout.SupersetId ?? string.Empty);
                    sqlCommand.Parameters.AddWithValue("@ExerciseNotes", (object?)workout.ExerciseNotes ?? string.Empty);
                    sqlCommand.Parameters.AddWithValue("@SetIndex", workout.SetIndex);
                    sqlCommand.Parameters.AddWithValue("@SetType", (object?)workout.SetType ?? string.Empty);
                    sqlCommand.Parameters.AddWithValue("@WeightKg", (object?)workout.WeightKg ?? 0.0);
                    sqlCommand.Parameters.AddWithValue("@Reps", (object?)workout.Reps ?? 0);
                    sqlCommand.Parameters.AddWithValue("@DistanceKm", (object?)workout.DistanceKm ?? 0.0);
                    sqlCommand.Parameters.AddWithValue("@DurationSeconds", (object?)workout.DurationSeconds ?? 0);
                    sqlCommand.Parameters.AddWithValue("@Rpe", (object?)workout.Rpe ?? 0);

                    await sqlCommand.ExecuteNonQueryAsync();

                    current++;
                   
                    percentage = (double)current / total * 100;

                    progress?.Report((percentage, "Inserted: "));
                   
                }

                foreach (var item in updateList)
                {
                    var workout = item.Workout;

                    await using var sqlCommand = new MySqlCommand(
                        DatabaseCommands.GetWorkoutLogUpdateByWorkoutIdSql(),
                        sqlServerConnection,
                        (MySqlTransaction)tx);

                    sqlCommand.Parameters.AddWithValue("@WorkoutId", item.WorkoutId);
                    sqlCommand.Parameters.AddWithValue("@Uuid", item.Hash);

                    sqlCommand.Parameters.AddWithValue("@Title", workout.Title);
                    sqlCommand.Parameters.AddWithValue("@StartTime", workout.StartTime);
                    sqlCommand.Parameters.AddWithValue("@EndTime", workout.EndTime);
                    sqlCommand.Parameters.AddWithValue("@Description", (object?)workout.Description ?? string.Empty);
                    sqlCommand.Parameters.AddWithValue("@ExerciseTitle", workout.ExerciseTitle);
                    sqlCommand.Parameters.AddWithValue("@SupersetId", (object?)workout.SupersetId ?? string.Empty);
                    sqlCommand.Parameters.AddWithValue("@ExerciseNotes", (object?)workout.ExerciseNotes ?? string.Empty);
                    sqlCommand.Parameters.AddWithValue("@SetIndex", workout.SetIndex);
                    sqlCommand.Parameters.AddWithValue("@SetType", (object?)workout.SetType ?? string.Empty);
                    sqlCommand.Parameters.AddWithValue("@WeightKg", (object?)workout.WeightKg ?? 0.0);
                    sqlCommand.Parameters.AddWithValue("@Reps", (object?)workout.Reps ?? 0);
                    sqlCommand.Parameters.AddWithValue("@DistanceKm", (object?)workout.DistanceKm ?? 0.0);
                    sqlCommand.Parameters.AddWithValue("@DurationSeconds", (object?)workout.DurationSeconds ?? 0);
                    sqlCommand.Parameters.AddWithValue("@Rpe", (object?)workout.Rpe ?? 0);

                    await sqlCommand.ExecuteNonQueryAsync();

                    current++;
                    percentage = (double)current / total * 100;

                    progress?.Report((percentage, "Updated: "));
                }

                

                foreach (var workoutId in deleteList)
                {
                    await using var sqlCommand = new MySqlCommand(
                        DatabaseCommands.GetWorkoutLogDeleteByWorkoutIdSql(),
                        sqlServerConnection,
                        (MySqlTransaction)tx);

                    sqlCommand.Parameters.AddWithValue("@WorkoutId", workoutId);

                    await sqlCommand.ExecuteNonQueryAsync();

                    current++;
                    percentage = (double)current / total * 100;

                    progress?.Report((percentage, "Deleted: "));
                }

                await tx.CommitAsync();

                percentage = 100;

                progress?.Report((percentage, "Finished: "));

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error inserting Heavy App workout data: {ex.Message}";
                //await tx.RollbackAsync();

            }

            return false;
        }



        /// <summary>
        /// Inserts Heavy App workout data into the database.
        /// Existing records are skipped based on DataUuid.
        /// </summary>
        /// <param name="personId">ID of the selected person.</param>
        /// <param name="heavyAppData">Heavy App workout data.</param>
        /// <param name="progress">Optional progress reporting.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> AddWorkoutLogAsync(int personId,
                                                   IEnumerable<HeavyAppCSVModel> heavyAppData,
                                                   IProgress<double>? progress = null)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return false;

            await using var tx = await sqlServerConnection.BeginTransactionAsync();

            try
            {
                var workouts = heavyAppData.ToList();

                int total = workouts.Count;
                int current = 0;

                foreach (var workout in workouts)
                {
                    await using var sqlCommand = new MySqlCommand(DatabaseCommands.GetWorkoutLogUpsertSql(),
                                                                  sqlServerConnection,
                                                                  (MySqlTransaction)tx);

                    sqlCommand.Parameters.AddWithValue("@PersonID_FK", personId);
                    //sqlCommand.Parameters.AddWithValue("@DataUuid", workout.DataUuid.ToString());
                    sqlCommand.Parameters.AddWithValue("@Title", workout.Title);
                    sqlCommand.Parameters.AddWithValue("@StartTime", workout.StartTime);
                    sqlCommand.Parameters.AddWithValue("@EndTime", workout.EndTime);
                    sqlCommand.Parameters.AddWithValue("@Description", (object?)workout.Description ?? string.Empty);
                    sqlCommand.Parameters.AddWithValue("@ExerciseTitle", workout.ExerciseTitle);
                    sqlCommand.Parameters.AddWithValue("@SupersetId", (object?)workout.SupersetId ?? string.Empty);
                    sqlCommand.Parameters.AddWithValue("@ExerciseNotes", (object?)workout.ExerciseNotes ?? string.Empty);
                    sqlCommand.Parameters.AddWithValue("@SetIndex", workout.SetIndex);
                    sqlCommand.Parameters.AddWithValue("@SetType", (object?)workout.SetType ?? string.Empty);
                    sqlCommand.Parameters.AddWithValue("@WeightKg", (object?)workout.WeightKg ?? 0.0);
                    sqlCommand.Parameters.AddWithValue("@Reps", (object?)workout.Reps ?? 0);
                    sqlCommand.Parameters.AddWithValue("@DistanceKm", (object?)workout.DistanceKm ?? 0.0);
                    sqlCommand.Parameters.AddWithValue("@DurationSeconds", (object?)workout.DurationSeconds ?? 0);
                    sqlCommand.Parameters.AddWithValue("@Rpe", (object?)workout.Rpe ?? 0);
                    await sqlCommand.ExecuteNonQueryAsync();

                    current++;

                    progress?.Report((double)current / total * 100.0);
                }

                await tx.CommitAsync();

                progress?.Report(100);

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error inserting Heavy App workout data: {ex.Message}";
                await tx.RollbackAsync();

            }

            return false;
        }


        /// <summary>
        /// Asynchronously synchronizes a collection of Samsung workout records for a specific person into the database, 
        /// performing bulk insertions for new entries and transactional updates for existing records based on unique data identifiers and update timestamps.
        /// </summary>
        /// <param name="personId">ID of the selected person.</param>
        /// <param name="heavyAppData">Heavy App workout data.</param>
        /// <param name="progress">Optional progress reporting.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> SyncWorkoutLogAsync(int personId, ICollection<HeavyAppCSVModel> wokoutDatas,
                                                    IProgress<(double value, string prgressText)>? progress = null)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return false;

            try
            {

                var dbEntries = new Dictionary<string,
                     (
                         int WorkoutId,
                         string Hash,
                         DateTime StartTime
                     )>();
                var csvEntries = new Dictionary<string, HeavyAppCSVModel>();

                // Create a dictionary for the CSV file using the data GUID
                foreach (var workout in wokoutDatas)
                {
                    csvEntries[workout.DataUuid] = workout;
                }

                await using var cmd = new MySqlCommand(DatabaseCommands.GetWorkoutLogUuidSql(),
                                                        sqlServerConnection);

                cmd.Parameters.AddWithValue("@PersonId", personId);

                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var id = reader.GetInt32("exercise_log_id");
                        var hash = reader.GetString("uuid");
                        var start = reader.GetDateTime("start_time");

                        dbEntries[hash] = (id, hash, start);
                    }
                }



                var insertList = new List<HeavyAppCSVModel>();
                var updateList = new List<(int WorkoutId, string Hash, HeavyAppCSVModel Workout)>();
                var deleteList = new List<int>();


                    // Retrieve entries for updating and inserting
                    foreach (var csvItem in csvEntries)
                    {
                        var hash = csvItem.Key;
                        var workout = csvItem.Value;

                        if (dbEntries.ContainsKey(hash)) continue;

                        var sameStartDate = dbEntries.Values
                            .FirstOrDefault(x => x.StartTime == workout.StartTime);

                            insertList.Add(workout);
                    }

                    // Retrieve entries for deleting
                    foreach (var dbEntry in dbEntries.Values)
                    {
                        if (!csvEntries.ContainsKey(dbEntry.Hash))
                        {
                            deleteList.Add(dbEntry.WorkoutId);
                        }
                    }

                int total = insertList.Count + updateList.Count + deleteList.Count;
                int current = 0;
                double percentage = 0;


                Debug.WriteLine("================");
                Debug.WriteLine($"Insert Table: {insertList.Count} | Update Table: {updateList.Count} | Delete Tabel: {deleteList.Count}");
                Debug.WriteLine("================");


                if (insertList.Count > 0)
                {
                    var table = HeavyAppDataTable.CreateHeavyAppDataTable(personId, insertList);

                    if (table.Rows.Count <= 0) Debug.WriteLine("table entries");
                    var bulkCopy = new MySqlBulkCopy(sqlServerConnection)
                    {
                        DestinationTableName = "tbl_WorkoutLog",
                        NotifyAfter = 1000
                    };

                    bulkCopy.MySqlRowsCopied += (sender, e) =>
                    {
                        percentage = ((double)e.RowsCopied / total) * 100;

                        progress?.Report((percentage, $"Inserted: "));
                    };

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(0, "person_id"));

                    #region ... rest of the bulk copy 

                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(1, "workout_title"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(2, "start_time"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(3, "end_time"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(4, "update_at"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(5, "description"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(6, "exercise_title"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(7, "superset_id"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(8, "exercise_notes"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(9, "set_index"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(10, "set_type"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(11, "weight"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(12, "reps"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(13, "distance"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(14, "duration_sec"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(15, "rpe"));
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(16, "uuid"));

                    #endregion


                    await bulkCopy.WriteToServerAsync(table);
                }

                await using var tx = await sqlServerConnection.BeginTransactionAsync();


                foreach (var item in updateList)
                {
                    var workout = item.Workout;

                    await using var sqlCommand = new MySqlCommand(
                        DatabaseCommands.GetWorkoutLogUpdateByWorkoutIdSql(),
                        sqlServerConnection,
                        (MySqlTransaction)tx);

                    sqlCommand.Parameters.AddWithValue("@WorkoutId", item.WorkoutId);
                    sqlCommand.Parameters.AddWithValue("@Uuid", item.Hash);

                    sqlCommand.Parameters.AddWithValue("@Title", workout.Title);
                    sqlCommand.Parameters.AddWithValue("@StartTime", workout.StartTime);
                    sqlCommand.Parameters.AddWithValue("@EndTime", workout.EndTime);
                    sqlCommand.Parameters.AddWithValue("@Description", (object?)workout.Description ?? string.Empty);
                    sqlCommand.Parameters.AddWithValue("@ExerciseTitle", workout.ExerciseTitle);
                    sqlCommand.Parameters.AddWithValue("@SupersetId", (object?)workout.SupersetId ?? string.Empty);
                    sqlCommand.Parameters.AddWithValue("@ExerciseNotes", (object?)workout.ExerciseNotes ?? string.Empty);
                    sqlCommand.Parameters.AddWithValue("@SetIndex", workout.SetIndex);
                    sqlCommand.Parameters.AddWithValue("@SetType", (object?)workout.SetType ?? string.Empty);
                    sqlCommand.Parameters.AddWithValue("@WeightKg", (object?)workout.WeightKg ?? 0.0);
                    sqlCommand.Parameters.AddWithValue("@Reps", (object?)workout.Reps ?? 0);
                    sqlCommand.Parameters.AddWithValue("@DistanceKm", (object?)workout.DistanceKm ?? 0.0);
                    sqlCommand.Parameters.AddWithValue("@DurationSeconds", (object?)workout.DurationSeconds ?? 0);
                    sqlCommand.Parameters.AddWithValue("@Rpe", (object?)workout.Rpe ?? 0);

                    await sqlCommand.ExecuteNonQueryAsync();

                    current++;
                    percentage = (double)current / total * 100;

                    progress?.Report((percentage, "Updated: "));
                }



                foreach (var workoutId in deleteList)
                {

                    await using var sqlCommand = new MySqlCommand(
                        DatabaseCommands.GetWorkoutLogDeleteByWorkoutIdSql(),
                        sqlServerConnection,
                        (MySqlTransaction)tx);

                    sqlCommand.Parameters.AddWithValue("@WorkoutId", workoutId);

                    await sqlCommand.ExecuteNonQueryAsync();

                    current++;
                    percentage = (double)current / total * 100;

                    progress?.Report((percentage, "Deleted: "));
                }

                await tx.CommitAsync();

                percentage = 100;

                progress?.Report((percentage, "Finished: "));

                return true;
            }
            catch (Exception ex)
            {
                //await transaction.RollbackAsync();
                if(ex.InnerException != null) ErrorMessage = ex.InnerException.Message;
                ErrorMessage = ex.Message;
                    return false;
            }
        }

        /// <summary>
        /// Retrieves all gym workout entries for a specific person.
        /// </summary>
        /// <param name="personId">
        /// The ID of the person whose workout entries should be loaded.
        /// </param>
        /// <returns>
        /// A collection of <see cref="GymWorkoutEntryModel"/> objects.
        /// </returns>
        public async Task<List<GymWorkoutEntryModel>> GetHeavyAppWorkoutsAsync(int personId)
        {
            var list = new List<GymWorkoutEntryModel>();

            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return list;
            try
            {

                await using var SqlCommand = new MySqlCommand(DatabaseCommands.GetWorkoutEntriesSpecificSql(), sqlServerConnection);

                SqlCommand.Parameters.AddWithValue("@PersonID", personId);

                await using var sqlDataReader = await SqlCommand.ExecuteReaderAsync();

                while (await sqlDataReader.ReadAsync())
                {
                    var entry = new GymWorkoutEntryModel
                    {
                        ExcerciseDate = sqlDataReader.IsDBNull(0) ? DateTime.MinValue : sqlDataReader.GetDateTime(0),
                        ExerciseName = sqlDataReader.IsDBNull(2) ? string.Empty : sqlDataReader.GetString(2),
                        Weight = 0.0,
                        Reps = 0.0,
                        SetIndex = 0
                    };

                    if (!sqlDataReader.IsDBNull(3))
                    {
                        var obj = sqlDataReader.GetValue(3);
                        entry.Weight = obj == null || obj is DBNull ? 0.0 : Convert.ToDouble(obj);
                    }

                    if (!sqlDataReader.IsDBNull(4))
                    {
                        var obj = sqlDataReader.GetValue(4);
                        entry.Reps = obj == null || obj is DBNull ? 0.0 : Convert.ToDouble(obj);
                    }

                    if (!sqlDataReader.IsDBNull(8))
                    {
                        var obj = sqlDataReader.GetValue(8);
                        entry.SetIndex = obj == null || obj is DBNull ? 0 : Convert.ToInt32(obj);
                    }

                    list.Add(entry);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Get Get Heavy App Workout Entries Async: {ex.Message}";
            }

            return list;
        }

        ///// <summary>
        ///// Asynchronously retrieves and returns Samsung step trend records for a specified person.
        ///// </summary>
        ///// <remarks>Establishes a database connection, executes the step trend command retrieved from <see cref="DatabaseCommands.GetSamsungStepTrendSql"/> with the given person identifier parameter, and reads the result set into a list of <see cref="SamsungStepTrendModel"/> instances.</remarks>
        ///// <param name="personId">The unique identifier of the person whose step trends are being requested.</param>
        ///// <returns>A task representing the asynchronous operation, containing a list of <see cref="SamsungStepTrendModel"/> objects.</returns>
        //public async Task<List<SamsungExerciseModel>> GetSamsungExerciseAsync(int personId)
        //{
        //    var result = new List<SamsungExerciseModel>();

        //    await using var sqlServerConnection = await OpenConnectionAsync();

        //    if (!IsConnected)
        //        return result;

        //    try
        //    {
        //        var sqlCommand = new MySqlCommand(
        //            DatabaseCommands.GetSamsungExerciseSql(),
        //            sqlServerConnection);

        //        sqlCommand.Parameters.AddWithValue("@PersonID", personId);

        //        await using var reader = await sqlCommand.ExecuteReaderAsync();

        //        while (await reader.ReadAsync())
        //        {
        //            result.Add(new SamsungExerciseModel
        //            {
        //                ExerciseID = reader.IsDBNull("exercise_id") ? null : reader.GetInt32("exercise_id"),
        //                LiveDataInternal = reader.IsDBNull("live_data_internal") ? null : reader.GetString("live_data_internal"),
        //                MissionValue = reader.IsDBNull("mission_value") ? null : reader.GetString("mission_value"),
        //                RaceTarget = reader.IsDBNull("race_target") ? null : reader.GetString("race_target"),
        //                SubsetData = reader.IsDBNull("subset_data") ? null : reader.GetString("subset_data"),
        //                StartLongitude = reader.IsDBNull("start_longitude") ? null : reader.GetDouble("start_longitude"),
        //                RoutineDataUuid = reader.IsDBNull("routine_data_uuid") ? null : reader.GetString("routine_data_uuid"),
        //                TotalCalorie = reader.IsDBNull("total_calorie") ? null : reader.GetDouble("total_calorie"),
        //                CompletionStatus = reader.IsDBNull("completion_status") ? null : reader.GetInt32("completion_status"),
        //                PaceInfoId = reader.IsDBNull("pace_info_id") ? null : reader.GetInt64("pace_info_id"),
        //                ActivityType = reader.IsDBNull("activity_type") ? null : reader.GetInt32("activity_type"),
        //                PaceLiveData = reader.IsDBNull("pace_live_data") ? null : reader.GetString("pace_live_data"),
        //                SensingStatus = reader.IsDBNull("sensing_status") ? null : reader.GetString("sensing_status"),
        //                SourceType = reader.IsDBNull("source_type") ? null : reader.GetInt32("source_type"),
        //                MissionType = reader.IsDBNull("mission_type") ? null : reader.GetInt32("mission_type"),
        //                Ftp = reader.IsDBNull("ftp") ? null : reader.GetDouble("ftp"),
        //                TrackingStatus = reader.IsDBNull("tracking_status") ? null : reader.GetInt32("tracking_status"),
        //                ProgramId = reader.IsDBNull("program_id") ? null : reader.GetInt64("program_id"),
        //                Title = reader.IsDBNull("title") ? null : reader.GetString("title"),
        //                RewardStatus = reader.IsDBNull("reward_status") ? null : reader.GetInt32("reward_status"),
        //                HeartRateSampleCount = reader.IsDBNull("heart_rate_sample_count") ? null : reader.GetInt32("heart_rate_sample_count"),
        //                StartLatitude = reader.IsDBNull("start_latitude") ? null : reader.GetDouble("start_latitude"),
        //                MissionExtraValue = reader.IsDBNull("mission_extra_value") ? null : reader.GetString("mission_extra_value"),
        //                ProgramScheduleId = reader.IsDBNull("program_schedule_id") ? null : reader.GetInt64("program_schedule_id"),
        //                HeartRateDeviceUuid = reader.IsDBNull("heart_rate_device_uuid") ? null : reader.GetString("heart_rate_device_uuid"),
        //                LocationDataInternal = reader.IsDBNull("location_data_internal") ? null : reader.GetString("location_data_internal"),
        //                CustomId = reader.IsDBNull("custom_id") ? null : reader.GetString("custom_id"),
        //                AdditionalInternal = reader.IsDBNull("additional_internal") ? null : reader.GetString("additional_internal"),

        //                Duration = reader.IsDBNull("duration") ? null : reader.GetInt64("duration"),
        //                Additional = reader.IsDBNull("additional") ? null : reader.GetString("additional"),
        //                CreateShVer = reader.IsDBNull("create_sync_version") ? null : reader.GetString("create_sync_version"),
        //                MeanCaloricBurnRate = reader.IsDBNull("mean_caloric_burn_rate") ? null : reader.GetDouble("mean_caloric_burn_rate"),
        //                LocationData = reader.IsDBNull("location_data") ? null : reader.GetString("location_data"),
        //                StartTime = reader.IsDBNull("start_time") ? null : reader.GetDateTime("start_time"),
        //                ExerciseType = reader.IsDBNull("exercise_type") ? null : reader.GetInt32("exercise_type"),
        //                Custom = reader.IsDBNull("custom_text") ? null : reader.GetString("custom_text"),
        //                MaxAltitude = reader.IsDBNull("max_altitude") ? null : reader.GetDouble("max_altitude"),
        //                InclineDistance = reader.IsDBNull("incline_distance") ? null : reader.GetDouble("incline_distance"),
        //                MeanHeartRate = reader.IsDBNull("mean_heart_rate") ? null : reader.GetDouble("mean_heart_rate"),
        //                CountType = reader.IsDBNull("count_type") ? null : reader.GetInt32("count_type"),
        //                MeanRpm = reader.IsDBNull("mean_rpm") ? null : reader.GetDouble("mean_rpm"),
        //                MinAltitude = reader.IsDBNull("min_altitude") ? null : reader.GetDouble("min_altitude"),
        //                ModifyShVer = reader.IsDBNull("modify_sync_version") ? null : reader.GetString("modify_sync_version"),
        //                MaxHeartRate = reader.IsDBNull("max_heart_rate") ? null : reader.GetDouble("max_heart_rate"),

        //                UpdateTime = reader.IsDBNull("update_at") ? null : reader.GetDateTime("update_at"),
        //                CreateTime = reader.IsDBNull("create_at") ? null : reader.GetDateTime("create_at"),

        //                ClientDataId = reader.IsDBNull("client_data_id") ? null : reader.GetString("client_data_id"),
        //                MaxPower = reader.IsDBNull("max_power") ? null : reader.GetDouble("max_power"),
        //                MaxSpeed = reader.IsDBNull("max_speed") ? null : reader.GetDouble("max_speed"),
        //                MeanCadence = reader.IsDBNull("mean_cadence") ? null : reader.GetDouble("mean_cadence"),
        //                MinHeartRate = reader.IsDBNull("min_heart_rate") ? null : reader.GetDouble("min_heart_rate"),
        //                ClientDataVer = reader.IsDBNull("client_data_version") ? null : reader.GetString("client_data_version"),
        //                Count = reader.IsDBNull("count_value") ? null : reader.GetInt32("count_value"),
        //                Distance = reader.IsDBNull("distance") ? null : reader.GetDouble("distance"),
        //                MaxCaloricBurnRate = reader.IsDBNull("max_caloric_burn_rate") ? null : reader.GetDouble("max_caloric_burn_rate"),
        //                Calorie = reader.IsDBNull("calorie") ? null : reader.GetDouble("calorie"),
        //                MaxCadence = reader.IsDBNull("max_cadence") ? null : reader.GetDouble("max_cadence"),
        //                DeclineDistance = reader.IsDBNull("decline_distance") ? null : reader.GetDouble("decline_distance"),
        //                Vo2Max = reader.IsDBNull("vo2_max") ? null : reader.GetDouble("vo2_max"),
        //                TimeOffset = reader.IsDBNull("time_offset") ? null : reader.GetString("time_offset"),
        //                DeviceUuid = reader.IsDBNull("device_uuid") ? null : reader.GetString("device_uuid"),
        //                MaxRpm = reader.IsDBNull("max_rpm") ? null : reader.GetDouble("max_rpm"),
        //                Comment = reader.IsDBNull("comment_text") ? null : reader.GetString("comment_text"),
        //                LiveData = reader.IsDBNull("live_data") ? null : reader.GetString("live_data"),
        //                MeanPower = reader.IsDBNull("mean_power") ? null : reader.GetDouble("mean_power"),
        //                MeanSpeed = reader.IsDBNull("mean_speed") ? null : reader.GetDouble("mean_speed"),
        //                PkgName = reader.IsDBNull("package_name") ? null : reader.GetString("package_name"),
        //                AltitudeGain = reader.IsDBNull("altitude_gain") ? null : reader.GetDouble("altitude_gain"),
        //                AltitudeLoss = reader.IsDBNull("altitude_loss") ? null : reader.GetDouble("altitude_loss"),
        //                ExerciseCustomType = reader.IsDBNull("exercise_custom_type") ? null : reader.GetInt32("exercise_custom_type"),
        //                AuxiliaryDevices = reader.IsDBNull("auxiliary_devices") ? null : reader.GetString("auxiliary_devices"),
        //                EndTime = reader.IsDBNull("end_time") ? null : reader.GetDateTime("end_time"),
        //                DataUuid = reader.IsDBNull("data_uuid") ? null : reader.GetString("data_uuid"),
        //                SweatLoss = reader.IsDBNull("sweat_loss") ? null : reader.GetDouble("sweat_loss")
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorMessage = $"Error Get Samsung Exercise Async: {ex.Message}";
        //    }

        //    return result;
        //}

        /// <summary>
        /// Asynchronously deletes a specific heart rate record from the database using its unique data identifier.
        /// </summary>
        /// <param name="heartRateDataUuid">The unique data identifier (UUID) of the heart rate record to be removed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteHeavyAppAsync(int heavyAppID)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return;

            try
            {
                var sqlCommand = new MySqlCommand(DatabaseCommands.GetWorkoutLogDeleteByWorkoutIdSql(), sqlServerConnection);
                sqlCommand.Parameters.AddWithValue("@id", heavyAppID);
                await sqlCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Delete Heart Rate Async: {ex.Message}";
            }
        }

        /// <summary>
        /// Lädt HeavyApp Trainingsdaten für eine Person.
        /// </summary>
        /// <param name="personId">Personen-ID.</param>
        /// <returns>Liste der Trainingseinträge.</returns>
        public async Task<List<HeavyAppCSVModel>> GetHeavyAppAsync(int personId)
        {
            var result = new List<HeavyAppCSVModel>();

            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected)
                return result;

            try
            {
                var sqlCommand = new MySqlCommand(
                    DatabaseCommands.GetWorkoutEntriesSql(),
                    sqlServerConnection);

                sqlCommand.Parameters.AddWithValue("@PersonID", personId);

                await using var reader = await sqlCommand.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(new HeavyAppCSVModel
                    {
                        ExerciseLogID = reader.GetInt32("exercise_log_id"),

                        Title = reader.IsDBNull("workout_title")
                            ? string.Empty
                            : reader.GetString("workout_title"),

                        UpdateTime = reader.IsDBNull("update_at")
                            ? null
                            : reader.GetDateTime("update_at"),

                        StartTime = reader.GetDateTime("start_time"),

                        EndTime = reader.GetDateTime("end_time"),

                        Description = reader.IsDBNull("description")
                            ? null
                            : reader.GetString("description"),

                        ExerciseTitle = reader.IsDBNull("exercise_title")
                            ? string.Empty
                            : reader.GetString("exercise_title"),

                        SupersetId = reader.IsDBNull("superset_id")
                            ? null
                            : reader.GetString("superset_id"),

                        ExerciseNotes = reader.IsDBNull("exercise_notes")
                            ? null
                            : reader.GetString("exercise_notes"),

                        SetIndex = reader.GetInt32("set_index"),

                        SetType = reader.IsDBNull("set_type")
                            ? null
                            : reader.GetString("set_type"),

                        WeightKg = reader.IsDBNull("weight")
                            ? null
                            : reader.GetDouble("weight"),

                        Reps = reader.IsDBNull("reps")
                            ? null
                            : Convert.ToDouble(reader.GetInt32("reps")),

                        DistanceKm = reader.IsDBNull("distance")
                            ? null
                            : reader.GetDouble("distance"),

                        DurationSeconds = reader.IsDBNull("duration_sec")
                            ? null
                            : reader.GetInt32("duration_sec"),

                        Rpe = reader.IsDBNull("rpe")
                            ? null
                            : reader.GetDouble("rpe"),

                        DataUuid = reader.GetString("uuid")
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Get HeavyApp Async: {ex.Message}";
            }

            return result;
        }

        #endregion

        #region Default SQL Funciton

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
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return list;

            try
            {
                await using var SqlCommand = new MySqlCommand(DatabaseCommands.GetPersonExistenceCheckSql(), sqlServerConnection);
                await using var SqlDataReader = await SqlCommand.ExecuteReaderAsync();

                while (await SqlDataReader.ReadAsync())
                {
                    list.Add(new PersonModel
                    {
                        PersonID = SqlDataReader.GetInt32(0),
                        PersonFirstName = SqlDataReader.IsDBNull(1) ? string.Empty : SqlDataReader.GetString(1),
                        PersonLastName = SqlDataReader.IsDBNull(2) ? string.Empty : SqlDataReader.GetString(2),
                        PersonBirthDate = SqlDataReader.IsDBNull(3) ? null : SqlDataReader.GetDateTime(3),
                        PersonHeight = SqlDataReader.GetFloat(4)
                    });
                }

            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error Get Person: {ex.Message}";
            }

            return list;
        }

        /// <summary>
        /// Asynchronously retrieves the total number of records stored in the 'tbl_Persons' table.
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
        public async Task<int> GetPersonCountAsync()
        {

            var sqlServerConnection = await OpenConnectionAsync();
            if (!IsConnected) return -1;

            try
            {
                await using var SqlCommand = new MySqlCommand(DatabaseCommands.GetPersonCountSql(), sqlServerConnection);
                var result = await SqlCommand.ExecuteScalarAsync();

                // Validation: If the result is null (DBNull), count is set to 0 to avoid exceptions
                int count = result != null ? Convert.ToInt32(result) : 0;

                // Return logic: Ensures the method never returns negative values, maintaining UI consistency
                return count > 0 ? count : 0;
            }

            catch (Exception ex)
            {
                ErrorMessage = $"Error Get Person StepCount: {ex.Message}";
            }

            return -1;

        }

        #endregion

    }
}

