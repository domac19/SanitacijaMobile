using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Infobit.DDD.Data;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Support.V7.App;
using Android.Content.PM;
namespace App4New
{
    [Activity(Label = "Activity_NovaLokacija", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_NovaLokacija : Activity
    {   
        List<DID_Lokacija> lokacijeList, lokacijeListFiltered;
        EditText searchInput, nazivInput, mjestoInput, ulicaBrInput;
        TextView resultMessage, opcinaData, msgNaziv, msgMjesto, msgAdresa;
        Intent intent;
        ListView lokacijaListView;
        LinearLayout poznataLokLayout;
        ScrollView novaLokLayout;
        RadioButton poznataLokBtn, novaLokBtn;
        Button spremiBtn, odustaniBtn, odaberiOpcinuBtn;
        Spinner spinnerTipLokacije, spinnerNaselja;
        CheckBox anketePoPozicijama;
        bool flag;
        string partnerOIB, sifraOpcine, sifraKomitenta = localKomitentLokacija.GetString("sifraKomitenta", null);
        readonly int radniNalog = localRadniNalozi.GetInt("id", 0);

        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);
        static readonly ISharedPreferences localKomitentLokacija = Application.Context.GetSharedPreferences("localKomitentLokacija", FileCreationMode.Private);
        static ISharedPreferencesEditor localKomitentLokacijaEdit = localKomitentLokacija.Edit();
        static readonly ISharedPreferences localUsername = Application.Context.GetSharedPreferences("userLogin", FileCreationMode.Private);
        static readonly ISharedPreferences localPozicija = Application.Context.GetSharedPreferences("pozicija", FileCreationMode.Private);
        static ISharedPreferencesEditor localPozicijaEdit = localPozicija.Edit();


        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.novaLokacija);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            searchInput = FindViewById<EditText>(Resource.Id.searchInput);
            resultMessage = FindViewById<TextView>(Resource.Id.resultMessage);
            lokacijaListView = FindViewById<ListView>(Resource.Id.lokacijaListView);
            novaLokLayout = FindViewById<ScrollView>(Resource.Id.novaLokLayout);
            poznataLokLayout = FindViewById<LinearLayout>(Resource.Id.poznataLokLayout);
            novaLokBtn = FindViewById<RadioButton>(Resource.Id.novaLokBtn);
            poznataLokBtn = FindViewById<RadioButton>(Resource.Id.poznataLokBtn);
            nazivInput = FindViewById<EditText>(Resource.Id.nazivInput);
            ulicaBrInput = FindViewById<EditText>(Resource.Id.ulicaBrInput);
            mjestoInput = FindViewById<EditText>(Resource.Id.mjestoInput);
            spremiBtn = FindViewById<Button>(Resource.Id.spremiBtn);
            odustaniBtn = FindViewById<Button>(Resource.Id.odustaniBtn);
            odaberiOpcinuBtn = FindViewById<Button>(Resource.Id.odaberiOpcinuBtn);
            opcinaData = FindViewById<TextView>(Resource.Id.opcinaData);
            msgNaziv = FindViewById<TextView>(Resource.Id.msgNaziv);
            msgMjesto = FindViewById<TextView>(Resource.Id.msgMjesto);
            msgAdresa = FindViewById<TextView>(Resource.Id.msgAdresa);
            spinnerTipLokacije = FindViewById<Spinner>(Resource.Id.spinnerTipLokacije);
            spinnerNaselja = FindViewById<Spinner>(Resource.Id.spinnerNaselja);
            anketePoPozicijama = FindViewById<CheckBox>(Resource.Id.anketePoPozicijama);

            SetActionBar(toolbar);
            ActionBar.Title = "Dodavanje lokacije";
            lokacijaListView.ItemClick += LokacijaListView_ItemClick;
            spremiBtn.Click += SpremiBtn_Click;
            odustaniBtn.Click += OdustaniBtn_Click;
            nazivInput.TextChanged += NazivInput_TextChanged;
            ulicaBrInput.TextChanged += UlicaBrInput_TextChanged;
            mjestoInput.TextChanged += MjestoInput_TextChanged;
            odaberiOpcinuBtn.Click += OdaberiOpcinuBtn_Click;
            novaLokBtn.Click += NovaLokBtn_Click;
            poznataLokBtn.Click += PoznataLokBtn_Click;
            searchInput.KeyPress += SearchInput_KeyPress;
            lokacijeList = db.Query<DID_Lokacija>(
                "SELECT * " +
                "FROM DID_Lokacija " +
                "LEFT JOIN DID_RadniNalog_Lokacija ON DID_Lokacija.SAN_Id = DID_RadniNalog_Lokacija.Lokacija " +
                "WHERE IFNULL(DID_RadniNalog_Lokacija.RadniNalog, 0) != ? " +
                "AND DID_Lokacija.SAN_KD_Sifra = ? " +
                "GROUP BY SAN_Naziv, SAN_Id", radniNalog, sifraKomitenta);

            lokacijeListFiltered = lokacijeList;

            if (!lokacijeListFiltered.Any() || localKomitentLokacija.GetBoolean("prikazNovaLokacija", false))
                novaLokBtn.PerformClick();
            else
                poznataLokBtn.PerformClick();
        }

        public void SearchInput_KeyPress(object sender, View.KeyEventArgs e)
        {
            e.Handled = false;
            if (e.KeyCode == Keycode.Enter)
                e.Handled = true;
        }

        public void NazivInput_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(nazivInput.Text) || nazivInput.Text.Length > 100)
                msgNaziv.Visibility = Android.Views.ViewStates.Visible;
            else
                msgNaziv.Visibility = Android.Views.ViewStates.Invisible;
        }

        public void UlicaBrInput_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(ulicaBrInput.Text) || ulicaBrInput.Text.Length > 100)
                msgAdresa.Visibility = Android.Views.ViewStates.Visible;
            else
                msgAdresa.Visibility = Android.Views.ViewStates.Invisible;
        }

        public void MjestoInput_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(mjestoInput.Text) || mjestoInput.Text.Length > 50)
                msgMjesto.Visibility = Android.Views.ViewStates.Visible;
            else
                msgMjesto.Visibility = Android.Views.ViewStates.Invisible;
        }

        public void NovaLokBtn_Click(object sender, EventArgs args)
        {
            poznataLokLayout.Visibility = Android.Views.ViewStates.Gone;
            novaLokLayout.Visibility = Android.Views.ViewStates.Visible;
            nazivInput.RequestFocus();

            List<DID_TipLokacije> tipoviLokacije = db.Query<DID_TipLokacije>(
                "SELECT * " +
                "FROM DID_TipLokacije " +
                "WHERE Sifra != 0");
            List<string> tipoviList = new List<string>();
            foreach (var item in tipoviLokacije)
                tipoviList.Add(item.Naziv);
            ArrayAdapter<string> adapterTipoviList = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, tipoviList);
            adapterTipoviList.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinnerTipLokacije.Adapter = adapterTipoviList;

            partnerOIB = db.Query<T_KUPDOB>(
                "SELECT * " +
                "FROM T_KUPDOB " +
                "WHERE SIFRA = ?", sifraKomitenta).FirstOrDefault().OIB;

            if (localKomitentLokacija.GetBoolean("opcineVisited", false))
            {
                nazivInput.Text = localKomitentLokacija.GetString("nazivLokacije", null);
                mjestoInput.Text = localKomitentLokacija.GetString("mjestoLokacije", null);
                ulicaBrInput.Text = localKomitentLokacija.GetString("ulicaLokacije", null);
                opcinaData.Text = localKomitentLokacija.GetString("nazivOpcine", null);
                sifraOpcine = localKomitentLokacija.GetString("sifraOpcine", null);
            }
            else
            {
                T_OPCINE opcina = db.Query<T_OPCINE>(
                    "SELECT * " +
                    "FROM T_OPCINE " +
                    "GROUP BY Naziv").FirstOrDefault();
                string nazivZupanije = db.Query<T_ZUPANIJE>(
                    "SELECT * " +
                    "FROM T_ZUPANIJE " +
                    "WHERE Sifra = ?", opcina.Zupanija).FirstOrDefault().Naziv;

                opcinaData.Text = opcina.Naziv + " / " + nazivZupanije;
                sifraOpcine = opcina.Sifra;
            }

            List<T_NASELJA> naselja = db.Query<T_NASELJA>(
                "SELECT * " +
                "FROM T_NASELJA " +
                "WHERE Opcina = ? " +
                "GROUP BY Naziv", sifraOpcine);
            List<string> naseljaList = new List<string>();
            foreach (var item in naselja)
                naseljaList.Add(item.Naziv);
            naseljaList.Add("");
            ArrayAdapter<string> adapterNaseljaList = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, naseljaList);
            adapterNaseljaList.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinnerNaselja.Adapter = adapterNaseljaList;
        }

        public void PoznataLokBtn_Click(object sender, EventArgs args)
        {
            poznataLokLayout.Visibility = Android.Views.ViewStates.Visible;
            novaLokLayout.Visibility = Android.Views.ViewStates.Gone;
            PrikaziLokaciju();

            searchInput.TextChanged += delegate
            {
                string input = searchInput.Text.ToLower();
                if (!string.IsNullOrEmpty(input))
                {
                    resultMessage.Visibility = Android.Views.ViewStates.Invisible;
                    lokacijeListFiltered = lokacijeList.Where(i =>
                        (i.SAN_UlicaBroj != null && i.SAN_UlicaBroj.ToLower().Contains(input)) ||
                        (i.SAN_Mjesto != null && i.SAN_Mjesto.ToLower().Contains(input)) ||
                        (i.SAN_Naziv != null && i.SAN_Naziv.ToLower().Contains(input))).ToList();

                    lokacijaListView.Adapter = new Adapter_Lokacija(this, lokacijeListFiltered);

                    if (!lokacijeListFiltered.Any())
                    {
                        lokacijaListView.Adapter = new Adapter_Lokacija(this, lokacijeListFiltered);
                        resultMessage.Text = "Nije pronađena lokacija sa unesenim pojmom!";
                        resultMessage.Visibility = Android.Views.ViewStates.Visible;
                    }
                }
                else
                    PrikaziLokaciju();
            };
        }

        public void PrikaziLokaciju()
        {
            lokacijeListFiltered = lokacijeList;

            if (lokacijeListFiltered.Any())
            {
                lokacijaListView.Adapter = new Adapter_Lokacija(this, lokacijeListFiltered);
                resultMessage.Visibility = Android.Views.ViewStates.Gone;
            }
            else
            {
                resultMessage.Text = "Nema postojećih lokacija!";
                resultMessage.Visibility = Android.Views.ViewStates.Visible;
            }
        }

        public void OdaberiOpcinuBtn_Click(object sender, EventArgs e)
        {
            localKomitentLokacijaEdit.PutString("nazivLokacije", nazivInput.Text);
            localKomitentLokacijaEdit.PutString("mjestoLokacije", mjestoInput.Text);
            localKomitentLokacijaEdit.PutString("ulicaLokacije", ulicaBrInput.Text);
            localKomitentLokacijaEdit.Commit();
            intent = new Intent(this, typeof(Activity_NovaLokacija_OpcinaList));
            StartActivity(intent);
        }

        public void OdustaniBtn_Click(object sender, EventArgs e)
        {
            intent = new Intent(this, typeof(Activity_Pocetna));
            StartActivity(intent);
        }

        public override void OnBackPressed()
        {
            if (localRadniNalozi.GetBoolean("noviRN", false))
                intent = new Intent(this, typeof(Activity_NoviKomitent));
            else
                intent = new Intent(this, typeof(Activity_Lokacije));
            StartActivity(intent);
        }

        public void SpremiBtn_Click(object sender, EventArgs e)
        {
            flag = true;
            if (string.IsNullOrEmpty(ulicaBrInput.Text) || ulicaBrInput.Text.Length > 100)
            {
                msgAdresa.Visibility = Android.Views.ViewStates.Visible;
                ulicaBrInput.RequestFocus();
                flag = false;
            }
            if (string.IsNullOrEmpty(mjestoInput.Text) || mjestoInput.Text.Length > 50)
            {
                msgMjesto.Visibility = Android.Views.ViewStates.Visible;
                mjestoInput.RequestFocus();
                flag = false;
            }
            if (string.IsNullOrEmpty(nazivInput.Text) || nazivInput.Text.Length > 100)
            {
                msgNaziv.Visibility = Android.Views.ViewStates.Visible;
                nazivInput.RequestFocus();
                flag = false;
            }

            if (flag)
            {
                string naselje;
                int id = -1, RNLokacijeId = -1;
                string spinnerTip = spinnerTipLokacije.SelectedItem.ToString();
                int tipLokacije = db.Query<DID_TipLokacije>(
                    "SELECT * " +
                    "FROM DID_TipLokacije " +
                    "WHERE Naziv = ?", spinnerTip).FirstOrDefault().Sifra;

                List<DID_Lokacija> lokacije = db.Query<DID_Lokacija>(
                    "SELECT * " +
                    "FROM DID_Lokacija " +
                    "WHERE SAN_Id < 0 " +
                    "ORDER BY SAN_Id");
                if (lokacije.Any())
                    id = lokacije.FirstOrDefault().SAN_Id - 1;

                List<DID_RadniNalog_Lokacija> radniNalogLokacije = db.Query<DID_RadniNalog_Lokacija>(
                    "SELECT * " +
                    "FROM DID_RadniNalog_Lokacija " +
                    "WHERE Id < 0 " +
                    "ORDER BY Id");
                if (radniNalogLokacije.Any())
                    RNLokacijeId = radniNalogLokacije.FirstOrDefault().Id - 1;

                List<T_NASELJA> naseljaList = db.Query<T_NASELJA>(
                    "SELECT * " +
                    "FROM T_NASELJA " +
                    "WHERE Naziv = ?", spinnerNaselja.SelectedItem.ToString());
                if (!naseljaList.Any())
                    naselje = "";
                else
                    naselje = naseljaList.FirstOrDefault().Sifra;

                string tipPartnera = db.Query<T_KUPDOB>(
                    "SELECT * " +
                    "FROM T_KUPDOB " +
                    "WHERE SIFRA = ?", sifraKomitenta).FirstOrDefault().TIP_PARTNERA;

                db.Query<DID_Lokacija>(
                    "INSERT INTO DID_Lokacija (" +
                        "SAN_Id, " +
                        "SAN_Naziv, " +
                        "SAN_Mjesto, " +
                        "SAN_Naselje, " +
                        "SAN_GradOpcina, " +
                        "SAN_UlicaBroj, " +
                        "SAN_OIBPartner, " +
                        "SAN_KD_Sifra, " +
                        "SAN_Tip, " +
                        "SAN_Tip2," +
                        "SinhronizacijaPrivremeniKljuc, " +
                        "SAN_TipPartnera, " +
                        "SAN_AnketePoPozicijama, " +
                        "SAN_Status)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        id,
                        nazivInput.Text,
                        mjestoInput.Text,
                        naselje,
                        sifraOpcine,
                        ulicaBrInput.Text,
                        partnerOIB,
                        sifraKomitenta,
                        tipLokacije,
                        2,
                        id,
                        tipPartnera,
                        anketePoPozicijama.Checked,
                        0
                );

                db.Query<DID_RadniNalog_Lokacija>(
                    "INSERT INTO DID_RadniNalog_Lokacija (" +
                        "Id, " +
                        "Status, " +
                        "RadniNalog, " +
                        "Lokacija, " +
                        "VrijemeDolaska, " +
                        "TipAkcije, " +
                        "SinhronizacijaPrivremeniKljuc)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?)",
                        RNLokacijeId,
                        1,
                        radniNalog,
                        id,
                        DateTime.Now,
                        1,
                        RNLokacijeId
                );

                List<DID_RadniNalog_Lokacija> izvrseneLokacije = db.Query<DID_RadniNalog_Lokacija>(
                    "SELECT * " +
                    "FROM DID_RadniNalog_Lokacija " +
                    "WHERE RadniNalog = ? " +
                    "AND Status == 3", radniNalog);

                if (izvrseneLokacije.Any())
                {
                    db.Query<DID_RadniNalog>(
                        "UPDATE DID_RadniNalog " +
                        "SET Status = ? " +
                        "WHERE Id = ?", 4, radniNalog);
                }
                else
                {
                    db.Query<DID_RadniNalog>(
                        "UPDATE DID_RadniNalog " +
                        "SET Status = ? " +
                        "WHERE Id = ?", 3, radniNalog);
                }

                DID_RadniNalog radniNalogObject = db.Query<DID_RadniNalog>(
                    "SELECT * " +
                    "FROM DID_RadniNalog " +
                    "WHERE Id = ?", radniNalog).FirstOrDefault();

                if (radniNalogObject.SinhronizacijaStatus == 2 && izvrseneLokacije.Any())
                {
                    db.Query<DID_RadniNalog>(
                        "UPDATE DID_RadniNalog " +
                        "SET SinhronizacijaStatus = 1 " +
                        "WHERE Id = ?", radniNalog);
                }

                localKomitentLokacijaEdit.PutInt("radniNalogLokacijaId", RNLokacijeId);
                localKomitentLokacijaEdit.PutInt("lokacijaId", id);
                localKomitentLokacijaEdit.PutString("lokacijaNaziv", nazivInput.Text);
                localKomitentLokacijaEdit.Commit();

                if (anketePoPozicijama.Checked)
                    intent = new Intent(this, typeof(Activity_Pozicije));
                else
                {
                    int pozicijaId = -1;
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
                            id,
                            1,
                            null,
                            null,
                            1,
                            1,
                            null,
                            pozicijaId,
                            1
                    );

                    Guid anketaId = Guid.NewGuid();
                    var username = localUsername.GetString("nazivDjelatnika", null);
                    db.Query<DID_Anketa>(
                        "INSERT INTO DID_Anketa (" +
                            "Id, " +
                            "ANK_TipDeratizacije, " +
                            "ANK_RadniNalog, " +
                            "ANK_KorisnickoIme, " +
                            "ANK_POZ_Id, " +
                            "ANK_DatumVrijeme, " +
                            "ANK_TipDeratizacijskeKutijeSifra, " +
                            "SinhronizacijaPrivremeniKljuc, " +
                            "SinhronizacijaStatus)" +
                        "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)",
                            anketaId,
                            2,
                            radniNalog,
                            username,
                            pozicijaId,
                            DateTime.Now,
                            1,
                            anketaId,
                            2
                        );

                    localPozicijaEdit.PutInt("pozicijaId", -1);
                    localPozicijaEdit.Commit();
                    intent = new Intent(this, typeof(Activity_PotroseniMaterijali_List));
                }

                StartActivity(intent);
            }
        }

        private void LokacijaListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            int lokacijaId = lokacijeListFiltered[e.Position].SAN_Id;
            int RNLokacijeId = -1;
            //opet moram provjeriu raditi za radninalog lokaciju
            List<DID_RadniNalog_Lokacija> radniNalogLokacije = db.Query<DID_RadniNalog_Lokacija>(
                "SELECT * " +
                "FROM DID_RadniNalog_Lokacija " +
                "WHERE Id < 0");

            if (radniNalogLokacije.Any())
                RNLokacijeId = radniNalogLokacije.FirstOrDefault().Id - 1;

            List<DID_LokacijaPozicija> pozicije = db.Query<DID_LokacijaPozicija>(
                "SELECT * " +
                "FROM DID_LokacijaPozicija " +
                "WHERE SAN_Id = ?", lokacijaId);

            if (pozicije.Any())
            {
                db.Query<DID_RadniNalog_Lokacija>(
                    "INSERT INTO DID_RadniNalog_Lokacija (" +
                        "Id, " +
                        "RadniNalog, " +
                        "Lokacija, " +
                        "VrijemeDolaska, " +
                        "TipAkcije, " +
                        "SinhronizacijaPrivremeniKljuc, " +
                        "Status)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?)",
                        RNLokacijeId,
                        radniNalog,
                        lokacijaId,
                        DateTime.Now,
                        2,
                        RNLokacijeId,
                        1
                );
            }
            else
            {
                db.Query<DID_RadniNalog_Lokacija>(
                    "INSERT INTO DID_RadniNalog_Lokacija (" +
                        "Id, " +
                        "RadniNalog, " +
                        "Lokacija, " +
                        "VrijemeDolaska, " +
                        "TipAkcije, " +
                        "SinhronizacijaPrivremeniKljuc, " +
                        "Status)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?)",
                        RNLokacijeId,
                        radniNalog,
                        lokacijaId,
                        DateTime.Now,
                        1,
                        RNLokacijeId,
                        1
                );
            }

            List<DID_RadniNalog_Lokacija> izvrseneLokacije = db.Query<DID_RadniNalog_Lokacija>(
                "SELECT * " +
                "FROM DID_RadniNalog_Lokacija " +
                "WHERE RadniNalog = ? " +
                "AND Status == 3", radniNalog);

            if (izvrseneLokacije.Any())
            {
                db.Query<DID_RadniNalog>(
                    "UPDATE DID_RadniNalog " +
                    "SET Status = ? " +
                    "WHERE Id = ?", 4, radniNalog);
            }
            else
            {
                db.Query<DID_RadniNalog>(
                    "UPDATE DID_RadniNalog " +
                    "SET Status = ? " +
                    "WHERE Id = ?", 3, radniNalog);
            }

            localKomitentLokacijaEdit.PutString("lokacijaNaziv", lokacijeListFiltered[e.Position].SAN_Naziv);
            localKomitentLokacijaEdit.PutInt("radniNalogLokacijaId", RNLokacijeId);
            localKomitentLokacijaEdit.PutInt("lokacijaId", lokacijeListFiltered[e.Position].SAN_Id);
            localKomitentLokacijaEdit.Commit();
            intent = new Intent(this, typeof(Activity_LokacijaPostavke));
            StartActivity(intent);
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
                if (localRadniNalozi.GetBoolean("noviRN", false))
                    intent = new Intent(this, typeof(Activity_NoviKomitent));
                else
                    intent = new Intent(this, typeof(Activity_Lokacije));
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