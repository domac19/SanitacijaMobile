using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using System.IO;
using SQLite;
using Plugin.Connectivity;
using Infobit.DDD.Data;
using System.Collections.Generic;
using Android.Content.PM;

namespace App4New
{
    [Activity(Label = "Activity_Pocetna", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_Pocetna : Activity
    {
        Button deratizacijaBtn, pregledOdradenihDeratizacijaBtn, syncBtn, pregledPotrosenihMaterijalaBtn, syncMasterDataBtn, pdfBtn;
        Intent intent;
        TextView userName, verzijaTextView, messageError;
        LinearLayout profil;
        Spinner spinnerSkladiste;
        List<string> skladistaSifreList;

        static readonly ISharedPreferences localPotvrda = Application.Context.GetSharedPreferences("potvrda", FileCreationMode.Private);
        static readonly ISharedPreferences localMaterijali = Application.Context.GetSharedPreferences("localMaterijali", FileCreationMode.Private);
        static readonly ISharedPreferences localTipDeratizacije = Application.Context.GetSharedPreferences("tipDeratizacije", FileCreationMode.Private);
        static readonly ISharedPreferences localOdradeneAnkete = Application.Context.GetSharedPreferences("ankete", FileCreationMode.Private);
        static readonly ISharedPreferences localPozicija = Application.Context.GetSharedPreferences("pozicija", FileCreationMode.Private);
        static readonly ISharedPreferences localAnketa = Application.Context.GetSharedPreferences("anketa", FileCreationMode.Private);
        static readonly ISharedPreferences localKomitentLokacija = Application.Context.GetSharedPreferences("localKomitentLokacija", FileCreationMode.Private);
        static readonly ISharedPreferences localDoneDeratization = Application.Context.GetSharedPreferences("doneDeratization", FileCreationMode.Private);
        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);
        static readonly ISharedPreferences localDeratization = Application.Context.GetSharedPreferences("deratizacija", FileCreationMode.Private);

        static readonly ISharedPreferences localUsername = Application.Context.GetSharedPreferences("userLogin", FileCreationMode.Private);
        static ISharedPreferencesEditor usernameEdit = localUsername.Edit();

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.pocetna);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            deratizacijaBtn = (Button)FindViewById(Resource.Id.deratizacijaBtn);
            pregledOdradenihDeratizacijaBtn = (Button)FindViewById(Resource.Id.pregledOdradenihDeratizacijaBtn);
            pregledPotrosenihMaterijalaBtn = (Button)FindViewById(Resource.Id.pregledPotrosenihMaterijalaBtn);
            syncBtn = (Button)FindViewById(Resource.Id.syncBtn);
            syncMasterDataBtn = (Button)FindViewById(Resource.Id.syncMasterDataBtn);
            userName = FindViewById<TextView>(Resource.Id.userNameTextView);
            messageError = FindViewById<TextView>(Resource.Id.messageError);
            profil = FindViewById<LinearLayout>(Resource.Id.profil);
            verzijaTextView = FindViewById<TextView>(Resource.Id.verzijaTextView);
            spinnerSkladiste = FindViewById<Spinner>(Resource.Id.spinnerSkladiste);

            SetActionBar(toolbar);
            ActionBar.Title = "Početna";
            syncBtn.Click += SyncBtn_Click;
            syncMasterDataBtn.Click += SyncMasterDataBtn_Click;
            deratizacijaBtn.Click += DeratizacijaBtn_Click;
            pregledPotrosenihMaterijalaBtn.Click += PregledPotrosenihMaterijalaBtn_Click;
            pregledOdradenihDeratizacijaBtn.Click += PregledOdradenihDeratizacijaBtn_Click;
            localPozicija.Edit().Clear().Commit();
            localAnketa.Edit().Clear().Commit();
            localKomitentLokacija.Edit().Clear().Commit();
            localDoneDeratization.Edit().Clear().Commit();
            localRadniNalozi.Edit().Clear().Commit();
            localDeratization.Edit().Clear().Commit();
            localOdradeneAnkete.Edit().Clear().Commit();
            localTipDeratizacije.Edit().Clear().Commit();
            localMaterijali.Edit().Clear().Commit();
            localPotvrda.Edit().Clear().Commit();
            profil.Click += Profil_Click;
            userName.Text = localUsername.GetString("nazivDjelatnika", null);
            spinnerSkladiste.ItemSelected += SpinnerSkladiste_ItemSelected;

            List<T_SKL> skladista = db.Query<T_SKL>(
                "SELECT * " +
                "FROM T_SKL " +
                "WHERE SKL_SIFRA != 1000");

            List<string> skladistaList = new List<string>();
            skladistaSifreList = new List<string>();

            skladistaList.Add("Odaberi skladište");
            skladistaSifreList.Add("");
            foreach (var item in skladista)
            {
                skladistaList.Add(item.SKL_SIFRA + " - " + item.SKL_NAZIV);
                skladistaSifreList.Add(item.SKL_SIFRA);
            }
            ArrayAdapter<string> adapterSkladista = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, skladistaList);
            adapterSkladista.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinnerSkladiste.Adapter = adapterSkladista;

            if (localUsername.GetString("aktivnoSkladiste", null) != null)
            {
                string aktivnoSkladiste = localUsername.GetString("aktivnoSkladiste", null);
                spinnerSkladiste.SetSelection(skladistaSifreList.IndexOf(aktivnoSkladiste));
            }

            verzijaTextView.Text = "Verzija: " + Application.Context.ApplicationContext.PackageManager.GetPackageInfo(Application.Context.ApplicationContext.PackageName, 0).VersionName.ToString();

        }

        private void SpinnerSkladiste_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if(skladistaSifreList[e.Position] == "" || skladistaSifreList[e.Position] == null)
            {
                EnableButtons(false);
                e.View.SetBackgroundColor(Android.Graphics.Color.Red);
            }
            else {
                EnableButtons(true);

                usernameEdit.PutString("aktivnoSkladiste", skladistaSifreList[e.Position]);
                usernameEdit.Commit();
            }
        }

        private void EnableButtons(bool enable)
        {
            deratizacijaBtn.Enabled = enable;
            pregledOdradenihDeratizacijaBtn.Enabled = enable;
            pregledPotrosenihMaterijalaBtn.Enabled = enable;
            syncBtn.Enabled = enable;
        }

        private void Profil_Click(object sender, EventArgs e)
        {
            intent = new Intent(this, typeof(Activity_Profile));
            StartActivity(intent);
        }

        public void DeratizacijaBtn_Click(object sender, EventArgs args)
        {
            intent = new Intent(this, typeof(Activity_RadniNalozi));
            StartActivity(intent);
        }

        public void PregledOdradenihDeratizacijaBtn_Click(object sender, EventArgs args)
        {
            intent = new Intent(this, typeof(Activity_OdradeneAnkete));
            StartActivity(intent);
        }

        public void PregledPotrosenihMaterijalaBtn_Click(object sender, EventArgs args)
        {
            intent = new Intent(this, typeof(Activity_PotroseniMaterijali));
            StartActivity(intent);
        }

        public void SyncBtn_Click(object sender, EventArgs args)
        {
            Toast.MakeText(this, "Sinkronizacija radnih naloga podataka", ToastLength.Short).Show();

            if (CrossConnectivity.Current.IsConnected)
            {
                intent = new Intent(this, typeof(Activity_Sinkronizacija));
                StartActivity(intent);
            }
            else
                Toast.MakeText(this, "Sinkronizacija se ne može pokrenuti jer nemate konekciju na internet!", ToastLength.Short).Show();
        }

        public void SyncMasterDataBtn_Click(object sender, EventArgs args)
        {
            Toast.MakeText(this, "Sinkronizacija matičnih podataka", ToastLength.Short).Show();

            if (CrossConnectivity.Current.IsConnected)
            {
                intent = new Intent(this, typeof(Activity_SinkronizacijaMaticnihPodataka));
                StartActivity(intent);
            }
            else
                Toast.MakeText(this, "Sinkronizacija se ne može pokrenuti jer nemate konekciju na internet!", ToastLength.Short).Show();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menuLogout, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override void OnBackPressed()
        {
            RunOnUiThread(() =>
            {
                Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
                alert.SetTitle("Potvrda");
                alert.SetMessage("Jeste li sigurni da se želite odjaviti kao korisnik " + userName.Text + "?");
                alert.SetPositiveButton("Odjava", (senderAlert, arg) =>
                {
                    intent = new Intent(this, typeof(Activity_Login));
                    StartActivity(intent);
                    FinishAffinity();
                    Toast.MakeText(this, "Uspješno ste se odjavili!", ToastLength.Short).Show();
                });
                alert.SetNegativeButton("Odustani", (senderAlert, arg) => { });
                Dialog dialog = alert.Create();
                dialog.Show();
                alert.Dispose();
            });
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.TitleFormatted.ToString() == "odjava")
            {
                RunOnUiThread(() =>
                {
                    Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
                    alert.SetTitle("Potvrda");
                    alert.SetMessage("Jeste li sigurni da se želite odjaviti kao korisnik " + userName.Text + "?");
                    alert.SetPositiveButton("Odjava", (senderAlert, arg) =>
                    {
                        intent = new Intent(this, typeof(Activity_Login));
                        StartActivity(intent);
                        FinishAffinity();
                        Toast.MakeText(this, "Uspješno ste se odjavili!", ToastLength.Short).Show();
                    });
                    alert.SetNegativeButton("Odustani", (senderAlert, arg) => { });
                    Dialog dialog = alert.Create();
                    dialog.Show();
                    alert.Dispose();
                });
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}