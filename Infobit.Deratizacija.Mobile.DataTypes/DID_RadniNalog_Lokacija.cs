using System;
using System.Runtime.Serialization;
using SQLite;

namespace Infobit.DDD.Data
{
    public class DID_RadniNalog_Lokacija : SyncData
    {
        [DataMember, PrimaryKey, Unique]
        public int Id { get; set; }
        [DataMember]
        public int TipAkcije { get; set; }
        [DataMember]
        public int RadniNalog { get; set; }
        [DataMember]
        public int Lokacija { get; set; }
        [DataMember]
        public int RedniBroj { get; set; }
        [DataMember]
        public DateTime VrijemeDolaska { get; set; }
        [DataMember]
        public string OpisPosla { get; set; }
        [DataMember]
        public string Napomena { get; set; }
        [DataMember]
        public int RazlogNeizvrsenja { get; set; }
        [DataMember]
        public int Status { get; set; }
    }
}
