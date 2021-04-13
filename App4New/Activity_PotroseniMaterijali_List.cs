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
    [Activity(Label = "Activity_PotroseniMaterijali_List", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_PotroseniMaterijali_List : Activity
    {
        LinearLayout izradaPotvrde;
        List<DID_StanjeSkladista> materijaliListaSkladiste, materijaliListaSkladisteFiltered;
        EditText searchInput;
        TextView resultMessage;
        Intent intent;
        Button potvrdaBtn, noviMaterijalBtn;
        RecyclerView materijaliListView;
        RecyclerView.LayoutManager mLayoutManager;
        Adapter_MatrijalSkladisteRecycleView mAdapter;
        int radniNalog, lokacijaId;
        string skladiste;

        static readonly ISharedPreferences localOdradeneAnkete = Application.Context.GetSharedPreferences("ankete", FileCreationMode.Private);
        static readonly ISharedPreferences localMaterijali = Application.Context.GetSharedPreferences("localMaterijali", FileCreationMode.Private);
        static ISharedPreferencesEditor localMaterijaliEdit = localMaterijali.Edit();
        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);
        static readonly ISharedPreferences localKomitentLokacija = Application.Context.GetSharedPreferences("localKomitentLokacija", FileCreationMode.Private);
        static readonly ISharedPreferences localPotvrda = Application.Context.GetSharedPreferences("potvrda", FileCreationMode.Private);
        static ISharedPreferencesEditor localPotvrdaEdit = localPotvrda.Edit();

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.potroseniMaterijali_Lista);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            materijaliListView = FindViewById<RecyclerView>(Resource.Id.materijaliListView);
            searchInput = FindViewById<EditText>(Resource.Id.searchInput);
            resultMessage = FindViewById<TextView>(Resource.Id.resultMessage);
            potvrdaBtn = FindViewById<Button>(Resource.Id.potvrdaBtn);
            noviMaterijalBtn = FindViewById<Button>(Resource.Id.noviMaterijalBtn);
            izradaPotvrde = FindViewById<LinearLayout>(Resource.Id.izradaPotvrde);

            SetActionBar(toolbar);
            ActionBar.Title = "Odabir materijala";
            radniNalog = localRadniNalozi.GetInt("id", 0);
            lokacijaId = localKomitentLokacija.GetInt("lokacijaId", 0);
            potvrdaBtn.Click += PotvrdaBtn_Click;
            noviMaterijalBtn.Click += NoviMaterijalBtn_Click;
            searchInput.KeyPress += SearchInput_KeyPress;
            skladiste = db.Query<DID_RadniNalog>(
                "SELECT * " +
                "FROM DID_RadniNalog " +
                "WHERE Id = ?", radniNalog).FirstOrDefault().PokretnoSkladiste;

            if (!localOdradeneAnkete.GetBoolean("visited", false))
                potvrdaBtn.Visibility = Android.Views.ViewStates.Visible;
            else
                HidePotvrdaButton();

            if (!localMaterijali.GetBoolean("materijaliPoPoziciji", false))
                ProvjeriIzdavanjePotvrde();
            else
                HidePotvrdaButton();

            //Prikaz svih materijala na skladistu
            materijaliListaSkladiste = db.Query<DID_StanjeSkladista>(
               "SELECT * " +
               "FROM DID_StanjeSkladista " +
               "WHERE Skladiste = ? " +
               "GROUP BY MaterijalNaziv", skladiste);

            materijaliListaSkladisteFiltered = materijaliListaSkladiste;

            if (materijaliListaSkladisteFiltered.Any())
            {
                mLayoutManager = new LinearLayoutManager(this);
                mAdapter = new Adapter_MatrijalSkladisteRecycleView(materijaliListaSkladisteFiltered);
                materijaliListView.SetLayoutManager(mLayoutManager);
                mAdapter.ItemClick += MAdapter_ItemClick;
                materijaliListView.SetAdapter(mAdapter);
                resultMessage.Visibility = Android.Views.ViewStates.Gone;
            }
            else
            {
                resultMessage.Text = "Nema dostupnih materijala na skladištu!";
                resultMessage.Visibility = Android.Views.ViewStates.Visible;
                Window.SetSoftInputMode(SoftInput.StateAlwaysHidden);
            }

            searchInput.TextChanged += delegate
            {
                string input = searchInput.Text.ToLower();
                if (!string.IsNullOrEmpty(input))
                {
                    resultMessage.Visibility = Android.Views.ViewStates.Invisible;
                    materijaliListaSkladisteFiltered = materijaliListaSkladiste.Where(i =>
                        (i.Materijal != null && i.Materijal.ToLower().Contains(input)) ||
                        (i.MaterijalNaziv != null && i.MaterijalNaziv.ToLower().Contains(input))).ToList();

                    if (materijaliListaSkladisteFiltered.Any())
                    {
                        mLayoutManager = new LinearLayoutManager(this);
                        mAdapter = new Adapter_MatrijalSkladisteRecycleView(materijaliListaSkladisteFiltered);
                        materijaliListView.SetLayoutManager(mLayoutManager);
                        mAdapter.ItemClick += MAdapter_ItemClick;
                        materijaliListView.SetAdapter(mAdapter);
                        resultMessage.Visibility = Android.Views.ViewStates.Gone;
                    }
                    else
                    {
                        mLayoutManager = new LinearLayoutManager(this);
                        mAdapter = new Adapter_MatrijalSkladisteRecycleView(materijaliListaSkladisteFiltered);
                        materijaliListView.SetLayoutManager(mLayoutManager);
                        mAdapter.ItemClick += MAdapter_ItemClick;
                        materijaliListView.SetAdapter(mAdapter);
                        resultMessage.Text = "Nije pronađen materijal sa unesenim pojmom!";
                        resultMessage.Visibility = Android.Views.ViewStates.Visible;
                    }
                }
                else
                {
                    materijaliListaSkladisteFiltered = materijaliListaSkladiste;

                    if (materijaliListaSkladisteFiltered.Any())
                    {
                        mLayoutManager = new LinearLayoutManager(this);
                        mAdapter = new Adapter_MatrijalSkladisteRecycleView(materijaliListaSkladisteFiltered);
                        materijaliListView.SetLayoutManager(mLayoutManager);
                        mAdapter.ItemClick += MAdapter_ItemClick;
                        materijaliListView.SetAdapter(mAdapter);
                        resultMessage.Visibility = Android.Views.ViewStates.Gone;
                    }
                    else
                    {
                        resultMessage.Text = "Nema dostupnih materijala na skladištu!";
                        resultMessage.Visibility = Android.Views.ViewStates.Visible;
                        Window.SetSoftInputMode(SoftInput.StateAlwaysHidden);
                        mLayoutManager = new LinearLayoutManager(this);
                        mAdapter = new Adapter_MatrijalSkladisteRecycleView(materijaliListaSkladisteFiltered);
                        materijaliListView.SetLayoutManager(mLayoutManager);
                        mAdapter.ItemClick += MAdapter_ItemClick;
                        materijaliListView.SetAdapter(mAdapter);
                        resultMessage.Visibility = Android.Views.ViewStates.Gone;
                    }
                }
            };
        }

        public void SearchInput_KeyPress(object sender, View.KeyEventArgs e)
        {
            e.Handled = false;
            if (e.KeyCode == Keycode.Enter)
                e.Handled = true;
        }

        public void HidePotvrdaButton()
        {
            izradaPotvrde.Visibility = Android.Views.ViewStates.Gone;
            RelativeLayout.LayoutParams imgViewParams = new RelativeLayout.LayoutParams(140, 140);
            imgViewParams.AddRule(LayoutRules.AlignParentBottom);
            imgViewParams.AddRule(LayoutRules.AlignParentRight);
            imgViewParams.SetMargins(10, 10, 20, 20);
            noviMaterijalBtn.LayoutParameters = imgViewParams;
        }

        public void ProvjeriIzdavanjePotvrde()
        {
            int radniNalogLokacijaId = localKomitentLokacija.GetInt("radniNalogLokacijaId", 0);
            List<DID_Potvrda> potvrda = db.Query<DID_Potvrda>(
                "SELECT * " +
                "FROM DID_Potvrda " +
                "WHERE RadniNalogLokacijaId = ?", radniNalogLokacijaId);

            if (potvrda.Any())
            {
                izradaPotvrde.Visibility = Android.Views.ViewStates.Visible;
                potvrdaBtn.Text = "Prikaži potvrdu";

                string infestacija = db.Query<DID_RazinaInfestacije>(
                    "SELECT * " +
                    "FROM DID_RazinaInfestacije " +
                    "WHERE Sifra = ?", potvrda.FirstOrDefault().Infestacija).FirstOrDefault().Naziv;

                //Potvrda str.1
                localPotvrdaEdit.PutBoolean("potvrdaPage1", true);
                localPotvrdaEdit.PutString("godina", potvrda.FirstOrDefault().Godina);
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

                List<DID_Potvrda_Djelatnost> djelatnostSuzbijanjeStetnika = db.Query<DID_Potvrda_Djelatnost>(
                   "SELECT * " +
                   "FROM DID_Potvrda_Djelatnost " +
                   "WHERE Potvrda = ? " +
                   "AND Djelatnost = ?", potvrda.FirstOrDefault().Id, 6);

                List<DID_Potvrda_Djelatnost> djelatnostKIInsekti = db.Query<DID_Potvrda_Djelatnost>(
                   "SELECT * " +
                   "FROM DID_Potvrda_Djelatnost " +
                   "WHERE Potvrda = ? " +
                   "AND Djelatnost = ?", potvrda.FirstOrDefault().Id, 7);

                List<DID_Potvrda_Djelatnost> djelatnostKIGlodavci = db.Query<DID_Potvrda_Djelatnost>(
                   "SELECT * " +
                   "FROM DID_Potvrda_Djelatnost " +
                   "WHERE Potvrda = ? " +
                   "AND Djelatnost = ?", potvrda.FirstOrDefault().Id, 8);

                List<DID_Potvrda_Djelatnost> djelatnostuzimanjeBriseva = db.Query<DID_Potvrda_Djelatnost>(
                   "SELECT * " +
                   "FROM DID_Potvrda_Djelatnost " +
                   "WHERE Potvrda = ? " +
                   "AND Djelatnost = ?", potvrda.FirstOrDefault().Id, 9);

                List<DID_Potvrda_Djelatnost> djelatnostsuzbijanjeKorova = db.Query<DID_Potvrda_Djelatnost>(
                   "SELECT * " +
                   "FROM DID_Potvrda_Djelatnost " +
                   "WHERE Potvrda = ? " +
                   "AND Djelatnost = ?", potvrda.FirstOrDefault().Id, 10);

                List<DID_Potvrda_Djelatnost> djelatnostkosnjaTrave = db.Query<DID_Potvrda_Djelatnost>(
                   "SELECT * " +
                   "FROM DID_Potvrda_Djelatnost " +
                   "WHERE Potvrda = ? " +
                   "AND Djelatnost = ?", potvrda.FirstOrDefault().Id, 11);

                localPotvrdaEdit.PutBoolean("dezodorizacijaBtn", djelatnostDezodorizacija.Any() ? true : false);
                localPotvrdaEdit.PutBoolean("suzbijanjeStetnikaBtn", djelatnostSuzbijanjeStetnika.Any() ? true : false);
                localPotvrdaEdit.PutBoolean("KIInsektiBtn", djelatnostKIInsekti.Any() ? true : false);
                localPotvrdaEdit.PutBoolean("KIGlodavciBtn", djelatnostKIGlodavci.Any() ? true : false);
                localPotvrdaEdit.PutBoolean("uzimanjeBrisevaBtn", djelatnostuzimanjeBriseva.Any() ? true : false);
                localPotvrdaEdit.PutBoolean("suzbijanjeKorovaBtn", djelatnostsuzbijanjeKorova.Any() ? true : false);
                localPotvrdaEdit.PutBoolean("kosnjaTraveBtn", djelatnostkosnjaTrave.Any() ? true : false);
                localPotvrdaEdit.PutBoolean("potvrdaPage2", true);

                //Potvrda str.3
                List<DID_Potvrda_Nametnik> nametnici = db.Query<DID_Potvrda_Nametnik>(
                    "SELECT * " +
                    "FROM DID_Potvrda_Nametnik " +
                    "WHERE Potvrda = ?", potvrda.FirstOrDefault().Id);

                foreach (var nametnik in nametnici)
                {
                    if (nametnik.Nametnik == "11")
                        localPotvrdaEdit.PutBoolean("mis", true);
                    if (nametnik.Nametnik == "12")
                        localPotvrdaEdit.PutBoolean("stakor", true);
                    if (nametnik.Nametnik == "13")
                        localPotvrdaEdit.PutBoolean("ostaliGlodavci", true);
                    if (nametnik.Nametnik == "21")
                        localPotvrdaEdit.PutBoolean("muha", true);
                    if (nametnik.Nametnik == "210")
                        localPotvrdaEdit.PutBoolean("ostaliInsekti", true);
                    if (nametnik.Nametnik == "22")
                        localPotvrdaEdit.PutBoolean("buha", true);
                    if (nametnik.Nametnik == "23")
                        localPotvrdaEdit.PutBoolean("zohar", true);
                    if (nametnik.Nametnik == "24")
                        localPotvrdaEdit.PutBoolean("mrav", true);
                    if (nametnik.Nametnik == "25")
                        localPotvrdaEdit.PutBoolean("komarac", true);
                    if (nametnik.Nametnik == "26")
                        localPotvrdaEdit.PutBoolean("stjenica", true);
                    if (nametnik.Nametnik == "27")
                        localPotvrdaEdit.PutBoolean("us", true);
                    if (nametnik.Nametnik == "28")
                        localPotvrdaEdit.PutBoolean("osa", true);
                    if (nametnik.Nametnik == "29")
                        localPotvrdaEdit.PutBoolean("strsljen", true);
                    if (nametnik.Nametnik == "30")
                        localPotvrdaEdit.PutBoolean("zmije", true);
                    if (nametnik.Nametnik == "31")
                        localPotvrdaEdit.PutBoolean("bakterije", true);
                    if (nametnik.Nametnik == "32")
                        localPotvrdaEdit.PutBoolean("komaracAdulti", true);
                    if (nametnik.Nametnik == "33")
                        localPotvrdaEdit.PutBoolean("moljci", true);
                }
                localPotvrdaEdit.PutBoolean("potvrdaPage3", true);
                localPotvrdaEdit.PutInt("id", potvrda.FirstOrDefault().Id);
                localPotvrdaEdit.PutBoolean("edit", true);
                localPotvrdaEdit.Commit();
            }
            else
            {
                DID_Lokacija lokacija = db.Query<DID_Lokacija>(
                    "SELECT * " +
                    "FROM DID_Lokacija " +
                    "WHERE DID_Lokacija.SAN_Id = ?", lokacijaId).FirstOrDefault();

                List<DID_LokacijaPozicija> pozicijeOdradene = db.Query<DID_LokacijaPozicija>(
                   "SELECT * " +
                   "FROM DID_LokacijaPozicija " +
                   "INNER JOIN DID_Anketa ON DID_LokacijaPozicija.POZ_Id = DID_Anketa.ANK_POZ_Id " +
                   "WHERE DID_Anketa.ANK_RadniNalog = ? " +
                   "AND DID_LokacijaPozicija.SAN_Id = ?", radniNalog, lokacijaId);

                List<DID_LokacijaPozicija> pozicijeUkupno = db.Query<DID_LokacijaPozicija>(
                    "SELECT * " +
                    "FROM DID_LokacijaPozicija " +
                    "INNER JOIN DID_RadniNalog_Lokacija ON DID_LokacijaPozicija.SAN_Id = DID_RadniNalog_Lokacija.Lokacija " +
                    "WHERE DID_RadniNalog_Lokacija.RadniNalog = ? " +
                    "AND DID_LokacijaPozicija.SAN_Id = ?", radniNalog, lokacijaId);

                // AKo je lokacija oznacena kao da ne postoje pozicije onda odma mos kreirati potvrdu
                if (pozicijeOdradene.Count == pozicijeUkupno.Count || !lokacija.SAN_AnketePoPozicijama)
                    izradaPotvrde.Visibility = Android.Views.ViewStates.Visible;
                else
                    HidePotvrdaButton();
            }
        }

        public override void OnBackPressed()
        {
            DID_Lokacija lokacija = db.Query<DID_Lokacija>(
                "SELECT * " +
                "FROM DID_Lokacija " +
                "WHERE SAN_Id = ?", lokacijaId).FirstOrDefault();

            if (!lokacija.SAN_AnketePoPozicijama)
            {
                intent = new Intent(this, typeof(Activity_Lokacije));
                StartActivity(intent);
            }
            else if (!localMaterijali.GetBoolean("materijaliPoPoziciji", false))
            {
                intent = new Intent(this, typeof(Activity_Pozicije));
                StartActivity(intent);
            }
            else
            {
                intent = new Intent(this, typeof(Activity_PotroseniMaterijali_Pozicija));
                StartActivity(intent);
            }
        }

        public void NoviMaterijalBtn_Click(object sender, EventArgs args)
        {
            localMaterijaliEdit.PutBoolean("checkBox", true);
            localMaterijaliEdit.Commit();
            intent = new Intent(this, typeof(Activity_PotroseniMaterijali_Novo_Lista));
            StartActivity(intent);
        }

        public void PotvrdaBtn_Click(object sender, EventArgs args)
        {
            intent = new Intent(this, typeof(Activity_Potvrda_page1));
            StartActivity(intent);
        }

        public void MAdapter_ItemClick(object sender, int e)
        {
            localMaterijaliEdit.PutString("sifra", materijaliListaSkladisteFiltered[e].Materijal);
            localMaterijaliEdit.PutInt("mjernaJedinicaId", materijaliListaSkladisteFiltered[e].MjernaJedinica);
            localMaterijaliEdit.PutBoolean("checkBox", true);
            localMaterijaliEdit.Commit();
            intent = new Intent(this, typeof(Activity_PotroseniMaterijali_Dodaj));
            StartActivity(intent);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            DID_Lokacija lokacija = db.Query<DID_Lokacija>(
                "SELECT * " +
                "FROM DID_Lokacija " +
                "WHERE SAN_Id = ?", lokacijaId).FirstOrDefault();

            if(!lokacija.SAN_AnketePoPozicijama)
                MenuInflater.Inflate(Resource.Menu.menu, menu);
            else if (localOdradeneAnkete.GetBoolean("visited", false) || localMaterijali.GetBoolean("materijaliPoPoziciji", false))
                MenuInflater.Inflate(Resource.Menu.menuClose, menu);
            else
                MenuInflater.Inflate(Resource.Menu.menuNextHome, menu);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            DID_Lokacija lokacija = db.Query<DID_Lokacija>(
                "SELECT * " +
                "FROM DID_Lokacija " +
                "WHERE SAN_Id = ?", lokacijaId).FirstOrDefault();

            //localMaterijaliEdit.Clear().Commit();
            if (!lokacija.SAN_AnketePoPozicijama)
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
            }
            else if (localOdradeneAnkete.GetBoolean("visited", false) || localMaterijali.GetBoolean("materijaliPoPoziciji", false))
            {
                if (item.TitleFormatted.ToString() == "close")
                {
                    // Ako dolazi sa popisa materijala na poziciji bi trebao ici na ovaj activity
                    //intent = new Intent(this, typeof(Activity_PotroseniMaterijali_Pozicija));

                    intent = new Intent(this, typeof(Activity_PotroseniMaterijali));
                    StartActivity(intent);
                }
            }
            else
            {
                if (item.TitleFormatted.ToString() == "Početna")
                    intent = new Intent(this, typeof(Activity_Pocetna));
                else if (item.TitleFormatted.ToString() == "naprijed")
                    intent = new Intent(this, typeof(Activity_Pozicije));
                else if (item.TitleFormatted.ToString() == "Radni nalozi")
                    intent = new Intent(this, typeof(Activity_RadniNalozi));
                else if (item.TitleFormatted.ToString() == "Prikaz odradenih anketa")
                    intent = new Intent(this, typeof(Activity_OdradeneAnkete));
                else if (item.TitleFormatted.ToString() == "Prikaz potrošenih materijala")
                    intent = new Intent(this, typeof(Activity_PotroseniMaterijali));

                StartActivity(intent);
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}