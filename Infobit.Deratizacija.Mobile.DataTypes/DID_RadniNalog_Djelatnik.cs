using System;
using System.Runtime.Serialization;
using SQLite;

namespace Infobit.DDD.Data
{
    public class DID_RadniNalog_Djelatnik : SyncData
    {
        [DataMember, PrimaryKey, Unique]
        public int Id { get; set; }
        [DataMember]
        public int RadniNalog { get; set; }
        [DataMember]
        public string Djelatnik { get; set; }
    }
}
