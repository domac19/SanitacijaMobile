using System.Runtime.Serialization;
using SQLite;

namespace Infobit.DDD.Data
{
    public class T_MjerneJedinice
    {
        [PrimaryKey, Unique, DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Naziv { get; set; }
        [DataMember]
        public string Oznaka { get; set; }
        [DataMember]
        public int VecaJedinicaId { get; set; }
        [DataMember]
        public bool GlavnaJedinica { get; set; }
        [DataMember]
        public decimal KoeficijentPretvorbe { get; set; }
        [DataMember]
        public string Tip { get; set; }
    }
}