using System.Runtime.Serialization;
using SQLite;

namespace Infobit.DDD.Data
{
    public class DID_TipLokacije : SyncData
    {
        [Unique, DataMember]
        public int Sifra { get; set; }
        [DataMember]
        public string Naziv { get; set; }
        //[DataMember]
        //public string ProsireniNaziv { get; set; }
    }
}