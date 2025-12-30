using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using BodyTracker.Models;

namespace BodyTracker.Services
{
    public class DatabaseService
    {
        private readonly string _cs;
        public DatabaseService(string connectionString) { _cs = connectionString; }

        public async Task InitializeAsync()
        {
            await using var conn = new MySqlConnection(_cs);
            await conn.OpenAsync();
            var sql = @"CREATE TABLE IF NOT EXISTS tbl_Personen (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    PersonFirstName VARCHAR(50),
    PersonLastName VARCHAR(50),
    PersonBirthDate DATE
);
CREATE TABLE IF NOT EXISTS tbl_KoerperMetriken (
    MetrikID INT AUTO_INCREMENT PRIMARY KEY,
    PersonID_FK INT NOT NULL,
    MeasurementDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    Gewicht_kg DECIMAL(5,2),
    BMI DECIMAL(4,2),
    Koerperfett_Prozent DECIMAL(4,1),
    Muskelmasse_Prozent DECIMAL(4,1),
    BodyVisceralFat TINYINT,
    CONSTRAINT FK_PersonMetrik FOREIGN KEY (PersonID_FK) REFERENCES tbl_Personen (ID) ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS tbl_Abmessungen (
    AbmessungID INT AUTO_INCREMENT PRIMARY KEY,
    PersonID_FK INT NOT NULL,
    MeasurementDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    Brustumfang_cm DECIMAL(5,2),
    Bauchumfang_cm DECIMAL(5,2),
    Hueftumfang_cm DECIMAL(5,2),
    CONSTRAINT FK_PersonAbmessung FOREIGN KEY (PersonID_FK) REFERENCES tbl_Personen (ID) ON DELETE CASCADE
);";
            await using var cmd = new MySqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<PersonModel>> GetPersonsAsync()
        {
            var list = new List<PersonModel>();
            await using var conn = new MySqlConnection(_cs);
            await conn.OpenAsync();
            var sql = "SELECT ID, Vorname, Nachname, Geburtsdatum FROM tbl_Personen ORDER BY Nachname, Vorname";
            await using var cmd = new MySqlCommand(sql, conn);
            await using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new PersonModel
                {
                    PersonID = rdr.GetInt32(0),
                    PersonFirstName = rdr.IsDBNull(1) ? string.Empty : rdr.GetString(1),
                    PersonLastName = rdr.IsDBNull(2) ? string.Empty : rdr.GetString(2),
                    PersonBirthDate = rdr.IsDBNull(3) ? null : rdr.GetDateTime(3)
                });
            }
            return list;
        }

        public async Task<int> CreatePersonAsync(string vorname, string nachname, DateTime? geburtsdatum)
        {
            await using var conn = new MySqlConnection(_cs);
            await conn.OpenAsync();
            var sql = "INSERT INTO tbl_Personen (Vorname, Nachname, Geburtsdatum) VALUES (@v, @n, @g); SELECT LAST_INSERT_ID();";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@v", vorname);
            cmd.Parameters.AddWithValue("@n", nachname);
            cmd.Parameters.AddWithValue("@g", geburtsdatum.HasValue ? geburtsdatum.Value : (object)DBNull.Value);
            var idObj = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(idObj);
        }

        public async Task<BodyMetricModel?> GetLastMetrikAsync(int personId, DateTime today)
        {
            await using var conn = new MySqlConnection(_cs);
            await conn.OpenAsync();
            var sql = @"SELECT MetrikID, PersonID_FK, Messdatum, Gewicht_kg, BMI, Koerperfett_Prozent, Muskelmasse_Prozent, Viszeralfett
                        FROM tbl_KoerperMetriken
                        WHERE PersonID_FK=@pid AND DATE(Messdatum) < @today
                        ORDER BY Messdatum DESC LIMIT 1";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@pid", personId);
            cmd.Parameters.AddWithValue("@today", today.Date);
            await using var rdr = await cmd.ExecuteReaderAsync();
            if (await rdr.ReadAsync())
            {
                return new BodyMetricModel
                {
                    MetricID = rdr.GetInt32(0),
                    PersonID = rdr.GetInt32(1),
                    MeasurementDate = rdr.GetDateTime(2),
                    BodyWeight = rdr.IsDBNull(3)?(float?)null:rdr.GetFloat(3),
                    BMI = rdr.IsDBNull(4)?(float?)null:rdr.GetFloat(4),
                    BodyFatPercentage = rdr.IsDBNull(5)?(float?)null:rdr.GetFloat(5),
                    BodyMusclePercentage = rdr.IsDBNull(6)?(float?)null:rdr.GetFloat(6),
                    BodyVisceralFat = rdr.IsDBNull(7)?(int?)null:rdr.GetInt32(7)
                };
            }
            return null;
        }

        public async Task<BodyDimensionsModel?> GetLastAbmessungAsync(int personId, DateTime today)
        {
            await using var conn = new MySqlConnection(_cs);
            await conn.OpenAsync();
            var sql = @"SELECT AbmessungID, PersonID_FK, Messdatum, Brustumfang_cm, Bauchumfang_cm, Hueftumfang_cm
                        FROM tbl_Abmessungen
                        WHERE PersonID_FK=@pid AND DATE(Messdatum) < @today
                        ORDER BY Messdatum DESC LIMIT 1";
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
            await using var conn = new MySqlConnection(_cs);
            await conn.OpenAsync();
            var sql = @"INSERT INTO tbl_KoerperMetriken
                        (PersonID_FK, Messdatum, Gewicht_kg, BMI, Koerperfett_Prozent, Muskelmasse_Prozent, Viszeralfett)
                        VALUES (@pid, @dt, @gw, @bmi, @kf, @mm, @vf)";
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
            await using var conn = new MySqlConnection(_cs);
            await conn.OpenAsync();
            var sql = @"INSERT INTO tbl_Abmessungen
                        (PersonID_FK, Messdatum, Brustumfang_cm, Bauchumfang_cm, Hueftumfang_cm)
                        VALUES (@pid, @dt, @br, @ba, @hu)";
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
            await using var conn = new MySqlConnection(_cs);
            await conn.OpenAsync();
            var sql = @"SELECT m.MetrikID, a.AbmessungID, m.Messdatum,
               m.Gewicht_kg, m.BMI, m.Koerperfett_Prozent, m.Muskelmasse_Prozent, m.Viszeralfett,
               a.Brustumfang_cm, a.Bauchumfang_cm, a.Hueftumfang_cm
            FROM tbl_KoerperMetriken m
            LEFT JOIN tbl_Abmessungen a ON DATE(m.Messdatum) = DATE(a.Messdatum) AND a.PersonID_FK = m.PersonID_FK
            WHERE m.PersonID_FK = @pid
            ORDER BY m.Messdatum";
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
            await using var conn = new MySqlConnection(_cs);
            await conn.OpenAsync();
            var cmd = new MySqlCommand("DELETE FROM tbl_KoerperMetriken WHERE MetrikID=@id", conn);
            cmd.Parameters.AddWithValue("@id", metrikId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAbmessungAsync(int abmessungId)
        {
            await using var conn = new MySqlConnection(_cs);
            await conn.OpenAsync();
            var cmd = new MySqlCommand("DELETE FROM tbl_Abmessungen WHERE AbmessungID=@id", conn);
            cmd.Parameters.AddWithValue("@id", abmessungId);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
