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
    [Activity(Label = "Activity_ProvedbeniPlan_page3", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_ProvedbeniPlan_page3 : Activity
    {
        Intent intent;
        EditText brojIzvidaGlodavacaGodisnje, rasporedIzvidaGlodavaca, brojOpcihDeratizacijaGodisnje, rasporedRadaDeratizacije,
            brojIzvidaInsekataGodisnje, rasporedIzvidaInsekata, brojOpcihDezinsekcijeGodisnje, rasporedRadaDezinsekcije, brojDanaDoDodatneKontroleInsekata,
            brojDanaDoDodatneDezinsekcije, brojOpcihDezinfekacijaGodisnje, rasporedRadaDezinfekacije;
        Button spremiBtn, odustaniBtn, backBtn;
        RecyclerView materijaliDeratizacijeListView, materijaliDezinsekcijaListView, materijaliDezinfekcijaListView;
        RecyclerView.LayoutManager mLayoutManagerDeratizacija, mLayoutManagerDezinsekcija, mLayoutManagerDezinfekcija;
        Adapter_Materijali_ProvedbeniPlan mAdapterDeratizacija, mAdapterDezinsekcija, mAdapterDezinfekcija;
        int lokacijaId, dezinfekcijaSat, dezinfekcijaMinuta, dezinsekcijaSat, dezinsekcijaMinuta;
        List<T_NAZR> materijaliDeratizacije, materijaliDezinsekcije, materijaliDezinfekcija;
        LinearLayout buttonsLayout, buttonBack;

        TimePicker satZaProvedbuDezinfekacije, satZaProvedbuDezinsekcije;
        TimeSpan satDezinfekcija, satDezinsekcija;
        TextView satZaProvedbuDezinfekacijeTV, satZaProvedbuDezinsekcijeTV;

        static readonly ISharedPreferences localProvedbeniPlan = Application.Context.GetSharedPreferences("localProvedbeniPlan", FileCreationMode.Private);
        static ISharedPreferencesEditor localProvedbeniPlanEdit = localProvedbeniPlan.Edit();
        static readonly ISharedPreferences localUsername = Application.Context.GetSharedPreferences("userLogin", FileCreationMode.Private);

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.provedbeniPlan_page3);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            buttonsLayout = FindViewById<LinearLayout>(Resource.Id.buttonsLayout);
            buttonBack = FindViewById<LinearLayout>(Resource.Id.buttonBack);
            spremiBtn = FindViewById<Button>(Resource.Id.spremiBtn);
            odustaniBtn = FindViewById<Button>(Resource.Id.odustaniBtn);
            backBtn = FindViewById<Button>(Resource.Id.backBtn);
            brojIzvidaGlodavacaGodisnje = FindViewById<EditText>(Resource.Id.brojIzvidaGlodavacaGodisnje);
            rasporedIzvidaGlodavaca = FindViewById<EditText>(Resource.Id.rasporedIzvidaGlodavaca);
            brojOpcihDeratizacijaGodisnje = FindViewById<EditText>(Resource.Id.brojOpcihDeratizacijaGodisnje);
            rasporedRadaDeratizacije = FindViewById<EditText>(Resource.Id.rasporedRadaDeratizacije);
            brojIzvidaInsekataGodisnje = FindViewById<EditText>(Resource.Id.brojIzvidaInsekataGodisnje);
            rasporedIzvidaInsekata = FindViewById<EditText>(Resource.Id.rasporedIzvidaInsekata);
            brojOpcihDezinsekcijeGodisnje = FindViewById<EditText>(Resource.Id.brojOpcihDezinsekcijeGodisnje);
            rasporedRadaDezinsekcije = FindViewById<EditText>(Resource.Id.rasporedRadaDezinsekcije);
            satZaProvedbuDezinsekcije = FindViewById<TimePicker>(Resource.Id.satZaProvedbuDezinsekcije);
            brojDanaDoDodatneKontroleInsekata = FindViewById<EditText>(Resource.Id.brojDanaDoDodatneKontroleInsekata);
            brojDanaDoDodatneDezinsekcije = FindViewById<EditText>(Resource.Id.brojDanaDoDodatneDezinsekcije);
            brojOpcihDezinfekacijaGodisnje = FindViewById<EditText>(Resource.Id.brojOpcihDezinfekacijaGodisnje);
            rasporedRadaDezinfekacije = FindViewById<EditText>(Resource.Id.rasporedRadaDezinfekacije);
            satZaProvedbuDezinfekacije = FindViewById<TimePicker>(Resource.Id.satZaProvedbuDezinfekacije);
            materijaliDeratizacijeListView = FindViewById<RecyclerView>(Resource.Id.materijaliDeratizacijeListView);
            materijaliDezinsekcijaListView = FindViewById<RecyclerView>(Resource.Id.materijaliDezinsekcijaListView);
            materijaliDezinfekcijaListView = FindViewById<RecyclerView>(Resource.Id.materijaliDezinfekcijaListView);
            satZaProvedbuDezinsekcije.TimeChanged += SatZaProvedbuDezinsekcije_TimeChanged;
            satZaProvedbuDezinfekacije.TimeChanged += SatZaProvedbuDezinfekacije_TimeChanged;
            satZaProvedbuDezinsekcijeTV = FindViewById<TextView>(Resource.Id.satZaProvedbuDezinsekcijeTV);
            satZaProvedbuDezinfekacijeTV = FindViewById<TextView>(Resource.Id.satZaProvedbuDezinfekacijeTV);

            materijaliDeratizacije = db.Query<T_NAZR>(
                "SELECT * " +
                "FROM T_NAZR " +
                "WHERE TipKemikalije = 1");
            mLayoutManagerDeratizacija = new LinearLayoutManager(this);
            mAdapterDeratizacija = new Adapter_Materijali_ProvedbeniPlan(materijaliDeratizacije);
            materijaliDeratizacijeListView.SetLayoutManager(mLayoutManagerDeratizacija);
            mAdapterDeratizacija.CheckboxClick += MAdapter_CheckboxClickDeratizacija;
            materijaliDeratizacijeListView.SetAdapter(mAdapterDeratizacija);

            materijaliDezinsekcije = db.Query<T_NAZR>(
                "SELECT * " +
                "FROM T_NAZR " +
                "WHERE TipKemikalije = 2");
            mLayoutManagerDezinsekcija = new LinearLayoutManager(this);
            mAdapterDezinsekcija = new Adapter_Materijali_ProvedbeniPlan(materijaliDezinsekcije);
            materijaliDezinsekcijaListView.SetLayoutManager(mLayoutManagerDezinsekcija);
            mAdapterDezinsekcija.CheckboxClick += MAdapter_CheckboxClickDezinsekcija;
            materijaliDezinsekcijaListView.SetAdapter(mAdapterDezinsekcija);

            materijaliDezinfekcija = db.Query<T_NAZR>(
                "SELECT * " +
                "FROM T_NAZR " +
                "WHERE TipKemikalije = 4");
            mLayoutManagerDezinfekcija = new LinearLayoutManager(this);
            mAdapterDezinfekcija = new Adapter_Materijali_ProvedbeniPlan(materijaliDezinfekcija);
            materijaliDezinfekcijaListView.SetLayoutManager(mLayoutManagerDezinfekcija);
            mAdapterDezinfekcija.CheckboxClick += MAdapter_CheckboxClickDezinfekcija;
            materijaliDezinfekcijaListView.SetAdapter(mAdapterDezinfekcija);

            SetActionBar(toolbar);  
            ActionBar.Title = "Provedbeni plan";
            spremiBtn.Click += SpremiBtn_Click;
            odustaniBtn.Click += OdustaniBtn_Click;
            backBtn.Click += BackBtn_Click;
            lokacijaId = localProvedbeniPlan.GetInt("lokacijaId", 0);

            CheckIfProvedbeniPlanExist();
        }

        private void SatZaProvedbuDezinsekcije_TimeChanged(object sender, TimePicker.TimeChangedEventArgs args)
        {
            satDezinsekcija = new TimeSpan(args.HourOfDay, args.Minute, 0);
            dezinsekcijaSat = args.HourOfDay;
            dezinsekcijaMinuta = args.Minute;
        }

        private void SatZaProvedbuDezinfekacije_TimeChanged(object sender, TimePicker.TimeChangedEventArgs args)
        {
            satDezinfekcija = new TimeSpan(args.HourOfDay, args.Minute, 0);
            dezinfekcijaSat = args.HourOfDay;
            dezinfekcijaMinuta = args.Minute;
        }

        public void CheckIfProvedbeniPlanExist()
        {
            var provedbeniPlan = db.Query<DID_ProvedbeniPlan>(
                "SELECT * " +
                "FROM DID_ProvedbeniPlan " +
                "WHERE Lokacija = ? " +
                "AND Godina = ?", lokacijaId, DateTime.Now.Year.ToString());

            if (provedbeniPlan.Any())
            {
                satZaProvedbuDezinsekcije.Hour = provedbeniPlan.FirstOrDefault().SatZaProvedbuDezinsekcije.Hours;
                satZaProvedbuDezinsekcije.Minute = provedbeniPlan.FirstOrDefault().SatZaProvedbuDezinsekcije.Minutes;
                satZaProvedbuDezinfekacije.Hour = provedbeniPlan.FirstOrDefault().SatZaProvedbuDezinfekcije.Hours;
                satZaProvedbuDezinfekacije.Minute = provedbeniPlan.FirstOrDefault().SatZaProvedbuDezinfekcije.Minutes;
                satZaProvedbuDezinsekcijeTV.Text = satDezinsekcija.ToString();
                satZaProvedbuDezinfekacijeTV.Text = satDezinfekcija.ToString();

                // Prikaz dodanih nametnika na provedbenom planu
                var materijaliList = db.Query<DID_ProvedbeniPlan_Materijal>(
                    "SELECT * " +
                    "FROM DID_ProvedbeniPlan_Materijal " +
                    "WHERE ProvedbeniPlan = ?", provedbeniPlan.FirstOrDefault().Id);

                foreach (var materijal in materijaliList)
                    localProvedbeniPlanEdit.PutString("materijal" + materijal.MaterijalSifra, materijal.MaterijalSifra);

                localProvedbeniPlanEdit.PutInt("provedbeniPlanId", provedbeniPlan.FirstOrDefault().Id);
                localProvedbeniPlanEdit.Commit();
                DisableInput();
            }
            else
                IsPageVisited();
        }

        public void DisableInput()
        {
            // Disablanje inputa -> provedbeni plan nije moguće editirati
            buttonsLayout.Visibility = Android.Views.ViewStates.Gone;
            buttonBack.Visibility = Android.Views.ViewStates.Visible;
            brojIzvidaGlodavacaGodisnje.Enabled = false;
            rasporedIzvidaGlodavaca.Enabled = false;
            brojOpcihDeratizacijaGodisnje.Enabled = false;
            rasporedRadaDeratizacije.Enabled = false;
            brojIzvidaInsekataGodisnje.Enabled = false;
            rasporedIzvidaInsekata.Enabled = false;
            brojOpcihDezinsekcijeGodisnje.Enabled = false;
            rasporedRadaDezinsekcije.Enabled = false;
            brojDanaDoDodatneKontroleInsekata.Enabled = false;
            brojDanaDoDodatneDezinsekcije.Enabled = false;
            brojOpcihDezinfekacijaGodisnje.Enabled = false;
            rasporedRadaDezinfekacije.Enabled = false;

            satZaProvedbuDezinsekcije.Visibility = Android.Views.ViewStates.Gone;
            satZaProvedbuDezinfekacije.Visibility = Android.Views.ViewStates.Gone;
            satZaProvedbuDezinsekcijeTV.Visibility = Android.Views.ViewStates.Visible;
            satZaProvedbuDezinfekacijeTV.Visibility = Android.Views.ViewStates.Visible;
        }

        public void MAdapter_CheckboxClickDeratizacija(object sender, int e)
        {
            SetOdabraniMaterijal(materijaliDeratizacije[e]);
        }

        public void MAdapter_CheckboxClickDezinsekcija(object sender, int e)
        {
            SetOdabraniMaterijal(materijaliDezinsekcije[e]);
        }

        public void MAdapter_CheckboxClickDezinfekcija(object sender, int e)
        {
            SetOdabraniMaterijal(materijaliDezinfekcija[e]);
        }

        public void SetOdabraniMaterijal(T_NAZR odabraniMaterijal)
        {
            if (localProvedbeniPlan.GetString("materijal" + odabraniMaterijal.NAZR_SIFRA, null) != null)
                localProvedbeniPlanEdit.PutString("materijal" + odabraniMaterijal.NAZR_SIFRA, null);
            else
                localProvedbeniPlanEdit.PutString("materijal" + odabraniMaterijal.NAZR_SIFRA, odabraniMaterijal.NAZR_SIFRA);

            localProvedbeniPlanEdit.Commit();
        }

        public void IsPageVisited()
        {
            if(localProvedbeniPlan.GetBoolean("visited_page3", false))
            {
                brojIzvidaGlodavacaGodisnje.Text = localProvedbeniPlan.GetString("brojIzvidaGlodavacaGodisnje_page3", null);
                rasporedIzvidaGlodavaca.Text = localProvedbeniPlan.GetString("rasporedIzvidaGlodavaca_page3", null);
                brojOpcihDeratizacijaGodisnje.Text = localProvedbeniPlan.GetString("brojOpcihDeratizacijaGodisnje_page3", null);
                rasporedRadaDeratizacije.Text = localProvedbeniPlan.GetString("rasporedRadaDeratizacije_page3", null);
                brojIzvidaInsekataGodisnje.Text = localProvedbeniPlan.GetString("brojIzvidaInsekataGodisnje_page3", null);
                rasporedIzvidaInsekata.Text = localProvedbeniPlan.GetString("rasporedIzvidaInsekata_page3", null);
                rasporedRadaDezinsekcije.Text = localProvedbeniPlan.GetString("rasporedRadaDezinsekcije_page3", null);
                brojDanaDoDodatneKontroleInsekata.Text = localProvedbeniPlan.GetString("brojDanaDoDodatneKontroleInsekata_page3", null);
                brojDanaDoDodatneDezinsekcije.Text = localProvedbeniPlan.GetString("brojDanaDoDodatneDezinsekcije_page3", null);
                brojOpcihDezinfekacijaGodisnje.Text = localProvedbeniPlan.GetString("brojOpcihDezinfekacijaGodisnje_page3", null);
                rasporedRadaDezinfekacije.Text = localProvedbeniPlan.GetString("rasporedRadaDezinfekacije_page3", null);

                dezinsekcijaSat = localProvedbeniPlan.GetInt("dezinsekcijaSat", 0);
                dezinsekcijaMinuta = localProvedbeniPlan.GetInt("dezinsekcijaMinuta", 0);
                satZaProvedbuDezinsekcije.Hour = dezinsekcijaSat;
                satZaProvedbuDezinsekcije.Minute = dezinsekcijaMinuta;
                dezinfekcijaSat = localProvedbeniPlan.GetInt("dezinfekcijaSat", 0);
                dezinfekcijaMinuta = localProvedbeniPlan.GetInt("dezinfekcijaMinuta", 0);
                satZaProvedbuDezinfekacije.Hour = dezinfekcijaSat;
                satZaProvedbuDezinfekacije.Minute = dezinfekcijaMinuta;
            }
        }

        public void SpremiBtn_Click(object sender, EventArgs args)
        {
            // Spremanje Provedbenog plana
            // OPCI podatci
            var nazivZaposlenika = localUsername.GetString("nazivDjelatnika", null);


            // PAGE 1
            DateTime datumInput_page1 = Convert.ToDateTime(localProvedbeniPlan.GetString("datumIzvida_page1", null));
            var vrijemeSat = localProvedbeniPlan.GetInt("vrijemeIzvidaSat_page1", 0);
            var vrijemeMinute = localProvedbeniPlan.GetInt("vrijemeIzvidaMinuta_page1", 0);

            DateTime vrijemeIzvida = new DateTime(datumInput_page1.Date.Year, datumInput_page1.Date.Month, datumInput_page1.Date.Day, vrijemeSat, vrijemeMinute, 0);

            var higijenskoStanjeDobro_page2 = localProvedbeniPlan.GetBoolean("higijenskoStanje_page1", false);
            var higijenskoStanjeOpis_page2 = localProvedbeniPlan.GetString("higijenskoStanjeOpis_page1", null);
            var gradevinskoStanjeDobro_page2 = localProvedbeniPlan.GetBoolean("gradevinskoStanje_page1", false);
            var gradevinskoStanjeOpis_page2 = localProvedbeniPlan.GetString("gradevinskoStanjeOpis_page1", null);
            var tehnickaOpremljenostDobro_page2 = localProvedbeniPlan.GetBoolean("tehnickaOpremljenost_page1", false);

            // PAGE 2
            var zapazanjaGlodavciInput_page2 = localProvedbeniPlan.GetString("zapazanjaGlodavciInput_page2", null);
            var zapazanjaInsektiInput_page2 = localProvedbeniPlan.GetString("zapazanjaInsektiInput_page2", null);
            var glodavciKriticneTockeInput_page2 = localProvedbeniPlan.GetString("glodavciKriticneTockeInput_page2", null);
            var insektiKriticneTockeInput_page2 = localProvedbeniPlan.GetString("insektiKriticneTockeInput_page2", null);
            var preporukeInput_page2 = localProvedbeniPlan.GetString("preporukeInput_page2", null);
            var napomeneInput_page2 = localProvedbeniPlan.GetString("napomeneInput_page2", null);
            var datumIzlaganjaG_page2 = Convert.ToDateTime(localProvedbeniPlan.GetString("datumIzlaganjaGlodavci_page2", null));
            var datumIzvidaG_page2 = Convert.ToDateTime(localProvedbeniPlan.GetString("datumIzvidaGlodavci_page2", null));
            var datumIzlaganjaI_page2 = Convert.ToDateTime(localProvedbeniPlan.GetString("datumIzlaganjaInsekti_page2", null));
            var datumIzvidaI_page2 = Convert.ToDateTime(localProvedbeniPlan.GetString("datumIzvidaInsekti_page2", null));


            // DODAVANJE PROVEDBENOG PLANA
            var provedbeniPlanId = -1;

            if(localProvedbeniPlan.GetInt("provedbeniPlanId", 0) != 0)
                provedbeniPlanId = localProvedbeniPlan.GetInt("provedbeniPlanId", 0);
            else
            {
                List<DID_ProvedbeniPlan> provedbeniPlan = db.Query<DID_ProvedbeniPlan>(
                    "SELECT * " +
                    "FROM DID_ProvedbeniPlan " +
                    "WHERE Id < 0 " +
                    "ORDER BY Id");

                if (provedbeniPlan.Any())
                    provedbeniPlanId = provedbeniPlan.FirstOrDefault().Id - 1;

                var dsds = db.Query<DID_ProvedbeniPlan>("SELECT * FROM DID_ProvedbeniPlan");

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
                        "SatZaProvedbuDezinfekcije, " +
                        "SinhronizacijaPrivremeniKljuc)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        provedbeniPlanId,
                        lokacijaId,
                        DateTime.Now.Date.Year,
                        datumInput_page1,
                        vrijemeIzvida,
                        nazivZaposlenika,
                        nazivZaposlenika,
                        higijenskoStanjeDobro_page2,
                        higijenskoStanjeOpis_page2,
                        gradevinskoStanjeDobro_page2,
                        gradevinskoStanjeOpis_page2,
                        tehnickaOpremljenostDobro_page2,
                        datumIzlaganjaG_page2,
                        datumIzvidaG_page2,
                        zapazanjaGlodavciInput_page2,
                        datumIzlaganjaI_page2,
                        datumIzvidaI_page2,
                        zapazanjaInsektiInput_page2,
                        glodavciKriticneTockeInput_page2,
                        insektiKriticneTockeInput_page2,
                        preporukeInput_page2,
                        napomeneInput_page2,
                        brojIzvidaGlodavacaGodisnje.Text,
                        rasporedIzvidaGlodavaca.Text,
                        brojOpcihDeratizacijaGodisnje.Text,
                        rasporedRadaDeratizacije.Text,
                        brojIzvidaInsekataGodisnje.Text,
                        rasporedIzvidaInsekata.Text,
                        brojOpcihDezinsekcijeGodisnje.Text,
                        rasporedRadaDezinsekcije.Text,
                        satDezinsekcija,
                        brojDanaDoDodatneKontroleInsekata.Text,
                        brojDanaDoDodatneDezinsekcije.Text,
                        brojOpcihDezinfekacijaGodisnje.Text,
                        rasporedRadaDezinfekacije.Text,
                        satDezinfekcija,
                        provedbeniPlanId);
            }


            // DODAVANJE MATERIJALA NA PROVEDBENI PLAN
            var materijalList = db.Query<T_NAZR>("SELECT * FROM T_NAZR");
            foreach (var materijal in materijalList)
            {
                if (!String.IsNullOrEmpty(localProvedbeniPlan.GetString("materijal" + materijal.NAZR_SIFRA, null)))
                {
                    int materijalId = -1;
                    var zadnjiId = db.Query<DID_ProvedbeniPlan_Materijal>(
                        "SELECT * " +
                        "FROM DID_ProvedbeniPlan_Materijal " +
                        "WHERE Id < 0 " +
                        "ORDER BY Id");

                    if (zadnjiId.Count > 0)
                        materijalId = zadnjiId.FirstOrDefault().Id - 1;

                    db.Execute(
                       "INSERT INTO DID_ProvedbeniPlan_Materijal (" +
                           "Id, " +
                           "ProvedbeniPlan, " +
                           "MaterijalSifra, " +
                           "MaterijalNaziv, " +
                           "TipKemikalije, " +
                           "SinhronizacijaPrivremeniKljuc)" +
                       "VALUES (?, ?, ?, ?, ?, ?)",
                           materijalId,
                           provedbeniPlanId,
                           materijal.NAZR_SIFRA,
                           materijal.NAZR_NAZIV,
                           materijal.TipKemikalije,
                           materijalId);
                }
            }


            // DODAVANJE NAMETNIKA NA PROVEDBENI PLAN
            var nametniciList = db.Query<DID_Nametnik>("SELECT * FROM DID_Nametnik");

            foreach(var nametnik in nametniciList)
            {
                if(!String.IsNullOrEmpty(localProvedbeniPlan.GetString("nametnik" + nametnik.Sifra, null)))
                {
                    int nametnikId = -1;
                    var zadnjiId = db.Query<DID_ProvedbeniPlan_Nametnik>(
                        "SELECT * " +
                        "FROM DID_ProvedbeniPlan_Nametnik " +
                        "WHERE Id < 0 " +
                        "ORDER BY Id");

                    if(zadnjiId.Count > 0)
                        nametnikId = zadnjiId.FirstOrDefault().Id - 1;

                    db.Execute(
                       "INSERT INTO DID_ProvedbeniPlan_Nametnik (" +
                           "Id, " +
                           "ProvedbeniPlan, " +
                           "NametnikSifra, " +
                           "NametnikNaziv, " +
                           "SinhronizacijaPrivremeniKljuc)" +
                       "VALUES (?, ?, ?, ?, ?)",
                           nametnikId,
                           provedbeniPlanId,
                           nametnik.Sifra,
                           nametnik.Naziv,
                           nametnikId);
                }
            }


            Toast.MakeText(this, "Provedbeni plan je spremljen!", ToastLength.Long).Show();
            intent = new Intent(this, typeof(Activity_Lokacije));
            StartActivity(intent);
        }

        public void OdustaniBtn_Click(object sender, EventArgs args)
        {
            Toast.MakeText(this, "Provedbeni plan nije spremljen!", ToastLength.Long).Show();
            intent = new Intent(this, typeof(Activity_Lokacije));
            StartActivity(intent);
        }

        public void BackBtn_Click(object sender, EventArgs args)
        {
            intent = new Intent(this, typeof(Activity_Lokacije));
            StartActivity(intent);
        }

        public void SaveState()
        {
            localProvedbeniPlanEdit.PutString("brojIzvidaGlodavacaGodisnje_page3", brojIzvidaGlodavacaGodisnje.Text);
            localProvedbeniPlanEdit.PutString("rasporedIzvidaGlodavaca_page3", rasporedIzvidaGlodavaca.Text);
            localProvedbeniPlanEdit.PutString("brojOpcihDeratizacijaGodisnje_page3", brojOpcihDeratizacijaGodisnje.Text);
            localProvedbeniPlanEdit.PutString("rasporedRadaDeratizacije_page3", rasporedRadaDeratizacije.Text); 
            localProvedbeniPlanEdit.PutString("brojIzvidaInsekataGodisnje_page3", brojIzvidaInsekataGodisnje.Text);
            localProvedbeniPlanEdit.PutString("rasporedIzvidaInsekata_page3", rasporedIzvidaInsekata.Text);
            localProvedbeniPlanEdit.PutString("rasporedRadaDezinsekcije_page3", rasporedRadaDezinsekcije.Text);
            localProvedbeniPlanEdit.PutString("brojDanaDoDodatneKontroleInsekata_page3", brojDanaDoDodatneKontroleInsekata.Text);
            localProvedbeniPlanEdit.PutString("brojDanaDoDodatneDezinsekcije_page3", brojDanaDoDodatneDezinsekcije.Text);
            localProvedbeniPlanEdit.PutString("brojOpcihDezinfekacijaGodisnje_page3", brojOpcihDezinfekacijaGodisnje.Text);
            localProvedbeniPlanEdit.PutString("rasporedRadaDezinfekacije_page3", rasporedRadaDezinfekacije.Text);
            localProvedbeniPlanEdit.PutInt("dezinsekcijaSat", dezinsekcijaSat);
            localProvedbeniPlanEdit.PutInt("dezinsekcijaMinuta", dezinsekcijaMinuta);
            localProvedbeniPlanEdit.PutInt("dezinfekcijaSat", dezinfekcijaSat);
            localProvedbeniPlanEdit.PutInt("dezinfekcijaMinuta", dezinfekcijaMinuta);

            localProvedbeniPlanEdit.PutBoolean("visited_page3", true);
            localProvedbeniPlanEdit.Commit();
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
            {
                SaveState();
                intent = new Intent(this, typeof(Activity_ProvedbeniPlan_page2));
            }
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