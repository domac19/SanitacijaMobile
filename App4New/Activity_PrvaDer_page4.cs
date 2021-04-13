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
    [Activity(Label = "Activity_PrvaDer_page4", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_PrvaDer_page4 : Activity
    {
        Button saveBtn;
        CheckBox neprovedenoRadioBtn;
        Spinner spinnerNeprovedeno;
        EditText napomeneInput;
        TextView neprovedenoTextView, msgNapomena;
        Intent intent;
        int tipDeratizacijskeKutije,
            radniNalogId = localRadniNalozi.GetInt("id", 0),
            lokacijaId = localKomitentLokacija.GetInt("lokacijaId", 0),
            radniNalogLokacijaId = localKomitentLokacija.GetInt("radniNalogLokacijaId", 0);
        string koriniscnoIme = localUsername.GetString("username", null),
            barcode = localPozicija.GetString("barcode", null),
            opisPozicije = localPozicija.GetString("opisPozicije", null),
            positionBroj = localPozicija.GetString("positionBroj", null),
            positionBrojOznaka = localPozicija.GetString("positionBrojOznaka", null);

        static readonly ISharedPreferences localPotvrda = Application.Context.GetSharedPreferences("potvrda", FileCreationMode.Private);
        static ISharedPreferencesEditor localPotvrdaEdit = localPotvrda.Edit();
        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);
        static readonly ISharedPreferences localPozicija = Application.Context.GetSharedPreferences("pozicija", FileCreationMode.Private);
        static ISharedPreferencesEditor localPozicijaEdit = localPozicija.Edit();
        static readonly ISharedPreferences localUsername = Application.Context.GetSharedPreferences("userLogin", FileCreationMode.Private);
        static readonly ISharedPreferences localKomitentLokacija = Application.Context.GetSharedPreferences("localKomitentLokacija", FileCreationMode.Private);
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

        public void SpremiAnketu()
        {
            int tipDeratizacije = 2;
            int pozicijaId = -1;
            bool novaPozicija = localPozicija.GetBoolean("novaPozicija", false);

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

            List<DID_LokacijaPozicija> pozicije = db.Query<DID_LokacijaPozicija>(
                "SELECT * " +
                "FROM DID_LokacijaPozicija " +
                "WHERE POZ_Id < 0 " +
                "ORDER BY POZ_Id");

            if (pozicije.Any())
                pozicijaId = pozicije.FirstOrDefault().POZ_Id - 1;

            //spremanje nove pozicije u bazu
            db.Query<DID_LokacijaPozicija>(
                "INSERT INTO DID_LokacijaPozicija (" +
                    "POZ_Id, " +
                    "SAN_Id, " +
                    "POZ_Broj, " +
                    "POZ_BrojOznaka, " +
                    "POZ_Barcode, " +
                    "POZ_Opis, " +
                    "POZ_Tip, " +
                    "SinhronizacijaPrivremeniKljuc)" +
                "VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                    pozicijaId,
                    lokacijaId,
                    positionBroj,
                    positionBrojOznaka,
                    barcode,
                    opisPozicije,
                    tipDeratizacijskeKutije,
                    pozicijaId
                );

            Guid id = Guid.NewGuid();
            //dodavanje ankete ukoliko se radi o novoj deratizacijji
            db.Query<DID_Anketa>(
                "INSERT INTO DID_Anketa (" +
                    "Id, " +
                    "ANK_RadniNalog, " +
                    "ANK_KorisnickoIme, " +
                    "ANK_POZ_Id, " +
                    "ANK_DatumVrijeme, " +
                    "LastEditDate, " +
                    "ANK_TipDeratizacije, " +
                    "ANK_Napomena, " +
                    "ANK_RazlogNeizvrsenja, " +
                    "ANK_Poc_Prisutnost_Rupe, " +
                    "ANK_Poc_Prisutnost_TragoviNogu, " +
                    "ANK_Poc_Prisutnost_Izmet, " +
                    "ANK_Poc_Prisutnost_Steta, " +
                    "ANK_Poc_Prisutnost_Leglo, " +
                    "ANK_Poc_Prisutnost_GlodavacZivi, " +
                    "ANK_Poc_Prisutnost_GlodavacUginuli, " +
                    "ANK_Poc_Infestacija_Misevi, " +
                    "ANK_Poc_Infestacija_Stakori, " +
                    "ANK_Poc_Infestacija_DrugiGlodavci, " +
                    "ANK_Poc_Uvjeti_Hrana, " +
                    "ANK_Poc_Uvjeti_NeispravnaOdvodnja, " +
                    "ANK_Poc_Uvjeti_NeuredanOkolis, " +
                    "ANK_Ucinjeno_Rot_PostavljeniMamci, " +
                    "ANK_Ucinjeno_Rot_NovaKutija, " +
                    "ANK_Ucinjeno_Rot_NadopunjenaMekom, " +
                    "ANK_Ucinjeno_Ljep_NovaLjepljivaPodloska, " +
                    "ANK_Ucinjeno_Ljep_NovaKutija, " +
                    "ANK_Ucinjeno_Ziv_NovaKutija, " +
                    "ANK_TipDeratizacijskeKutijeSifra, " +
                    "SinhronizacijaPrivremeniKljuc" +
                    ")" +
                "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                    id,
                    radniNalogId,
                    koriniscnoIme,
                    pozicijaId,
                    DateTime.Now,
                    DateTime.Now,
                    tipDeratizacije,
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
                    id
                );

            db.Execute(
                "UPDATE DID_RadniNalog_Lokacija " +
                "SET Status = 2, " +
                "SinhronizacijaStatus = 0 " +
                "WHERE Id = ?", radniNalogLokacijaId);

            // ciscenje globalnih varijabli
            localPozicija.Edit().Clear().Commit();
            localAnketa.Edit().Clear().Commit();
            localPozicijaEdit.PutBoolean("novaPozicijaPrivremeno", novaPozicija);
            localPozicijaEdit.PutInt("pozicijaId", pozicijaId);
            localPozicijaEdit.PutInt("lokacijaId", lokacijaId);
            localPozicijaEdit.PutString("positionBroj", positionBroj);
            localPozicijaEdit.PutString("positionBrojOznaka", positionBrojOznaka);
            localPozicijaEdit.Commit();
            localPotvrdaEdit.PutBoolean("fromList", false);
            localPotvrdaEdit.Commit();
            Toast.MakeText(this, "Nova deratizacija je spremljena", ToastLength.Short).Show();
            intent = new Intent(this, typeof(Activity_PotroseniMaterijali_List));
            StartActivity(intent);
        }

        public void SaveBtn_Click(object sender, EventArgs args)
        {
            if (napomeneInput.Text.Length > 512)
                return;

            string lokacijaNaziv = db.Query<DID_Lokacija>(
                "SELECT * " +
                "FROM DID_Lokacija " +
                "WHERE SAN_Id = ?", lokacijaId).FirstOrDefault().SAN_Naziv;

            DID_RadniNalog_Lokacija radniNalogLokacija = db.Query<DID_RadniNalog_Lokacija>(
                "SELECT * " +
                "FROM DID_RadniNalog_Lokacija " +
                "WHERE Id = ?", radniNalogLokacijaId).FirstOrDefault();

            if (radniNalogLokacija.Status == 3)
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Upozorenje!");
                alert.SetMessage("Dodavanjem ankete lokacija " + lokacijaNaziv + " gubi status izvršene lokacije. Jeste li sigurni da želite nastaviti?");
                alert.SetPositiveButton("NASTAVI", (senderAlert, arg) =>
                {
                    SpremiAnketu();

                    db.Execute(
                        "UPDATE DID_RadniNalog_Lokacija " +
                        "SET Status = 2, " +
                        "SinhronizacijaStatus = 0 " +
                        "WHERE Id = ?", radniNalogLokacijaId);

                    db.Execute(
                        "UPDATE DID_Lokacija " +
                        "SET SinhronizacijaStatus = 0 " +
                        "WHERE SAN_Id = ?", lokacijaId);

                    DID_RadniNalog radniNalog = db.Query<DID_RadniNalog>(
                       "SELECT * " +
                       "FROM DID_RadniNalog " +
                       "WHERE Id = ?", radniNalogId).FirstOrDefault();

                    if (radniNalog.SinhronizacijaStatus == 2)
                        db.Query<DID_RadniNalog>(
                            "UPDATE DID_RadniNalog " +
                            "SET SinhronizacijaStatus = 1 " +
                            "WHERE Id = ?", radniNalogId);

                    List<DID_RadniNalog_Lokacija> zavrseneLokacije = db.Query<DID_RadniNalog_Lokacija>(
                        "SELECT * " +
                        "FROM DID_RadniNalog_Lokacija " +
                        "WHERE RadniNalog = ? " +
                        "AND Status = 3", radniNalogId);

                    if (zavrseneLokacije.Any())
                    {
                        db.Query<DID_RadniNalog>(
                            "UPDATE DID_RadniNalog " +
                            "SET Status = 4 " +
                            "WHERE Id = ?", radniNalogId);
                    }
                    else
                    {
                        db.Query<DID_RadniNalog>(
                            "UPDATE DID_RadniNalog " +
                            "SET Status = 3 " +
                            "WHERE Id = ?", radniNalogId);
                    }
                });

                alert.SetNegativeButton("ODUSTANI", (senderAlert, arg) =>
                {
                    localPozicija.Edit().Clear().Commit();
                    localAnketa.Edit().Clear().Commit();
                    intent = new Intent(this, typeof(Activity_Lokacije));
                    StartActivity(intent);
                });
                Dialog dialog = alert.Create();
                dialog.Show();
            }
            else
                SpremiAnketu();
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