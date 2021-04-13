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
    [Activity(Label = "Activity_PotroseniMaterijali", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_PotroseniMaterijali : Activity
    {
        RecyclerView materijaliListView;
        RecyclerView.LayoutManager mLayoutManager;
        Adapter_PotroseniMaterijali mAdapter;
        Intent intent;
        TextView ukupanIznosTextView, message, messagePotvrda, radniNalogTV;
        Spinner spinnerLokacija, spinnerPartner, spinnerRadniNalog;
        ScrollView prikazMaterijala;
        LinearLayout prikazDropdowna;
        RelativeLayout ukupnoKn;
        Button noviMaterijalBtn;
        List<DID_AnketaMaterijali> filtriranePotrosnje;
        List<string> partneriSifreList, lokacijeIdList, radniNaloziIdList;
        int lokacijaId, radniNalogId;
        decimal ukupanIznos, kolicinaBefore, potroseno, stanjeKolicine;
        string sifraPartnera;

        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);
        static ISharedPreferencesEditor radniNaloziEdit = localRadniNalozi.Edit();
        static readonly ISharedPreferences localMaterijali = Application.Context.GetSharedPreferences("localMaterijali", FileCreationMode.Private);
        static ISharedPreferencesEditor localMaterijaliEdit = localMaterijali.Edit();
        static readonly ISharedPreferences localKomitentLokacija = Application.Context.GetSharedPreferences("localKomitentLokacija", FileCreationMode.Private);
        static ISharedPreferencesEditor localKomitentLokacijaEdit = localKomitentLokacija.Edit();
        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.potroseniMaterijali);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            materijaliListView = FindViewById<RecyclerView>(Resource.Id.materijaliListView);
            ukupanIznosTextView = FindViewById<TextView>(Resource.Id.ukupanIznosTextView);
            radniNalogTV = FindViewById<TextView>(Resource.Id.radniNalogTV);
            spinnerPartner = FindViewById<Spinner>(Resource.Id.spinnerPartner);
            spinnerLokacija = FindViewById<Spinner>(Resource.Id.spinnerLokacija);
            spinnerRadniNalog = FindViewById<Spinner>(Resource.Id.spinnerRadniNalog);
            message = FindViewById<TextView>(Resource.Id.message);
            messagePotvrda = FindViewById<TextView>(Resource.Id.messagePotvrda);
            prikazMaterijala = FindViewById<ScrollView>(Resource.Id.prikazMaterijala);
            prikazDropdowna = FindViewById<LinearLayout>(Resource.Id.prikazDropdowna);
            ukupnoKn = FindViewById<RelativeLayout>(Resource.Id.ukupnoKn);
            noviMaterijalBtn = FindViewById<Button>(Resource.Id.noviMaterijalBtn);

            SetActionBar(toolbar);
            ActionBar.Title = "Popis materijala";
            spinnerPartner.ItemSelected += SpinnerPartner_ItemSelected;
            spinnerLokacija.ItemSelected += SpinnerLokacija_ItemSelected;
            spinnerRadniNalog.ItemSelected += SpinnerRadniNalog_ItemSelected;
            noviMaterijalBtn.Click += NoviMaterijalBtn_Click;
            lokacijaId = localKomitentLokacija.GetInt("lokacijaId", 0);
            mLayoutManager = new LinearLayoutManager(this);
            materijaliListView.SetLayoutManager(mLayoutManager);
            localMaterijaliEdit.PutBoolean("button", false);
            localMaterijaliEdit.Commit();
            List<DID_RadniNalog> radniNalozi = db.Query<DID_RadniNalog>(
                "SELECT rn.Id, rn.Godina, rn.Broj, rn.Status, rn.PokretnoSkladiste, rn.Voditelj, rn.Izdavatelj, rn.Primatelj, rn.DatumOd, rn.DatumDo, rn.DatumIzrade, rn.DatumIzvrsenja, rn.SinhronizacijaDatumVrijeme, rn.SinhronizacijaPrivremeniKljuc " +
                "FROM DID_RadniNalog rn " +
                "INNER JOIN DID_RadniNalog_Lokacija ON rn.Id = DID_RadniNalog_Lokacija.RadniNalog " +
                "INNER JOIN DID_LokacijaPozicija ON DID_RadniNalog_Lokacija.Lokacija = DID_LokacijaPozicija.SAN_Id " +
                "INNER JOIN DID_AnketaMaterijali ON DID_LokacijaPozicija.POZ_Id = DID_AnketaMaterijali.PozicijaId " +
                "GROUP BY Broj, Godina");

            if (radniNalozi.Any())
            {
                ToggleVisibility(true);

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
            }
            else
                ToggleVisibility(false);
        }

        public override void OnBackPressed()
        {
            intent = new Intent(this, typeof(Activity_Pocetna));
            StartActivity(intent);
        }

        public void ToggleVisibility(bool visible)
        {
            if (visible)
            {
                message.Visibility = Android.Views.ViewStates.Gone;
                spinnerRadniNalog.Visibility = Android.Views.ViewStates.Visible;
                radniNalogTV.Visibility = Android.Views.ViewStates.Visible;
                prikazDropdowna.Visibility = Android.Views.ViewStates.Visible;
                prikazMaterijala.Visibility = Android.Views.ViewStates.Visible;
                ukupnoKn.Visibility = Android.Views.ViewStates.Visible;
            }
            else
            {
                message.Visibility = Android.Views.ViewStates.Visible;
                ukupnoKn.Visibility = Android.Views.ViewStates.Gone;
                spinnerRadniNalog.Visibility = Android.Views.ViewStates.Gone;
                radniNalogTV.Visibility = Android.Views.ViewStates.Gone;
                prikazDropdowna.Visibility = Android.Views.ViewStates.Gone;
                prikazMaterijala.Visibility = Android.Views.ViewStates.Gone;
            }
        }

        public void NoviMaterijalBtn_Click(object sender, EventArgs args)
        {
            radniNaloziEdit.PutInt("id", radniNalogId);
            radniNaloziEdit.Commit();
            localMaterijaliEdit.PutString("sifraPartnera", sifraPartnera);
            localMaterijaliEdit.Commit();
            localKomitentLokacijaEdit.PutInt("lokacijaId", lokacijaId);
            localKomitentLokacijaEdit.Commit();
            localMaterijaliEdit.PutBoolean("materijaliPoPoziciji", true);
            localMaterijaliEdit.Commit();
            intent = new Intent(this, typeof(Activity_PotroseniMaterijali_List));
            StartActivity(intent);
        }

        private void SpinnerRadniNalog_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            radniNalogId = Convert.ToInt32(radniNaloziIdList[e.Position]);
            List<T_KUPDOB> komitentiList = db.Query<T_KUPDOB>(
                "SELECT * " +
                "FROM T_KUPDOB " +
                "INNER JOIN DID_Lokacija ON T_KUPDOB.SIFRA = DID_Lokacija.SAN_KD_SIFRA " +
                "INNER JOIN DID_RadniNalog_Lokacija ON DID_Lokacija.SAN_Id = DID_RadniNalog_Lokacija.Lokacija " +
                "INNER JOIN DID_LokacijaPozicija ON DID_RadniNalog_Lokacija.Lokacija = DID_LokacijaPozicija.SAN_Id " +
                "INNER JOIN DID_AnketaMaterijali ON DID_LokacijaPozicija.POZ_Id = DID_AnketaMaterijali.PozicijaId " +
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
            List<DID_Lokacija> lokacijaList = db.Query<DID_Lokacija>(
                "SELECT * " +
                "FROM DID_Lokacija " +
                "INNER JOIN DID_LokacijaPozicija ON DID_Lokacija.SAN_Id = DID_LokacijaPozicija.SAN_Id " +
                "INNER JOIN DID_AnketaMaterijali ON DID_LokacijaPozicija.POZ_Id = DID_AnketaMaterijali.PozicijaId " +
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
            spinnerLokacija.SetSelection(lokacijeIdList.IndexOf(lokacijaId.ToString()));
        }

        private void SpinnerLokacija_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            ukupanIznos = 0;
            lokacijaId = Convert.ToInt32(lokacijeIdList[e.Position]);
            List<DID_Potvrda> potvrda = db.Query<DID_Potvrda>(
               "SELECT * " +
               "FROM DID_Potvrda " +
               "WHERE Lokacija = ?", lokacijaId);

            if (potvrda.Any())
                messagePotvrda.Visibility = Android.Views.ViewStates.Visible;
            else
                messagePotvrda.Visibility = Android.Views.ViewStates.Invisible;

            filtriranePotrosnje = db.Query<DID_AnketaMaterijali>(
                 "SELECT mat.Id, mat.Cijena, mat.LokacijaId, TOTAL(mat.Iznos) AS Iznos, mat.RadniNalog, mat.PozicijaId, mat.MaterijalSifra, mat.MaterijalNaziv, mat.MjernaJedinica, TOTAL(mat.Kolicina) AS Kolicina " +
                 "FROM DID_AnketaMaterijali mat " +
                 "INNER JOIN DID_LokacijaPozicija poz On mat.PozicijaId = poz.POZ_Id " +
                 "WHERE poz.SAN_Id = ? " +
                 "AND mat.RadniNalog = ? " +
                 "GROUP BY mat.MaterijalNaziv", lokacijaId, radniNalogId);

            foreach (var materijal in filtriranePotrosnje)
                ukupanIznos += Math.Round(materijal.Iznos, 2);
            ukupanIznosTextView.Text = Math.Round(ukupanIznos, 2).ToString("F2").Replace('.', ',') + " kn";

            mAdapter = new Adapter_PotroseniMaterijali(filtriranePotrosnje);
            mAdapter.ItemClick += MAdapter_ItemClick;
            mAdapter.ItemDelete += MAdapter_ItemDelete;
            materijaliListView.SetAdapter(mAdapter);
        }

        public void DeleteMaterijal(DID_AnketaMaterijali materijal)
        {
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
                "AND RadniNalog = ?", materijal.Id, radniNalogId);
        }

        private void MAdapter_ItemDelete(object sender, int e)
        {
            string nazivLokacije = db.Query<DID_Lokacija>(
                "SELECT * " +
                "FROM DID_Lokacija " +
                "WHERE SAN_Id = ?", lokacijaId).FirstOrDefault().SAN_Naziv;

            List<DID_Potvrda> potvrda = db.Query<DID_Potvrda>(
                "SELECT * " +
                "FROM DID_Potvrda " +
                "WHERE Lokacija = ? " +
                "AND RadniNalog = ?", lokacijaId, radniNalogId);

            int statusLokacije = db.Query<DID_RadniNalog_Lokacija>(
                "SELECT * " +
                "FROM DID_RadniNalog_Lokacija " +
                "WHERE Lokacija = ? " +
                "AND RadniNalog = ?", lokacijaId, radniNalogId).FirstOrDefault().Status;

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
            radniNaloziEdit.PutInt("id", radniNalogId);
            radniNaloziEdit.Commit();
            localMaterijaliEdit.PutInt("id", filtriranePotrosnje[e].Id);
            localMaterijaliEdit.PutString("sifra", filtriranePotrosnje[e].MaterijalSifra);
            localMaterijaliEdit.PutString("sifraPartnera", sifraPartnera);
            localMaterijaliEdit.Commit();
            localKomitentLokacijaEdit.PutInt("lokacijaId", lokacijaId);
            localKomitentLokacijaEdit.Commit();
            intent = new Intent(this, typeof(Activity_PotroseniMaterijali_PrikazPozicija));
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