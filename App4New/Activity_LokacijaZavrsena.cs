using Infobit.DDD.Data;
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
using Android.Content.PM;

namespace App4New
{
    [Activity(Label = "Activity_LokacijaZavrsena", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_LokacijaZavrsena : Activity
    {
        TextView prikazText;
        Intent intent;
        Button odustaniBtn, spremiBtn;
        string lokacijaNaziv = localKomitentLokacija.GetString("lokacijaNaziv", null);
        int radniNalog = localRadniNalozi.GetInt("id", 0),
            radniNalogLokacijaId = localKomitentLokacija.GetInt("radniNalogLokacijaId", 0);

        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);
        static readonly ISharedPreferences localKomitentLokacija = Application.Context.GetSharedPreferences("localKomitentLokacija", FileCreationMode.Private);

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lokacijaZavrsena);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            prikazText = FindViewById<TextView>(Resource.Id.prikazText);
            spremiBtn = FindViewById<Button>(Resource.Id.spremiBtn);
            odustaniBtn = FindViewById<Button>(Resource.Id.odustaniBtn);

            SetActionBar(toolbar);
            ActionBar.Title = "Lokacija";
            spremiBtn.Click += SpremiBtn_Click;
            odustaniBtn.Click += OdustaniBtn_Click;
            prikazText.Text = "Za lokaciju " + lokacijaNaziv + " je izdana potvrda. Želite li spremiti lokaciju kao izvršenu?";
        }

        public void UpdateStatusRadniNalog()
        {
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
        }

        public void SpremiBtn_Click(object sender, EventArgs args)
        {
            //UPDATE RadniNalog lokacije
            db.Query<DID_RadniNalog_Lokacija>(
                "UPDATE DID_RadniNalog_Lokacija " +
                "SET Status = 3 " +
                "WHERE Id = ?", radniNalogLokacijaId);

            UpdateStatusRadniNalog();

            Toast.MakeText(this, "Lokacija " + lokacijaNaziv + " je zaključana!", ToastLength.Long).Show();
            intent = new Intent(this, typeof(Activity_Lokacije));
            StartActivity(intent);
        }

        public void OdustaniBtn_Click(object sender, EventArgs args)
        {
            //UPDATE RadniNalog lokacije
            db.Query<DID_RadniNalog_Lokacija>(
                "UPDATE DID_RadniNalog_Lokacija " +
                "SET Status = 2 " +
                "WHERE Id = ?", radniNalogLokacijaId);

            UpdateStatusRadniNalog();

            Toast.MakeText(this, "Lokacija " + lokacijaNaziv + " je otključana!", ToastLength.Long).Show();
            intent = new Intent(this, typeof(Activity_Lokacije));
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