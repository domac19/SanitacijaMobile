using System.Runtime.Serialization;
using SQLite;

namespace Infobit.DDD.Data
{
    public class T_ZUPANIJE : SyncData
    {
        [PrimaryKey, Unique, DataMember]
        public string Sifra { get; set; }
        [DataMember]
        public string Naziv { get; set; }
    }
}