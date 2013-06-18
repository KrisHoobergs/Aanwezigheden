using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace Aanwezigheden
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IPresenceService
    {
        [OperationContract]
        Int16 getNextSequence(string tablename);

        [OperationContract]
        void setNextSequence(int next_value, string tablename);
                
        [OperationContract]
        List<Ploeg> getPloegen();

        [OperationContract]
        List<Lid> getSpelers(int ploegid);

        [OperationContract]
        DateTime getLastDataEntry(int ploegid);
        
        [OperationContract]
        List<Wedstrijd> getWedstrijd(int ploegId, DateTime lastDataEntryDate, Boolean previous, Boolean current, Boolean next);

        [OperationContract]
        DateTime getNextTrainingsDatum(int ploegid, DateTime datum);

        [OperationContract]
        List<Aanwezigheid> getNextDataEntry(int ploegid);

        [OperationContract]
        List<Aanwezigheid> getNextDataEntryWDatum(int ploegid, DateTime datum);

        [OperationContract]
        List<AanwezigheidSpeler> getAanwezighedenByPloegAndDatum(int ploegId, DateTime datum);

        [OperationContract]
        Int16 setAanwezigheid(int ploegid, string activiteittype, DateTime datum, string opmerking, string activity);

        [OperationContract]
        Boolean setAanwezigheidSpelers(int aanwezigheidid, List<AanwezigheidSpeler> _aanwezigheidspelers);

        [OperationContract]
        void setUitslag(Int32 wedstrijdID, string wedstrijdUitslag);

        [OperationContract]
        void deleteAanwezigheid(int aanwezigheidID);

        [OperationContract]
        List<OverzichtAanwezigheid> getAanwezighedenOverview(int ploegID);

        [OperationContract]
        void getAantalActiviteiten(int ploegid, out Int32 aantalWedstrijden, out Int32 aantalTrainingen);

        [OperationContract]
        List<WeekOverzicht> getWeekOverzicht(int ploegID, DateTime datum);

        //s_extra
        //[OperationContract]
        //List<Ploeg> getPloegen();

        [OperationContract]
        List<LidType> getLidTypes();

        [OperationContract]
        List<Member> getMembers(string naam, string voornaam, int ploegId, int lidTypeId, string geboortejaar);

        [OperationContract]
        string getSpelersSQL(string naam, string voornaam, int ploegId, int lidTypeId, string geboortejaar);

        [OperationContract]
        List<ContactInfo> getContactInfoLidByLidID(int lidID);
        //e_extra
    }

    //s_extra
    [DataContract]
    public class LidType
    {
        [DataMember]
        public int LidTypeId { get; set; }

        [DataMember]
        public string Type { get; set; }

    }

    [DataContract]
    public class Member
    {
        [DataMember]
        public int LidId { get; set; }

        [DataMember]
        public string Fullname { get; set; }

        [DataMember]
        public string Naam { get; set; }

        [DataMember]
        public string Voornaam { get; set; }

        [DataMember]
        public string Geboortejaar { get; set; }

        [DataMember]
        public string Type { get; set; }
    }

    [DataContract]
    public class ContactInfo
    {
        [DataMember]
        public string naamSpeler { get; set; }
        [DataMember]
        public string voornaamSpeler { get; set; }
        [DataMember]
        public string familienaamSpeler { get; set; }
        [DataMember]
        public string naamVader { get; set; }
        [DataMember]
        public string naamMoeder { get; set; }

        [DataMember]
        public string telSpeler { get; set; }
        [DataMember]
        public string telVader { get; set; }
        [DataMember]
        public string telMoeder { get; set; }

        [DataMember]
        public string gsmSpeler { get; set; }
        [DataMember]
        public string gsmVader { get; set; }
        [DataMember]
        public string gsmMoeder { get; set; }

        [DataMember]
        public string emailSpeler { get; set; }
        [DataMember]
        public string emailVader { get; set; }
        [DataMember]
        public string emailMoeder { get; set; }
    }
    //e_extra
       
    [DataContract]
    public class Ploeg
    {
        [DataMember]
        public int PloegId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int DisplaySequence { get; set; }
    }

    [DataContract]
    public class Lid
    {
        [DataMember]
        public int LidId { get; set; }

        [DataMember]
        public string Naam { get; set; }

        [DataMember]
        public string Voornaam { get; set; }
    }

    [DataContract]
    public class AanwezigheidSpeler
    {
        [DataMember]
        public Int32 AanwezigheidId { get; set; }
        
        [DataMember]
        public Int32 AanwezigheidSpelersId { get; set; }

        [DataMember]
        public int LidId { get; set; }

        [DataMember]
        public string Naam { get; set; }

        [DataMember]
        public string Voornaam { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public string Reden { get; set; }

        [DataMember]
        public Boolean NietVoldoendeGespeeld { get; set; }

        [DataMember]
        public string RowStatus { get; set; }
    }

    [DataContract]
    public class Aanwezigheid
    {
        [DataMember]
        public DateTime Datum { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string Tegenstander { get; set; }

        
    }

    [DataContract]
    public class Wedstrijd
    {
        [DataMember]
        public int WedstrijdId { get; set; }

        [DataMember]
        public string Tegenstander { get; set; }

        [DataMember]
        public DateTime WedstrijdDatum { get; set; }

        [DataMember]
        public Boolean Thuis { get; set; }

        [DataMember]
        public string Uitslag { get; set; }

    }

    [DataContract]
    public class OverzichtAanwezigheid
    {
        [DataMember]
        public string naam { get; set; }
        [DataMember]
        public string voornaam { get; set; }
        [DataMember]
        public Int16 aanwezig { get; set; }
        [DataMember]
        public Int16 verwittigd { get; set; }
        [DataMember]
        public Int16 nietverwittigd { get; set; }
        [DataMember]
        public Int16 keeperstraining { get; set; }
        [DataMember]
        public Int16 telaat { get; set; }
        [DataMember]
        public Int16 blessure { get; set; }
        [DataMember]
        public Int16 blessureaanwezig { get; set; }
        [DataMember]
        public Int16 ziek { get; set; }
        [DataMember]
        public Int16 beurtrol { get; set; }
        [DataMember]
        public Int16 nietgeselecteerd { get; set; }
        [DataMember]
        public Int16 geschorst { get; set; }
        [DataMember]
        public Int16 disciplinair { get; set; }
        [DataMember]
        public Int16 vakantie { get; set; }
        [DataMember]
        public Int16 doorgeschoven { get; set; }
        [DataMember]
        public Int16 selectie { get; set; }
        [DataMember]
        public Int16 testen { get; set; }
        [DataMember]
        public Int32 speelgelegenheid { get; set; }
        [DataMember]
        public Int32 nietvoldoendegespeeld { get; set; }

    }

    [DataContract]
    public class WeekOverzicht
    {
        [DataMember]
        public string naam { get; set; }
        [DataMember]
        public string voornaam { get; set; }
        [DataMember]
        public string maandag { get; set; }
        [DataMember]
        public string dinsdag { get; set; }
        [DataMember]
        public string woensdag { get; set; }
        [DataMember]
        public string donderdag { get; set; }
        [DataMember]
        public string vrijdag { get; set; }
        [DataMember]
        public string zaterdag { get; set; }
        [DataMember]
        public string zondag { get; set; }
    }

    [DataContract]
    public class PloegActiviteit
    {
        [DataMember]
        public int Dag { get; set; }

    }

    [DataContract]
    public class AanwezigheidType
    {
        [DataMember]
        public string Type { get; set; }

    }
}
