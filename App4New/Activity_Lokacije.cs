using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Infobit.DDD.Data;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Content.PM;

namespace App4New
{
    [Activity(Label = "Activity_Lokacije", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_Lokacije : Activity
    {
        List<DID_Lokacija> lokacijeList, lokacijeListFiltered;
        EditText searchInput;
        TextView resultMessage;
        Intent intent;
        RecyclerView lokacijaListView;
        RecyclerView.LayoutManager mLayoutManager;
        Adapter_LokacijeRecycleView mAdapter;
        Button novaLokacijaBtn;
        DID_RadniNalog_Lokacija radniNalogLokacija;
        int radniNalog, pozicijaId = 0;

        static readonly ISharedPreferences localUsername = Application.Context.GetSharedPreferences("userLogin", FileCreationMode.Private);
        static readonly ISharedPreferences localNeizvrsernaLokacija = Application.Context.GetSharedPreferences("lokacija", FileCreationMode.Private);
        static ISharedPreferencesEditor localNeizvrsernaLokacijaEdit = localNeizvrsernaLokacija.Edit();
        static readonly ISharedPreferences localPotvrda = Application.Context.GetSharedPreferences("potvrda", FileCreationMode.Private);
        static ISharedPreferencesEditor localPotvrdaEdit = localPotvrda.Edit();
        static readonly ISharedPreferences localKomitentLokacija = Application.Context.GetSharedPreferences("localKomitentLokacija", FileCreationMode.Private);
        static ISharedPreferencesEditor localKomitentLokacijaEdit = localKomitentLokacija.Edit();
        static readonly ISharedPreferences localPozicija = Application.Context.GetSharedPreferences("pozicija", FileCreationMode.Private);
        static ISharedPreferencesEditor localPozicijaEdit = localPozicija.Edit();
        static readonly ISharedPreferences localAnketa = Application.Context.GetSharedPreferences("anketa", FileCreationMode.Private);
        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);
        static readonly ISharedPreferences localProvedbeniPlan = Application.Context.GetSharedPreferences("localProvedbeniPlan", FileCreationMode.Private);
        static ISharedPreferencesEditor localProvedbeniPlanEdit = localProvedbeniPlan.Edit();

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lokacije);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            lokacijaListView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            searchInput = FindViewById<EditText>(Resource.Id.searchInput);
            resultMessage = FindViewById<TextView>(Resource.Id.resultMessage);
            novaLokacijaBtn = FindViewById<Button>(Resource.Id.novaLokacijaBtn);

            SetActionBar(toolbar);
            ActionBar.Title = "Odabir lokacije";
            localAnketa.Edit().Clear().Commit();
            localPozicija.Edit().Clear().Commit();
            localPotvrda.Edit().Clear().Commit();
            localNeizvrsernaLokacija.Edit().Clear().Commit();
            localProvedbeniPlan.Edit().Clear().Commit();
            localKomitentLokacijaEdit.PutBoolean("noviKomitent", false);
            string sifraPartnera = localKomitentLokacija.GetString("sifraKomitenta", null);
            radniNalog = localRadniNalozi.GetInt("id", 0);
            lokacijeList = db.Query<DID_Lokacija>(
                "SELECT * " +
                "FROM DID_Lokacija " +
                "INNER JOIN T_KUPDOB ON DID_Lokacija.SAN_KD_SIFRA = T_KUPDOB.SIFRA " +
                "INNER JOIN DID_RadniNalog_Lokacija ON DID_Lokacija.SAN_Id = DID_RadniNalog_Lokacija.Lokacija " +
                "WHERE DID_Lokacija.SAN_KD_Sifra = ? " +
                "AND DID_RadniNalog_Lokacija.RadniNalog = ?", sifraPartnera, radniNalog);

            lokacijeListFiltered = lokacijeList;
            mLayoutManager = new LinearLayoutManager(this);
            mAdapter = new Adapter_LokacijeRecycleView(lokacijeListFiltered);
            lokacijaListView.SetLayoutManager(mLayoutManager);
            lokacijaListView.SetAdapter(mAdapter);
            mAdapter.ItemPotvrda += MAdapter_PotvrdaClick;
            mAdapter.ItemClick += MAdapter_ItemClick;
            mAdapter.ItemZakljucaj += MAdapter_ItemZakljucaj;
            mAdapter.ItemProvedbeniPlan += MAdapter_ItemProvedbeniPlan;
            mAdapter.ItemPostavke += MAdapter_ItemPostavke;
            searchInput.KeyPress += SearchInput_KeyPress;
            searchInput.TextChanged += SearchInput_TextChanged;
            novaLokacijaBtn.Click += NovaLokacijaBtn_Click;
        }

        public override void OnBackPressed()
        {
            intent = new Intent(this, typeof(Activity_Komitenti));
            StartActivity(intent);
        }

        public void SearchInput_KeyPress(object sender, View.KeyEventArgs e)
        {
            e.Handled = false;
            if (e.KeyCode == Keycode.Enter)
                e.Handled = true;
        }

        public void SearchInput_TextChanged(object sender, EventArgs e)
        {
            string input = searchInput.Text.ToLower();
            if (!string.IsNullOrEmpty(input))
            {
                resultMessage.Visibility = Android.Views.ViewStates.Invisible;
                lokacijeListFiltered = lokacijeList.Where(i =>
                    (i.SAN_UlicaBroj != null && i.SAN_UlicaBroj.ToLower().Contains(input)) ||
                    (i.SAN_Mjesto != null && i.SAN_Mjesto.ToLower().Contains(input)) ||
                    (i.SAN_Naziv != null && i.SAN_Naziv.ToLower().Contains(input))).ToList();

                if (lokacijeListFiltered.Any())
                {
                    mLayoutManager = new LinearLayoutManager(this);
                    mAdapter = new Adapter_LokacijeRecycleView(lokacijeListFiltered);
                    lokacijaListView.SetLayoutManager(mLayoutManager);
                    mAdapter.ItemPotvrda += MAdapter_PotvrdaClick;
                    mAdapter.ItemClick += MAdapter_ItemClick;
                    lokacijaListView.SetAdapter(mAdapter);
                }
                else
                {
                    mLayoutManager = new LinearLayoutManager(this);
                    mAdapter = new Adapter_LokacijeRecycleView(lokacijeListFiltered);
                    lokacijaListView.SetLayoutManager(mLayoutManager);
                    mAdapter.ItemPotvrda += MAdapter_PotvrdaClick;
                    mAdapter.ItemClick += MAdapter_ItemClick;
                    lokacijaListView.SetAdapter(mAdapter);
                    resultMessage.Visibility = Android.Views.ViewStates.Visible;
                }
            }
            else
            {
                lokacijeListFiltered = lokacijeList;

                mLayoutManager = new LinearLayoutManager(this);
                mAdapter = new Adapter_LokacijeRecycleView(lokacijeListFiltered);
                lokacijaListView.SetLayoutManager(mLayoutManager);
                mAdapter.ItemPotvrda += MAdapter_PotvrdaClick;
                mAdapter.ItemClick += MAdapter_ItemClick;
                lokacijaListView.SetAdapter(mAdapter);
                resultMessage.Visibility = Android.Views.ViewStates.Invisible;
            }
        }

        public void NovaLokacijaBtn_Click(object sender, EventArgs e)
        {
            intent = new Intent(this, typeof(Activity_NovaLokacija));
            StartActivity(intent);
        }

        public void MAdapter_PotvrdaClick(object sender, int e)
        {
            int radniNalogLokacijaId = db.Query<DID_RadniNalog_Lokacija>(
                "SELECT * " +
                "FROM DID_RadniNalog_Lokacija " +
                "WHERE Lokacija = ? " +
                "AND RadniNalog = ?", lokacijeListFiltered[e].SAN_Id, radniNalog).FirstOrDefault().Id;

            List<DID_Potvrda> potvrda = db.Query<DID_Potvrda>(
                "SELECT * " +
                "FROM DID_Potvrda " +
                "WHERE RadniNalogLokacijaId = ?", radniNalogLokacijaId);

            // Ako postoji potvrda onda prikazi postojecu potvrdu i spremi njezine vrijednosti u context
            if (potvrda.Any())
            {
                string infestacija = db.Query<DID_RazinaInfestacije>(
                    "SELECT * " +
                    "FROM DID_RazinaInfestacije " +
                    "WHERE Sifra = ?", potvrda.FirstOrDefault().Infestacija).FirstOrDefault().Naziv;

                //Potvrda str.1
                localPotvrdaEdit.PutBoolean("potvrdaPage1", true);
                localPotvrdaEdit.PutString("godina", potvrda.FirstOrDefault().Godina);
                localPotvrdaEdit.PutInt("lokacijaTipBrojObjekata", potvrda.FirstOrDefault().LokacijaTipBrojObjekata);
                localPotvrdaEdit.PutInt("lokacijaTip2BrojObjekata", potvrda.FirstOrDefault().LokacijaTip2BrojObjekata);
                localPotvrdaEdit.PutString("datumPotvrde", potvrda.FirstOrDefault().DatumVrijeme.ToString());
                localPotvrdaEdit.PutString("opisRadaInput", potvrda.FirstOrDefault().OpisRada);
                localPotvrdaEdit.PutString("potvrdaBrInput", potvrda.FirstOrDefault().Broj.ToString());
                localPotvrdaEdit.PutString("razinaInfestacije", infestacija);

                //Potvrda str.2
                List<DID_Potvrda_Djelatnost> djelatnostDeratizacija = db.Query<DID_Potvrda_Djelatnost>(
                    "SELECT * " +
                    "FROM DID_Potvrda_Djelatnost " +
                    "WHERE Potvrda = ? " +
                    "AND Djelatnost = ?", potvrda.FirstOrDefault().Id, 1);

                if (djelatnostDeratizacija.Any())
                {
                    localPotvrdaEdit.PutBoolean("deratizacijaBtn", true);
                    localPotvrdaEdit.PutString("brObjekataDeratizacija", djelatnostDeratizacija.FirstOrDefault().BrojObjekata.ToString());
                    localPotvrdaEdit.PutString("brObjekataDeratizacija2", djelatnostDeratizacija.FirstOrDefault().BrojObjekataTip2.ToString());
                    foreach (var posao in djelatnostDeratizacija)
                    {
                        if (posao.TipPosla == "1")
                            localPotvrdaEdit.PutBoolean("postavljanjeMaterijala", true);
                        else if (posao.TipPosla == "2")
                            localPotvrdaEdit.PutBoolean("kontrola", true);
                    }
                }
                else
                {
                    localPotvrdaEdit.PutBoolean("deratizacijaBtn", false);
                    localPotvrdaEdit.PutBoolean("postavljanjeMaterijala", false);
                    localPotvrdaEdit.PutBoolean("kontrola", false);
                }

                List<DID_Potvrda_Djelatnost> djelatnostDezinsekcija = db.Query<DID_Potvrda_Djelatnost>(
                    "SELECT * " +
                    "FROM DID_Potvrda_Djelatnost " +
                    "WHERE Potvrda = ? " +
                    "AND Djelatnost = ?", potvrda.FirstOrDefault().Id, 2);

                if (djelatnostDezinsekcija.Any())
                {
                    localPotvrdaEdit.PutString("brObjekataDezinsekcija", djelatnostDezinsekcija.FirstOrDefault().BrojObjekata.ToString());
                    localPotvrdaEdit.PutString("brObjekataDezinsekcija2", djelatnostDezinsekcija.FirstOrDefault().BrojObjekataTip2.ToString());
                    localPotvrdaEdit.PutBoolean("dezinsekcijaBtn", true);
                    foreach (var posao in djelatnostDezinsekcija)
                    {
                        if (posao.TipPosla == "3")
                            localPotvrdaEdit.PutBoolean("postavljanjeLovki", true);
                    }
                }
                else
                {
                    localPotvrdaEdit.PutBoolean("dezinsekcijaBtn", false);
                    localPotvrdaEdit.PutBoolean("postavljanjeLovki", false);
                }

                List<DID_Potvrda_Djelatnost> djelatnostDezinfekcija = db.Query<DID_Potvrda_Djelatnost>(
                    "SELECT * " +
                    "FROM DID_Potvrda_Djelatnost " +
                    "WHERE Potvrda = ? " +
                    "AND Djelatnost = ?", potvrda.FirstOrDefault().Id, 3);

                if (djelatnostDezinfekcija.Any())
                {
                    localPotvrdaEdit.PutString("brObjekataDezinfekcija", djelatnostDezinfekcija.FirstOrDefault().BrojObjekata.ToString());
                    localPotvrdaEdit.PutString("brObjekataDezinfekcija2", djelatnostDezinfekcija.FirstOrDefault().BrojObjekataTip2.ToString());
                    localPotvrdaEdit.PutBoolean("dezinfekcijaBtn", true);
                    foreach (var posao in djelatnostDezinfekcija)
                    {
                        if (posao.TipPosla == "4")
                            localPotvrdaEdit.PutBoolean("tretman", true);
                    }
                }
                else
                {
                    localPotvrdaEdit.PutBoolean("dezinfekcijaBtn", false);
                    localPotvrdaEdit.PutBoolean("tretman", false);
                }

                List<DID_Potvrda_Djelatnost> djelatnostZasistaBilja = db.Query<DID_Potvrda_Djelatnost>(
                    "SELECT * " +
                    "FROM DID_Potvrda_Djelatnost " +
                    "WHERE Potvrda = ? " +
                    "AND Djelatnost = ?", potvrda.FirstOrDefault().Id, 5);

                if (djelatnostZasistaBilja.Any())
                {
                    localPotvrdaEdit.PutString("brObjekataZastitaBilja", djelatnostZasistaBilja.FirstOrDefault().BrojObjekata.ToString());
                    localPotvrdaEdit.PutString("brObjekataZastitaBilja2", djelatnostZasistaBilja.FirstOrDefault().BrojObjekataTip2.ToString());
                    localPotvrdaEdit.PutBoolean("zastitaBiljaBtn", true);
                    foreach (var posao in djelatnostZasistaBilja)
                    {
                        if (posao.TipPosla == "6")
                            localPotvrdaEdit.PutBoolean("tretman2", true);
                    }
                }
                else
                {
                    localPotvrdaEdit.PutBoolean("zastitaBiljaBtn", false);
                    localPotvrdaEdit.PutBoolean("tretman2", false);
                }

                List<DID_Potvrda_Djelatnost> djelatnostDezodorizacija = db.Query<DID_Potvrda_Djelatnost>(
                   "SELECT * " +
                   "FROM DID_Potvrda_Djelatnost " +
                   "WHERE Potvrda = ? " +
                   "AND Djelatnost = ?", potvrda.FirstOrDefault().Id, 4);

                if (djelatnostDezodorizacija.Any())
                {
                    localPotvrdaEdit.PutString("brObjekataDezodorizacija", djelatnostDezodorizacija.FirstOrDefault().BrojObjekata.ToString());
                    localPotvrdaEdit.PutString("brObjekataDezodorizacija2", djelatnostDezodorizacija.FirstOrDefault().BrojObjekataTip2.ToString());
                }

                List<DID_Potvrda_Djelatnost> djelatnostSuzbijanjeStetnika = db.Query<DID_Potvrda_Djelatnost>(
                   "SELECT * " +
                   "FROM DID_Potvrda_Djelatnost " +
                   "WHERE Potvrda = ? " +
                   "AND Djelatnost = ?", potvrda.FirstOrDefault().Id, 6);

                if (djelatnostSuzbijanjeStetnika.Any())
                {
                    localPotvrdaEdit.PutString("brObjekataSuzbijanjeStetnika", djelatnostSuzbijanjeStetnika.FirstOrDefault().BrojObjekata.ToString());
                    localPotvrdaEdit.PutString("brObjekataSuzbijanjeStetnika2", djelatnostSuzbijanjeStetnika.FirstOrDefault().BrojObjekataTip2.ToString());
                }

                List<DID_Potvrda_Djelatnost> djelatnostKIInsekti = db.Query<DID_Potvrda_Djelatnost>(
                   "SELECT * " +
                   "FROM DID_Potvrda_Djelatnost " +
                   "WHERE Potvrda = ? " +
                   "AND Djelatnost = ?", potvrda.FirstOrDefault().Id, 7);

                if (djelatnostKIInsekti.Any())
                {
                    localPotvrdaEdit.PutString("brObjekataKIInsekti", djelatnostKIInsekti.FirstOrDefault().BrojObjekata.ToString());
                    localPotvrdaEdit.PutString("brObjekataKIInsekti2", djelatnostKIInsekti.FirstOrDefault().BrojObjekataTip2.ToString());
                }

                List<DID_Potvrda_Djelatnost> djelatnostKIGlodavci = db.Query<DID_Potvrda_Djelatnost>(
                   "SELECT * " +
                   "FROM DID_Potvrda_Djelatnost " +
                   "WHERE Potvrda = ? " +
                   "AND Djelatnost = ?", potvrda.FirstOrDefault().Id, 8);

                if (djelatnostKIGlodavci.Any())
                {
                    localPotvrdaEdit.PutString("brObjekataKIGlodavci", djelatnostKIGlodavci.FirstOrDefault().BrojObjekata.ToString());
                    localPotvrdaEdit.PutString("brObjekataKIGlodavci2", djelatnostKIGlodavci.FirstOrDefault().BrojObjekataTip2.ToString());
                }

                List<DID_Potvrda_Djelatnost> djelatnostuzimanjeBriseva = db.Query<DID_Potvrda_Djelatnost>(
                   "SELECT * " +
                   "FROM DID_Potvrda_Djelatnost " +
                   "WHERE Potvrda = ? " +
                   "AND Djelatnost = ?", potvrda.FirstOrDefault().Id, 9);

                if (djelatnostuzimanjeBriseva.Any())
                {
                    localPotvrdaEdit.PutString("brObjekataUzimanjeBriseva", djelatnostuzimanjeBriseva.FirstOrDefault().BrojObjekata.ToString());
                    localPotvrdaEdit.PutString("brObjekataUzimanjeBriseva2", djelatnostuzimanjeBriseva.FirstOrDefault().BrojObjekataTip2.ToString());
                }

                List<DID_Potvrda_Djelatnost> djelatnostsuzbijanjeKorova = db.Query<DID_Potvrda_Djelatnost>(
                   "SELECT * " +
                   "FROM DID_Potvrda_Djelatnost " +
                   "WHERE Potvrda = ? " +
                   "AND Djelatnost = ?", potvrda.FirstOrDefault().Id, 10);

                if (djelatnostsuzbijanjeKorova.Any())
                {
                    localPotvrdaEdit.PutString("brObjekataSuzbijanjeKorova", djelatnostsuzbijanjeKorova.FirstOrDefault().BrojObjekata.ToString());
                    localPotvrdaEdit.PutString("brObjekataSuzbijanjeKorova2", djelatnostsuzbijanjeKorova.FirstOrDefault().BrojObjekataTip2.ToString());
                }

                List<DID_Potvrda_Djelatnost> djelatnostkosnjaTrave = db.Query<DID_Potvrda_Djelatnost>(
                   "SELECT * " +
                   "FROM DID_Potvrda_Djelatnost " +
                   "WHERE Potvrda = ? " +
                   "AND Djelatnost = ?", potvrda.FirstOrDefault().Id, 11);

                if (djelatnostkosnjaTrave.Any())
                {
                    localPotvrdaEdit.PutString("brObjekatakosnjaTrave", djelatnostkosnjaTrave.FirstOrDefault().BrojObjekata.ToString());
                    localPotvrdaEdit.PutString("brObjekatakosnjaTrave2", djelatnostkosnjaTrave.FirstOrDefault().BrojObjekataTip2.ToString());
                }

                localPotvrdaEdit.PutBoolean("dezodorizacijaBtn", djelatnostDezodorizacija.Any() ? true : false);
                localPotvrdaEdit.PutBoolean("suzbijanjeStetnikaBtn", djelatnostSuzbijanjeStetnika.Any() ? true : false);
                localPotvrdaEdit.PutBoolean("KIInsektiBtn", djelatnostKIInsekti.Any() ? true : false);
                localPotvrdaEdit.PutBoolean("KIGlodavciBtn", djelatnostKIGlodavci.Any() ? true : false);
                localPotvrdaEdit.PutBoolean("uzimanjeBrisevaBtn", djelatnostuzimanjeBriseva.Any() ? true : false);
                localPotvrdaEdit.PutBoolean("suzbijanjeKorovaBtn", djelatnostsuzbijanjeKorova.Any() ? true : false);
                localPotvrdaEdit.PutBoolean("kosnjaTraveBtn", djelatnostkosnjaTrave.Any() ? true : false);
                localPotvrdaEdit.PutBoolean("potvrdaPage2", true);

                // Potvrda str.3
                localPotvrdaEdit.PutBoolean("potvrdaPage3", true);


                // Potvrda -> opci podaci u contexu
                localPotvrdaEdit.PutInt("id", potvrda.FirstOrDefault().Id);
                localPotvrdaEdit.PutBoolean("edit", true);
                localPotvrdaEdit.Commit();
            }

            SaveState(lokacijeListFiltered[e]);

            intent = new Intent(this, typeof(Activity_Potvrda_page1));
            StartActivity(intent);
        }

        public void MAdapter_ItemPostavke(object sender, int e)
        {
            SaveState(lokacijeListFiltered[e]);

            intent = new Intent(this, typeof(Activity_LokacijaPostavke));
            StartActivity(intent);
        }

        public void MAdapter_ItemProvedbeniPlan(object sender, int e)
        {
            // Trenutno u razvoju
            localProvedbeniPlanEdit.PutInt("lokacijaId", lokacijeListFiltered[e].SAN_Id);
            localProvedbeniPlanEdit.Commit();

            intent = new Intent(this, typeof(Activity_ProvedbeniPlan_page1));
            StartActivity(intent);
        }

        public void MAdapter_ItemClick(object sender, int e)
        {
            // ako lokacija ima 1 poziciju i ta pozicija nema barcode, znaci da ta lokacija nema pozicija (ta jedna je fiktivna)
            // -> bacaj na stranicu za dodavanje potrosenih materijala

            SaveState(lokacijeListFiltered[e]);

            DID_RadniNalog_Lokacija rnLokacija = db.Query<DID_RadniNalog_Lokacija>(
                "SELECT * " +
                "FROM DID_RadniNalog_Lokacija " +
                "WHERE Lokacija = ? " +
                "AND RadniNalog = ?", lokacijeListFiltered[e].SAN_Id, radniNalog).FirstOrDefault();

            if(rnLokacija.Status == 4)
            {
                intent = new Intent(this, typeof(Activity_LokacijaPostavke));
                StartActivity(intent);
            }
            else if (!lokacijeListFiltered[e].SAN_AnketePoPozicijama)
            {
                // 1. Odabirem prvu poziciju
                // 2. Ako nema ni jedne pozicije generiram poziciju s brojem 1 
                List<DID_LokacijaPozicija> pozicije = db.Query<DID_LokacijaPozicija>(
                    "SELECT * " +
                    "FROM DID_LokacijaPozicija " +
                    "INNER JOIN DID_RadniNalog_Lokacija ON DID_LokacijaPozicija.SAN_Id = DID_RadniNalog_Lokacija.Lokacija " +
                    "WHERE DID_LokacijaPozicija.SAN_Id = ? " +
                    "AND DID_RadniNalog_Lokacija.RadniNalog = ?" +
                    "ORDER BY POZ_Broj, POZ_BrojOznaka, POZ_Id, SAN_Id, POZ_Barcode, POZ_Tip, POZ_Status, POZ_Opis", lokacijeListFiltered[e].SAN_Id, radniNalog);

                if(!pozicije.Any())
                {
                    pozicijaId = -1;
                    List<DID_LokacijaPozicija> pozicijeNove = db.Query<DID_LokacijaPozicija>(
                       "SELECT * " +
                       "FROM DID_LokacijaPozicija " +
                       "WHERE POZ_Id < 0 " +
                       "ORDER BY POZ_Id");

                    if (pozicijeNove.Any())
                        pozicijaId = pozicijeNove.FirstOrDefault().POZ_Id - 1;

                    db.Query<DID_LokacijaPozicija>(
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
                            pozicijaId,
                            lokacijeListFiltered[e].SAN_Id,
                            1,
                            null,
                            null,
                            1,
                            1,
                            null,
                            pozicijaId,
                            1
                    );

                    Guid id = Guid.NewGuid();
                    var username = localUsername.GetString("nazivDjelatnika", null);
                    db.Query<DID_Anketa>(
                        "INSERT INTO DID_Anketa (" +
                            "Id, " +
                            "ANK_TipDeratizacije, " +
                            "ANK_RadniNalog, " +
                            "ANK_KorisnickoIme, " +
                            "ANK_POZ_Id, " +
                            "ANK_DatumVrijeme, " +
                            "LastEditDate, " +
                            "ANK_TipDeratizacijskeKutijeSifra, " +
                            "SinhronizacijaPrivremeniKljuc, " +
                            "SinhronizacijaStatus)" +
                        "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                            id,
                            2,
                            radniNalog,
                            username,
                            pozicijaId,
                            DateTime.Now,
                            DateTime.Now,
                            1,
                            id,
                            2
                        );
                }
                else
                {
                    pozicijaId = pozicije.FirstOrDefault().POZ_Id;

                    List<DID_Anketa> anketa = db.Query<DID_Anketa>(
                        "SELECT * " +
                        "FROM DID_Anketa " +
                        "WHERE ANK_POZ_Id = ? " +
                        "AND ANK_RadniNalog = ?", pozicijaId, radniNalog);

                    if (!anketa.Any())
                    {
                        Guid id = Guid.NewGuid();
                        var username = localUsername.GetString("nazivDjelatnika", null);
                        db.Query<DID_Anketa>(
                            "INSERT INTO DID_Anketa (" +
                                "Id, " +
                                "ANK_TipDeratizacije, " +
                                "ANK_RadniNalog, " +
                                "ANK_KorisnickoIme, " +
                                "ANK_POZ_Id, " +
                                "ANK_DatumVrijeme, " +
                                "LastEditDate, " +
                                "ANK_TipDeratizacijskeKutijeSifra, " +
                                "SinhronizacijaPrivremeniKljuc, " +
                                "SinhronizacijaStatus)" +
                            "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                                id,
                                2,
                                radniNalog,
                                username,
                                pozicijaId,
                                DateTime.Now,
                                DateTime.Now,
                                1,
                                id,
                                2);
                    }
                    else
                    {
                        db.Query<DID_Anketa>(
                            "UPDATE DID_Anketa " +
                            "SET ANK_TipDeratizacije = ?, " +
                                "ANK_DatumVrijeme = ?, " +
                                "LastEditDate = ? " +
                            "WHERE Id = ?",
                                2,
                                DateTime.Now,
                                DateTime.Now,
                                anketa.FirstOrDefault().Id);
                    }
                }

                localPozicijaEdit.PutInt("pozicijaId", pozicijaId);
                localPozicijaEdit.Commit();
                intent = new Intent(this, typeof(Activity_PotroseniMaterijali_List));
                StartActivity(intent);
            }
            else 
            {
                intent = new Intent(this, typeof(Activity_Pozicije));
                StartActivity(intent);
            }
        }

        public void MAdapter_ItemZakljucaj(object sender, int e)
        {
            DID_RadniNalog_Lokacija radniNalogLokacijaId = db.Query<DID_RadniNalog_Lokacija>(
                "SELECT * " +
                "FROM DID_RadniNalog_Lokacija " +
                "WHERE Lokacija = ? " +
                "AND RadniNalog = ?", lokacijeListFiltered[e].SAN_Id, radniNalog).FirstOrDefault();

            localKomitentLokacijaEdit.PutInt("radniNalogLokacijaId", radniNalogLokacijaId.Id);
            localKomitentLokacijaEdit.PutInt("lokacijaId", lokacijeListFiltered[e].SAN_Id);
            localKomitentLokacijaEdit.PutString("lokacijaNaziv", lokacijeListFiltered[e].SAN_Naziv);
            localKomitentLokacijaEdit.Commit();

            intent = new Intent(this, typeof(Activity_LokacijaZavrsena));
            StartActivity(intent);
        }

        public void SaveState(DID_Lokacija lokacija)
        {
            radniNalogLokacija = db.Query<DID_RadniNalog_Lokacija>(
                "SELECT * " +
                "FROM DID_RadniNalog_Lokacija " +
                "WHERE Lokacija = ? " +
                "AND RadniNalog = ?", lokacija.SAN_Id, radniNalog).FirstOrDefault();

            List<DID_Potvrda> potvrde = db.Query<DID_Potvrda>(
                "SELECT * " +
                "FROM DID_Potvrda " +
                "WHERE RadniNalogLokacijaId = ?", radniNalogLokacija.Id);

            if (!potvrde.Any() && radniNalogLokacija.Status == 4)
            {
                string razlogNeizvrsenja = db.Query<DID_RazlogNeizvrsenjaDeratizacije>(
                    "SELECT * " +
                    "FROM DID_RazlogNeizvrsenjaDeratizacije " +
                    "WHERE Sifra = ?", radniNalogLokacija.RazlogNeizvrsenja).FirstOrDefault().Naziv;

                localNeizvrsernaLokacijaEdit.PutBoolean("visitedNeizvrsenaLokacija", true);
                localNeizvrsernaLokacijaEdit.PutBoolean("neprovedenoRadioBtn", true);
                localNeizvrsernaLokacijaEdit.PutString("opisPosla", radniNalogLokacija.OpisPosla);
                localNeizvrsernaLokacijaEdit.PutString("napomena", radniNalogLokacija.Napomena);
                localNeizvrsernaLokacijaEdit.PutString("spinnerSelectedItem", razlogNeizvrsenja);
                localNeizvrsernaLokacijaEdit.Commit();
            }

            localPotvrdaEdit.PutBoolean("fromList", true);
            localPotvrdaEdit.Commit();
            localKomitentLokacijaEdit.PutInt("radniNalogLokacijaId", radniNalogLokacija.Id);
            localKomitentLokacijaEdit.PutInt("lokacijaId", lokacija.SAN_Id);
            localKomitentLokacijaEdit.PutString("lokacijaNaziv", lokacija.SAN_Naziv);
            localKomitentLokacijaEdit.Commit();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menuBackHome, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.TitleFormatted.ToString() == "Početna")
                intent = new Intent(this, typeof(Activity_Pocetna));
            else if (item.TitleFormatted.ToString() == "nazad")
                intent = new Intent(this, typeof(Activity_Komitenti));
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