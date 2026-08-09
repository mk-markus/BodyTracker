namespace BodyTracker.Services
{
    public class SqlCommandProvider : ISqlCommandProvider
    {
        /// <summary>
        /// Generates the SQL command string for inserting a new person record into the database.
        /// </summary>
        /// <remarks>Constructs an INSERT statement for the <c>tbl_Personen</c> table using parameterized values for first name, last name, date of birth, and height, followed by a SELECT statement to retrieve the last inserted identifier.</remarks>
        /// <returns>A string containing the parameterized SQL insert query and ID selection command.</returns>
        public string CmdCreatePerson()
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
        /// Generates the SQL command string for deleting a person's body bodyMeasurement or dimension record by its unique identifier.
        /// </summary>
        /// <remarks>Constructs a parameterized DELETE statement targeting the <c>tbl_Abmessungen</c> table.</remarks>
        /// <returns>A string containing the parameterized SQL delete query for body dimensions.</returns>
        public string CmdDeletePersonDimension()
        {
            return "DELETE FROM tbl_Abmessungen WHERE dimension_id=@id";
        }

        /// <summary>
        /// Generates the SQL command string for deleting a person's body metric record by its unique identifier.
        /// </summary>
        /// <remarks>Constructs a parameterized DELETE statement targeting the <c>tbl_KoerperMetriken</c> table.</remarks>
        /// <returns>A string containing the parameterized SQL delete query for body metrics.</returns>
        public string CmdDeletePersonMetric()
        {
            return "DELETE FROM tbl_KoerperMetriken WHERE metric_id=@id";
        }

        /// <summary>
        /// Generates the SQL query string to retrieve the most recent body dimension or circumference record for a specific person up to a given date.
        /// </summary>
        /// <remarks>Constructs a SELECT statement targeting the <c>tbl_Abmessungen</c> table, filtering by person ID and date boundary, ordered descending by bodyMeasurement date with a limit of one.</remarks>
        /// <returns>A string containing the parameterized SQL query for retrieving the last person dimension record.</returns>
        public string CmdGetLastPersonDimension()
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
        public string CmdGetLastPersonMetric()
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
        public string CmdGetMeasurement()
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

        public string CmdGetInitialMeasurement()
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
        public string CmdGetPerson()
        {
            return "SELECT person_id, person_first_name, person_last_name, person_birth_date, person_height FROM tbl_Personen ORDER BY person_last_name, person_first_name";
        }

        /// <summary>
        /// Generates the SQL command string for inserting a new body dimension or circumference record into the database.
        /// </summary>
        /// <remarks>Constructs an INSERT statement targeting the <c>tbl_Abmessungen</c> table using parameterized values for circumferences and skinfold measurements.</remarks>
        /// <returns>A string containing the parameterized SQL insert query for person dimensions.</returns>
        public string CmdInsertPersonDimension()
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
        public string CmdInsertPersonMetric()
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
        /// Generates the SQL command string for inserting or updating Samsung Health food intake records using an upsert pattern.
        /// </summary>
        /// <remarks>Constructs an INSERT statement targeting the <c>tbl_FoodIntake</c> table with conditional <c>ON DUPLICATE KEY UPDATE</c> logic based on update timestamps.</remarks>
        /// <returns>A string containing the parameterized SQL upsert query for food intake data.</returns>
        public string CmdInsertSamsungHealthFoodIntake()
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
        /// Generates the SQL command string for inserting or updating Samsung Health daily step trend records using an upsert pattern.
        /// </summary>
        /// <remarks>Constructs an INSERT statement targeting the <c>tbl_StepDailyTrend</c> table with conditional <c>ON DUPLICATE KEY UPDATE</c> logic based on update timestamps.</remarks>
        /// <returns>A string containing the parameterized SQL upsert query for daily step trends.</returns>
        public string CmdInsertSamsungHealthStepDailyTrend()
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
        /// Generates the SQL command string for inserting or updating Hevy workout application records using an upsert pattern.
        /// </summary>
        /// <remarks>Constructs an INSERT statement targeting the <c>tbl_HeavyApp</c> table with an <c>ON DUPLICATE KEY UPDATE</c> clause that refreshes workout set details and sets the update timestamp.</remarks>
        /// <returns>A string containing the parameterized SQL upsert query for workout tracking data.</returns>
        public string CmdInsertHeavyApp()
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
        /// Generates the SQL query string to count the total number of person records stored in the database.
        /// </summary>
        /// <remarks>Constructs a <c>SELECT COUNT(*)</c> statement targeting the <c>tbl_Personen</c> table.</remarks>
        /// <returns>A string containing the SQL count query.</returns>
        public string CmdCountPersonsInTable()
        {
            return "SELECT COUNT(*) FROM tbl_Personen";
        }

        /// <summary>
        /// Generates the SQL command string for updating an existing person's body metric record in the database.
        /// </summary>
        /// <remarks>Constructs an UPDATE statement targeting the <c>tbl_KoerperMetriken</c> table using parameterized values for weight, BMI, body fat percentages, muscle mass, body water, bone mass, and visceral fat, filtered by metric ID.</remarks>
        /// <returns>A string containing the parameterized SQL update query for person metrics.</returns>
        public string CmdUpdatePersonMetric()
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
        /// <remarks>Constructs an UPDATE statement targeting the <c>tbl_Abmessungen</c> table using parameterized values for circumferences and skinfold measurements, filtered by dimension ID.</remarks>
        /// <returns>A string containing the parameterized SQL update query for person dimensions.</returns>
        public string CmdUpdatePersonDimension()
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
        /// <remarks>Constructs a <c>SELECT COUNT(1)</c> statement targeting the <c>tbl_KoerperMetriken</c> table, filtering by person ID and a half-open date interval.</remarks>
        /// <returns>A string containing the parameterized SQL existence check query.</returns>
        public string CmdCheckIfPersonHasMeasurementsExists()
        {
            return @"SELECT COUNT(1) FROM tbl_KoerperMetriken WHERE person_id = @pid AND create_at >= @start AND create_at < @end";
        }

        /// <summary>
        /// Generates the SQL command string for updating an existing person's details in the database.
        /// </summary>
        /// <remarks>Constructs an UPDATE statement targeting the <c>tbl_Personen</c> table using parameterized values for first name, last name, date of birth, and height, filtered by person ID.</remarks>
        /// <returns>A string containing the parameterized SQL update query for person details.</returns>
        public string CmdUpdatePerson()
        {
            return "UPDATE tbl_Personen SET person_first_name=@v, person_last_name=@n, person_birth_date=@g, person_height=@k WHERE person_id=@pid";
        }

        /// <summary>
        /// Generates the SQL query string to retrieve Samsung Health daily step trend records for a specific person.
        /// </summary>
        /// <remarks>Constructs a SELECT statement targeting the <c>tbl_StepDailyTrend</c> table, filtering by person ID and source type <c>-2</c>, ordered ascending by creation time.</remarks>
        /// <returns>A string containing the parameterized SQL query for step trends.</returns>
        public string CmdGetSamsungHealthStepDailyTrend()
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
        /// Generates the SQL query string to retrieve workout set entries from the Hevy application records for a specific person.
        /// </summary>
        /// <remarks>Constructs a SELECT statement targeting the <c>tbl_HeavyApp</c> table, filtering by person ID and ordered by start time, exercise title, and set index.</remarks>
        /// <returns>A string containing the parameterized SQL query for workout entries.</returns>
        public string CmdGetHeavyAppWorkoutEntries()
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
    }
}
