using System.Runtime.Serialization;
using SQLite;

namespace Infobit.DDD.Data
{
    public class DID_TipDeratizacijskeKutije
    {
        [PrimaryKey, DataMember]
        public int Sifra { get; set; }
        [DataMember]
        public string Naziv { get; set; }
    }
}