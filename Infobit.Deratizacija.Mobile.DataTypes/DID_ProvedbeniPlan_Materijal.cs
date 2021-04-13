using SQLite;
using System;
using System.Runtime.Serialization;

namespace Infobit.DDD.Data
{
    public class DID_ProvedbeniPlan_Materijal : SyncData
    {
        [PrimaryKey, Unique, DataMember]
        public int Id { get; set; }
        [DataMember]
        public int ProvedbeniPlan { get; set; }
        [DataMember]
        public string MaterijalSifra { get; set; }
        [DataMember]
        public string MaterijalNaziv { get; set; }
        [DataMember]
        public int TipKemikalije { get; set; }

    }
}