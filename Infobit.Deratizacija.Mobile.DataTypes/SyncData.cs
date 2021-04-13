using System;
using System.Runtime.Serialization;

namespace Infobit.DDD.Data
{
    public enum SyncAction
    {
        AddNew = 1,
        Update = 2,
        Delete = 3
    }

    public class SyncData
    {
        //status sinkronizacije:
        // 0 - nije sinkronizirano
        // 1 - djelomicno sinkronizirano
        // 2 - sinkronizirano

        [DataMember]
        public int SinhronizacijaStatus { get; set; }
        [DataMember]
        public SyncAction SinhronizacijaAkcija { get; set; }
        [DataMember]
        public DateTime SinhronizacijaDatumVrijeme { get; set; }
        [DataMember]
        public string SinhronizacijaPrivremeniKljuc { get; set; }

        public bool Promijenjeno { get; set; }
    }
}
