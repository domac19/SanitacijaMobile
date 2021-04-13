using System;
using System.Runtime.Serialization;
using SQLite;

namespace Infobit.DDD.Data
{
    public class T_SKL
    {
        [PrimaryKey, DataMember, Unique]
        public string SKL_SIFRA { get; set; }
        [DataMember]
        public string SKL_NAZIV { get; set; }
    }
}
