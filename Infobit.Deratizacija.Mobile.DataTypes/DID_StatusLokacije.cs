using System;
using System.Runtime.Serialization;
using SQLite;

namespace Infobit.DDD.Data
{
    public class DID_StatusLokacije : SyncData
    {
        [PrimaryKey, Unique]
        public byte Sifra { get; set; }
        [DataMember]
        public string Naziv { get; set; }
    }
}
