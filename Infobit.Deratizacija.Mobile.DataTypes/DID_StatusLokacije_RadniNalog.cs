using System;
using System.Runtime.Serialization;
using SQLite;

namespace Infobit.DDD.Data
{
    public class DID_StatusLokacije_RadniNalog : SyncData
    {
        [PrimaryKey, Unique]
        public int Sifra { get; set; }
        [DataMember]
        public string Naziv { get; set; }
    }
}
