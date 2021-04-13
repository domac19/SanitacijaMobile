using SQLite;
using System.Runtime.Serialization;

namespace Infobit.DDD.Data
{
    public class DID_AnketaMaterijali : SyncData
    {
        [PrimaryKey, Unique, DataMember]
        public int Id { get; set; }
        [DataMember]
        public int RadniNalog { get; set; }
        [DataMember]
        public int PozicijaId { get; set; }
        [DataMember]
        public string MaterijalSifra { get; set; }
        [DataMember]
        public int MjernaJedinica { get; set; }
        [DataMember]
        public decimal Kolicina { get; set; }

        public string MaterijalNaziv { get; set; }
        public int RedniBroj { get; set; }
        public decimal Iznos { get; set; }
        public decimal Cijena { get; set; }
        public int LokacijaId { get; set; }
    }
}
