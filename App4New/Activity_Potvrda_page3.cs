using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Infobit.DDD.Data;
using SQLite;
using System.Collections.Generic;
using System.IO;
using Android.Content.PM;

namespace App4New
{
    [Activity(Label = "Activity_Potvrda_page3", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_Potvrda_page3 : Activity
    {
        Intent intent;
        RecyclerView nametniciListView;
        RecyclerView.LayoutManager mLayoutManager;
        Adapter_Nametnici_ProvedbeniPlan mAdapter;
        List<DID_Nametnik> nametnici;

        static readonly ISharedPreferences localPotvrda = Application.Context.GetSharedPreferences("potvrda", FileCreationMode.Private);
        static ISharedPreferencesEditor localPotvrdaEdit = localPotvrda.Edit();

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.potvrda_page3);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            nametniciListView = FindViewById<RecyclerView>(Resource.Id.nametniciListView);

            SetActionBar(toolbar);
            ActionBar.Title = "Potvrda - nametnici";
            nametnici = db.Query<DID_Nametnik>(
                "SELECT * " +
                "FROM DID_Nametnik");
            mLayoutManager = new LinearLayoutManager(this);
            mAdapter = new Adapter_Nametnici_ProvedbeniPlan(nametnici);
            nametniciListView.SetLayoutManager(mLayoutManager);
            mAdapter.CheckboxClick += MAdapter_CheckboxClick;
            nametniciListView.SetAdapter(mAdapter);

            CheckIfPotvrdaExist();
        }

        public void CheckIfPotvrdaExist()
        {
            if(localPotvrda.GetBoolean("edit", false))
            {
                // Prikaz dodanih nametnika na provedbenom planu
                var nametniciList = db.Query<DID_Potvrda_Nametnik>(
                    "SELECT * " +
                    "FROM DID_Potvrda_Nametnik " +
                    "WHERE Potvrda = ?", localPotvrda.GetInt("id", 0));

                foreach (var nametnik in nametniciList)
                    localPotvrdaEdit.PutString("nametnik" + nametnik.Nametnik, nametnik.Nametnik);

                localPotvrdaEdit.Commit();
            }
        }

        public void MAdapter_CheckboxClick(object sender, int e)
        {
            if (localPotvrda.GetString("nametnik" + nametnici[e].Sifra, null) != null)
                localPotvrdaEdit.PutString("nametnik" + nametnici[e].Sifra, null);
            else
                localPotvrdaEdit.PutString("nametnik" + nametnici[e].Sifra, nametnici[e].Naziv);

            localPotvrdaEdit.Commit();
        }

        public void SaveState()
        {
            localPotvrdaEdit.PutBoolean("potvrdaPage3", true);
            localPotvrdaEdit.Commit();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menuBackNextHome, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.TitleFormatted.ToString() == "Početna")
                intent = new Intent(this, typeof(Activity_Pocetna));
            else if (item.TitleFormatted.ToString() == "nazad")
            {
                SaveState();
                intent = new Intent(this, typeof(Activity_Potvrda_page2));
            }
            else if (item.TitleFormatted.ToString() == "naprijed")
            {
                SaveState();
                intent = new Intent(this, typeof(Activity_Potvrda_page4));
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