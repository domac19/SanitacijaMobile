using SQLite;
using System;
using System.Runtime.Serialization;

namespace Infobit.DDD.Data
{
    public class DID_Potvrda : SyncData
    {
        [PrimaryKey, Unique, DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Godina { get; set; }
        [DataMember]
        public string Broj { get; set; }
        [DataMember]
        public int RadniNalog { get; set; }
        [DataMember]
        public int Lokacija { get; set; }
        [DataMember]
        public DateTime DatumVrijeme { get; set; }
        [DataMember]
        public int Infestacija { get; set; }
        [DataMember]
        public string OpisRada { get; set; }
        [DataMember]
        public int RadniNalogLokacijaId { get; set; }

        //Novo
        [DataMember]
        public byte LokacijaTip { get; set; }
        [DataMember]
        public bool LokacijaTipOdradjen { get; set; }
        [DataMember]
        public int LokacijaTipBrojObjekata { get; set; }

        [DataMember]
        public byte LokacijaTip2 { get; set; }
        [DataMember]
        public bool LokacijaTip2Odradjen { get; set; }
        [DataMember]
        public int LokacijaTip2BrojObjekata { get; set; }
    }
}