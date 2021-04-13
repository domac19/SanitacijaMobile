using SQLite;
using System;
using System.Runtime.Serialization;

namespace Infobit.DDD.Data
{
    public class DID_ProvedbeniPlan : SyncData
    {
        [PrimaryKey, Unique, DataMember]
        public int Id { get; set; }
        [DataMember]
        public int Lokacija { get; set; }
        [DataMember]
        public string Godina { get; set; }
        [DataMember]
        public DateTime Datum { get; set; }
        [DataMember]
        public DateTime VrijemeIzvida { get; set; }
        [DataMember]
        public string Izdao { get; set; }
        [DataMember]
        public string Preuzeo { get; set; }
        [DataMember]
        public bool HigijenskoStanje { get; set; }
        [DataMember]
        public string HigijenskoStanjeNedostatci { get; set; }
        [DataMember]
        public bool GradjevinskoStanje { get; set; }
        [DataMember]
        public string GradjevinskoStanjeNedostatci { get; set; }
        [DataMember]
        public bool TehnickaOpremljenost { get; set; }
        [DataMember]
        public DateTime GlodavciDatumPostavljanjaKlopki { get; set; }
        [DataMember]
        public DateTime GlodavciDatumIzvida { get; set; }
        [DataMember]
        public string GlodavciZapazanja { get; set; }
        [DataMember]
        public DateTime InsektiDatumPostavljanjaKlopki { get; set; }
        [DataMember]
        public DateTime InsektiDatumIzvida { get; set; }
        [DataMember]
        public string InsektiZapazanja { get; set; }
        [DataMember]
        public string GlodavciKriticneTocke { get; set; }
        [DataMember]
        public string InsektiKriticneTocke { get; set; }
        [DataMember]
        public string Preporuke { get; set; }
        [DataMember]
        public string Napomene { get; set; }
        [DataMember]
        public int BrojIzvidaGlodavacaGodisnje { get; set; }
        [DataMember]
        public string RasporedIzvidaGlodavaca { get; set; }
        [DataMember]
        public int BrojOpcihDeratizacijaGodisnje { get; set; }
        [DataMember]
        public string RasporedRadaDeratizacije { get; set; }
        [DataMember]
        public int BrojIzvidaInsekataGodisnje { get; set; }
        [DataMember]
        public string RasporedIzvidaInsekata { get; set; }
        [DataMember]
        public int BrojOpcihDezinsekcijaGodisnje { get; set; }
        [DataMember]
        public string RasporedRadaDezinsekcije { get; set; }
        [DataMember]
        public TimeSpan SatZaProvedbuDezinsekcije { get; set; }
        [DataMember]
        public int BrojDanaDoDodatneKontroleInsekata { get; set; }
        [DataMember]
        public int BrojDanaDoDodatneDezinsekcije { get; set; }
        [DataMember]
        public int BrojOpcihDezinfekcijaGodisnje { get; set; }
        [DataMember]
        public string RasporedRadaDezinfekcija { get; set; }
        [DataMember]
        public TimeSpan SatZaProvedbuDezinfekcije { get; set; }
    }
}