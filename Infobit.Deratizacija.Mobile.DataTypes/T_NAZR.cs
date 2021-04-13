using System;
using System.Runtime.Serialization;
using SQLite;

namespace Infobit.DDD.Data
{
    public class T_NAZR : SyncData
    {
        [PrimaryKey, DataMember, Unique]
        public string NAZR_SIFRA { get; set; }
        [DataMember]
        public string NAZR_NAZIV { get; set; }
        [DataMember]
        public string NAZR_BARKOD { get; set; }
        [DataMember]
        public decimal NAZR_CIJENA_ART { get; set; }
        [DataMember]
        public string NAZR_GRUPA { get; set; }
        [DataMember]
        public int NAZR_JM_SIFRA { get; set; }
        [DataMember]
        public int TipKemikalije { get; set; }
    }
}
