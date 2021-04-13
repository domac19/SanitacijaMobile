using SQLite;
using System;
using System.Runtime.Serialization;

namespace Infobit.DDD.Data
{
    public class DID_Anketa : SyncData
    {
        [PrimaryKey, DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public int ANK_RadniNalog { get; set; }
        [DataMember]
        public string ANK_KorisnickoIme { get; set; }
        [DataMember]
        public int ANK_POZ_Id { get; set; }
        [DataMember]
        public DateTime ANK_DatumVrijeme { get; set; }
        [DataMember]
        public int ANK_TipDeratizacijskeKutijeSifra { get; set; }
        [DataMember]
        public bool ANK_Utvrdjeno_Rot_KutijaUredna { get; set; }
        [DataMember]
        public bool ANK_Utvrdjeno_Rot_KutijaOstecena { get; set; }
        [DataMember]
        public bool ANK_Utvrdjeno_Rot_NemaKutije { get; set; }
        [DataMember]
        public bool ANK_Utvrdjeno_Rot_TragoviGlodanja { get; set; }
        [DataMember]
        public bool ANK_Utvrdjeno_Rot_Izmet { get; set; }
        [DataMember]
        public bool ANK_Utvrdjeno_Rot_PojedenaMeka { get; set; }
        [DataMember]
        public bool ANK_Utvrdjeno_Rot_OstecenaMeka { get; set; }
        [DataMember]
        public bool ANK_Utvrdjeno_Rot_UginuliStakor { get; set; }
        [DataMember]
        public bool ANK_Utvrdjeno_Rot_UginuliMis { get; set; }
        [DataMember]
        public bool ANK_Utvrdjeno_Ljep_KutijaUredna { get; set; }
        [DataMember]
        public bool ANK_Utvrdjeno_Ljep_KutijaOstecena { get; set; }
        [DataMember]
        public bool ANK_Utvrdjeno_Ljep_NemaKutije { get; set; }
        [DataMember]
        public bool ANK_Utvrdjeno_Ljep_UlovljenStakor { get; set; }
        [DataMember]
        public bool ANK_Utvrdjeno_Ljep_UlovljenMis { get; set; }
        [DataMember]
        public bool ANK_Utvrdjeno_Ziv_KutijaUredna { get; set; }
        [DataMember]
        public bool ANK_Utvrdjeno_Ziv_KutijaOstecena { get; set; }
        [DataMember]
        public bool ANK_Utvrdjeno_Ziv_NemaKutije { get; set; }
        [DataMember]
        public bool ANK_Utvrdjeno_Ziv_UlovljenStakor { get; set; }
        [DataMember]
        public bool ANK_Utvrdjeno_Ziv_UlovljenMis { get; set; }
        [DataMember]
        public bool ANK_Ucinjeno_Rot_PostavljeniMamci { get; set; }
        [DataMember]
        public bool ANK_Ucinjeno_Rot_NovaKutija { get; set; }
        [DataMember]
        public bool ANK_Ucinjeno_Rot_NadopunjenaMekom { get; set; }
        [DataMember]
        public string ANK_Ucinjeno_Rot_MasaMeke1 { get; set; }
        [DataMember]
        public string ANK_Ucinjeno_Rot_MasaMeke2 { get; set; }
        [DataMember]
        public string ANK_Ucinjeno_Rot_MasaMeke3 { get; set; }
        [DataMember]
        public bool ANK_Ucinjeno_Ljep_NovaLjepljivaPodloska { get; set; }
        [DataMember]
        public bool ANK_Ucinjeno_Ljep_NovaKutija { get; set; }
        [DataMember]
        public bool ANK_Ucinjeno_Ziv_NovaKutija { get; set; }
        [DataMember]
        public int ANK_RazlogNeizvrsenja { get; set; }
        [DataMember]
        public string ANK_Napomena { get; set; }
        [DataMember]
        public bool ANK_Poc_Prisutnost_Rupe { get; set; }
        [DataMember]
        public bool ANK_Poc_Prisutnost_TragoviNogu { get; set; }
        [DataMember]
        public bool ANK_Poc_Prisutnost_Izmet { get; set; }
        [DataMember]
        public bool ANK_Poc_Prisutnost_Steta { get; set; }
        [DataMember]
        public bool ANK_Poc_Prisutnost_Leglo { get; set; }
        [DataMember]
        public bool ANK_Poc_Prisutnost_GlodavacZivi { get; set; }
        [DataMember]
        public bool ANK_Poc_Prisutnost_GlodavacUginuli { get; set; }
        [DataMember]
        public bool ANK_Poc_Infestacija_Stakori { get; set; }
        [DataMember]
        public bool ANK_Poc_Infestacija_Misevi { get; set; }
        [DataMember]
        public bool ANK_Poc_Infestacija_DrugiGlodavci { get; set; }
        [DataMember]
        public bool ANK_Poc_Uvjeti_Hrana { get; set; }
        [DataMember]
        public bool ANK_Poc_Uvjeti_NeispravnaOdvodnja { get; set; }
        [DataMember]
        public bool ANK_Poc_Uvjeti_NeuredanOkolis { get; set; }

        //columne samo na mobu(one koje se ne sinkroniziraju)
        //Probaj izbrisati ovu ispod iz projekta
        public int ANK_TipDeratizacije { get; set; }
        public DateTime LastEditDate { get; set; }
    }
}