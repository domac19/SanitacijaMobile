using SQLite;
using System;
using System.Runtime.Serialization;

namespace Infobit.DDD.Data
{
    public class DID_ProvedbeniPlan_Nametnik : SyncData
    {
        [PrimaryKey, Unique, DataMember]
        public int Id { get; set; }
        [DataMember]
        public int ProvedbeniPlan { get; set; }
        [DataMember]
        public string NametnikSifra { get; set; }
        [DataMember]
        public string NametnikNaziv { get; set; }
    }
}