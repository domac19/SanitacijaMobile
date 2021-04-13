using System;
using System.Runtime.Serialization;
using SQLite;

namespace Infobit.DDD.Data
{
    public class Adapter_SyncData : SyncData
    {
        public Adapter_SyncData() { }

        public Adapter_SyncData(string naziv, int poslano, int primljeno)
        {
            Naziv = naziv;
            Poslano = poslano;
            Primljeno = primljeno;
        }

        public string Naziv { get; set; }
        public int Poslano { get; set; }
        public int Primljeno { get; set; }
    }
}