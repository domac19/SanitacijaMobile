using System;
using System.Runtime.Serialization;
using SQLite;

namespace Infobit.DDD.Data
{
    public class DID_Lokacija : SyncData
    {
        [DataMember, PrimaryKey, Unique]
        public int SAN_Id { get; set; }
        [DataMember]
        public string SAN_KD_Sifra { get; set; }
        [DataMember]
        public string SAN_Sifra { get; set; }
        [DataMember]
        public string SAN_Naziv { get; set; }
        [DataMember]
        public string SAN_Mjesto { get; set; }
        [DataMember]
        public string SAN_Naselje { get; set; }
        [DataMember]
        public string SAN_GradOpcina { get; set; }
        [DataMember]
        public string SAN_UlicaBroj { get; set; }
        [DataMember]
        public string SAN_OIBPartner { get; set; }
        [DataMember]
        public string SAN_TipPartnera { get; set; }
        [DataMember]
        public bool SAN_AnketePoPozicijama { get; set; }
        [DataMember]
        public int SAN_Status { get; set; }
        [DataMember]
        public int SAN_Tip { get; set; }
        [DataMember]
        public int SAN_Tip2 { get; set; }
        [DataMember]
        public byte[] Tlocrt { get; set; }
    }
}