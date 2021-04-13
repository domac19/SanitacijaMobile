using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Infobit.DDD.Data;
using SQLite;

namespace App4New
{
    [Activity(Label = "Activity_Kontola_page3_Edit", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_Kontola_page3_Edit : Activity
    {
        Button saveBtn;
        CheckBox neprovedenoRadioBtn;
        Spinner spinnerNeprovedeno;
        EditText napomeneInput;
        TextView neprovedenoTextView;
        bool novaKutijaUcinjenoZiv, uginuliStakorZiv, uginuliMisZiv, nemaKutijeZiv, kutijaOstecenaZiv, kutijaUrednaZiv, novaKutijaUcinjenoLjep, novaLjepPodUcinjenoLjep,
            ulovljenMisLjep, ulovljenStakorLjep, nemaKutijeLjep, kutijaOstecenaLjep, kutijaUrednaLjep, uginuliMisRot, noviMamciUcinjenoRot, novaKutijaUcinjenoRot,
            nadopunjenaMekomUcinjenoRot, kutijaUrednaRot, kutijaOstecenaRot, nemaKutijeRot, tragoviGlodanjaRot, izmetRot, pojedenaMekaRot, ostecenaMekaRot, uginuliStakorRot;
        Intent intent;

        static readonly ISharedPreferences localPozicija = Application.Context.GetSharedPreferences("pozicija", FileCreationMode.Private);
        static readonly ISharedPreferences localUsername = Application.Context.GetSharedPreferences("userLogin", FileCreationMode.Private);
        static readonly ISharedPreferences localAnketa = Application.Context.GetSharedPreferences("anketa", FileCreationMode.Private);
        static ISharedPreferencesEditor localAnketaEdit = localAnketa.Edit();

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.kontola_page3);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            neprovedenoRadioBtn = FindViewById<CheckBox>(Resource.Id.neprovedenoRadioBtn);
            neprovedenoTextView = FindViewById<TextView>(Resource.Id.neprovedenoTextView);
            napomeneInput = FindViewById<EditText>(Resource.Id.napomeneInput);
            spinnerNeprovedeno = FindViewById<Spinner>(Resource.Id.spinnerNeprovedeno);
            saveBtn = FindViewById<Button>(Resource.Id.saveBtn);

            SetActionBar(toolbar);
            ActionBar.Title = "Anketa";
            saveBtn.Click += SaveBtn_Click;
            spinnerNeprovedeno.Enabled = false;
            neprovedenoRadioBtn.Click += NeprovedenoRadioBtn_Click;

            //dropdown menu za razlog neizvrsene deratizacije
            List<DID_RazlogNeizvrsenjaDeratizacije> razlozi = db.Query<DID_RazlogNeizvrsenjaDeratizacije>
                  ("SELECT * " +
                  "FROM DID_RazlogNeizvrsenjaDeratizacije");

            List<string> razloziList = new List<string>();
            for (var i = 1; i < razlozi.Count; i++)
                razloziList.Add(razlozi[i].Naziv);

            ArrayAdapter<string> adapterPartnerList = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, razloziList);
            adapterPartnerList.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinnerNeprovedeno.Adapter = adapterPartnerList;

            if (localAnketa.GetBoolean("visitedPage3", false))
            {
                napomeneInput.Text = localAnketa.GetString("napomeneInput", null);
                neprovedenoRadioBtn.Checked = localAnketa.GetBoolean("neprovedenoRadioBtn", false);
                if (neprovedenoRadioBtn.Checked)
                {
                    spinnerNeprovedeno.Enabled = true;
                    spinnerNeprovedeno.SetSelection(razloziList.IndexOf(localAnketa.GetString("spinnerSelectedItem", null)));
                }
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
                neprovedenoTextView.SetTextColor(Android.Graphics.Color.ParseColor("#ff8c99a0"));
                spinnerNeprovedeno.Enabled = false;
            }
        }

        public void SaveState()
        {
            localAnketaEdit.PutString("napomeneInput", napomeneInput.Text);
            localAnketaEdit.PutString("spinnerSelectedItem", spinnerNeprovedeno.SelectedItem.ToString());
            localAnketaEdit.PutBoolean("neprovedenoRadioBtn", neprovedenoRadioBtn.Checked);
            localAnketaEdit.PutBoolean("visitedPage3", true);
            localAnketaEdit.Commit();
        }

        public void SaveBtn_Click(object sender, EventArgs args)
        {
            string anketaId = localPozicija.GetString("anketaId", null);
            int pozicijaId = localPozicija.GetInt("pozicijaId", -1);
            string koriniscnoIme = localUsername.GetString("username", null);
            int tipKutije = localPozicija.GetInt("tipKutije", -1);

            if (tipKutije == 1)
            {
                kutijaUrednaRot = localAnketa.GetBoolean("kutijaUrednaRot", false);
                kutijaOstecenaRot = localAnketa.GetBoolean("kutijaOstecenaRot", false);
                nemaKutijeRot = localAnketa.GetBoolean("nemaKutijeRot", false);
                tragoviGlodanjaRot = localAnketa.GetBoolean("tragoviGlodanjaRot", false);
                izmetRot = localAnketa.GetBoolean("izmetRot", false);
                pojedenaMekaRot = localAnketa.GetBoolean("pojedenaMekaRot", false);
                ostecenaMekaRot = localAnketa.GetBoolean("ostecenaMekaRot", false);
                uginuliStakorRot = localAnketa.GetBoolean("uginuliStakorRot", false);
                uginuliMisRot = localAnketa.GetBoolean("uginuliMisRot", false);
                noviMamciUcinjenoRot = localAnketa.GetBoolean("noviMamciUcinjenoRot", false);
                novaKutijaUcinjenoRot = localAnketa.GetBoolean("novaKutijaUcinjenoRot", false);
                nadopunjenaMekomUcinjenoRot = localAnketa.GetBoolean("nadopunjenaMekomUcinjenoRot", false);
            }
            else if (tipKutije == 2)
            {
                kutijaUrednaLjep = localAnketa.GetBoolean("kutijaUrednaLjep", false);
                kutijaOstecenaLjep = localAnketa.GetBoolean("kutijaOstecenaLjep", false);
                nemaKutijeLjep = localAnketa.GetBoolean("nemaKutijeLjep", false);
                ulovljenStakorLjep = localAnketa.GetBoolean("ulovljenStakorLjep", false);
                ulovljenMisLjep = localAnketa.GetBoolean("ulovljenMisLjep", false);
                novaLjepPodUcinjenoLjep = localAnketa.GetBoolean("novaLjepPodUcinjenoLjep", false);
                novaKutijaUcinjenoLjep = localAnketa.GetBoolean("novaKutijaUcinjenoLjep", false);
            }
            else
            {
                kutijaUrednaZiv = localAnketa.GetBoolean("kutijaUrednaZiv", false);
                kutijaOstecenaZiv = localAnketa.GetBoolean("kutijaOstecenaZiv", false);
                nemaKutijeZiv = localAnketa.GetBoolean("nemaKutijeZiv", false);
                uginuliMisZiv = localAnketa.GetBoolean("uginuliMisZiv", false);
                uginuliStakorZiv = localAnketa.GetBoolean("uginuliStakorZiv", false);
                novaKutijaUcinjenoZiv = localAnketa.GetBoolean("novaKutijaUcinjenoZiv", false);
            }

            int razlogNeizvrsenja = 0;
            if (neprovedenoRadioBtn.Checked)
            {
                string spinnerSelectedItem = spinnerNeprovedeno.SelectedItem.ToString();
                razlogNeizvrsenja = db.Query<DID_RazlogNeizvrsenjaDeratizacije>
                      ("SELECT * " +
                      "FROM DID_RazlogNeizvrsenjaDeratizacije " +
                      "WHERE Naziv = ?", spinnerSelectedItem).FirstOrDefault().Sifra;
            }

            localPozicija.Edit().Clear().Commit();
            localAnketa.Edit().Clear().Commit();

            db.Query<DID_Anketa>(
                "UPDATE DID_Anketa " +
                "SET ANK_KorisnickoIme = ?, " +
                    "ANK_POZ_Id = ?, " +
                    "LastEditDate = ?, " +
                    "ANK_Napomena = ?, " +
                    "ANK_RazlogNeizvrsenja = ?, " +
                    "ANK_Utvrdjeno_Rot_KutijaUredna = ?, " +
                    "ANK_Utvrdjeno_Rot_KutijaOstecena = ?, " +
                    "ANK_Utvrdjeno_Rot_NemaKutije = ?, " +
                    "ANK_Utvrdjeno_Rot_TragoviGlodanja = ?, " +
                    "ANK_Utvrdjeno_Rot_Izmet = ?, " +
                    "ANK_Utvrdjeno_Rot_PojedenaMeka = ?, " +
                    "ANK_Utvrdjeno_Rot_OstecenaMeka = ?, " +
                    "ANK_Utvrdjeno_Rot_UginuliStakor = ?, " +
                    "ANK_Utvrdjeno_Rot_UginuliMis = ?, " +
                    "ANK_Utvrdjeno_Ljep_KutijaUredna = ?, " +
                    "ANK_Utvrdjeno_Ljep_KutijaOstecena = ?, " +
                    "ANK_Utvrdjeno_Ljep_NemaKutije = ?, " +
                    "ANK_Utvrdjeno_Ljep_UlovljenStakor = ?, " +
                    "ANK_Utvrdjeno_Ljep_UlovljenMis = ?, " +
                    "ANK_Utvrdjeno_Ziv_KutijaUredna = ?, " +
                    "ANK_Utvrdjeno_Ziv_KutijaOstecena = ?, " +
                    "ANK_Utvrdjeno_Ziv_NemaKutije = ?, " +
                    "ANK_Utvrdjeno_Ziv_UlovljenStakor = ?, " +
                    "ANK_Utvrdjeno_Ziv_UlovljenMis = ?, " +
                    "ANK_Ucinjeno_Rot_PostavljeniMamci = ?, " +
                    "ANK_Ucinjeno_Rot_NovaKutija = ?, " +
                    "ANK_Ucinjeno_Rot_NadopunjenaMekom = ?, " +
                    "ANK_Ucinjeno_Ljep_NovaLjepljivaPodloska = ?, " +
                    "ANK_Ucinjeno_Ljep_NovaKutija = ?, " +
                    "ANK_Ucinjeno_Ziv_NovaKutija = ?, " +
                    "ANK_TipDeratizacijskeKutijeSifra = ? " +
                "WHERE Id = ?",
                    koriniscnoIme,
                    pozicijaId,
                    DateTime.Now,
                    napomeneInput.Text,
                    razlogNeizvrsenja,
                    kutijaUrednaRot,
                    kutijaOstecenaRot,
                    nemaKutijeRot,
                    tragoviGlodanjaRot,
                    izmetRot,
                    pojedenaMekaRot,
                    ostecenaMekaRot,
                    uginuliStakorRot,
                    uginuliMisRot,
                    kutijaUrednaLjep,
                    kutijaOstecenaLjep,
                    nemaKutijeLjep,
                    ulovljenStakorLjep,
                    ulovljenMisLjep,
                    kutijaUrednaZiv,
                    kutijaOstecenaZiv,
                    nemaKutijeZiv,
                    uginuliStakorZiv,
                    uginuliMisZiv,
                    noviMamciUcinjenoRot,
                    novaKutijaUcinjenoRot,
                    nadopunjenaMekomUcinjenoRot,
                    novaLjepPodUcinjenoLjep,
                    novaKutijaUcinjenoLjep,
                    novaKutijaUcinjenoZiv,
                    tipKutije,
                    anketaId
                );

            Toast.MakeText(this, "Nova deratizacija je spremljena", ToastLength.Short).Show();
            intent = new Intent(this, typeof(Activity_OdradeneAnkete));
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
            {
                SaveState();
                intent = new Intent(this, typeof(Activity_Kontola_page2));
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
