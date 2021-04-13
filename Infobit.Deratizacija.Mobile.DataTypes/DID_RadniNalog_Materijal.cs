using System.Runtime.Serialization;
using SQLite;

namespace Infobit.DDD.Data
{
    public class DID_RadniNalog_Materijal : SyncData
    {
        [PrimaryKey, Unique, DataMember]
        public int Id { get; set; }
        [DataMember]
        public int RadniNalog { get; set; }
        [DataMember]
        public string Materijal { get; set; }
        [DataMember]
        public string MaterijalNaziv { get; set; }
        [DataMember]
        public int MjernaJedninica { get; set; }
        [DataMember]
        public string MjernaJedinicaOznaka { get; set; }
        [DataMember]
        public decimal Izdano { get; set; }
        [DataMember]
        public decimal Vraceno { get; set; }
        [DataMember]
        public decimal Utroseno { get; set; }
        [DataMember]
        public decimal Predvidjeno { get; set; }

        public decimal StanjeKolicine { get; set; }
        public decimal Potroseno { get; set; }
    }
}