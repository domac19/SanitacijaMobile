using System;
using System.Runtime.Serialization;
using SQLite;

namespace Infobit.DDD.Data
{
    public class KE_DJELATNICI : SyncData
    {
        [PrimaryKey, DataMember, Unique]
        public string KE_MBR { get; set; }
        [DataMember]
        public string KE_IME { get; set; }
        [DataMember]
        public string KE_PREZIME { get; set; }
        [DataMember]
        public string KE_MOBITEL { get; set; }
        [DataMember] 
        public string KE_TELEFON { get; set; }
        [DataMember]
        public string KE_TELEFON_POSAO { get; set; }
        [DataMember]
        public string KE_OIB { get; set; }
        [DataMember]
        public string KE_EMAIL { get; set; }
        [DataMember]
        public string KE_ADRESA { get; set; }
        [DataMember]
        public string KE_POSTA { get; set; }
        [DataMember]
        public string KE_MJESTO { get; set; }
    }
}