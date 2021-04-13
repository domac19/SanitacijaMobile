using System.Runtime.Serialization;
using SQLite;

namespace Infobit.DDD.Data
{
    public class T_KUPDOB : SyncData
    {
        [PrimaryKey, DataMember, Unique]
        public string SIFRA { get; set; }
        [DataMember]
        public int STATUS_PARTNERA { get; set; }
        [DataMember]
        public string NAZIV { get; set; }
        [DataMember]
        public string IME { get; set; }
        [DataMember]
        public string PREZIME { get; set; } 
        [DataMember]
        public string POSTA { get; set; }
        [DataMember]
        public string GRAD { get; set; }
        [DataMember]
        public string ULICA { get; set; }
        [DataMember]
        public string UL_BROJ { get; set; }
        [DataMember]
        public string DRZAVA { get; set; }
        [DataMember]
        public string OIB { get; set; }
        [DataMember]
        public string OIB2 { get; set; }
        [DataMember]
        public string ZIRO { get; set; }
        [DataMember]
        public string TELEFON { get; set; }
        [DataMember]
        public string TIP_PARTNERA { get; set; }
    }
}