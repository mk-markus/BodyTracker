namespace BodyTracker.Services
{
    public class SqlCommandProvider : ISqlCommandProvider
    {
        public string CmdCreatePerson()
        {
            return "INSERT INTO tbl_Personen (Vorname, Nachname, Geburtsdatum) VALUES (@v, @n, @g); SELECT LAST_INSERT_ID();";
        }

        public string CmdCreateTableIfNotExist()
        {
            return @"CREATE TABLE IF NOT EXISTS tbl_Personen (
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
        }

        public string CmdDeletePersonDimension()
        {
            return "DELETE FROM tbl_Abmessungen WHERE AbmessungID=@id";
        }

        public string CmdDeletePersonMetric()
        {
            return "DELETE FROM tbl_KoerperMetriken WHERE MetrikID=@id";
        }

        public string CmdGetLastPersonDimension()
        {
            return @"SELECT AbmessungID, PersonID_FK, Messdatum, Brustumfang_cm, Bauchumfang_cm, Hueftumfang_cm
                        FROM tbl_Abmessungen
                        WHERE PersonID_FK=@pid AND DATE(Messdatum) < @today
                        ORDER BY Messdatum DESC LIMIT 1";
        }

        public string CmdGetLastPersonMetric()
        {
            return @"SELECT MetrikID, PersonID_FK, Messdatum, Gewicht_kg, BMI, Koerperfett_Prozent, Muskelmasse_Prozent, Viszeralfett
                        FROM tbl_KoerperMetriken
                        WHERE PersonID_FK=@pid AND DATE(Messdatum) < @today
                        ORDER BY Messdatum DESC LIMIT 1";
        }

        public string CmdGetMeasurement()
        {
            return @"SELECT m.MetrikID, a.AbmessungID, m.Messdatum,
               m.Gewicht_kg, m.BMI, m.Koerperfett_Prozent, m.Muskelmasse_Prozent, m.Viszeralfett,
               a.Brustumfang_cm, a.Bauchumfang_cm, a.Hueftumfang_cm
            FROM tbl_KoerperMetriken m
            LEFT JOIN tbl_Abmessungen a ON DATE(m.Messdatum) = DATE(a.Messdatum) AND a.PersonID_FK = m.PersonID_FK
            WHERE m.PersonID_FK = @pid
            ORDER BY m.Messdatum";
        }

        public string CmdGetPerson()
        {
            return "SELECT ID, Vorname, Nachname, Geburtsdatum FROM tbl_Personen ORDER BY Nachname, Vorname";
        }

        public string CmdInsertPersonDimension()
        {
            return @"INSERT INTO tbl_Abmessungen
                        (PersonID_FK, Messdatum, Brustumfang_cm, Bauchumfang_cm, Hueftumfang_cm)
                        VALUES (@pid, @dt, @br, @ba, @hu)";
        }

        public string CmdInsertPersonMetric()
        {
            return @"INSERT INTO tbl_KoerperMetriken
                        (PersonID_FK, Messdatum, Gewicht_kg, BMI, Koerperfett_Prozent, Muskelmasse_Prozent, Viszeralfett)
                        VALUES (@pid, @dt, @gw, @bmi, @kf, @mm, @vf)";
        }

        public string CmdCountPersonsInTable()
        {
            return "SELECT COUNT(*) FROM tbl_Personen";
        }

    }
}
