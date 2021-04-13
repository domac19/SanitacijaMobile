using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Infobit.DDD.Data;
using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Content.PM;

namespace App4New
{
    [Activity(Label = "Activity_PotroseniMaterijali_PrikazPozicija", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_PotroseniMaterijali_PrikazPozicija : Activity
    {
        ListView materijaliListView;
        TextView materijalData, lokacijaData, partnerData;
        Intent intent;
        List<DID_LokacijaPozicija> pozicije;
        int radniNalog, lokacijaId;
        string materijalSifra;

        static readonly ISharedPreferences localMaterijali = Application.Context.GetSharedPreferences("localMaterijali", FileCreationMode.Private);
        static ISharedPreferencesEditor localMaterijaliEdit = localMaterijali.Edit();
        static readonly ISharedPreferences localKomitentLokacija = Application.Context.GetSharedPreferences("localKomitentLokacija", FileCreationMode.Private);
        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.potroseniMaterijali_PrikazPozicija);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            materijaliListView = FindViewById<ListView>(Resource.Id.materijaliListView);
            partnerData = FindViewById<TextView>(Resource.Id.partnerData);
            lokacijaData = FindViewById<TextView>(Resource.Id.lokacijaData);
            materijalData = FindViewById<TextView>(Resource.Id.materijalData);

            SetActionBar(toolbar);
            ActionBar.Title = "Pozicije";
            radniNalog = localRadniNalozi.GetInt("id", 0);
            materijalSifra = localMaterijali.GetString("sifra", null);
            lokacijaId = localKomitentLokacija.GetInt("lokacijaId", 0);
            lokacijaData.Text = db.Query<DID_Lokacija>(
                "SELECT * " +
                "FROM DID_Lokacija " +
                "WHERE SAN_Id = ?", lokacijaId).FirstOrDefault().SAN_Naziv;

            materijalData.Text = db.Query<T_NAZR>(
                "SELECT * " +
                "FROM T_NAZR " +
                "WHERE NAZR_SIFRA = ?", materijalSifra).FirstOrDefault().NAZR_NAZIV;
            partnerData.Text = db.Query<T_KUPDOB>(
                "SELECT * " +
                "FROM T_KUPDOB " +
                "WHERE SIFRA = ?", localMaterijali.GetString("sifraPartnera", null)).FirstOrDefault().NAZIV;

            pozicije = db.Query<DID_LokacijaPozicija>(
                "SELECT * " +
                "FROM DID_LokacijaPozicija poz " +
                "INNER JOIN DID_AnketaMaterijali mat ON poz.POZ_Id = mat.PozicijaId " +
                "WHERE mat.RadniNalog = ? " +
                "AND poz.SAN_Id = ? " +
                "AND mat.MaterijalSifra = ? " +
                "GROUP BY poz.POZ_Broj, poz.POZ_BrojOznaka", radniNalog, lokacijaId, materijalSifra);

            materijaliListView.Adapter = new Adapter_MaterijaliPozicija(this, pozicije);
            materijaliListView.ItemClick += PartneriListView_ItemClick;
        }

        private void PartneriListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            localMaterijaliEdit.PutBoolean("visitedAnkete", false);
            localMaterijaliEdit.PutInt("pozicijaId", pozicije[e.Position].POZ_Id);
            localMaterijaliEdit.PutInt("lokacijaId", lokacijaId);
            localMaterijaliEdit.Commit();
            intent = new Intent(this, typeof(Activity_PotroseniMaterijali_Pozicija));
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
                intent = new Intent(this, typeof(Activity_PotroseniMaterijali));
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