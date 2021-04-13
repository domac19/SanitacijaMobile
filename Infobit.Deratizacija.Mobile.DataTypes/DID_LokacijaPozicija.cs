using System;
using System.Runtime.Serialization;
using SQLite;

namespace Infobit.DDD.Data
{
    public class DID_LokacijaPozicija : SyncData
    {
        [PrimaryKey, Unique, DataMember]
        public int POZ_Id { get; set; }
        [DataMember]
        public int SAN_Id { get; set; }
        [DataMember]
        public string POZ_Broj { get; set; }
        [DataMember]
        public string POZ_BrojOznaka { get; set; }
        [DataMember]
        public string POZ_Barcode { get; set; }
        [DataMember]
        public int POZ_Tip { get; set; }
        [DataMember]
        public int POZ_Status { get; set; }
        [DataMember]
        public string POZ_Opis { get; set; }
    }
}