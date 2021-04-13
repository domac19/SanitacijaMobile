using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Infobit.DDD.Data
{
    [DataContract]
    public class DeratizacijaSyncData
    {
        public DeratizacijaSyncData() { }

        public DeratizacijaSyncData(string username, string password, string aktivnoSkladiste = "")
        {
            Login.Usename = username;
            Login.Password = password;
            AktivnoSkladiste = aktivnoSkladiste;
        }

        [DataMember]
        public List<KE_DJELATNICI> Djelatnici = new List<KE_DJELATNICI>();
        [DataMember]
        public List<DID_DjelatnikUsername> DjelatniciUsername = new List<DID_DjelatnikUsername>();
        [DataMember]
        public List<DID_RadniNalog> RadniNalozi = new List<DID_RadniNalog>();
        [DataMember]
        public List<DID_RadniNalog_Djelatnik> RadniNaloziDjelatnici = new List<DID_RadniNalog_Djelatnik>();
        [DataMember]
        public List<DID_RadniNalog_Lokacija> RadniNaloziLokacije = new List<DID_RadniNalog_Lokacija>();
        [DataMember]
        public List<DID_Lokacija> Lokacije = new List<DID_Lokacija>();
        [DataMember]
        public List<DID_LokacijaPozicija> LokacijePozicije = new List<DID_LokacijaPozicija>();
        [DataMember]
        public List<DID_Anketa> Ankete = new List<DID_Anketa>();
        [DataMember]
        public List<DID_AnketaMaterijali> AnketeMaterijali = new List<DID_AnketaMaterijali>();
        [DataMember]
        public List<T_KUPDOB> Komitenti = new List<T_KUPDOB>();
        [DataMember]
        public List<DID_RadniNalog_Materijal> RadniNaloziMaterijali = new List<DID_RadniNalog_Materijal>();
        [DataMember]
        public List<DID_StanjeSkladista> StanjaSkladista = new List<DID_StanjeSkladista>();
        [DataMember]
        public List<DID_Potvrda> Potvrde  = new List<DID_Potvrda>();
        [DataMember]
        public List<DID_Potvrda_Materijal> PotvrdaMaterijali = new List<DID_Potvrda_Materijal>();
        [DataMember]
        public List<DID_Potvrda_Nametnik> PotvrdaNametnici = new List<DID_Potvrda_Nametnik>();
        [DataMember]
        public List<DID_Potvrda_Djelatnost> PotvrdeDjelatnosti = new List<DID_Potvrda_Djelatnost>();
        [DataMember]
        public List<DID_Djelatnost> Djelatnosti = new List<DID_Djelatnost>();
        [DataMember]
        public List<DID_Nametnik> Nametnici = new List<DID_Nametnik>();
        [DataMember]
        public List<T_SKL> Skladista = new List<T_SKL>();
        [DataMember]
        public List<T_NAZR> Materijali = new List<T_NAZR>();
        [DataMember]
        public List<T_GRU> MaterijaliGrupe = new List<T_GRU>();
        [DataMember]
        public List<T_MjerneJedinice> MjerneJedinice = new List<T_MjerneJedinice>();
        [DataMember]
        public List<DID_TipLokacije> TipoviLokacije = new List<DID_TipLokacije>();
        [DataMember]
        public List<DID_TipPosla> TipoviPosla = new List<DID_TipPosla>();
        [DataMember]
        public List<DID_TipDeratizacijskeKutije> TipoviDeratizacijskeKutije = new List<DID_TipDeratizacijskeKutije>();
        [DataMember]
        public List<DID_RazinaInfestacije> RazineInfestacije = new List<DID_RazinaInfestacije>();
        [DataMember]
        public List<DID_RazlogNeizvrsenjaDeratizacije> RazloziNeizvrsenjaDeratizacije = new List<DID_RazlogNeizvrsenjaDeratizacije>();
        [DataMember]
        public List<T_ZUPANIJE> Zupanije = new List<T_ZUPANIJE>();
        [DataMember]
        public List<T_OPCINE> Opcine = new List<T_OPCINE>();
        [DataMember]
        public List<T_NASELJA> Naselja = new List<T_NASELJA>();
        [DataMember]
        public List<DID_StatusRadnogNaloga> StatusRadnogNaloga = new List<DID_StatusRadnogNaloga>();
        [DataMember]
        public List<DID_StatusLokacije> StatusLokacije = new List<DID_StatusLokacije>();
        [DataMember]
        public List<DID_StatusLokacije_RadniNalog> StatusLokacijeRadnogNaloga = new List<DID_StatusLokacije_RadniNalog>();
        [DataMember]
        public List<DID_ProvedbeniPlan> ProvedbeniPlanovi = new List<DID_ProvedbeniPlan>();
        [DataMember]
        public List<DID_ProvedbeniPlan_Nametnik> ProvedbeniPlanNametnici = new List<DID_ProvedbeniPlan_Nametnik>();
        [DataMember]
        public List<DID_ProvedbeniPlan_Materijal> ProvedbeniPlanMaterijali = new List<DID_ProvedbeniPlan_Materijal>();
        [DataMember]
        public Credentials Login = new Credentials();
        [DataMember]
        public Exception Error = null;
        [DataMember]
        public string AktivnoSkladiste = null;
    }
}
