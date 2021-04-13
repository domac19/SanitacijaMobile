using SQLite;
using System;
using System.Runtime.Serialization;

namespace Infobit.DDD.Data
{
    public class DID_Potvrda_Djelatnost : SyncData
    {
        [PrimaryKey, Unique, DataMember]
        public int Id { get; set; }
        [DataMember]
        public int Potvrda { get; set; }
        [DataMember]
        public int Djelatnost { get; set; }
        [DataMember]
        public string TipPosla { get; set; }
        [DataMember]
        public int BrojObjekata { get; set; }

        //Novo
        [DataMember]
        public bool ObjektiTip1 { get; set; }
        [DataMember]
        public bool ObjektiTip2 { get; set; }
        [DataMember]
        public int BrojObjekataTip2 { get; set; }
    }
}