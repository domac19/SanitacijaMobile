using System.Runtime.Serialization;
using SQLite;

namespace Infobit.DDD.Data
{
    public class DID_TipPosla : SyncData
    {
        [PrimaryKey, Unique, DataMember]
        public string Sifra {get; set;}
        [DataMember]
        public string Naziv { get; set; }
        [DataMember]
        public int Djelatnost { get; set; }
    }
}