using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using MySql.Data;
using MySql;
using MySql.Data.MySqlClient;
using System.Data;

namespace Aanwezigheden
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    public class PresenceService : IPresenceService
    {
        //server=mysql5.kskh-jeugd.be;database=socad;user id=kskhnet;password=KHnr1ste; pooling=false
        //server=mysql5.hoobergs.be;database=kskhadmindevelop;user id=kskhdevelop;password=KHnr1ste; pooling=false
        //server=mysql5.hoobergs.be;database=SocadSpalbeek;user id=KurBon;password=Coor@Spal!; pooling=false
        string connectionString = "server=mysql5.kskh-jeugd.be;database=socad;user id=kskhnet;password=KHnr1ste; pooling=false";

        private List<TResult> ExecuteCommand<TResult>(string sqlStatement, Func<IDataReader, TResult> mapper)
        {
            var result = new List<TResult>();

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = sqlStatement;

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    result.Add(mapper(reader));
                }

                connection.Close();
            }

            return result;
        }

        public List<Ploeg> getPloegen()
        {
            return ExecuteCommand<Ploeg>("SELECT * FROM ploeg order by displaysequence", 
                                         (reader) => new Ploeg() 
                                                   { 
                                                     PloegId = Convert.ToInt32(reader["ploegid"]), 
                                                     Name = Convert.ToString(reader["PloegNaam"]),
                                                     DisplaySequence = Convert.ToInt32(reader["DisplaySequence"]) 
                                                   });
        }

        public Int16 getNextSequence(string tablename)
        {
            Int16 next_value = -99;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                string sql = string.Format("SELECT * FROM sequence where name = '{0}'",tablename);
                command.CommandText = sql;
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    next_value = Convert.ToInt16(reader["next_value"]);
                }
                connection.Close();
            }
            return next_value;
        }

        public void setNextSequence(int next_value, string tablename)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                string sql = string.Format("UPDATE sequence SET next_value = {0} WHERE name = '{1}'", next_value,tablename);
                command.CommandText = sql;
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    next_value = Convert.ToInt16(reader["next_value"]);
                }
                connection.Close();
            }
        }



        public List<Lid> getSpelers(int ploegId)
        {
            //SELECT ploegxlid.PloegId,lid.Naam,lid.Voornaam FROM ploegxlid Inner Join lid ON ploegxlid.LidId = lid.LidId WHERE ploegxlid.PloegId =  '8'

            List<Lid> leden = new List<Lid>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                string sql = string.Format("SELECT ploegxlid.PloegId,lid.lidid,lid.Naam,lid.Voornaam FROM ploegxlid Inner Join lid ON ploegxlid.LidId = lid.LidId WHERE ploegxlid.PloegId =  '{0}' order by lid.naam",ploegId);
                command.CommandText = sql;
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Lid lid = new Lid() { LidId = Convert.ToInt32(reader["lidid"]), 
                                          Naam = Convert.ToString(reader["Naam"]),
                                          Voornaam = Convert.ToString(reader["Voornaam"]) };
                    leden.Add(lid);
                }
                connection.Close();
            }
            return leden;
        }

        public DateTime getLastDataEntry(int ploegId)
        {
            //SELECT aanwezigheid.datum FROM aanwezigheid WHERE aanwezigheid.PloegId = '8' ORDER BY aanwezigheid.datum DESC limit 1
            DateTime lastDataEntryDate = DateTime.Today;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                string sql = string.Format("SELECT aanwezigheid.datum FROM aanwezigheid WHERE aanwezigheid.PloegId = {0} ORDER BY aanwezigheid.datum DESC limit 1", ploegId);
                command.CommandText = sql;
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    lastDataEntryDate = Convert.ToDateTime(reader["Datum"]);
                }
                connection.Close();
            }
            return lastDataEntryDate;
        }

        public List<Wedstrijd> getWedstrijd(int ploegId, DateTime lastDataEntryDate, Boolean previous, Boolean current, Boolean next)
        {
            //SELECT wedstrijd.WedstrijdId,wedstrijd.Tegenstander,wedstrijd.WedstrijdDatum FROM wedstrijd WHERE ploegid = 8 and  WedstrijdDatum >= '2010-07-21' and WedstrijdDatum <= 'today' order by wedstrijddatum  limit 1
            DateTime tempDate = DateTime.Today;
            List<Wedstrijd> wedstrijden = new List<Wedstrijd>();
            string sql = string.Empty;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                if (next)
                    sql = string.Format("SELECT wedstrijd.WedstrijdId,wedstrijd.Tegenstander,wedstrijd.WedstrijdDatum,wedstrijd.thuis,wedstrijd.uitslag FROM wedstrijd WHERE ploegid = {0} and  WedstrijdDatum > '{1}-{2}-{3}' and WedstrijdDatum <= 'today' and wedstrijdtype <> 'L' order by wedstrijddatum  limit 1", ploegId, lastDataEntryDate.Year, lastDataEntryDate.Month, lastDataEntryDate.Day);
                if (previous)
                    sql = string.Format("SELECT wedstrijd.WedstrijdId,wedstrijd.Tegenstander,wedstrijd.WedstrijdDatum,wedstrijd.thuis,wedstrijd.uitslag FROM wedstrijd WHERE ploegid = {0} and  WedstrijdDatum < '{1}-{2}-{3}' and wedstrijdtype <> 'L'  order by wedstrijddatum desc limit 1", ploegId, lastDataEntryDate.Year, lastDataEntryDate.Month, lastDataEntryDate.Day);
                if (current)
                    sql = string.Format("SELECT wedstrijd.WedstrijdId,wedstrijd.Tegenstander,wedstrijd.WedstrijdDatum,wedstrijd.thuis,wedstrijd.uitslag FROM wedstrijd WHERE ploegid = {0} and  WedstrijdDatum = '{1}-{2}-{3}' and wedstrijdtype <> 'L' ", ploegId, lastDataEntryDate.Year, lastDataEntryDate.Month, lastDataEntryDate.Day);
                command.CommandText = sql;
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Wedstrijd wedstrijd = new Wedstrijd()
                    {
                        WedstrijdId = Convert.ToInt32(reader["WedstrijdId"]),
                        Tegenstander = Convert.ToString(reader["Tegenstander"]),
                        WedstrijdDatum = Convert.ToDateTime(reader["WedstrijdDatum"]),
                        Thuis = Convert.ToBoolean(reader["thuis"]),
                        Uitslag = Convert.ToString(reader["uitslag"])
                    };
                    tempDate = wedstrijd.WedstrijdDatum;
                    wedstrijden.Add(wedstrijd);
                }
                connection.Close();
            }
            return wedstrijden;
        }

        public DateTime getNextTrainingsDatum(int ploegId, DateTime lastDataEntryDate)
        {
            Boolean dateResolved = false;
            int laatsteIngaveDag = 0;
            DateTime nextTrainingsdatum = DateTime.Today;
            //SELECT ploegactiviteit.Dag, ploegactiviteit.Activiteit, ploegactiviteit.PloegId FROM ploegactiviteit WHERE ploegactiviteit.PloegId = '8' 
            List<PloegActiviteit> trainingen = new List<PloegActiviteit>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                string sql = string.Format("SELECT ploegactiviteit.Dag, ploegactiviteit.Activiteit, ploegactiviteit.PloegId FROM ploegactiviteit WHERE ploegactiviteit.PloegId = '{0}' and ploegactiviteit.activiteit = 'T' order by dag", ploegId);
                command.CommandText = sql;
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    PloegActiviteit training = new PloegActiviteit()
                    {
                        Dag = Convert.ToInt16(reader["Dag"])
                    };
                    trainingen.Add(training);
                }
                connection.Close();

                //zoek de dagnummer van de laatste ingave (of van de huidige dag als er geen ingaves zijn, opgevangen door method getLastDataEntry)
                laatsteIngaveDag = ((int)lastDataEntryDate.DayOfWeek);
                if (laatsteIngaveDag == 0)
                    laatsteIngaveDag = 7;  //voor zondag omdat die 0 is en bij mij in socad 7
               
                //zoek of er een hogere dagnummer is
                foreach (PloegActiviteit training in trainingen)
                {
                    if (training.Dag > laatsteIngaveDag)
                    {
                        //resolve datum
                        nextTrainingsdatum = lastDataEntryDate.AddDays(training.Dag - laatsteIngaveDag);
                        dateResolved = true;
                        break;
                    }
                }
                
                //als geen hogere dagnummer gevonden is wil het zeggen , dat we gewoon de eerste training van de week moeten nemen
                if (!dateResolved)
                {
                    //resolvedatum
                    nextTrainingsdatum = lastDataEntryDate.AddDays((7 - laatsteIngaveDag) + trainingen[0].Dag);
                }

            }
            return nextTrainingsdatum;
        }

        public List<Aanwezigheid> getNextDataEntry(int ploegId)
        {
            List<Wedstrijd> _wedstrijden;
            List<Aanwezigheid> _aanwezigheden = new List<Aanwezigheid>();
            DateTime lastDataEntryDate;
            DateTime nextTrainingsDate;
            lastDataEntryDate = getLastDataEntry(ploegId);
            _wedstrijden = getWedstrijd(ploegId, lastDataEntryDate,false,false,true);
            nextTrainingsDate = getNextTrainingsDatum(ploegId, lastDataEntryDate);
            if (_wedstrijden[0].WedstrijdDatum <= nextTrainingsDate)
            {
                if (_wedstrijden[0].WedstrijdDatum <= DateTime.Today)
                {
                    Aanwezigheid _aanwezigheid = new Aanwezigheid()
                    {
                        Datum = _wedstrijden[0].WedstrijdDatum,
                        Type = "W",
                        Tegenstander = _wedstrijden[0].Tegenstander
                    };
                    _aanwezigheden.Add(_aanwezigheid);
                }
                else
                {
                    Aanwezigheid _aanwezigheid = new Aanwezigheid()
                    {
                        Datum = DateTime.Today,
                        Type = "T"
                    };
                    _aanwezigheden.Add(_aanwezigheid);
                }
            }
            else
            {
                Aanwezigheid _aanwezigheid = new Aanwezigheid()
                {
                    Datum = nextTrainingsDate > DateTime.Today ? DateTime.Today:nextTrainingsDate,
                    Type = "T"
                };
                _aanwezigheden.Add(_aanwezigheid);
                
            }
            return _aanwezigheden;
        }

        public List<Aanwezigheid> getNextDataEntryWDatum(int ploegId, DateTime lastDataEntryDate)
        {
            List<Wedstrijd> _wedstrijden;
            List<Aanwezigheid> _aanwezigheden = new List<Aanwezigheid>();
            DateTime nextTrainingsDate;
            _wedstrijden = getWedstrijd(ploegId, lastDataEntryDate,false,false,true);
            nextTrainingsDate = getNextTrainingsDatum(ploegId, lastDataEntryDate);
            if (_wedstrijden[0].WedstrijdDatum <= nextTrainingsDate)
            {
                if (_wedstrijden[0].WedstrijdDatum <= DateTime.Today)
                {
                    Aanwezigheid _aanwezigheid = new Aanwezigheid()
                    {
                        Datum = _wedstrijden[0].WedstrijdDatum,
                        Type = "W",
                        Tegenstander = _wedstrijden[0].Tegenstander
                    };
                    _aanwezigheden.Add(_aanwezigheid);
                }
                else
                {
                    Aanwezigheid _aanwezigheid = new Aanwezigheid()
                    {
                        Datum = DateTime.Today,
                        Type = "T"
                    };
                    _aanwezigheden.Add(_aanwezigheid);
                }
            }
            else
            {
                Aanwezigheid _aanwezigheid = new Aanwezigheid()
                {
                    Datum = nextTrainingsDate > DateTime.Today ? DateTime.Today : nextTrainingsDate,
                    Type = "T",

                };
                _aanwezigheden.Add(_aanwezigheid);
            }
            return _aanwezigheden;
        }

        public List<AanwezigheidSpeler> getAanwezighedenByPloegAndDatum(int ploegId, DateTime datum)
        {
            //SELECT aanwezigheid.datum,aanwezigheid.ActiviteitType,aanwezigheidspelers.Status,lid.Naam,lid.Voornaam FROM aanwezigheid 
            //Inner Join aanwezigheidspelers ON aanwezigheid.AanwezigheidId = aanwezigheidspelers.AanwezigheidId 
            //Inner Join lid ON aanwezigheidspelers.LidId = lid.LidId 
            //WHERE aanwezigheid.PloegId =  '8' AND aanwezigheid.datum =  '2011-07-18'
            MySqlDataReader reader;
            List<AanwezigheidSpeler> aanwezigheden = new List<AanwezigheidSpeler>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                string sql = string.Format("SELECT aanwezigheid.aanwezigheidid, aanwezigheid.datum,aanwezigheid.ActiviteitType,aanwezigheidspelers.Status,aanwezigheidspelers.Reden,aanwezigheidspelers.NietVoldoendeGespeeld,aanwezigheidspelers.aanwezigheidspelersid,lid.lidid,lid.Naam,lid.Voornaam FROM aanwezigheid Inner Join aanwezigheidspelers ON aanwezigheid.AanwezigheidId = aanwezigheidspelers.AanwezigheidId Inner Join lid ON aanwezigheidspelers.LidId = lid.LidId WHERE aanwezigheid.PloegId = {0} AND aanwezigheid.datum = '{1}-{2}-{3}' order by lid.naam", ploegId, datum.Year, datum.Month, datum.Day);
                command.CommandText = sql;
                reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        AanwezigheidSpeler _aanwezigheidspeler = new AanwezigheidSpeler()
                        {
                            LidId = Convert.ToInt32(reader["lidid"]),
                            Naam = Convert.ToString(reader["naam"]),
                            Voornaam = Convert.ToString(reader["voornaam"]),
                            Status = Convert.ToString(reader["status"]),
                            AanwezigheidSpelersId = Convert.ToInt32(reader["aanwezigheidspelersid"]),
                            AanwezigheidId = Convert.ToInt32(reader["aanwezigheidid"]),
                            Reden = Convert.ToString(reader["Reden"]),
                            NietVoldoendeGespeeld = Convert.ToBoolean(reader["NietVoldoendeGespeeld"]),
                            RowStatus = "M"
                        };
                        aanwezigheden.Add(_aanwezigheidspeler);
                    }
                    
                }
                else
                {
                    connection.Close();
                    connection.ConnectionString = connectionString;
                    connection.Open();
                    sql = string.Format("SELECT ploegxlid.PloegId,lid.lidid,lid.Naam,lid.Voornaam FROM ploegxlid Inner Join lid ON ploegxlid.LidId = lid.LidId WHERE ploegxlid.PloegId =  '{0}' order by lid.naam", ploegId);
                    command.CommandText = sql;
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        AanwezigheidSpeler _aanwezigheidspeler = new AanwezigheidSpeler()
                        {
                            LidId = Convert.ToInt32(reader["lidid"]),
                            Naam = Convert.ToString(reader["naam"]),
                            Voornaam = Convert.ToString(reader["voornaam"]),
                            RowStatus = "N"
                        };
                        aanwezigheden.Add(_aanwezigheidspeler);
                    }
                    connection.Close();
                }
                //checken of deze datum een wedstrijddatum is
            }
            return aanwezigheden;
        }
        
        public Int16 setAanwezigheid(int ploegid, string activiteittype, DateTime datum, string opmerking, string activity)
        {
            Int16 iNextAanwezigheid = 0;
            string sql = string.Empty;

            if (activity.ToUpper() != "CREATE")
                return iNextAanwezigheid;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                
                iNextAanwezigheid = getNextSequence("aanwezigheid");
                sql = string.Format("INSERT INTO aanwezigheid (aanwezigheidid,ploegid,datum,activiteittype,opmerking) VALUES({0},{1},'{2}-{3}-{4}','{5}','{6}')",
                                     iNextAanwezigheid, ploegid, datum.Year, datum.Month, datum.Day, activiteittype, opmerking);
                
                command.CommandText = sql;
                MySqlDataReader reader = command.ExecuteReader();
                connection.Close();
            }
            setNextSequence(++iNextAanwezigheid, "aanwezigheid");
            return --iNextAanwezigheid;
           
        }

        public void deleteAanwezigheid(int aanwezigheidID)
        {
            string sql = string.Empty;
            MySqlDataReader reader;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();
                    MySqlCommand command = connection.CreateCommand();
                    sql = string.Format("DELETE FROM aanwezigheidspelers WHERE aanwezigheidID = {0}", aanwezigheidID);
                    command.CommandText = sql;
                    reader = command.ExecuteReader();
                    reader.Close();

                    sql = string.Format("DELETE FROM aanwezigheid WHERE aanwezigheidID = {0}", aanwezigheidID);
                    command.CommandText = sql;
                    reader = command.ExecuteReader();

                    reader.Close();

                    connection.Close();
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }
        }

        public Boolean setAanwezigheidSpelers(int aanwezigheidid, List<AanwezigheidSpeler> _aanwezigheidspelers)
        {
            Int16 iNextAanwezigheidspeler = getNextSequence("aanwezigheidspelers");
            string sql = string.Empty;
            string sAction = string.Empty;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                MySqlCommand command = connection.CreateCommand();

                foreach (AanwezigheidSpeler _aanwezigheidspeler in _aanwezigheidspelers)
                {
                    if (_aanwezigheidspeler.RowStatus == "N")
                        sql = string.Format("INSERT INTO aanwezigheidspelers (aanwezigheidspelersid,aanwezigheidid,lidid,status,nietvoldoendegespeeld,reden) VALUES({0},{1},'{2}','{3}',{4},'{5}')",
                                                   iNextAanwezigheidspeler, aanwezigheidid, _aanwezigheidspeler.LidId, _aanwezigheidspeler.Status, _aanwezigheidspeler.NietVoldoendeGespeeld, _aanwezigheidspeler.Reden );
                    else
                        sql = string.Format("UPDATE aanwezigheidspelers SET status = '{0}', reden = '{1}', nietgenoeggespeeld = {2} where AanwezigheidSpelersId = {3}", _aanwezigheidspeler.Status, _aanwezigheidspeler.Reden, _aanwezigheidspeler.NietVoldoendeGespeeld, _aanwezigheidspeler.AanwezigheidSpelersId);
                                                   
                    command.CommandText = sql;
                    MySqlDataReader reader = command.ExecuteReader();
                    reader.Close();
                    if (_aanwezigheidspeler.RowStatus == "N")
                    {
                        sAction = "Create";
                        iNextAanwezigheidspeler++;
                    }
                }
                
                connection.Close();
            }
            if (sAction == "Create")
                setNextSequence(iNextAanwezigheidspeler, "aanwezigheidspelers");
            return true;
        }

        public void setUitslag(Int32 wedstrijdID, string wedstrijdUitslag)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                string sql;
                sql = string.Format("UPDATE wedstrijd SET Uitslag='{0}' WHERE WedstrijdId={1}", wedstrijdUitslag, wedstrijdID);
                command.CommandText = sql;
                command.ExecuteReader();
            }

        }

        public List<OverzichtAanwezigheid> getAanwezighedenOverview(int ploegID)
        {
             //SELECT lid.Naam,lid.Voornaam,
             //COUNT( NULLIF(Status ='Aanwezig', 0) ) aanwezig ,
             //COUNT( NULLIF(Status ='Verwittigd', 0) ) verwittigd ,
             //COUNT( NULLIF(Status ='NIET verwittigd', 0) )Nverwittigd ,
             //COUNT( NULLIF(Status ='Keeperstraining', 0) ) Keeperstraining,
             //COUNT( NULLIF(Status ='Te laat', 0) ) teLaat
             //FROM ploegxlid Inner Join lid ON ploegxlid.LidId = lid.LidId Inner Join aanwezigheidspelers ON lid.LidId = aanwezigheidspelers.LidId
             //WHERE ploegxlid.PloegId = '8' GROUP BY lid.Naam,lid.Voornaam,aanwezigheidspelers.LidId

            Int32 aantalWedstrijden;
            Int32 aantalTrainingen;
            getAantalActiviteiten(ploegID, out aantalWedstrijden, out aantalTrainingen); 

            List<OverzichtAanwezigheid> _overzichtAanwezigheden = new List<OverzichtAanwezigheid>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                string sql = string.Format(
                          @"SELECT lid.Naam,lid.Voornaam,
                            COUNT( NULLIF(Status ='Aanwezig', 0) ) aanwezig ,
                            COUNT( NULLIF(Status ='Verwittigd', 0) ) verwittigd ,
                            COUNT( NULLIF(Status ='NIET verwittigd', 0) )nietverwittigd ,
                            COUNT( NULLIF(Status ='Keeperstraining', 0) ) Keeperstraining,
                            COUNT( NULLIF(Status ='Te laat', 0) ) telaat,
                            COUNT( NULLIF(Status ='Blessure', 0) ) blessure,
                            COUNT( NULLIF(Status ='Blessure AANWEZIG', 0) )  blessureaanwezig,
                            COUNT( NULLIF(Status ='Ziek', 0) ) ziek,
                            COUNT( NULLIF(Status ='Beurtrol', 0) ) beurtrol,
                            COUNT( NULLIF(Status ='Niet geselecteerd', 0) ) nietgeselecteerd,
                            COUNT( NULLIF(Status ='Geschorst', 0) ) geschorst,
                            COUNT( NULLIF(Status ='Disciplinair', 0) ) disciplinair,
                            COUNT( NULLIF(Status ='Vakantie', 0) ) vakantie,
                            COUNT( NULLIF(Status ='Doorgeschoven', 0) ) doorgeschoven,
                            COUNT( NULLIF(Status ='Selectie', 0) ) selectie,
                            COUNT( NULLIF(Status ='Testen andere club', 0) ) testen,
                            COUNT( NULLIF(NietVoldoendeGespeeld=true, 0) ) '-50%'
                            FROM ploegxlid Inner Join lid ON ploegxlid.LidId = lid.LidId Inner Join aanwezigheidspelers ON lid.LidId = aanwezigheidspelers.LidId
                            WHERE ploegxlid.PloegId = {0} GROUP BY lid.Naam,lid.Voornaam,aanwezigheidspelers.LidId", ploegID);
                command.CommandText = sql;
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    OverzichtAanwezigheid _overzichtAanwezigheid = new OverzichtAanwezigheid()
                    {
                        naam = Convert.ToString(reader["Naam"]),
                        voornaam = Convert.ToString(reader["Voornaam"]),
                        aanwezig = Convert.ToInt16(reader["aanwezig"]),
                        verwittigd = Convert.ToInt16(reader["verwittigd"]),
                        nietverwittigd = Convert.ToInt16(reader["nietverwittigd"]),
                        keeperstraining = Convert.ToInt16(reader["keeperstraining"]),
                        telaat = Convert.ToInt16(reader["telaat"]),
                        blessure = Convert.ToInt16(reader["blessure"]),
                        blessureaanwezig = Convert.ToInt16(reader["blessureaanwezig"]),
                        ziek = Convert.ToInt16(reader["ziek"]),
                        beurtrol = Convert.ToInt16(reader["beurtrol"]),
                        nietgeselecteerd = Convert.ToInt16(reader["nietgeselecteerd"]),
                        geschorst = Convert.ToInt16(reader["geschorst"]),
                        disciplinair = Convert.ToInt16(reader["disciplinair"]),
                        vakantie = Convert.ToInt16(reader["vakantie"]),
                        doorgeschoven = Convert.ToInt16(reader["doorgeschoven"]),
                        selectie = Convert.ToInt16(reader["selectie"]),
                        testen = Convert.ToInt16(reader["testen"]),
                        nietvoldoendegespeeld = Convert.ToInt16(reader["-50%"]),
                        speelgelegenheid = ( Convert.ToInt16(reader["aanwezig"]) + Convert.ToInt16(reader["verwittigd"]) + Convert.ToInt16(reader["Keeperstraining"]) + Convert.ToInt16(reader["telaat"])
                                             + Convert.ToInt16(reader["doorgeschoven"]) + Convert.ToInt16(reader["selectie"]) + Convert.ToInt16(reader["testen"]) )
                                         * 100 / (aantalWedstrijden + aantalTrainingen)
                    };
                   

                    _overzichtAanwezigheden.Add(_overzichtAanwezigheid);
                }
                connection.Close();
            }
            return _overzichtAanwezigheden;

        }

        public void getAantalActiviteiten(int ploegid, out Int32 aantalWedstrijden, out Int32 aantalTrainingen)
        {
            //SELECT COUNT( NULLIF(`aanwezigheid`.`ActiviteitType` ='W', 0) ) wedstrijden ,
            //COUNT( NULLIF(`aanwezigheid`.`ActiviteitType` ='T', 0) ) wedstrijden
            //FROM `aanwezigheid`
            aantalWedstrijden = 0;
            aantalTrainingen = 0;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                string sql = string.Format(@"SELECT COUNT( NULLIF(ActiviteitType ='W', 0) ) wedstrijden ,
                                                    COUNT( NULLIF(ActiviteitType ='T', 0) ) trainingen
                                             FROM aanwezigheid
                                             WHERE ploegid = {0} ", ploegid);
                command.CommandText = sql;
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    aantalWedstrijden = Convert.ToInt16(reader["wedstrijden"]);
                    aantalTrainingen = Convert.ToInt16(reader["trainingen"]);
                }
                connection.Close();
            }
        }

        public List<WeekOverzicht> getWeekOverzicht(int ploegID, DateTime datum)
        {
          //SELECT aanwezigheidspelers.Status,aanwezigheid.Datum,aanwezigheid.ActiviteitType,lid.Naam,lid.Voornaam,aanwezigheidspelers.LidId FROM aanwezigheid
          //Inner Join aanwezigheidspelers ON aanwezigheid.AanwezigheidId = aanwezigheidspelers.AanwezigheidId 
          //Inner Join lid ON aanwezigheidspelers.LidId = lid.LidId WHERE aanwezigheidspelers.LidId = '2' ORDER BY aanwezigheid.Datum ASC

            List<WeekOverzicht> _weekoverzicht = new List<WeekOverzicht>();
            List<Lid> _spelers = getSpelers(ploegID);
            WeekOverzicht _overzicht = null;

            DateTime startDate;
            DateTime endDate;
            getStartEndForWeekByDate(datum, out startDate, out endDate);

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                MySqlCommand command = connection.CreateCommand();

                foreach (Lid _lid in _spelers)
                {
                    string sql = string.Format(@"SELECT aanwezigheidspelers.Status,aanwezigheid.Datum,aanwezigheid.ActiviteitType,lid.Naam,lid.Voornaam,aanwezigheidspelers.LidId FROM aanwezigheid
                                                 Inner Join aanwezigheidspelers ON aanwezigheid.AanwezigheidId = aanwezigheidspelers.AanwezigheidId 
                                                 Inner Join lid ON aanwezigheidspelers.LidId = lid.LidId 
                                                 WHERE aanwezigheidspelers.LidId = {0} and aanwezigheid.Datum >= '{1}-{2}-{3}' and aanwezigheid.Datum <= '{4}-{5}-{6}' 
                                                 ORDER BY aanwezigheid.Datum ASC", _lid.LidId,startDate.Year,startDate.Month,startDate.Day,endDate.Year,endDate.Month,endDate.Day);
                    command.CommandText = sql;
                    MySqlDataReader reader = command.ExecuteReader();
                    _overzicht = new WeekOverzicht();
                    while (reader.Read())
                    {
                        _overzicht.naam = Convert.ToString(reader["naam"]);
                        _overzicht.voornaam = Convert.ToString(reader["voornaam"]);
                        
                        switch ((Int32) ((DateTime)(reader["datum"])).DayOfWeek )
                        {
                            case 1:
                                _overzicht.maandag = Convert.ToString(reader["status"]);
                                break;
                            case 2:
                                _overzicht.dinsdag = Convert.ToString(reader["status"]);
                                break;
                            case 3:
                                _overzicht.woensdag = Convert.ToString(reader["status"]);
                                break;
                            case 4:
                                _overzicht.donderdag = Convert.ToString(reader["status"]);
                                break;
                            case 5:
                                _overzicht.vrijdag = Convert.ToString(reader["status"]);
                                break;
                            case 6:
                                _overzicht.zaterdag = Convert.ToString(reader["status"]);
                                break;
                            case 7:
                                _overzicht.zondag = Convert.ToString(reader["status"]);
                                break;
                        }
                    }
                    _weekoverzicht.Add(_overzicht);
                    reader.Close();
                }
                connection.Close();
            }
            return _weekoverzicht;
        }

        public void getStartEndForWeekByDate(DateTime datum, out DateTime startDate, out DateTime endDate)
        {
            startDate = DateTime.Today;
            endDate = DateTime.Today;

            switch ((Int32)datum.DayOfWeek)
            {
                case 1:
                    startDate = datum;
                    endDate = datum.AddDays(6);
                    break;
                case 2:
                    startDate = datum.AddDays(-1);
                    endDate = datum.AddDays(5);
                    break;
                case 3:
                    startDate = datum.AddDays(-2);
                    endDate = datum.AddDays(4);
                    break;
                case 4:
                    startDate = datum.AddDays(-3);
                    endDate = datum.AddDays(3);
                    break;
                case 5:
                    startDate = datum.AddDays(-4);
                    endDate = datum.AddDays(1);
                    break;
                case 6:
                    startDate = datum.AddDays(-5);
                    endDate = datum.AddDays(1);
                    break;
                case 7:
                    startDate = datum.AddDays(-6);
                    endDate = datum;
                    break;
            }
        }

        //s_extra
        public List<LidType> getLidTypes()
        {
            return ExecuteCommand<LidType>("SELECT * FROM lidtype order by lidtypeid",
                                         (reader) => new LidType()
                                         {
                                             LidTypeId = Convert.ToInt32(reader["lidtypeid"]),
                                             Type = Convert.ToString(reader["type"])
                                         });
        }

        public string getSpelersSQL(string naam, string voornaam, int ploegId, int lidTypeId, string geboortejaar)
        {

            //SELECT Lid.LidId, Lid.Naam, Lid.Voornaam, Lid.geboortedatum, QueryTypeXRef.LidTypeId, QueryTypeXRef.Type, QueryPloegXRef.PloegId, QueryPloegXRef.Ploegnaam 
            //FROM (Lid LEFT JOIN QueryPloegXRef ON Lid.LidId = QueryPloegXRef.LidId) LEFT JOIN QueryTypeXRef ON Lid.LidId = QueryTypeXRef.LidId  
            //WHERE QueryPloegXRef.ploegid =8

            if (naam == "") naam = null;
            if (voornaam == "") voornaam = null;
            if (geboortejaar == "") geboortejaar = null;

            string select = "SELECT Lid.LidId, Lid.Naam, Lid.Voornaam, Lid.geboortedatum, QueryTypeXRef.LidTypeId, QueryTypeXRef.Type, QueryPloegXRef.PloegId, QueryPloegXRef.Ploegnaam ";
            string from = "FROM (Lid LEFT JOIN QueryPloegXRef ON Lid.LidId = QueryPloegXRef.LidId) LEFT JOIN QueryTypeXRef ON Lid.LidId = QueryTypeXRef.LidId  ";
            string where = string.Empty;
            string order = " ORDER BY Lid.Naam, Lid.Voornaam";
            string sql = string.Empty;

            if (!string.IsNullOrEmpty(naam))
                where += where == string.Empty ? string.Format(" WHERE lid.naam like '{0}%' ", naam) : "";
            if (!string.IsNullOrEmpty(voornaam))
                where += where == string.Empty ? string.Format(" WHERE lid.voornaam like '{0}%' ", voornaam) : string.Format(" AND lid.voornaam like '{0}%'", voornaam);
            if (ploegId != 0)
                where += where == string.Empty ? string.Format(" WHERE QueryPloegXRef.ploegid ={0} ", ploegId) : string.Format(" AND QueryPloegXRef.ploegid ={0} ", ploegId);
            if (lidTypeId != 0)
                where += where == string.Empty ? string.Format(" WHERE QueryTypeXRef.LidTypeid ={0} ", lidTypeId) : string.Format(" AND QueryTypeXRef.LidTypeid ={0} ", lidTypeId);
            if (!string.IsNullOrEmpty(geboortejaar))
                where += where == string.Empty ? string.Format(" WHERE YEAR(Lid.GeboorteDatum) ={0} ", Convert.ToInt16(geboortejaar)) : string.Format(" AND YEAR(Lid.GeboorteDatum) ={0} ", Convert.ToInt16(geboortejaar));

            return sql = string.Format("{0} {1} {2} {3}", select, from, where, order);
        }

        public List<Member> getMembers(string naam, string voornaam, int ploegId, int lidTypeId, string geboortejaar)
        {
            //SELECT Lid.LidId, Lid.Naam, Lid.Voornaam, Lid.geboortedatum, QueryTypeXRef.LidTypeId, QueryTypeXRef.Type, QueryPloegXRef.PloegId, QueryPloegXRef.Ploegnaam 
            //FROM (Lid LEFT JOIN QueryPloegXRef ON Lid.LidId = QueryPloegXRef.LidId) LEFT JOIN QueryTypeXRef ON Lid.LidId = QueryTypeXRef.LidId  
            //WHERE QueryPloegXRef.ploegid =8

            string select = "SELECT Lid.LidId, Lid.Naam, Lid.Voornaam, Lid.geboortedatum, QueryTypeXRef.LidTypeId, QueryTypeXRef.Type, QueryPloegXRef.PloegId, QueryPloegXRef.Ploegnaam ";
            string from = "FROM (Lid LEFT JOIN QueryPloegXRef ON Lid.LidId = QueryPloegXRef.LidId) LEFT JOIN QueryTypeXRef ON Lid.LidId = QueryTypeXRef.LidId  ";
            string where = string.Empty;
            string order = " ORDER BY Lid.Naam, Lid.Voornaam";
            string sql = string.Empty;

            if (!string.IsNullOrEmpty(naam))
                where += where == string.Empty ? string.Format(" WHERE lid.naam like '{0}%' ", naam) : "";
            if (!string.IsNullOrEmpty(voornaam))
                where += where == string.Empty ? string.Format(" WHERE lid.voornaam like '{0}%' ", voornaam) : string.Format(" AND lid.voornaam like '{0}%'", voornaam);
            if (ploegId != 0)
                where += where == string.Empty ? string.Format(" WHERE QueryPloegXRef.ploegid ={0} ", ploegId) : string.Format(" AND QueryPloegXRef.ploegid ={0} ", ploegId);
            if (lidTypeId != 0)
                where += where == string.Empty ? string.Format(" WHERE QueryTypeXRef.LidTypeid ={0} ", lidTypeId) : string.Format(" AND QueryTypeXRef.LidTypeid ={0} ", lidTypeId);
            if (!string.IsNullOrEmpty(geboortejaar))
                where += where == string.Empty ? string.Format(" WHERE YEAR(Lid.GeboorteDatum) ={0} ",  Convert.ToInt16(geboortejaar)) : string.Format(" AND YEAR(Lid.GeboorteDatum) ={0} ", Convert.ToInt16(geboortejaar));

            sql = string.Format("{0} {1} {2} {3}", select, from, where, order);
            
            return ExecuteCommand<Member>(sql,
                                        (reader) => new Member()
                                        {
                                            LidId = Convert.ToInt32(reader["lidid"]),
                                            Geboortejaar =  reader.IsDBNull(3) ? "?" :  Convert.ToDateTime(reader["geboortedatum"]).Year.ToString(),
                                            Naam = Convert.ToString(reader["naam"]),
                                            Type = Convert.ToString(reader["type"]),
                                            Fullname = string.Format("{0} {1}", Convert.ToString(reader["naam"]),Convert.ToString(reader["voornaam"])),
                                            Voornaam = Convert.ToString(reader["voornaam"])
                                        });

        }

        public List<ContactInfo> getContactInfoLidByLidID(int lidID)
        {
            //SELECT lid.Naam,lid.Voornaam,lid.Telefoon,lid.Gsm,lid.Email,vader.Naam,vader.Voornaam,vader.Telefoon,vader.Gsm,vader.Email, moeder.Naam,moeder.Voornaam,moeder.Telefoon,moeder.Gsm,moeder.Email
            //FROM lid Left Outer Join contact AS vader ON lid.LidId = vader.LidId AND vader.Relatie = 'Vader' Left Outer Join contact AS moeder ON lid.LidId = moeder.LidId AND moeder.Relatie = 'moeder'

            string select = "SELECT lid.Naam,lid.Voornaam,lid.Telefoon,lid.Gsm,lid.Email, vader.Naam as vnaam,vader.Voornaam as vvoornaam,vader.Telefoon as vtelefoon,vader.Gsm as vgsm,vader.Email as vemail, moeder.Naam as mnaam,moeder.Voornaam as mvoornaam ,moeder.Telefoon as mtelefoon,moeder.Gsm as mgsm,moeder.Email as memail ";
            string from = "FROM lid Left Outer Join contact AS vader ON lid.LidId = vader.LidId AND vader.Relatie = 'Vader' Left Outer Join contact AS moeder ON lid.LidId = moeder.LidId AND moeder.Relatie = 'moeder' ";
            string where = string.Format("WHERE lid.lidid = {0}", lidID);
            string sql = string.Format("{0} {1} {2}", select, from, where);

            return ExecuteCommand<ContactInfo>(sql,
                                         (reader) => new ContactInfo()
                                         {
                                             naamSpeler = string.Format("{0} {1}", Convert.ToString(reader["naam"]), Convert.ToString(reader["voornaam"])),
                                             voornaamSpeler =  Convert.ToString(reader["voornaam"]),
                                             familienaamSpeler = Convert.ToString(reader["naam"]),
                                             naamVader = string.Format("{0} {1}", Convert.ToString(reader["vnaam"]), Convert.ToString(reader["vvoornaam"])),
                                             naamMoeder = string.Format("{0} {1}", Convert.ToString(reader["mnaam"]), Convert.ToString(reader["mvoornaam"])),
                                             telSpeler = Convert.ToString(reader["telefoon"]),
                                             telVader = Convert.ToString(reader["vtelefoon"]),
                                             telMoeder = Convert.ToString(reader["mtelefoon"]),
                                             gsmSpeler = Convert.ToString(reader["gsm"]),
                                             gsmVader = Convert.ToString(reader["vgsm"]),
                                             gsmMoeder = Convert.ToString(reader["mgsm"]),
                                             emailSpeler = Convert.ToString(reader["email"]),
                                             emailVader = Convert.ToString(reader["vemail"]),
                                             emailMoeder = Convert.ToString(reader["memail"])
                                         });
        }
        
    
        //e_extra
    }
}
