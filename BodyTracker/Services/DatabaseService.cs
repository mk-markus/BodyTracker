using BodyTracker.MVVM.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace BodyTracker.Services
{
    /// <summary>
    /// Orchestrates all database interactions for the application. 
    /// It manages the connection lifecycle and utilizes a command provider to execute 
    /// operations against the MySQL database.
    /// </summary>
    public partial class DatabaseService : ObservableObject
    {
        #region Members
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
        /// Gets or sets a value indicating whether the server connection is established successfully.
        /// </summary>
        [ObservableProperty] public bool serverConnectionOK;

        #endregion

        #region Constructor

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

        #endregion

        #region Default Database Functions

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
                if (SqlServerConnection.State == ConnectionState.Open) ServerConnectionOK = true;
                else ServerConnectionOK = false;

            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }

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
        public async Task<int> CreatePersonAsync(string FirstName, string LastName, DateTime? BirthDate, double Height)
        {
            var SqlServerConnection = await EtablishSqlServerConnection();
            await using var SqlCommand = new MySqlCommand(DatabaseCommands.CmdCreatePerson(), SqlServerConnection);
            SqlCommand.Parameters.AddWithValue("@v", FirstName);
            SqlCommand.Parameters.AddWithValue("@n", LastName);
            SqlCommand.Parameters.AddWithValue("@g", BirthDate.HasValue ? BirthDate.Value : (object)DBNull.Value);
            SqlCommand.Parameters.AddWithValue("@k", Height);
            var idObj = await SqlCommand.ExecuteScalarAsync();
            return Convert.ToInt32(idObj);
        }

        /// <summary>
        /// Determines whether a measurement exists for a specified person on a given date.
        /// </summary>
        /// <param name="dateToCheck">The date to check for an existing measurement.</param>
        /// <param name="PersonId">The identifier of the person whose measurements are being checked.</param>
        /// <returns>true if a measurement exists for the specified date and person; otherwise, false.</returns>
        public async Task<bool> IsMeasurementExistingAsync(DateTime dateToCheck, int PersonId)
        {
            var SqlServerConnection = await EtablishSqlServerConnection();

            // Wir nutzen DATE(@TargetDate), um nur den Kalendertag zu vergleichen
            // Die SQL-Abfrage sollte lauten: 
            // "SELECT 1 FROM measurements WHERE DATE(MeasurementDate) = DATE(@TargetDate) LIMIT 1"
            await using var SqlCommand = new MySqlCommand(DatabaseCommands.CmdCheckIfPersonHasMeasurementsExists(), SqlServerConnection);

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

            return false;
        }

        #endregion

        #region Functions Insert Datas to Database

        /// <summary>
        /// Asynchronously inserts a new body metric record into the database.
        /// This includes physiological data such as weight, BMI, and body fat percentages.
        /// </summary>
        /// <param name="m">The <see cref="BodyMetricModel"/> containing the data to be stored.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InsertBodyMetricAsync(BodyMetricModel bodyMetricModel)
        {

            DateTime finalDate = bodyMetricModel.MeasurementDate.Date.Add(DateTime.Now.TimeOfDay);

            var SqlServerConnection = await EtablishSqlServerConnection();
            await using var SqlCommand = new MySqlCommand(DatabaseCommands.CmdInsertPersonMetric(), SqlServerConnection);

            SqlCommand.Parameters.AddWithValue("@pid", bodyMetricModel.PersonID);
            SqlCommand.Parameters.Add("@dt", (DbType)SqlDbType.DateTime2).Value = finalDate;
            SqlCommand.Parameters.AddWithValue("@gw", (object?)bodyMetricModel.BodyWeight ?? DBNull.Value);
            SqlCommand.Parameters.AddWithValue("@bmi", (object?)bodyMetricModel.BMI ?? DBNull.Value);
            SqlCommand.Parameters.AddWithValue("@kf", (object?)bodyMetricModel.BodyFatPercentage ?? DBNull.Value);
            SqlCommand.Parameters.AddWithValue("@kfo", (object?)bodyMetricModel.BodyFatPercentageTop ?? DBNull.Value);
            SqlCommand.Parameters.AddWithValue("@kfu", (object?)bodyMetricModel.BodyFatPercentageBottom ?? DBNull.Value);
            SqlCommand.Parameters.AddWithValue("@mm", (object?)bodyMetricModel.BodyMusclePercentage ?? DBNull.Value);
            SqlCommand.Parameters.AddWithValue("@mmo", (object?)bodyMetricModel.BodyMusclePercentageTop ?? DBNull.Value);
            SqlCommand.Parameters.AddWithValue("@mmu", (object?)bodyMetricModel.BodyMusclePercentageBottom ?? DBNull.Value);
            SqlCommand.Parameters.AddWithValue("@kw", (object?)bodyMetricModel.BodyWaterPercentage ?? DBNull.Value);
            SqlCommand.Parameters.AddWithValue("@kk", (object?)bodyMetricModel.BodyBoneMass ?? DBNull.Value);
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
            SqlCommand.Parameters.AddWithValue("@fzbreast", (object?)bodyDemensionModel.FatTongBreastCrease ?? DBNull.Value);
            SqlCommand.Parameters.AddWithValue("@fzarmpit", (object?)bodyDemensionModel.FatTongArmpitCrease ?? DBNull.Value);
            SqlCommand.Parameters.AddWithValue("@fzabdomen", (object?)bodyDemensionModel.FatTongAbdominalCrease ?? DBNull.Value);
            SqlCommand.Parameters.AddWithValue("@fzhip", (object?)bodyDemensionModel.FatTongHipCrease ?? DBNull.Value);
            SqlCommand.Parameters.AddWithValue("@fzthigh", (object?)bodyDemensionModel.FatTongThighCrease ?? DBNull.Value);
            SqlCommand.Parameters.AddWithValue("@fzback", (object?)bodyDemensionModel.FatTongBackCrease ?? DBNull.Value);
            SqlCommand.Parameters.AddWithValue("@fztricep", (object?)bodyDemensionModel.FatTongTricepsCrease ?? DBNull.Value);

            await SqlCommand.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Inserts a collection of Samsung Health food intake records for a specific person into the database.
        /// Utilizes a database transaction to ensure data integrity; performs a rollback in case of failure.
        /// </summary>
        /// <param name="PersonId">The unique identifier of the person to whom the data belongs.</param>
        /// <param name="foodIntakeList">A collection of <see cref="FoodIntakeModel"/> objects containing the nutritional data to be stored.</param>
        /// <param name="progress">An optional progress indicator that reports the percentage of the insertion process.</param>
        /// <returns>Returns 'true' if the operation completes successfully.</returns>
        /// <exception cref="Exception">Throws an exception if the database operation fails, triggering an automatic rollback.</exception>
        public async Task<bool> InsertSamsungHealthFoodIntake(int PersonId, ICollection<FoodIntakeModel> foodIntakeList, IProgress<double>? progress = null)
        {
            var sqlServerConnection = await EtablishSqlServerConnection();

            await using var tx = await sqlServerConnection.BeginTransactionAsync();

            try
            {
                int total = foodIntakeList.Count;
                int current = 0;

                foreach (var foodIntake in foodIntakeList)
                {
                    current++;

                    await using var sqlCommand = new MySqlCommand(DatabaseCommands.CmdInsertSamsungHealthFoodIntake(),
                                                                sqlServerConnection,
                                                                (MySqlTransaction)tx);

                    sqlCommand.Parameters.AddWithValue("@PersonID_FK", PersonId);
                    sqlCommand.Parameters.AddWithValue("@CreateShVer", foodIntake.CreateShVer);
                    sqlCommand.Parameters.AddWithValue("@StartTime", foodIntake.StartTime);
                    sqlCommand.Parameters.AddWithValue("@Amount", foodIntake.Amount);
                    sqlCommand.Parameters.AddWithValue("@Custom", (object?)foodIntake.Custom ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@ModifyShVer", foodIntake.ModifyShVer);
                    sqlCommand.Parameters.AddWithValue("@UpdateTime", foodIntake.UpdateTime);
                    sqlCommand.Parameters.AddWithValue("@CreateTime", foodIntake.CreateTime);
                    sqlCommand.Parameters.AddWithValue("@MealType", foodIntake.MealType);
                    sqlCommand.Parameters.AddWithValue("@ClientDataId", (object?)foodIntake.ClientDataId ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@Name", foodIntake.Name);
                    sqlCommand.Parameters.AddWithValue("@Unit", foodIntake.Unit);
                    sqlCommand.Parameters.AddWithValue("@ClientDataVer", (object?)foodIntake.ClientDataVer ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@Calorie", foodIntake.Calorie);
                    sqlCommand.Parameters.AddWithValue("@TimeOffset", foodIntake.TimeOffset);
                    sqlCommand.Parameters.AddWithValue("@DeviceUuid", foodIntake.DeviceUuid);
                    sqlCommand.Parameters.AddWithValue("@Comment", (object?)foodIntake.Comment ?? DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@PkgName", foodIntake.PkgName);
                    sqlCommand.Parameters.AddWithValue("@DataUuid", foodIntake.DataUuid.ToString());
                    sqlCommand.Parameters.AddWithValue("@FoodInfoId", (object?)foodIntake.FoodInfoId ?? DBNull.Value);

                    await sqlCommand.ExecuteNonQueryAsync();

                    progress?.Report((double)current / total * 100);
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
        /// Inserts a collection of daily step trend records for a specific person into the database.
        /// Utilizes a database transaction to ensure data integrity; performs a rollback in case of failure.
        /// </summary>
        /// <param name="PersonId">The unique identifier of the person to whom the data belongs.</param>
        /// <param name="stepDailyTrendList">A collection of <see cref="StepDailyTrendModel"/> objects containing the step trend data to be stored.</param>
        /// <param name="progress">An optional progress indicator that reports the percentage of the insertion process.</param>
        /// <returns>Returns 'true' if the operation completes successfully.</returns>
        /// <exception cref="Exception">Throws an exception if the database operation fails, triggering an automatic rollback.</exception>
        public async Task<bool> InsertSamsungHealthStepDailyTrend(int PersonId, IEnumerable<StepDailyTrendModel> stepDailyTrendList, IProgress<double>? progress = null)
        {
            var sqlServerConnection = await EtablishSqlServerConnection();

            await using var tx = await sqlServerConnection.BeginTransactionAsync();

            try
            {
                var trends = stepDailyTrendList.ToList();

                int total = trends.Count;
                int current = 0;

                foreach (var trend in trends)
                {
                    await using var sqlCommand = new MySqlCommand(DatabaseCommands.CmdInsertSamsungHealthStepDailyTrend(),
                                                                sqlServerConnection,
                                                                (MySqlTransaction)tx);

                    sqlCommand.Parameters.AddWithValue("@PersonID_FK", PersonId);
                    sqlCommand.Parameters.AddWithValue("@BinningData", trend.BinningData);
                    sqlCommand.Parameters.AddWithValue("@UpdateTime", trend.UpdateTime);
                    sqlCommand.Parameters.AddWithValue("@CreateTime", trend.CreateTime);
                    sqlCommand.Parameters.AddWithValue("@SourcePkgName", trend.SourcePkgName);
                    sqlCommand.Parameters.AddWithValue("@SourceType", trend.SourceType);
                    sqlCommand.Parameters.AddWithValue("@Count", trend.Count);
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
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        #endregion

        #region Functions Get Datas from Databse

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
                    PersonBirthDate = SqlDataReader.IsDBNull(3) ? null : SqlDataReader.GetDateTime(3),
                    PersonHeight = SqlDataReader.GetFloat(4)
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
        public async Task<int> GetPersonCountAsync()
        {
            await using var SqlServerConnection = await EtablishSqlServerConnection();
            await using var SqlCommand = new MySqlCommand(DatabaseCommands.CmdCountPersonsInTable(), SqlServerConnection);
            var result = await SqlCommand.ExecuteScalarAsync();

            // Validation: If the result is null (DBNull), count is set to 0 to avoid exceptions
            int count = result != null ? Convert.ToInt32(result) : 0;

            // Return logic: Ensures the method never returns negative values, maintaining UI consistency
            return count > 0 ? count : 0;
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
            return null;
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
        public async Task<List<FullBodyMeasurementDatas>> GetBodyMeasurementAsync(int personId)
        {
            var list = new List<FullBodyMeasurementDatas>();

            var SqlServerConnection = await EtablishSqlServerConnection();
            await using var SqlCommand = new MySqlCommand(DatabaseCommands.CmdGetMeasurement(), SqlServerConnection);

            SqlCommand.Parameters.AddWithValue("@pid", personId);

            await using var SqlDataReader = await SqlCommand.ExecuteReaderAsync();

            while (await SqlDataReader.ReadAsync())
            {
                list.Add(new FullBodyMeasurementDatas
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

        public async Task<List<StepDailyTrendChartModel>> GetStepDailyTrendAsync(int personId)
        {
            var result = new List<StepDailyTrendChartModel>();

            await using var conn = await EtablishSqlServerConnection();

            await using var cmd = new MySqlCommand(
                DatabaseCommands.CmdGetSamsungHealthStepDailyTrend(),
                conn);

            cmd.Parameters.AddWithValue("@PersonID", personId);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new StepDailyTrendChartModel
                {
                    CreateTime = reader.GetDateTime("create_time"),
                    SourceType = reader.GetInt32("source_type"),
                    Count = reader.GetInt32("count"),
                    Distance = reader.GetDouble("distance"),
                    Calorie = reader.GetDouble("calorie")
                });
            }

            return result;
        }

        #endregion

        #region Functions Update Datas to Datebase

        /// <summary>
        /// Performs an atomic upsert operation for both metric and dimension datasets within a single database transaction.
        /// </summary>
        /// <param name="personId">The unique identifier of the target user.</param>
        /// <param name="metricId">Optional ID for existing metric records; if null, an INSERT is performed.</param>
        /// <param name="dimensionId">Optional ID for existing dimension records; if null, an INSERT is performed.</param>
        /// <param name="measurementDate">The effective timestamp for the measurement record.</param>
        /// <param name="bodyWeight">Measured body weight in kilograms.</param>
        /// <param name="bmi">Calculated Body Mass Index.</param>
        /// <param name="fat">Body fat percentage.</param>
        /// <param name="muscle">Muscle mass percentage.</param>
        /// <param name="visceralFat">Visceral fat level indicator.</param>
        /// <param name="chest">Chest circumference in centimeters.</param>
        /// <param name="waist">Waist circumference in centimeters.</param>
        /// <param name="hips">Hip circumference in centimeters.</param>
        /// <param name="fattongs">Skinfold measurement (caliper) value.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous database operation.</returns>
        /// <exception cref="MySqlException">Thrown if the transaction fails or the connection is interrupted.</exception>
        public async Task UpdateMeasurementAsync(
            int personId,
            int? metricId, int? dimensionId,
            DateTime measurementDate,
            float? bodyWeight, float? bmi, float? fat, float? fato, float? fatu, float? muscle, float? muscleo, float? muscleu, float? bodyw, float? bodyb, int? visceralFat,
            float? chest, float? waist, float? hips, float? fatTongBreastCrease, float? fatTongArmpitCrease, float? fatTongAbdominalCrease,
            float? fatTongHipCrease, float? fatTongThighCrease, float? fatTongBackCrease, float? fatTongTricepsCrease)
        {
            await using var conn = await EtablishSqlServerConnection();
            await using var tx = await conn.BeginTransactionAsync();
            try
            {
                if (metricId.HasValue)
                {
                    await using var cmdUpdateMetric = new MySqlCommand(DatabaseCommands.CmdUpdatePersonMetric(), conn, (MySqlTransaction)tx);
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
                else
                {
                    await using var cmdInsertMetric = new MySqlCommand(DatabaseCommands.CmdInsertPersonMetric(), conn, (MySqlTransaction)tx);
                    cmdInsertMetric.Parameters.AddWithValue("@mid", metricId.Value);
                    cmdInsertMetric.Parameters.AddWithValue("@gw", (object?)bodyWeight ?? DBNull.Value);
                    cmdInsertMetric.Parameters.AddWithValue("@bmi", (object?)bmi ?? DBNull.Value);
                    cmdInsertMetric.Parameters.AddWithValue("@kf", (object?)fat ?? DBNull.Value);
                    cmdInsertMetric.Parameters.AddWithValue("@kfo", (object?)fato ?? DBNull.Value);
                    cmdInsertMetric.Parameters.AddWithValue("@kfu", (object?)fatu ?? DBNull.Value);
                    cmdInsertMetric.Parameters.AddWithValue("@mm", (object?)muscle ?? DBNull.Value);
                    cmdInsertMetric.Parameters.AddWithValue("@mmo", (object?)muscleo ?? DBNull.Value);
                    cmdInsertMetric.Parameters.AddWithValue("@mmu", (object?)muscleu ?? DBNull.Value);
                    cmdInsertMetric.Parameters.AddWithValue("@kw", (object?)bodyw ?? DBNull.Value);
                    cmdInsertMetric.Parameters.AddWithValue("@kk", (object?)bodyb ?? DBNull.Value);
                    cmdInsertMetric.Parameters.AddWithValue("@vf", (object?)visceralFat ?? DBNull.Value);
                    await cmdInsertMetric.ExecuteNonQueryAsync();
                }

                if (dimensionId.HasValue)
                {
                    await using var cmdUpdateDim = new MySqlCommand(DatabaseCommands.CmdUpdatePersonDimension(), conn, (MySqlTransaction)tx);
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
                else
                {
                    await using var cmdInsertDim = new MySqlCommand(DatabaseCommands.CmdInsertPersonDimension(), conn, (MySqlTransaction)tx);
                    cmdInsertDim.Parameters.AddWithValue("@pid", personId);
                    cmdInsertDim.Parameters.AddWithValue("@dt", measurementDate);
                    cmdInsertDim.Parameters.AddWithValue("@br", (object?)chest ?? DBNull.Value);
                    cmdInsertDim.Parameters.AddWithValue("@ba", (object?)waist ?? DBNull.Value);
                    cmdInsertDim.Parameters.AddWithValue("@hu", (object?)hips ?? DBNull.Value);
                    cmdInsertDim.Parameters.AddWithValue("@fzbrust", (object?)fatTongBreastCrease ?? DBNull.Value);
                    cmdInsertDim.Parameters.AddWithValue("@fzarmpit", (object?)fatTongArmpitCrease ?? DBNull.Value);
                    cmdInsertDim.Parameters.AddWithValue("@fzabdomen", (object?)fatTongAbdominalCrease ?? DBNull.Value);
                    cmdInsertDim.Parameters.AddWithValue("@fzhip", (object?)fatTongHipCrease ?? DBNull.Value);
                    cmdInsertDim.Parameters.AddWithValue("@fzthigh", (object?)fatTongThighCrease ?? DBNull.Value);
                    cmdInsertDim.Parameters.AddWithValue("@fzback", (object?)fatTongBackCrease ?? DBNull.Value);
                    cmdInsertDim.Parameters.AddWithValue("@fztriceps", (object?)fatTongTricepsCrease ?? DBNull.Value);
                    await cmdInsertDim.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
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
        public async Task UpdatePersonAsync(int PersonId, string firstName, string lastName, DateTime? birthDate, float height)
        {
            await using var conn = await EtablishSqlServerConnection();
            await using var tx = await conn.BeginTransactionAsync();

            try
            {
                await using var cmdUpdatePerson = new MySqlCommand(DatabaseCommands.CmdUpdatePerson(), conn, (MySqlTransaction)tx);
                cmdUpdatePerson.Parameters.AddWithValue("@pid", PersonId);
                cmdUpdatePerson.Parameters.AddWithValue("@v", firstName);
                cmdUpdatePerson.Parameters.AddWithValue("@n", lastName);
                cmdUpdatePerson.Parameters.AddWithValue("@g", birthDate.HasValue ? birthDate.Value : (object)DBNull.Value);
                cmdUpdatePerson.Parameters.AddWithValue("@k", height);
                await cmdUpdatePerson.ExecuteNonQueryAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        #endregion

    }
}
