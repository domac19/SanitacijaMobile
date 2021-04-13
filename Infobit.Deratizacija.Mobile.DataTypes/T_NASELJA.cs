using SQLite;
using System;
using System.Runtime.Serialization;

namespace Infobit.DDD.Data
{
    public class T_NASELJA : SyncData
    {
        [PrimaryKey, DataMember, Unique]
        public string Sifra { get; set; }
        [DataMember]
        public string Naziv { get; set; }
        [DataMember]
        public string Opcina { get; set; }
        [DataMember]
        public string Posta { get; set; }
        [DataMember]
        public int Tip { get; set; }
    }
}