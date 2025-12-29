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
    Vorname VARCHAR(50),
    Nachname VARCHAR(50),
    Geburtsdatum DATE
);
CREATE TABLE IF NOT EXISTS tbl_KoerperMetriken (
    MetrikID INT AUTO_INCREMENT PRIMARY KEY,
    PersonID_FK INT NOT NULL,
    Messdatum DATETIME DEFAULT CURRENT_TIMESTAMP,
    Gewicht_kg DECIMAL(5,2),
    BMI DECIMAL(4,2),
    Koerperfett_Prozent DECIMAL(4,1),
    Muskelmasse_Prozent DECIMAL(4,1),
    Viszeralfett TINYINT,
    CONSTRAINT FK_PersonMetrik FOREIGN KEY (PersonID_FK) REFERENCES tbl_Personen (ID) ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS tbl_Abmessungen (
    AbmessungID INT AUTO_INCREMENT PRIMARY KEY,
    PersonID_FK INT NOT NULL,
    Messdatum DATETIME DEFAULT CURRENT_TIMESTAMP,
    Brustumfang_cm DECIMAL(5,2),
    Bauchumfang_cm DECIMAL(5,2),
    Hueftumfang_cm DECIMAL(5,2),
    CONSTRAINT FK_PersonAbmessung FOREIGN KEY (PersonID_FK) REFERENCES tbl_Personen (ID) ON DELETE CASCADE
);";
            await using var cmd = new MySqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Person>> GetPersonsAsync()
        {
            var list = new List<Person>();
            await using var conn = new MySqlConnection(_cs);
            await conn.OpenAsync();
            var sql = "SELECT ID, Vorname, Nachname, Geburtsdatum FROM tbl_Personen ORDER BY Nachname, Vorname";
            await using var cmd = new MySqlCommand(sql, conn);
            await using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new Person
                {
                    Id = rdr.GetInt32(0),
                    Vorname = rdr.IsDBNull(1) ? string.Empty : rdr.GetString(1),
                    Nachname = rdr.IsDBNull(2) ? string.Empty : rdr.GetString(2),
                    Geburtsdatum = rdr.IsDBNull(3) ? null : rdr.GetDateTime(3)
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

        public async Task<KoerperMetrik?> GetLastMetrikAsync(int personId, DateTime today)
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
                return new KoerperMetrik
                {
                    MetrikId = rdr.GetInt32(0),
                    PersonId = rdr.GetInt32(1),
                    Messdatum = rdr.GetDateTime(2),
                    GewichtKg = rdr.IsDBNull(3)?(decimal?)null:rdr.GetDecimal(3),
                    Bmi = rdr.IsDBNull(4)?(decimal?)null:rdr.GetDecimal(4),
                    KoerperfettProzent = rdr.IsDBNull(5)?(decimal?)null:rdr.GetDecimal(5),
                    MuskelmasseProzent = rdr.IsDBNull(6)?(decimal?)null:rdr.GetDecimal(6),
                    Viszeralfett = rdr.IsDBNull(7)?(int?)null:rdr.GetInt32(7)
                };
            }
            return null;
        }

        public async Task<Abmessung?> GetLastAbmessungAsync(int personId, DateTime today)
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
                return new Abmessung
                {
                    AbmessungId = rdr.GetInt32(0),
                    PersonId = rdr.GetInt32(1),
                    Messdatum = rdr.GetDateTime(2),
                    BrustumfangCm = rdr.IsDBNull(3)?(decimal?)null:rdr.GetDecimal(3),
                    BauchumfangCm = rdr.IsDBNull(4)?(decimal?)null:rdr.GetDecimal(4),
                    HueftumfangCm = rdr.IsDBNull(5)?(decimal?)null:rdr.GetDecimal(5)
                };
            }
            return null;
        }

        public async Task InsertMetrikAsync(KoerperMetrik m)
        {
            await using var conn = new MySqlConnection(_cs);
            await conn.OpenAsync();
            var sql = @"INSERT INTO tbl_KoerperMetriken
                        (PersonID_FK, Messdatum, Gewicht_kg, BMI, Koerperfett_Prozent, Muskelmasse_Prozent, Viszeralfett)
                        VALUES (@pid, @dt, @gw, @bmi, @kf, @mm, @vf)";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@pid", m.PersonId);
            cmd.Parameters.AddWithValue("@dt", m.Messdatum);
            cmd.Parameters.AddWithValue("@gw", (object?)m.GewichtKg ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@bmi", (object?)m.Bmi ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@kf", (object?)m.KoerperfettProzent ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@mm", (object?)m.MuskelmasseProzent ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@vf", (object?)m.Viszeralfett ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task InsertAbmessungAsync(Abmessung a)
        {
            await using var conn = new MySqlConnection(_cs);
            await conn.OpenAsync();
            var sql = @"INSERT INTO tbl_Abmessungen
                        (PersonID_FK, Messdatum, Brustumfang_cm, Bauchumfang_cm, Hueftumfang_cm)
                        VALUES (@pid, @dt, @br, @ba, @hu)";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@pid", a.PersonId);
            cmd.Parameters.AddWithValue("@dt", a.Messdatum);
            cmd.Parameters.AddWithValue("@br", (object?)a.BrustumfangCm ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ba", (object?)a.BauchumfangCm ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@hu", (object?)a.HueftumfangCm ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<MessungView>> GetMessungenAsync(int personId)
        {
            var list = new List<MessungView>();
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
                list.Add(new MessungView
                {
                    MetrikId = rdr.IsDBNull(0)?(int?)null:rdr.GetInt32(0),
                    AbmessungId = rdr.IsDBNull(1)?(int?)null:rdr.GetInt32(1),
                    Messdatum = rdr.GetDateTime(2),
                    GewichtKg = rdr.IsDBNull(3)?(decimal?)null:rdr.GetDecimal(3),
                    Bmi = rdr.IsDBNull(4)?(decimal?)null:rdr.GetDecimal(4),
                    KoerperfettProzent = rdr.IsDBNull(5)?(decimal?)null:rdr.GetDecimal(5),
                    MuskelmasseProzent = rdr.IsDBNull(6)?(decimal?)null:rdr.GetDecimal(6),
                    Viszeralfett = rdr.IsDBNull(7)?(int?)null:rdr.GetInt32(7),
                    BrustumfangCm = rdr.IsDBNull(8)?(decimal?)null:rdr.GetDecimal(8),
                    BauchumfangCm = rdr.IsDBNull(9)?(decimal?)null:rdr.GetDecimal(9),
                    HueftumfangCm = rdr.IsDBNull(10)?(decimal?)null:rdr.GetDecimal(10)
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
