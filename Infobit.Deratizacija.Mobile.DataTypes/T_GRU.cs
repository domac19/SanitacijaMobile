using System;
using System.Runtime.Serialization;
using SQLite;

namespace Infobit.DDD.Data
{
    public class T_GRU
    {
        [PrimaryKey, DataMember, Unique]
        public string GRU_SIFRA { get; set; }
        [DataMember]
        public string GRU_NAZIV { get; set; }
    }
}
