namespace BodyTracker.Services
{
    public class SqlCommandProvider
    {
        #region Standard SQL Commands

        /// <summary>
        /// Generates the SQL command string for inserting a new person record into the database.
        /// </summary>
        /// <remarks>Constructs an INSERT statement for the <c>tbl_Personen</c> table using parameterized values for first name, last name, date of birth, and height, followed by a SELECT statement to retrieve the last inserted identifier.</remarks>
        /// <returns>A string containing the parameterized SQL insert query and ID selection command.</returns>
        public string GetNewPersonCreateSql()
        {
            return "INSERT INTO tbl_Personen (person_first_name, person_last_name, person_birth_date, person_height) VALUES (@v, @n, @g, @k); SELECT LAST_INSERT_ID();";
        }

        /// <summary>
        /// Generates the SQL script to create the core application database tables if they do not already exist.
        /// </summary>
        /// <remarks>Creates the <c>tbl_Personen</c>, <c>tbl_KoerperMetriken</c> (with unique constraints and foreign key relationships), and <c>tbl_Abmessungen</c> tables for storing personal information, body metrics, and circumference or skinfold measurements.</remarks>
        /// <returns>A string containing the complete multi-table creation SQL script.</returns>
        public string CmdCreateTableIfNotExist()
        {
            return @"CREATE TABLE IF NOT EXISTS tbl_Personen (  ID INT AUTO_INCREMENT PRIMARY KEY,
                                                                person_first_name VARCHAR(50),
                                                                person_last_name VARCHAR(50),
                                                                person_birth_date DATE
                                                            );
                                                            CREATE TABLE IF NOT EXISTS tbl_KoerperMetriken (
	                                                            MetrikID INT(11) NOT NULL AUTO_INCREMENT,
	                                                            PersonID_FK INT(11) NOT NULL,
	                                                            create_at DATETIME NULL DEFAULT current_timestamp(),
	                                                            Gewicht_kg DECIMAL(7,2) NULL DEFAULT NULL,
	                                                            BMI DECIMAL(7,2) NULL DEFAULT NULL,
	                                                            Koerperfett_Prozent DECIMAL(7,2) NULL DEFAULT NULL,
	                                                            Körperfett_oben_Prozent DECIMAL(7,2) NULL DEFAULT NULL,
	                                                            Körperfett_unten_Prozent DECIMAL(7,2) NULL DEFAULT NULL,
	                                                            Muskelmasse_Prozent DECIMAL(7,2) NULL DEFAULT NULL,
	                                                            Muskelmasse_oben_Prozent DECIMAL(7,2) NULL DEFAULT NULL,
	                                                            Muskelmasse_unten_Prozent DECIMAL(7,2) NULL DEFAULT NULL,
	                                                            Körperknochen_Masse DECIMAL(7,2) NULL DEFAULT NULL,
	                                                            Körperwasser_Prozent DECIMAL(7,2) NULL DEFAULT NULL,
	                                                            Viszeralfett INT(11) NULL DEFAULT NULL,
	                                                            PRIMARY KEY (MetrikID) USING BTREE,
	                                                            UNIQUE INDEX UQ_Metrik_Person_Datum (PersonID_FK, create_at) USING BTREE,
	                                                            CONSTRAINT FK_PersonMetrik FOREIGN KEY (PersonID_FK) REFERENCES tbl_Personen (ID) ON UPDATE RESTRICT ON DELETE CASCADE
                                                            );
                                                            CREATE TABLE IF NOT EXISTS tbl_Abmessungen (
                                                                AbmessungID INT AUTO_INCREMENT PRIMARY KEY,
                                                                PersonID_FK INT NOT NULL,
                                                                create_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                                                                Brustumfang_cm DECIMAL(7,2),
                                                                Bauchumfang_cm DECIMAL(7,2),
                                                                Hueftumfang_cm DECIMAL(7,2),
                                                                Brustfalte_mm DECIMAL(7,2),
                                                                Achselfalte_mm DECIMAL(7,2),
                                                                Bauchfalte_mm DECIMAL(7,2),
                                                                Hüftfalte_mm DECIMAL(7,2),
                                                                Oberschenkelfalte_mm DECIMAL(7,2),
                                                                Rückenfalte_mm DECIMAL(7,2),
                                                                Trizepsfalte_mm DECIMAL(7,2),
                                                                CONSTRAINT FK_PersonAbmessung FOREIGN KEY (PersonID_FK) REFERENCES tbl_Personen (ID) ON DELETE CASCADE
                                                            );";
        }

        /// <summary>
        /// Generates the SQL query string to count the total number of person records stored in the database.
        /// </summary>
        /// <remarks>Constructs a <c>SELECT COUNT(*)</c> statement targeting the <c>tbl_Personen</c> table.</remarks>
        /// <returns>A string containing the SQL count query.</returns>
        public string GetPersonCountSql()
        {
            return "SELECT COUNT(*) FROM tbl_Personen";
        }

        #endregion

        #region Person Measurement / Dimension / Metric SQL Commands

        /// <summary>
        /// Generates the SQL command string for deleting a person's body bodyMeasurement or dimension record by its unique identifier.
        /// </summary>
        /// <remarks>Constructs a parameterized DELETE statement targeting the <c>tbl_Abmessungen</c> table.</remarks>
        /// <returns>A string containing the parameterized SQL delete query for body dimensions.</returns>
        public string GetPersonDimensionDeleteSql()
        {
            return "DELETE FROM tbl_Abmessungen WHERE dimension_id=@id";
        }

        /// <summary>
        /// Generates the SQL command string for deleting a person's body metric record by its unique identifier.
        /// </summary>
        /// <remarks>Constructs a parameterized DELETE statement targeting the <c>tbl_KoerperMetriken</c> table.</remarks>
        /// <returns>A string containing the parameterized SQL delete query for body metrics.</returns>
        public string GetPersonMetricDeleteSql()
        {
            return "DELETE FROM tbl_KoerperMetriken WHERE metric_id=@id";
        }

        /// <summary>
        /// Generates the SQL query string to retrieve the most recent body dimension or circumference record for a specific person up to a given date.
        /// </summary>
        /// <remarks>Constructs a SELECT statement targeting the <c>tbl_Abmessungen</c> table, filtering by person ID and date boundary, ordered descending by bodyMeasurement date with a limit of one.</remarks>
        /// <returns>A string containing the parameterized SQL query for retrieving the last person dimension record.</returns>
        public string GetLastPersonDimensionsSql()
        {
            return @"SELECT
                dimension_id,
                person_id,
                create_at,
                chest_circumference_current,
                abdomen_circumference_current,
                hip_circumference_current,
                chest_skin_fold_current,
                axilla_skin_fold_current,
                abdomen_skin_fold_current,
                hip_skin_fold_current,
                thigh_skin_fold_current,
                back_skin_fold_current,
                tricep_skin_fold_current
            FROM tbl_Abmessungen
            WHERE person_id = @pid
              AND DATE(create_at) <= @today
            ORDER BY create_at DESC
            LIMIT 1";
        }

        /// <summary>
        /// Generates the SQL query string to retrieve the most recent body metric record for a specific person up to a given date.
        /// </summary>
        /// <remarks>Constructs a SELECT statement targeting the <c>tbl_KoerperMetriken</c> table, filtering by person ID and date boundary, ordered descending by bodyMeasurement date with a limit of one.</remarks>
        /// <returns>A string containing the parameterized SQL query for retrieving the last person metric record.</returns>
        public string GetLastPersonMetricsSql()
        {
            return @"SELECT
                metric_id,
                person_id,
                create_at,
                weight_current,
                bmi_current,
                body_fat_percentage_current,
                upper_body_fat_percentage_current,
                lower_body_fat_percentage_current,
                muscle_mass_percentage_current,
                upper_body_muscle_mass_percentage_current,
                lower_body_muscle_mass_percentage_current,
                bone_mass_current,
                body_water_percentage_current,
                visceral_fat_current
            FROM tbl_KoerperMetriken
            WHERE person_id = @pid
              AND DATE(create_at) <= @today
            ORDER BY create_at DESC
            LIMIT 1";
        }

        /// <summary>
        /// Generates the SQL query string to retrieve combined body metrics and dimensions joined by bodyMeasurement date for a specific person.
        /// </summary>
        /// <remarks>Constructs a SELECT statement with a LEFT JOIN between <c>tbl_KoerperMetriken</c> and <c>tbl_Abmessungen</c> based on 
        /// matching dates and person IDs, ordered ascending by bodyMeasurement date.</remarks>
        /// <returns>A string containing the parameterized SQL join query for complete current measurements.</returns>
        public string GetPersonMeasurementsSql()
        {
            return @"SELECT
                m.metric_id,
                a.dimension_id,
                m.create_at,
                m.weight_current,
                m.bmi_current,
                m.body_fat_percentage_current,
                m.upper_body_fat_percentage_current,
                m.lower_body_fat_percentage_current,
                m.muscle_mass_percentage_current,
                m.upper_body_muscle_mass_percentage_current,
                m.lower_body_muscle_mass_percentage_current,
                m.bone_mass_current,
                m.body_water_percentage_current,
                m.visceral_fat_current,

                a.chest_circumference_current,
                a.abdomen_circumference_current,
                a.hip_circumference_current,
                a.chest_skin_fold_current,
                a.axilla_skin_fold_current,
                a.abdomen_skin_fold_current,
                a.hip_skin_fold_current,
                a.thigh_skin_fold_current,
                a.back_skin_fold_current,
                a.tricep_skin_fold_current

            FROM tbl_KoerperMetriken m
            LEFT JOIN tbl_Abmessungen a
                ON DATE(m.create_at) = DATE(a.create_at)
                AND a.person_id = m.person_id
            WHERE m.person_id = @pid
            ORDER BY m.create_at";
        }

        /// <summary>
        /// Generates the SQL query string to retrieve initial baseline body composition and dimension measurements for a person.
        /// </summary>
        /// <remarks>Constructs a SELECT statement joining the <c>tbl_KoerperMetriken</c> and <c>tbl_Abmessungen</c> tables by person ID and 
        /// matching creation dates, ordered chronologically.</remarks>
        /// <returns>A string containing the SQL query for initial person measurements.</returns>
        public string GetInitialPersonMeasurementsSql()
        {
            return @"SELECT
                m.metric_id,
                a.dimension_id,
                m.create_at,

                m.weight_initial,
                m.bmi_initial,
                m.body_fat_percentage_initial,
                m.upper_body_fat_percentage_initial,
                m.lower_body_fat_percentage_initial,
                m.muscle_mass_percentage_initial,
                m.upper_body_muscle_mass_percentage_initial,
                m.lower_body_muscle_mass_percentage_initial,
                m.bone_mass_initial,
                m.body_water_percentage_initial,
                m.visceral_fat_initial,

                a.chest_circumference_initial,
                a.abdomen_circumference_initial,
                a.hip_circumference_initial,
                a.chest_skin_fold_initial,
                a.axilla_skin_fold_initial,
                a.abdomen_skin_fold_initial,
                a.hip_skin_fold_initial,
                a.thigh_skin_fold_initial,
                a.back_skin_fold_initial,
                a.tricep_skin_fold_initial

            FROM tbl_KoerperMetriken m
            LEFT JOIN tbl_Abmessungen a
                ON DATE(m.create_at) = DATE(a.create_at)
                AND a.person_id = m.person_id
            WHERE m.person_id = @pid
            ORDER BY m.create_at";
        }

        /// <summary>
        /// Generates the SQL query string to retrieve all person records ordered alphabetically by last name and first name.
        /// </summary>
        /// <remarks>Constructs a SELECT statement targeting the <c>tbl_Personen</c> table.</remarks>
        /// <returns>A string containing the SQL query to fetch all persons.</returns>
        public string GetPersonExistenceCheckSql()
        {
            return "SELECT person_id, person_first_name, person_last_name, person_birth_date, person_height FROM tbl_Personen ORDER BY person_last_name, person_first_name";
        }

        /// <summary>
        /// Generates the SQL command string for inserting a new body dimension or circumference record into the database.
        /// </summary>
        /// <remarks>Constructs an INSERT statement targeting the <c>tbl_Abmessungen</c> table using parameterized values for circumferences and skinfold measurements.</remarks>
        /// <returns>A string containing the parameterized SQL insert query for person dimensions.</returns>
        public string GetPersonDimensionInsertSql()
        {
            return @"INSERT INTO tbl_Abmessungen(
                person_id,
                create_at,
                update_at,

                chest_circumference_current,
                chest_circumference_initial,

                abdomen_circumference_current,
                abdomen_circumference_initial,

                hip_circumference_current,
                hip_circumference_initial,

                chest_skin_fold_current,
                chest_skin_fold_initial,

                axilla_skin_fold_current,
                axilla_skin_fold_initial,

                abdomen_skin_fold_current,
                abdomen_skin_fold_initial,

                hip_skin_fold_current,
                hip_skin_fold_initial,

                thigh_skin_fold_current,
                thigh_skin_fold_initial,

                back_skin_fold_current,
                back_skin_fold_initial,

                tricep_skin_fold_current,
                tricep_skin_fold_initial
            )
            VALUES(
                @pid,
                @dt,
                @dt,

                @br,@br,
                @ba,@ba,
                @hu,@hu,

                @fzbreast,@fzbreast,
                @fzarmpit,@fzarmpit,
                @fzabdomen,@fzabdomen,
                @fzhip,@fzhip,
                @fzthigh,@fzthigh,
                @fzback,@fzback,
                @fztricep,@fztricep
            )";
        }

        /// <summary>
        /// Generates the SQL command string for inserting a new body metric record into the database.
        /// </summary>
        /// <remarks>Constructs an INSERT statement targeting the <c>tbl_KoerperMetriken</c> table using parameterized values for weight, BMI, body fat percentages, muscle mass, bone mass, body water, and visceral fat.</remarks>
        /// <returns>A string containing the parameterized SQL insert query for person metrics.</returns>
        public string GetPersonMetricInsertSql()
        {
            return @"INSERT INTO tbl_KoerperMetriken(
                person_id,
                create_at,
                update_at,

                weight_current,
                weight_initial,

                bmi_current,
                bmi_initial,

                body_fat_percentage_current,
                body_fat_percentage_initial,

                upper_body_fat_percentage_current,
                upper_body_fat_percentage_initial,

                lower_body_fat_percentage_current,
                lower_body_fat_percentage_initial,

                muscle_mass_percentage_current,
                muscle_mass_percentage_initial,

                upper_body_muscle_mass_percentage_current,
                upper_body_muscle_mass_percentage_initial,

                lower_body_muscle_mass_percentage_current,
                lower_body_muscle_mass_percentage_initial,

                bone_mass_current,
                bone_mass_initial,

                body_water_percentage_current,
                body_water_percentage_initial,

                visceral_fat_current,
                visceral_fat_initial
            )
            VALUES(
                @pid,
                @dt,
                @dt,

                @gw,@gw,
                @bmi,@bmi,
                @kf,@kf,
                @kfo,@kfo,
                @kfu,@kfu,
                @mm,@mm,
                @mmo,@mmo,
                @mmu,@mmu,
                @kk,@kk,
                @kw,@kw,
                @vf,@vf
            )";
        }

        /// <summary>
        /// Generates the SQL command string for updating an existing person's body metric record in the database.
        /// </summary>
        /// <remarks>Constructs an UPDATE statement targeting the <c>tbl_KoerperMetriken</c> table using parameterized values 
        /// for weight, BMI, body fat percentages, muscle mass, body water, bone mass, and visceral fat, filtered by metric ID.</remarks>
        /// <returns>A string containing the parameterized SQL update query for person metrics.</returns>
        public string GetPersonMetricUpdateSql()
        {
            return @"UPDATE tbl_KoerperMetriken
                    SET
                        weight_current=@gw,
                        bmi_current=@bmi,
                        body_fat_percentage_current=@kf,
                        upper_body_fat_percentage_current=@kfo,
                        lower_body_fat_percentage_current=@kfu,
                        muscle_mass_percentage_current=@mm,
                        upper_body_muscle_mass_percentage_current=@mmo,
                        lower_body_muscle_mass_percentage_current=@mmu,
                        bone_mass_current=@kk,
                        body_water_percentage_current=@kw,
                        visceral_fat_current=@vf,
                        update_at=CURRENT_TIMESTAMP
                    WHERE metric_id=@mid";
        }

        /// <summary>
        /// Generates the SQL command string for updating an existing person's body dimension or circumference record in the database.
        /// </summary>
        /// <remarks>Constructs an UPDATE statement targeting the <c>tbl_Abmessungen</c> table using parameterized values for 
        /// circumferences and skinfold measurements, filtered by dimension ID.</remarks>
        /// <returns>A string containing the parameterized SQL update query for person dimensions.</returns>
        public string GetPersonDimensionUpdateSql()
        {
            return @"UPDATE tbl_Abmessungen
                    SET
                        chest_circumference_current=@br,
                        abdomen_circumference_current=@ba,
                        hip_circumference_current=@hu,

                        chest_skin_fold_current=@fzbreast,
                        axilla_skin_fold_current=@fzarmpit,
                        abdomen_skin_fold_current=@fzabdomen,
                        hip_skin_fold_current=@fzhip,
                        thigh_skin_fold_current=@fzthigh,
                        back_skin_fold_current=@fzback,
                        tricep_skin_fold_current=@fztricep,

                        update_at=CURRENT_TIMESTAMP
                    WHERE dimension_id=@aid";
        }

        /// <summary>
        /// Generates the SQL query string to check whether any body measurements exist for a specific person within a given date range.
        /// </summary>
        /// <remarks>Constructs a <c>SELECT COUNT(1)</c> statement targeting the <c>tbl_KoerperMetriken</c> table, filtering by 
        /// person ID and a half-open date interval.</remarks>
        /// <returns>A string containing the parameterized SQL existence check query.</returns>
        public string GetPersonMeasurementsExistSql()
        {
            return @"SELECT COUNT(1) FROM tbl_KoerperMetriken WHERE person_id = @pid AND create_at >= @start AND create_at < @end";
        }

        /// <summary>
        /// Generates the SQL command string for updating an existing person's details in the database.
        /// </summary>
        /// <remarks>Constructs an UPDATE statement targeting the <c>tbl_Personen</c> table using parameterized values for first name, last name, date of birth, and height, filtered by person ID.</remarks>
        /// <returns>A string containing the parameterized SQL update query for person details.</returns>
        public string GetPersonUpdateSql()
        {
            return "UPDATE tbl_Personen SET person_first_name=@v, person_last_name=@n, person_birth_date=@g, person_height=@k WHERE person_id=@pid";
        }

        #endregion

        #region Samsung Health Heart Rate SQL Command

        /// <summary>
        /// Generates the SQL statement (Upsert) to insert or update Samsung Health heart rate records in the tbl_HeartRate table.
        /// </summary>
        /// <returns>An SQL string containing an INSERT...ON DUPLICATE KEY UPDATE command.</returns>
        public string GetSamsungHeartRateUpsertSql()
        {
            return @"INSERT INTO tbl_HeartRate
        (
            person_id,
            source,
            tag_id,
            createShVer,

            start_time,
            end_time,
            update_time,
            create_time,
            time_offset,

            custom,
            binning_data,
            modify_shver,
            client_data_id,

            heart_rate,
            heart_rate_max,
            heart_rate_min,
            heart_beat_count,

            client_dataver,

            comment,
            package_name,

            device_uuid,
            data_uuid
        )
        VALUES
        (
            @PersonId,

            @Source,
            @TagId,
            @CreateShVer,

            @StartTime,
            @EndTime,
            @UpdateTime,
            @CreateTime,
            @TimeOffset,

            @Custom,
            @BinningData,
            @ModifyShVer,
            @ClientDataId,

            @HeartRate,
            @HeartRateMax,
            @HeartRateMin,
            @HeartBeatCount,

            @ClientDataVer,

            @Comment,
            @PackageName,

            @DeviceUuid,
            @DataUuid
        )
        ON DUPLICATE KEY UPDATE

            source           = VALUES(source),
            tag_id           = VALUES(tag_id),
            createShVer      = VALUES(createShVer),

            start_time       = VALUES(start_time),
            end_time         = VALUES(end_time),
            update_time      = VALUES(update_time),
            create_time      = VALUES(create_time),
            time_offset      = VALUES(time_offset),

            custom           = VALUES(custom),
            binning_data     = VALUES(binning_data),
            modify_shver     = VALUES(modify_shver),
            client_data_id   = VALUES(client_data_id),

            heart_rate       = VALUES(heart_rate),
            heart_rate_max   = VALUES(heart_rate_max),
            heart_rate_min   = VALUES(heart_rate_min),
            heart_beat_count = VALUES(heart_beat_count),

            client_dataver   = VALUES(client_dataver),

            comment          = VALUES(comment),
            package_name     = VALUES(package_name),
            update_time      = CURRENT_TIMESTAMP;";
        }

        /// <summary>
        /// Generates the SQL command string for updating existing heart rate records in the database.
        /// </summary>
        /// <remarks>Constructs an UPDATE statement targeting the <c>tbl_HeartRate</c> table using parameterized metrics and metadata, filtered by data UUID.</remarks>
        /// <returns>A string containing the parameterized SQL update query for heart rates.</returns>
        public string GetHeartRateUpdateSql()
        {
            return @"UPDATE tbl_HeartRate
                    SET
                        source = @Source,
                        tag_id = @TagId,
                        createShVer = @CreateShVer,
                        start_time = @StartTime,
                        end_time = @EndTime,
                        update_time = @UpdateTime,
                        create_time = @CreateTime,
                        time_offset = @TimeOffset,
                        custom = @Custom,
                        binning_data = @BinningData,
                        modify_shver = @ModifyShVer,
                        client_data_id = @ClientDataId,
                        heart_rate = @HeartRate,
                        heart_rate_max = @HeartRateMax,
                        heart_rate_min = @HeartRateMin,
                        heart_beat_count = @HeartBeatCount,
                        client_dataver = @ClientDataVer,
                        comment = @Comment,
                        package_name = @PackageName,
                        device_uuid = @DeviceUuid
                    WHERE data_uuid = @DataUuid;";
        }

        /// <summary>
        /// Generates the SQL query string to retrieve existing heart rate identifiers for synchronization checks.
        /// </summary>
        /// <remarks>Constructs a SELECT statement targeting the <c>tbl_HeartRate</c> table to fetch data UUIDs and update timestamps for a specific person.</remarks>
        /// <returns>A string containing the SQL query for existing heart rate checks.</returns>
        public string GetHeartRateExistenceCheckSql()
        {
            return @"SELECT
               data_uuid,
               update_time
               FROM tbl_HeartRate
               WHERE person_id = @PersonId;";
        }

        #endregion

        #region Samsug Health Oxygen Saturation SQL Command

        /// <summary>
        /// Generates the SQL statement (Upsert) to insert or update Samsung Health oxygen saturation records in the tbl_OxygenSaturation table.
        /// </summary>
        /// <returns>An SQL string containing an INSERT...ON DUPLICATE KEY UPDATE command.</returns>
        public string GetSamsungOxygenSaturationUpsertSql()
        {
            return @"INSERT INTO tbl_OxygenSaturation
            (
                person_id,

                integrated_id,
                client_data_id,
                tag_id,

                start_time,
                end_time,
                update_time,
                create_time,

                time_offset,

                custom,

                spo2,
                spo2_max,
                spo2_min,

                low_spo2duration,
                coverage_rate,

                heart_rate,

                comment,

                data_uuid,
                device_uuid,

                package_name,

                createShVer,
                modify_shver,
                client_dataver,

                source,
                binning_data
            )
            VALUES
            (
                @PersonId,

                @IntegratedId,
                @ClientDataId,
                @TagId,

                @StartTime,
                @EndTime,
                @UpdateTime,
                @CreateTime,

                @TimeOffset,

                @Custom,

                @Spo2,
                @Spo2Max,
                @Spo2Min,

                @LowSpo2Duration,
                @CoverageRate,

                @HeartRate,

                @Comment,

                @DataUuid,
                @DeviceUuid,

                @PackageName,

                @CreateShVer,
                @ModifyShVer,
                @ClientDataVer,

                @Source,
                @BinningData
            )
            ON DUPLICATE KEY UPDATE

                person_id       = VALUES(person_id),

                integrated_id   = VALUES(integrated_id),
                client_data_id  = VALUES(client_data_id),
                tag_id          = VALUES(tag_id),

                start_time      = VALUES(start_time),
                end_time        = VALUES(end_time),
                create_time     = VALUES(create_time),

                time_offset     = VALUES(time_offset),

                custom          = VALUES(custom),

                spo2            = VALUES(spo2),
                spo2_max        = VALUES(spo2_max),
                spo2_min        = VALUES(spo2_min),

                low_spo2duration = VALUES(low_spo2duration),
                coverage_rate    = VALUES(coverage_rate),

                heart_rate      = VALUES(heart_rate),

                comment         = VALUES(comment),

                device_uuid     = VALUES(device_uuid),

                package_name    = VALUES(package_name),

                createShVer     = VALUES(createShVer),
                modify_shver    = VALUES(modify_shver),
                client_dataver  = VALUES(client_dataver),

                source          = VALUES(source),
                binning_data    = VALUES(binning_data),

                update_time     = CURRENT_TIMESTAMP;";
        }

        /// <summary>
        /// Generates the SQL query string to retrieve existing Samsung Health oxygen saturation identifiers for synchronization checks.
        /// </summary>
        /// <remarks>Constructs a SELECT statement targeting the <c>tbl_OxygenSaturation</c> table to fetch data UUIDs
        /// and update timestamps for a specific person.</remarks>
        /// <returns>A string containing the SQL query for existing oxygen saturation checks.</returns>
        public string GetSamsungOxygenSaturationExistenceCheckSql()
        {
            return @"SELECT
                     data_uuid,
                     update_time
                     FROM tbl_OxygenSaturation
                     WHERE person_id = @PersonId;";
        }

        /// <summary>
        /// Generates the SQL command string for updating existing Samsung Health oxygen saturation records in the database.
        /// </summary>
        /// <remarks>Constructs an UPDATE statement targeting the <c>tbl_OxygenSaturation</c> table using parameterized SpO2 metrics
        /// and device metadata, filtered by person ID and data UUID.</remarks>
        /// <returns>A string containing the parameterized SQL update query for oxygen saturation.</returns>
        public string GetSamsungOxygenSaturationUpdateSql()
        {
            return @"UPDATE tbl_OxygenSaturation
            SET
                integrated_id   = @IntegratedId,
                client_data_id  = @ClientDataId,
                tag_id          = @TagId,

                start_time      = @StartTime,
                end_time        = @EndTime,
                update_time     = CURRENT_TIMESTAMP,
                create_time     = @CreateTime,

                time_offset     = @TimeOffset,

                custom          = @Custom,

                spo2            = @Spo2,
                spo2_max        = @Spo2Max,
                spo2_min        = @Spo2Min,

                low_spo2duration = @LowSpo2Duration,
                coverage_rate    = @CoverageRate,

                heart_rate      = @HeartRate,

                comment         = @Comment,

                device_uuid     = @DeviceUuid,
                package_name    = @PackageName,

                createShVer     = @CreateShVer,
                modify_shver    = @ModifyShVer,
                client_dataver  = @ClientDataVer,

                source          = @Source,
                binning_data    = @BinningData

            WHERE
                person_id = @PersonId
                AND data_uuid = @DataUuid;";
        }

        #endregion

        #region Samsung Health Step Trend SQL Command

        /// <summary>
        /// Generates the SQL query string to retrieve Samsung Health daily step trend records for a specific person.
        /// </summary>
        /// <remarks>Constructs a SELECT statement targeting the <c>tbl_StepDailyTrend</c> table, filtering by person ID and 
        /// source type <c>-2</c>, ordered ascending by creation time.</remarks>
        /// <returns>A string containing the parameterized SQL query for step trends.</returns>
        public string GetSamsungStepTrendSql()
        {
            return @"SELECT
                    create_at,
                    source_type,
                    step_count_current,
                    distance_current,
                    calories_current
                    FROM tbl_StepDailyTrend
                    WHERE person_id = @PersonID
                    AND source_type = -2
                    ORDER BY create_at";
        }

        /// <summary>
        /// Generates the SQL command string for updating existing Samsung Health daily step trend records in the database.
        /// </summary>
        /// <remarks>Constructs an UPDATE statement targeting the <c>tbl_StepDailyTrend</c> table using parameterized activity metrics and source metadata, filtered by data UUID.</remarks>
        /// <returns>A string containing the parameterized SQL update query for daily step trends.</returns>
        public string GetSamsungStepTrendUpdateSql()
        {
            return @"UPDATE tbl_StepDailyTrend
                    SET
                        person_id = @PersonID_FK,
                        binning_data = @BinningData,
                        update_at = @UpdateTime,
                        create_at = @CreateTime,
                        source_package = @SourcePkgName,
                        source_type = @SourceType,

                        step_count_current = @Count,
                        speed_current = @Speed,
                        distance_current = @Distance,
                        calories_current = @Calorie,

                        device_uuid = @DeviceUuid,
                        package_name = @PkgName,

                        record_date = CURRENT_TIMESTAMP

                    WHERE data_uuid = @DataUuid;";
        }


        /// <summary>
        /// Generates the SQL command string for inserting or updating Samsung Health daily step trend records using an upsert pattern.
        /// </summary>
        /// <remarks>Constructs an INSERT statement targeting the <c>tbl_StepDailyTrend</c> table with conditional <c>ON DUPLICATE KEY UPDATE</c> 
        /// logic based on update timestamps.</remarks>
        /// <returns>A string containing the parameterized SQL upsert query for daily step trends.</returns>
        public string GetSamsungStepTrendUpsertSql()
        {
            return @"INSERT INTO tbl_StepDailyTrend(
                person_id,
                binning_data,
                update_at,
                create_at,
                source_package,
                source_type,

                step_count_current,
                step_count_initial,

                speed_current,
                speed_initial,

                distance_current,
                distance_initial,

                calories_current,
                calories_initial,

                device_uuid,
                package_name,
                data_uuid,
                record_date
            )
            VALUES (
                @PersonID_FK,
                @BinningData,
                @UpdateTime,
                @CreateTime,
                @SourcePkgName,
                @SourceType,

                @Count,
                @Count,

                @Speed,
                @Speed,

                @Distance,
                @Distance,

                @Calorie,
                @Calorie,

                @DeviceUuid,
                @PkgName,
                @DataUuid,

                CURRENT_TIMESTAMP
            )
            ON DUPLICATE KEY UPDATE

                person_id = person_id,

                step_count_current = @Count,
                speed_current = @Speed,
                distance_current = @Distance,
                calories_current = @Calorie,

                update_at = CURRENT_TIMESTAMP,

                record_date = CURRENT_TIMESTAMP;";
        }

        /// <summary>
        /// Generates the SQL query string to retrieve existing Samsung Health daily step trend identifiers for synchronization checks.
        /// </summary>
        /// <remarks>Constructs a SELECT statement targeting the <c>tbl_StepDailyTrend</c> table to fetch data UUIDs and update timestamps for a specific person.</remarks>
        /// <returns>A string containing the SQL query for existing step trend checks.</returns>
        public string GetSamsungStepTrendExistenceCheckSql()
        {
            return @"SELECT
               data_uuid,
               update_at
               FROM tbl_StepDailyTrend
               WHERE person_id = @PersonId;";
        }

        #endregion

        #region Samsung Health Exercise SQL Commands

        /// <summary>
        /// Generates the SQL command string for updating existing exercise records in the database.
        /// </summary>
        /// <remarks>Constructs an UPDATE statement targeting the <c>tbl_Exercise</c> table using comprehensive performance and tracking parameters, filtered by data UUID.</remarks>
        /// <returns>A string containing the parameterized SQL update query for exercises.</returns>
        public string GetExerciseUpdateSql()
        {
            return @"UPDATE tbl_Exercise
            SET
                live_data_internal      = @LiveDataInternal,
                mission_value           = @MissionValue,
                race_target             = @RaceTarget,
                subset_data             = @SubsetData,
                start_longitude         = @StartLongitude,
                routine_data_uuid       = @RoutineDataUuid,
                total_calorie           = @TotalCalorie,
                completion_status       = @CompletionStatus,
                pace_info_id            = @PaceInfoId,
                activity_type           = @ActivityType,
                pace_live_data          = @PaceLiveData,
                sensing_status          = @SensingStatus,
                source_type             = @SourceType,
                mission_type            = @MissionType,
                ftp                     = @Ftp,
                tracking_status         = @TrackingStatus,
                program_id              = @ProgramId,
                title                   = @Title,
                reward_status           = @RewardStatus,
                heart_rate_sample_count = @HeartRateSampleCount,
                start_latitude          = @StartLatitude,
                mission_extra_value     = @MissionExtraValue,
                program_schedule_id     = @ProgramScheduleId,
                heart_rate_device_uuid  = @HeartRateDeviceUuid,
                location_data_internal  = @LocationDataInternal,
                custom_id               = @CustomId,
                additional_internal     = @AdditionalInternal,

                duration                = @Duration,
                additional              = @Additional,
                create_sync_version     = @CreateShVer,
                mean_caloric_burn_rate  = @MeanCaloricBurnRate,
                location_data           = @LocationData,
                start_time              = @StartTime,
                exercise_type           = @ExerciseType,
                custom_text             = @Custom,
                max_altitude            = @MaxAltitude,
                incline_distance        = @InclineDistance,
                mean_heart_rate         = @MeanHeartRate,
                count_type              = @CountType,
                mean_rpm                = @MeanRpm,
                min_altitude            = @MinAltitude,
                modify_sync_version     = @ModifyShVer,
                max_heart_rate          = @MaxHeartRate,

                update_at               = @UpdateTime,
                create_at               = @CreateTime,

                client_data_id          = @ClientDataId,
                max_power               = @MaxPower,
                max_speed               = @MaxSpeed,
                mean_cadence            = @MeanCadence,
                min_heart_rate          = @MinHeartRate,
                client_data_version     = @ClientDataVer,
                count_value             = @Count,
                distance                = @Distance,
                max_caloric_burn_rate   = @MaxCaloricBurnRate,
                calorie                 = @Calorie,
                max_cadence             = @MaxCadence,
                decline_distance        = @DeclineDistance,
                vo2_max                 = @Vo2Max,
                time_offset             = @TimeOffset,
                device_uuid             = @DeviceUuid,
                max_rpm                 = @MaxRpm,
                comment_text            = @Comment,
                live_data               = @LiveData,
                mean_power              = @MeanPower,
                mean_speed              = @MeanSpeed,
                package_name            = @PkgName,
                altitude_gain           = @AltitudeGain,
                altitude_loss           = @AltitudeLoss,
                exercise_custom_type    = @ExerciseCustomType,
                auxiliary_devices       = @AuxiliaryDevices,
                end_time                = @EndTime,
                sweat_loss              = @SweatLoss

            WHERE data_uuid = @DataUuid;";
        }

        /// <summary>
        /// Generates the SQL command string for inserting or updating Samsung Health exercise records.
        /// </summary>
        /// <remarks>
        /// Inserts a new exercise record and updates the existing record when the same data_uuid already exists.
        /// update_at is always set to CURRENT_TIMESTAMP on updates.
        /// </remarks>
        /// <returns>
        /// A parameterized SQL upsert query.
        /// </returns>
        public string GetSamsungExerciseUpsertSql()
        {
            return @"INSERT INTO tbl_Exercise
                    (
                        person_id,

                        live_data_internal,
                        mission_value,
                        race_target,
                        subset_data,
                        start_longitude,
                        routine_data_uuid,
                        total_calorie,
                        completion_status,
                        pace_info_id,
                        activity_type,
                        pace_live_data,
                        sensing_status,
                        source_type,
                        mission_type,
                        ftp,
                        tracking_status,
                        program_id,
                        title,
                        reward_status,
                        heart_rate_sample_count,
                        start_latitude,
                        mission_extra_value,
                        program_schedule_id,
                        heart_rate_device_uuid,
                        location_data_internal,
                        custom_id,
                        additional_internal,

                        duration,
                        additional,
                        create_sync_version,
                        mean_caloric_burn_rate,
                        location_data,
                        start_time,
                        exercise_type,
                        custom_text,
                        max_altitude,
                        incline_distance,
                        mean_heart_rate,
                        count_type,
                        mean_rpm,
                        min_altitude,
                        modify_sync_version,
                        max_heart_rate,

                        update_at,
                        create_at,

                        client_data_id,
                        max_power,
                        max_speed,
                        mean_cadence,
                        min_heart_rate,
                        client_data_version,
                        count_value,
                        distance,
                        max_caloric_burn_rate,
                        calorie,
                        max_cadence,
                        decline_distance,
                        vo2_max,
                        time_offset,
                        device_uuid,
                        max_rpm,
                        comment_text,
                        live_data,
                        mean_power,
                        mean_speed,
                        package_name,
                        altitude_gain,
                        altitude_loss,
                        exercise_custom_type,
                        auxiliary_devices,
                        end_time,
                        data_uuid,
                        sweat_loss
                    )
                    VALUES
                    (
                        @person_id,

                        @LiveDataInternal,
                        @MissionValue,
                        @RaceTarget,
                        @SubsetData,
                        @StartLongitude,
                        @RoutineDataUuid,
                        @TotalCalorie,
                        @CompletionStatus,
                        @PaceInfoId,
                        @ActivityType,
                        @PaceLiveData,
                        @SensingStatus,
                        @SourceType,
                        @MissionType,
                        @Ftp,
                        @TrackingStatus,
                        @ProgramId,
                        @Title,
                        @RewardStatus,
                        @HeartRateSampleCount,
                        @StartLatitude,
                        @MissionExtraValue,
                        @ProgramScheduleId,
                        @HeartRateDeviceUuid,
                        @LocationDataInternal,
                        @CustomId,
                        @AdditionalInternal,

                        @Duration,
                        @Additional,
                        @CreateShVer,
                        @MeanCaloricBurnRate,
                        @LocationData,
                        @StartTime,
                        @ExerciseType,
                        @Custom,
                        @MaxAltitude,
                        @InclineDistance,
                        @MeanHeartRate,
                        @CountType,
                        @MeanRpm,
                        @MinAltitude,
                        @ModifyShVer,
                        @MaxHeartRate,

                        @UpdateTime,
                        @CreateTime,

                        @ClientDataId,
                        @MaxPower,
                        @MaxSpeed,
                        @MeanCadence,
                        @MinHeartRate,
                        @ClientDataVer,
                        @Count,
                        @Distance,
                        @MaxCaloricBurnRate,
                        @Calorie,
                        @MaxCadence,
                        @DeclineDistance,
                        @Vo2Max,
                        @TimeOffset,
                        @DeviceUuid,
                        @MaxRpm,
                        @Comment,
                        @LiveData,
                        @MeanPower,
                        @MeanSpeed,
                        @PkgName,
                        @AltitudeGain,
                        @AltitudeLoss,
                        @ExerciseCustomType,
                        @AuxiliaryDevices,
                        @EndTime,
                        @DataUuid,
                        @SweatLoss
                    )
                    ON DUPLICATE KEY UPDATE
                        live_data_internal     = VALUES(live_data_internal),
                        mission_value          = VALUES(mission_value),
                        race_target            = VALUES(race_target),
                        subset_data            = VALUES(subset_data),
                        start_longitude        = VALUES(start_longitude),
                        routine_data_uuid      = VALUES(routine_data_uuid),
                        total_calorie          = VALUES(total_calorie),
                        completion_status      = VALUES(completion_status),
                        pace_info_id           = VALUES(pace_info_id),
                        activity_type          = VALUES(activity_type),
                        pace_live_data         = VALUES(pace_live_data),
                        sensing_status         = VALUES(sensing_status),
                        source_type            = VALUES(source_type),
                        mission_type           = VALUES(mission_type),
                        ftp                    = VALUES(ftp),
                        tracking_status        = VALUES(tracking_status),
                        program_id             = VALUES(program_id),
                        title                  = VALUES(title),
                        reward_status          = VALUES(reward_status),
                        heart_rate_sample_count= VALUES(heart_rate_sample_count),
                        start_latitude         = VALUES(start_latitude),
                        mission_extra_value    = VALUES(mission_extra_value),
                        program_schedule_id    = VALUES(program_schedule_id),
                        heart_rate_device_uuid = VALUES(heart_rate_device_uuid),
                        location_data_internal = VALUES(location_data_internal),
                        custom_id              = VALUES(custom_id),
                        additional_internal    = VALUES(additional_internal),

                        duration               = VALUES(duration),
                        additional             = VALUES(additional),
                        create_sync_version    = VALUES(create_sync_version),
                        mean_caloric_burn_rate = VALUES(mean_caloric_burn_rate),
                        location_data          = VALUES(location_data),
                        start_time             = VALUES(start_time),
                        exercise_type          = VALUES(exercise_type),
                        custom_text            = VALUES(custom_text),
                        max_altitude           = VALUES(max_altitude),
                        incline_distance       = VALUES(incline_distance),
                        mean_heart_rate        = VALUES(mean_heart_rate),
                        count_type             = VALUES(count_type),
                        mean_rpm               = VALUES(mean_rpm),
                        min_altitude           = VALUES(min_altitude),
                        modify_sync_version    = VALUES(modify_sync_version),
                        max_heart_rate         = VALUES(max_heart_rate),

                        client_data_id         = VALUES(client_data_id),
                        max_power              = VALUES(max_power),
                        max_speed              = VALUES(max_speed),
                        mean_cadence           = VALUES(mean_cadence),
                        min_heart_rate         = VALUES(min_heart_rate),
                        client_data_version    = VALUES(client_data_version),
                        count_value            = VALUES(count_value),
                        distance               = VALUES(distance),
                        max_caloric_burn_rate  = VALUES(max_caloric_burn_rate),
                        calorie                = VALUES(calorie),
                        max_cadence            = VALUES(max_cadence),
                        decline_distance       = VALUES(decline_distance),
                        vo2_max                = VALUES(vo2_max),
                        time_offset            = VALUES(time_offset),
                        device_uuid            = VALUES(device_uuid),
                        max_rpm                = VALUES(max_rpm),
                        comment_text           = VALUES(comment_text),
                        live_data              = VALUES(live_data),
                        mean_power             = VALUES(mean_power),
                        mean_speed             = VALUES(mean_speed),
                        package_name           = VALUES(package_name),
                        altitude_gain          = VALUES(altitude_gain),
                        altitude_loss          = VALUES(altitude_loss),
                        exercise_custom_type   = VALUES(exercise_custom_type),
                        auxiliary_devices      = VALUES(auxiliary_devices),
                        end_time               = VALUES(end_time),
                        sweat_loss             = VALUES(sweat_loss),

                        update_at              = CURRENT_TIMESTAMP;";
        }

        /// <summary>
        /// Generates the SQL query string to retrieve existing exercise identifiers for synchronization checks.
        /// </summary>
        /// <remarks>Constructs a SELECT statement targeting the <c>tbl_Exercise</c> table to fetch data UUIDs and update timestamps for a specific person.</remarks>
        /// <returns>A string containing the SQL query for existing exercise checks.</returns>
        public string GetExerciseExistenceCheckSql()
        {
            return @"SELECT
               data_uuid,
               update_at
               FROM tbl_Exercise
               WHERE person_id = @PersonId;";
        }

        #endregion

        #region Samsung Health Food Intake SQL Commands

        /// <summary>
        /// Generates the SQL command string for inserting or updating Samsung Health food intake records using an upsert pattern.
        /// </summary>
        /// <remarks>Constructs an INSERT statement targeting the <c>tbl_FoodIntake</c> table with conditional <c>ON DUPLICATE KEY UPDATE</c> logic based on update timestamps.</remarks>
        /// <returns>A string containing the parameterized SQL upsert query for food intake data.</returns>
        public string GetSamsungFoodIntakeUpsertSql()
        {
            return @"INSERT INTO tbl_FoodIntake(
                person_id,
                create_sync_version,
                start_time,

                amount_current,
                amount_initial,

                custom_text,
                modify_sync_version,

                update_at,
                create_at,

                meal_type,
                client_data_id,
                food_name,
                unit_id,
                client_data_version,

                calories_current,
                calories_initial,

                time_offset,
                device_uuid,
                comment_text,
                package_name,
                data_uuid,
                food_info_id
            )
            VALUES
            (
                @PersonID_FK,
                @CreateShVer,
                @StartTime,

                @Amount,
                @Amount,

                @Custom,
                @ModifyShVer,

                @UpdateTime,
                @CreateTime,

                @MealType,
                @ClientDataId,
                @Name,
                @Unit,
                @ClientDataVer,

                @Calorie,
                @Calorie,

                @TimeOffset,
                @DeviceUuid,
                @Comment,
                @PkgName,
                @DataUuid,
                @FoodInfoId
            )
            ON DUPLICATE KEY UPDATE

                person_id           = VALUES(person_id),
                create_sync_version = VALUES(create_sync_version),
                start_time          = VALUES(start_time),

                amount_current      = VALUES(amount_current),

                custom_text         = VALUES(custom_text),
                modify_sync_version = VALUES(modify_sync_version),

                meal_type           = VALUES(meal_type),
                client_data_id      = VALUES(client_data_id),
                food_name           = VALUES(food_name),
                unit_id             = VALUES(unit_id),
                client_data_version = VALUES(client_data_version),

                calories_current    = VALUES(calories_current),

                time_offset         = VALUES(time_offset),
                device_uuid         = VALUES(device_uuid),
                comment_text        = VALUES(comment_text),
                package_name        = VALUES(package_name),
                food_info_id        = VALUES(food_info_id),

                update_at           = CURRENT_TIMESTAMP;";
        }

        /// <summary>
        /// Generates the SQL command string for updating existing Samsung Health food intake records in the database.
        /// </summary>
        /// <remarks>Constructs an UPDATE statement targeting the <c>tbl_FoodIntake</c> table using parameterized nutritional attributes, filtered by data UUID and person ID.</remarks>
        /// <returns>A string containing the parameterized SQL update query for food intake.</returns>
        public string GetSamsungHealthFoodIntakeUpdateSql()
        {
            return @"UPDATE tbl_FoodIntake
            SET
                create_sync_version = @CreateShVer,
                start_time = @StartTime,

                amount_current = @Amount,

                custom_text = @Custom,
                modify_sync_version = @ModifyShVer,

                update_at = @UpdateTime,
                create_at = @CreateTime,

                meal_type = @MealType,
                client_data_id = @ClientDataId,
                food_name = @Name,
                unit_id = @Unit,
                client_data_version = @ClientDataVer,

                calories_current = @Calorie,

                time_offset = @TimeOffset,
                device_uuid = @DeviceUuid,
                comment_text = @Comment,
                package_name = @PkgName,
                food_info_id = @FoodInfoId

            WHERE
                data_uuid = @DataUuid
                AND person_id = @PersonID_FK;";
        }

        /// <summary>
        /// Generates the SQL query string to retrieve existing food intake identifiers for synchronization checks.
        /// </summary>
        /// <remarks>Constructs a SELECT statement targeting the <c>tbl_FoodIntake</c> table to fetch data UUIDs and update timestamps for a specific person.</remarks>
        /// <returns>A string containing the SQL query for existing food intake checks.</returns>
        public string GetFoodIntakeExistenceCheckSql()
        {
            return @"SELECT
               data_uuid,
               update_at
               FROM tbl_FoodIntake
               WHERE person_id = @PersonId;";
        }

        #endregion

        #region Heavy App SQL Commands

        /// <summary>
        /// Generates the SQL command string for inserting or updating Hevy workout application records using an upsert pattern.
        /// </summary>
        /// <remarks>Constructs an INSERT statement targeting the <c>tbl_HeavyApp</c> table with an <c>ON DUPLICATE KEY UPDATE</c> 
        /// clause that refreshes workout set details and sets the update timestamp.</remarks>
        /// <returns>A string containing the parameterized SQL upsert query for workout tracking data.</returns>
        public string GetWorkoutUpsertSql()
        {
            return @"INSERT INTO tbl_HeavyApp(
                person_id,
                workout_title,
                start_time,
                end_time,
                update_at,
                description,
                exercise_title,
                superset_id,
                exercise_notes,
                set_index,
                set_type,

                weight_current,
                weight_initial,

                reps_current,
                reps_initial,

                distance_current,
                distance_initial,

                duration_sec_current,
                duration_sec_initial,

                rpe_current,
                rpe_initial

            )
            VALUES
            (
                @PersonID_FK,
                @Title,
                @StartTime,
                @EndTime,
                @StartTime,
                @Description,
                @ExerciseTitle,
                @SupersetId,
                @ExerciseNotes,
                @SetIndex,
                @SetType,

                @WeightKg,
                @WeightKg,

                @Reps,
                @Reps,

                @DistanceKm,
                @DistanceKm,

                @DurationSeconds,
                @DurationSeconds,

                @Rpe,
                @Rpe
            )
            ON DUPLICATE KEY UPDATE

                workout_title = VALUES(workout_title),
                description = VALUES(description),
                exercise_notes = VALUES(exercise_notes),
                superset_id = VALUES(superset_id),
                set_type = VALUES(set_type),

                weight_current = VALUES(weight_current),
                reps_current = VALUES(reps_current),
                distance_current = VALUES(distance_current),
                duration_sec_current = VALUES(duration_sec_current),
                rpe_current = VALUES(rpe_current),

                update_at = CURRENT_TIMESTAMP;";
        }

        /// <summary>
        /// Generates the SQL command string for updating existing Hevy workout set records in the database.
        /// </summary>
        /// <remarks>Constructs an UPDATE statement targeting the <c>tbl_HeavyApp</c> table using parameterized workout metrics and metadata, 
        /// filtered by person ID, start time, exercise title, and set index.</remarks>
        /// <returns>A string containing the parameterized SQL update query for Hevy workouts.</returns>
        public string GetWorkoutUpdateSql()
        {
            return @"UPDATE tbl_HeavyApp
                    SET
                        workout_title = @Title,
                        start_time = @StartTime,
                        end_time = @EndTime,
                        update_at = CURRENT_TIMESTAMP,
                        description = @Description,
                        superset_id = @SupersetId,
                        exercise_notes = @ExerciseNotes,
                        set_type = @SetType,

                        weight_current = @WeightKg,
                        reps_current = @Reps,
                        distance_current = @DistanceKm,
                        duration_sec_current = @DurationSeconds,
                        rpe_current = @Rpe

                    WHERE
                        person_id = @PersonID_FK
                        AND start_time = @StartTime
                        AND exercise_title = @ExerciseTitle
                        AND set_index = @SetIndex;";
        }

        /// <summary>
        /// Generates the SQL query string to retrieve existing Hevy workout set records for synchronization checks.
        /// </summary>
        /// <remarks>Constructs a SELECT statement targeting the <c>tbl_HeavyApp</c> table to fetch core timing, exercise details, and performance metrics for a specific person.</remarks>
        /// <returns>A string containing the SQL query for existing Hevy workout checks.</returns>
        public string GetWorkoutExistenceCheckSql()
        {
            return @"SELECT
                    start_time,
                    end_time,
                    exercise_title,
                    set_index,
                    weight_current,
                    reps_current,
                    distance_current,
                    duration_sec_current,
                    rpe_current
                    FROM tbl_HeavyApp
                    WHERE person_id = @PersonId;";
        }

        /// <summary>
        /// Generates the SQL query string to retrieve workout set entries from the Hevy application records for a specific person.
        /// </summary>
        /// <remarks>Constructs a SELECT statement targeting the <c>tbl_HeavyApp</c> table, filtering by person ID and ordered by start time, exercise title, and set index.</remarks>
        /// <returns>A string containing the parameterized SQL query for workout entries.</returns>
        public string GetWorkoutEntriesSql()
        {
            return @"SELECT
                start_time,
                exercise_title,
                weight_current,
                reps_current,
                set_index
            FROM tbl_HeavyApp
            WHERE person_id = @PersonID
            ORDER BY start_time,
                     exercise_title,
                     set_index;";
        }

        #endregion
    }
}
