namespace BodyTracker.Services
{
    public class SqlCommandProvider : ISqlCommandProvider
    {
        public string CmdCreatePerson()
        {
            return "INSERT INTO tbl_Personen (Vorname, Nachname, Geburtsdatum, Koerpergroesse) VALUES (@v, @n, @g, @k); SELECT LAST_INSERT_ID();";
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
	MetrikID INT(11) NOT NULL AUTO_INCREMENT,
	PersonID_FK INT(11) NOT NULL,
	Messdatum DATETIME NULL DEFAULT current_timestamp(),
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
	UNIQUE INDEX UQ_Metrik_Person_Datum (PersonID_FK, Messdatum) USING BTREE,
	CONSTRAINT FK_PersonMetrik FOREIGN KEY (PersonID_FK) REFERENCES tbl_Personen (ID) ON UPDATE RESTRICT ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS tbl_Abmessungen (
    AbmessungID INT AUTO_INCREMENT PRIMARY KEY,
    PersonID_FK INT NOT NULL,
    Messdatum DATETIME DEFAULT CURRENT_TIMESTAMP,
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
            return @"SELECT AbmessungID, PersonID_FK, Messdatum, Brustumfang_cm, Bauchumfang_cm, Hueftumfang_cm, Brustfalte_mm, Achselfalte_mm, Bauchfalte_mm, 
                    Hüftfalte_mm, Oberschenkelfalte_mm, Rückenfalte_mm, Trizepsfalte_mm
                        FROM tbl_Abmessungen
                        WHERE PersonID_FK=@pid AND DATE(Messdatum) <= @today
                        ORDER BY Messdatum DESC LIMIT 1";
        }

        public string CmdGetLastPersonMetric()
        {
            return @"SELECT MetrikID, PersonID_FK, Messdatum, Gewicht_kg, BMI, Koerperfett_Prozent, Koerperfett_oben_Prozent, Koerperfett_unten_Prozent,
                            Muskelmasse_Prozent, Muskelmasse_oben_Prozent, Muskelmasse_unten_Prozent, Koerperknochen_Masse, Koerperwasser_Prozent, Viszeralfett
                        FROM tbl_KoerperMetriken
                        WHERE PersonID_FK=@pid AND DATE(Messdatum) <= @today
                        ORDER BY Messdatum DESC LIMIT 1";
        }

        public string CmdGetMeasurement()
        {
            return @"SELECT m.MetrikID, a.AbmessungID, m.Messdatum, m.Gewicht_kg, m.BMI, 
                m.Koerperfett_Prozent, m.Koerperfett_oben_Prozent, m.Koerperfett_unten_Prozent, 
                m.Muskelmasse_Prozent, m.Muskelmasse_oben_Prozent, m.Muskelmasse_unten_Prozent, m.Koerperknochen_Masse, m.Koerperwasser_Prozent, m.Viszeralfett,
                a.Brustumfang_cm, a.Bauchumfang_cm, a.Hueftumfang_cm, a.Brustfalte_mm, a.Achselfalte_mm, a.Bauchfalte_mm, a.Hüftfalte_mm, a.Oberschenkelfalte_mm, a.Rückenfalte_mm, a.Trizepsfalte_mm
            FROM tbl_KoerperMetriken m
            LEFT JOIN tbl_Abmessungen a ON DATE(m.Messdatum) = DATE(a.Messdatum) AND a.PersonID_FK = m.PersonID_FK
            WHERE m.PersonID_FK = @pid
            ORDER BY m.Messdatum";
        }

        public string CmdGetPerson()
        {
            return "SELECT ID, Vorname, Nachname, Geburtsdatum, Koerpergroesse FROM tbl_Personen ORDER BY Nachname, Vorname";
        }

        public string CmdInsertPersonDimension()
        {
            return @"INSERT INTO tbl_Abmessungen
                        (PersonID_FK, Messdatum, Brustumfang_cm, Bauchumfang_cm, Hueftumfang_cm, Brustfalte_mm, Achselfalte_mm, Bauchfalte_mm, Hüftfalte_mm, Oberschenkelfalte_mm, 
                         Rückenfalte_mm, Trizepsfalte_mm)
                        VALUES (@pid, @dt, @br, @ba, @hu, @fzbreast, @fzarmpit, @fzabdomen, @fzhip, @fzthigh, @fzback, @fztricep)";
        }

        public string CmdInsertPersonMetric()
        {
            return @"INSERT INTO tbl_KoerperMetriken
                        (PersonID_FK, Messdatum, Gewicht_kg, BMI, Koerperfett_Prozent, Koerperfett_oben_Prozent, Koerperfett_unten_Prozent, 
                        Muskelmasse_Prozent, Muskelmasse_oben_Prozent, Muskelmasse_unten_Prozent, Koerperknochen_Masse, Koerperwasser_Prozent, Viszeralfett)
                        VALUES (@pid, @dt, @gw, @bmi, @kf, @kfo, @kfu, @mm, @mmo, @mmu, @kk, @kw, @vf)";
        }

        public string CmdCountPersonsInTable()
        {
            return "SELECT COUNT(*) FROM tbl_Personen";
        }

        
        public string CmdUpdatePersonMetric()
        {
            return @"UPDATE tbl_KoerperMetriken SET Gewicht_kg=@gw, BMI=@bmi, 
                    Koerperfett_Prozent=@kf, Koerperfett_oben_Prozent=@kfo, Koerperfett_unten_Prozent=@kfu, 
                    Muskelmasse_Prozent=@mm, Muskelmasse_oben_Prozent=@mmo, Muskelmasse_unten_Prozent=@mmu, 
                    Koerperwasser_Prozent=@kw, Koerperknochen_Masse=@kk, Viszeralfett=@vf WHERE MetrikID=@mid;";
        }
        
        public string CmdUpdatePersonDimension()
        {
            return @"UPDATE tbl_Abmessungen SET Brustumfang_cm=@br, Bauchumfang_cm=@ba, Hueftumfang_cm=@hu, Brustfalte_mm=@fzbreast, Achselfalte_mm=@fzarmpit,
                    Bauchfalte_mm=@fzabdomen, Hüftfalte_mm=@fzhip, Oberschenkelfalte_mm=@fzthigh, Rückenfalte_mm=@fzback, Trizepsfalte_mm=@fztricep WHERE AbmessungID=@aid;";
        }

        public string CmdCheckIfPersonHasMeasurementsExists()
        {
            return @"SELECT COUNT(1) FROM tbl_KoerperMetriken WHERE PersonID_FK = @pid AND Messdatum >= @start AND Messdatum < @end";
        }


        public string CmdUpdatePerson()
        {
            return "UPDATE tbl_Personen SET Vorname=@v, Nachname=@n, Geburtsdatum=@g, Koerpergroesse=@k WHERE ID=@pid";
        }

    }
}
