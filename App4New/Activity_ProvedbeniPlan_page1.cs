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
    [Activity(Label = "Activity_ProvedbeniPlan_page1", Theme = "@style/Base.Theme.DesignDemo", NoHistory = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_ProvedbeniPlan_page1 : Activity
    {
        Intent intent;
        DatePicker datum;
        DateTime datumInput;
        TextView datumTV;
        EditText vrijemeInput, higijenskoStanjeLoseInput, gradevinskoStanjeLoseInput, uoceniGlodavciInput, uoceniInsektiInput, osaloInput;
        RadioButton higijenskoStanjeDobroRBtn, higijenskoStanjeLoseRBtn, gradevinskoStanjeLoseRBtn, gradevinskoStanjeDobroRBtn, tehnickaOpremljenostDobroRBtn, tehnickaOpremljenostLoseRBtn;
        RecyclerView nametniciListView;
        RecyclerView.LayoutManager mLayoutManager;
        Adapter_Nametnici_ProvedbeniPlan mAdapter;
        TimePicker vrijeme;
        LinearLayout datumLinearLayout;
        List<DID_Nametnik> nametnici;
        int lokacijaId, vrijemeSat, vrijemeMinuta;

        static readonly ISharedPreferences localProvedbeniPlan = Application.Context.GetSharedPreferences("localProvedbeniPlan", FileCreationMode.Private);
        static ISharedPreferencesEditor localProvedbeniPlanEdit = localProvedbeniPlan.Edit();

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.provedbeniPlan_page1);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            datum = FindViewById<DatePicker>(Resource.Id.datum);
            datumLinearLayout = FindViewById<LinearLayout>(Resource.Id.datumLinearLayout);
            datumTV = FindViewById<TextView>(Resource.Id.datumTV);
            //vrijemeInput = FindViewById<EditText>(Resource.Id.vrijemeInput);
            higijenskoStanjeDobroRBtn = FindViewById<RadioButton>(Resource.Id.higijenskoStanjeDobroRBtn);
            higijenskoStanjeLoseRBtn = FindViewById<RadioButton>(Resource.Id.higijenskoStanjeLoseRBtn);
            higijenskoStanjeLoseInput = FindViewById<EditText>(Resource.Id.higijenskoStanjeLoseInput);
            gradevinskoStanjeDobroRBtn = FindViewById<RadioButton>(Resource.Id.gradevinskoStanjeDobroRBtn);
            gradevinskoStanjeLoseRBtn = FindViewById<RadioButton>(Resource.Id.gradevinskoStanjeLoseRBtn);
            gradevinskoStanjeLoseInput = FindViewById<EditText>(Resource.Id.gradevinskoStanjeLoseInput);
            tehnickaOpremljenostDobroRBtn = FindViewById<RadioButton>(Resource.Id.tehnickaOpremljenostDobroRBtn);
            tehnickaOpremljenostLoseRBtn = FindViewById<RadioButton>(Resource.Id.tehnickaOpremljenostLoseRBtn);
            nametniciListView = FindViewById<RecyclerView>(Resource.Id.nametniciListView);

            vrijeme = FindViewById<TimePicker>(Resource.Id.vrijeme);
            vrijeme.TimeChanged += Vrijeme_TimeChanged;
            vrijeme.Hour = DateTime.Now.Hour;
            vrijeme.Minute = DateTime.Now.Minute;

            SetActionBar(toolbar);
            ActionBar.Title = "Provedbeni plan";
            higijenskoStanjeDobroRBtn.Click += HigijenskoStanjeDobroRBtn_Click;
            higijenskoStanjeLoseRBtn.Click += HigijenskoStanjeLoseRBtn_Click;
            gradevinskoStanjeDobroRBtn.Click += GradevinskoStanjeDobroRBtn_Click;
            gradevinskoStanjeLoseRBtn.Click += GradevinskoStanjeLoseRBtn_Click;
            //vrijemeInput.Text = DateTime.Now.ToString("HH:mm");
            lokacijaId = localProvedbeniPlan.GetInt("lokacijaId", 0);
            datumInput = DateTime.Now.Date;
            datum.UpdateDate(datumInput.Year, datumInput.Month - 1, datumInput.Day);

            nametnici = db.Query<DID_Nametnik>(
                "SELECT * " +
                "FROM DID_Nametnik");
            mLayoutManager = new LinearLayoutManager(this);
            mAdapter = new Adapter_Nametnici_ProvedbeniPlan(nametnici);
            nametniciListView.SetLayoutManager(mLayoutManager);
            mAdapter.CheckboxClick += MAdapter_CheckboxClick;
            nametniciListView.SetAdapter(mAdapter);

            CheckIfProvedbeniPlanExist();
        }

        private void Vrijeme_TimeChanged(object sender, TimePicker.TimeChangedEventArgs args)
        {
            vrijemeSat = args.HourOfDay;
            vrijemeMinuta = args.Minute;
        }

        public void MAdapter_CheckboxClick(object sender, int e)
        {
            if (localProvedbeniPlan.GetString("nametnik" + nametnici[e].Sifra, null) != null)
                localProvedbeniPlanEdit.PutString("nametnik" + nametnici[e].Sifra, null);
            else
                localProvedbeniPlanEdit.PutString("nametnik" + nametnici[e].Sifra, nametnici[e].Naziv);

            localProvedbeniPlanEdit.Commit();
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
                datumTV.Text = provedbeniPlan.FirstOrDefault().Datum.ToLongDateString();
                higijenskoStanjeLoseInput.Text = provedbeniPlan.FirstOrDefault().HigijenskoStanjeNedostatci;
                gradevinskoStanjeLoseInput.Text = provedbeniPlan.FirstOrDefault().GradjevinskoStanjeNedostatci;
                vrijeme.Hour = provedbeniPlan.FirstOrDefault().VrijemeIzvida.Hour;
                vrijeme.Minute = provedbeniPlan.FirstOrDefault().VrijemeIzvida.Minute;

                if (provedbeniPlan.FirstOrDefault().HigijenskoStanje)
                    higijenskoStanjeDobroRBtn.PerformClick();
                else
                    higijenskoStanjeLoseRBtn.PerformClick();

                if (provedbeniPlan.FirstOrDefault().GradjevinskoStanje)
                    gradevinskoStanjeDobroRBtn.PerformClick();
                else
                    gradevinskoStanjeLoseRBtn.PerformClick();

                if (provedbeniPlan.FirstOrDefault().TehnickaOpremljenost)
                    tehnickaOpremljenostDobroRBtn.PerformClick();
                else
                    tehnickaOpremljenostLoseRBtn.PerformClick();

                // Prikaz dodanih nametnika na provedbenom planu
                var nametniciList = db.Query<DID_ProvedbeniPlan_Nametnik>(
                    "SELECT * " +
                    "FROM DID_ProvedbeniPlan_Nametnik " +
                    "WHERE ProvedbeniPlan = ?", provedbeniPlan.FirstOrDefault().Id);

                foreach (var nametnik in nametniciList)
                    localProvedbeniPlanEdit.PutString("nametnik" + nametnik.NametnikSifra, nametnik.NametnikSifra);

                localProvedbeniPlanEdit.PutInt("provedbeniPlanId", provedbeniPlan.FirstOrDefault().Id);
                localProvedbeniPlanEdit.Commit();
                DisableInput();
            }
            else
                IsPageVisited();
        }

        public void IsPageVisited()
        {
            if (localProvedbeniPlan.GetBoolean("visited_page1", false))
            {
                datumInput = Convert.ToDateTime(localProvedbeniPlan.GetString("datumIzvida_page1", null));
                datum.UpdateDate(datumInput.Year, datumInput.Month - 1, datumInput.Day);
                higijenskoStanjeLoseInput.Text = localProvedbeniPlan.GetString("higijenskoStanjeOpis_page1", null);
                gradevinskoStanjeLoseInput.Text = localProvedbeniPlan.GetString("gradevinskoStanjeOpis_page1", null);
                vrijeme.Hour = localProvedbeniPlan.GetInt("vrijemeIzvidaSat_page1", 0);
                vrijeme.Minute = localProvedbeniPlan.GetInt("vrijemeIzvidaMinuta_page1", 0);

                if (localProvedbeniPlan.GetBoolean("higijenskoStanje_page1", false))
                    higijenskoStanjeDobroRBtn.PerformClick();
                else
                    higijenskoStanjeLoseRBtn.PerformClick();

                if (localProvedbeniPlan.GetBoolean("gradevinskoStanje_page1", false))
                    gradevinskoStanjeDobroRBtn.PerformClick();
                else
                    gradevinskoStanjeLoseRBtn.PerformClick();

                if (localProvedbeniPlan.GetBoolean("tehnickaOpremljenost_page1", false))
                    tehnickaOpremljenostDobroRBtn.PerformClick();
                else
                    tehnickaOpremljenostLoseRBtn.PerformClick();
            }
        }

        public void DisableInput()
        {
            // Disablanje inputa -> provedbeni plan nije moguće editirati
            //vrijemeInput.Enabled = false;

            datum.Enabled = false;
            higijenskoStanjeDobroRBtn.Enabled = false;
            higijenskoStanjeLoseRBtn.Enabled = false;
            higijenskoStanjeLoseInput.Enabled = false;
            gradevinskoStanjeDobroRBtn.Enabled = false;
            gradevinskoStanjeLoseRBtn.Enabled = false;
            gradevinskoStanjeLoseInput.Enabled = false;
            tehnickaOpremljenostDobroRBtn.Enabled = false;
            tehnickaOpremljenostLoseRBtn.Enabled = false;

            datum.Visibility = Android.Views.ViewStates.Gone;
            datumLinearLayout.Visibility = Android.Views.ViewStates.Visible;
        }

        public void HigijenskoStanjeDobroRBtn_Click(object sender, EventArgs args)
        {
            higijenskoStanjeLoseInput.Visibility = Android.Views.ViewStates.Gone;
        }

        public void HigijenskoStanjeLoseRBtn_Click(object sender, EventArgs args)
        {
            higijenskoStanjeLoseInput.Visibility = Android.Views.ViewStates.Visible;
        }

        public void GradevinskoStanjeLoseRBtn_Click(object sender, EventArgs args)
        {
            gradevinskoStanjeLoseInput.Visibility = Android.Views.ViewStates.Visible;
        }

        public void GradevinskoStanjeDobroRBtn_Click(object sender, EventArgs args)
        {
            gradevinskoStanjeLoseInput.Visibility = Android.Views.ViewStates.Gone;
        }

        public void SaveState()
        {
            localProvedbeniPlanEdit.PutString("datumIzvida_page1", datum.DateTime.ToString());
            localProvedbeniPlanEdit.PutInt("vrijemeIzvidaSat_page1", vrijemeSat);
            localProvedbeniPlanEdit.PutInt("vrijemeIzvidaMinuta_page1", vrijemeMinuta);
            localProvedbeniPlanEdit.PutBoolean("higijenskoStanje_page1", higijenskoStanjeDobroRBtn.Checked);
            localProvedbeniPlanEdit.PutString("higijenskoStanjeOpis_page1", higijenskoStanjeLoseInput.Text);
            localProvedbeniPlanEdit.PutBoolean("gradevinskoStanje_page1", gradevinskoStanjeDobroRBtn.Checked);
            localProvedbeniPlanEdit.PutString("gradevinskoStanjeOpis_page1", gradevinskoStanjeLoseInput.Text);
            localProvedbeniPlanEdit.PutBoolean("tehnickaOpremljenost_page1", tehnickaOpremljenostDobroRBtn.Checked);
            localProvedbeniPlanEdit.PutBoolean("visited_page1", true);
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
                intent = new Intent(this, typeof(Activity_Lokacije));
            else if (item.TitleFormatted.ToString() == "naprijed")
            {
                intent = new Intent(this, typeof(Activity_ProvedbeniPlan_page2));
                SaveState();
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