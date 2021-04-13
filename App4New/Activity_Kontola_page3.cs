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
    [Activity(Label = "Activity_Kontola_page3", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_Kontola_page3 : Activity
    {
        Button saveBtn;
        CheckBox neprovedenoRadioBtn;
        Spinner spinnerNeprovedeno;
        EditText napomeneInput;
        TextView neprovedenoTextView, msgNapomena;
        bool ANK_Kutija_Ljep, ANK_Kutija_Ziv, ANK_Kutija_Rot, novaKutijaUcinjenoZiv, uginuliStakorZiv, uginuliMisZiv, nemaKutijeZiv, kutijaOstecenaZiv, kutijaUrednaZiv,
            novaKutijaUcinjenoLjep, novaLjepPodUcinjenoLjep, ulovljenMisLjep, ulovljenStakorLjep, nemaKutijeLjep, kutijaOstecenaLjep, kutijaUrednaLjep, uginuliMisRot,
            noviMamciUcinjenoRot, novaKutijaUcinjenoRot, nadopunjenaMekomUcinjenoRot, kutijaUrednaRot, kutijaOstecenaRot, nemaKutijeRot, tragoviGlodanjaRot, izmetRot,
            pojedenaMekaRot, ostecenaMekaRot, uginuliStakorRot;
        Intent intent;

        int radniNalogId = localRadniNalozi.GetInt("id", 0);
        string koriniscnoIme = localUsername.GetString("username", null);
        string opisPozicije = localPozicija.GetString("opisPozicije", null);
        int pozicijaId = localPozicija.GetInt("pozicijaId", 0);
        string positionInput = localPozicija.GetString("positionInput", null);
        int tipDeratizacije = 1;
        int tipKutije = localPozicija.GetInt("tipKutije", 0);
        int lokacijaId = localKomitentLokacija.GetInt("lokacijaId", 0);     
        int radniNalogLokacijaId = localKomitentLokacija.GetInt("radniNalogLokacijaId", 0);

        static readonly ISharedPreferences localPotvrda = Application.Context.GetSharedPreferences("potvrda", FileCreationMode.Private);
        static ISharedPreferencesEditor localPotvrdaEdit = localPotvrda.Edit();
        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);
        static readonly ISharedPreferences localKomitentLokacija = Application.Context.GetSharedPreferences("localKomitentLokacija", FileCreationMode.Private);
        static readonly ISharedPreferences localPozicija = Application.Context.GetSharedPreferences("pozicija", FileCreationMode.Private);
        static ISharedPreferencesEditor localPozicijaEdit = localPozicija.Edit();
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
            msgNapomena = FindViewById<TextView>(Resource.Id.msgNapomena);

            SetActionBar(toolbar);
            ActionBar.Title = "Anketa";
            saveBtn.Click += SaveBtn_Click;
            spinnerNeprovedeno.Enabled = false;
            //napomeneInput.RequestFocus();
            napomeneInput.TextChanged += NapomeneInput_TextChanged;
            neprovedenoRadioBtn.Click += NeprovedenoRadioBtn_Click;

            List<DID_RazlogNeizvrsenjaDeratizacije> razlozi = db.Query<DID_RazlogNeizvrsenjaDeratizacije>(
                "SELECT * " +
                "FROM DID_RazlogNeizvrsenjaDeratizacije");

            List<string> razloziList = new List<string>();
            for(var i = 1; i < razlozi.Count; i++)
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

        public void NapomeneInput_TextChanged(object sender, EventArgs e)
        {
            if (napomeneInput.Text.Length > 512)
                msgNapomena.Visibility = Android.Views.ViewStates.Visible;
            else
                msgNapomena.Visibility = Android.Views.ViewStates.Invisible;
        }

        public void SaveState()
        {
            localPotvrdaEdit.PutBoolean("edit", false);
            localPotvrdaEdit.Commit();
            localAnketaEdit.PutString("napomeneInput", napomeneInput.Text);
            localAnketaEdit.PutString("spinnerSelectedItem", spinnerNeprovedeno.SelectedItem.ToString());
            localAnketaEdit.PutBoolean("neprovedenoRadioBtn", neprovedenoRadioBtn.Checked);
            localAnketaEdit.PutBoolean("visitedPage3", true);
            localAnketaEdit.Commit();
        }

        public void SpremiAnketu()
        {
            if (tipKutije == 1)
            {
                ANK_Kutija_Rot = true;
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
                ANK_Kutija_Ljep = true;
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
                ANK_Kutija_Ziv = true;
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

            db.Query<DID_LokacijaPozicija>(
                "UPDATE DID_LokacijaPozicija " +
                "SET POZ_Opis = ? " +
                "WHERE POZ_Id = ?", opisPozicije, pozicijaId);

            Guid id = Guid.NewGuid();
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
                    "ANK_Utvrdjeno_Rot_KutijaUredna, " +
                    "ANK_Utvrdjeno_Rot_KutijaOstecena, " +
                    "ANK_Utvrdjeno_Rot_NemaKutije, " +
                    "ANK_Utvrdjeno_Rot_TragoviGlodanja, " +
                    "ANK_Utvrdjeno_Rot_Izmet, " +
                    "ANK_Utvrdjeno_Rot_PojedenaMeka, " +
                    "ANK_Utvrdjeno_Rot_OstecenaMeka, " +
                    "ANK_Utvrdjeno_Rot_UginuliStakor, " +
                    "ANK_Utvrdjeno_Rot_UginuliMis, " +
                    "ANK_Utvrdjeno_Ljep_KutijaUredna, " +
                    "ANK_Utvrdjeno_Ljep_KutijaOstecena, " +
                    "ANK_Utvrdjeno_Ljep_NemaKutije, " +
                    "ANK_Utvrdjeno_Ljep_UlovljenStakor, " +
                    "ANK_Utvrdjeno_Ljep_UlovljenMis, " +
                    "ANK_Utvrdjeno_Ziv_KutijaUredna, " +
                    "ANK_Utvrdjeno_Ziv_KutijaOstecena, " +
                    "ANK_Utvrdjeno_Ziv_NemaKutije, " +
                    "ANK_Utvrdjeno_Ziv_UlovljenStakor, " +
                    "ANK_Utvrdjeno_Ziv_UlovljenMis, " +
                    "ANK_Ucinjeno_Rot_PostavljeniMamci, " +
                    "ANK_Ucinjeno_Rot_NovaKutija, " +
                    "ANK_Ucinjeno_Rot_NadopunjenaMekom, " +
                    "ANK_Ucinjeno_Ljep_NovaLjepljivaPodloska, " +
                    "ANK_Ucinjeno_Ljep_NovaKutija, " +
                    "ANK_Ucinjeno_Ziv_NovaKutija, " +
                    "ANK_TipDeratizacijskeKutijeSifra, " +
                    "SinhronizacijaPrivremeniKljuc" +
                    ")" +
                "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                    id,
                    radniNalogId,
                    koriniscnoIme,
                    pozicijaId,
                    DateTime.Now,
                    DateTime.Now,
                    tipDeratizacije,
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
                    id
                );

            localPozicijaEdit.PutInt("pozicijaId", pozicijaId);
            localPozicijaEdit.PutInt("lokacijaId", lokacijaId);
            localPozicijaEdit.PutString("positionInput", positionInput);
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
                        db.Query<DID_RadniNalog>(
                            "UPDATE DID_RadniNalog " +
                            "SET Status = 4 " +
                            "WHERE Id = ?", radniNalogId);
                    else
                        db.Query<DID_RadniNalog>(
                            "UPDATE DID_RadniNalog " +
                            "SET Status = 3 " +
                            "WHERE Id = ?", radniNalogId);
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