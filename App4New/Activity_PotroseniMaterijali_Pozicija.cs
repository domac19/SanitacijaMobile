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
    [Activity(Label = "Activity_PotroseniMaterijali_Pozicija", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_PotroseniMaterijali_Pozicija : Activity
    {
        RecyclerView materijaliListView;
        RecyclerView.LayoutManager mLayoutManager;
        Adapter_PotroseniMaterijali mAdapter;
        Intent intent;
        TextView ukupanIznosTextView, message, pozicijaData, lokacijaData, partnerData;
        List<DID_AnketaMaterijali> filtriranePotrosnje;
        ScrollView prikazMaterijala;
        Button noviMaterijalBtn;
        decimal ukupanIznos;
        int radniNalog, lokacija, materijalId;
        string materijalSifra, sifraSkladista;

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
            SetContentView(Resource.Layout.potroseniMaterijali_Pozicija);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            materijaliListView = FindViewById<RecyclerView>(Resource.Id.materijaliListView);
            ukupanIznosTextView = FindViewById<TextView>(Resource.Id.ukupanIznosTextView);
            message = FindViewById<TextView>(Resource.Id.message);
            prikazMaterijala = FindViewById<ScrollView>(Resource.Id.prikazMaterijala);
            pozicijaData = FindViewById<TextView>(Resource.Id.pozicijaData);
            lokacijaData = FindViewById<TextView>(Resource.Id.lokacijaData);
            partnerData = FindViewById<TextView>(Resource.Id.partnerData);
            noviMaterijalBtn = FindViewById<Button>(Resource.Id.noviMaterijalBtn);
            SetActionBar(toolbar);
            ActionBar.Title = "Popis materijala";
            mLayoutManager = new LinearLayoutManager(this);
            materijaliListView.SetLayoutManager(mLayoutManager);
            message.Visibility = Android.Views.ViewStates.Gone;
            lokacija = localMaterijali.GetInt("lokacijaId", 0);
            materijalSifra = localMaterijali.GetString("sifra", null);
            var visitedOdradeneAnkete = localMaterijali.GetBoolean("visitedAnkete", false);

            noviMaterijalBtn.Click += NoviMaterijalBtn_Click;
            partnerData.Text = db.Query<T_KUPDOB>(
                "SELECT NAZIV " +
                "FROM T_KUPDOB " +
                "WHERE SIFRA = ?", localMaterijali.GetString("sifraPartnera", null)).FirstOrDefault().NAZIV;
            lokacijaData.Text = db.Query<DID_Lokacija>(
                "SELECT * " +
                "FROM DID_Lokacija " +
                "WHERE SAN_Id = ?", lokacija).FirstOrDefault().SAN_Naziv;
            DID_LokacijaPozicija pozicija = db.Query<DID_LokacijaPozicija>(
                "SELECT * " +
                "FROM DID_LokacijaPozicija " +
                "WHERE POZ_Id = ?", localMaterijali.GetInt("pozicijaId", 0)).FirstOrDefault();
            pozicijaData.Text = pozicija.POZ_Broj + pozicija.POZ_BrojOznaka;
            radniNalog = localRadniNalozi.GetInt("id", 0);
            sifraSkladista = db.Query<DID_RadniNalog>(
                "SELECT * " +
                "FROM DID_RadniNalog " +
                "WHERE Id = ?", radniNalog).FirstOrDefault().PokretnoSkladiste;

            if (visitedOdradeneAnkete)
            {
                filtriranePotrosnje = db.Query<DID_AnketaMaterijali>(
                    "SELECT mat.Id, mat.Cijena, mat.LokacijaId, TOTAL(mat.Iznos) AS Iznos, mat.RadniNalog, mat.PozicijaId, mat.MaterijalSifra, mat.MaterijalNaziv, mat.MjernaJedinica, TOTAL(mat.Kolicina) AS Kolicina " +
                    "FROM DID_AnketaMaterijali mat " +
                    "WHERE mat.PozicijaId = ? " +
                    "AND mat.RadniNalog = ? " +
                    "GROUP BY mat.MaterijalNaziv", pozicija.POZ_Id, radniNalog);
            }
            else
            {
                filtriranePotrosnje = db.Query<DID_AnketaMaterijali>(
                    "SELECT mat.Id, mat.Cijena, mat.LokacijaId, TOTAL(mat.Iznos) AS Iznos, mat.RadniNalog, mat.PozicijaId, mat.MaterijalSifra, mat.MaterijalNaziv, mat.MjernaJedinica, TOTAL(mat.Kolicina) AS Kolicina " +
                    "FROM DID_AnketaMaterijali mat " +
                    "WHERE mat.PozicijaId = ? " +
                    "AND mat.RadniNalog = ? " +
                    "AND mat.MaterijalSifra = ? " +
                    "GROUP BY mat.MaterijalNaziv", pozicija.POZ_Id, radniNalog, materijalSifra);
            }
            
            
            if (filtriranePotrosnje.Any())
            {
                prikazMaterijala.Visibility = Android.Views.ViewStates.Visible;
                message.Visibility = Android.Views.ViewStates.Gone;
                mAdapter = new Adapter_PotroseniMaterijali(filtriranePotrosnje);
                mAdapter.ItemClick += MAdapter_ItemClick;
                mAdapter.ItemDelete += MAdapter_ItemDelete;
                materijaliListView.SetAdapter(mAdapter);

                foreach (var materijal in filtriranePotrosnje)
                    ukupanIznos += materijal.Iznos;
                ukupanIznosTextView.Text = ukupanIznos.ToString("F2") + " kn";
            }
            else
            {
                ukupanIznosTextView.Text = "00.00 kn";
                prikazMaterijala.Visibility = Android.Views.ViewStates.Gone;
                message.Visibility = Android.Views.ViewStates.Visible;
            }
        }

        public void UpdateMaterijala()
        {
            decimal kolicinaBefore = db.Query<DID_AnketaMaterijali>(
                "SELECT * " +
                "FROM DID_AnketaMaterijali " +
                "WHERE Id = ?", materijalId).FirstOrDefault().Kolicina;

            List<DID_RadniNalog_Materijal> materijali = db.Query<DID_RadniNalog_Materijal>(
                "SELECT * " +
                "FROM DID_RadniNalog_Materijal " +
                "WHERE Materijal = ? " +
                "AND RadniNalog = ?", materijalSifra, radniNalog);

            if (materijali.Any())
            {
                decimal stanjeKolicine = db.Query<DID_RadniNalog_Materijal>(
                    "SELECT * " +
                    "FROM DID_RadniNalog_Materijal " +
                    "WHERE Materijal = ? " +
                    "AND RadniNalog = ?", materijalSifra, radniNalog).FirstOrDefault().StanjeKolicine + Convert.ToDecimal(kolicinaBefore);
                decimal potroseno = db.Query<DID_RadniNalog_Materijal>(
                    "SELECT * " +
                    "FROM DID_RadniNalog_Materijal " +
                    "WHERE Materijal = ? " +
                    "AND RadniNalog = ?", materijalSifra, radniNalog).FirstOrDefault().Izdano - stanjeKolicine;
                db.Query<DID_RadniNalog_Materijal>(
                  "UPDATE DID_RadniNalog_Materijal " +
                  "SET Izdano = ?, Utroseno = ? " +
                  "WHERE Materijal = ? " +
                  "AND RadniNalog = ?", stanjeKolicine, potroseno, materijalSifra, radniNalog);
            }
            
            decimal kolicina = db.Query<DID_StanjeSkladista>(
                "SELECT * " +
                "FROM DID_StanjeSkladista " +
                "WHERE Materijal = ? " +
                "AND Skladiste = ?", materijalSifra, sifraSkladista).FirstOrDefault().Kolicina + Convert.ToDecimal(kolicinaBefore);
            db.Query<DID_StanjeSkladista>(
                "UPDATE DID_StanjeSkladista " +
                "SET Kolicina = ? " +
                "WHERE Materijal = ? " +
                "AND Skladiste = ?", kolicina, materijalSifra, sifraSkladista);
            db.Query<DID_AnketaMaterijali>("DELETE FROM DID_AnketaMaterijali WHERE Id = ?", materijalId);

            intent = new Intent(this, typeof(Activity_PotroseniMaterijali_Pozicija));
            StartActivity(intent);
        }

        public void DeleteMaterijal(DID_AnketaMaterijali materijal)
        {
            db.Query<DID_AnketaMaterijali>(
                "DELETE FROM DID_AnketaMaterijali " +
                "WHERE Id = ? " +
                "AND RadniNalog = ?", materijalId, radniNalog);


            var skladiste = db.Query<DID_RadniNalog>(
                "SELECT * " +
                "FROM DID_RadniNalog " +
                "WHERE Id = ?", materijal.RadniNalog).FirstOrDefault();
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

            db.Query<DID_AnketaMaterijali>(
                "DELETE FROM DID_AnketaMaterijali " +
                "WHERE Id = ? " +
                "AND RadniNalog = ?", materijal.Id, radniNalog);
        }

        public void NoviMaterijalBtn_Click(object sender, EventArgs args)
        {
            //localMaterijaliEdit.PutBoolean("activity_PotroseniMaterijali_pozicija", true);
            localMaterijaliEdit.PutBoolean("materijaliPoPoziciji", true);
            localMaterijaliEdit.Commit();
            intent = new Intent(this, typeof(Activity_PotroseniMaterijali_List));
            StartActivity(intent);
        }

        private void MAdapter_ItemDelete(object sender, int e)
        {
            string nazivLokacije = db.Query<DID_Lokacija>(
                "SELECT * " +
                "FROM DID_Lokacija " +
                "WHERE SAN_Id = ?", lokacija).FirstOrDefault().SAN_Naziv;

            List<DID_Potvrda> potvrda = db.Query<DID_Potvrda>(
                "SELECT * " +
                "FROM DID_Potvrda " +
                "WHERE Lokacija = ? " +
                "AND RadniNalog = ?", lokacija, radniNalog);

            int statusLokacije = db.Query<DID_RadniNalog_Lokacija>(
                "SELECT * " +
                "FROM DID_RadniNalog_Lokacija " +
                "WHERE Lokacija = ? " +
                "AND RadniNalog = ?", lokacija, radniNalog).FirstOrDefault().Status;

            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Potvrda");
            if (potvrda.Any())
            {
                alert.SetMessage("Lokacija " + nazivLokacije + " je zaključana. Jeste li sigurni da želite obrisati materijal?");
                alert.SetPositiveButton("OBRIŠI", (senderAlert, arg) =>
                {
                    DeleteMaterijal(filtriranePotrosnje[e]);

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

                    intent = new Intent(this, typeof(Activity_PotroseniMaterijali));
                    StartActivity(intent);
                });

                alert.SetNegativeButton("ODUSTANI", (senderAlert, arg) => { });
                Dialog dialog = alert.Create();
                dialog.Show();
            }
            else
            {
                alert.SetMessage("Jeste li sigurni da želite obrisati materijal?");
                alert.SetPositiveButton("OBRIŠI", (senderAlert, arg) =>
                {
                    DeleteMaterijal(filtriranePotrosnje[e]);

                    var fsdfs = db.Query<DID_Potvrda_Materijal>("SELECT * FROM DID_Potvrda_Materijal");

                    intent = new Intent(this, typeof(Activity_PotroseniMaterijali));
                    StartActivity(intent);
                });

                alert.SetNegativeButton("ODUSTANI", (senderAlert, arg) => { });
                Dialog dialog = alert.Create();
                dialog.Show();
            }
        }

        private void MAdapter_ItemClick(object sender, int e)
        {
            localMaterijaliEdit.PutBoolean("materijaliPoPoziciji", true);
            localMaterijaliEdit.PutInt("id", filtriranePotrosnje[e].Id);
            localMaterijaliEdit.PutString("sifra", filtriranePotrosnje[e].MaterijalSifra);
            localMaterijaliEdit.PutInt("mjernaJedinica", filtriranePotrosnje[e].MjernaJedinica);
            localMaterijaliEdit.Commit();
            intent = new Intent(this, typeof(Activity_PotroseniMaterijali_Edit));
            StartActivity(intent);
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