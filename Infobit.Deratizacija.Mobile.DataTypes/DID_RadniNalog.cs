using SQLite;
using System;
using System.Runtime.Serialization;

namespace Infobit.DDD.Data
{
    public class DID_RadniNalog : SyncData
    {
        [DataMember, PrimaryKey, Unique]
        public int Id { get; set; }
        [DataMember]
        public string Godina { get; set; }
        [DataMember]
        public int Broj { get; set; }
        [DataMember]
        public int Status { get; set; }
        [DataMember]
        public string PokretnoSkladiste { get; set; }
        [DataMember]
        public string Voditelj { get; set; }
        [DataMember]
        public string VoditeljKontakt { get; set; }
        [DataMember]
        public string Izdavatelj { get; set; }
        [DataMember]
        public string Primatelj { get; set; }
        [DataMember]
        public DateTime DatumOd { get; set; }
        [DataMember]
        public DateTime DatumDo { get; set; }
        [DataMember]
        public DateTime DatumIzrade { get; set; }
        [DataMember]
        public DateTime DatumIzvrsenja { get; set; }
    }
}