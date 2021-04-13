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
    [Activity(Label = "Activity_PrvaDer_page4_Edit", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_PrvaDer_page4_Edit : Activity
    {
        Button saveBtn;
        CheckBox neprovedenoRadioBtn;
        Spinner spinnerNeprovedeno;
        EditText napomeneInput;
        Intent intent;
        TextView neprovedenoTextView, msgNapomena;
        int tipDeratizacijskeKutije;

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
            SetContentView(Resource.Layout.prvaDer_page4);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            neprovedenoRadioBtn = FindViewById<CheckBox>(Resource.Id.neprovedenoRadioBtn);
            neprovedenoTextView = FindViewById<TextView>(Resource.Id.neprovedenoTextView);
            napomeneInput = FindViewById<EditText>(Resource.Id.napomeneInput);
            spinnerNeprovedeno = FindViewById<Spinner>(Resource.Id.spinnerNeprovedeno);
            saveBtn = FindViewById<Button>(Resource.Id.saveBtn);
            msgNapomena = FindViewById<TextView>(Resource.Id.msgNapomena);

            SetActionBar(toolbar);
            ActionBar.Title = "Anketa";
            saveBtn.Click += SaveBtn_Click;
            spinnerNeprovedeno.Enabled = false;
            napomeneInput.TextChanged += NapomeneInput_TextChanged;
            neprovedenoRadioBtn.Click += NeprovedenoRadioBtn_Click;

            //dropdown menu za razlog neizvrsene deratizacije
            List<DID_RazlogNeizvrsenjaDeratizacije> razlozi = db.Query<DID_RazlogNeizvrsenjaDeratizacije>
                  ("SELECT * " +
                  "FROM DID_RazlogNeizvrsenjaDeratizacije");

            List<string> razloziList = new List<string>();
            foreach (var item in razlozi)
                razloziList.Add(item.Naziv);
            ArrayAdapter<string> adapterPartnerList = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, razloziList);
            adapterPartnerList.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinnerNeprovedeno.Adapter = adapterPartnerList;

            if (localAnketa.GetBoolean("visitedPage4", false))
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

        public void NeprovedenoRadioBtn_Click(object sender, EventArgs e)
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

        public void NapomeneInput_TextChanged(object sender, EventArgs e)
        {
            if (napomeneInput.Text.Length > 512)
                msgNapomena.Visibility = Android.Views.ViewStates.Visible;
            else
                msgNapomena.Visibility = Android.Views.ViewStates.Invisible;
        }

        public void SaveState()
        {
            localAnketaEdit.PutString("napomeneInput", napomeneInput.Text);
            localAnketaEdit.PutString("spinnerSelectedItem", spinnerNeprovedeno.SelectedItem.ToString());
            localAnketaEdit.PutBoolean("neprovedenoRadioBtn", neprovedenoRadioBtn.Checked);
            localAnketaEdit.PutBoolean("visitedPage4", true);
            localAnketaEdit.Commit();
        }

        public void SaveBtn_Click(object sender, EventArgs args)
        {
            if (napomeneInput.Text.Length > 512)
                return;

            string anketaId = localPozicija.GetString("anketaId", null);
            int pozicijaId = localPozicija.GetInt("pozicijaId", -1);
            string koriniscnoIme = localUsername.GetString("username", null);

            bool rupeBox = localAnketa.GetBoolean("rupeBox", false);
            bool legloBox = localAnketa.GetBoolean("legloBox", false);
            bool tragoviNoguBox = localAnketa.GetBoolean("tragoviNoguBox", false);
            bool videnZiviGlodavacBox = localAnketa.GetBoolean("videnZiviGlodavacBox", false);
            bool izmetBox = localAnketa.GetBoolean("izmetBox", false);
            bool videnUginuliGlodavacBox = localAnketa.GetBoolean("videnUginuliGlodavacBox", false);
            bool stetaBox = localAnketa.GetBoolean("stetaBox", false);

            bool ANK_Kutija_Ljep = localAnketa.GetBoolean("ANK_Kutija_Ljep", false);
            bool ANK_Kutija_Rot = localAnketa.GetBoolean("ANK_Kutija_Rot", false);
            bool ANK_Kutija_Ziv = localAnketa.GetBoolean("ANK_Kutija_Ziv", false);
            bool stakoriBox = localAnketa.GetBoolean("stakoriBox", false);
            bool miseviBox = localAnketa.GetBoolean("miseviBox", false);
            bool drugiGlodavciBox = localAnketa.GetBoolean("drugiGlodavciBox", false);
            bool okolisBox = localAnketa.GetBoolean("okolisBox", false);
            bool hranaBox = localAnketa.GetBoolean("hranaBox", false);
            bool odvodnjaBox = localAnketa.GetBoolean("odvodnjaBox", false);

            bool novaLjepPodUcinjenoLjep = localAnketa.GetBoolean("novaLjepPodUcinjenoLjep", false);
            bool novaKutijaUcinjenoLjep = localAnketa.GetBoolean("novaKutijaUcinjenoLjep", false);
            bool noviMamciUcinjenoRot = localAnketa.GetBoolean("noviMamciUcinjenoRot", false);
            bool novaKutijaUcinjenoRot = localAnketa.GetBoolean("novaKutijaUcinjenoRot", false);
            bool nadopunjenaMekomUcinjenoRot = localAnketa.GetBoolean("nadopunjenaMekomUcinjenoRot", false);
            bool novaKutijaUcinjenoZiv = localAnketa.GetBoolean("novaKutijaUcinjenoZiv", false);

            if (ANK_Kutija_Ljep)
                tipDeratizacijskeKutije = 2;
            else if (ANK_Kutija_Rot)
                tipDeratizacijskeKutije = 1;
            else if (ANK_Kutija_Ziv)
                tipDeratizacijskeKutije = 3;

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
            localAnketa.Edit().Clear().Commit();

            db.Query<DID_LokacijaPozicija>(
                "UPDATE DID_LokacijaPozicija " +
                "SET POZ_Tip = ? " +
                "WHERE POZ_Id = ?", pozicijaId, tipDeratizacijskeKutije);

            db.Query<DID_Anketa>(
                "UPDATE DID_Anketa " +
                "SET ANK_KorisnickoIme = ?, " +
                    "ANK_POZ_Id = ?, " +
                    "LastEditDate = ?, " +
                    "ANK_Napomena = ?, " +
                    "ANK_RazlogNeizvrsenja = ?, " +
                    "ANK_Poc_Prisutnost_Rupe= ?, " +
                    "ANK_Poc_Prisutnost_TragoviNogu = ?, " +
                    "ANK_Poc_Prisutnost_Izmet = ?, " +
                    "ANK_Poc_Prisutnost_Steta = ?, " +
                    "ANK_Poc_Prisutnost_Leglo = ?, " +
                    "ANK_Poc_Prisutnost_GlodavacZivi = ?, " +
                    "ANK_Poc_Prisutnost_GlodavacUginuli = ?, " +
                    "ANK_Poc_Infestacija_Misevi = ?, " +
                    "ANK_Poc_Infestacija_Stakori = ?, " +
                    "ANK_Poc_Infestacija_DrugiGlodavci = ?, " +
                    "ANK_Poc_Uvjeti_Hrana = ?, " +
                    "ANK_Poc_Uvjeti_NeispravnaOdvodnja = ?, " +
                    "ANK_Poc_Uvjeti_NeuredanOkolis = ?, " +
                    "ANK_Ucinjeno_Rot_PostavljeniMamci = ?, " +
                    "ANK_Ucinjeno_Rot_NovaKutija = ?, " +
                    "ANK_Ucinjeno_Rot_NadopunjenaMekom = ?, " +
                    "ANK_Ucinjeno_Ljep_NovaLjepljivaPodloska = ?, " +
                    "ANK_Ucinjeno_Ljep_NovaKutija = ?, " +
                    "ANK_Ucinjeno_Ziv_NovaKutija = ?, " +
                    "ANK_TipDeratizacijskeKutijeSifra= ? " +
                "WHERE Id = ?",
                    koriniscnoIme,
                    pozicijaId,
                    DateTime.Now,
                    napomeneInput.Text,
                    razlogNeizvrsenja,
                    rupeBox,
                    tragoviNoguBox,
                    izmetBox,
                    stetaBox,
                    legloBox,
                    videnZiviGlodavacBox,
                    videnUginuliGlodavacBox,
                    miseviBox,
                    stakoriBox,
                    drugiGlodavciBox,
                    hranaBox,
                    odvodnjaBox,
                    okolisBox,
                    noviMamciUcinjenoRot,
                    novaKutijaUcinjenoRot,
                    nadopunjenaMekomUcinjenoRot,
                    novaLjepPodUcinjenoLjep,
                    novaKutijaUcinjenoLjep,
                    novaKutijaUcinjenoZiv,
                    tipDeratizacijskeKutije,
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
                intent = new Intent(this, typeof(Activity_PrvaDer_page3));
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

