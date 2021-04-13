using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Infobit.DDD.Data;
using Infobit.Security;
using Newtonsoft.Json;
using SQLite;

namespace App4New
{
    [Activity(Label = "Sanitacija", Theme = "@style/Base.Theme.DesignDemo", NoHistory = true, MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_Login : Activity
    {
        LinearLayout username, password, loadingLayout;
        RelativeLayout prijava;
        EditText usernameInput, passwordInput;
        TextView errMessage, verzijaTextView;
        Button loginBtn;
        CheckBox zapamtiPrijavu;
        bool flag, flagUsername = true, flagPassword = true;
        DeratizacijaSyncData SyncDataSend, SyncDataGet;

        static readonly ISharedPreferences localUsername = Application.Context.GetSharedPreferences("userLogin", FileCreationMode.Private);
        static ISharedPreferencesEditor usernameEdit = localUsername.Edit();

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.login);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            usernameInput = FindViewById<EditText>(Resource.Id.usernameInput);
            passwordInput = FindViewById<EditText>(Resource.Id.paswordInput);
            errMessage = FindViewById<TextView>(Resource.Id.errMessage);
            loginBtn = FindViewById<Button>(Resource.Id.loginBtn);
            username = FindViewById<LinearLayout>(Resource.Id.username);
            password = FindViewById<LinearLayout>(Resource.Id.password);
            loadingLayout = FindViewById<LinearLayout>(Resource.Id.loading);
            prijava = FindViewById<RelativeLayout>(Resource.Id.prijava);
            zapamtiPrijavu = FindViewById<CheckBox>(Resource.Id.zapamtiPrijavu);
            verzijaTextView = FindViewById<TextView>(Resource.Id.verzijaTextView);

            localUsername.Edit().Clear().Commit();
            SetActionBar(toolbar);
            ActionBar.Title = "Prijava";
            loginBtn.Click += LoginBtn_Click;
            usernameInput.FocusChange += HandleAnimationUsername;
            passwordInput.FocusChange += HandleAnimationPassword;
            usernameInput.TextChanged += UsernameInput_TextChanged;
            passwordInput.TextChanged += PasswordInput_TextChanged;
            passwordInput.KeyPress += PasswordInput_KeyPress;

            verzijaTextView.Text = "Verzija: " +
                Application.Context.ApplicationContext.PackageManager.GetPackageInfo(Application.Context.ApplicationContext.PackageName, 0).VersionName.ToString();

            Database.GetDBConnection();

            List<RememberCredentials> rememberedCredentials = db.Query<RememberCredentials>(
                "SELECT * " +
                "FROM RememberCredentials");

            if (rememberedCredentials.Any())
            {
                usernameInput.Text = rememberedCredentials.FirstOrDefault().Username;
                passwordInput.Text = rememberedCredentials.FirstOrDefault().Password;
                usernameInput.RequestFocus();
                passwordInput.RequestFocus();
                zapamtiPrijavu.Checked = true;
            }
        }

        public void PasswordInput_KeyPress(object sender, View.KeyEventArgs e)
        {
            e.Handled = false;
            if (e.KeyCode == Keycode.Enter)
            {
                loginBtn.PerformClick();
                e.Handled = true;
            }
        }

        public void HandleAnimationPassword(object sender, EventArgs e)
        {
            if (flagPassword)
            {
                ObjectAnimator.OfFloat(password, "scaleX", .80f).SetDuration(300).Start();
                ObjectAnimator.OfFloat(password, "scaleY", .80f).SetDuration(300).Start();
                ObjectAnimator.OfFloat(password, "translationX", -80).SetDuration(300).Start();
                ObjectAnimator.OfFloat(password, "translationY", -50).SetDuration(300).Start();
                flagPassword = false;
            }
            else if(!flagPassword && string.IsNullOrEmpty(passwordInput.Text))
            {
                ObjectAnimator.OfFloat(password, "scaleY", 1f).SetDuration(300).Start();
                ObjectAnimator.OfFloat(password, "scaleX", 1f).SetDuration(300).Start();
                ObjectAnimator.OfFloat(password, "translationX", 0).SetDuration(300).Start();
                ObjectAnimator.OfFloat(password, "translationY", 0).SetDuration(300).Start();
                flagPassword = true;
            }
        }

        public void HandleAnimationUsername(object sender, EventArgs e)
        {
            if (flagUsername)
            {
                ObjectAnimator.OfFloat(username, "scaleX", .80f).SetDuration(300).Start();
                ObjectAnimator.OfFloat(username, "scaleY", .80f).SetDuration(300).Start();
                ObjectAnimator.OfFloat(username, "translationX", -80).SetDuration(300).Start();
                ObjectAnimator.OfFloat(username, "translationY", -50).SetDuration(300).Start();
                flagUsername = false;
            }
            else if(!flagUsername && String.IsNullOrEmpty(usernameInput.Text))
            {
                ObjectAnimator.OfFloat(username, "scaleY", 1f).SetDuration(300).Start();
                ObjectAnimator.OfFloat(username, "scaleX", 1f).SetDuration(300).Start();
                ObjectAnimator.OfFloat(username, "translationX", 0).SetDuration(300).Start();
                ObjectAnimator.OfFloat(username, "translationY", 0).SetDuration(300).Start();
                flagUsername = true;
            }
        }

        public void UsernameInput_TextChanged(object sender, EventArgs e)
        {
            if (flag)
            {
                // Promijeni poruku errora
                if (String.IsNullOrEmpty(usernameInput.Text))
                    errMessage.Visibility = Android.Views.ViewStates.Visible;
                else
                    errMessage.Visibility = Android.Views.ViewStates.Invisible;
            }
        }

        public void PasswordInput_TextChanged(object sender, EventArgs e)
        {
            if (flag)
            {
                // Promijeni poruku errora
                if (String.IsNullOrEmpty(passwordInput.Text))
                    errMessage.Visibility = Android.Views.ViewStates.Visible;
                else
                    errMessage.Visibility = Android.Views.ViewStates.Invisible;
            }
        }

        public void EnableElements(bool show)
        {
            loginBtn.Enabled = show;
            zapamtiPrijavu.Enabled = show;
            usernameInput.Enabled = show;
            passwordInput.Enabled = show;
        }

        public async void LoginBtn_Click(object sender, EventArgs args)
        {
            EnableElements(false);

            if (String.IsNullOrEmpty(usernameInput.Text) || String.IsNullOrEmpty(passwordInput.Text))
            {
                loadingLayout.Visibility = Android.Views.ViewStates.Gone;
                

                if (String.IsNullOrEmpty(usernameInput.Text))
                {
                    errMessage.Visibility = Android.Views.ViewStates.Visible;
                    errMessage.Text = "Unesite korisničko ime!";
                    usernameInput.RequestFocus();
                    return;
                }
                else
                    errMessage.Visibility = Android.Views.ViewStates.Invisible;

                if (String.IsNullOrEmpty(passwordInput.Text))
                {
                    errMessage.Visibility = Android.Views.ViewStates.Visible;
                    errMessage.Text = "Unesite lozinku!";
                    passwordInput.RequestFocus();
                }
                else
                    errMessage.Visibility = Android.Views.ViewStates.Invisible;

                EnableElements(true);
            }
            else
            {
                //ako korisnik nema pristu internetu onda provjeri lokalno
                List<DID_DjelatnikUsername> user = db.Query<DID_DjelatnikUsername>(
                    "SELECT * " +
                    "FROM DID_DjelatnikUsername " +
                    "WHERE Username = ?", usernameInput.Text);

                if (user.Any())
                {
                    if (SecurePasswordHasher.Verify(passwordInput.Text, user.FirstOrDefault().Password))
                    {
                        // Ako je prvo pokretanje aplikacije skini maticne podatke sa servera
                        if (localUsername.GetBoolean("prvoPokretanje", false))
                        {
                            RunOnUiThread(() =>
                            {
                                flag = true;
                                loadingLayout.Visibility = Android.Views.ViewStates.Visible;
                            });

                            SyncDataSend = new DeratizacijaSyncData(user.FirstOrDefault().Username, user.FirstOrDefault().Password);
                            await Sinkornizacija();
                        }

                        KE_DJELATNICI djelatnik = db.Query<KE_DJELATNICI>(
                            "SELECT * " +
                            "FROM KE_DJELATNICI " +
                            "WHERE KE_MBR = ?", user.FirstOrDefault().Djelatnik).FirstOrDefault();

                        db.DeleteAll<RememberCredentials>();

                        if (zapamtiPrijavu.Checked)
                        {
                            db.Query<RememberCredentials>(
                               "INSERT INTO RememberCredentials (" +
                                   "Username, " +
                                   "Password)" +
                               "VALUES (?, ?)",
                                   usernameInput.Text,
                                   passwordInput.Text);
                        }

                        usernameEdit.PutString("username", user.FirstOrDefault().Username);
                        usernameEdit.PutString("nazivDjelatnika", djelatnik.KE_IME + ' ' + djelatnik.KE_PREZIME);
                        usernameEdit.PutString("sifraDjelatnika", djelatnik.KE_MBR);
                        usernameEdit.Commit();
                        errMessage.Visibility = Android.Views.ViewStates.Gone;
                        Toast.MakeText(this, "Uspješno ste se prijavili!", ToastLength.Short).Show();
                        Intent intent = new Intent(this, typeof(Activity_Pocetna));
                        StartActivity(intent);
                        Finish();
                        return;
                    }
                    else
                    {
                        EnableElements(true);

                        loadingLayout.Visibility = Android.Views.ViewStates.Gone;
                        errMessage.Text = "Unesite ispravno korisničko ime i lozinku!";
                        passwordInput.Text = "";
                        passwordInput.RequestFocus();
                    }
                }
                else
                {
                    EnableElements(true);

                    loadingLayout.Visibility = Android.Views.ViewStates.Gone;
                    errMessage.Visibility = Android.Views.ViewStates.Visible;
                    errMessage.Text = "Unesite ispravno korisničko ime i lozinku!";
                    passwordInput.Text = "";
                    flagPassword = true;
                    passwordInput.RequestFocus();
                }
            }
        }


        public async Task Sinkornizacija()
        {
            await Task.Run(() => {
                try
                {
                    CallWebService(JsonConvert.ToString(JsonConvert.SerializeObject(SyncDataSend)));
                    SpremanjePrimljenihPodataka();
                }
                catch (CallServiceExeption e)
                {
                    
                }
                flag = true;
            });
        }


        private string CallWebService(string JSON)
        {
            try
            {
                String output = "";
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 180);
                    string requestUrl = "http://services.infobit.info/DDD_Test/SyncService.svc/get/maticni";
                    //string requestUrl = "http://services.infobit.info/DDD/SyncService.svc/get/maticni";
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var request = new StringContent(JSON, Encoding.UTF8, "application/json");
                    var response = client.PostAsync(requestUrl, request).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        output = response.Content.ReadAsStringAsync().Result;
                        SyncDataGet = JsonConvert.DeserializeObject<DeratizacijaSyncData>(JsonConvert.DeserializeObject<string>(output));
                        if (!SyncDataGet.Login.Valid)
                        {
                            //syncErrMessage = "Greška prilikom autorizacije! Provjerite korisničko ime i lozinku!";
                            output = "Greška prilikom autorizacije! Provjerite korisničko ime i lozinku!";
                            throw new CallServiceExeption(output);
                        }
                        else if (SyncDataGet.Error != null)
                        {
                            //syncErrMessage = "Došlo je do pogreške prilikom sinhronizacije:\n" + SyncDataGet.Error.Message;
                            output = "Došlo je do pogreške prilikom sinhronizacije:\n" + SyncDataGet.Error.Message;
                            throw new CallServiceExeption(output);
                        }
                    }
                    else
                    {
                        //syncErrMessage = "Došlo je do neočekivane pogreške prilikom poziva sinhronizacije!";
                        output = "Došlo je do neočekivane pogreške prilikom poziva sinhronizacije!";
                        throw new CallServiceExeption(output);
                    }
                }
                return output;
            }
            catch (Exception e)
            {
                throw new CallServiceExeption(e.Message);
                //return e.Message;
                //syncErrMessage = "Došlo je do pogreške prilikom sinhronizacije:\n" + SyncDataGet.Error.Message;
                //output = "Došlo je do pogreške prilikom sinhronizacije:\n" + SyncDataGet.Error.Message;
            }
        }

        public class CallServiceExeption : Exception
        {
            public CallServiceExeption(string message)
               : base(message)
            {
            }
        }

        public void SpremanjePrimljenihPodataka()
        {
            foreach (var djelatnik in SyncDataGet.Djelatnici)
            {
                try
                {
                    db.Execute(
                    "INSERT INTO KE_DJELATNICI ( " +
                        "KE_MBR, " +
                        "KE_IME, " +
                        "KE_PREZIME, " +
                        "KE_MOBITEL, " +
                        "KE_OIB, " +
                        "KE_EMAIL, " +
                        "KE_ADRESA, " +
                        "KE_POSTA, " +
                        "KE_MJESTO) " +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        djelatnik.KE_MBR,
                        djelatnik.KE_IME,
                        djelatnik.KE_PREZIME,
                        djelatnik.KE_MOBITEL,
                        djelatnik.KE_OIB,
                        djelatnik.KE_EMAIL,
                        djelatnik.KE_ADRESA,
                        djelatnik.KE_POSTA,
                        djelatnik.KE_MJESTO);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE KE_DJELATNICI " +
                        "SET KE_IME = ?, " +
                            "KE_PREZIME = ?, " +
                            "KE_MOBITEL = ?, " +
                            "KE_OIB = ?, " +
                            "KE_EMAIL = ?, " +
                            "KE_ADRESA = ?, " +
                            "KE_POSTA = ?, " +
                            "KE_MJESTO = ? " +
                        "WHERE KE_MBR = ?",
                            djelatnik.KE_IME,
                            djelatnik.KE_PREZIME,
                            djelatnik.KE_MOBITEL,
                            djelatnik.KE_OIB,
                            djelatnik.KE_EMAIL,
                            djelatnik.KE_ADRESA,
                            djelatnik.KE_POSTA,
                            djelatnik.KE_MJESTO,
                            djelatnik.KE_MBR);
                }
            }

            foreach (var djelatnikUsername in SyncDataGet.DjelatniciUsername)
            {
                try
                {
                    db.Execute(
                        "INSERT INTO DID_DjelatnikUsername (" +
                            "Djelatnik, " +
                            "Username, " +
                            "Password)" +
                        "VALUES (?, ?, ?)",
                            djelatnikUsername.Djelatnik,
                            djelatnikUsername.Username,
                            djelatnikUsername.Password);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_DjelatnikUsername " +
                        "SET Username = ?, " +
                            "Password = ? " +
                        "WHERE Djelatnik = ?",
                            djelatnikUsername.Username,
                            djelatnikUsername.Password,
                            djelatnikUsername.Djelatnik);
                }
            }

            foreach (var pozicija in SyncDataGet.LokacijePozicije)
            {
                try
                {
                    db.Execute(
                        "INSERT INTO DID_LokacijaPozicija (" +
                            "POZ_Id, " +
                            "SAN_Id, " +
                            "POZ_Broj, " +
                            "POZ_BrojOznaka, " +
                            "POZ_Barcode, " +
                            "POZ_Tip, " +
                            "POZ_Status, " +
                            "POZ_Opis, " +
                            "SinhronizacijaPrivremeniKljuc, " +
                            "SinhronizacijaStatus)" +
                        "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                            pozicija.POZ_Id,
                            pozicija.SAN_Id,
                            pozicija.POZ_Broj,
                            pozicija.POZ_BrojOznaka,
                            pozicija.POZ_Barcode,
                            pozicija.POZ_Tip,
                            pozicija.POZ_Status,
                            pozicija.POZ_Opis,
                            null,
                            2);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_LokacijaPozicija " +
                        "SET SAN_Id = ?, " +
                            "POZ_Broj = ?, " +
                            "POZ_BrojOznaka = ?, " +
                            "POZ_Barcode = ?, " +
                            "POZ_Tip = ?, " +
                            "POZ_Status = ?, " +
                            "POZ_Opis = ?, " +
                            "SinhronizacijaPrivremeniKljuc = ?, " +
                            "SinhronizacijaStatus = ? " +
                        "WHERE POZ_Id = ?",
                            pozicija.SAN_Id,
                            pozicija.POZ_Broj,
                            pozicija.POZ_BrojOznaka,
                            pozicija.POZ_Barcode,
                            pozicija.POZ_Tip,
                            pozicija.POZ_Status,
                            pozicija.POZ_Opis,
                            null,
                            2,
                            pozicija.POZ_Id);
                }
            }

            foreach (var lokacija in SyncDataGet.Lokacije)
            {
                try
                {
                    db.Execute(
                        "INSERT INTO DID_Lokacija (" +
                             "SAN_Id, " +
                             "SAN_AnketePoPozicijama, " +
                             "SAN_KD_Sifra, " +
                             "SAN_Sifra, " +
                             "SAN_Naziv, " +
                             "SAN_Mjesto, " +
                             "SAN_Naselje, " +
                             "SAN_GradOpcina, " +
                             "SAN_UlicaBroj, " +
                             "SAN_OIBPartner, " +
                             "SAN_Status, " +
                             "SAN_Tip, " +
                             "SAN_Tip2, " +
                             "SinhronizacijaStatus)" +
                         "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                             lokacija.SAN_Id,
                             lokacija.SAN_AnketePoPozicijama,
                             lokacija.SAN_KD_Sifra,
                             lokacija.SAN_Sifra,
                             lokacija.SAN_Naziv,
                             lokacija.SAN_Mjesto,
                             lokacija.SAN_Naselje,
                             lokacija.SAN_GradOpcina,
                             lokacija.SAN_UlicaBroj,
                             lokacija.SAN_OIBPartner,
                             lokacija.SAN_Status,
                             lokacija.SAN_Tip,
                             lokacija.SAN_Tip2,
                             2);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_Lokacija " +
                        "SET SAN_AnketePoPozicijama = ?, " +
                            "SAN_KD_Sifra = ?, " +
                            "SAN_Sifra = ?, " +
                            "SAN_Naziv = ?, " +
                            "SAN_Mjesto = ?, " +
                            "SAN_Naselje = ?, " +
                            "SAN_GradOpcina = ?, " +
                            "SAN_UlicaBroj = ?, " +
                            "SAN_OIBPartner = ?, " +
                            "SAN_Status = ?, " +
                            "SAN_Tip = ?, " +
                            "SAN_Tip2 = ?," +
                            "SinhronizacijaPrivremeniKljuc = ?, " +
                            "SinhronizacijaStatus = ? " +
                        "WHERE SAN_Id = ?",
                            lokacija.SAN_AnketePoPozicijama,
                            lokacija.SAN_KD_Sifra,
                            lokacija.SAN_Sifra,
                            lokacija.SAN_Naziv,
                            lokacija.SAN_Mjesto,
                            lokacija.SAN_Naselje,
                            lokacija.SAN_GradOpcina,
                            lokacija.SAN_UlicaBroj,
                            lokacija.SAN_OIBPartner,
                            lokacija.SAN_Status,
                            lokacija.SAN_Tip,
                            lokacija.SAN_Tip2,
                            null,
                            2,
                            lokacija.SAN_Id);
                }
            }

            foreach (var komitent in SyncDataGet.Komitenti)
            {
                try
                {
                    db.Execute(
                        "INSERT INTO T_KUPDOB (" +
                            "SIFRA, " +
                            "TIP_PARTNERA, " +
                            "NAZIV, " +
                            "POSTA, " +
                            "GRAD, " +
                            "ULICA, " +
                            "UL_BROJ, " +
                            "DRZAVA, " +
                            "OIB, " +
                            "OIB2, " +
                            "ZIRO, " +
                            "TELEFON)" +
                        "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                            komitent.SIFRA,
                            komitent.TIP_PARTNERA,
                            komitent.NAZIV,
                            komitent.POSTA,
                            komitent.GRAD,
                            komitent.ULICA,
                            komitent.UL_BROJ,
                            komitent.DRZAVA,
                            komitent.OIB,
                            komitent.OIB2,
                            komitent.ZIRO,
                            komitent.TELEFON);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE T_KUPDOB " +
                        "SET TIP_PARTNERA = ?, " +
                            "NAZIV = ?, " +
                            "POSTA = ?, " +
                            "GRAD = ?, " +
                            "ULICA = ?, " +
                            "UL_BROJ = ?, " +
                            "DRZAVA = ?, " +
                            "OIB = ?, " +
                            "OIB2 = ?, " +
                            "ZIRO = ?, " +
                            "TELEFON = ?, " +
                            "SinhronizacijaPrivremeniKljuc = ? " +
                        "WHERE SIFRA = ?",
                            komitent.TIP_PARTNERA,
                            komitent.NAZIV,
                            komitent.POSTA,
                            komitent.GRAD,
                            komitent.ULICA,
                            komitent.UL_BROJ,
                            komitent.DRZAVA,
                            komitent.OIB,
                            komitent.OIB2,
                            komitent.ZIRO,
                            komitent.TELEFON,
                            null,
                            komitent.SinhronizacijaPrivremeniKljuc,
                            komitent.SIFRA);
                }
            }

            foreach (var djelatnost in SyncDataGet.Djelatnosti)
            {
                try
                {
                    db.Execute(
                        "INSERT INTO DID_Djelatnost (" +
                            "Sifra, " +
                            "Naziv)" +
                        "VALUES (?, ?)",
                            djelatnost.Naziv,
                            djelatnost.Sifra);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_Djelatnost " +
                        "SET Naziv = ? " +
                        "WHERE Sifra = ?",
                            djelatnost.Naziv,
                            djelatnost.Sifra);
                }
            }

            foreach (var nametnik in SyncDataGet.Nametnici)
            {
                try
                {
                    db.Execute(
                        "INSERT INTO DID_Nametnik (" +
                            "Sifra, " +
                            "Naziv, " +
                            "Tip)" +
                        "VALUES (?, ?, ?)",
                            nametnik.Sifra,
                            nametnik.Naziv,
                            nametnik.Tip);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_Nametnik " +
                        "SET Naziv = ?, " +
                            "Tip = ? " +
                        "WHERE Sifra = ?",
                            nametnik.Naziv,
                            nametnik.Tip,
                            nametnik.Sifra);
                }
            }

            foreach (var skladiste in SyncDataGet.Skladista)
            {
                try
                {
                    db.Execute(
                        "INSERT INTO T_SKL  (" +
                            "SKL_SIFRA , " +
                            "SKL_NAZIV) " +
                        "VALUES (?, ?)",
                            skladiste.SKL_SIFRA,
                            skladiste.SKL_NAZIV);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE T_SKL " +
                        "SET SKL_NAZIV = ? " +
                        "WHERE SKL_SIFRA = ?",
                            skladiste.SKL_NAZIV,
                            skladiste.SKL_SIFRA);
                }
            }

            foreach (var materijal in SyncDataGet.Materijali)
            {
                try
                {
                    db.Execute(
                        "INSERT INTO T_NAZR  (" +
                            "NAZR_SIFRA , " +
                            "NAZR_NAZIV, " +
                            "NAZR_BARKOD, " +
                            "NAZR_CIJENA_ART, " +
                            "NAZR_GRUPA, " +
                            "NAZR_JM_SIFRA, " +
                            "TipKemikalije) " +
                        "VALUES (?, ?, ?, ?, ?, ?, ?)",
                            materijal.NAZR_SIFRA,
                            materijal.NAZR_NAZIV,
                            materijal.NAZR_BARKOD,
                            materijal.NAZR_CIJENA_ART,
                            materijal.NAZR_GRUPA,
                            materijal.NAZR_JM_SIFRA,
                            materijal.TipKemikalije);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE T_NAZR " +
                        "SET NAZR_NAZIV = ?, " +
                            "NAZR_SIFRA = ?, " +
                            "NAZR_BARKOD = ?, " +
                            "NAZR_CIJENA_ART = ?, " +
                            "NAZR_GRUPA = ?, " +
                            "NAZR_JM_SIFRA = ?, " +
                            "TipKemikalije = ?, " +
                            "SinhronizacijaPrivremeniKljuc = ? " +
                        "WHERE NAZR_SIFRA = ?",
                            materijal.NAZR_NAZIV,
                            materijal.NAZR_SIFRA,
                            materijal.NAZR_BARKOD,
                            materijal.NAZR_CIJENA_ART,
                            materijal.NAZR_GRUPA,
                            materijal.NAZR_JM_SIFRA,
                            materijal.TipKemikalije,
                            materijal.SinhronizacijaPrivremeniKljuc,
                            materijal.NAZR_SIFRA);
                }
            }

            foreach (var grupa in SyncDataGet.MaterijaliGrupe)
            {
                try
                {
                    db.Execute(
                        "INSERT INTO T_GRU (GRU_SIFRA, GRU_NAZIV) " +
                        "VALUES (?, ?)",
                            grupa.GRU_SIFRA,
                            grupa.GRU_NAZIV);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE T_GRU " +
                        "SET GRU_NAZIV = ? " +
                        "WHERE GRU_SIFRA = ?",
                            grupa.GRU_NAZIV,
                            grupa.GRU_SIFRA);
                }
            }

            foreach (var mjernaJedinica in SyncDataGet.MjerneJedinice)
            {
                try
                {
                    db.Execute(
                        "INSERT INTO T_MjerneJedinice (" +
                            "Id, " +
                            "Naziv, " +
                            "Oznaka, " +
                            "VecaJedinicaId, " +
                            "GlavnaJedinica, " +
                            "KoeficijentPretvorbe, " +
                            "Tip) " +
                        "VALUES (?, ?, ?, ?, ?, ?, ?)",
                            mjernaJedinica.Id,
                            mjernaJedinica.Naziv,
                            mjernaJedinica.Oznaka,
                            mjernaJedinica.VecaJedinicaId,
                            mjernaJedinica.GlavnaJedinica,
                            mjernaJedinica.KoeficijentPretvorbe,
                            mjernaJedinica.Tip);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE T_MjerneJedinice " +
                        "SET Naziv = ?, " +
                            "Oznaka = ?, " +
                            "VecaJedinicaId = ?, " +
                            "GlavnaJedinica = ?, " +
                            "KoeficijentPretvorbe = ?, " +
                            "Tip = ? " +
                        "WHERE Id = ?",
                            mjernaJedinica.Naziv,
                            mjernaJedinica.Oznaka,
                            mjernaJedinica.VecaJedinicaId,
                            mjernaJedinica.GlavnaJedinica,
                            mjernaJedinica.KoeficijentPretvorbe,
                            mjernaJedinica.Tip,
                            mjernaJedinica.Id);
                }
            }

            foreach (var tipLokacije in SyncDataGet.TipoviLokacije)
            {
                try
                {
                    db.Execute(
                        "INSERT INTO DID_TipLokacije (" +
                            "Sifra, " +
                            "Naziv)" +
                        "VALUES (?, ?)",
                                tipLokacije.Sifra,
                                tipLokacije.Naziv);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_TipLokacije " +
                        "SET Naziv = ? " +
                        "WHERE Sifra = ?",
                            tipLokacije.Naziv,
                            tipLokacije.Sifra);
                }
            }

            foreach (var tipPosla in SyncDataGet.TipoviPosla)
            {
                try
                {
                    db.Execute(
                       "INSERT INTO DID_TipPosla (" +
                            "Sifra, " +
                            "Naziv, " +
                            "Djelatnost) " +
                        "VALUES (?, ?, ?)",
                            tipPosla.Sifra,
                            tipPosla.Naziv,
                            tipPosla.Djelatnost);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_TipPosla " +
                        "SET Naziv = ?, " +
                            "Djelatnost = ? " +
                        "WHERE Sifra = ?",
                            tipPosla.Naziv,
                            tipPosla.Djelatnost,
                            tipPosla.Sifra);
                }
            }

            foreach (var tipKutije in SyncDataGet.TipoviDeratizacijskeKutije)
            {
                try
                {
                    db.Execute(
                       "INSERT INTO DID_TipDeratizacijskeKutije (" +
                            "Sifra, " +
                            "Naziv) " +
                        "VALUES (?, ?)",
                            tipKutije.Sifra,
                            tipKutije.Naziv);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_TipDeratizacijskeKutije " +
                        "SET Naziv = ? " +
                        "WHERE Sifra = ?",
                            tipKutije.Naziv,
                            tipKutije.Sifra);
                }
            }

            foreach (var razinaInfestacije in SyncDataGet.RazineInfestacije)
            {
                try
                {
                    db.Execute(
                       "INSERT INTO DID_RazinaInfestacije (" +
                            "Sifra, " +
                            "Naziv) " +
                        "VALUES (?, ?)",
                            razinaInfestacije.Sifra,
                            razinaInfestacije.Naziv);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_RazinaInfestacije " +
                        "SET Naziv = ? " +
                        "WHERE Sifra = ?",
                            razinaInfestacije.Naziv,
                            razinaInfestacije.Sifra);
                }
            }

            foreach (var razlogNeizvrsenja in SyncDataGet.RazloziNeizvrsenjaDeratizacije)
            {
                try
                {
                    db.Execute(
                       "INSERT INTO DID_RazlogNeizvrsenjaDeratizacije (" +
                            "Sifra, " +
                            "Naziv) " +
                        "VALUES (?, ?)",
                            razlogNeizvrsenja.Sifra,
                            razlogNeizvrsenja.Naziv);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_RazlogNeizvrsenjaDeratizacije " +
                        "SET Naziv = ? " +
                        "WHERE Sifra = ?",
                            razlogNeizvrsenja.Naziv,
                            razlogNeizvrsenja.Sifra);
                }
            }

            foreach (var zupanija in SyncDataGet.Zupanije)
            {
                try
                {
                    db.Execute(
                       "INSERT INTO T_ZUPANIJE (" +
                            "Sifra, " +
                            "Naziv) " +
                        "VALUES (?, ?)",
                            zupanija.Sifra,
                            zupanija.Naziv);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE T_ZUPANIJE " +
                        "SET Naziv = ? " +
                        "WHERE Sifra = ?",
                            zupanija.Naziv,
                            zupanija.Sifra);
                }
            }

            foreach (var opcina in SyncDataGet.Opcine)
            {
                try
                {
                    db.Execute(
                       "INSERT INTO T_OPCINE (" +
                            "Sifra, " +
                            "Naziv, " +
                            "Zupanija) " +
                        "VALUES (?, ?, ?)",
                            opcina.Sifra,
                            opcina.Naziv,
                            opcina.Zupanija);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE T_OPCINE " +
                        "SET Naziv = ?, " +
                            "Zupanija = ?  " +
                        "WHERE Sifra = ?",
                            opcina.Naziv,
                            opcina.Zupanija,
                            opcina.Sifra);
                }
            }

            foreach (var naselje in SyncDataGet.Naselja)
            {
                try
                {
                    db.Execute(
                       "INSERT INTO T_NASELJA (" +
                            "Sifra, " +
                            "Naziv, " +
                            "Opcina, " +
                            "Posta, " +
                            "Tip) " +
                        "VALUES (?, ?, ?, ?, ?)",
                            naselje.Sifra,
                            naselje.Naziv,
                            naselje.Opcina,
                            naselje.Posta,
                            naselje.Tip);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE T_NASELJA " +
                        "SET Naziv = ?, " +
                            "Opcina = ?, " +
                            "Posta = ?, " +
                            "Tip = ? " +
                        "WHERE Sifra = ?",
                            naselje.Naziv,
                            naselje.Opcina,
                            naselje.Posta,
                            naselje.Tip,
                            naselje.Sifra);
                }
            }

            foreach (var statusRadnogNaloga in SyncDataGet.StatusRadnogNaloga)
            {
                try
                {
                    db.Execute(
                       "INSERT INTO DID_StatusRadnogNaloga (" +
                            "Sifra, " +
                            "Naziv)" +
                        "VALUES (?, ?)",
                            statusRadnogNaloga.Sifra,
                            statusRadnogNaloga.Naziv);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_StatusRadnogNaloga " +
                        "SET Naziv = ? " +
                        "WHERE Sifra = ?",
                            statusRadnogNaloga.Sifra,
                            statusRadnogNaloga.Naziv);
                }
            }

            foreach (var statusLokacije in SyncDataGet.StatusLokacije)
            {
                try
                {
                    db.Execute(
                       "INSERT INTO DID_StatusLokacije (" +
                            "Sifra, " +
                            "Naziv)" +
                        "VALUES (?, ?)",
                            statusLokacije.Sifra,
                            statusLokacije.Naziv);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_StatusLokacije " +
                        "SET Naziv = ? " +
                        "WHERE Sifra = ?",
                            statusLokacije.Sifra,
                            statusLokacije.Naziv);
                }
            }

            foreach (var statusLokacije_RadniNalog in SyncDataGet.StatusLokacijeRadnogNaloga)
            {
                try
                {
                    db.Execute(
                       "INSERT INTO DID_StatusLokacije_RadniNalog (" +
                            "Sifra, " +
                            "Naziv)" +
                        "VALUES (?, ?)",
                            statusLokacije_RadniNalog.Sifra,
                            statusLokacije_RadniNalog.Naziv);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_StatusLokacije_RadniNalog " +
                        "SET Naziv = ? " +
                        "WHERE Sifra = ?",
                            statusLokacije_RadniNalog.Sifra,
                            statusLokacije_RadniNalog.Naziv);
                }
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menuClose, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.TitleFormatted.ToString() == "close")
            {
                FinishAffinity();
                Java.Lang.JavaSystem.Exit(0);
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}