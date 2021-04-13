using SQLite;
using System;
using System.Runtime.Serialization;

namespace Infobit.DDD.Data
{
    public class DID_Nametnik : SyncData
    {
        [PrimaryKey, DataMember, Unique]
        public string Sifra { get; set; }
        [DataMember]
        public string Naziv { get; set; }
        [DataMember]
        public string Tip { get; set; }
    }
}