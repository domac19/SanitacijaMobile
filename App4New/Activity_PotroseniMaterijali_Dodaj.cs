using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Infobit.DDD.Data;
using SQLite;
using Android.Content.PM;

namespace App4New
{
    [Activity(Label = "Activity_PotroseniMaterijali_Dodaj", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_PotroseniMaterijali_Dodaj : SelectedCulture
    {
        EditText kolicinaInput;
        TextView IznosData, cijenaData, nazivData, sifraData, messageKolicina, mjernaJedinicaTV, dostupno;
        Button spremiBtn, odustaniBtn;
        Intent intent;
        DID_RadniNalog skladiste;
        decimal cijena, staraKolicinaNaSkladistu;
        int radniNalog, pozicijaId, lokacijaId, mjernaJedinicaId;
        string materijalSifra;

        static readonly ISharedPreferences localPozicija = Application.Context.GetSharedPreferences("pozicija", FileCreationMode.Private);
        static readonly ISharedPreferences localMaterijali = Application.Context.GetSharedPreferences("localMaterijali", FileCreationMode.Private);
        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);
        static readonly ISharedPreferences localKomitentLokacija = Application.Context.GetSharedPreferences("localKomitentLokacija", FileCreationMode.Private);

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.potroseniMaterijal_Dodaj);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            kolicinaInput = FindViewById<EditText>(Resource.Id.kolicinaInput);
            IznosData = FindViewById<TextView>(Resource.Id.IznosData);
            cijenaData = FindViewById<TextView>(Resource.Id.cijenaData);
            nazivData = FindViewById<TextView>(Resource.Id.nazivData);
            sifraData = FindViewById<TextView>(Resource.Id.sifraData);
            mjernaJedinicaTV = FindViewById<TextView>(Resource.Id.mjernaJedinicaTV);
            spremiBtn = FindViewById<Button>(Resource.Id.spremiBtn);
            odustaniBtn = FindViewById<Button>(Resource.Id.odustaniBtn);
            messageKolicina = FindViewById<TextView>(Resource.Id.messageKolicina);
            dostupno = FindViewById<TextView>(Resource.Id.dostupno);

            SetActionBar(toolbar);
            ActionBar.Title = "Dodaj materijal";
            spremiBtn.Click += SpremiBtn_Click;
            odustaniBtn.Click += OdustaniBtn_Click;
            kolicinaInput.KeyPress += KolicinaInput_KeyPress;
            kolicinaInput.TextChanged += KolicinaInput_TextChanged;
            kolicinaInput.Text = "1,000";
            kolicinaInput.RequestFocus();
            materijalSifra = localMaterijali.GetString("sifra", null);
            radniNalog = localRadniNalozi.GetInt("id", 0);
            pozicijaId = localPozicija.GetInt("pozicijaId", 0);
            lokacijaId = localKomitentLokacija.GetInt("lokacijaId", 0);
            mjernaJedinicaId = localMaterijali.GetInt("mjernaJedinicaId", 0);

            var mjernaJedinica = db.Query<T_MjerneJedinice>(
                "SELECT * " +
                "FROM T_MjerneJedinice " +
                "WHERE Id = ?", mjernaJedinicaId).FirstOrDefault();
            T_NAZR materijal = db.Query<T_NAZR>(
                "SELECT * " +
                "FROM T_NAZR " +
                "WHERE NAZR_SIFRA = ?", materijalSifra).FirstOrDefault();

            skladiste = db.Query<DID_RadniNalog>(
                  "SELECT * " +
                  "FROM DID_RadniNalog " +
                  "WHERE Id = ?", radniNalog).FirstOrDefault();
            staraKolicinaNaSkladistu = db.Query<DID_StanjeSkladista>(
                "SELECT * " +
                "FROM DID_StanjeSkladista " +
                "WHERE Skladiste = ? " +
                "AND Materijal = ?", skladiste.PokretnoSkladiste, materijalSifra).FirstOrDefault().Kolicina;

            dostupno.Text = staraKolicinaNaSkladistu.ToString("F3").Replace('.', ',');
            cijena = materijal.NAZR_CIJENA_ART;
            nazivData.Text = materijal.NAZR_NAZIV;
            sifraData.Text = materijal.NAZR_SIFRA;
            cijenaData.Text = cijena.ToString("F2").Replace('.', ',');
            IznosData.Text = cijena.ToString("F2").Replace('.', ',');
            mjernaJedinicaTV.Text = mjernaJedinica.Oznaka;
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

        public void KolicinaInput_TextChanged(object sender, EventArgs e)
        {
            try
            {
                messageKolicina.Visibility = Android.Views.ViewStates.Invisible;
                if (string.IsNullOrEmpty(kolicinaInput.Text))
                    IznosData.Text = "00,00";
                else
                {
                    var kolicina2 = Double.Parse(kolicinaInput.Text);
                    double iznos2 = Double.Parse(cijena.ToString()) * kolicina2;
                    double resultatIznos2 = Math.Round(iznos2, 2);
                    IznosData.Text = resultatIznos2.ToString("F2").Replace('.', ',');   
                }
            }
            catch (Exception ex)
            {
                messageKolicina.Visibility = Android.Views.ViewStates.Visible;
            }
        }

        public void OdustaniBtn_Click(object sender, EventArgs e)
        {
            intent = new Intent(this, typeof(Activity_PotroseniMaterijali_List));
            StartActivity(intent);
        }

        public void SpremiBtn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(kolicinaInput.Text) && Convert.ToDecimal(kolicinaInput.Text) > 0)
            {
                messageKolicina.Visibility = Android.Views.ViewStates.Invisible;
                int anketaMaterijalId = -1;

                List<DID_AnketaMaterijali> materijal = db.Query<DID_AnketaMaterijali>(
                    "SELECT * " +
                    "FROM DID_AnketaMaterijali " +
                    "WHERE PozicijaId = ? " +
                    "AND RadniNalog = ? " +
                    "AND MaterijalSifra = ? " +
                    "AND MjernaJedinica = ?", pozicijaId, radniNalog, materijalSifra, mjernaJedinicaId);

                if (materijal.Any())
                {
                    decimal dodanaKolicina = Convert.ToDecimal(kolicinaInput.Text);
                    decimal kolicina = materijal.FirstOrDefault().Kolicina + dodanaKolicina;
                    decimal dodaniIznos = Convert.ToDecimal(IznosData.Text);
                    decimal iznos = materijal.FirstOrDefault().Iznos + dodaniIznos;
                    anketaMaterijalId = materijal.FirstOrDefault().Id;

                    db.Execute(
                        "UPDATE DID_AnketaMaterijali " +
                        "SET Kolicina = ?, " +
                            "Iznos = ? " +
                        "WHERE Id = ?", kolicina, iznos, anketaMaterijalId);
                }
                else
                {
                    List<DID_AnketaMaterijali> anketaMaterijali = db.Query<DID_AnketaMaterijali>(
                        "SELECT * " +
                        "FROM DID_AnketaMaterijali " +
                        "WHERE Id < 0");

                    if (anketaMaterijali.Any())
                        anketaMaterijalId = anketaMaterijali.FirstOrDefault().Id - 1;

                    db.Query<DID_AnketaMaterijali>(
                        "INSERT INTO DID_AnketaMaterijali (" +
                            "Id, " +
                            "RadniNalog, " +
                            "PozicijaId, " +
                            "MaterijalSifra, " +
                            "MaterijalNaziv, " +
                            "MjernaJedinica, " +
                            "Kolicina, " +
                            "Iznos, " +
                            "Cijena, " +
                            "LokacijaId, " +
                            "SinhronizacijaPrivremeniKljuc)" +
                        "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                            anketaMaterijalId,
                            radniNalog,
                            pozicijaId,
                            materijalSifra,
                            nazivData.Text,
                            mjernaJedinicaId,
                            Convert.ToDecimal(kolicinaInput.Text),
                            Convert.ToDecimal(IznosData.Text),
                            Convert.ToDecimal(cijenaData.Text),
                            lokacijaId,
                            anketaMaterijalId
                    );
                }

                // UPDATE Kolicine na skladistu
                decimal novaKolicinaNaSkladistu = Convert.ToDecimal(staraKolicinaNaSkladistu.ToString("F3")) - Convert.ToDecimal(kolicinaInput.Text);
                db.Execute(
                    "UPDATE DID_StanjeSkladista " +
                    "SET Kolicina = ?, " +
                        "StaraKolicina = ? " +
                    "WHERE Skladiste = ? " +
                    "AND Materijal = ?",
                        novaKolicinaNaSkladistu,
                        novaKolicinaNaSkladistu,
                        skladiste.PokretnoSkladiste,
                        materijalSifra);

                // UPDATE potvrde i svih podataka vezanih uz nju
                List<DID_Potvrda> potvrda = db.Query<DID_Potvrda>(
                    "SELECT * " +
                    "FROM DID_Potvrda " +
                    "WHERE Lokacija = ? " +
                    "AND RadniNalog = ?", lokacijaId, radniNalog);

                if (potvrda.Any())
                {
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
                    foreach (var materijalPotvrda in listaMaterijalaPotvrda)
                    {
                        db.Execute(
                            "UPDATE DID_Potvrda_Materijal " +
                            "SET SinhronizacijaPrivremeniKljuc = ? " +
                            "WHERE Id = ?", materijalPotvrda.Id, materijalPotvrda.Id);
                    }
                }

                intent = new Intent(this, typeof(Activity_PotroseniMaterijali_List));
                StartActivity(intent);
            }
            else
                messageKolicina.Visibility = Android.Views.ViewStates.Visible;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menuClose, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.TitleFormatted.ToString() == "close")
                intent = new Intent(this, typeof(Activity_PotroseniMaterijali_List));

            StartActivity(intent);
            return base.OnOptionsItemSelected(item);
        }
    }
}