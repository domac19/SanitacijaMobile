using System;
using System.Runtime.Serialization;
using SQLite;

namespace Infobit.DDD.Data
{
    public class DID_Potvrda_Nametnik : SyncData
    {
        [PrimaryKey, Unique, DataMember]
        public int Id { get; set; }
        [DataMember]
        public int Potvrda { get; set; }
        [DataMember]
        public string Nametnik { get; set; }
    }
}
