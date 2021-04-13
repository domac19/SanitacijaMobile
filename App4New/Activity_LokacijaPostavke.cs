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
    [Activity(Label = "Activity_LokacijaPostavke", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_LokacijaPostavke : Activity
    {
        CheckBox neprovedenoRadioBtn, anketePoPozicijama;
        Spinner spinnerNeprovedeno;
        EditText napomeneInput, opisInput;
        TextView neprovedenoTextView;
        Intent intent;
        Button saveBtn;

        int radniNalogId = localRadniNalozi.GetInt("id", 0);
        int lokacijaId = localKomitentLokacija.GetInt("lokacijaId", -1);
        
        static readonly ISharedPreferences localNeizvrsernaLokacija = Application.Context.GetSharedPreferences("lokacija", FileCreationMode.Private);
        static ISharedPreferencesEditor localNeizvrsernaLokacijaEdit = localNeizvrsernaLokacija.Edit();
        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);
        static readonly ISharedPreferences localKomitentLokacija = Application.Context.GetSharedPreferences("localKomitentLokacija", FileCreationMode.Private);

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lokacijaNeizvrsenje);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            neprovedenoRadioBtn = FindViewById<CheckBox>(Resource.Id.neprovedenoRadioBtn);
            neprovedenoTextView = FindViewById<TextView>(Resource.Id.neprovedenoTextView);
            napomeneInput = FindViewById<EditText>(Resource.Id.napomeneInput);
            opisInput = FindViewById<EditText>(Resource.Id.opisInput);
            spinnerNeprovedeno = FindViewById<Spinner>(Resource.Id.spinnerNeprovedeno);
            saveBtn = FindViewById<Button>(Resource.Id.saveBtn);
            anketePoPozicijama = FindViewById<CheckBox>(Resource.Id.anketePoPozicijama);

            SetActionBar(toolbar);
            ActionBar.Title = "Lokacija";
            spinnerNeprovedeno.Enabled = false;
            saveBtn.Click += SaveBtn_Click;
            DID_RadniNalog_Lokacija radniNalogLok = db.Query<DID_RadniNalog_Lokacija>(
               "SELECT * " +
               "FROM DID_RadniNalog_Lokacija " +
               "WHERE RadniNalog = ? " +
               "AND Lokacija = ?", radniNalogId, lokacijaId).FirstOrDefault();

            bool anketePozicije = db.Query<DID_Lokacija>(
                "SELECT * " +
                "FROM DID_Lokacija " +
                "WHERE SAN_Id = ?", lokacijaId).FirstOrDefault().SAN_AnketePoPozicijama;
            anketePoPozicijama.Checked = anketePozicije;
            napomeneInput.Text = radniNalogLok.Napomena;
            opisInput.Text = radniNalogLok.OpisPosla;
            neprovedenoRadioBtn.Click += NeprovedenoRadioBtn_Click;

            //dropdown menu za razlog neizvrsene deratizacije
            List<DID_RazlogNeizvrsenjaDeratizacije> razlozi = db.Query<DID_RazlogNeizvrsenjaDeratizacije>(
                "SELECT * " +
                "FROM DID_RazlogNeizvrsenjaDeratizacije");

            List<string> razloziList = new List<string>();
            for (var i = 1; i < razlozi.Count; i++)
                razloziList.Add(razlozi[i].Naziv);

            ArrayAdapter<string> adapterPartnerList = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, razloziList);
            adapterPartnerList.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinnerNeprovedeno.Adapter = adapterPartnerList;

            if (radniNalogLok.Status == 4)
            {
                neprovedenoRadioBtn.PerformClick();
                spinnerNeprovedeno.SetSelection(radniNalogLok.RazlogNeizvrsenja - 1);
            }
        }

        public void NeprovedenoRadioBtn_Click(object sender, EventArgs args)
        {
            if (neprovedenoRadioBtn.Checked)
            {
                spinnerNeprovedeno.Enabled = true;
                neprovedenoTextView.SetTextColor(Android.Graphics.Color.ParseColor("#ff0288d1"));
            }
            else
            {
                spinnerNeprovedeno.Enabled = false;
                neprovedenoTextView.SetTextColor(Android.Graphics.Color.ParseColor("#ff8c99a0"));
            }
        }

        public void SaveState()
        {
            localNeizvrsernaLokacijaEdit.PutBoolean("visitedNeizvrsenaLokacija", true);
            localNeizvrsernaLokacijaEdit.PutString("napomeneInput", napomeneInput.Text);
            localNeizvrsernaLokacijaEdit.PutString("spinnerSelectedItem", spinnerNeprovedeno.SelectedItem.ToString());
            localNeizvrsernaLokacijaEdit.PutBoolean("neprovedenoRadioBtn", neprovedenoRadioBtn.Checked);
            localNeizvrsernaLokacijaEdit.Commit();
        }

        public void SaveBtn_Click(object sender, EventArgs args)
        {
            int razlogNeizvrsenja = 0;
            if (neprovedenoRadioBtn.Checked)
            {
                string spinnerSelectedItem = spinnerNeprovedeno.SelectedItem.ToString();
                razlogNeizvrsenja = db.Query<DID_RazlogNeizvrsenjaDeratizacije>(
                    "SELECT * " +
                    "FROM DID_RazlogNeizvrsenjaDeratizacije " +
                    "WHERE Naziv = ?", spinnerSelectedItem).FirstOrDefault().Sifra;

                db.Query<DID_RadniNalog_Lokacija>(
                    "UPDATE DID_RadniNalog_Lokacija " +
                    "SET VrijemeDolaska = ?," +
                        "OpisPosla = ?, " +
                        "Napomena = ?, " +
                        "RazlogNeizvrsenja = ?, " +
                        "Status = ? " +
                    "WHERE Lokacija = ? " +
                    "AND RadniNalog = ?",
                        DateTime.Now,
                        opisInput.Text,
                        napomeneInput.Text,
                        razlogNeizvrsenja,
                        4,
                        lokacijaId,
                        radniNalogId);
            }
            else
            {
                db.Query<DID_RadniNalog_Lokacija>(
                "UPDATE DID_RadniNalog_Lokacija " +
                "SET VrijemeDolaska = ?," +
                    "OpisPosla = ?, " +
                    "Napomena = ?, " +
                    "RazlogNeizvrsenja = ?, " +
                    "Status = ? " +
                "WHERE Lokacija = ? " +
                "AND RadniNalog = ?",
                    DateTime.Now,
                    opisInput.Text,
                    napomeneInput.Text,
                    razlogNeizvrsenja,
                    1,
                    lokacijaId,
                    radniNalogId);
            }


            db.Query<DID_Lokacija>(
                "UPDATE DID_Lokacija " +
                "SET SAN_AnketePoPozicijama = ? " +
                "WHERE SAN_Id = ? ",
                    anketePoPozicijama.Checked,
                    lokacijaId);

            List<DID_RadniNalog_Lokacija> sveLokacije = db.Query<DID_RadniNalog_Lokacija>(
                "SELECT * " +
                "FROM DID_RadniNalog_Lokacija " +
                "WHERE RadniNalog = ?", radniNalogId);

            List<DID_RadniNalog_Lokacija> izvrseneLokacije = db.Query<DID_RadniNalog_Lokacija>(
                "SELECT * " +
                "FROM DID_RadniNalog_Lokacija " +
                "WHERE RadniNalog = ? " +
                "AND Status = 3", radniNalogId);

            if (sveLokacije.Count == izvrseneLokacije.Count)
            {
                db.Query<DID_RadniNalog>(
                    "UPDATE DID_RadniNalog " +
                    "SET Status = ?, DatumIzvrsenja = ? " +
                    "WHERE Id = ?", 5, DateTime.Now, radniNalogId);
            }
            else if (izvrseneLokacije.Any())
            {
                db.Query<DID_RadniNalog>(
                    "UPDATE DID_RadniNalog " +
                    "SET Status = ? " +
                    "WHERE Id = ?", 4, radniNalogId);
            }
            else
            {
                db.Query<DID_RadniNalog>(
                    "UPDATE DID_RadniNalog " +
                    "SET Status = ? " +
                    "WHERE Id = ?", 3, radniNalogId);
            }

            intent = new Intent(this, typeof(Activity_Lokacije));
            StartActivity(intent);
        }

        public override void OnBackPressed()
        {
            intent = new Intent(this, typeof(Activity_Lokacije));
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
                intent = new Intent(this, typeof(Activity_Lokacije));

            StartActivity(intent);
            return base.OnOptionsItemSelected(item);
        }
    }
}