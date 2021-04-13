using SQLite;
using System;
using System.Runtime.Serialization;

namespace Infobit.DDD.Data
{
    public class DID_Potvrda_Materijal : SyncData
    {
        [PrimaryKey, Unique, DataMember]
        public int Id { get; set; }
        [DataMember]
        public int Potvrda { get; set; }
        [DataMember]
        public string Materijal { get; set; }
        [DataMember]
        public string MaterijalNaziv { get; set; }
        [DataMember]
        public string DjelatnaTvar { get; set; }
        [DataMember]
        public string DjelatnaTvarOpis { get; set; }
        [DataMember]
        public decimal UdioDjelatneTvari { get; set; }
        [DataMember]
        public decimal Utroseno { get; set; }
        [DataMember]
        public string UdjeliDjelatnihTvariOpis { get; set; }
        [DataMember]
        public decimal Razrjedjenje { get; set; }
        [DataMember]
        public decimal Predvidjeno { get; set; }
    }
}