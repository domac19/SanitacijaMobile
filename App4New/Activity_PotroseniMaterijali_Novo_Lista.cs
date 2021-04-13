using Infobit.DDD.Data;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using Android.Content.PM;

namespace App4New
{
    [Activity(Label = "Activity_PotroseniMaterijali_Novo_Lista", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_PotroseniMaterijali_Novo_Lista : Activity
    {
        TextView resultMessage, message;
        ListView materijalListView;
        List<T_NAZR> materijaliList, materijaliFiltered;
        Intent intent;
        RadioButton noviMatBtn, poznatiMatBtn;
        LinearLayout prikazPoznatihMaterijala;
        ScrollView prikazNovogMaterijala;
        EditText searchInput, nazivInput, cijenaInput, kolicinaInput, barcodeInput;
        Spinner spinnerMjernaJedinica, spinnerGrupa;
        Button odustaniBtn, spremiBtn;
        int radniNalog = localRadniNalozi.GetInt("id", 0);

        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);
        static readonly ISharedPreferences localMaterijali = Application.Context.GetSharedPreferences("localMaterijali", FileCreationMode.Private);
        static ISharedPreferencesEditor localMaterijaliEdit = localMaterijali.Edit();

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.materijaliList);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            materijalListView = FindViewById<ListView>(Resource.Id.materijalListView);
            resultMessage = FindViewById<TextView>(Resource.Id.resultMessage);
            message = FindViewById<TextView>(Resource.Id.message);
            poznatiMatBtn = FindViewById<RadioButton>(Resource.Id.poznatiMatBtn);
            noviMatBtn = FindViewById<RadioButton>(Resource.Id.noviMatBtn);
            prikazPoznatihMaterijala = FindViewById<LinearLayout>(Resource.Id.prikazPoznatihMaterijala);
            prikazNovogMaterijala = FindViewById<ScrollView>(Resource.Id.prikazNovogMaterijala);
            searchInput = FindViewById<EditText>(Resource.Id.searchInput);
            nazivInput = FindViewById<EditText>(Resource.Id.nazivInput);
            cijenaInput = FindViewById<EditText>(Resource.Id.cijenaInput);
            kolicinaInput = FindViewById<EditText>(Resource.Id.kolicinaInput);
            barcodeInput = FindViewById<EditText>(Resource.Id.barcodeInput);
            spinnerMjernaJedinica = FindViewById<Spinner>(Resource.Id.spinnerMjernaJedinica);
            spinnerGrupa = FindViewById<Spinner>(Resource.Id.spinnerGrupa);
            spremiBtn = FindViewById<Button>(Resource.Id.spremiBtn);
            odustaniBtn = FindViewById<Button>(Resource.Id.odustaniBtn);

            SetActionBar(toolbar);
            ActionBar.Title = "Novi materijala";
            materijalListView.ItemClick += MaterijalListView_ItemClick;
            spremiBtn.Click += SpremiBtn_Click;
            odustaniBtn.Click += OdustaniBtn_Click;
            poznatiMatBtn.Click += PoznatiMatBtn_Click;
            noviMatBtn.Click += NoviMatBtn_Click;
            nazivInput.TextChanged += Input_TextChanged;
            cijenaInput.TextChanged += Input_TextChanged;
            kolicinaInput.TextChanged += Input_TextChanged;
            kolicinaInput.KeyPress += KolicinaInput_KeyPress;
            searchInput.KeyPress += SearchInput_KeyPress;
            searchInput.TextChanged += SearchInput_TextChanged;

            //Spinner za mjerne jedinice
            List < T_MjerneJedinice> mjerneJediniceList = db.Query<T_MjerneJedinice>(
                "SELECT * " +
                "FROM T_MjerneJedinice");
            List<string> listMjernaJedinica = new List<string>();
            foreach (var item in mjerneJediniceList)
                listMjernaJedinica.Add(item.Oznaka);
            ArrayAdapter<string> adapterMjernaJedinicaList = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, listMjernaJedinica);
            adapterMjernaJedinicaList.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinnerMjernaJedinica.Adapter = adapterMjernaJedinicaList;

            //Spinner za grupe materijala
            List<T_GRU> grupaList = db.Query<T_GRU>(
                "SELECT * " +
                "FROM T_GRU " +
                "GROUP BY GRU_NAZIV");
            List<string> listGrupa = new List<string>();
            foreach (var item in grupaList)
                listGrupa.Add(item.GRU_NAZIV);
            ArrayAdapter<string> adapterGrupaList = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, listGrupa);
            adapterGrupaList.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinnerGrupa.Adapter = adapterGrupaList;

            //Prikaz poznatih materijala za dodavanje
            materijaliList = db.Query<T_NAZR>(
                "SELECT * " +
                "FROM T_NAZR " +
                "GROUP BY NAZR_NAZIV");

            materijaliFiltered = materijaliList;

            if (materijaliFiltered.Any())
            {
                materijalListView.Visibility = Android.Views.ViewStates.Visible;
                materijalListView.Adapter = new Adapter_NoviMaterijali(this, materijaliFiltered);
            }
            else
                noviMatBtn.PerformClick();
        }

        public void KolicinaInput_KeyPress(object sender, View.KeyEventArgs e)
        {
            e.Handled = false;
            if (e.KeyCode == Keycode.Enter)
            {
                spremiBtn.PerformClick();
                e.Handled = true;
            }
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
                materijaliFiltered = materijaliList.Where(i =>
                    (i.NAZR_NAZIV != null && i.NAZR_NAZIV.ToLower().Contains(input)) ||
                    (i.NAZR_SIFRA != null && i.NAZR_SIFRA.ToLower().Contains(input))).ToList();

                if (materijaliFiltered.Any())
                {
                    materijalListView.Visibility = Android.Views.ViewStates.Visible;
                    materijalListView.Adapter = new Adapter_NoviMaterijali(this, materijaliFiltered);
                    resultMessage.Visibility = Android.Views.ViewStates.Gone;
                }
                else
                {
                    materijalListView.Visibility = Android.Views.ViewStates.Gone;
                    resultMessage.Text = "Nije pronađen materijal sa traženim pojmom!";
                    resultMessage.Visibility = Android.Views.ViewStates.Visible;
                }
            }
            else
            {
                materijaliFiltered = materijaliList;
                if (materijaliFiltered.Any())
                {
                    materijalListView.Visibility = Android.Views.ViewStates.Visible;
                    materijalListView.Adapter = new Adapter_NoviMaterijali(this, materijaliFiltered);
                }
                else
                {
                    materijalListView.Visibility = Android.Views.ViewStates.Gone;
                    resultMessage.Text = "Nema poznatih materijala!";
                    resultMessage.Visibility = Android.Views.ViewStates.Visible;
                }
            }
        }

        public void Input_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(nazivInput.Text) || String.IsNullOrEmpty(cijenaInput.Text) || String.IsNullOrEmpty(kolicinaInput.Text))
                message.Visibility = Android.Views.ViewStates.Visible;
            else
                message.Visibility = Android.Views.ViewStates.Invisible;
        }

        public void PoznatiMatBtn_Click(object sender, EventArgs args)
        {
            prikazPoznatihMaterijala.Visibility = Android.Views.ViewStates.Visible;
            prikazNovogMaterijala.Visibility = Android.Views.ViewStates.Gone;

            if (!materijaliFiltered.Any())
            {
                resultMessage.Visibility = Android.Views.ViewStates.Visible;
                resultMessage.Text = "Nema poznatih materijala!";
                materijalListView.Visibility = Android.Views.ViewStates.Gone;
            }
            else
            {
                resultMessage.Visibility = Android.Views.ViewStates.Gone;
                materijalListView.Visibility = Android.Views.ViewStates.Visible;
                materijalListView.Adapter = new Adapter_NoviMaterijali(this, materijaliFiltered);
            }
        }

        public void NoviMatBtn_Click(object sender, EventArgs args)
        {
            prikazPoznatihMaterijala.Visibility = Android.Views.ViewStates.Gone;
            prikazNovogMaterijala.Visibility = Android.Views.ViewStates.Visible;
            nazivInput.RequestFocus();
        }

        public void ShowMessage(string msg, EditText input)
        {
            message.Visibility = Android.Views.ViewStates.Visible;
            message.Text = msg;
            input.RequestFocus();
        }

        public void SpremiBtn_Click(object sender, EventArgs args)
        {
            if (String.IsNullOrEmpty(nazivInput.Text))
                ShowMessage("Popunite sva polja koju su označena zvijezdicom*", nazivInput);
            else if (String.IsNullOrEmpty(cijenaInput.Text))
                ShowMessage("Popunite sva polja koju su označena zvijezdicom*", cijenaInput);
            else if (String.IsNullOrEmpty(kolicinaInput.Text))
                ShowMessage("Popunite sva polja koju su označena zvijezdicom*", kolicinaInput);
            else if(nazivInput.Text.Length > 100)
                ShowMessage("Naziv materijala ne smije biti dulji od 100 znakova*", nazivInput);
            else if(barcodeInput.Text.Length > 16)
                ShowMessage("Barkod mora imati 16 znakova*", barcodeInput);
            else
            {
                message.Visibility = Android.Views.ViewStates.Invisible;
                string mjernaJedinicaOznaka = spinnerMjernaJedinica.SelectedItem.ToString();
                int mjernaJedinicaId = db.Query<T_MjerneJedinice>(
                    "SELECT * " +
                    "FROM T_MjerneJedinice " +
                    "WHERE Oznaka = ?", mjernaJedinicaOznaka).FirstOrDefault().Id;

                string grupaNaziv = spinnerGrupa.SelectedItem.ToString();
                string grupaId = db.Query<T_GRU>(
                    "SELECT * " +
                    "FROM T_GRU " +
                    "WHERE GRU_NAZIV = ?", grupaNaziv).FirstOrDefault().GRU_SIFRA;

                string sifra;
                if (nazivInput.Text.Length > 6)
                    sifra = nazivInput.Text.Substring(0, 6);
                else
                    sifra = nazivInput.Text;

                localMaterijaliEdit.PutString("sifra", sifra);
                localMaterijaliEdit.Commit();

                var cijena = Convert.ToDecimal(Convert.ToDecimal(cijenaInput.Text).ToString("F2").Replace('.', ','));

                db.Execute(
                    "INSERT INTO T_NAZR (NAZR_SIFRA, NAZR_NAZIV, NAZR_CIJENA_ART, NAZR_BARKOD, NAZR_GRUPA, NAZR_JM_SIFRA, SinhronizacijaPrivremeniKljuc)" +
                    "VALUES(?, ?, ?, ?, ?, ?, ?)", sifra, nazivInput.Text, cijena, barcodeInput.Text, grupaId, mjernaJedinicaId, sifra);

                string skladiste = db.Query<DID_RadniNalog>(
                    "SELECT * " +
                    "FROM DID_RadniNalog " +
                    "WHERE Id = ?", radniNalog).FirstOrDefault().PokretnoSkladiste;

                string nazivSkladista = db.Query<T_SKL>(
                    "SELECT * " +
                    "FROM T_SKL " +
                    "WHERE SKL_SIFRA = ?", skladiste).FirstOrDefault().SKL_NAZIV;

                decimal kolicina = Convert.ToDecimal(Convert.ToDecimal(kolicinaInput.Text).ToString("F3").Replace('.', ','));
                decimal iznos = Convert.ToDecimal((Convert.ToDecimal(kolicina) * Convert.ToDecimal(cijena)).ToString("F2").Replace('.', ','));

                //db.Query<DID_StanjeSkladista>(
                //    "INSERT INTO DID_StanjeSkladista (" +
                //        "Skladiste, " +
                //        "SkladisteNaziv, " +
                //        "Materijal, " +
                //        "MaterijalNaziv, " +
                //        "MjernaJedinica, " +
                //        "MjernaJedinicaOznaka, " +
                //        "Kolicina, " +
                //        "Cijena, " +
                //        "Iznos, " +
                //        "Dodano, " +
                //        "StaraKolicina)" +
                //    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                //        skladiste,
                //        nazivSkladista,
                //        sifra,
                //        nazivInput.Text,
                //        selectedMjernaJedinica.Id,
                //        selectedMjernaJedinica.Oznaka,
                //        kolicina,
                //        cijena,
                //        iznos,
                //        kolicina,
                //        kolicina);

                //db.Query<DID_StanjeSkladista>(
                //    "INSERT INTO DID_StanjeSkladista (" +
                //        "Id, " +
                //        "Materijal, " +
                //        "Skladiste, " +
                //        "SkladisteNaziv, " +
                //        "MaterijalNaziv, " +
                //        "MjernaJedinica, " +
                //        "MjernaJedinicaOznaka, " +
                //        "Kolicina, " +
                //        "Cijena, " +
                //        "SinhronizacijaPrivremeniKljuc)" +
                //    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                //        stanjeSkladistaId,
                //        sifra,
                //        skladiste,
                //        nazivSkladista,
                //        nazivInput.Text,
                //        mjernaJedinicaId,
                //        mjernaJedinicaOznaka,
                //        Convert.ToDecimal(kolicinaInput.Text),
                //        Convert.ToDecimal(cijenaInput.Text),
                //        stanjeSkladistaId
                //);

                intent = new Intent(this, typeof(Activity_PotroseniMaterijali_List));
                StartActivity(intent);
            }
        }

        public void OdustaniBtn_Click(object sender, EventArgs args)
        {
            intent = new Intent(this, typeof(Activity_PotroseniMaterijali_List));
            StartActivity(intent);
        }

        private void MaterijalListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            localMaterijaliEdit.PutString("naziv", materijaliFiltered[e.Position].NAZR_NAZIV);
            localMaterijaliEdit.PutString("cijena", materijaliFiltered[e.Position].NAZR_CIJENA_ART.ToString());
            localMaterijaliEdit.PutString("sifra", materijaliFiltered[e.Position].NAZR_SIFRA);
            localMaterijaliEdit.Commit();
            intent = new Intent(this, typeof(Activity_PotroseniMaterijali_Novo_Dodaj));
            StartActivity(intent);
        }

        public override void OnBackPressed()
        {
            intent = new Intent(this, typeof(Activity_PotroseniMaterijali_List));
            StartActivity(intent);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menuClose, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.TitleFormatted.ToString() == "close")
                OnBackPressed();
            return base.OnOptionsItemSelected(item);
        }
    }
}