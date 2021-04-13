using System.Runtime.Serialization;
using SQLite;

namespace Infobit.DDD.Data
{
    public class DID_StanjeSkladista : SyncData
    {
        [DataMember]
        public string Skladiste { get; set; }
        [DataMember]
        public string SkladisteNaziv { get; set; }
        [DataMember]
        public string Materijal { get; set; }
        [DataMember]
        public string MaterijalNaziv { get; set; }
        [DataMember]
        public int MjernaJedinica { get; set; }
        [DataMember]
        public string MjernaJedinicaOznaka { get; set; }
        [DataMember]
        public string TarifniBroj { get; set; }
        [DataMember]
        public decimal TarifniBrojStopa { get; set; }
        [DataMember]
        public decimal Kolicina { get; set; }
        [DataMember]
        public decimal Cijena { get; set; }
        [DataMember]
        public decimal Iznos { get; set; }
        [DataMember]
        public decimal Dodano { get; set; }


        public decimal StaraKolicina { get; set; }
    }
}