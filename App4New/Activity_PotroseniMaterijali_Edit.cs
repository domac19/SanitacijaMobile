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
    [Activity(Label = "Activity_PotroseniMaterijali_Edit", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_PotroseniMaterijali_Edit : SelectedCulture
    {
        EditText kolicinaInput;
        TextView IznosData, cijenaData, nazivData, sifraData, messageKolicina, mjernaJedinicaTV, dostupno;
        Button spremiBtn, odustaniBtn;
        Intent intent;
        decimal cijena, staraKolicinaNaSkladistu;
        int materijalId, radniNalog, pozicijaId, lokacijaId, mjernaJedinicaId;
        string materijalSifra;
        DID_AnketaMaterijali materijal;

        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);
        static readonly ISharedPreferences localMaterijali = Application.Context.GetSharedPreferences("localMaterijali", FileCreationMode.Private);

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
            mjernaJedinicaTV = FindViewById<TextView>(Resource.Id.mjernaJedinicaTV);
            cijenaData = FindViewById<TextView>(Resource.Id.cijenaData);
            nazivData = FindViewById<TextView>(Resource.Id.nazivData);
            sifraData = FindViewById<TextView>(Resource.Id.sifraData);
            spremiBtn = FindViewById<Button>(Resource.Id.spremiBtn);
            odustaniBtn = FindViewById<Button>(Resource.Id.odustaniBtn);
            messageKolicina = FindViewById<TextView>(Resource.Id.messageKolicina);
            dostupno = FindViewById<TextView>(Resource.Id.dostupno);

            SetActionBar(toolbar);
            ActionBar.Title = "Promjeni materijal";
            spremiBtn.Click += SpremiBtn_Click;
            odustaniBtn.Click += OdustaniBtn_Click;
            kolicinaInput.TextChanged += KolicinaInput_TextChanged;
            materijalSifra = localMaterijali.GetString("sifra", null);
            materijalId = localMaterijali.GetInt("id", 0);
            radniNalog = localRadniNalozi.GetInt("id", 0);
            pozicijaId = localMaterijali.GetInt("pozicijaId", 0);
            lokacijaId = localMaterijali.GetInt("lokacijaId", 0);
            mjernaJedinicaId = localMaterijali.GetInt("mjernaJedinica", 0);
            kolicinaInput.KeyPress += KolicinaInput_KeyPress;

            var mjernaJedinica = db.Query<T_MjerneJedinice>(
               "SELECT * " +
               "FROM T_MjerneJedinice " +
               "WHERE Id = ?", mjernaJedinicaId).FirstOrDefault();
            materijal = db.Query<DID_AnketaMaterijali>(
                "SELECT * " +
                "FROM DID_AnketaMaterijali " +
                "WHERE RadniNalog = ? " +
                "AND PozicijaId = ? " +
                "AND MaterijalSifra = ?", radniNalog, pozicijaId, materijalSifra).FirstOrDefault();

            mjernaJedinicaTV.Text = db.Query<T_MjerneJedinice>(
                "SELECT * " +
                "FROM T_MjerneJedinice " +
                "WHERE Id = ?", mjernaJedinicaId).FirstOrDefault().Oznaka;

            var skladiste = db.Query<DID_RadniNalog>(
                  "SELECT * " +
                  "FROM DID_RadniNalog " +
                  "WHERE Id = ?", radniNalog).FirstOrDefault();
            staraKolicinaNaSkladistu = db.Query<DID_StanjeSkladista>(
                "SELECT * " +
                "FROM DID_StanjeSkladista " +
                "WHERE Skladiste = ? " +
                "AND Materijal = ?", skladiste.PokretnoSkladiste, materijalSifra).FirstOrDefault().StaraKolicina;

            dostupno.Text = staraKolicinaNaSkladistu.ToString("F3").Replace('.', ',');
            cijena = materijal.Cijena;
            nazivData.Text = materijal.MaterijalNaziv;
            sifraData.Text = materijal.MaterijalSifra;
            cijenaData.Text = cijena.ToString("F2").Replace('.', ',');
            IznosData.Text = cijena.ToString("F2").Replace('.', ',');
            kolicinaInput.Text = materijal.Kolicina.ToString("F3").Replace('.', ',');
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
                    IznosData.Text = "00.00";
                else if (Convert.ToDecimal(kolicinaInput.Text) <= 0)
                {
                    messageKolicina.Visibility = Android.Views.ViewStates.Visible;
                }
                else
                {
                    var kolicina = Double.Parse(kolicinaInput.Text);
                    double iznos = Double.Parse(cijena.ToString()) * kolicina;
                    double resultatIznos2 = Math.Round(iznos, 2);
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
            if (localMaterijali.GetBoolean("potvrda", false))
                intent = new Intent(this, typeof(Activity_Potvrda_page4));
            else if (localMaterijali.GetBoolean("materijaliPoPoziciji", false))
                intent = new Intent(this, typeof(Activity_PotroseniMaterijali_Pozicija));
            else
                intent = new Intent(this, typeof(Activity_PotroseniMaterijali));

            StartActivity(intent);
        }

        public void UpdateMaterijala()
        {
            messageKolicina.Visibility = Android.Views.ViewStates.Invisible;
            db.Query<DID_AnketaMaterijali>(
                "UPDATE DID_AnketaMaterijali " +
                "SET Kolicina = ?, Iznos = ? " +
                "WHERE Id = ?", kolicinaInput.Text.Replace(',', '.'), IznosData.Text.Replace(',', '.'), materijalId);

            // Update stanja na skladistu
            var skladiste = db.Query<DID_RadniNalog>(
                "SELECT * " +
                "FROM DID_RadniNalog " +
                "WHERE Id = ?", radniNalog).FirstOrDefault();

            // UPDATE Kolicine na skladistu
            decimal novaKolicinaNaSkladistu = Convert.ToDecimal(staraKolicinaNaSkladistu + Convert.ToDecimal(materijal.Kolicina)) - Convert.ToDecimal(kolicinaInput.Text);
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

                foreach (var materijal in listaMaterijalaPotvrda)
                {
                    db.Execute(
                        "UPDATE DID_Potvrda_Materijal " +
                        "SET SinhronizacijaPrivremeniKljuc = ? " +
                        "WHERE Id = ?", materijal.Id, materijal.Id);
                }
            }
        }

        public void SpremiBtn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(kolicinaInput.Text) && Convert.ToDecimal(kolicinaInput.Text) > 0)
            {
                int statusLokacije = db.Query<DID_RadniNalog_Lokacija>(
                    "SELECT * " +
                    "FROM DID_RadniNalog_Lokacija " +
                    "WHERE Lokacija = ? " +
                    "AND RadniNalog = ?", lokacijaId, radniNalog).FirstOrDefault().Status;

                string nazivLokacije = db.Query<DID_Lokacija>(
                    "SELECT * " +
                    "FROM DID_Lokacija " +
                    "WHERE SAN_Id = ?", lokacijaId).FirstOrDefault().SAN_Naziv;

                if (statusLokacije == 2)
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                    alert.SetTitle("Upozorenje!");
                    alert.SetMessage("Lokacija " + nazivLokacije + " je zaključana. Jeste li sigurni da želite nastaviti?");
                    alert.SetPositiveButton("DA", (senderAlert, arg) =>
                    {
                        UpdateMaterijala();

                        List<DID_RadniNalog_Lokacija> sveLokacije = db.Query<DID_RadniNalog_Lokacija>(
                            "SELECT * " +
                            "FROM DID_RadniNalog_Lokacija " +
                            "WHERE RadniNalog = ?", radniNalog);

                        List<DID_RadniNalog_Lokacija> izvrseneLokacije = db.Query<DID_RadniNalog_Lokacija>(
                            "SELECT * " +
                            "FROM DID_RadniNalog_Lokacija " +
                            "WHERE RadniNalog = ? " +
                            "AND Status = 3", radniNalog);

                        if (sveLokacije.Count == izvrseneLokacije.Count)
                        {
                            db.Query<DID_RadniNalog>(
                                "UPDATE DID_RadniNalog " +
                                "SET Status = ?, DatumIzvrsenja = ? " +
                                "WHERE Id = ?", 5, DateTime.Now, radniNalog);
                        }
                        else if (izvrseneLokacije.Any())
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

                        if (localMaterijali.GetBoolean("potvrda", false))
                            intent = new Intent(this, typeof(Activity_Potvrda_page4));
                        else if (localMaterijali.GetBoolean("materijaliPoPoziciji", false))
                            intent = new Intent(this, typeof(Activity_PotroseniMaterijali_Pozicija));
                        else
                            intent = new Intent(this, typeof(Activity_PotroseniMaterijali));
                        StartActivity(intent);
                    });

                    alert.SetNegativeButton("NE", (senderAlert, arg) => { });
                    Dialog dialog = alert.Create();
                    dialog.Show();
                }
                else
                {
                    UpdateMaterijala();
                    intent = new Intent(this, typeof(Activity_PotroseniMaterijali_Pozicija));
                    StartActivity(intent);
                }
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
            {
                if (localMaterijali.GetBoolean("potvrda", false))
                    intent = new Intent(this, typeof(Activity_Potvrda_page4));
                else if (localMaterijali.GetBoolean("materijaliPoPoziciji", false))
                    intent = new Intent(this, typeof(Activity_PotroseniMaterijali_Pozicija));
                else
                    intent = new Intent(this, typeof(Activity_PotroseniMaterijali));
            }
            StartActivity(intent);
            return base.OnOptionsItemSelected(item);
        }
    }
}