using BodyTracker.Models;
using BodyTracker.Models.Chart;
using BodyTracker.Models.Export;
using BodyTracker.Models.FullBodyMeasurement;
using BodyTracker.Models.SamsungHealth;
using BodyTracker.Resources.Languages;
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
using System.Windows.Controls;
using System.Windows.Documents;


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

        /// <summary>
        /// Gets the name of the database being utilized by the currentCount service instance.
        /// </summary>
        public string DatabaseName { private set; get; } = string.Empty;

        /// <summary>
        /// Gets the database user name or credentials identifier used for authentication and connection purposes.
        /// </summary>
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
                    throw new Exception($"Error SQL Dispose: {ex.Message}", ex);

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

                if (!IsConnected) return;

            }
            catch (Exception ex)
            {
                throw new Exception($"Error during sqlServerConnection check/reconnect: {ex.Message}", ex);
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
            catch (TaskCanceledException ex)
            {
                throw new Exception("Connection monitoring task was canceled.", ex);
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
            }
            catch (Exception ex)
            {
                IsConnected = false;
                sqlServerConnection.Dispose();
                throw new Exception($"Database sqlServerConnection error: {ex.Message}", ex);
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
                await tx.CommitAsync();
                throw new Exception($"Error Update Person Async: {ex.Message}", ex);
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
                throw new Exception($"Error creating person: {ex.Message}", ex);
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
                throw new Exception($"Error Checking Measurement Existing: {ex.Message}", ex);
            }

            return false;
        }

        /// <summary>
        /// Asynchronously insertsList a new body metric record into the database.
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
                throw new Exception($"Error insert Body Metric: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Asynchronously insertsList a new body dimension record (circumferences) into the database.
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
                throw new Exception($"Error Insert Body Dimension: {ex.Message}", ex);
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
                throw new Exception($"Error Get Last Body Metrics: {ex.Message}", ex);
            }

            return new BodyMetricModel();
        }

        /// <summary>
        /// Asynchronously retrieves a list of body metric records for a specific person, 
        /// filtered within a specified timeframe leading up to the provided end date.
        /// </summary>
        /// <param name="personId">The unique identifier of the person whose metrics are being retrieved.</param>
        /// <param name="endDate">The reference end date used to define the upper bound of the retrieval window.</param>
        /// <param name="HowManyMonthsBefore">The number of months prior to the end date to include in the retrieval period.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a list of <see cref="BodyMetricModel"/> records found within the range.
        /// </returns>
        /// <remarks>
        /// This method handles nullable database fields for Weight, BMI, Fat, and Muscle percentages 
        /// by converting DBNull values to C# nullable types (float? or int?).
        /// </remarks>
        public async Task<List<BodyMetricModel>> GetBodyMetricForAIAsync(int personId, DateTime startDate, DateTime endDate)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();



            var list = new List<BodyMetricModel>();

            if (!IsConnected) return list;

            try
            {
                await using var SqlCommand = new MySqlCommand(DatabaseCommands.GetLastPersonMetricsForAISql(), sqlServerConnection);
                SqlCommand.Parameters.AddWithValue("@pid", personId);
                SqlCommand.Parameters.AddWithValue("@end", endDate.Date);
                SqlCommand.Parameters.AddWithValue("@start", startDate.Date);
                await using var SqlDataReader = await SqlCommand.ExecuteReaderAsync();
                while (await SqlDataReader.ReadAsync())
                {
                    list.Add(new BodyMetricModel
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
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error Get Body Metrics for AI: {ex.Message}", ex);
            }

            return list;
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
                throw new Exception($"Error Get Body Dimensions: {ex.Message}", ex);
            }

            return new BodyDimensionsModel();
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
        public async Task<FullBodyMeasurementExportModel> GetFullMeasurementDatasForExport(int personId)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            List<BodyMetricsExportModel> bodyMetric = new List<BodyMetricsExportModel>();

            List<BodyDimensionsExportModel> bodyDimension = new List<BodyDimensionsExportModel>();


            if (!IsConnected) return new FullBodyMeasurementExportModel();

            try
            {
                // Body Dimensions
                await using (var sqlCommand = new MySqlCommand(
                    "SELECT * FROM tbl_BodyDimensions WHERE person_id = @pid",
                    sqlServerConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@pid", personId);

                    await using var dimensionReader = await sqlCommand.ExecuteReaderAsync();

                    while (await dimensionReader.ReadAsync())
                    {
                        bodyDimension.Add(new BodyDimensionsExportModel
                        {
                            Id = dimensionReader.GetInt32(0),

                            CreateTime = dimensionReader.GetDateTime(2),
                            UpdateTime = dimensionReader.GetDateTime(3),

                            CurrentChestCircumference = dimensionReader.IsDBNull(4) ? null : dimensionReader.GetFloat(4),
                            InitialChestCircumference = dimensionReader.IsDBNull(5) ? null : dimensionReader.GetFloat(5),

                            CurrentWaistCircumference = dimensionReader.IsDBNull(6) ? null : dimensionReader.GetFloat(6),
                            InitialWaistCircumference = dimensionReader.IsDBNull(7) ? null : dimensionReader.GetFloat(7),

                            CurrentHipsCircumference = dimensionReader.IsDBNull(8) ? null : dimensionReader.GetFloat(8),
                            InitialHipsCircumference = dimensionReader.IsDBNull(9) ? null : dimensionReader.GetFloat(9),

                            CurrentFatTongBreastCrease = dimensionReader.IsDBNull(10) ? null : dimensionReader.GetFloat(10),
                            InitialFatTongBreastCrease = dimensionReader.IsDBNull(11) ? null : dimensionReader.GetFloat(11),

                            CurrentFatTongArmpitCrease = dimensionReader.IsDBNull(12) ? null : dimensionReader.GetFloat(12),
                            InitialFatTongArmpitCrease = dimensionReader.IsDBNull(13) ? null : dimensionReader.GetFloat(13),

                            CurrentFatTongAbdominalCrease = dimensionReader.IsDBNull(14) ? null : dimensionReader.GetFloat(14),
                            InitialFatTongAbdominalCrease = dimensionReader.IsDBNull(15) ? null : dimensionReader.GetFloat(15),

                            CurrentFatTongHipCrease = dimensionReader.IsDBNull(16) ? null : dimensionReader.GetFloat(16),
                            InitialFatTongHipCrease = dimensionReader.IsDBNull(17) ? null : dimensionReader.GetFloat(17),

                            CurrentFatTongThighCrease = dimensionReader.IsDBNull(18) ? null : dimensionReader.GetFloat(18),
                            InitialFatTongThighCrease = dimensionReader.IsDBNull(19) ? null : dimensionReader.GetFloat(19),

                            CurrentFatTongBackCrease = dimensionReader.IsDBNull(20) ? null : dimensionReader.GetFloat(20),
                            InitialFatTongBackCrease = dimensionReader.IsDBNull(21) ? null : dimensionReader.GetFloat(21),

                            CurrentFatTongTricepsCrease = dimensionReader.IsDBNull(22) ? null : dimensionReader.GetFloat(22),
                            InitialFatTongTricepsCrease = dimensionReader.IsDBNull(23) ? null : dimensionReader.GetFloat(23)
                        });
                    }
                }

                // Body Metrics
                await using (var sqlCommand = new MySqlCommand(
                    "SELECT * FROM tbl_BodyMetrics WHERE person_id = @pid",
                    sqlServerConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@pid", personId);

                    await using var metricReader = await sqlCommand.ExecuteReaderAsync();


                    while (await metricReader.ReadAsync())
                    {
                        bodyMetric.Add(new BodyMetricsExportModel
                        {
                            Id = metricReader.GetInt32(0),

                            CurrentTime = metricReader.GetDateTime(2),
                            UpdateTime = metricReader.GetDateTime(3),

                            CurrentBodyWeight = metricReader.IsDBNull(4) ? null : metricReader.GetFloat(4),
                            CurrentBMI = metricReader.IsDBNull(5) ? null : metricReader.GetFloat(5),

                            CurrentBodyFatPercentage = metricReader.IsDBNull(6) ? null : metricReader.GetFloat(6),
                            CurrentBodyFatPercentageTop = metricReader.IsDBNull(7) ? null : metricReader.GetFloat(7),
                            CurrentBodyFatPercentageBottom = metricReader.IsDBNull(8) ? null : metricReader.GetFloat(8),

                            CurrentBodyMusclePercentage = metricReader.IsDBNull(9) ? null : metricReader.GetFloat(9),
                            CurrentBodyMusclePercentageTop = metricReader.IsDBNull(10) ? null : metricReader.GetFloat(10),
                            CurrentBodyMusclePercentageBottom = metricReader.IsDBNull(11) ? null : metricReader.GetFloat(11),

                            CurrentBodyWaterPercentage = metricReader.IsDBNull(12) ? null : metricReader.GetFloat(12),

                            CurrentBodyBoneMass = metricReader.IsDBNull(13) ? null : metricReader.GetFloat(13),

                            CurrentBodyVisceralFat = metricReader.IsDBNull(14) ? null : metricReader.GetInt32(14),

                            InitialBodyWeight = metricReader.IsDBNull(15) ? null : metricReader.GetFloat(15),
                            InitialBMI = metricReader.IsDBNull(16) ? null : metricReader.GetFloat(16),

                            InitialBodyFatPercentage = metricReader.IsDBNull(17) ? null : metricReader.GetFloat(17),
                            InitialBodyFatPercentageTop = metricReader.IsDBNull(18) ? null : metricReader.GetFloat(18),
                            InitialBodyFatPercentageBottom = metricReader.IsDBNull(19) ? null : metricReader.GetFloat(19),

                            InitialBodyMusclePercentage = metricReader.IsDBNull(20) ? null : metricReader.GetFloat(20),
                            InitialBodyMusclePercentageTop = metricReader.IsDBNull(21) ? null : metricReader.GetFloat(21),
                            InitialBodyMusclePercentageBottom = metricReader.IsDBNull(22) ? null : metricReader.GetFloat(22),

                            InitialBodyWaterPercentage = metricReader.IsDBNull(23) ? null : metricReader.GetFloat(23),

                            InitialBodyBoneMass = metricReader.IsDBNull(24) ? null : metricReader.GetFloat(24),

                            InitialBodyVisceralFat = metricReader.IsDBNull(25) ? null : metricReader.GetInt32(25)
                        });
                    }
                }


            }
            catch (Exception ex)
            {
                throw new Exception($"Error Get Body Dimensions : {ex.Message}", ex);
            }


            return new FullBodyMeasurementExportModel
            {
                BodyDimensions = bodyDimension,
                BodyMetrics = bodyMetric
            };
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
                throw new Exception($"Error Get Body Measurement Async: {ex.Message}", ex);
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
                throw new Exception($"Error Delete Body Metric Async: {ex.Message}", ex);
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
                throw new Exception($"Error Delete Body Dimension Async: {ex.Message}", ex);
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

                await tx.CommitAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error Update Measurement Async: {ex.Message}", ex);
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
                throw new Exception($"Error Insert Food Intake: {ex.Message}", ex);
            }
            ;
        }

        /// <summary>
        /// Asynchronously synchronizes a collection of Samsung Health food intake records for a specific person into the database, 
        /// performing bulk insertions for new entries and transactional updatesList for existing records based on unique data identifiers and update timestamps.
        /// </summary>
        /// <param name="PersonId">The unique identifier of the person to whom the data belongs.</param>
        /// <param name="foodIntakeList">A collection of <see cref="SamsungFoodIntakeModel"/> objects containing the nutritional data to be stored.</param>
        /// <param name="progress">An optional progress indicator that reports the percentage of the insertion process.</param>
        /// <returns>Returns 'true' if the operation completes successfully.</returns>
        public async Task<bool> UpsertFoodIntakeSqlAsync(
          int personId,
          ICollection<SamsungFoodIntakeModel> foodIntakeList,
          IProgress<(double value, string prgressText)>? progress = null)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected)
                return false;

            try
            {
                var existingEntries = new Dictionary<string, (
                    int FoodIntakeId,
                    string Uuid,
                    DateTime? UpdateTime)>();

                await using var cmd = new MySqlCommand(
                    DatabaseCommands.GetFoodIntakeExistenceCheckSql(),
                    sqlServerConnection);

                cmd.Parameters.AddWithValue("@PersonId", personId);

                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        existingEntries.Add(
                            reader.GetString("data_uuid"),
                            (
                                reader.GetInt32("food_intake_id"),
                                reader.GetString("data_uuid"),
                                reader.IsDBNull("update_at")
                                    ? null
                                    : reader.GetDateTime("update_at")
                            ));
                    }
                }

                var insertsList = new List<SamsungFoodIntakeModel>();
                var updatesList = new List<SamsungFoodIntakeModel>();
                var deleteList = new List<int>();

                double percentage = 0;

                foreach (var item in foodIntakeList)
                {
                    if (string.IsNullOrWhiteSpace(item.DataUuid))
                        continue;

                    if (!existingEntries.TryGetValue(item.DataUuid, out var dbEntry))
                    {
                        insertsList.Add(item);
                        continue;
                    }

                    if (item.UpdateTime.HasValue &&
                        (!dbEntry.UpdateTime.HasValue ||
                         item.UpdateTime > dbEntry.UpdateTime))
                    {
                        updatesList.Add(item);
                    }
                }

                // Retrieve entries for deleting
                foreach (var dbEntry in existingEntries.Values)
                {
                    if (!foodIntakeList.Any(x => x.DataUuid == dbEntry.Uuid))
                    {
                        deleteList.Add(dbEntry.FoodIntakeId);
                    }
                }

                #region Inserts

                await using var transactionInsert =
                    await sqlServerConnection.BeginTransactionAsync();

                if (insertsList.Count > 0)
                {
                    var table =
                        SamsungFoodIntakeDataTable.CreateFoodIntakeDataTable(
                            personId,
                            insertsList);

                    await using var commandInsert = new MySqlCommand(
                        DatabaseCommands.GetSamsungFoodIntakeUpsertSql(),
                        sqlServerConnection,
                        (MySqlTransaction)transactionInsert);

                    foreach (DataColumn column in table.Columns)
                    {
                        commandInsert.Parameters.Add(
                            new MySqlParameter(
                                "@" + column.ColumnName,
                                null));
                    }

                    commandInsert.Prepare();

                    int insertCount = 0;

                    foreach (DataRow row in table.Rows)
                    {
                        foreach (DataColumn column in table.Columns)
                        {
                            commandInsert.Parameters["@" + column.ColumnName].Value =
                                row[column] ?? DBNull.Value;
                        }

                        await commandInsert.ExecuteNonQueryAsync();

                        insertCount++;

                        if (insertCount % 100 == 0)
                        {
                            percentage =
                                (double)insertCount / insertsList.Count * 100;

                            progress?.Report(
                                (percentage,
                                 UiStrings.Status_Database_Inserted));
                        }
                    }
                }

                await transactionInsert.CommitAsync();

                #endregion

                #region Updates

                int currentCount = 0;
                int total = updatesList.Count;

                await using var transactionUpdate =
                    await sqlServerConnection.BeginTransactionAsync();

                foreach (var item in updatesList)
                {
                    await using var command = new MySqlCommand(
                        DatabaseCommands.GetSamsungHealthFoodIntakeUpdateSql(),
                        sqlServerConnection,
                        (MySqlTransaction)transactionUpdate);

                    command.Parameters.AddWithValue("@PersonID_FK", personId);

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

                    await command.ExecuteNonQueryAsync();

                    currentCount++;

                    if (currentCount % 100 == 0)
                    {
                        percentage =
                            (double)currentCount / total * 100;

                        progress?.Report(
                            (percentage,
                             UiStrings.Status_Database_Updated));
                    }
                }

                await transactionUpdate.CommitAsync();

                #endregion

                #region Delete

                int deleteCount = 0;

                await using var transactionDelete =
                    await sqlServerConnection.BeginTransactionAsync();

                foreach (var foodIntakeId in deleteList)
                {
                    await using var command = new MySqlCommand(
                        DatabaseCommands.GetFoodIntakeDeleteSql(),
                        sqlServerConnection,
                        (MySqlTransaction)transactionDelete);

                    command.Parameters.AddWithValue("@id", foodIntakeId);

                    await command.ExecuteNonQueryAsync();

                    deleteCount++;

                    if (deleteCount % 100 == 0)
                    {
                        percentage =
                            (double)deleteCount / deleteList.Count * 100;

                        progress?.Report(
                            (percentage,
                             UiStrings.Status_Database_Deleted));
                    }
                }

                await transactionDelete.CommitAsync();

                #endregion

                percentage = 100;

                progress?.Report(
                    (percentage,
                     UiStrings.Status_Database_Finished));

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error Insert Food Intake Datas", ex);
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
                throw new Exception($"Error Get Food Intake Async: {ex.Message}", ex);

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
                throw new Exception($"Error Delete Food Intake Async: {ex.Message}", ex);
            }
        }

        #endregion

        #region Samsung Food Info

        /// <summary>
        /// Asynchronously synchronizes a collection of Samsung Health food intake records for a specific person into the database, 
        /// performing bulk insertions for new entries and transactional updatesList for existing records based on unique data identifiers and update timestamps.
        /// </summary>
        /// <param name="PersonId">The unique identifier of the person to whom the data belongs.</param>
        /// <param name="foodInfoList">A collection of <see cref="SamsungFoodIntakeModel"/> objects containing the nutritional data to be stored.</param>
        /// <param name="progress">An optional progress indicator that reports the percentage of the insertion process.</param>
        /// <returns>Returns 'true' if the operation completes successfully.</returns>
        public async Task<bool> UpsertFoodInfoSqlAsync(
            int personId,
            ICollection<SamsungFoodInfoModel> foodInfoList,
            IProgress<(double value, string prgressText)>? progress = null)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected)
                return false;

            try
            {
                var existingEntries = new Dictionary<string, (
                    int FoodInfoId,
                    string Uuid,
                    DateTime? UpdateTime)>();

                await using var cmd = new MySqlCommand(
                    DatabaseCommands.GetFoodInfoExistenceCheckSql(),
                    sqlServerConnection);

                cmd.Parameters.AddWithValue("@PersonId", personId);

                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        existingEntries.Add(
                            reader.GetString("data_uuid"),
                            (
                                reader.GetInt32("food_info_id"),
                                reader.GetString("data_uuid"),
                                reader.IsDBNull("update_at")
                                    ? null
                                    : reader.GetDateTime("update_at")
                            ));
                    }
                }

                var insertsList = new List<SamsungFoodInfoModel>();
                var updatesList = new List<SamsungFoodInfoModel>();
                var deleteList = new List<int>();

                double percentage = 0;

                foreach (var item in foodInfoList)
                {
                    if (string.IsNullOrWhiteSpace(item.DataUuid))
                        continue;

                    if (!existingEntries.TryGetValue(item.DataUuid, out var dbEntry))
                    {
                        insertsList.Add(item);
                        continue;
                    }

                    if (item.UpdateTime.HasValue &&
                        (!dbEntry.UpdateTime.HasValue ||
                         item.UpdateTime > dbEntry.UpdateTime))
                    {
                        updatesList.Add(item);
                    }
                }

                // Retrieve entries for deleting
                foreach (var dbEntry in existingEntries.Values)
                {
                    if (!foodInfoList.Any(x => x.DataUuid == dbEntry.Uuid))
                    {
                        deleteList.Add(dbEntry.FoodInfoId);
                    }
                }

                #region Inserts

                await using var transactionInsert =
                    await sqlServerConnection.BeginTransactionAsync();

                if (insertsList.Count > 0)
                {
                    var table =
                        SamsungFoodInfoDataTable.CreateFoodInfoDataTable(
                            personId,
                            insertsList);

                    await using var commandInsert = new MySqlCommand(
                        DatabaseCommands.GetSamsungFoodInfoInsertSql(),
                        sqlServerConnection,
                        (MySqlTransaction)transactionInsert);

                    foreach (DataColumn column in table.Columns)
                    {
                        commandInsert.Parameters.Add(
                            new MySqlParameter(
                                "@" + column.ColumnName,
                                null));
                    }

                    commandInsert.Prepare();

                    int insertCount = 0;

                    foreach (DataRow row in table.Rows)
                    {
                        foreach (DataColumn column in table.Columns)
                        {
                            commandInsert.Parameters["@" + column.ColumnName].Value =
                                row[column] ?? DBNull.Value;
                        }

                        await commandInsert.ExecuteNonQueryAsync();

                        insertCount++;

                        if (insertCount % 100 == 0)
                        {
                            percentage =
                                (double)insertCount / insertsList.Count * 100;

                            progress?.Report(
                                (percentage,
                                 UiStrings.Status_Database_Inserted));
                        }
                    }
                }

                await transactionInsert.CommitAsync();

                #endregion

                #region Updates

                int currentCount = 0;
                int total = updatesList.Count;

                await using var transactionUpdate =
                    await sqlServerConnection.BeginTransactionAsync();

                foreach (var item in updatesList)
                {
                    await using var command = new MySqlCommand(
                        DatabaseCommands.GetSamsungHealthFoodInfoUpdateSql(),
                        sqlServerConnection,
                        (MySqlTransaction)transactionUpdate);

                    command.Parameters.AddWithValue("@Potassium", (object?)item.Potassium ?? DBNull.Value);
                    command.Parameters.AddWithValue("@VitaminA", (object?)item.VitaminA ?? DBNull.Value);
                    command.Parameters.AddWithValue("@VitaminC", (object?)item.VitaminC ?? DBNull.Value);
                    command.Parameters.AddWithValue("@VitaminD", (object?)item.VitaminD ?? DBNull.Value);

                    command.Parameters.AddWithValue("@Cholesterol", (object?)item.Cholesterol ?? DBNull.Value);

                    command.Parameters.AddWithValue("@Description", (object?)item.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Custom", (object?)item.Custom ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ProviderFoodId", (object?)item.ProviderFoodId ?? DBNull.Value);

                    command.Parameters.AddWithValue("@MetricServingAmount", (object?)item.MetricServingAmount ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MetricServingUnit", (object?)item.MetricServingUnit ?? DBNull.Value);

                    command.Parameters.AddWithValue("@Sodium", (object?)item.Sodium ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DietaryFiber", (object?)item.DietaryFiber ?? DBNull.Value);

                    command.Parameters.AddWithValue("@TotalFat", (object?)item.TotalFat ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MonosaturatedFat", (object?)item.MonosaturatedFat ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PolysaturatedFat", (object?)item.PolysaturatedFat ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SaturatedFat", (object?)item.SaturatedFat ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TransFat", (object?)item.TransFat ?? DBNull.Value);

                    command.Parameters.AddWithValue("@Protein", (object?)item.Protein ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Iron", (object?)item.Iron ?? DBNull.Value);

                    command.Parameters.AddWithValue("@Sugar", (object?)item.Sugar ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AddedSugar", (object?)item.AddedSugar ?? DBNull.Value);

                    command.Parameters.AddWithValue("@Calcium", (object?)item.Calcium ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Calorie", (object?)item.Calorie ?? DBNull.Value);

                    command.Parameters.AddWithValue("@ServingDescription", (object?)item.ServingDescription ?? DBNull.Value);
                    command.Parameters.AddWithValue("@InfoProvider", (object?)item.InfoProvider ?? DBNull.Value);

                    command.Parameters.AddWithValue("@Name", (object?)item.Name ?? DBNull.Value);

                    command.Parameters.AddWithValue("@Carbohydrate", (object?)item.Carbohydrate ?? DBNull.Value);

                    command.Parameters.AddWithValue("@UnitCountPerCalorie", (object?)item.UnitCountPerCalorie ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DefaultNumberOfServingUnit", (object?)item.DefaultNumberOfServingUnit ?? DBNull.Value);

                    command.Parameters.AddWithValue("@DeviceUuid", (object?)item.DeviceUuid ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PkgName", (object?)item.PkgName ?? DBNull.Value);

                    command.Parameters.AddWithValue("@UpdateTime", (object?)item.UpdateTime ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CreateTime", (object?)item.CreateTime ?? DBNull.Value);

                    command.Parameters.AddWithValue("@DataUuid", item.DataUuid);
                    command.Parameters.AddWithValue("@PersonID_FK", personId);

                    await command.ExecuteNonQueryAsync();

                    currentCount++;

                    if (currentCount % 100 == 0)
                    {
                        percentage =
                            (double)currentCount / total * 100;

                        progress?.Report(
                            (percentage,
                             UiStrings.Status_Database_Updated));
                    }
                }

                await transactionUpdate.CommitAsync();

                #endregion

                #region Delete

                int deleteCount = 0;

                await using var transactionDelete =
                    await sqlServerConnection.BeginTransactionAsync();

                foreach (var foodInfoId in deleteList)
                {
                    await using var command = new MySqlCommand(
                        DatabaseCommands.GetFoodInfoDeleteSql(),
                        sqlServerConnection,
                        (MySqlTransaction)transactionDelete);

                    command.Parameters.AddWithValue("@id", foodInfoId);

                    await command.ExecuteNonQueryAsync();

                    deleteCount++;

                    if (deleteCount % 100 == 0)
                    {
                        percentage =
                            (double)deleteCount / deleteList.Count * 100;

                        progress?.Report(
                            (percentage,
                             UiStrings.Status_Database_Deleted));
                    }
                }

                await transactionDelete.CommitAsync();

                #endregion

                percentage = 100;

                progress?.Report((percentage, UiStrings.Status_Database_Finished));

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error Insert Food Info Async", ex);
            }
        }

        /// <summary>
        /// Asynchronously retrieves and returns Samsung food info records for a specified person.
        /// </summary>
        /// <param name="personId">The unique identifier of the person whose food info records are being requested.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of <see cref="SamsungFoodInfoModel"/> objects.</returns>
        public async Task<List<SamsungFoodInfoModel>> GetSamsungFoodInfoSqlAsync(int personId)
        {
            var result = new List<SamsungFoodInfoModel>();

            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected)
                return result;

            try
            {
                var sqlCommand = new MySqlCommand(
                    DatabaseCommands.GetSamsungFoodInfoSql(), // Oder der entsprechende Select-Befehl
                    sqlServerConnection);

                sqlCommand.Parameters.AddWithValue("@PersonId", personId);

                await using var reader = await sqlCommand.ExecuteReaderAsync();
                result.Clear();
                while (await reader.ReadAsync())
                {
                    result.Add(new SamsungFoodInfoModel
                    {
                        FoodInfoID = reader.GetInt32("food_info_id"),
                        Potassium = reader.IsDBNull("potassium") ? (double?)null : reader.GetDouble("potassium"),
                        VitaminA = reader.IsDBNull("vitamin_a") ? (double?)null : reader.GetDouble("vitamin_a"),
                        VitaminC = reader.IsDBNull("vitamin_c") ? (double?)null : reader.GetDouble("vitamin_c"),
                        VitaminD = reader.IsDBNull("vitamin_d") ? (double?)null : reader.GetDouble("vitamin_d"),
                        Cholesterol = reader.IsDBNull("cholesterol") ? (double?)null : reader.GetDouble("cholesterol"),
                        Description = reader.IsDBNull("description") ? null : reader.GetString("description"),
                        Custom = reader.IsDBNull("custom") ? null : reader.GetString("custom"),
                        ProviderFoodId = reader.IsDBNull("provider_food_id") ? null : reader.GetString("provider_food_id"),
                        MetricServingAmount = reader.IsDBNull("metric_serving_amount") ? (double?)null : reader.GetDouble("metric_serving_amount"),
                        MetricServingUnit = reader.IsDBNull("metric_serving_unit") ? null : reader.GetString("metric_serving_unit"),
                        Sodium = reader.IsDBNull("sodium") ? (double?)null : reader.GetDouble("sodium"),
                        DietaryFiber = reader.IsDBNull("dietary_fiber") ? (double?)null : reader.GetDouble("dietary_fiber"),
                        TotalFat = reader.IsDBNull("total_fat") ? (double?)null : reader.GetDouble("total_fat"),
                        MonosaturatedFat = reader.IsDBNull("monosaturated_fat") ? (double?)null : reader.GetDouble("monosaturated_fat"),
                        PolysaturatedFat = reader.IsDBNull("polysaturated_fat") ? (double?)null : reader.GetDouble("polysaturated_fat"),
                        SaturatedFat = reader.IsDBNull("saturated_fat") ? (double?)null : reader.GetDouble("saturated_fat"),
                        TransFat = reader.IsDBNull("trans_fat") ? null : reader.GetDouble("trans_fat"),
                        Protein = reader.IsDBNull("protein") ? (double?)null : reader.GetDouble("protein"),
                        Iron = reader.IsDBNull("iron") ? (double?)null : reader.GetDouble("iron"),
                        Sugar = reader.IsDBNull("sugar") ? (double?)null : reader.GetDouble("sugar"),
                        AddedSugar = reader.IsDBNull("added_sugar") ? (double?)null : reader.GetDouble("added_sugar"),
                        Calcium = reader.IsDBNull("calcium") ? (double?)null : reader.GetDouble("calcium"),
                        Calorie = reader.IsDBNull("calorie") ? (double?)null : reader.GetDouble("calorie"),
                        ServingDescription = reader.IsDBNull("serving_description") ? null : reader.GetString("serving_description"),
                        InfoProvider = reader.IsDBNull("info_provider") ? null : reader.GetString("info_provider"),
                        Name = reader.IsDBNull("name") ? null : reader.GetString("name"),
                        Carbohydrate = reader.IsDBNull("carbohydrate") ? (double?)null : reader.GetDouble("carbohydrate"),
                        UnitCountPerCalorie = reader.IsDBNull("unit_count_per_calorie") ? (double?)null : reader.GetDouble("unit_count_per_calorie"),
                        DefaultNumberOfServingUnit = reader.IsDBNull("default_number_of_serving_unit") ? (double?)null : reader.GetDouble("default_number_of_serving_unit"),
                        DeviceUuid = reader.IsDBNull("device_uuid") ? null : reader.GetString("device_uuid"),
                        PkgName = reader.IsDBNull("pkg_name") ? null : reader.GetString("pkg_name"),
                        DataUuid = reader.IsDBNull("data_uuid") ? null : reader.GetString("data_uuid"),
                        UpdateTime = reader.GetDateTime("update_at"),
                        CreateTime = reader.GetDateTime("create_at")
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error Get Food Indo Async", ex);
            }

            return result;
        }

        /// <summary>
        /// Asynchronously deletes a specific food info record from the database using its unique identifier.
        /// </summary>
        /// <param name="foodInfoId">The primary key (ID) of the food info record to be removed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GetFoodInfoDeleteSqlAsync(int foodInfoId)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return;

            try
            {
                var sqlCommand = new MySqlCommand(DatabaseCommands.GetFoodInfoDeleteSql(), sqlServerConnection);
                sqlCommand.Parameters.AddWithValue("@id", foodInfoId);
                await sqlCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {

                throw new Exception($"Error Delete Food Info Async: {ex.Message}", ex);
            }
        }

        #endregion

        #region Samsung Food Nutrtion

        /// <summary>
        /// Synchronizes food nutrition records with the database via upsert and delete operations.
        /// </summary>
        /// <param name="personId">The ID of the person.</param>
        /// <param name="nutritionList">The list of nutrition records.</param>
        /// <param name="progress">Optional progress reporter.</param>
        /// <returns>True if successful.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during execution.</exception>
        public async Task<bool> UpsertFoodNutritionSqlAsync(
            int personId,
            ICollection<SamsungFoodNutritionModel> nutritionList,
            IProgress<(double value, string progressText)>? progress = null)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected)
                return false;

            try
            {
                var existingEntries = new Dictionary<string, (int FoodNutritionId, string Uuid, DateTime UpdateTime)>();

                await using var cmd = new MySqlCommand(
                    DatabaseCommands.GetFoodNutritionExistenceCheckSql(),
                    sqlServerConnection);

                cmd.Parameters.AddWithValue("@PersonId", personId);

                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        existingEntries.Add(
                            reader.GetString("datauuid"),
                            (
                                reader.GetInt16("food_nutrition_id"),
                                 reader.GetString("datauuid"),

                                reader.GetDateTime("update_time"))
                            );

                    }
                }

                var insertsList = new List<SamsungFoodNutritionModel>();
                var updatesList = new List<SamsungFoodNutritionModel>();
                var deleteList = new List<int>();
                double percentage = 0;


                foreach (var item in nutritionList)
                {
                    if (string.IsNullOrWhiteSpace(item.DataUuid))
                        continue;

                    if (!existingEntries.TryGetValue(item.DataUuid, out var _dbEntries))
                    {
                        insertsList.Add(item);
                        continue;
                    }

                    if (item.UpdateTime > _dbEntries.UpdateTime)
                    {
                        updatesList.Add(item);
                    }
                }

                // Retrieve entries for deleting
                foreach (var dbEntry in existingEntries.Values)
                {
                    if (!nutritionList.Any(n => n.DataUuid == dbEntry.Uuid))
                    {
                        deleteList.Add(dbEntry.FoodNutritionId);
                    }
                }

                #region Inserts

                await using var transactionInsert =
                    await sqlServerConnection.BeginTransactionAsync();

                if (insertsList.Count > 0)
                {
                    var table =
                        SamsungFoodNutritionDataTable.CreateFoodNutritionDataTable(personId, insertsList);

                    await using var commandInsert = new MySqlCommand(
                        DatabaseCommands.GetSamsungNutritionInsertSql(),
                        sqlServerConnection,
                        (MySqlTransaction)transactionInsert);

                    foreach (DataColumn column in table.Columns)
                    {
                        commandInsert.Parameters.Add(new MySqlParameter("@" + column.ColumnName, null));
                    }

                    commandInsert.Prepare();

                    int insertCount = 0;

                    foreach (DataRow row in table.Rows)
                    {
                        foreach (DataColumn column in table.Columns)
                        {
                            commandInsert.Parameters["@" + column.ColumnName].Value =
                                row[column] ?? DBNull.Value;
                        }

                        await commandInsert.ExecuteNonQueryAsync();

                        insertCount++;

                        if (insertCount % 100 == 0)
                        {
                            percentage = (double)insertCount / insertsList.Count * 100;
                            progress?.Report((percentage, UiStrings.Status_Database_Inserted));
                        }
                    }
                }

                await transactionInsert.CommitAsync();

                #endregion

                #region Updates

                int currentCount = 0;
                int total = updatesList.Count;

                await using var transactionUpdate =
                    await sqlServerConnection.BeginTransactionAsync();

                foreach (var item in updatesList)
                {
                    await using var command = new MySqlCommand(
                        DatabaseCommands.GetSamsungNutritionUpdateSql(),
                        sqlServerConnection,
                        (MySqlTransaction)transactionUpdate);

                    command.Parameters.AddWithValue("@Magnesium", (object?)item.Magnesium ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Potassium", (object?)item.Potassium ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CreateShVer", (object?)item.CreateShVer ?? DBNull.Value);

                    command.Parameters.AddWithValue("@VitaminA", (object?)item.VitaminA ?? DBNull.Value);
                    command.Parameters.AddWithValue("@VitaminC", (object?)item.VitaminC ?? DBNull.Value);
                    command.Parameters.AddWithValue("@VitaminD", (object?)item.VitaminD ?? DBNull.Value);
                    command.Parameters.AddWithValue("@VitaminE", (object?)item.VitaminE ?? DBNull.Value);
                    command.Parameters.AddWithValue("@VitaminK", (object?)item.VitaminK ?? DBNull.Value);
                    command.Parameters.AddWithValue("@VitaminB12", (object?)item.VitaminB12 ?? DBNull.Value);

                    command.Parameters.AddWithValue("@Cholesterol", (object?)item.Cholesterol ?? DBNull.Value);

                    command.Parameters.AddWithValue("@StartTime", (object?)item.StartTime ?? DBNull.Value);

                    command.Parameters.AddWithValue("@Biotin", (object?)item.Biotin ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Phosphorus", (object?)item.Phosphorus ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Copper", (object?)item.Copper ?? DBNull.Value);

                    command.Parameters.AddWithValue("@Custom", (object?)item.Custom ?? DBNull.Value);

                    command.Parameters.AddWithValue("@Thiamin", (object?)item.Thiamin ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Folate", (object?)item.Folate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Iodine", (object?)item.Iodine ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Niacin", (object?)item.Niacin ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Sodium", (object?)item.Sodium ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Molybdenum", (object?)item.Molybdenum ?? DBNull.Value);
                    command.Parameters.AddWithValue("@FolicAcid", (object?)item.FolicAcid ?? DBNull.Value);

                    command.Parameters.AddWithValue("@ModifyShVer", (object?)item.ModifyShVer ?? DBNull.Value);

                    command.Parameters.AddWithValue("@DietaryFiber", (object?)item.DietaryFiber ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TotalFat", (object?)item.TotalFat ?? DBNull.Value);

                    command.Parameters.AddWithValue("@UpdateTime", (object?)item.UpdateTime ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CreateTime", (object?)item.CreateTime ?? DBNull.Value);

                    command.Parameters.AddWithValue("@MealType", (object?)item.MealType ?? DBNull.Value);

                    command.Parameters.AddWithValue("@Caffeine", (object?)item.Caffeine ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MonosaturatedFat", (object?)item.MonosaturatedFat ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Protein", (object?)item.Protein ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Manganese", (object?)item.Manganese ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PolysaturatedFat", (object?)item.PolysaturatedFat ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Iron", (object?)item.Iron ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Zinc", (object?)item.Zinc ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Riboflavin", (object?)item.Riboflavin ?? DBNull.Value);

                    command.Parameters.AddWithValue("@Sugar", (object?)item.Sugar ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Title", (object?)item.Title ?? DBNull.Value);

                    command.Parameters.AddWithValue("@AddedSugar", (object?)item.AddedSugar ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CaloriesFromFat", (object?)item.CaloriesFromFat ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Calcium", (object?)item.Calcium ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Calorie", (object?)item.Calorie ?? DBNull.Value);

                    command.Parameters.AddWithValue("@TimeOffset", (object?)item.TimeOffset ?? DBNull.Value);

                    command.Parameters.AddWithValue("@DeviceUuid", (object?)item.DeviceUuid ?? DBNull.Value);

                    command.Parameters.AddWithValue("@PolyunsaturatedFat", (object?)item.PolyunsaturatedFat ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SaturatedFat", (object?)item.SaturatedFat ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TransFat", (object?)item.TransFat ?? DBNull.Value);

                    command.Parameters.AddWithValue("@PkgName", (object?)item.PkgName ?? DBNull.Value);

                    command.Parameters.AddWithValue("@Selenium", (object?)item.Selenium ?? DBNull.Value);
                    command.Parameters.AddWithValue("@VitaminB6", (object?)item.VitaminB6 ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Carbohydrate", (object?)item.Carbohydrate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@UnsaturatedFat", (object?)item.UnsaturatedFat ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Chloride", (object?)item.Chloride ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PantothenicAcid", (object?)item.PantothenicAcid ?? DBNull.Value);

                    command.Parameters.AddWithValue("@Chromium", (object?)item.Chromium ?? DBNull.Value);

                    command.Parameters.AddWithValue("@DataUuid", item.DataUuid);
                    command.Parameters.AddWithValue("@PersonID_FK", personId);

                    await command.ExecuteNonQueryAsync();

                    currentCount++;

                    if (currentCount % 100 == 0)
                    {
                        percentage = (double)currentCount / total * 100;
                        progress?.Report((percentage, UiStrings.Status_Database_Updated));
                    }
                }

                await transactionUpdate.CommitAsync();

                #endregion

                #region delete

                int deleteCount = 0;

                foreach (var nutritionID in deleteList)
                {
                    await using var sqlCommand = new MySqlCommand(
                        DatabaseCommands.GetNutritionDeleteSql(),
                        sqlServerConnection);

                    sqlCommand.Parameters.AddWithValue("@Id", nutritionID);

                    await sqlCommand.ExecuteNonQueryAsync();

                    deleteCount++;

                    if (deleteCount % 100 == 0)
                    {
                        percentage = (double)deleteCount / insertsList.Count * 100;
                        progress?.Report((percentage, UiStrings.Status_Database_Deleted));
                    }
                }

                #endregion



                percentage = 100;

                progress?.Report((percentage, UiStrings.Status_Database_Finished));


                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error Insert Nutrition Async", ex);
            }
        }

        /// <summary>
        /// Asynchronously retrieves and returns Samsung nutrition records for a specified person.
        /// </summary>
        /// <param name="personId">The unique identifier of the person whose nutrition records are being requested.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a list of
        /// <see cref="SamsungFoodNutritionModel"/> objects.
        /// </returns>
        public async Task<List<SamsungFoodNutritionModel>> GetSamsungNutritionSqlAsync(int personId)
        {
            var result = new List<SamsungFoodNutritionModel>();

            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected)
                return result;

            try
            {
                var sqlCommand = new MySqlCommand(
                    DatabaseCommands.GetSamsungNutritionSql(),
                    sqlServerConnection);

                sqlCommand.Parameters.AddWithValue("@PersonId", personId);

                await using var reader = await sqlCommand.ExecuteReaderAsync();

                result.Clear();

                while (await reader.ReadAsync())
                {
                    result.Add(new SamsungFoodNutritionModel
                    {
                        FoodNutritionID = reader.GetInt32("food_nutrition_id"),

                        Magnesium = reader.IsDBNull("magnesium") ? (double?)null : reader.GetDouble("magnesium"),
                        Potassium = reader.IsDBNull("potassium") ? (double?)null : reader.GetDouble("potassium"),

                        CreateShVer = reader.IsDBNull("create_sh_ver")
                            ? null
                            : reader.GetString("create_sh_ver"),

                        VitaminA = reader.IsDBNull("vitamin_a") ? (double?)null : reader.GetDouble("vitamin_a"),
                        VitaminC = reader.IsDBNull("vitamin_c") ? (double?)null : reader.GetDouble("vitamin_c"),
                        VitaminD = reader.IsDBNull("vitamin_d") ? (double?)null : reader.GetDouble("vitamin_d"),
                        VitaminE = reader.IsDBNull("vitamin_e") ? (double?)null : reader.GetDouble("vitamin_e"),
                        VitaminK = reader.IsDBNull("vitamin_k") ? (double?)null : reader.GetDouble("vitamin_k"),

                        Cholesterol = reader.IsDBNull("cholesterol") ? (double?)null : reader.GetDouble("cholesterol"),

                        VitaminB12 = reader.IsDBNull("vitamin_b12") ? (double?)null : reader.GetDouble("vitamin_b12"),

                        StartTime = reader.GetDateTime("start_time"),

                        Biotin = reader.IsDBNull("biotin") ? (double?)null : reader.GetDouble("biotin"),
                        Phosphorus = reader.IsDBNull("phosphorus") ? (double?)null : reader.GetDouble("phosphorus"),
                        Copper = reader.IsDBNull("copper") ? (double?)null : reader.GetDouble("copper"),

                        Custom = reader.IsDBNull("custom")
                            ? null
                            : reader.GetString("custom"),

                        Thiamin = reader.IsDBNull("thiamin") ? (double?)null : reader.GetDouble("thiamin"),
                        Folate = reader.IsDBNull("folate") ? (double?)null : reader.GetDouble("folate"),
                        Iodine = reader.IsDBNull("iodine") ? (double?)null : reader.GetDouble("iodine"),
                        Niacin = reader.IsDBNull("niacin") ? (double?)null : reader.GetDouble("niacin"),
                        Sodium = reader.IsDBNull("sodium") ? (double?)null : reader.GetDouble("sodium"),
                        Molybdenum = reader.IsDBNull("molybdenum") ? (double?)null : reader.GetDouble("molybdenum"),
                        FolicAcid = reader.IsDBNull("folic_acid") ? (double?)null : reader.GetDouble("folic_acid"),

                        ModifyShVer = reader.IsDBNull("modify_sh_ver")
                            ? null
                            : reader.GetString("modify_sh_ver"),

                        DietaryFiber = reader.IsDBNull("dietary_fiber") ? (double?)null : reader.GetDouble("dietary_fiber"),
                        TotalFat = reader.IsDBNull("total_fat") ? (double?)null : reader.GetDouble("total_fat"),

                        UpdateTime = reader.GetDateTime("update_time"),

                        CreateTime = reader.GetDateTime("create_time"),

                        MealType = reader.IsDBNull("meal_type")
                            ? (int?)null
                            : reader.GetInt32("meal_type"),

                        Caffeine = reader.IsDBNull("caffeine") ? (double?)null : reader.GetDouble("caffeine"),
                        MonosaturatedFat = reader.IsDBNull("monosaturated_fat") ? (double?)null : reader.GetDouble("monosaturated_fat"),
                        Protein = reader.IsDBNull("protein") ? (double?)null : reader.GetDouble("protein"),
                        Manganese = reader.IsDBNull("manganese") ? (double?)null : reader.GetDouble("manganese"),
                        PolysaturatedFat = reader.IsDBNull("polysaturated_fat") ? (double?)null : reader.GetDouble("polysaturated_fat"),
                        Iron = reader.IsDBNull("iron") ? (double?)null : reader.GetDouble("iron"),
                        Zinc = reader.IsDBNull("zinc") ? (double?)null : reader.GetDouble("zinc"),
                        Riboflavin = reader.IsDBNull("riboflavin") ? (double?)null : reader.GetDouble("riboflavin"),

                        Sugar = reader.IsDBNull("sugar") ? (double?)null : reader.GetDouble("sugar"),

                        Title = reader.IsDBNull("title")
                            ? null
                            : reader.GetString("title"),

                        AddedSugar = reader.IsDBNull("added_sugar") ? (double?)null : reader.GetDouble("added_sugar"),
                        CaloriesFromFat = reader.IsDBNull("calories_from_fat") ? (double?)null : reader.GetDouble("calories_from_fat"),
                        Calcium = reader.IsDBNull("calcium") ? (double?)null : reader.GetDouble("calcium"),
                        Calorie = reader.IsDBNull("calorie") ? (double?)null : reader.GetDouble("calorie"),

                        TimeOffset = reader.IsDBNull("time_offset")
                            ? (string?)null
                            : reader.GetString("time_offset"),

                        DeviceUuid = reader.IsDBNull("deviceuuid")
                            ? null
                            : reader.GetString("deviceuuid"),

                        PolyunsaturatedFat = reader.IsDBNull("polyunsaturated_fat")
                            ? (double?)null
                            : reader.GetDouble("polyunsaturated_fat"),

                        SaturatedFat = reader.IsDBNull("saturated_fat")
                            ? (double?)null
                            : reader.GetDouble("saturated_fat"),

                        TransFat = reader.IsDBNull("trans_fat")
                            ? (double?)null
                            : reader.GetDouble("trans_fat"),

                        PkgName = reader.IsDBNull("pkg_name")
                            ? null
                            : reader.GetString("pkg_name"),

                        Selenium = reader.IsDBNull("selenium")
                            ? (double?)null
                            : reader.GetDouble("selenium"),

                        VitaminB6 = reader.IsDBNull("vitamin_b6")
                            ? (double?)null
                            : reader.GetDouble("vitamin_b6"),

                        Carbohydrate = reader.IsDBNull("carbohydrate")
                            ? (double?)null
                            : reader.GetDouble("carbohydrate"),

                        UnsaturatedFat = reader.IsDBNull("unsaturated_fat")
                            ? (double?)null
                            : reader.GetDouble("unsaturated_fat"),

                        Chloride = reader.IsDBNull("chloride")
                            ? (double?)null
                            : reader.GetDouble("chloride"),

                        PantothenicAcid = reader.IsDBNull("pantothenic_acid")
                            ? (double?)null
                            : reader.GetDouble("pantothenic_acid"),

                        DataUuid = reader.GetString("datauuid"),

                        Chromium = reader.IsDBNull("chromium")
                            ? (double?)null
                            : reader.GetDouble("chromium")
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error Get Nutrition Async", ex);
            }

            return result;
        }

        /// <summary>
        /// Asynchronously deletes a specific nutrition record from the database using its unique identifier.
        /// </summary>
        /// <param name="nutritionId">
        /// The primary key (ID) of the nutrition record to be removed.
        /// </param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GetNutritionDeleteSqlAsync(int nutritionId)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected)
                return;

            try
            {
                var sqlCommand = new MySqlCommand(
                    DatabaseCommands.GetNutritionDeleteSql(),
                    sqlServerConnection);

                sqlCommand.Parameters.AddWithValue("@id", nutritionId);

                await sqlCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error Delete Nutrition Async: {ex.Message}", ex);
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
                await tx.RollbackAsync();
                throw new Exception($"Error Insert Exercises: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Asynchronously synchronizes a collection of Samsung Health exercise records for a specific person into the database, 
        /// performing bulk insertions for new entries and transactional updatesList for existing records based on unique data identifiers and update timestamps.
        /// </summary>
        /// <param name="personId">The unique identifier of the person associated with the exercise records.</param>
        /// <param name="exerciseList">The collection of Samsung exercise records to synchronize.</param>
        /// <param name="progress">An optional progress reporter for tracking completion percentages and status messages.</param>
        /// <returns>True if successful; otherwise false.</returns>
        public async Task<bool> UpsertExerciseSqlAsync(
            int personId,
            ICollection<SamsungExerciseModel> exerciseList,
            IProgress<(double value, string prgressText)>? progress = null)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected)
                return false;

            try
            {
                var existingEntries = new Dictionary<string, (
                    int ExerciseId,
                    string Uuid,
                    DateTime? UpdateTime)>();

                await using var cmd = new MySqlCommand(
                    DatabaseCommands.GetSamsungExerciseExistenceCheckSql(),
                    sqlServerConnection);

                cmd.Parameters.AddWithValue("@PersonId", personId);

                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        existingEntries.Add(
                            reader.GetString("data_uuid"),
                            (
                                reader.GetInt32("exercise_id"),
                                reader.GetString("data_uuid"),
                                reader.IsDBNull("update_at")
                                    ? null
                                    : reader.GetDateTime("update_at")
                            ));
                    }
                }

                var insertsList = new List<SamsungExerciseModel>();
                var updatesList = new List<SamsungExerciseModel>();
                var deleteList = new List<int>();

                double percentage = 0;

                foreach (var item in exerciseList)
                {
                    if (string.IsNullOrWhiteSpace(item.DataUuid))
                        continue;

                    if (!existingEntries.TryGetValue(item.DataUuid, out var dbEntry))
                    {
                        insertsList.Add(item);
                        continue;
                    }

                    if (item.UpdateTime.HasValue &&
                        (!dbEntry.UpdateTime.HasValue ||
                         item.UpdateTime > dbEntry.UpdateTime))
                    {
                        updatesList.Add(item);
                    }
                }

                // Retrieve entries for deleting
                foreach (var dbEntry in existingEntries.Values)
                {
                    if (!exerciseList.Any(x => x.DataUuid == dbEntry.Uuid))
                    {
                        deleteList.Add(dbEntry.ExerciseId);
                    }
                }

                #region Inserts

                await using var transactionInsert =
                    await sqlServerConnection.BeginTransactionAsync();

                if (insertsList.Count > 0)
                {
                    var table =
                        SamsungExerciseDataTable.CreateExerciseDataTable(
                            personId,
                            insertsList);

                    await using var commandInsert = new MySqlCommand(
                        DatabaseCommands.GetSamsungExerciseUpsertSql(),
                        sqlServerConnection,
                        (MySqlTransaction)transactionInsert);

                    foreach (DataColumn column in table.Columns)
                    {
                        commandInsert.Parameters.Add(
                            new MySqlParameter(
                                "@" + column.ColumnName,
                                null));
                    }

                    commandInsert.Prepare();

                    int insertCount = 0;

                    foreach (DataRow row in table.Rows)
                    {
                        foreach (DataColumn column in table.Columns)
                        {
                            commandInsert.Parameters["@" + column.ColumnName].Value =
                                row[column] ?? DBNull.Value;
                        }

                        await commandInsert.ExecuteNonQueryAsync();

                        insertCount++;

                        if (insertCount % 100 == 0)
                        {
                            percentage =
                                (double)insertCount / insertsList.Count * 100;

                            progress?.Report(
                                (percentage,
                                 UiStrings.Status_Database_Inserted));
                        }
                    }
                }

                await transactionInsert.CommitAsync();

                #endregion

                #region Updates

                int currentCount = 0;
                int total = updatesList.Count;

                await using var transactionUpdate =
                    await sqlServerConnection.BeginTransactionAsync();

                foreach (var item in updatesList)
                {
                    await using var command = new MySqlCommand(
                        DatabaseCommands.GetExerciseUpdateSql(),
                        sqlServerConnection,
                        (MySqlTransaction)transactionUpdate);

                    command.Parameters.AddWithValue("@LiveDataInternal", (object?)item.LiveDataInternal ?? DBNull.Value);

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

                    await command.ExecuteNonQueryAsync();

                    currentCount++;

                    if (currentCount % 100 == 0)
                    {
                        percentage =
                            (double)currentCount / total * 100;

                        progress?.Report(
                            (percentage,
                             UiStrings.Status_Database_Updated));
                    }
                }

                await transactionUpdate.CommitAsync();

                #endregion

                #region Delete

                int deleteCount = 0;

                await using var transactionDelete =
                    await sqlServerConnection.BeginTransactionAsync();

                foreach (var exerciseId in deleteList)
                {
                    await using var command = new MySqlCommand(
                        DatabaseCommands.GetSamsungExerciseDeleteSql(),
                        sqlServerConnection,
                        (MySqlTransaction)transactionDelete);

                    command.Parameters.AddWithValue("@id", exerciseId);

                    await command.ExecuteNonQueryAsync();

                    deleteCount++;

                    if (deleteCount % 100 == 0)
                    {
                        percentage =
                            (double)deleteCount / deleteList.Count * 100;

                        progress?.Report(
                            (percentage,
                             UiStrings.Status_Database_Deleted));
                    }
                }

                await transactionDelete.CommitAsync();

                #endregion

                percentage = 100;

                progress?.Report(
                    (percentage,
                     UiStrings.Status_Database_Finished));

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error Insert Samsung Exercises", ex);
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
                throw new Exception($"Error Get Exercise Async: {ex.Message}", ex);
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
                throw new Exception($"Error Get Samsung Exercise Async: {ex.Message}", ex);
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
                throw new Exception($"Error Delete Exercise Async: {ex.Message}", ex);
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
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                throw new Exception($"Error Insert Heart Rates: {ex.Message}", ex);

            }
        }

        /// <summary>
        /// Asynchronously synchronizes a collection of Samsung Health Heart Rate records for a specific person into the database, 
        /// performing bulk insertions for new entries and transactional updatesList for existing records based on unique data identifiers and update timestamps.
        /// </summary>
        /// <param name="personId">The unique identifier of the person.</param>
        /// <param name="heartRates">The heart rate records to store.</param>
        /// <param name="progress">Optional progress reporter.</param>
        /// <returns>True if the operation succeeds; otherwise false.</returns>
        public async Task<bool> UpsertHeartRatesSqlAsync(
          int personId,
          ICollection<SamsungHeartRateModel> heartRates,
          IProgress<(double value, string prgressText)>? progress = null)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected)
                return false;

            try
            {
                var existingEntries = new Dictionary<string, (
                    int HeartRateId,
                    string Uuid,
                    DateTime? UpdateTime)>();

                await using var cmd = new MySqlCommand(
                    DatabaseCommands.GetHeartRateExistenceCheckSql(),
                    sqlServerConnection);

                cmd.Parameters.AddWithValue("@PersonId", personId);

                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        existingEntries.Add(
                            reader.GetString("data_uuid"),
                            (
                                reader.GetInt32("heart_rate_id"),
                                reader.GetString("data_uuid"),
                                reader.IsDBNull("update_time")
                                    ? null
                                    : reader.GetDateTime("update_time")
                            ));
                    }
                }

                var insertsList = new List<SamsungHeartRateModel>();
                var updatesList = new List<SamsungHeartRateModel>();
                var deleteList = new List<int>();

                double percentage = 0;

                foreach (var item in heartRates)
                {
                    if (string.IsNullOrWhiteSpace(item.DataUuid))
                        continue;

                    if (!existingEntries.TryGetValue(item.DataUuid, out var dbEntry))
                    {
                        insertsList.Add(item);
                        continue;
                    }

                    if (item.UpdateTime.HasValue &&
                        (!dbEntry.UpdateTime.HasValue ||
                         item.UpdateTime > dbEntry.UpdateTime))
                    {
                        updatesList.Add(item);
                    }
                }

                // Retrieve entries for deleting
                foreach (var dbEntry in existingEntries.Values)
                {
                    if (!heartRates.Any(x => x.DataUuid == dbEntry.Uuid))
                    {
                        deleteList.Add(dbEntry.HeartRateId);
                    }
                }

                #region Inserts

                await using var transactionInsert =
                    await sqlServerConnection.BeginTransactionAsync();

                if (insertsList.Count > 0)
                {
                    var table =
                        SamsungHeartRateDataTable.CreateHeartRateDataTable(
                            personId,
                            insertsList);

                    await using var commandInsert = new MySqlCommand(
                        DatabaseCommands.GetSamsungHeartRateUpsertSql(),
                        sqlServerConnection,
                        (MySqlTransaction)transactionInsert);

                    foreach (DataColumn column in table.Columns)
                    {
                        commandInsert.Parameters.Add(
                            new MySqlParameter(
                                "@" + column.ColumnName,
                                null));
                    }

                    commandInsert.Prepare();

                    int insertCount = 0;

                    foreach (DataRow row in table.Rows)
                    {
                        foreach (DataColumn column in table.Columns)
                        {
                            commandInsert.Parameters["@" + column.ColumnName].Value =
                                row[column] ?? DBNull.Value;
                        }

                        await commandInsert.ExecuteNonQueryAsync();

                        insertCount++;

                        if (insertCount % 100 == 0)
                        {
                            percentage =
                                (double)insertCount / insertsList.Count * 100;

                            progress?.Report(
                                (percentage,
                                 UiStrings.Status_Database_Inserted));
                        }
                    }
                }

                await transactionInsert.CommitAsync();

                #endregion

                #region Updates

                int currentCount = 0;
                int total = updatesList.Count;

                await using var transactionUpdate =
                    await sqlServerConnection.BeginTransactionAsync();

                foreach (var item in updatesList)
                {
                    await using var command = new MySqlCommand(
                        DatabaseCommands.GetHeartRateUpdateSql(),
                        sqlServerConnection,
                        (MySqlTransaction)transactionUpdate);

                    command.Parameters.AddWithValue("@Source", (object?)item.Source ?? DBNull.Value);
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

                    await command.ExecuteNonQueryAsync();

                    currentCount++;

                    if (currentCount % 100 == 0)
                    {
                        percentage =
                            (double)currentCount / total * 100;

                        progress?.Report(
                            (percentage,
                             UiStrings.Status_Database_Updated));
                    }
                }

                await transactionUpdate.CommitAsync();

                #endregion

                #region Delete

                int deleteCount = 0;

                await using var transactionDelete =
                    await sqlServerConnection.BeginTransactionAsync();

                foreach (var heartRateId in deleteList)
                {
                    await using var command = new MySqlCommand(
                        DatabaseCommands.GetSamsungHeartRateDeleteSql(),
                        sqlServerConnection,
                        (MySqlTransaction)transactionDelete);

                    command.Parameters.AddWithValue("@id", heartRateId);

                    await command.ExecuteNonQueryAsync();

                    deleteCount++;

                    if (deleteCount % 100 == 0)
                    {
                        percentage =
                            (double)deleteCount / deleteList.Count * 100;

                        progress?.Report(
                            (percentage,
                             UiStrings.Status_Database_Deleted));
                    }
                }

                await transactionDelete.CommitAsync();

                #endregion

                percentage = 100;

                progress?.Report(
                    (percentage,
                     UiStrings.Status_Database_Finished));

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error inserting Samsung Heart Rates",
                    ex);
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
                throw new Exception($"Error Get Samsung Heart Rate Async: {ex.Message}", ex);
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
                throw new Exception($"Error Get Exercise Async: {ex.Message}", ex);
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
                throw new Exception($"Error Delete Heart Rate Async: {ex.Message}", ex);
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
                throw new Exception($"Error inserting Samsung Health Daily Steps Trend: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Asynchronously synchronizes a collection of Samsung Health Step Trend records for a specific person into the database, 
        /// performing bulk insertions for new entries and transactional updatesList for existing records based on unique data identifiers and update timestamps.
        /// </summary>
        /// <param name="PersonId">The unique identifier of the person to whom the data belongs.</param>
        /// <param name="stepDailyTrendList">A collection of <see cref="SamsungStepTrendModel"/> objects containing the step trend data to be stored.</param>
        /// <param name="progress">An optional progress indicator that reports the percentage of the insertion process.</param>
        /// <returns>Returns 'true' if the operation completes successfully.</returns>
        public async Task<bool> UpsertStepTrendSqlAsync(
         int personId,
         ICollection<SamsungStepTrendModel> stepDailyTrendList,
         IProgress<(double value, string prgressText)>? progress = null)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected)
                return false;

            try
            {
                var existingEntries = new Dictionary<string, (
                    int StepTrendId,
                    string Uuid,
                    DateTime? UpdateTime)>();

                await using var cmd = new MySqlCommand(
                    DatabaseCommands.GetSamsungStepTrendExistenceCheckSql(),
                    sqlServerConnection);

                cmd.Parameters.AddWithValue("@PersonId", personId);

                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        existingEntries.Add(
                            reader.GetString("data_uuid"),
                            (
                                reader.GetInt32("step_trend_id"),
                                reader.GetString("data_uuid"),
                                reader.IsDBNull("update_at")
                                    ? null
                                    : reader.GetDateTime("update_at")
                            ));
                    }
                }

                var insertsList = new List<SamsungStepTrendModel>();
                var updatesList = new List<SamsungStepTrendModel>();
                var deleteList = new List<int>();

                double percentage = 0;

                foreach (var item in stepDailyTrendList)
                {
                    var uuid = item.DataUuid?.Trim();

                    if (string.IsNullOrWhiteSpace(uuid))
                        continue;

                    if (!existingEntries.TryGetValue(uuid, out var dbEntry))
                    {
                        insertsList.Add(item);
                        continue;
                    }

                    if (item.UpdateTime.HasValue &&
                        (!dbEntry.UpdateTime.HasValue ||
                         item.UpdateTime > dbEntry.UpdateTime))
                    {
                        updatesList.Add(item);
                    }
                }

                // Retrieve entries for deleting
                foreach (var dbEntry in existingEntries.Values)
                {
                    if (!stepDailyTrendList.Any(x => x.DataUuid == dbEntry.Uuid))
                    {
                        deleteList.Add(dbEntry.StepTrendId);
                    }
                }

                #region Inserts

                await using var transactionInsert =
                    await sqlServerConnection.BeginTransactionAsync();

                if (insertsList.Count > 0)
                {
                    var table =
                        SamsungHealthStepDailyTrendDataTable
                            .CreateStepDailyTrendDataTable(
                                personId,
                                insertsList);

                    await using var commandInsert = new MySqlCommand(
                        DatabaseCommands.GetSamsungStepTrendUpsertSql(),
                        sqlServerConnection,
                        (MySqlTransaction)transactionInsert);

                    foreach (DataColumn column in table.Columns)
                    {
                        commandInsert.Parameters.Add(
                            new MySqlParameter(
                                "@" + column.ColumnName,
                                null));
                    }

                    commandInsert.Prepare();

                    int insertCount = 0;

                    foreach (DataRow row in table.Rows)
                    {
                        foreach (DataColumn column in table.Columns)
                        {
                            commandInsert.Parameters["@" + column.ColumnName].Value =
                                row[column] ?? DBNull.Value;
                        }

                        await commandInsert.ExecuteNonQueryAsync();

                        insertCount++;

                        if (insertCount % 100 == 0)
                        {
                            percentage =
                                (double)insertCount / insertsList.Count * 100;

                            progress?.Report(
                                (
                                    percentage,
                                    UiStrings.Status_Database_Inserted
                                ));
                        }
                    }
                }

                await transactionInsert.CommitAsync();

                #endregion

                #region Updates

                int currentCount = 0;
                int total = updatesList.Count;

                await using var transactionUpdate =
                    await sqlServerConnection.BeginTransactionAsync();

                foreach (var item in updatesList)
                {
                    await using var command = new MySqlCommand(
                        DatabaseCommands.GetSamsungStepTrendUpdateSql(),
                        sqlServerConnection,
                        (MySqlTransaction)transactionUpdate);

                    command.Parameters.AddWithValue("@PersonID_FK", personId);

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

                    await command.ExecuteNonQueryAsync();

                    currentCount++;

                    if (currentCount % 100 == 0)
                    {
                        percentage =
                            (double)currentCount / total * 100;

                        progress?.Report(
                            (
                                percentage,
                                UiStrings.Status_Database_Updated
                            ));
                    }
                }

                await transactionUpdate.CommitAsync();

                #endregion

                #region Delete

                int deleteCount = 0;

                await using var transactionDelete =
                    await sqlServerConnection.BeginTransactionAsync();

                foreach (var stepTrendId in deleteList)
                {
                    await using var command = new MySqlCommand(
                        DatabaseCommands.GetSamsungStepTrendDeleteSql(),
                        sqlServerConnection,
                        (MySqlTransaction)transactionDelete);

                    command.Parameters.AddWithValue("@id", stepTrendId);

                    await command.ExecuteNonQueryAsync();

                    deleteCount++;

                    if (deleteCount % 100 == 0)
                    {
                        percentage =
                            (double)deleteCount / deleteList.Count * 100;

                        progress?.Report(
                            (
                                percentage,
                                UiStrings.Status_Database_Deleted
                            ));
                    }
                }

                await transactionDelete.CommitAsync();

                #endregion

                percentage = 100;

                progress?.Report(
                    (
                        percentage,
                        UiStrings.Status_Database_Finished
                    ));

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error Insert Step Trend Datas", ex);
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
                throw new Exception($"Error Get Step Trend Async: {ex.Message}", ex);
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
                throw new Exception($"Error Get Step Daily Trend Async: {ex.Message}", ex);
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
                throw new Exception($"Error Delete Step Trend Async: {ex.Message}", ex);
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
                await tx.RollbackAsync();
                throw new Exception($"Error Insert SpO2 Datas: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Asynchronously synchronizes a collection of Samsung Health Step Trend records for a specific person into the database, 
        /// performing bulk insertions for new entries and transactional updatesList for existing records based on unique data identifiers and update timestamps.
        /// </summary>
        /// <param name="PersonId">The unique identifier of the person to whom the data belongs.</param>
        /// <param name="stepDailyTrendList">A collection of <see cref="SamsungStepTrendModel"/> objects containing the step trend data to be stored.</param>
        /// <param name="progress">An optional progress indicator that reports the percentage of the insertion process.</param>
        /// <returns>Returns 'true' if the operation completes successfully.</returns>
        public async Task<bool> UpsertSpO2SqlAsync(
          int personId,
          ICollection<SamsungOxygenSaturationModel> oxygenSaturation,
          IProgress<(double value, string prgressText)>? progress = null)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected)
                return false;

            try
            {
                var existingEntries = new Dictionary<string, (
                    int OxygenSaturationId,
                    string Uuid,
                    DateTime? UpdateTime)>();

                await using var cmd = new MySqlCommand(
                    DatabaseCommands.GetSamsungOxygenSaturationExistenceCheckSql(),
                    sqlServerConnection);

                cmd.Parameters.AddWithValue("@PersonId", personId);

                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        existingEntries.Add(
                            reader.GetString("data_uuid"),
                            (
                                reader.GetInt32("oxygen_saturation_id"),
                                reader.GetString("data_uuid"),
                                reader.IsDBNull("update_time")
                                    ? null
                                    : reader.GetDateTime("update_time")
                            ));
                    }
                }

                var insertsList = new List<SamsungOxygenSaturationModel>();
                var updatesList = new List<SamsungOxygenSaturationModel>();
                var deleteList = new List<int>();

                double percentage = 0;

                foreach (var item in oxygenSaturation)
                {
                    var uuid = item.DataUuid?.Trim();

                    if (string.IsNullOrWhiteSpace(uuid))
                        continue;

                    if (!existingEntries.TryGetValue(uuid, out var dbEntry))
                    {
                        insertsList.Add(item);
                        continue;
                    }

                    if (item.UpdateTime.HasValue &&
                        (!dbEntry.UpdateTime.HasValue ||
                         item.UpdateTime > dbEntry.UpdateTime))
                    {
                        updatesList.Add(item);
                    }
                }

                // Retrieve entries for deleting
                foreach (var dbEntry in existingEntries.Values)
                {
                    if (!oxygenSaturation.Any(x => x.DataUuid == dbEntry.Uuid))
                    {
                        deleteList.Add(dbEntry.OxygenSaturationId);
                    }
                }

                #region Inserts

                await using var transactionInsert =
                    await sqlServerConnection.BeginTransactionAsync();

                if (insertsList.Count > 0)
                {
                    var table =
                        SamsungHealthOxygenDataTable.CreateOxygenDataTable(
                            personId,
                            insertsList);

                    await using var commandInsert = new MySqlCommand(
                        DatabaseCommands.GetSamsungOxygenSaturationUpsertSql(),
                        sqlServerConnection,
                        (MySqlTransaction)transactionInsert);

                    foreach (DataColumn column in table.Columns)
                    {
                        commandInsert.Parameters.Add(
                            new MySqlParameter(
                                "@" + column.ColumnName,
                                null));
                    }

                    commandInsert.Prepare();

                    int insertCount = 0;

                    foreach (DataRow row in table.Rows)
                    {
                        foreach (DataColumn column in table.Columns)
                        {
                            commandInsert.Parameters["@" + column.ColumnName].Value =
                                row[column] ?? DBNull.Value;
                        }

                        await commandInsert.ExecuteNonQueryAsync();

                        insertCount++;

                        if (insertCount % 100 == 0)
                        {
                            percentage =
                                (double)insertCount / insertsList.Count * 100;

                            progress?.Report(
                                (
                                    percentage,
                                    UiStrings.Status_Database_Inserted
                                ));
                        }
                    }
                }

                await transactionInsert.CommitAsync();

                #endregion

                #region Updates

                int currentCount = 0;
                int total = updatesList.Count;

                await using var transactionUpdate =
                    await sqlServerConnection.BeginTransactionAsync();

                foreach (var item in updatesList)
                {
                    await using var command = new MySqlCommand(
                        DatabaseCommands.GetSamsungOxygenSaturationUpdateSql(),
                        sqlServerConnection,
                        (MySqlTransaction)transactionUpdate);

                    command.Parameters.AddWithValue("@PersonId", personId);

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

                    await command.ExecuteNonQueryAsync();

                    currentCount++;

                    if (currentCount % 100 == 0)
                    {
                        percentage =
                            (double)currentCount / total * 100;

                        progress?.Report(
                            (
                                percentage,
                                UiStrings.Status_Database_Updated
                            ));
                    }
                }

                await transactionUpdate.CommitAsync();

                #endregion

                #region Delete

                int deleteCount = 0;

                await using var transactionDelete =
                    await sqlServerConnection.BeginTransactionAsync();

                foreach (var oxygenSaturationId in deleteList)
                {
                    await using var command = new MySqlCommand(
                        DatabaseCommands.GetOxygenSaturationDeleteSql(),
                        sqlServerConnection,
                        (MySqlTransaction)transactionDelete);

                    command.Parameters.AddWithValue("@id", oxygenSaturationId);

                    await command.ExecuteNonQueryAsync();

                    deleteCount++;

                    if (deleteCount % 100 == 0)
                    {
                        percentage =
                            (double)deleteCount / deleteList.Count * 100;

                        progress?.Report(
                            (
                                percentage,
                                UiStrings.Status_Database_Deleted
                            ));
                    }
                }

                await transactionDelete.CommitAsync();

                #endregion

                percentage = 100;

                progress?.Report(
                    (
                        percentage,
                        UiStrings.Status_Database_Finished
                    ));

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error Insert SpO2 Datas", ex);
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
                throw new Exception($"Error Get SpO2 Datas: {ex.Message}", ex);
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
                throw new Exception($"Error Get SpO2 Async: {ex.Message}", ex);
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
                throw new Exception($"Error Delete SpO2 Async: {ex.Message}", ex);
            }
        }

        #endregion

        #region Workout Log

        /// <summary>
        /// Inserts Hevy App workout data into the database.
        /// Existing records are skipped based on DataUuid.
        /// </summary>
        /// <param name="personId">ID of the selected person.</param>
        /// <param name="wokoutDatas">Hevy App workout data.</param>
        /// <param name="progress">Optional progress reporting.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> UpsertWorkoutLogAsync(int personId,
            IEnumerable<HevyAppCSVModel> wokoutDatas, IProgress<(double value, string prgressText)>? progress = null)
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
                var csvEntries = new Dictionary<string, HevyAppCSVModel>();

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

                var insertList = new List<HevyAppCSVModel>();
                var updateList = new List<(int WorkoutId, string Hash, HevyAppCSVModel Workout)>();
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

                    progress?.Report((percentage, UiStrings.Status_Database_Inserted));

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

                    progress?.Report((percentage, UiStrings.Status_Database_Updated));
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

                    progress?.Report((percentage, UiStrings.Status_Database_Deleted));
                }

                await tx.CommitAsync();

                percentage = 100;

                progress?.Report((percentage, UiStrings.Status_Database_Finished));

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error Workout Log Datas: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Inserts Hevy App workout data into the database.
        /// Existing records are skipped based on DataUuid.
        /// </summary>
        /// <param name="personId">ID of the selected person.</param>
        /// <param name="hevyAppData">Hevy App workout data.</param>
        /// <param name="progress">Optional progress reporting.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> AddWorkoutLogAsync(int personId,
                                                   IEnumerable<HevyAppCSVModel> hevyAppData,
                                                   IProgress<double>? progress = null)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return false;

            await using var tx = await sqlServerConnection.BeginTransactionAsync();

            try
            {
                var workouts = hevyAppData.ToList();

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
                throw new Exception($"Error Insert Workout Log Datas: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Asynchronously synchronizes a collection of Samsung workout records for a specific person into the database, 
        /// performing bulk insertions for new entries and transactional updatesList for existing records based on unique data identifiers and update timestamps.
        /// </summary>
        /// <param name="personId">ID of the selected person.</param>
        /// <param name="wokoutDatas">Hevy App workout data.</param>
        /// <param name="progress">Optional progress reporting.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> SyncWorkoutLogAsync(int personId, ICollection<HevyAppCSVModel> wokoutDatas,
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
                var csvEntries = new Dictionary<string, HevyAppCSVModel>();

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



                var insertList = new List<HevyAppCSVModel>();
                var updateList = new List<(int WorkoutId, string Hash, HevyAppCSVModel Workout)>();
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

                if (insertList.Count > 0)
                {
                    var table = HevyAppDataTable.CreateHevyAppDataTable(personId, insertList);

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

                    progress?.Report((percentage, UiStrings.Status_Database_Updated));
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

                    progress?.Report((percentage, UiStrings.Status_Database_Deleted));
                }

                await tx.CommitAsync();

                percentage = 100;

                progress?.Report((percentage, UiStrings.Status_Database_Finished));

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error Insert Workload Datas: {ex.Message}", ex);
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
        public async Task<List<GymWorkoutEntryModel>> GetHevyAppWorkoutsAsync(int personId)
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
                throw new Exception($"Error Get Hevy App Workout Entries Async: {ex.Message}", ex);
            }

            return list;
        }

        /// <summary>
        /// Asynchronously deletes a specific heart rate record from the database using its unique data identifier.
        /// </summary>
        /// <param name="heartRateDataUuid">The unique data identifier (UUID) of the heart rate record to be removed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteHevyAppAsync(int hevyAppID)
        {
            await using var sqlServerConnection = await OpenConnectionAsync();

            if (!IsConnected) return;

            try
            {
                var sqlCommand = new MySqlCommand(DatabaseCommands.GetWorkoutLogDeleteByWorkoutIdSql(), sqlServerConnection);
                sqlCommand.Parameters.AddWithValue("@id", hevyAppID);
                await sqlCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error Delete Hevy App Async: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lädt HevyApp Trainingsdaten für eine Person.
        /// </summary>
        /// <param name="personId">Personen-ID.</param>
        /// <returns>Liste der Trainingseinträge.</returns>
        public async Task<List<HevyAppCSVModel>> GetHevyAppAsync(int personId)
        {
            var result = new List<HevyAppCSVModel>();

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
                    result.Add(new HevyAppCSVModel
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
                throw new Exception($"Error Get HevyApp Async: {ex.Message}", ex);
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
                throw new Exception($"Error Get Person: {ex.Message}", ex);
            }

            return list;
        }

        /// <summary>
        /// Asynchronously retrieves the total number of records stored in the 'tbl_Persons' table.
        /// </summary>
        /// <remarks>
        /// This method utilizes <see cref="MySqlCommand.ExecuteScalarAsync"/> to efficiently fetch the insertCount result. 
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

                // Validation: If the result is null (DBNull), insertCount is set to 0 to avoid exceptions
                int count = result != null ? Convert.ToInt32(result) : 0;

                // Return logic: Ensures the method never returns negative values, maintaining UI consistency
                return count > 0 ? count : 0;
            }

            catch (Exception ex)
            {
                throw new Exception($"Error Get Person StepCount: {ex.Message}", ex);
            }
        }

        #endregion

    }
}

