using System;
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
    [Activity(Label = "Activity_ProvedbeniPlan_page2", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_ProvedbeniPlan_page2 : Activity
    {
        Intent intent;
        EditText zapazanjaGlodavciInput, zapazanjaInsektiInput, zapazanjaGlodavciInput1, zapazanjaInsektiInput1, glodavciKriticneTockeInput, insektiKriticneTockeInput, preporukeInput, napomeneInput;
        DatePicker datumIzlaganjaGlodavci, datumIzvidaGlodavci, datumIzlaganjaInsekti, datumIzvidaInsekti;
        LinearLayout editLayout, showLayout;
        TextView datumIzlaganjaGlodavciTV, datumIzvidaGlodavciTV, datumIzlaganjaInsektiTV, datumIzvidaInsektiTV;
        int lokacijaId;

        static readonly ISharedPreferences localProvedbeniPlan = Application.Context.GetSharedPreferences("localProvedbeniPlan", FileCreationMode.Private);
        static ISharedPreferencesEditor localProvedbeniPlanEdit = localProvedbeniPlan.Edit();

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.provedbeniPlan_page2);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            datumIzlaganjaGlodavci = FindViewById<DatePicker>(Resource.Id.datumIzlaganjaGlodavci);
            datumIzvidaGlodavci = FindViewById<DatePicker>(Resource.Id.datumIzvidaGlodavci);
            datumIzlaganjaInsekti = FindViewById<DatePicker>(Resource.Id.datumIzlaganjaInsekti);
            datumIzvidaInsekti = FindViewById<DatePicker>(Resource.Id.datumIzvidaInsekti);
            zapazanjaGlodavciInput = FindViewById<EditText>(Resource.Id.zapazanjaGlodavciInput);
            zapazanjaInsektiInput = FindViewById<EditText>(Resource.Id.zapazanjaInsektiInput);
            glodavciKriticneTockeInput = FindViewById<EditText>(Resource.Id.glodavciKriticneTockeInput);
            insektiKriticneTockeInput = FindViewById<EditText>(Resource.Id.insektiKriticneTockeInput);
            preporukeInput = FindViewById<EditText>(Resource.Id.preporukeInput);
            napomeneInput = FindViewById<EditText>(Resource.Id.napomeneInput);
            editLayout = FindViewById<LinearLayout>(Resource.Id.editLayout);
            showLayout = FindViewById<LinearLayout>(Resource.Id.showLayout);
            zapazanjaGlodavciInput1 = FindViewById<EditText>(Resource.Id.zapazanjaGlodavciInput1);
            zapazanjaInsektiInput1 = FindViewById<EditText>(Resource.Id.zapazanjaInsektiInput1);
            datumIzlaganjaGlodavciTV = FindViewById<TextView>(Resource.Id.datumIzlaganjaGlodavciTV);
            datumIzvidaGlodavciTV = FindViewById<TextView>(Resource.Id.datumIzvidaGlodavciTV);
            datumIzlaganjaInsektiTV = FindViewById<TextView>(Resource.Id.datumIzlaganjaInsektiTV);
            datumIzvidaInsektiTV = FindViewById<TextView>(Resource.Id.datumIzvidaInsektiTV);

            SetActionBar(toolbar);  
            ActionBar.Title = "Provedbeni plan";
            lokacijaId = localProvedbeniPlan.GetInt("lokacijaId", 0);
            var datum = DateTime.Now.Date;
            datumIzlaganjaGlodavci.UpdateDate(datum.Year, datum.Month - 1, datum.Day);
            datumIzvidaGlodavci.UpdateDate(datum.Year, datum.Month - 1, datum.Day);
            datumIzlaganjaInsekti.UpdateDate(datum.Year, datum.Month - 1, datum.Day);
            datumIzvidaInsekti.UpdateDate(datum.Year, datum.Month - 1, datum.Day);

            CheckIfProvedbeniPlanExist();
        }

        public void CheckIfProvedbeniPlanExist()
        {
            var provedbeniPlan = db.Query<DID_ProvedbeniPlan>(
                "SELECT * " +
                "FROM DID_ProvedbeniPlan " +
                "WHERE Lokacija = ? " +
                "AND Godina = ?", lokacijaId, DateTime.Now.Year.ToString());

            if (provedbeniPlan.Any())
            {
                datumIzlaganjaGlodavciTV.Text = provedbeniPlan.FirstOrDefault().GlodavciDatumPostavljanjaKlopki.ToLongDateString();
                datumIzvidaGlodavciTV.Text = provedbeniPlan.FirstOrDefault().GlodavciDatumIzvida.ToLongDateString();
                datumIzlaganjaInsektiTV.Text = provedbeniPlan.FirstOrDefault().InsektiDatumPostavljanjaKlopki.ToLongDateString();
                datumIzvidaInsektiTV.Text = provedbeniPlan.FirstOrDefault().InsektiDatumIzvida.ToLongDateString();
                zapazanjaGlodavciInput1.Text = provedbeniPlan.FirstOrDefault().GlodavciZapazanja;
                zapazanjaInsektiInput1.Text = provedbeniPlan.FirstOrDefault().InsektiZapazanja;
                glodavciKriticneTockeInput.Text = provedbeniPlan.FirstOrDefault().GlodavciKriticneTocke;
                insektiKriticneTockeInput.Text = provedbeniPlan.FirstOrDefault().InsektiKriticneTocke;
                preporukeInput.Text = provedbeniPlan.FirstOrDefault().Preporuke;
                napomeneInput.Text = provedbeniPlan.FirstOrDefault().Napomene;

                localProvedbeniPlanEdit.Commit();
                DisableInput();
            }
            else
                IsPageVisited();
        }

        public void DisableInput()
        {
            // Disablanje inputa -> provedbeni plan nije moguće editirati
            glodavciKriticneTockeInput.Enabled = false;
            insektiKriticneTockeInput.Enabled = false;
            preporukeInput.Enabled = false;
            napomeneInput.Enabled = false;
            zapazanjaGlodavciInput1.Enabled = false;
            zapazanjaInsektiInput1.Enabled = false;

            showLayout.Visibility = Android.Views.ViewStates.Visible;
            editLayout.Visibility = Android.Views.ViewStates.Gone;
        }

        public void IsPageVisited()
        {
            if(localProvedbeniPlan.GetBoolean("visited_page2", false))
            {
                zapazanjaGlodavciInput.Text = localProvedbeniPlan.GetString("zapazanjaGlodavciInput_page2", null);
                zapazanjaInsektiInput.Text = localProvedbeniPlan.GetString("zapazanjaInsektiInput_page2", null);
                glodavciKriticneTockeInput.Text = localProvedbeniPlan.GetString("glodavciKriticneTockeInput_page2", null);
                insektiKriticneTockeInput.Text = localProvedbeniPlan.GetString("insektiKriticneTockeInput_page2", null);
                preporukeInput.Text = localProvedbeniPlan.GetString("preporukeInput_page2", null);
                napomeneInput.Text = localProvedbeniPlan.GetString("napomeneInput_page2", null);

                var datumIzlaganjaG = Convert.ToDateTime(localProvedbeniPlan.GetString("datumIzlaganjaGlodavci_page2", null));
                var datumIzvidaG = Convert.ToDateTime(localProvedbeniPlan.GetString("datumIzvidaGlodavci_page2", null));
                var datumIzlaganjaI = Convert.ToDateTime(localProvedbeniPlan.GetString("datumIzlaganjaInsekti_page2", null));
                var datumIzvidaI = Convert.ToDateTime(localProvedbeniPlan.GetString("datumIzvidaInsekti_page2", null));

                datumIzlaganjaGlodavci.UpdateDate(datumIzlaganjaG.Year, datumIzlaganjaG.Month - 1, datumIzlaganjaG.Day);
                datumIzvidaGlodavci.UpdateDate(datumIzvidaG.Year, datumIzvidaG.Month - 1, datumIzvidaG.Day);
                datumIzlaganjaInsekti.UpdateDate(datumIzlaganjaI.Year, datumIzlaganjaI.Month - 1, datumIzlaganjaI.Day);
                datumIzvidaInsekti.UpdateDate(datumIzvidaI.Year, datumIzvidaI.Month - 1, datumIzvidaI.Day);
            }
        }

        public void SaveState()
        {
            localProvedbeniPlanEdit.PutString("datumIzlaganjaGlodavci_page2", datumIzlaganjaGlodavci.DateTime.ToString());
            localProvedbeniPlanEdit.PutString("datumIzvidaGlodavci_page2", datumIzvidaGlodavci.DateTime.ToString());
            localProvedbeniPlanEdit.PutString("datumIzlaganjaInsekti_page2", datumIzlaganjaInsekti.DateTime.ToString());
            localProvedbeniPlanEdit.PutString("datumIzvidaInsekti_page2", datumIzvidaInsekti.DateTime.ToString());
            localProvedbeniPlanEdit.PutString("zapazanjaGlodavciInput_page2", zapazanjaGlodavciInput.Text);
            localProvedbeniPlanEdit.PutString("zapazanjaInsektiInput_page2", zapazanjaInsektiInput.Text);
            localProvedbeniPlanEdit.PutString("glodavciKriticneTockeInput_page2", glodavciKriticneTockeInput.Text);
            localProvedbeniPlanEdit.PutString("insektiKriticneTockeInput_page2", insektiKriticneTockeInput.Text);
            localProvedbeniPlanEdit.PutString("preporukeInput_page2", preporukeInput.Text);
            localProvedbeniPlanEdit.PutString("napomeneInput_page2", napomeneInput.Text);
            localProvedbeniPlanEdit.PutBoolean("visited_page2", true);
            localProvedbeniPlanEdit.Commit();
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
                intent = new Intent(this, typeof(Activity_ProvedbeniPlan_page1));
            }
            else if(item.TitleFormatted.ToString() == "naprijed")
            {
                SaveState();
                intent = new Intent(this, typeof(Activity_ProvedbeniPlan_page3));
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