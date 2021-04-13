using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using SQLite;
using Infobit.DDD.Data;
using Android.Content.PM;

namespace App4New
{
    [Activity(Label = "Activity_PotroseniMaterijali_Novo_Dodaj", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_PotroseniMaterijali_Novo_Dodaj : Activity
    {
        EditText kolicinaInput;
        TextView message, nazivData, cijenaData;
        Button spremiBtn, odustaniBtn;
        Spinner spinnerMjernaJedinica;
        Intent intent;
        List<T_MjerneJedinice> mjerneJediniceList;
        string skladiste, cijena;

        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);
        static readonly ISharedPreferences localMaterijali = Application.Context.GetSharedPreferences("localMaterijali", FileCreationMode.Private);

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.potroseniMaterijal_NoviMaterijal);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            kolicinaInput = FindViewById<EditText>(Resource.Id.kolicinaInput);
            nazivData = FindViewById<TextView>(Resource.Id.nazivData);
            cijenaData = FindViewById<TextView>(Resource.Id.cijenaData);
            message = FindViewById<TextView>(Resource.Id.message);
            spremiBtn = FindViewById<Button>(Resource.Id.spremiBtn);
            odustaniBtn = FindViewById<Button>(Resource.Id.odustaniBtn);
            spinnerMjernaJedinica = FindViewById<Spinner>(Resource.Id.spinnerMjernaJedinica);

            SetActionBar(toolbar);
            ActionBar.Title = "Novi materijal";
            skladiste = localRadniNalozi.GetString("skladiste", null);
            spremiBtn.Click += SpremiBtn_Click;
            odustaniBtn.Click += OdustaniBtn_Click;
            kolicinaInput.KeyPress += KolicinaInput_KeyPress;
            kolicinaInput.TextChanged += Input_TextChanged;
            kolicinaInput.Text = "1,000";
            nazivData.Text = localMaterijali.GetString("naziv", null);
            cijena = Convert.ToDecimal(localMaterijali.GetString("cijena", null)).ToString("F2").Replace('.', ',');
            cijenaData.Text = cijena;
            mjerneJediniceList = db.Query<T_MjerneJedinice>(
                "SELECT * " +
                "FROM T_MjerneJedinice");

            List<string> listMjernaJedinica = new List<string>();
            foreach (var item in mjerneJediniceList)
                listMjernaJedinica.Add(item.Oznaka);

            ArrayAdapter<string> adapterMjernaJedinicaList = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, listMjernaJedinica);
            adapterMjernaJedinicaList.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinnerMjernaJedinica.Adapter = adapterMjernaJedinicaList;
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

        public void Input_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(kolicinaInput.Text))
                message.Visibility = Android.Views.ViewStates.Visible;
            else
                message.Visibility = Android.Views.ViewStates.Invisible;
        }

        public void OdustaniBtn_Click(object sender, EventArgs e)
        {
            intent = new Intent(this, typeof(Activity_PotroseniMaterijali_Novo_Lista));
            StartActivity(intent);
        }

        public void SpremiBtn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(kolicinaInput.Text) && Convert.ToDecimal(kolicinaInput.Text) > 0)
            {
                message.Visibility = Android.Views.ViewStates.Invisible;
                string spinnerMJ = spinnerMjernaJedinica.SelectedItem.ToString();
                T_MjerneJedinice selectedMjernaJedinica = db.Query<T_MjerneJedinice>(
                    "SELECT * " +
                    "FROM T_MjerneJedinice " +
                    "WHERE Oznaka = ?", spinnerMJ).FirstOrDefault();

                string materijalSifra = localMaterijali.GetString("sifra", null);

                List<DID_StanjeSkladista> dodaniMaterijalNaSkladistu = db.Query<DID_StanjeSkladista>(
                    "SELECT * " +
                    "FROM DID_StanjeSkladista " +
                    "WHERE Materijal = ? " +
                    "AND MjernaJedinica = ? " +
                    "AND Skladiste = ?", materijalSifra, selectedMjernaJedinica.Id, skladiste);

                //OVO JE OPCIJA KADA BI SE MOGLO DOGODITI DA SE DODAJE NOVI MATERIJAL, ALI MOZE SE SAMO DODATI ODABRANI MATERIJAL KOJI VEC POSTOJI
                if (dodaniMaterijalNaSkladistu.Any())
                {
                    var staraKolicinaNaSkladistu = db.Query<DID_StanjeSkladista>(
                       "SELECT * " +
                       "FROM DID_StanjeSkladista " +
                       "WHERE Skladiste = ? " +
                       "AND MjernaJedinica = ? " +
                       "AND Materijal = ?", skladiste, selectedMjernaJedinica.Id, materijalSifra).FirstOrDefault().Kolicina;

                    decimal novaKolicinaNaSkladistu = Convert.ToDecimal(staraKolicinaNaSkladistu.ToString("F3")) + Convert.ToDecimal(kolicinaInput.Text);

                    db.Execute(
                        "UPDATE DID_StanjeSkladista " +
                        "SET Kolicina = ?, " +
                            "StaraKolicina = ?, " +
                            "Dodano = ? " +
                        "WHERE Skladiste = ? " +
                        "AND Materijal = ? " +
                        "AND MjernaJedinica = ?",
                            novaKolicinaNaSkladistu,
                            novaKolicinaNaSkladistu,
                            Convert.ToDecimal(kolicinaInput.Text),
                            skladiste,
                            materijalSifra,
                            selectedMjernaJedinica.Id);
                }
                else
                {
                    string nazivSkladista = db.Query<T_SKL>(
                        "SELECT * " +
                        "FROM T_SKL " +
                        "WHERE SKL_SIFRA = ?", skladiste).FirstOrDefault().SKL_NAZIV;

                    decimal kolicina = Convert.ToDecimal(Convert.ToDecimal(kolicinaInput.Text).ToString("F3").Replace('.', ','));
                    decimal iznos = Convert.ToDecimal((Convert.ToDecimal(kolicina) * Convert.ToDecimal(cijena)).ToString("F2").Replace('.', ','));

                    db.Query<DID_StanjeSkladista>(
                        "INSERT INTO DID_StanjeSkladista (" +
                            "Skladiste, " +
                            "SkladisteNaziv, " +
                            "Materijal, " +
                            "MaterijalNaziv, " +
                            "MjernaJedinica, " +
                            "MjernaJedinicaOznaka, " +
                            "Kolicina, " +
                            "Cijena, " +
                            "Iznos, " +
                            "Dodano, " +
                            "StaraKolicina)" +
                        "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                            skladiste,
                            nazivSkladista,
                            materijalSifra,
                            nazivData.Text,
                            selectedMjernaJedinica.Id,
                            selectedMjernaJedinica.Oznaka,
                            kolicina,
                            cijena,
                            iznos,
                            kolicina,
                            kolicina);
                }

                intent = new Intent(this, typeof(Activity_PotroseniMaterijali_List));
                StartActivity(intent);
            }
            else
                message.Visibility = Android.Views.ViewStates.Visible;
        }
        public override void OnBackPressed()
        {
            intent = new Intent(this, typeof(Activity_PotroseniMaterijali_Novo_Lista));
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