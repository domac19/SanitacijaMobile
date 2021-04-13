using Infobit.DDD.Data;
using System.IO;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using SQLite;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Android.Support.V7.Widget;
using Plugin.Connectivity;
using Android.Support.V4.Content;
using Android.Content.PM;

namespace App4New
{
    [Activity(Label = "Activity_Sinkronizacija", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_Sinkronizacija : Activity
    {
        ProgressBar progressBar1;
        TextView message;
        LinearLayout prikazPodataka;
        ImageView alertIcon;
        RecyclerView sinkronizacijaListView;
        RecyclerView.LayoutManager mLayoutManager;
        Adapter_Syncdata mAdapter;
        LinearLayout info;
        Intent intent;
        DeratizacijaSyncData SyncDataSend, SyncDataGet;
        DID_DjelatnikUsername user;
        bool flag = false;
        string syncErrMessage;

        static readonly ISharedPreferences localUsername = Application.Context.GetSharedPreferences("userLogin", FileCreationMode.Private);

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.sinkronizacija);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            progressBar1 = FindViewById<ProgressBar>(Resource.Id.progressBar1);
            message = FindViewById<TextView>(Resource.Id.message);
            prikazPodataka = FindViewById<LinearLayout>(Resource.Id.prikazPodataka);
            sinkronizacijaListView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            alertIcon = FindViewById<ImageView>(Resource.Id.alertIcon);
            info = FindViewById<LinearLayout>(Resource.Id.info);
            SetActionBar(toolbar);
            ActionBar.Title = "Sinkronizacija";
            message.Text = "Sinkronizacija radnih naloga u tijeku...";

            if (CrossConnectivity.Current.IsConnected)
            {
                string aktivnoSkladiste = localUsername.GetString("aktivnoSkladiste", null);
                user = db.Query<DID_DjelatnikUsername>(
                    "SELECT * " +
                    "FROM DID_DjelatnikUsername " +
                    "WHERE Djelatnik = ?", localUsername.GetString("sifraDjelatnika", null)).FirstOrDefault();
                SyncDataSend = new DeratizacijaSyncData(user.Username, user.Password, aktivnoSkladiste);
                await Sinkornizacija();
            }
            else
            {
                flag = true;
                sinkronizacijaListView.Visibility = Android.Views.ViewStates.Gone;
                alertIcon.Visibility = Android.Views.ViewStates.Visible;
                prikazPodataka.Visibility = Android.Views.ViewStates.Gone;
                progressBar1.Visibility = Android.Views.ViewStates.Gone;
                message.Text = "Sinkronizacija se ne može pokrenuti bez pristupa internetu. Uključite internet pa probajte ponovno!";
                message.SetTextColor(ContextCompat.GetColorStateList(this, Resource.Color.redPrimary));
            }   
        }

        public override void OnBackPressed()
        {
            if (flag)
            {
                Intent intent = new Intent(this, typeof(Activity_Pocetna));
                StartActivity(intent);
            }
            else
                Toast.MakeText(this, "Ne možete prekiniti sinkronizaciju jer bi moglo doći do gubljenja podataka", ToastLength.Long).Show();
        }

        public async Task Sinkornizacija()
        {
            await Task.Run(() => {
                try
                {
                    CallWebService(PripremanjePodatakaZaSnkronizaciju());
                    SpremanjePrimljenihPodataka();
                    PrikazSinkronizacije();
                }
                catch(CallServiceExeption e)
                {
                    PrikazErrora(e.ToString());
                }
                flag = true;
            });
        }

        public string PripremanjePodatakaZaSnkronizaciju()
        {
            List<DID_RadniNalog> sviRadniNalozi = db.Query<DID_RadniNalog>(
                "SELECT DID_RadniNalog.Id, DID_RadniNalog.Godina, DID_RadniNalog.Godina, DID_RadniNalog.Broj, DID_RadniNalog.Status, " +
                    "DID_RadniNalog.PokretnoSkladiste, DID_RadniNalog.Voditelj, DID_RadniNalog.VoditeljKontakt, DID_RadniNalog.Izdavatelj, " +
                    "DID_RadniNalog.Primatelj, DID_RadniNalog.DatumOd, DID_RadniNalog.DatumDo, DID_RadniNalog.DatumIzrade, DID_RadniNalog.DatumIzvrsenja, DID_RadniNalog.SinhronizacijaStatus, DID_RadniNalog.SinhronizacijaPrivremeniKljuc " +
                "FROM DID_RadniNalog " +
                "INNER JOIN DID_RadniNalog_Djelatnik ON DID_RadniNalog.Id = DID_RadniNalog_Djelatnik.RadniNalog " +
                "WHERE DID_RadniNalog.SinhronizacijaStatus != 2 " +
                "AND (DID_RadniNalog.Status = 4 OR DID_RadniNalog.Status = 5) " +
                "AND DID_RadniNalog_Djelatnik.Djelatnik = ?", user.Djelatnik);

            foreach (var radniNalog in sviRadniNalozi)
            {
                SyncDataSend.RadniNalozi.Add(radniNalog);

                DID_RadniNalog_Djelatnik radniNalogDjelatnik = db.Query<DID_RadniNalog_Djelatnik>(
                    "SELECT * " +
                    "FROM DID_RadniNalog_Djelatnik " +
                    "WHERE RadniNalog = ? " +
                    "AND Djelatnik = ?", radniNalog.Id, user.Djelatnik).FirstOrDefault();
                SyncDataSend.RadniNaloziDjelatnici.Add(radniNalogDjelatnik);

                List<DID_Lokacija> sveLokacije = db.Query<DID_Lokacija>(
                    "SELECT *, DID_Lokacija.SinhronizacijaPrivremeniKljuc " +
                    "FROM DID_Lokacija " +
                    "INNER JOIN DID_RadniNalog_Lokacija on DID_Lokacija.SAN_Id = DID_RadniNalog_Lokacija.Lokacija " +
                    "WHERE DID_RadniNalog_Lokacija.RadniNalog = ? " +
                    "AND DID_RadniNalog_Lokacija.Status = 3 " +
                    "AND DID_RadniNalog_Lokacija.SinhronizacijaStatus != 2", radniNalog.Id);

                foreach (var lokacija in sveLokacije)
                {
                    if (!SyncDataSend.Lokacije.Exists(x => x.SAN_Id == lokacija.SAN_Id))
                        SyncDataSend.Lokacije.Add(lokacija);

                    T_KUPDOB komitent = db.Query<T_KUPDOB>(
                        "SELECT * " +
                        "FROM T_KUPDOB " +
                        "WHERE SIFRA = ?", lokacija.SAN_KD_Sifra).FirstOrDefault();

                    if (!SyncDataSend.Komitenti.Exists(x => x.SIFRA == komitent.SIFRA))
                        SyncDataSend.Komitenti.Add(komitent);

                    List<DID_LokacijaPozicija> svePozicije = db.Query<DID_LokacijaPozicija>(
                        "SELECT * " +
                        "FROM DID_LokacijaPozicija " +
                        "WHERE SAN_Id = ?", lokacija.SAN_Id);
                    foreach (var pozicija in svePozicije)
                    {
                        if (!SyncDataSend.LokacijePozicije.Exists(x => x.POZ_Id == pozicija.POZ_Id))
                            SyncDataSend.LokacijePozicije.Add(pozicija);
                    }

                    DID_RadniNalog_Lokacija radniNalogLokacija = db.Query<DID_RadniNalog_Lokacija>(
                        "SELECT * " +
                        "FROM DID_RadniNalog_Lokacija " +
                        "WHERE Lokacija = ? " +
                        "AND RadniNalog = ?", lokacija.SAN_Id, radniNalog.Id).FirstOrDefault();
                    SyncDataSend.RadniNaloziLokacije.Add(radniNalogLokacija);

                    List<DID_Anketa> ankete = db.Query<DID_Anketa>(
                        "SELECT *, DID_Anketa.SinhronizacijaPrivremeniKljuc " +
                        "FROM DID_Anketa " +
                        "INNER JOIN DID_LokacijaPozicija ON DID_Anketa.ANK_POZ_Id = DID_LokacijaPozicija.POZ_Id " +
                        "WHERE DID_LokacijaPozicija.SAN_Id = ? " +
                        "AND ANK_RadniNalog = ?", lokacija.SAN_Id, radniNalog.Id);
                    foreach (var anketa in ankete)
                        SyncDataSend.Ankete.Add(anketa);

                    List<DID_AnketaMaterijali> sviAnketaMaterijali = db.Query<DID_AnketaMaterijali>(
                        "SELECT DID_AnketaMaterijali.Id, DID_AnketaMaterijali.RadniNalog, DID_AnketaMaterijali.PozicijaId, DID_AnketaMaterijali.MaterijalSifra, " +
                            "DID_AnketaMaterijali.MjernaJedinica, DID_AnketaMaterijali.Kolicina, DID_AnketaMaterijali.SinhronizacijaPrivremeniKljuc " +
                        "FROM DID_AnketaMaterijali " +
                        "INNER JOIN DID_Anketa ON DID_AnketaMaterijali.PozicijaId = DID_Anketa.ANK_POZ_Id " +
                        "INNER JOIN DID_LokacijaPozicija ON DID_Anketa.ANK_POZ_Id = DID_LokacijaPozicija.POZ_Id " +
                        "WHERE DID_LokacijaPozicija.SAN_Id = ? " +
                        "AND DID_AnketaMaterijali.RadniNalog = ?", lokacija.SAN_Id, radniNalog.Id);
                    foreach (var anketaMaterijal in sviAnketaMaterijali)
                        SyncDataSend.AnketeMaterijali.Add(anketaMaterijal);

                    DID_Potvrda potvrda = db.Query<DID_Potvrda>(
                        "SELECT * " +
                        "FROM DID_Potvrda " +
                        "WHERE RadniNalogLokacijaId = ?", radniNalogLokacija.Id).FirstOrDefault();
                    SyncDataSend.Potvrde.Add(potvrda);

                    List<DID_Potvrda_Materijal> privremeniPotvrdaMaterijali = db.Query<DID_Potvrda_Materijal>(
                        "SELECT * " +
                        "FROM DID_Potvrda_Materijal " +
                        "WHERE Potvrda = ?", potvrda.Id);
                    foreach (DID_Potvrda_Materijal materijal in privremeniPotvrdaMaterijali)
                        SyncDataSend.PotvrdaMaterijali.Add(materijal);


                    List<DID_Potvrda_Djelatnost> privremenPotvrdeDjelatnosti = db.Query<DID_Potvrda_Djelatnost>(
                        "SELECT * " +
                        "FROM DID_Potvrda_Djelatnost " +
                        "WHERE Potvrda = ?", potvrda.Id);
                    foreach (DID_Potvrda_Djelatnost privremenaPotvrda in privremenPotvrdeDjelatnosti)
                        SyncDataSend.PotvrdeDjelatnosti.Add(privremenaPotvrda);

                    List<DID_Potvrda_Nametnik> privremeniNametnici = db.Query<DID_Potvrda_Nametnik>(
                        "SELECT * " +
                        "FROM DID_Potvrda_Nametnik " +
                        "WHERE Potvrda = ?", potvrda.Id);
                    foreach (DID_Potvrda_Nametnik nametnik in privremeniNametnici)
                        SyncDataSend.PotvrdaNametnici.Add(nametnik);
                }
            }


            List<T_NAZR> materijali = db.Query<T_NAZR>(
                "SELECT * " +
                "FROM T_NAZR " +
                "WHERE NAZR_SIFRA = NAZR_NAZIV");

            foreach (var materijal in materijali)
                if (!SyncDataSend.Materijali.Exists(x => x.NAZR_SIFRA == materijal.NAZR_SIFRA))
                    SyncDataSend.Materijali.Add(materijal);

            List<DID_ProvedbeniPlan> provedbeniPlanList = db.Query<DID_ProvedbeniPlan>(
                "SELECT * " +
                "FROM DID_ProvedbeniPlan " +
                "WHERE Id < 0");

            List<DID_ProvedbeniPlan_Nametnik> provedbeniPlan_NametnikList = db.Query<DID_ProvedbeniPlan_Nametnik>(
                "SELECT * " +
                "FROM DID_ProvedbeniPlan_Nametnik " +
                "WHERE ProvedbeniPlan < 0");

            List<DID_ProvedbeniPlan_Materijal> provedbeniPlan_MaterijalList = db.Query<DID_ProvedbeniPlan_Materijal>(
                "SELECT * " +
                "FROM DID_ProvedbeniPlan_Materijal " +
                "WHERE ProvedbeniPlan < 0");

            foreach (var provedbeniPlan in provedbeniPlanList)
                SyncDataSend.ProvedbeniPlanovi.Add(provedbeniPlan);

            foreach (var provedbeniPlanNametnik in provedbeniPlan_NametnikList)
                SyncDataSend.ProvedbeniPlanNametnici.Add(provedbeniPlanNametnik);

            foreach (var provedbeniPlanMaterijal in provedbeniPlan_MaterijalList)
                SyncDataSend.ProvedbeniPlanMaterijali.Add(provedbeniPlanMaterijal);

            return JsonConvert.ToString(JsonConvert.SerializeObject(SyncDataSend));
        }

        public void SpremanjePrimljenihPodataka()
        {
            foreach (var radniNalog in SyncDataGet.RadniNalozi)
            {
                db.Execute(
                    "DELETE FROM DID_RadniNalog " +
                    "WHERE Id = ? " +
                    "OR Id = ?", radniNalog.SinhronizacijaPrivremeniKljuc, radniNalog.Id);

                db.Execute(
                    "INSERT INTO DID_RadniNalog (" +
                        "Id, " +
                        "Broj, " +
                        "Godina, " +
                        "Status, " +
                        "PokretnoSkladiste, " +
                        "Voditelj, " +
                        "VoditeljKontakt, " +
                        "Izdavatelj, " +
                        "Primatelj, " +
                        "DatumOd, " +
                        "DatumDo, " +
                        "DatumIzrade, " +
                        "DatumIzvrsenja, " +
                        "SinhronizacijaStatus)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        radniNalog.Id,
                        radniNalog.Broj,
                        radniNalog.Godina,
                        radniNalog.Status,
                        radniNalog.PokretnoSkladiste,
                        radniNalog.Voditelj,
                        radniNalog.VoditeljKontakt,
                        radniNalog.Izdavatelj,
                        radniNalog.Primatelj,
                        radniNalog.DatumOd,
                        radniNalog.DatumDo,
                        radniNalog.DatumIzrade,
                        radniNalog.DatumIzvrsenja,
                        2);
            }

            foreach (var radniNalogDjelatnik in SyncDataGet.RadniNaloziDjelatnici)
            {
                db.Execute(
                    "DELETE FROM DID_RadniNalog_Djelatnik " +
                    "WHERE Id = ? " +
                    "OR Id = ?", radniNalogDjelatnik.SinhronizacijaPrivremeniKljuc, radniNalogDjelatnik.Id);

                db.Execute(
                    "INSERT INTO DID_RadniNalog_Djelatnik (" +
                        "Id, " +
                        "RadniNalog, " +
                        "Djelatnik, " +
                        "SinhronizacijaPrivremeniKljuc, " +
                        "SinhronizacijaStatus)" +
                    "VALUES (?, ?, ?, ?, ?)",
                        radniNalogDjelatnik.Id,
                        radniNalogDjelatnik.RadniNalog,
                        radniNalogDjelatnik.Djelatnik,
                        null,
                        2);
            }

            foreach (var pozicija in SyncDataGet.LokacijePozicije)
            {
                db.Execute(
                   "DELETE FROM DID_LokacijaPozicija " +
                   "WHERE POZ_Id = ? " +
                   "OR POZ_Id = ?", pozicija.SinhronizacijaPrivremeniKljuc, pozicija.POZ_Id);

                db.Execute(
                    "INSERT INTO DID_LokacijaPozicija (" +
                        "POZ_Id, " +
                        "SAN_Id, " +
                        "POZ_Broj, " +
                        "POZ_BrojOznaka, " +
                        "POZ_Barcode, " +
                        "POZ_Tip, " +
                        "POZ_Status, " +
                        "POZ_Opis, " +
                        "SinhronizacijaPrivremeniKljuc, " +
                        "SinhronizacijaStatus)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        pozicija.POZ_Id,
                        pozicija.SAN_Id,
                        pozicija.POZ_Broj,
                        pozicija.POZ_BrojOznaka,
                        pozicija.POZ_Barcode,
                        pozicija.POZ_Tip,
                        pozicija.POZ_Status,
                        pozicija.POZ_Opis,
                        null,
                        2);
            }

            foreach (var lokacija in SyncDataGet.Lokacije)
            {
                db.Execute(
                   "DELETE FROM DID_Lokacija " +
                   "WHERE SAN_Id = ? " +
                   "OR SAN_Id = ?", lokacija.SinhronizacijaPrivremeniKljuc, lokacija.SAN_Id);

                db.Execute(
                    "INSERT INTO DID_Lokacija (" +
                        "SAN_Id, " +
                        "SAN_AnketePoPozicijama, " +
                        "SAN_KD_Sifra, " +
                        "SAN_Sifra, " +
                        "SAN_Naziv, " +
                        "SAN_Mjesto, " +
                        "SAN_Naselje, " +
                        "SAN_GradOpcina, " +
                        "SAN_UlicaBroj, " +
                        "SAN_OIBPartner, " +
                        "SAN_Status, " +
                        "SAN_Tip, " +
                        "SAN_Tip2, " +
                        "SinhronizacijaStatus)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        lokacija.SAN_Id,
                        lokacija.SAN_AnketePoPozicijama,
                        lokacija.SAN_KD_Sifra,
                        lokacija.SAN_Sifra,
                        lokacija.SAN_Naziv,
                        lokacija.SAN_Mjesto,
                        lokacija.SAN_Naselje,
                        lokacija.SAN_GradOpcina,
                        lokacija.SAN_UlicaBroj,
                        lokacija.SAN_OIBPartner,
                        lokacija.SAN_Status,
                        lokacija.SAN_Tip,
                        lokacija.SAN_Tip2,
                        2);
            }

            foreach (var anketa in SyncDataGet.Ankete)
            {
                db.Execute(
                   "DELETE FROM DID_Anketa " +
                   "WHERE Id = ?", anketa.Id);

                db.Execute(
                    "INSERT INTO DID_Anketa (" +
                        "Id, " +
                        "ANK_TipDeratizacije, " +
                        "ANK_RadniNalog, " +
                        "ANK_KorisnickoIme, " +
                        "ANK_POZ_Id, " +
                        "ANK_DatumVrijeme, " +
                        "ANK_TipDeratizacijskeKutijeSifra, " +
                        "ANK_Utvrdjeno_Rot_KutijaUredna, " +
                        "ANK_Utvrdjeno_Rot_KutijaOstecena, " +
                        "ANK_Utvrdjeno_Rot_NemaKutije, " +
                        "ANK_Utvrdjeno_Rot_TragoviGlodanja, " +
                        "ANK_Utvrdjeno_Rot_Izmet, " +
                        "ANK_Utvrdjeno_Rot_PojedenaMeka, " +
                        "ANK_Utvrdjeno_Rot_OstecenaMeka, " +
                        "ANK_Utvrdjeno_Rot_UginuliStakor, " +
                        "ANK_Utvrdjeno_Rot_UginuliMis, " +
                        "ANK_Utvrdjeno_Ljep_KutijaUredna, " +
                        "ANK_Utvrdjeno_Ljep_KutijaOstecena, " +
                        "ANK_Utvrdjeno_Ljep_NemaKutije, " +
                        "ANK_Utvrdjeno_Ljep_UlovljenStakor, " +
                        "ANK_Utvrdjeno_Ljep_UlovljenMis, " +
                        "ANK_Utvrdjeno_Ziv_KutijaUredna, " +
                        "ANK_Utvrdjeno_Ziv_KutijaOstecena, " +
                        "ANK_Utvrdjeno_Ziv_NemaKutije, " +
                        "ANK_Utvrdjeno_Ziv_UlovljenStakor, " +
                        "ANK_Utvrdjeno_Ziv_UlovljenMis, " +
                        "ANK_Ucinjeno_Rot_PostavljeniMamci, " +
                        "ANK_Ucinjeno_Rot_NovaKutija, " +
                        "ANK_Ucinjeno_Rot_NadopunjenaMekom, " +
                        "ANK_Ucinjeno_Rot_MasaMeke1, " +
                        "ANK_Ucinjeno_Rot_MasaMeke2, " +
                        "ANK_Ucinjeno_Rot_MasaMeke3, " +
                        "ANK_Ucinjeno_Ljep_NovaLjepljivaPodloska, " +
                        "ANK_Ucinjeno_Ljep_NovaKutija, " +
                        "ANK_Ucinjeno_Ziv_NovaKutija, " +
                        "ANK_RazlogNeizvrsenja, " +
                        "ANK_Napomena, " +
                        "ANK_Poc_Prisutnost_Rupe, " +
                        "ANK_Poc_Prisutnost_TragoviNogu, " +
                        "ANK_Poc_Prisutnost_Izmet, " +
                        "ANK_Poc_Prisutnost_Steta, " +
                        "ANK_Poc_Prisutnost_Leglo, " +
                        "ANK_Poc_Prisutnost_GlodavacZivi, " +
                        "ANK_Poc_Prisutnost_GlodavacUginuli, " +
                        "ANK_Poc_Infestacija_Stakori, " +
                        "ANK_Poc_Infestacija_Misevi, " +
                        "ANK_Poc_Infestacija_DrugiGlodavci, " +
                        "ANK_Poc_Uvjeti_Hrana, " +
                        "ANK_Poc_Uvjeti_NeispravnaOdvodnja, " +
                        "ANK_Poc_Uvjeti_NeuredanOkolis, " +
                        "SinhronizacijaStatus)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        anketa.Id,
                        anketa.ANK_TipDeratizacije,
                        anketa.ANK_RadniNalog,
                        anketa.ANK_KorisnickoIme,
                        anketa.ANK_POZ_Id,
                        anketa.ANK_DatumVrijeme,
                        anketa.ANK_TipDeratizacijskeKutijeSifra,
                        anketa.ANK_Utvrdjeno_Rot_KutijaUredna,
                        anketa.ANK_Utvrdjeno_Rot_KutijaOstecena,
                        anketa.ANK_Utvrdjeno_Rot_NemaKutije,
                        anketa.ANK_Utvrdjeno_Rot_TragoviGlodanja,
                        anketa.ANK_Utvrdjeno_Rot_Izmet,
                        anketa.ANK_Utvrdjeno_Rot_PojedenaMeka,
                        anketa.ANK_Utvrdjeno_Rot_OstecenaMeka,
                        anketa.ANK_Utvrdjeno_Rot_UginuliStakor,
                        anketa.ANK_Utvrdjeno_Rot_UginuliMis,
                        anketa.ANK_Utvrdjeno_Ljep_KutijaUredna,
                        anketa.ANK_Utvrdjeno_Ljep_KutijaOstecena,
                        anketa.ANK_Utvrdjeno_Ljep_NemaKutije,
                        anketa.ANK_Utvrdjeno_Ljep_UlovljenStakor,
                        anketa.ANK_Utvrdjeno_Ljep_UlovljenMis,
                        anketa.ANK_Utvrdjeno_Ziv_KutijaUredna,
                        anketa.ANK_Utvrdjeno_Ziv_KutijaOstecena,
                        anketa.ANK_Utvrdjeno_Ziv_NemaKutije,
                        anketa.ANK_Utvrdjeno_Ziv_UlovljenStakor,
                        anketa.ANK_Utvrdjeno_Ziv_UlovljenMis,
                        anketa.ANK_Ucinjeno_Rot_PostavljeniMamci,
                        anketa.ANK_Ucinjeno_Rot_NovaKutija,
                        anketa.ANK_Ucinjeno_Rot_NadopunjenaMekom,
                        anketa.ANK_Ucinjeno_Rot_MasaMeke1,
                        anketa.ANK_Ucinjeno_Rot_MasaMeke2,
                        anketa.ANK_Ucinjeno_Rot_MasaMeke3,
                        anketa.ANK_Ucinjeno_Ljep_NovaLjepljivaPodloska,
                        anketa.ANK_Ucinjeno_Ljep_NovaKutija,
                        anketa.ANK_Ucinjeno_Ziv_NovaKutija,
                        anketa.ANK_RazlogNeizvrsenja,
                        anketa.ANK_Napomena,
                        anketa.ANK_Poc_Prisutnost_Rupe,
                        anketa.ANK_Poc_Prisutnost_TragoviNogu,
                        anketa.ANK_Poc_Prisutnost_Izmet,
                        anketa.ANK_Poc_Prisutnost_Steta,
                        anketa.ANK_Poc_Prisutnost_Leglo,
                        anketa.ANK_Poc_Prisutnost_GlodavacZivi,
                        anketa.ANK_Poc_Prisutnost_GlodavacUginuli,
                        anketa.ANK_Poc_Infestacija_Stakori,
                        anketa.ANK_Poc_Infestacija_Misevi,
                        anketa.ANK_Poc_Infestacija_DrugiGlodavci,
                        anketa.ANK_Poc_Uvjeti_Hrana,
                        anketa.ANK_Poc_Uvjeti_NeispravnaOdvodnja,
                        anketa.ANK_Poc_Uvjeti_NeuredanOkolis,
                        2);
            }

            foreach (var anketaMaterijal in SyncDataGet.AnketeMaterijali)
            {
                db.Execute(
                   "DELETE FROM DID_AnketaMaterijali " +
                   "WHERE Id = ? " +
                   "OR Id = ?", anketaMaterijal.SinhronizacijaPrivremeniKljuc, anketaMaterijal.Id);

                var materijal = db.Query<T_NAZR>(
                    "SELECT * " +
                    "FROM T_NAZR " +
                    "WHERE NAZR_SIFRA = ?", anketaMaterijal.MaterijalSifra).FirstOrDefault();

                decimal cijena = materijal.NAZR_CIJENA_ART;
                decimal iznos = anketaMaterijal.Kolicina * cijena;

                int lokacijaId = db.Query<DID_LokacijaPozicija>(
                    "SELECT * " +
                    "FROM DID_LokacijaPozicija " +
                    "WHERE POZ_Id = ?", anketaMaterijal.PozicijaId).FirstOrDefault().SAN_Id;


                db.Execute(
                    "INSERT INTO DID_AnketaMaterijali (" +
                        "Id, " +
                        "RadniNalog, " +
                        "PozicijaId, " +
                        "MaterijalSifra, " +
                        "MjernaJedinica, " +
                        "Kolicina, " +
                        "MaterijalNaziv, " +
                        "Iznos, " +
                        "Cijena, " +
                        "LokacijaId)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        anketaMaterijal.Id,
                        anketaMaterijal.RadniNalog,
                        anketaMaterijal.PozicijaId,
                        anketaMaterijal.MaterijalSifra,
                        anketaMaterijal.MjernaJedinica,
                        anketaMaterijal.Kolicina,
                        materijal.NAZR_NAZIV,
                        iznos,
                        cijena,
                        lokacijaId);
            }

            foreach (var radniNalogLokacija in SyncDataGet.RadniNaloziLokacije)
            {
                db.Execute(
                   "DELETE FROM DID_RadniNalog_Lokacija " +
                   "WHERE Id = ? " +
                   "OR Id = ?", radniNalogLokacija.SinhronizacijaPrivremeniKljuc, radniNalogLokacija.Id);

                db.Execute(
                    "INSERT INTO DID_RadniNalog_Lokacija (" +
                        "Id, " +
                        "TipAkcije, " +
                        "RadniNalog, " +
                        "Lokacija, " +
                        "RedniBroj, " +
                        "VrijemeDolaska, " +
                        "OpisPosla, " +
                        "Napomena, " +
                        "RazlogNeizvrsenja, " +
                        "Status, " +
                        "SinhronizacijaStatus)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        radniNalogLokacija.Id,
                        radniNalogLokacija.TipAkcije,
                        radniNalogLokacija.RadniNalog,
                        radniNalogLokacija.Lokacija,
                        radniNalogLokacija.RedniBroj,
                        radniNalogLokacija.VrijemeDolaska,
                        radniNalogLokacija.OpisPosla,
                        radniNalogLokacija.Napomena,
                        radniNalogLokacija.RazlogNeizvrsenja,
                        radniNalogLokacija.Status,
                        2);
            }

            foreach (var komitent in SyncDataGet.Komitenti)
            {
                db.Execute(
                   "DELETE FROM T_KUPDOB " +
                   "WHERE SIFRA = ? " +
                   "OR SIFRA = ?", komitent.SinhronizacijaPrivremeniKljuc, komitent.SIFRA);

                db.Execute(
                    "INSERT INTO T_KUPDOB (" +
                        "SIFRA, " +
                        "TIP_PARTNERA, " +
                        "NAZIV, " +
                        "POSTA, " +
                        "GRAD, " +
                        "ULICA, " +
                        "UL_BROJ, " +
                        "DRZAVA, " +
                        "OIB, " +
                        "OIB2, " +
                        "ZIRO, " +
                        "TELEFON)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        komitent.SIFRA,
                        komitent.TIP_PARTNERA,
                        komitent.NAZIV,
                        komitent.POSTA,
                        komitent.GRAD,
                        komitent.ULICA,
                        komitent.UL_BROJ,
                        komitent.DRZAVA,
                        komitent.OIB,
                        komitent.OIB2,
                        komitent.ZIRO,
                        komitent.TELEFON);
            }

            foreach (var radniNalogMaterijal in SyncDataGet.RadniNaloziMaterijali)
            {
                db.Execute(
                   "DELETE FROM DID_RadniNalog_Materijal " +
                   "WHERE Id = ? " +
                   "OR Id = ?", radniNalogMaterijal.SinhronizacijaPrivremeniKljuc, radniNalogMaterijal.Id);

                db.Execute(
                    "INSERT INTO DID_RadniNalog_Materijal (" +
                        "Id, " +
                        "RadniNalog, " +
                        "Materijal, " +
                        "MaterijalNaziv, " +
                        "MjernaJedninica, " +
                        "MjernaJedinicaOznaka, " +
                        "Izdano, " +
                        "Vraceno, " +
                        "StanjeKolicine, " +
                        "Utroseno, " +
                        "Predvidjeno)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        radniNalogMaterijal.Id,
                        radniNalogMaterijal.RadniNalog,
                        radniNalogMaterijal.Materijal,
                        radniNalogMaterijal.MaterijalNaziv,
                        radniNalogMaterijal.MjernaJedninica,
                        radniNalogMaterijal.MjernaJedinicaOznaka,
                        radniNalogMaterijal.Izdano,
                        radniNalogMaterijal.Vraceno,
                        radniNalogMaterijal.Izdano - radniNalogMaterijal.Utroseno,
                        radniNalogMaterijal.Utroseno,
                        radniNalogMaterijal.Predvidjeno);
            }

            foreach (var stanjeSkladista in SyncDataGet.StanjaSkladista)
            {
                db.Execute(
                   "DELETE FROM DID_StanjeSkladista " +
                   "WHERE Skladiste = ? " +
                   "AND Materijal = ?", stanjeSkladista.Skladiste, stanjeSkladista.Materijal);

                db.Execute(
                    "INSERT INTO DID_StanjeSkladista (" +
                        "Skladiste, " +
                        "SkladisteNaziv, " +
                        "Materijal, " +
                        "MaterijalNaziv, " +
                        "MjernaJedinica, " +
                        "MjernaJedinicaOznaka, " +
                        "TarifniBroj, " +
                        "TarifniBrojStopa, " +
                        "Kolicina, " +
                        "Cijena, " +
                        "Iznos)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        stanjeSkladista.Skladiste,
                        stanjeSkladista.SkladisteNaziv,
                        stanjeSkladista.Materijal,
                        stanjeSkladista.MaterijalNaziv,
                        stanjeSkladista.MjernaJedinica,
                        stanjeSkladista.MjernaJedinicaOznaka,
                        stanjeSkladista.TarifniBroj,
                        stanjeSkladista.TarifniBrojStopa,
                        stanjeSkladista.Kolicina,
                        stanjeSkladista.Cijena,
                        stanjeSkladista.Iznos);
            }

            foreach (var potvrda in SyncDataGet.Potvrde)
            {
                var skladista = db.Query<DID_Potvrda>("SELECT * FROM DID_Potvrda");

                db.Execute(
                   "DELETE FROM DID_Potvrda " +
                   "WHERE Id = ? " +
                   "OR Id = ?", potvrda.SinhronizacijaPrivremeniKljuc, potvrda.Id);

                var skladista1 = db.Query<DID_Potvrda>("SELECT * FROM DID_Potvrda");

                db.Execute(
                    "INSERT INTO DID_Potvrda (" +
                        "Id, " +
                        "Godina, " +
                        "Broj, " +
                        "RadniNalog, " +
                        "Lokacija, " +
                        "DatumVrijeme, " +
                        "Infestacija, " +
                        "OpisRada, " +
                        "RadniNalogLokacijaId, " +
                        "LokacijaTip, " +
                        "LokacijaTipOdradjen, " +
                        "LokacijaTipBrojObjekata, " +
                        "LokacijaTip2, " +
                        "LokacijaTip2Odradjen, " +
                        "LokacijaTip2BrojObjekata)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        potvrda.Id,
                        potvrda.Godina,
                        potvrda.Broj,
                        potvrda.RadniNalog,
                        potvrda.Lokacija,
                        potvrda.DatumVrijeme,
                        potvrda.Infestacija,
                        potvrda.OpisRada,
                        potvrda.RadniNalogLokacijaId,
                        potvrda.LokacijaTip,
                        potvrda.LokacijaTipOdradjen,
                        potvrda.LokacijaTipBrojObjekata,
                        potvrda.LokacijaTip2,
                        potvrda.LokacijaTip2Odradjen,
                        potvrda.LokacijaTip2BrojObjekata);

                var skladista3 = db.Query<DID_Potvrda>("SELECT * FROM DID_Potvrda");
            }

            foreach (var potvrdaMaterijal in SyncDataGet.PotvrdaMaterijali)
            {
                db.Execute(
                   "DELETE FROM DID_Potvrda_Materijal " +
                   "WHERE Id = ? " +
                   "OR Id = ?", potvrdaMaterijal.SinhronizacijaPrivremeniKljuc, potvrdaMaterijal.Id);

                db.Execute(
                    "INSERT INTO DID_Potvrda_Materijal (" +
                        "Id, " +
                        "Potvrda, " +
                        "Materijal, " +
                        "MaterijalNaziv, " +
                        "DjelatnaTvar, " +
                        "DjelatnaTvarOpis, " +
                        "Utroseno, " +
                        "UdjeliDjelatnihTvariOpis, " +
                        "Razrjedjenje, " +
                        "Predvidjeno)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        potvrdaMaterijal.Id,
                        potvrdaMaterijal.Potvrda,
                        potvrdaMaterijal.Materijal,
                        potvrdaMaterijal.MaterijalNaziv,
                        potvrdaMaterijal.DjelatnaTvar,
                        potvrdaMaterijal.DjelatnaTvarOpis,
                        potvrdaMaterijal.UdioDjelatneTvari,
                        potvrdaMaterijal.Utroseno,
                        potvrdaMaterijal.UdjeliDjelatnihTvariOpis,
                        potvrdaMaterijal.Razrjedjenje,
                        potvrdaMaterijal.Predvidjeno);
            }

            foreach (var potvrdaNametnik in SyncDataGet.PotvrdaNametnici)
            {
                db.Execute(
                   "DELETE FROM DID_Potvrda_Nametnik " +
                   "WHERE Id = ? " +
                   "OR Id = ?", potvrdaNametnik.SinhronizacijaPrivremeniKljuc, potvrdaNametnik.Id);

                db.Execute(
                    "INSERT INTO DID_Potvrda_Nametnik (" +
                        "Id, " +
                        "Potvrda, " +
                        "Nametnik)" +
                    "VALUES (?, ?, ?)",
                        potvrdaNametnik.Id,
                        potvrdaNametnik.Potvrda,
                        potvrdaNametnik.Nametnik);
            }

            foreach (var potvrdaDjelatnost in SyncDataGet.PotvrdeDjelatnosti)
            {
                db.Execute(
                   "DELETE FROM DID_Potvrda_Djelatnost " +
                   "WHERE Id = ? " +
                   "OR Id = ?", potvrdaDjelatnost.SinhronizacijaPrivremeniKljuc, potvrdaDjelatnost.Id);

                db.Execute(
                    "INSERT INTO DID_Potvrda_Djelatnost (" +
                        "Id, " +
                        "Potvrda, " +
                        "Djelatnost, " +
                        "TipPosla, " +
                        "BrojObjekata, " +
                        "ObjektiTip1, " +
                        "ObjektiTip2, " +
                        "BrojObjekataTip2)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                        potvrdaDjelatnost.Id,
                        potvrdaDjelatnost.Potvrda,
                        potvrdaDjelatnost.Djelatnost,
                        potvrdaDjelatnost.TipPosla,
                        potvrdaDjelatnost.BrojObjekata,
                        potvrdaDjelatnost.ObjektiTip1,
                        potvrdaDjelatnost.ObjektiTip2,
                        potvrdaDjelatnost.BrojObjekataTip2);
            }

            foreach (var provedbeniPlan in SyncDataGet.ProvedbeniPlanovi)
            {
                var dsds = db.Query<DID_ProvedbeniPlan>("SELECT * FROM DID_ProvedbeniPlan");

                db.Execute(
                    "DELETE FROM DID_ProvedbeniPlan " +
                    "WHERE Id = ? " +
                    "OR Id = ?", provedbeniPlan.SinhronizacijaPrivremeniKljuc, provedbeniPlan.Id);

                var dsds2 = db.Query<DID_ProvedbeniPlan>("SELECT * FROM DID_ProvedbeniPlan");

                db.Execute(
                    "INSERT INTO DID_ProvedbeniPlan (" +
                        "Id, " +
                        "Lokacija, " +
                        "Godina, " +
                        "Datum, " +
                        "VrijemeIzvida, " +
                        "Izdao, " +
                        "Preuzeo, " +
                        "HigijenskoStanje, " +
                        "HigijenskoStanjeNedostatci, " +
                        "GradjevinskoStanje, " +
                        "GradjevinskoStanjeNedostatci, " +
                        "TehnickaOpremljenost, " +
                        "GlodavciDatumPostavljanjaKlopki, " +
                        "GlodavciDatumIzvida, " +
                        "GlodavciZapazanja, " +
                        "InsektiDatumPostavljanjaKlopki, " +
                        "InsektiDatumIzvida, " +
                        "InsektiZapazanja, " +
                        "GlodavciKriticneTocke, " +
                        "InsektiKriticneTocke, " +
                        "Preporuke, " +
                        "Napomene, " +
                        "BrojIzvidaGlodavacaGodisnje, " +
                        "RasporedIzvidaGlodavaca, " +
                        "BrojOpcihDeratizacijaGodisnje, " +
                        "RasporedRadaDeratizacije, " +
                        "BrojIzvidaInsekataGodisnje , " +
                        "RasporedIzvidaInsekata, " +
                        "BrojOpcihDezinsekcijaGodisnje , " +
                        "RasporedRadaDezinsekcije, " +
                        "SatZaProvedbuDezinsekcije, " +
                        "BrojDanaDoDodatneKontroleInsekata, " +
                        "BrojDanaDoDodatneDezinsekcije, " +
                        "BrojOpcihDezinfekcijaGodisnje, " +
                        "RasporedRadaDezinfekcija, " +
                        "SatZaProvedbuDezinfekcije)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        provedbeniPlan.Id,
                        provedbeniPlan.Lokacija,
                        provedbeniPlan.Godina,
                        provedbeniPlan.Datum,
                        provedbeniPlan.VrijemeIzvida,
                        provedbeniPlan.Izdao,
                        provedbeniPlan.Preuzeo,
                        provedbeniPlan.HigijenskoStanje,
                        provedbeniPlan.HigijenskoStanjeNedostatci,
                        provedbeniPlan.GradjevinskoStanje,
                        provedbeniPlan.GradjevinskoStanjeNedostatci,
                        provedbeniPlan.TehnickaOpremljenost,
                        provedbeniPlan.GlodavciDatumPostavljanjaKlopki,
                        provedbeniPlan.GlodavciDatumIzvida,
                        provedbeniPlan.GlodavciZapazanja,
                        provedbeniPlan.InsektiDatumPostavljanjaKlopki,
                        provedbeniPlan.InsektiDatumIzvida,
                        provedbeniPlan.InsektiZapazanja,
                        provedbeniPlan.GlodavciKriticneTocke,
                        provedbeniPlan.InsektiKriticneTocke,
                        provedbeniPlan.Preporuke,
                        provedbeniPlan.Napomene,
                        provedbeniPlan.BrojIzvidaGlodavacaGodisnje,
                        provedbeniPlan.RasporedIzvidaGlodavaca,
                        provedbeniPlan.BrojOpcihDeratizacijaGodisnje,
                        provedbeniPlan.RasporedRadaDeratizacije,
                        provedbeniPlan.BrojIzvidaInsekataGodisnje,
                        provedbeniPlan.RasporedIzvidaInsekata,
                        provedbeniPlan.BrojOpcihDezinsekcijaGodisnje,
                        provedbeniPlan.RasporedRadaDezinsekcije,
                        provedbeniPlan.SatZaProvedbuDezinsekcije,
                        provedbeniPlan.BrojDanaDoDodatneKontroleInsekata,
                        provedbeniPlan.BrojDanaDoDodatneDezinsekcije,
                        provedbeniPlan.BrojOpcihDezinfekcijaGodisnje,
                        provedbeniPlan.RasporedRadaDezinfekcija,
                        provedbeniPlan.SatZaProvedbuDezinfekcije);

                var dsds3 = db.Query<DID_ProvedbeniPlan>("SELECT * FROM DID_ProvedbeniPlan");
            }

            foreach (var provedbeniPlanMaterijal in SyncDataGet.ProvedbeniPlanMaterijali)
            {
                db.Execute(
                    "DELETE FROM DID_ProvedbeniPlan_Materijal " +
                    "WHERE Id = ? " +
                    "OR Id = ?", provedbeniPlanMaterijal.SinhronizacijaPrivremeniKljuc, provedbeniPlanMaterijal.Id);

                db.Execute(
                    "INSERT INTO DID_ProvedbeniPlan_Materijal (" +
                        "Id, " +
                        "ProvedbeniPlan, " +
                        "MaterijalSifra, " +
                        "MaterijalNaziv, " +
                        "TipKemikalije) " +
                    "VALUES (?, ?, ?, ?, ?)",
                        provedbeniPlanMaterijal.Id,
                        provedbeniPlanMaterijal.ProvedbeniPlan,
                        provedbeniPlanMaterijal.MaterijalSifra,
                        provedbeniPlanMaterijal.MaterijalNaziv,
                        provedbeniPlanMaterijal.TipKemikalije);
            }

            foreach (var provedbeniPlanNametnik in SyncDataGet.ProvedbeniPlanNametnici)
            {
                db.Execute(
                    "DELETE FROM DID_ProvedbeniPlan_Nametnik " +
                    "WHERE Id = ? " +
                    "OR Id = ?", provedbeniPlanNametnik.SinhronizacijaPrivremeniKljuc, provedbeniPlanNametnik.Id);

                db.Execute(
                    "INSERT INTO DID_ProvedbeniPlan_Nametnik (" +
                        "Id, " +
                        "ProvedbeniPlan, " +
                        "NametnikSifra, " +
                        "NametnikNaziv) " +
                    "VALUES (?, ?, ?, ?)",
                        provedbeniPlanNametnik.Id,
                        provedbeniPlanNametnik.ProvedbeniPlan,
                        provedbeniPlanNametnik.NametnikSifra,
                        provedbeniPlanNametnik.NametnikNaziv);
            }

        }

        private string CallWebService(string JSON)
        {
            try
            {
                String output = "";
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 180);
                    string requestUrl = "http://services.infobit.info/DDD_Test/SyncService.svc/sync/all";
                    //string requestUrl = "http://services.infobit.info/DDD/SyncService.svc/sync/all";
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var request = new StringContent(JSON, Encoding.UTF8, "application/json");
                    var response = client.PostAsync(requestUrl, request).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        output = response.Content.ReadAsStringAsync().Result;
                        SyncDataGet = JsonConvert.DeserializeObject<DeratizacijaSyncData>(JsonConvert.DeserializeObject<string>(output));
                        if (!SyncDataGet.Login.Valid)
                        {
                            syncErrMessage = "Greška prilikom autorizacije! Provjerite korisničko ime i lozinku!";
                            output = "Greška prilikom autorizacije! Provjerite korisničko ime i lozinku!";
                            throw new CallServiceExeption(output);
                        }
                        else if (SyncDataGet.Error != null)
                        {
                            syncErrMessage = "Došlo je do pogreške prilikom sinhronizacije:\n" + SyncDataGet.Error.Message;
                            output = "Došlo je do pogreške prilikom sinhronizacije:\n" + SyncDataGet.Error.Message;
                            throw new CallServiceExeption(output);
                        }
                    }
                    else
                    {
                        syncErrMessage = "Došlo je do neočekivane pogreške prilikom poziva sinhronizacije!";
                        output = "Došlo je do neočekivane pogreške prilikom poziva sinhronizacije!";
                        throw new CallServiceExeption(output);
                    }
                }
                return output;
            }
            catch (Exception e)
            {
                throw new CallServiceExeption(e.Message);
                //return e.Message;
                //syncErrMessage = "Došlo je do pogreške prilikom sinhronizacije:\n" + SyncDataGet.Error.Message;
                //output = "Došlo je do pogreške prilikom sinhronizacije:\n" + SyncDataGet.Error.Message;
            }
        }

        public void PrikazSinkronizacije()
        {
            RunOnUiThread(() =>
            {
                bool dataFlag = false;
                
                List<Adapter_SyncData> ispis = new List<Adapter_SyncData>();

                if (SyncDataSend.RadniNalozi.Any() || SyncDataGet.RadniNalozi.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Radni nalog", SyncDataSend.RadniNalozi.Count(), SyncDataGet.RadniNalozi.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.Lokacije.Any() || SyncDataGet.Lokacije.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Lokacije", SyncDataSend.Lokacije.Count(), SyncDataGet.Lokacije.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.LokacijePozicije.Any() || SyncDataGet.LokacijePozicije.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Pozicije", SyncDataSend.LokacijePozicije.Count(), SyncDataGet.LokacijePozicije.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.Potvrde.Any() || SyncDataGet.Potvrde.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Potvrde", SyncDataSend.Potvrde.Count(), SyncDataGet.Potvrde.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.PotvrdaNametnici.Any() || SyncDataGet.PotvrdaNametnici.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Nametnici potvrda", SyncDataSend.PotvrdaNametnici.Count(), SyncDataGet.PotvrdaNametnici.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.PotvrdeDjelatnosti.Any() || SyncDataGet.PotvrdeDjelatnosti.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Djelatnost potvrda", SyncDataSend.PotvrdeDjelatnosti.Count(), SyncDataGet.PotvrdeDjelatnosti.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.Komitenti.Any() || SyncDataGet.Komitenti.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Komitenti", SyncDataSend.Komitenti.Count(), SyncDataGet.Komitenti.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.Ankete.Any() || SyncDataGet.Ankete.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Ankete", SyncDataSend.Ankete.Count(), SyncDataGet.Ankete.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.RadniNaloziLokacije.Any() || SyncDataGet.RadniNaloziLokacije.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Radni nalozi lokacije", SyncDataSend.RadniNaloziLokacije.Count(), SyncDataGet.RadniNaloziLokacije.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.RadniNaloziMaterijali.Any() || SyncDataGet.RadniNaloziMaterijali.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Radni nalozi materijali", SyncDataSend.RadniNaloziMaterijali.Count(), SyncDataGet.RadniNaloziMaterijali.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.AnketeMaterijali.Any() || SyncDataGet.AnketeMaterijali.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Anketa materijali", SyncDataSend.AnketeMaterijali.Count(), SyncDataGet.AnketeMaterijali.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.StanjaSkladista.Any() || SyncDataGet.StanjaSkladista.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Stanja skladista", SyncDataSend.StanjaSkladista.Count(), SyncDataGet.StanjaSkladista.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.Djelatnici.Any() || SyncDataGet.Djelatnici.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Djelatnici", SyncDataSend.Djelatnici.Count(), SyncDataGet.Djelatnici.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.ProvedbeniPlanovi.Any() || SyncDataGet.ProvedbeniPlanovi.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Provedbeni planovi", SyncDataSend.ProvedbeniPlanovi.Count(), SyncDataGet.ProvedbeniPlanovi.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.ProvedbeniPlanMaterijali.Any() || SyncDataGet.ProvedbeniPlanMaterijali.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Provedbeni plan materijali", SyncDataSend.ProvedbeniPlanMaterijali.Count(), SyncDataGet.ProvedbeniPlanMaterijali.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.ProvedbeniPlanNametnici.Any() || SyncDataGet.ProvedbeniPlanNametnici.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Provedbeni plan nametnici", SyncDataSend.ProvedbeniPlanNametnici.Count(), SyncDataGet.ProvedbeniPlanNametnici.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }

                if (dataFlag)
                {
                    info.Visibility = Android.Views.ViewStates.Gone;
                    prikazPodataka.Visibility = Android.Views.ViewStates.Visible;
                    sinkronizacijaListView.Visibility = Android.Views.ViewStates.Visible;
                    Toast.MakeText(this, "Sinkronizacija je uspješno izvršena!", ToastLength.Long).Show();
                    mLayoutManager = new LinearLayoutManager(this);
                    mAdapter = new Adapter_Syncdata(ispis);
                    sinkronizacijaListView.SetLayoutManager(mLayoutManager);
                    sinkronizacijaListView.SetAdapter(mAdapter);
                }
                else
                {
                    Toast.MakeText(this, "Sinkronizacija je uspješno izvršena!", ToastLength.Long).Show();
                    Toast.MakeText(this, "Sinkronizacija je uspješno zavrsena, ali se nije sinkronizirao niti jedan podatak!", ToastLength.Long).Show();
                    info.Visibility = Android.Views.ViewStates.Visible;
                    message.Text = "Sinkronizacija je uspješno zavrsena, ali se nije sinkronizirao niti jedan podatak!";
                    message.Visibility = Android.Views.ViewStates.Visible;
                    progressBar1.Visibility = Android.Views.ViewStates.Gone;
                    prikazPodataka.Visibility = Android.Views.ViewStates.Gone;
                    sinkronizacijaListView.Visibility = Android.Views.ViewStates.Gone;
                }
            });
        }

        public class CallServiceExeption : Exception
        {
            public CallServiceExeption(string message)
               : base(message)
            {
            }
        }   

        public void PrikazErrora(string err)
        {
            RunOnUiThread(() =>
            {
                alertIcon.Visibility = Android.Views.ViewStates.Visible;
                prikazPodataka.Visibility = Android.Views.ViewStates.Gone;
                progressBar1.Visibility = Android.Views.ViewStates.Gone;
                sinkronizacijaListView.Visibility = Android.Views.ViewStates.Gone;
                message.Text = "Pogreška! Sinkronizacija nije uspješno izvršena!\n\n " + syncErrMessage;
                message.SetTextColor(ContextCompat.GetColorStateList(this, Resource.Color.redPrimary));
            });
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.TitleFormatted.ToString() == "Početna")
            {
                if (flag)
                {
                    intent = new Intent(this, typeof(Activity_Pocetna));
                    StartActivity(intent);
                }
                else
                    Toast.MakeText(this, "Ne možete prekiniti sinkronizaciju jer bi moglo doći do gubljenja podataka", ToastLength.Long).Show();
            }
            else if (item.TitleFormatted.ToString() == "Radni nalozi")
            {
                if (flag)
                {
                    intent = new Intent(this, typeof(Activity_RadniNalozi));
                    StartActivity(intent);
                }
                else
                    Toast.MakeText(this, "Ne možete prekiniti sinkronizaciju jer bi moglo doći do gubljenja podataka", ToastLength.Long).Show();
            }
            else if (item.TitleFormatted.ToString() == "Prikaz odradenih anketa")
            {
                if (flag)
                {
                    intent = new Intent(this, typeof(Activity_OdradeneAnkete));
                    StartActivity(intent);
                }
                else
                    Toast.MakeText(this, "Ne možete prekiniti sinkronizaciju jer bi moglo doći do gubljenja podataka", ToastLength.Long).Show();
            }
            else if (item.TitleFormatted.ToString() == "Prikaz potrošenih materijala")
            {
                if (flag)
                {
                    intent = new Intent(this, typeof(Activity_PotroseniMaterijali));
                    StartActivity(intent);
                }
                else
                    Toast.MakeText(this, "Ne možete prekiniti sinkronizaciju jer bi moglo doći do gubljenja podataka", ToastLength.Long).Show();
            }
            else if (item.TitleFormatted.ToString() == "Sinkronizacija radnih naloga")
            {
                if (flag)
                {
                    intent = new Intent(this, typeof(Activity_Sinkronizacija));
                    StartActivity(intent);
                }
                else
                    Toast.MakeText(this, "Ne možete prekiniti sinkronizaciju jer bi moglo doći do gubljenja podataka", ToastLength.Long).Show();
            }
            else if (item.TitleFormatted.ToString() == "Sinkronizacija matičnih podataka")
            {
                if (flag)
                {
                    intent = new Intent(this, typeof(Activity_SinkronizacijaMaticnihPodataka));
                    StartActivity(intent);
                }
                else
                    Toast.MakeText(this, "Ne možete prekiniti sinkronizaciju jer bi moglo doći do gubljenja podataka", ToastLength.Long).Show();
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}


#region Comment
//public void PrikazSinkronizacije()
//{
//    prikazPodataka.Visibility = Android.Views.ViewStates.Visible;
//    sinkronizacijaListView.Visibility = Android.Views.ViewStates.Visible;
//    List<string> ispis = new List<string>();



//    if (SyncDataSend.RadniNalozi.Any())
//        ispis.Add("Radni nalog " + SyncDataSend.RadniNalozi.Count.ToString() + " / " + SyncDataGet.RadniNalozi.Count.ToString());
//    if (SyncDataSend.Komitenti.Any())
//        ispis.Add("Komitenti " + SyncDataSend.Komitenti.Count.ToString() + " / " + SyncDataGet.Komitenti.Count.ToString());
//    if (SyncDataSend.Lokacije.Any())
//        ispis.Add("Lokacije " + SyncDataSend.Lokacije.Count.ToString() + " / " + SyncDataGet.Lokacije.Count.ToString());
//    if (SyncDataSend.LokacijePozicije.Any())
//        ispis.Add("Pozicije " + SyncDataSend.LokacijePozicije.Count.ToString() + " / " + SyncDataGet.LokacijePozicije.Count.ToString());

//    ArrayAdapter<string> adapterRNList = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, ispis);
//    sinkronizacijaListView.Adapter = adapterRNList;
//}




//AssetManager assets = Assets;
//using (StreamReader stream_reader = new StreamReader(json))
//{
//    JsonSerializer serializer = new JsonSerializer();
//    DeratizacijaSyncDataSend data = (DeratizacijaSyncDataSend)serializer.Deserialize(json, typeof(DeratizacijaSyncDataSend));
//}


//Button button = FindViewById<Button>(Resource.Id.myButton);
//button.Click += delegate
//{
//    System.IO.MemoryStream ms = new System.IO.MemoryStream();
//    System.IO.StreamWriter writer = new System.IO.StreamWriter(ms);
//    writer.Write(json);
//    writer.Flush();
//    ms.Position = 0;

//    System.Net.Mime.ContentType ct = new System.Net.Mime.ContentType(System.Net.Mime.MediaTypeNames.Text.Plain);
//    System.Net.Mail.Attachment attach = new System.Net.Mail.Attachment(ms, ct);
//    attach.ContentDisposition.FileName = "myFile.txt";

//    string subject = "Xamarin App";
//    string body = "U privitku se nalazi JSON.txt file";
//    var mail = new MailMessage();
//    var smtpServer = new SmtpClient("smtp.gmail.com", 587);
//    mail.From = new MailAddress("grancaric.franko@gmail.com");
//    //mail.To.Add("zeljko.tutic@gmail.com");
//    mail.To.Add("jivekovi@gmail.com");
//    mail.Subject = subject;
//    mail.Body = body;

//    mail.Attachments.Add(attach);
//    smtpServer.Credentials = new NetworkCredential("grancaric.franko@gmail.com", "fsfsd");
//    smtpServer.UseDefaultCredentials = false;
//    smtpServer.EnableSsl = true;
//    smtpServer.Send(mail);

//    writer.Dispose();
//    ms.Close();
//};






//      if (response.IsSuccessStatusCode)
//                {
//                    HttpContent stream = response.Content;
//var data = stream.ReadAsStringAsync();
//output = data.Result;
//                }
//                else
//                {
//                    output = "No result - status code: " + response.StatusCode.ToString();
//                    throw new CallServiceExeption(output);
//                }

#endregion