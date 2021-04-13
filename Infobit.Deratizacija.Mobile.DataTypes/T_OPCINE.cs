using SQLite;
using System;
using System.Runtime.Serialization;

namespace Infobit.DDD.Data
{
    public class T_OPCINE : SyncData
    {
        [PrimaryKey, DataMember]
        public string Sifra { get; set; }
        [DataMember]
        public string Naziv { get; set; }
        [DataMember]
        public string Zupanija { get; set; }
    }
}