using System;
using System.Runtime.Serialization;
using SQLite;

namespace Infobit.DDD.Data
{
    public class DID_DjelatnikUsername
    {
        [PrimaryKey, Unique, DataMember]
        public string Djelatnik { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Password { get; set; }

        public bool Changed { get; set; }
    }
}