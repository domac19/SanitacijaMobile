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
    [Activity(Label = "Activity_SinkronizacijaMaticnihPodataka", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_SinkronizacijaMaticnihPodataka : Activity
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
        string syncErrMessage;
        DeratizacijaSyncData SyncDataSend, SyncDataGet;
        bool flag = false;
        DID_DjelatnikUsername user;

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
            message.Text = "Sinkronizacija matičnih podataka u tijeku...";

            if (CrossConnectivity.Current.IsConnected)
            {
                user = db.Query<DID_DjelatnikUsername>(
                    "SELECT * " +
                    "FROM DID_DjelatnikUsername " +
                    "WHERE Djelatnik = ?", localUsername.GetString("sifraDjelatnika", null)).FirstOrDefault();
                SyncDataSend = new DeratizacijaSyncData(user.Username, user.Password);
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

        public async Task Sinkornizacija()
        {
            await Task.Run(() => {
                try
                {
                    CallWebService(JsonConvert.ToString(JsonConvert.SerializeObject(SyncDataSend)));
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

        public void SpremanjePrimljenihPodataka()   
        {
            foreach (var djelatnik in SyncDataGet.Djelatnici)
            {
                try
                {
                    db.Execute(
                    "INSERT INTO KE_DJELATNICI ( " +
                        "KE_MBR, " +
                        "KE_IME, " +
                        "KE_PREZIME, " +
                        "KE_MOBITEL, " +
                        "KE_OIB, " +
                        "KE_EMAIL, " +
                        "KE_ADRESA, " +
                        "KE_POSTA, " +
                        "KE_MJESTO) " +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        djelatnik.KE_MBR,
                        djelatnik.KE_IME,
                        djelatnik.KE_PREZIME,
                        djelatnik.KE_MOBITEL,
                        djelatnik.KE_OIB,
                        djelatnik.KE_EMAIL,
                        djelatnik.KE_ADRESA,
                        djelatnik.KE_POSTA,
                        djelatnik.KE_MJESTO);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE KE_DJELATNICI " +
                        "SET KE_IME = ?, " +
                            "KE_PREZIME = ?, " +
                            "KE_MOBITEL = ?, " +
                            "KE_OIB = ?, " +
                            "KE_EMAIL = ?, " +
                            "KE_ADRESA = ?, " +
                            "KE_POSTA = ?, " +
                            "KE_MJESTO = ? " +
                        "WHERE KE_MBR = ?",
                            djelatnik.KE_IME,
                            djelatnik.KE_PREZIME,
                            djelatnik.KE_MOBITEL,
                            djelatnik.KE_OIB,
                            djelatnik.KE_EMAIL,
                            djelatnik.KE_ADRESA,
                            djelatnik.KE_POSTA,
                            djelatnik.KE_MJESTO,
                            djelatnik.KE_MBR);
                }
            }

            foreach (var djelatnikUsername in SyncDataGet.DjelatniciUsername)
            {
                try
                {
                    db.Execute(
                        "INSERT INTO DID_DjelatnikUsername (" +
                            "Djelatnik, " +
                            "Username, " +
                            "Password)" +
                        "VALUES (?, ?, ?)",
                            djelatnikUsername.Djelatnik,
                            djelatnikUsername.Username,
                            djelatnikUsername.Password);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_DjelatnikUsername " +
                        "SET Username = ?, " +
                            "Password = ? " +
                        "WHERE Djelatnik = ?",
                            djelatnikUsername.Username,
                            djelatnikUsername.Password,
                            djelatnikUsername.Djelatnik);
                }
            }

            foreach (var pozicija in SyncDataGet.LokacijePozicije)
            {
                try
                {
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
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_LokacijaPozicija " +
                        "SET SAN_Id = ?, " +
                            "POZ_Broj = ?, " +
                            "POZ_BrojOznaka = ?, " +
                            "POZ_Barcode = ?, " +
                            "POZ_Tip = ?, " +
                            "POZ_Status = ?, " +
                            "POZ_Opis = ?, " +
                            "SinhronizacijaPrivremeniKljuc = ?, " +
                            "SinhronizacijaStatus = ? " +
                        "WHERE POZ_Id = ?",
                            pozicija.SAN_Id,
                            pozicija.POZ_Broj,
                            pozicija.POZ_BrojOznaka,
                            pozicija.POZ_Barcode,
                            pozicija.POZ_Tip,
                            pozicija.POZ_Status,
                            pozicija.POZ_Opis,
                            null,
                            2,
                            pozicija.POZ_Id);
                }
            }

            foreach (var lokacija in SyncDataGet.Lokacije)
            {
                try
                {
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
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_Lokacija " +
                        "SET SAN_AnketePoPozicijama = ?, " +
                            "SAN_KD_Sifra = ?, " +
                            "SAN_Sifra = ?, " +
                            "SAN_Naziv = ?, " +
                            "SAN_Mjesto = ?, " +
                            "SAN_Naselje = ?, " +
                            "SAN_GradOpcina = ?, " +
                            "SAN_UlicaBroj = ?, " +
                            "SAN_OIBPartner = ?, " +
                            "SAN_Status = ?, " +
                            "SAN_Tip = ?, " +
                            "SAN_Tip2 = ?," +
                            "SinhronizacijaPrivremeniKljuc = ?, " +
                            "SinhronizacijaStatus = ? " +
                        "WHERE SAN_Id = ?",
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
                            null,
                            2,
                            lokacija.SAN_Id);
                }
            }

            foreach (var komitent in SyncDataGet.Komitenti)
            {
                try
                {
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
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE T_KUPDOB " +
                        "SET TIP_PARTNERA = ?, " +
                            "NAZIV = ?, " +
                            "POSTA = ?, " +
                            "GRAD = ?, " +
                            "ULICA = ?, " +
                            "UL_BROJ = ?, " +
                            "DRZAVA = ?, " +
                            "OIB = ?, " +
                            "OIB2 = ?, " +
                            "ZIRO = ?, " +
                            "TELEFON = ?, " +
                            "SinhronizacijaPrivremeniKljuc = ? " +
                        "WHERE SIFRA = ?",
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
                            komitent.TELEFON,
                            null,
                            komitent.SinhronizacijaPrivremeniKljuc,
                            komitent.SIFRA);
                }
            }

            foreach (var djelatnost in SyncDataGet.Djelatnosti)
            {
                try
                {
                    db.Execute(
                        "INSERT INTO DID_Djelatnost (" +
                            "Sifra, " +
                            "Naziv)" +
                        "VALUES (?, ?)",
                            djelatnost.Naziv,
                            djelatnost.Sifra);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_Djelatnost " +
                        "SET Naziv = ? " +
                        "WHERE Sifra = ?",
                            djelatnost.Naziv,
                            djelatnost.Sifra);
                }
            }

            foreach (var nametnik in SyncDataGet.Nametnici) 
            {
                try
                {
                    db.Execute(
                        "INSERT INTO DID_Nametnik (" +
                            "Sifra, " +
                            "Naziv, " +
                            "Tip)" +
                        "VALUES (?, ?, ?)",
                            nametnik.Sifra,
                            nametnik.Naziv,
                            nametnik.Tip);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_Nametnik " +
                        "SET Naziv = ?, " +
                            "Tip = ? " +
                        "WHERE Sifra = ?",
                            nametnik.Naziv,
                            nametnik.Tip,
                            nametnik.Sifra);
                }
            }

            foreach (var skladiste in SyncDataGet.Skladista)
            {
                try
                {
                    db.Execute(
                        "INSERT INTO T_SKL  (" +
                            "SKL_SIFRA , " +
                            "SKL_NAZIV) " +
                        "VALUES (?, ?)",
                            skladiste.SKL_SIFRA,
                            skladiste.SKL_NAZIV);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE T_SKL " +
                        "SET SKL_NAZIV = ? " +
                        "WHERE SKL_SIFRA = ?",
                            skladiste.SKL_NAZIV,
                            skladiste.SKL_SIFRA);
                }
            }

            foreach (var materijal in SyncDataGet.Materijali)
            {
                try
                {
                    db.Execute(
                        "INSERT INTO T_NAZR  (" +
                            "NAZR_SIFRA , " +
                            "NAZR_NAZIV, " +
                            "NAZR_BARKOD, " +
                            "NAZR_CIJENA_ART, " +
                            "NAZR_GRUPA, " +
                            "NAZR_JM_SIFRA, " +
                            "TipKemikalije) " +
                        "VALUES (?, ?, ?, ?, ?, ?, ?)",
                            materijal.NAZR_SIFRA,
                            materijal.NAZR_NAZIV,
                            materijal.NAZR_BARKOD,
                            materijal.NAZR_CIJENA_ART,
                            materijal.NAZR_GRUPA,
                            materijal.NAZR_JM_SIFRA,
                            materijal.TipKemikalije);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE T_NAZR " +
                        "SET NAZR_NAZIV = ?, " +
                            "NAZR_SIFRA = ?, " +
                            "NAZR_BARKOD = ?, " +
                            "NAZR_CIJENA_ART = ?, " +
                            "NAZR_GRUPA = ?, " +
                            "NAZR_JM_SIFRA = ?, " +
                            "TipKemikalije = ?, " +
                            "SinhronizacijaPrivremeniKljuc = ? " +
                        "WHERE NAZR_SIFRA = ?",
                            materijal.NAZR_NAZIV,
                            materijal.NAZR_SIFRA,
                            materijal.NAZR_BARKOD,
                            materijal.NAZR_CIJENA_ART,
                            materijal.NAZR_GRUPA,
                            materijal.NAZR_JM_SIFRA,
                            materijal.TipKemikalije,
                            materijal.SinhronizacijaPrivremeniKljuc,
                            materijal.NAZR_SIFRA);
                }
            }

            foreach (var grupa in SyncDataGet.MaterijaliGrupe)
            {
                try
                {
                    db.Execute(
                        "INSERT INTO T_GRU (GRU_SIFRA, GRU_NAZIV) " +
                        "VALUES (?, ?)",
                            grupa.GRU_SIFRA,
                            grupa.GRU_NAZIV);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE T_GRU " +
                        "SET GRU_NAZIV = ? " +
                        "WHERE GRU_SIFRA = ?",
                            grupa.GRU_NAZIV,
                            grupa.GRU_SIFRA);
                }
            }

            foreach (var mjernaJedinica in SyncDataGet.MjerneJedinice)
            {
                try
                {
                    db.Execute(
                        "INSERT INTO T_MjerneJedinice (" +
                            "Id, " +
                            "Naziv, " +
                            "Oznaka, " +
                            "VecaJedinicaId, " +
                            "GlavnaJedinica, " +
                            "KoeficijentPretvorbe, " +
                            "Tip) " +
                        "VALUES (?, ?, ?, ?, ?, ?, ?)",
                            mjernaJedinica.Id,
                            mjernaJedinica.Naziv,
                            mjernaJedinica.Oznaka,
                            mjernaJedinica.VecaJedinicaId,
                            mjernaJedinica.GlavnaJedinica,
                            mjernaJedinica.KoeficijentPretvorbe,
                            mjernaJedinica.Tip);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE T_MjerneJedinice " +
                        "SET Naziv = ?, " +
                            "Oznaka = ?, " +
                            "VecaJedinicaId = ?, " +
                            "GlavnaJedinica = ?, " +
                            "KoeficijentPretvorbe = ?, " +
                            "Tip = ? " +
                        "WHERE Id = ?",
                            mjernaJedinica.Naziv,
                            mjernaJedinica.Oznaka,
                            mjernaJedinica.VecaJedinicaId,
                            mjernaJedinica.GlavnaJedinica,
                            mjernaJedinica.KoeficijentPretvorbe,
                            mjernaJedinica.Tip,
                            mjernaJedinica.Id);
                }
            }

            foreach (var tipLokacije in SyncDataGet.TipoviLokacije)
            {
                try
                {
                    db.Execute(
                        "INSERT INTO DID_TipLokacije (" +
                            "Sifra, " +
                            "Naziv)" +
                        "VALUES (?, ?)",
                                tipLokacije.Sifra,
                                tipLokacije.Naziv);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_TipLokacije " +
                        "SET Naziv = ? " +
                        "WHERE Sifra = ?",
                            tipLokacije.Naziv,
                            tipLokacije.Sifra);
                }
            }

            foreach (var tipPosla in SyncDataGet.TipoviPosla)
            {
                try
                {
                    db.Execute(
                       "INSERT INTO DID_TipPosla (" +
                            "Sifra, " +
                            "Naziv, " +
                            "Djelatnost) " +
                        "VALUES (?, ?, ?)",
                            tipPosla.Sifra,
                            tipPosla.Naziv,
                            tipPosla.Djelatnost);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_TipPosla " +
                        "SET Naziv = ?, " +
                            "Djelatnost = ? " +
                        "WHERE Sifra = ?",
                            tipPosla.Naziv,
                            tipPosla.Djelatnost,
                            tipPosla.Sifra);
                }
            }

            foreach (var tipKutije in SyncDataGet.TipoviDeratizacijskeKutije)
            {
                try
                {
                    db.Execute(
                       "INSERT INTO DID_TipDeratizacijskeKutije (" +
                            "Sifra, " +
                            "Naziv) " +
                        "VALUES (?, ?)",
                            tipKutije.Sifra,
                            tipKutije.Naziv);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_TipDeratizacijskeKutije " +
                        "SET Naziv = ? " +
                        "WHERE Sifra = ?",
                            tipKutije.Naziv,
                            tipKutije.Sifra);
                }
            }

            foreach (var razinaInfestacije in SyncDataGet.RazineInfestacije)
            {
                try
                {
                    db.Execute(
                       "INSERT INTO DID_RazinaInfestacije (" +
                            "Sifra, " +
                            "Naziv) " +
                        "VALUES (?, ?)",
                            razinaInfestacije.Sifra,
                            razinaInfestacije.Naziv);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_RazinaInfestacije " +
                        "SET Naziv = ? " +
                        "WHERE Sifra = ?",
                            razinaInfestacije.Naziv,
                            razinaInfestacije.Sifra);
                }
            }

            foreach (var razlogNeizvrsenja in SyncDataGet.RazloziNeizvrsenjaDeratizacije)
            {
                try
                {
                    db.Execute(
                       "INSERT INTO DID_RazlogNeizvrsenjaDeratizacije (" +
                            "Sifra, " +
                            "Naziv) " +
                        "VALUES (?, ?)",
                            razlogNeizvrsenja.Sifra,
                            razlogNeizvrsenja.Naziv);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_RazlogNeizvrsenjaDeratizacije " +
                        "SET Naziv = ? " +
                        "WHERE Sifra = ?",
                            razlogNeizvrsenja.Naziv,
                            razlogNeizvrsenja.Sifra);
                }
            }

            foreach (var zupanija in SyncDataGet.Zupanije)
            {
                try
                {
                    db.Execute(
                       "INSERT INTO T_ZUPANIJE (" +
                            "Sifra, " +
                            "Naziv) " +
                        "VALUES (?, ?)",
                            zupanija.Sifra,
                            zupanija.Naziv);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE T_ZUPANIJE " +
                        "SET Naziv = ? " +
                        "WHERE Sifra = ?",
                            zupanija.Naziv,
                            zupanija.Sifra);
                }
            }

            foreach (var opcina in SyncDataGet.Opcine)
            {
                try
                {
                    db.Execute(
                       "INSERT INTO T_OPCINE (" +
                            "Sifra, " +
                            "Naziv, " +
                            "Zupanija) " +
                        "VALUES (?, ?, ?)",
                            opcina.Sifra,
                            opcina.Naziv,
                            opcina.Zupanija);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE T_OPCINE " +
                        "SET Naziv = ?, " +
                            "Zupanija = ?  " +
                        "WHERE Sifra = ?",
                            opcina.Naziv,
                            opcina.Zupanija,
                            opcina.Sifra);
                }
            }

            foreach (var naselje in SyncDataGet.Naselja)
            {
                try
                {
                    db.Execute(
                       "INSERT INTO T_NASELJA (" +
                            "Sifra, " +
                            "Naziv, " +
                            "Opcina, " +
                            "Posta, " +
                            "Tip) " +
                        "VALUES (?, ?, ?, ?, ?)",
                            naselje.Sifra,
                            naselje.Naziv,
                            naselje.Opcina,
                            naselje.Posta,
                            naselje.Tip);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE T_NASELJA " +
                        "SET Naziv = ?, " +
                            "Opcina = ?, " +
                            "Posta = ?, " +
                            "Tip = ? " +
                        "WHERE Sifra = ?",
                            naselje.Naziv,
                            naselje.Opcina,
                            naselje.Posta,
                            naselje.Tip,
                            naselje.Sifra);
                }
            }

            foreach (var statusRadnogNaloga in SyncDataGet.StatusRadnogNaloga)
            {
                try
                {
                    db.Execute(
                       "INSERT INTO DID_StatusRadnogNaloga (" +
                            "Sifra, " +
                            "Naziv)" +
                        "VALUES (?, ?)",
                            statusRadnogNaloga.Sifra,
                            statusRadnogNaloga.Naziv);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_StatusRadnogNaloga " +
                        "SET Naziv = ? " +
                        "WHERE Sifra = ?",
                            statusRadnogNaloga.Sifra,
                            statusRadnogNaloga.Naziv);
                }
            }

            foreach (var statusLokacije in SyncDataGet.StatusLokacije)
            {
                try
                {
                    db.Execute(
                       "INSERT INTO DID_StatusLokacije (" +
                            "Sifra, " +
                            "Naziv)" +
                        "VALUES (?, ?)",
                            statusLokacije.Sifra,
                            statusLokacije.Naziv);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_StatusLokacije " +
                        "SET Naziv = ? " +
                        "WHERE Sifra = ?",
                            statusLokacije.Sifra,
                            statusLokacije.Naziv);
                }
            }

            foreach (var statusLokacije_RadniNalog in SyncDataGet.StatusLokacijeRadnogNaloga)
            {
                try
                {
                    db.Execute(
                       "INSERT INTO DID_StatusLokacije_RadniNalog (" +
                            "Sifra, " +
                            "Naziv)" +
                        "VALUES (?, ?)",
                            statusLokacije_RadniNalog.Sifra,
                            statusLokacije_RadniNalog.Naziv);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_StatusLokacije_RadniNalog " +
                        "SET Naziv = ? " +
                        "WHERE Sifra = ?",
                            statusLokacije_RadniNalog.Sifra,
                            statusLokacije_RadniNalog.Naziv);
                }
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
                    string requestUrl = "http://services.infobit.info/DDD_Test/SyncService.svc/get/maticni";
                    //string requestUrl = "http://services.infobit.info/DDD/SyncService.svc/get/maticni";
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

                if (SyncDataSend.Djelatnici.Any() || SyncDataGet.Djelatnici.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Djelatnici", SyncDataSend.Djelatnici.Count(), SyncDataGet.Djelatnici.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.DjelatniciUsername.Any() || SyncDataGet.DjelatniciUsername.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Vjerodajnice", SyncDataSend.DjelatniciUsername.Count(), SyncDataGet.DjelatniciUsername.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.LokacijePozicije.Any() || SyncDataGet.LokacijePozicije.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Pozicije", SyncDataSend.LokacijePozicije.Count(), SyncDataGet.LokacijePozicije.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.Lokacije.Any() || SyncDataGet.Lokacije.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Lokacije", SyncDataSend.Lokacije.Count(), SyncDataGet.Lokacije.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.Komitenti.Any() || SyncDataGet.Komitenti.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Komitenti", SyncDataSend.Komitenti.Count(), SyncDataGet.Komitenti.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.Djelatnosti.Any() || SyncDataGet.Djelatnosti.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Djelatnost", SyncDataSend.Djelatnosti.Count(), SyncDataGet.Djelatnosti.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.Nametnici.Any() || SyncDataGet.Nametnici.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Nametnici", SyncDataSend.Nametnici.Count(), SyncDataGet.Nametnici.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.Skladista.Any() || SyncDataGet.Skladista.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Skladista", SyncDataSend.Skladista.Count(), SyncDataGet.Skladista.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.Materijali.Any() || SyncDataGet.Materijali.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Materijali", SyncDataSend.Materijali.Count, SyncDataGet.Materijali.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.MaterijaliGrupe.Any() || SyncDataGet.MaterijaliGrupe.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Grupe materijala", SyncDataSend.MaterijaliGrupe.Count, SyncDataGet.MaterijaliGrupe.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.MjerneJedinice.Any() || SyncDataGet.MjerneJedinice.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Mjerne jedinice", SyncDataSend.MjerneJedinice.Count, SyncDataGet.MjerneJedinice.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.TipoviLokacije.Any() || SyncDataGet.TipoviLokacije.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Tipovi lokacije", SyncDataSend.TipoviLokacije.Count(), SyncDataGet.TipoviLokacije.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.TipoviPosla.Any() || SyncDataGet.TipoviPosla.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Tipovi posla", SyncDataSend.TipoviPosla.Count(), SyncDataGet.TipoviPosla.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.TipoviDeratizacijskeKutije.Any() || SyncDataGet.TipoviDeratizacijskeKutije.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Tipovi deratizacijske kutije", SyncDataSend.TipoviDeratizacijskeKutije.Count(), SyncDataGet.TipoviDeratizacijskeKutije.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.RazineInfestacije.Any() || SyncDataGet.RazineInfestacije.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Razine infestacije", SyncDataSend.RazineInfestacije.Count(), SyncDataGet.RazineInfestacije.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.RazloziNeizvrsenjaDeratizacije.Any() || SyncDataGet.RazloziNeizvrsenjaDeratizacije.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Razlozi neizvršenja deratizacije", SyncDataSend.RazloziNeizvrsenjaDeratizacije.Count(), SyncDataGet.RazloziNeizvrsenjaDeratizacije.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.Zupanije.Any() || SyncDataGet.Zupanije.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Županije", SyncDataSend.Zupanije.Count(), SyncDataGet.Zupanije.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.Opcine.Any() || SyncDataGet.Opcine.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Općine", SyncDataSend.Opcine.Count(), SyncDataGet.Opcine.Count);
                    ispis.Add(DDD);
                    dataFlag = true;
                }
                if (SyncDataSend.Naselja.Any() || SyncDataGet.Naselja.Any())
                {
                    Adapter_SyncData DDD = new Adapter_SyncData("Naselja", SyncDataSend.Naselja.Count(), SyncDataGet.Naselja.Count);
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

//String output = "";
//using (HttpClient client = new HttpClient())
//{
//    client.Timeout = new TimeSpan(0, 0, 180);
//    string requestUrl = "https://services.infobit.info/DDD/SyncService.svc/sync/all";
//    var request = new StringContent(JsonConvert.ToString(JSON), Encoding.UTF8, "application/json");
//    var response = client.PostAsync(requestUrl, request).Result;
//    if (response.IsSuccessStatusCode)
//    {
//        var stream = response.Content;
//        var data = stream.ReadAsStringAsync();
//        var json = data.Result;
//        string tempJson = JsonConvert.DeserializeObject<string>(json);
//        var syncData = JsonConvert.DeserializeObject<DeratizacijaSyncData>(tempJson);
//        if (!String.IsNullOrEmpty(syncData.Error))
//            output = "Došlo je do pogreške prilikom sinhronizacije:\n" + SyncDataGet.Error;
//    }
//    else
//        output = "Došlo je do neočekivane pogreške prilikom poziva sinhronizacije!";
//}
//return output;







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