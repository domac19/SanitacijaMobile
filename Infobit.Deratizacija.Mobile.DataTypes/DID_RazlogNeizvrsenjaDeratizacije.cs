﻿using System;
using System.Runtime.Serialization;
using SQLite;

namespace Infobit.DDD.Data
{
    public class DID_RazlogNeizvrsenjaDeratizacije
    {
        [PrimaryKey, DataMember]
        public int Sifra { get; set; }
        [DataMember]
        public string Naziv { get; set; }
    }
}
