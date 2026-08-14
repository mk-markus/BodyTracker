namespace BodyTracker.Services
{
    public interface ISqlCommandProvider
    {
        /// <summary>
        /// Contains the command to create the specific table structure if the tables don't exist.
        /// This includes 'tbl_Personen', 'tbl_KoerperMetriken' and 'tbl_Abmessungen'.
        /// </summary>
        /// <returns>SQL command for database schema initialization.</returns>
        string CmdCreateTableIfNotExist();

        /// <summary>
        /// Provides the SQL command to retrieve all person records from 'tbl_Personen'.
        /// </summary>
        /// <returns>SQL SELECT command for all persons.</returns>
        string CmdGetPerson();


        /// <summary>
        /// Provides the SQL command to insert a new record into 'tbl_Personen'.
        /// Requires parameters for FirstName, LastName, and BirthDate.
        /// </summary>
        /// <returns>SQL INSERT command for a new person.</return
        string CmdCreatePerson();

        /// <summary>
        /// Provides the SQL command to fetch the most recent entry for a specific person 
        /// from 'tbl_KoerperMetriken' based on the MeasurementDate.
        /// </summary>
        /// <returns>SQL SELECT command for the latest metric record.</returns>
        string CmdGetLastPersonMetric();

        /// <summary>
        /// Provides the SQL command to insert new physiological data (Weight, BMI, BodyFat, etc.) 
        /// into 'tbl_KoerperMetriken' for a specific person.
        /// </summary>
        /// <returns>SQL INSERT command for metric data.</returns>
        string CmdInsertPersonMetric();

        /// <summary>
        /// Provides the SQL command to remove a specific metric record from 'tbl_KoerperMetriken'.
        /// Usually identified by MetrikID.
        /// </summary>
        /// <returns>SQL DELETE command for metric data.</returns>
        string CmdDeletePersonMetric();

        /// <summary>
        /// Provides the SQL command to retrieve the most recent physical measurements 
        /// (Chest, Waist, Hips) for a specific person from 'tbl_Abmessungen'.
        /// </summary>
        /// <returns>SQL SELECT command for the latest dimension record.</returns>
        string CmdGetLastPersonDimension();

        /// <summary>
        /// Provides the SQL command to insert new physical dimensions into 'tbl_Abmessungen'.
        /// Requires parameters for Chest, Waist, and Hip circumference.
        /// </summary>
        /// <returns>SQL INSERT command for dimension data.</returns>
        string CmdInsertPersonDimension();

        /// <summary>
        /// Provides the SQL command to delete a specific dimension record from 'tbl_Abmessungen'.
        /// </summary>
        /// <returns>SQL DELETE command for dimension data.</returns>
        string CmdDeletePersonDimension();

        /// <summary>
        /// Provides a generalized SQL command to retrieve bodyMeasurement history for a specific person, 
        /// often joining multiple tables or filtering by date ranges.
        /// </summary>
        /// <returns>SQL SELECT command for historical bodyMeasurement data.</returns>
        string CmdGetMeasurement();

        /// <summary>
        /// Generates or executes the SQL command string required to count the total number of records 
        /// in the persons table.
        /// </summary>
        /// <returns>
        /// A formatted string containing the SQL <c>COUNT</c> statement or the result of the operation, 
        /// depending on the specific implementation within the service.
        /// </returns>
        string CmdCountPersonsInTable();

        /// <summary>
        /// Provides the SQL command string required to update a person's biometric metrics 
        /// within the database.
        /// </summary>
        /// <returns>
        /// A SQL <c>UPDATE</c> statement string, specifically tailored to synchronize 
        /// modified user properties from the application state back to the database.
        /// </returns>
        string CmdUpdatePersonMetric();

        /// <summary>
        /// Provides the SQL command string required to update a person's physical dimensions 
        /// (e.g., height) within the database.
        /// </summary>
        /// <returns>
        /// A SQL <c>UPDATE</c> statement string designed to modify existing dimension 
        /// records for a specific user.
        /// </returns>
        string CmdUpdatePersonDimension();




    }
}
