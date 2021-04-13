using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Infobit.DDD.Data;
using SQLite;
using Android.Content.PM;

namespace App4New
{
    [Activity(Label = "Activity_OdradeneAnkete", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_OdradeneAnkete : Activity
    {

        Spinner spinnerLokacija, spinnerPartner, spinnerRadniNalog;
        RecyclerView mRecycleView;
        RecyclerView.LayoutManager mLayoutManager;
        Adapter_AnketeRecycleView mAdapter;
        Intent intent;
        LinearLayout prikazAnketa;
        TextView message, messagePotvrda;

        List<DID_Anketa> filtriraneAnkete;
        List<string> partneriSifreList, lokacijeIdList, radniNaloziIdList;
        List<DID_RadniNalog> radniNalozi;
        List<DID_Potvrda> potvrda;
        int lokacijaId, potvrdaId, radniNalogId;
        string sifraPartnera;
        bool flagLokacijaPotvrda;

        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);
        static ISharedPreferencesEditor radniNaloziEdit = localRadniNalozi.Edit();
        static readonly ISharedPreferences localMaterijali = Application.Context.GetSharedPreferences("localMaterijali", FileCreationMode.Private);
        static ISharedPreferencesEditor localMaterijaliEdit = localMaterijali.Edit();
        static readonly ISharedPreferences localKomitentLokacija = Application.Context.GetSharedPreferences("localKomitentLokacija", FileCreationMode.Private);
        static ISharedPreferencesEditor localKomitentLokacijaEdit = localKomitentLokacija.Edit();
        static readonly ISharedPreferences localOdradeneAnkete = Application.Context.GetSharedPreferences("ankete", FileCreationMode.Private);
        static ISharedPreferencesEditor localOdradeneAnketeEdit = localOdradeneAnkete.Edit();
        static readonly ISharedPreferences localPozicija = Application.Context.GetSharedPreferences("pozicija", FileCreationMode.Private);
        static ISharedPreferencesEditor localPozicijaEdit = localPozicija.Edit();
        static readonly ISharedPreferences localAnketa = Application.Context.GetSharedPreferences("anketa", FileCreationMode.Private);
        static ISharedPreferencesEditor localAnketaEdit = localAnketa.Edit();

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.odradeneAnkete);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            spinnerPartner = FindViewById<Spinner>(Resource.Id.spinnerPartner);
            spinnerLokacija = FindViewById<Spinner>(Resource.Id.spinnerLokacija);
            spinnerRadniNalog = FindViewById<Spinner>(Resource.Id.spinnerRadniNalog);
            mRecycleView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            message = FindViewById<TextView>(Resource.Id.message);
            messagePotvrda = FindViewById<TextView>(Resource.Id.messagePotvrda);
            prikazAnketa = FindViewById<LinearLayout>(Resource.Id.prikazAnketa);

            SetActionBar(toolbar);
            ActionBar.Title = "Odrađene ankete";
            spinnerPartner.ItemSelected += SpinnerPartner_ItemSelected;
            spinnerLokacija.ItemSelected += SpinnerLokacija_ItemSelected;
            spinnerRadniNalog.ItemSelected += SpinnerRadniNalog_ItemSelected;
            mLayoutManager = new LinearLayoutManager(this);
            mRecycleView.SetLayoutManager(mLayoutManager);
            radniNalozi = db.Query<DID_RadniNalog>(
                "SELECT rn.Id, rn.Godina, rn.Broj, rn.Status, rn.PokretnoSkladiste, rn.Voditelj, rn.Izdavatelj, rn.Primatelj, rn.DatumOd, rn.DatumDo, rn.DatumIzrade, rn.DatumIzvrsenja, rn.SinhronizacijaDatumVrijeme, rn.SinhronizacijaPrivremeniKljuc " +
                "FROM DID_RadniNalog rn " +
                "INNER JOIN DID_RadniNalog_Lokacija ON rn.Id = DID_RadniNalog_Lokacija.RadniNalog " +
                "INNER JOIN DID_LokacijaPozicija ON DID_RadniNalog_Lokacija.Lokacija = DID_LokacijaPozicija.SAN_Id " +
                "INNER JOIN DID_Anketa ON DID_LokacijaPozicija.POZ_Id = DID_Anketa.ANK_POZ_Id " +
                "GROUP BY Broj, Godina");

            if (radniNalozi.Any())
            {
                message.Visibility = Android.Views.ViewStates.Gone;
                prikazAnketa.Visibility = Android.Views.ViewStates.Visible;
                List<string> listRadniNalozi = new List<string>();
                radniNaloziIdList = new List<string>();
                foreach (var item in radniNalozi)
                {
                    listRadniNalozi.Add("Broj: " + item.Broj.ToString() + " / Godina: " + item.Godina);
                    radniNaloziIdList.Add(item.Id.ToString());
                }
                ArrayAdapter<string> adapterRNList = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, listRadniNalozi);
                adapterRNList.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                spinnerRadniNalog.Adapter = adapterRNList;

                // ako je vec gledao podatke za neki radni nalog zapamti ga i vrati ga na taj radni nalog, ako nije

                // plan je da uvijek pocetno pokazuje zadnji dodani radni nalog
                //var zadnjiRadniNalog = radniNaloziIdList.LastOrDefault();
                //spinnerRadniNalog.SetSelection(radniNaloziIdList.IndexOf(sifraUlogiranogDjelatnika.ToString()));

            }
            else
            {
                message.Visibility = Android.Views.ViewStates.Visible;
                prikazAnketa.Visibility = Android.Views.ViewStates.Gone;
            }
        }

        public override void OnBackPressed()
        {
            intent = new Intent(this, typeof(Activity_Pocetna));
            StartActivity(intent);
        }

        public void UpdateStatusRadniNalog()
        {
            List<DID_RadniNalog_Lokacija> sveLokacije = db.Query<DID_RadniNalog_Lokacija>(
                "SELECT * " +
                "FROM DID_RadniNalog_Lokacija " +
                "WHERE RadniNalog = ?", radniNalogId);

            List<DID_RadniNalog_Lokacija> izvrseneLokacije = db.Query<DID_RadniNalog_Lokacija>(
                "SELECT * " +
                "FROM DID_RadniNalog_Lokacija " +
                "WHERE RadniNalog = ? " +
                "AND Status = 3", radniNalogId);

            if (sveLokacije.Count == izvrseneLokacije.Count)
            {
                db.Query<DID_RadniNalog>(
                    "UPDATE DID_RadniNalog " +
                    "SET Status = ?, DatumIzvrsenja = ? " +
                    "WHERE Id = ?", 5, DateTime.Now, radniNalogId);
            }
            else if (izvrseneLokacije.Any())
            {
                db.Query<DID_RadniNalog>(
                    "UPDATE DID_RadniNalog " +
                    "SET Status = ? " +
                    "WHERE Id = ?", 4, radniNalogId);
            }
            else
            {
                db.Query<DID_RadniNalog>(
                    "UPDATE DID_RadniNalog " +
                    "SET Status = ? " +
                    "WHERE Id = ?", 3, radniNalogId);
            }
        }

        public void DeleteMaterijaleNaAnketama(int pozicijaId)
        {
            List<DID_AnketaMaterijali> materijaliNaAnketi = db.Query<DID_AnketaMaterijali>(
                "SELECT * " +
                "FROM DID_AnketaMaterijali " +
                "WHERE RadniNalog = ? " +
                "AND PozicijaId = ?", radniNalogId, pozicijaId);

            var skladiste = db.Query<DID_RadniNalog>(
                "SELECT * " +
                "FROM DID_RadniNalog " +
                "WHERE Id = ?", radniNalogId).FirstOrDefault();

            foreach (var materijal in materijaliNaAnketi)
            {
                var staraKolicinaNaSkladistu = db.Query<DID_StanjeSkladista>(
                    "SELECT * " +
                    "FROM DID_StanjeSkladista " +
                    "WHERE Skladiste = ? " +
                    "AND Materijal = ?", skladiste.PokretnoSkladiste, materijal.MaterijalSifra).FirstOrDefault().Kolicina;

                // UPDATE Kolicine na skladistu
                decimal novaKolicinaNaSkladistu = Convert.ToDecimal(staraKolicinaNaSkladistu.ToString("F3")) + Convert.ToDecimal(materijal.Kolicina);
                db.Execute(
                    "UPDATE DID_StanjeSkladista " +
                    "SET Kolicina = ?, " +
                        "StaraKolicina = ? " +
                    "WHERE Skladiste = ? " +
                    "AND Materijal = ?",
                        novaKolicinaNaSkladistu,
                        novaKolicinaNaSkladistu,
                        skladiste.PokretnoSkladiste,
                        materijal.MaterijalSifra);
            }

            db.Execute(
                "DELETE FROM DID_AnketaMaterijali " +
                "WHERE RadniNalog = ? " +
                "AND PozicijaId = ?", radniNalogId, pozicijaId);
        }

        private void MAdapter_ItemDelete(object sender, int e)
        {
            if (flagLokacijaPotvrda && filtriraneAnkete.Count == 1)
            {
                string nazivLokacije = db.Query<DID_Lokacija>(
                    "SELECT * " +
                    "FROM DID_Lokacija " +
                    "WHERE SAN_Id = ?", lokacijaId).FirstOrDefault().SAN_Naziv;

                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Upozorenje!");
                alert.SetMessage("Lokacija: " + nazivLokacije + " je zaključana. Ova anketa je jedina na lokaciji te ukoliko obrišete anketu automatski će se obrisati potvrda. Jeste li sigurni da želite obrisati anketu?");
                alert.SetPositiveButton("OBRIŠI", (senderAlert, arg) =>
                {
                    localOdradeneAnketeEdit.PutBoolean("visited", true);
                    localOdradeneAnketeEdit.Commit();
                    localPozicijaEdit.PutString("sifraPartnera", sifraPartnera);
                    localPozicijaEdit.Commit();
                    Guid anketaId = filtriraneAnkete[e].Id;
                    int pozicijaId = filtriraneAnkete[e].ANK_POZ_Id;
                    if (pozicijaId < 0)
                        db.Delete<DID_LokacijaPozicija>(pozicijaId);


                    DeleteMaterijaleNaAnketama(pozicijaId);


                    db.Delete<DID_Anketa>(anketaId);
                    //db.Execute(
                    //    "DELETE FROM DID_AnketaMaterijali " +
                    //    "WHERE RadniNalog = ? " +
                    //    "AND PozicijaId = ?", radniNalogId, pozicijaId);
                    db.Execute(
                       "DELETE FROM DID_Potvrda_Materijal " +
                       "WHERE Potvrda = ?", potvrda.FirstOrDefault().Id);
                    db.Query<DID_Potvrda_Djelatnost>(
                        "DELETE FROM DID_Potvrda_Djelatnost " +
                        "WHERE Potvrda = ?", potvrda.FirstOrDefault().Id);
                    db.Query<DID_Potvrda_Nametnik>(
                       "DELETE FROM DID_Potvrda_Nametnik " +
                       "WHERE Potvrda = ?", potvrda.FirstOrDefault().Id);
                    db.Delete<DID_Potvrda>(potvrdaId);

                    //Update lokacije
                    db.Query<DID_RadniNalog_Lokacija>(
                        "UPDATE DID_RadniNalog_Lokacija " +
                        "SET Status = 2 " +
                        "WHERE Lokacija = ? " +
                        "AND RadniNalog = ?", lokacijaId, radniNalogId);

                    UpdateStatusRadniNalog();

                    intent = new Intent(this, typeof(Activity_OdradeneAnkete));
                    StartActivity(intent);
                });

                alert.SetNegativeButton("ODUSTANI", (senderAlert, arg) => { });
                Dialog dialog = alert.Create();
                dialog.Show();
            }
            else if (flagLokacijaPotvrda && filtriraneAnkete.Count > 1)
            {
                string nazivLokacije = db.Query<DID_Lokacija>(
                    "SELECT * " +
                    "FROM DID_Lokacija " +
                    "WHERE SAN_Id = ?", lokacijaId).FirstOrDefault().SAN_Naziv;

                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Upozorenje!");
                alert.SetMessage("Lokacija: " + nazivLokacije + " je zaključana. Ako obrišete anketu obrisati će se i materijali vezani za odabranu poziciju. Jeste li sigurni da želite obrisati anketu?");
                alert.SetPositiveButton("OBRIŠI", (senderAlert, arg) =>
                {
                    localOdradeneAnketeEdit.PutBoolean("visited", true);
                    localOdradeneAnketeEdit.Commit();
                    localPozicijaEdit.PutString("sifraPartnera", sifraPartnera);
                    localPozicijaEdit.Commit();
                    Guid anketaId = filtriraneAnkete[e].Id;
                    int pozicijaId = filtriraneAnkete[e].ANK_POZ_Id;

                    if (pozicijaId < 0)
                        db.Delete<DID_LokacijaPozicija>(pozicijaId);

                    DeleteMaterijaleNaAnketama(pozicijaId);

                    db.Delete<DID_Anketa>(anketaId);
                    //db.Execute(
                    //    "DELETE FROM DID_AnketaMaterijali " +
                    //    "WHERE RadniNalog = ? " +
                    //    "AND PozicijaId = ?", radniNalogId, pozicijaId);

                    db.Execute(
                       "DELETE FROM DID_Potvrda_Materijal " +
                       "WHERE Potvrda = ?", potvrda.FirstOrDefault().Id);

                    db.Execute(
                        "INSERT INTO DID_Potvrda_Materijal (Potvrda, Materijal, Utroseno, MaterijalNaziv) " +
                        "SELECT pot.Id, mat.MaterijalSifra, TOTAL(mat.Kolicina), mat.MaterijalNaziv " +
                        "FROM DID_AnketaMaterijali mat " +
                        "INNER JOIN DID_LokacijaPozicija poz ON poz.POZ_Id = mat.PozicijaId " +
                        "INNER JOIN DID_Potvrda pot ON pot.RadniNalog = mat.RadniNalog " +
                        "AND pot.Lokacija = poz.SAN_Id " +
                        "WHERE pot.Id = ? " +
                        "GROUP BY mat.MaterijalSifra, mat.MjernaJedinica", potvrda.FirstOrDefault().Id);

                    var listaMaterijalaPotvrda = db.Query<DID_Potvrda_Materijal>("SELECT * FROM DID_Potvrda_Materijal WHERE Potvrda = ?", potvrda.FirstOrDefault().Id);

                    foreach (var materijal in listaMaterijalaPotvrda)
                        db.Execute(
                            "UPDATE DID_Potvrda_Materijal " +
                            "SET SinhronizacijaPrivremeniKljuc = ? " +
                            "WHERE Id = ?", materijal.Id, materijal.Id);

                    intent = new Intent(this, typeof(Activity_OdradeneAnkete));
                    StartActivity(intent);
                });

                alert.SetNegativeButton("ODUSTANI", (senderAlert, arg) => { });
                Dialog dialog = alert.Create();
                dialog.Show();
            }
            else
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Upozorenje!");
                alert.SetMessage("Brisanjem ankete obrisati će se i materijali vezani uz ovu poziciju. Jeste li sigurni da želite obrisati anketu?");
                alert.SetPositiveButton("OBRIŠI", (senderAlert, arg) =>
                {
                    localOdradeneAnketeEdit.PutBoolean("visited", true);
                    localOdradeneAnketeEdit.Commit();
                    localPozicijaEdit.PutString("sifraPartnera", sifraPartnera);
                    localPozicijaEdit.Commit();
                    Guid anketaId = filtriraneAnkete[e].Id;
                    int pozicijaId = filtriraneAnkete[e].ANK_POZ_Id;

                    if (pozicijaId < 0)
                        db.Delete<DID_LokacijaPozicija>(pozicijaId);

                    db.Delete<DID_Anketa>(anketaId);

                    DeleteMaterijaleNaAnketama(pozicijaId);

                    //db.Execute(
                    //    "DELETE FROM DID_AnketaMaterijali " +
                    //    "WHERE RadniNalog = ? " +
                    //    "AND PozicijaId = ?", radniNalogId, pozicijaId);

                    intent = new Intent(this, typeof(Activity_OdradeneAnkete));
                    StartActivity(intent);
                });

                alert.SetNegativeButton("ODUSTANI", (senderAlert, arg) => { });
                Dialog dialog = alert.Create();
                dialog.Show();
            }
        }

        private void MAdapter_ItemEdit(object sender, int e)
        {
            radniNaloziEdit.PutInt("id", filtriraneAnkete[e].ANK_RadniNalog);
            radniNaloziEdit.Commit();
            localOdradeneAnketeEdit.PutBoolean("visited", true);
            localPozicijaEdit.PutString("sifraPartnera", sifraPartnera);
            localMaterijaliEdit.PutString("sifraPartnera", sifraPartnera);
            localMaterijaliEdit.PutInt("pozicijaId", filtriraneAnkete[e].ANK_POZ_Id);
            //localMaterijaliEdit.PutInt("sifra", );
            localMaterijaliEdit.PutInt("lokacijaId", lokacijaId);
            localMaterijaliEdit.PutBoolean("visitedAnkete", true);
            //-----dodavanje potrosenog materijala tokom pregleda na odradenim anketama
            localPozicijaEdit.PutInt("pozicijaId", filtriraneAnkete[e].ANK_POZ_Id);
            localKomitentLokacijaEdit.PutInt("lokacijaId", lokacijaId);
            localKomitentLokacijaEdit.Commit();
            //-----dodavanje potrosenog materijala tokom pregleda na odradenim anketama
            localOdradeneAnketeEdit.Commit();
            localMaterijaliEdit.Commit();
            localPozicijaEdit.Commit();
            intent = new Intent(this, typeof(Activity_PotroseniMaterijali_Pozicija));
            StartActivity(intent);
        }

        private void MAdapter_ItemClick(object sender, int e)
        {
            localPozicija.Edit().Clear().Commit();
            localAnketa.Edit().Clear().Commit();
            localOdradeneAnketeEdit.PutBoolean("visited", true);
            localOdradeneAnketeEdit.Commit();

            var anketaId = filtriraneAnkete[e].Id;
            var anketa = db.Query<DID_Anketa>(
                "SELECT * " +
                "FROM DID_Anketa " +
                "WHERE Id = ?", anketaId).FirstOrDefault();
            int tipKutije = db.Query<DID_LokacijaPozicija>(
                "SELECT * " +
                "FROM DID_LokacijaPozicija " +
                "WHERE POZ_Id = ?", anketa.ANK_POZ_Id).FirstOrDefault().POZ_Tip;

            if (anketa.ANK_RazlogNeizvrsenja != 0)
            {
                string razlogNeizvrsenja = db.Query<DID_RazlogNeizvrsenjaDeratizacije>(
                    "SELECT * " +
                    "FROM DID_RazlogNeizvrsenjaDeratizacije " +
                    "WHERE Sifra = ?", anketa.ANK_RazlogNeizvrsenja).FirstOrDefault().Naziv;
                localAnketaEdit.PutString("spinnerSelectedItem", razlogNeizvrsenja);
                localAnketaEdit.PutBoolean("neprovedenoRadioBtn", true);
            }
            else
            {
                localAnketaEdit.PutString("spinnerSelectedItem", null);
                localAnketaEdit.PutBoolean("neprovedenoRadioBtn", false);
            }

            localPozicijaEdit.PutString("anketaId", anketaId.ToString());
            localPozicijaEdit.PutInt("pozicijaId", anketa.ANK_POZ_Id);
            localPozicijaEdit.PutInt("lokacijaId", lokacijaId);
            localPozicijaEdit.PutString("sifraPartnera", sifraPartnera);
            localPozicijaEdit.PutInt("tipKutije", tipKutije);
            localPozicijaEdit.Commit();

            localAnketaEdit.PutString("napomeneInput", anketa.ANK_Napomena);
            localAnketaEdit.PutBoolean("visitedPage1", true);
            localAnketaEdit.PutBoolean("visitedPage2", true);
            localAnketaEdit.PutBoolean("visitedPage3", true);
            localAnketaEdit.PutBoolean("visitedMaterijali", true);
            localAnketaEdit.PutBoolean("edit", true);
            localAnketaEdit.Commit();

            if (anketa.ANK_TipDeratizacije == 1)
            {
                if (anketa.ANK_TipDeratizacijskeKutijeSifra == 2)
                {
                    localAnketaEdit.PutBoolean("ANK_Kutija_Ljep", true);
                    localAnketaEdit.PutBoolean("kutijaUrednaLjep", anketa.ANK_Utvrdjeno_Ljep_KutijaUredna);
                    localAnketaEdit.PutBoolean("kutijaOstecenaLjep", anketa.ANK_Utvrdjeno_Ljep_KutijaOstecena);
                    localAnketaEdit.PutBoolean("nemaKutijeLjep", anketa.ANK_Utvrdjeno_Ljep_NemaKutije);
                    localAnketaEdit.PutBoolean("ulovljenStakorLjep", anketa.ANK_Utvrdjeno_Ljep_UlovljenStakor);
                    localAnketaEdit.PutBoolean("ulovljenMisLjep", anketa.ANK_Utvrdjeno_Ljep_UlovljenMis);
                    localAnketaEdit.PutBoolean("novaLjepPodUcinjenoLjep", anketa.ANK_Ucinjeno_Ljep_NovaLjepljivaPodloska);
                    localAnketaEdit.PutBoolean("novaKutijaUcinjenoLjep", anketa.ANK_Ucinjeno_Ljep_NovaKutija);
                }
                else if (anketa.ANK_TipDeratizacijskeKutijeSifra == 1)
                {
                    localAnketaEdit.PutBoolean("ANK_Kutija_Rot", true);
                    localAnketaEdit.PutBoolean("kutijaUrednaRot", anketa.ANK_Utvrdjeno_Rot_KutijaUredna);
                    localAnketaEdit.PutBoolean("kutijaOstecenaRot", anketa.ANK_Utvrdjeno_Rot_KutijaOstecena);
                    localAnketaEdit.PutBoolean("nemaKutijeRot", anketa.ANK_Utvrdjeno_Rot_NemaKutije);
                    localAnketaEdit.PutBoolean("tragoviGlodanjaRot", anketa.ANK_Utvrdjeno_Rot_TragoviGlodanja);
                    localAnketaEdit.PutBoolean("izmetRot", anketa.ANK_Utvrdjeno_Rot_Izmet);
                    localAnketaEdit.PutBoolean("pojedenaMekaRot", anketa.ANK_Utvrdjeno_Rot_PojedenaMeka);
                    localAnketaEdit.PutBoolean("ostecenaMekaRot", anketa.ANK_Utvrdjeno_Rot_OstecenaMeka);
                    localAnketaEdit.PutBoolean("uginuliStakorRot", anketa.ANK_Utvrdjeno_Rot_UginuliStakor);
                    localAnketaEdit.PutBoolean("uginuliMisRot", anketa.ANK_Utvrdjeno_Rot_UginuliMis);
                    localAnketaEdit.PutBoolean("noviMamciUcinjenoRot", anketa.ANK_Ucinjeno_Rot_PostavljeniMamci);
                    localAnketaEdit.PutBoolean("novaKutijaUcinjenoRot", anketa.ANK_Ucinjeno_Rot_NovaKutija);
                    localAnketaEdit.PutBoolean("nadopunjenaMekomUcinjenoRot", anketa.ANK_Ucinjeno_Rot_NadopunjenaMekom);
                }
                else if (anketa.ANK_TipDeratizacijskeKutijeSifra == 3)
                {
                    localAnketaEdit.PutBoolean("ANK_Kutija_Ziv", true);
                    localAnketaEdit.PutBoolean("kutijaUrednaZiv", anketa.ANK_Utvrdjeno_Ziv_KutijaUredna);
                    localAnketaEdit.PutBoolean("kutijaOstecenaZiv", anketa.ANK_Utvrdjeno_Ziv_KutijaOstecena);
                    localAnketaEdit.PutBoolean("nemaKutijeZiv", anketa.ANK_Utvrdjeno_Ziv_NemaKutije);
                    localAnketaEdit.PutBoolean("uginuliMisZiv", anketa.ANK_Utvrdjeno_Ziv_UlovljenMis);
                    localAnketaEdit.PutBoolean("uginuliStakorZiv", anketa.ANK_Utvrdjeno_Ziv_UlovljenStakor);
                    localAnketaEdit.PutBoolean("novaKutijaUcinjenoZiv", anketa.ANK_Ucinjeno_Ziv_NovaKutija);
                }
                localAnketaEdit.Commit();
                intent = new Intent(this, typeof(Activity_Kontola_page1));
                StartActivity(intent);
            }
            else if (anketa.ANK_TipDeratizacije == 2)
            {
                if (anketa.ANK_TipDeratizacijskeKutijeSifra == 2)
                {
                    localAnketaEdit.PutBoolean("ANK_Kutija_Ljep", true);
                    localAnketaEdit.PutBoolean("kutijaUrednaLjep", anketa.ANK_Utvrdjeno_Ljep_KutijaUredna);
                    localAnketaEdit.PutBoolean("kutijaOstecenaLjep", anketa.ANK_Utvrdjeno_Ljep_KutijaOstecena);
                    localAnketaEdit.PutBoolean("nemaKutijeLjep", anketa.ANK_Utvrdjeno_Ljep_NemaKutije);
                    localAnketaEdit.PutBoolean("ulovljenStakorLjep", anketa.ANK_Utvrdjeno_Ljep_UlovljenStakor);
                    localAnketaEdit.PutBoolean("ulovljenMisLjep", anketa.ANK_Utvrdjeno_Ljep_UlovljenMis);
                    localAnketaEdit.PutBoolean("novaLjepPodUcinjenoLjep", anketa.ANK_Ucinjeno_Ljep_NovaLjepljivaPodloska);
                    localAnketaEdit.PutBoolean("novaKutijaUcinjenoLjep", anketa.ANK_Ucinjeno_Ljep_NovaKutija);
                }
                else if (anketa.ANK_TipDeratizacijskeKutijeSifra == 1)
                {
                    localAnketaEdit.PutBoolean("ANK_Kutija_Rot", true);
                    localAnketaEdit.PutBoolean("kutijaUrednaRot", anketa.ANK_Utvrdjeno_Rot_KutijaUredna);
                    localAnketaEdit.PutBoolean("kutijaOstecenaRot", anketa.ANK_Utvrdjeno_Rot_KutijaOstecena);
                    localAnketaEdit.PutBoolean("nemaKutijeRot", anketa.ANK_Utvrdjeno_Rot_NemaKutije);
                    localAnketaEdit.PutBoolean("tragoviGlodanjaRot", anketa.ANK_Utvrdjeno_Rot_TragoviGlodanja);
                    localAnketaEdit.PutBoolean("izmetRot", anketa.ANK_Utvrdjeno_Rot_Izmet);
                    localAnketaEdit.PutBoolean("pojedenaMekaRot", anketa.ANK_Utvrdjeno_Rot_PojedenaMeka);
                    localAnketaEdit.PutBoolean("ostecenaMekaRot", anketa.ANK_Utvrdjeno_Rot_OstecenaMeka);
                    localAnketaEdit.PutBoolean("uginuliStakorRot", anketa.ANK_Utvrdjeno_Rot_UginuliStakor);
                    localAnketaEdit.PutBoolean("uginuliMisRot", anketa.ANK_Utvrdjeno_Rot_UginuliMis);
                    localAnketaEdit.PutBoolean("noviMamciUcinjenoRot", anketa.ANK_Ucinjeno_Rot_PostavljeniMamci);
                    localAnketaEdit.PutBoolean("novaKutijaUcinjenoRot", anketa.ANK_Ucinjeno_Rot_NovaKutija);
                    localAnketaEdit.PutBoolean("nadopunjenaMekomUcinjenoRot", anketa.ANK_Ucinjeno_Rot_NadopunjenaMekom);
                }
                else if (anketa.ANK_TipDeratizacijskeKutijeSifra == 3)
                {
                    localAnketaEdit.PutBoolean("ANK_Kutija_Ziv", true);
                    localAnketaEdit.PutBoolean("kutijaUrednaZiv", anketa.ANK_Utvrdjeno_Ziv_KutijaUredna);
                    localAnketaEdit.PutBoolean("kutijaOstecenaZiv", anketa.ANK_Utvrdjeno_Ziv_KutijaOstecena);
                    localAnketaEdit.PutBoolean("nemaKutijeZiv", anketa.ANK_Utvrdjeno_Ziv_NemaKutije);
                    localAnketaEdit.PutBoolean("uginuliMisZiv", anketa.ANK_Utvrdjeno_Ziv_UlovljenMis);
                    localAnketaEdit.PutBoolean("uginuliStakorZiv", anketa.ANK_Utvrdjeno_Ziv_UlovljenStakor);
                    localAnketaEdit.PutBoolean("novaKutijaUcinjenoZiv", anketa.ANK_Ucinjeno_Ziv_NovaKutija);
                }

                localAnketaEdit.PutBoolean("rupeBox", anketa.ANK_Poc_Prisutnost_Rupe);
                localAnketaEdit.PutBoolean("legloBox", anketa.ANK_Poc_Prisutnost_Leglo);
                localAnketaEdit.PutBoolean("tragoviNoguBox", anketa.ANK_Poc_Prisutnost_TragoviNogu);
                localAnketaEdit.PutBoolean("videnZiviGlodavacBox", anketa.ANK_Poc_Prisutnost_GlodavacZivi);
                localAnketaEdit.PutBoolean("izmetBox", anketa.ANK_Poc_Prisutnost_Izmet);
                localAnketaEdit.PutBoolean("videnUginuliGlodavacBox", anketa.ANK_Poc_Prisutnost_GlodavacUginuli);
                localAnketaEdit.PutBoolean("stetaBox", anketa.ANK_Poc_Prisutnost_Steta);
                localAnketaEdit.PutBoolean("stakoriBox", anketa.ANK_Poc_Infestacija_Stakori);
                localAnketaEdit.PutBoolean("miseviBox", anketa.ANK_Poc_Infestacija_Misevi);
                localAnketaEdit.PutBoolean("drugiGlodavciBox", anketa.ANK_Poc_Infestacija_DrugiGlodavci);
                localAnketaEdit.PutBoolean("okolisBox", anketa.ANK_Poc_Uvjeti_NeuredanOkolis);
                localAnketaEdit.PutBoolean("hranaBox", anketa.ANK_Poc_Uvjeti_Hrana);
                localAnketaEdit.PutBoolean("odvodnjaBox", anketa.ANK_Poc_Uvjeti_NeispravnaOdvodnja);
                localAnketaEdit.PutBoolean("visitedPage4", true);
                localAnketaEdit.Commit();
                intent = new Intent(this, typeof(Activity_PrvaDer_page1));
                StartActivity(intent);
            }
        }

        private void SpinnerRadniNalog_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            radniNalogId = Convert.ToInt32(radniNaloziIdList[e.Position]);
            localOdradeneAnketeEdit.PutInt("radniNalogId", radniNalogId);

            List<T_KUPDOB> komitentiList = db.Query<T_KUPDOB>(
                "SELECT * " +
                "FROM T_KUPDOB " +
                "INNER JOIN DID_Lokacija ON T_KUPDOB.SIFRA = DID_Lokacija.SAN_KD_SIFRA " +
                "INNER JOIN DID_RadniNalog_Lokacija ON DID_Lokacija.SAN_Id = DID_RadniNalog_Lokacija.Lokacija " +
                "INNER JOIN DID_LokacijaPozicija ON DID_RadniNalog_Lokacija.Lokacija = DID_LokacijaPozicija.SAN_Id " +
                "INNER JOIN DID_Anketa ON DID_LokacijaPozicija.POZ_Id = DID_Anketa.ANK_POZ_Id " +
                "WHERE DID_RadniNalog_Lokacija.RadniNalog = ? " +
                "GROUP BY SIFRA, TIP_PARTNERA, NAZIV, OIB", radniNalogId);

            List<string> listPartneri = new List<string>();
            partneriSifreList = new List<string>();
            foreach (var item in komitentiList)
            {
                listPartneri.Add(item.NAZIV);
                partneriSifreList.Add(item.SIFRA);
            }
            ArrayAdapter<string> adapterPartnerList = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, listPartneri);
            adapterPartnerList.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinnerPartner.Adapter = adapterPartnerList;
        }

        private void SpinnerPartner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            sifraPartnera = partneriSifreList[e.Position];
            localOdradeneAnketeEdit.PutString("sifraPartnera", sifraPartnera);
            List<DID_Lokacija> lokacijaList = db.Query<DID_Lokacija>(
                "SELECT * " +
                "FROM DID_Lokacija " +
                "INNER JOIN DID_LokacijaPozicija ON DID_Lokacija.SAN_Id = DID_LokacijaPozicija.SAN_Id " +
                "INNER JOIN DID_Anketa ON DID_LokacijaPozicija.POZ_Id = DID_Anketa.ANK_POZ_Id " +
                "WHERE DID_Lokacija.SAN_KD_Sifra = ? " +
                "GROUP BY DID_Lokacija.SAN_Id " +
                "ORDER BY LOWER(SAN_Naziv), SAN_Naziv", sifraPartnera);

            lokacijeIdList = new List<string>();
            List<string> listLokacija = new List<string>();
            foreach (var item in lokacijaList)
            {
                listLokacija.Add(item.SAN_Naziv);
                lokacijeIdList.Add(item.SAN_Id.ToString());
            }
            ArrayAdapter<string> adapterLocationList = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, listLokacija);
            adapterLocationList.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinnerLokacija.Adapter = adapterLocationList;
            spinnerLokacija.SetSelection(lokacijeIdList.IndexOf(localOdradeneAnkete.GetInt("lokacijaId", 0).ToString()));
        }

        private void SpinnerLokacija_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            lokacijaId = Convert.ToInt32(lokacijeIdList[e.Position]);
            localOdradeneAnketeEdit.PutInt("lokacijaId", lokacijaId);

            potvrda = db.Query<DID_Potvrda>(
                "SELECT * " +
                "FROM DID_Potvrda " +
                "WHERE Lokacija = ? " +
                "AND RadniNalog = ?", lokacijaId, radniNalogId);

            if (potvrda.Any())
            {
                messagePotvrda.Visibility = Android.Views.ViewStates.Visible;
                flagLokacijaPotvrda = true;
                potvrdaId = potvrda.FirstOrDefault().Id;
            }
            else
            {
                messagePotvrda.Visibility = Android.Views.ViewStates.Invisible;
                flagLokacijaPotvrda = false;
            }

            filtriraneAnkete = db.Query<DID_Anketa>(
                "SELECT DID_Anketa.Id, " +
                "DID_Anketa.ANK_RadniNalog, " +
                "DID_Anketa.ANK_KorisnickoIme, " +
                "DID_Anketa.ANK_POZ_Id, " +
                "DID_Anketa.ANK_DatumVrijeme, " +
                "DID_Anketa.ANK_TipDeratizacijskeKutijeSifra, " +
                "DID_Anketa.ANK_RazlogNeizvrsenja, " +
                "DID_Anketa.ANK_Napomena, " +
                "DID_Anketa.LastEditDate " +
                "FROM DID_Anketa " +
                "INNER JOIN DID_LokacijaPozicija On DID_Anketa.ANK_POZ_Id = DID_LokacijaPozicija.POZ_Id " +
                "WHERE DID_LokacijaPozicija.SAN_Id = ? " +
                "AND DID_Anketa.ANK_RadniNalog = ?", lokacijaId, radniNalogId);

            mAdapter = new Adapter_AnketeRecycleView(filtriraneAnkete);
            mAdapter.ItemClick += MAdapter_ItemClick;
            mAdapter.ItemDelete += MAdapter_ItemDelete;
            mAdapter.ItemEdit += MAdapter_ItemEdit;
            mRecycleView.SetAdapter(mAdapter);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.TitleFormatted.ToString() == "Početna")
                intent = new Intent(this, typeof(Activity_Pocetna));
            else if (item.TitleFormatted.ToString() == "Radni nalozi")
                intent = new Intent(this, typeof(Activity_RadniNalozi));
            else if (item.TitleFormatted.ToString() == "Prikaz odradenih anketa")
                intent = new Intent(this, typeof(Activity_OdradeneAnkete));
            else if (item.TitleFormatted.ToString() == "Prikaz potrošenih materijala")
                intent = new Intent(this, typeof(Activity_PotroseniMaterijali));

            StartActivity(intent);
            return base.OnOptionsItemSelected(item);
        }
    }
}