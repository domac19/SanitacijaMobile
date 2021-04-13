using SQLite;
using System;
using System.Runtime.Serialization;

namespace Infobit.DDD.Data
{
    public class DID_Potvrda_Djelatnost_ObjektTip2 : SyncData
    {
        [PrimaryKey, Unique, DataMember]
        public int Id { get; set; }
        [DataMember]
        public int PotvrdaDjelatnostId { get; set; }
        [DataMember]
        public int BrojObjekata { get; set; }
    }
}