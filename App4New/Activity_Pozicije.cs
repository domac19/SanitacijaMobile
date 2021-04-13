using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using Infobit.DDD.Data;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZXing.Mobile;

namespace App4New
{
    [Activity(Label = "Activity_Pozicije", Theme = "@style/Base.Theme.DesignDemo", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_Pozicije : Activity
    {
        TextView messagePoznata, messageBarcode, messagePosition, messageOpis;
        Button odabirKomitentaBtn, odabirLokacijeBtn, barcodeReaderNovaBtn, barcodeReaderPozBtn, searchPositionBtn, odaberPozicijuBtn, searchPozicijaNovaBtn, searchBarcodeBtn;
        RadioButton poznataPozicijaRadio, novaPozicijaRadio;
        LinearLayout poznataLayout, novaLayout;
        EditText positionInput, positionBrojInput, positionBrojOznakaInput, barcodeInput, opisPozicije;
        Intent intent;
        List<DID_LokacijaPozicija> pronadenaPozicija;
        int lokacijaId, radniNalog;
        bool flag, flagBarcode = true, sljedecaPozicija;

        static readonly ISharedPreferences localKomitentLokacija = Application.Context.GetSharedPreferences("localKomitentLokacija", FileCreationMode.Private);
        static readonly ISharedPreferences localPozicija = Application.Context.GetSharedPreferences("pozicija", FileCreationMode.Private);
        static ISharedPreferencesEditor localPozicijaEdit = localPozicija.Edit();
        static readonly ISharedPreferences localAnketa = Application.Context.GetSharedPreferences("anketa", FileCreationMode.Private);
        static ISharedPreferencesEditor localAnketaEdit = localAnketa.Edit();
        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.pozicije);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            opisPozicije = FindViewById<EditText>(Resource.Id.opisPozicije);
            messageBarcode = FindViewById<TextView>(Resource.Id.messageBarcode);
            barcodeInput = FindViewById<EditText>(Resource.Id.barcodeInput);
            searchBarcodeBtn = FindViewById<Button>(Resource.Id.searchBarcodeBtn);
            positionBrojInput = FindViewById<EditText>(Resource.Id.positionBrojInput);
            positionBrojOznakaInput = FindViewById<EditText>(Resource.Id.positionBrojOznakaInput);
            novaPozicijaRadio = FindViewById<RadioButton>(Resource.Id.novaPozicija);
            poznataPozicijaRadio = FindViewById<RadioButton>(Resource.Id.poznataPozicija);
            poznataLayout = FindViewById<LinearLayout>(Resource.Id.poznataLayout);
            novaLayout = FindViewById<LinearLayout>(Resource.Id.novaLayout);
            barcodeReaderNovaBtn = FindViewById<Button>(Resource.Id.barcodeReaderNovaBtn);
            barcodeReaderPozBtn = FindViewById<Button>(Resource.Id.barcodeReaderPozBtn);
            searchPositionBtn = FindViewById<Button>(Resource.Id.searchPositionBtn);
            odaberPozicijuBtn = FindViewById<Button>(Resource.Id.odaberPozicijuBtn);
            positionInput = FindViewById<EditText>(Resource.Id.positionInput);
            messagePoznata = FindViewById<TextView>(Resource.Id.messagePoznata);
            searchPozicijaNovaBtn = FindViewById<Button>(Resource.Id.searchPozicijaNovaBtn);
            messagePosition = FindViewById<TextView>(Resource.Id.messagePosition);
            odabirLokacijeBtn = FindViewById<Button>(Resource.Id.odabirLokacijeBtn);
            odabirKomitentaBtn = FindViewById<Button>(Resource.Id.odabirKomitentaBtn);
            odabirLokacijeBtn = FindViewById<Button>(Resource.Id.odabirLokacijeBtn);
            odabirKomitentaBtn = FindViewById<Button>(Resource.Id.odabirKomitentaBtn);
            messageOpis = FindViewById<TextView>(Resource.Id.messageOpis);

            SetActionBar(toolbar);
            ActionBar.Title = "Odabir pozicije";
            novaPozicijaRadio.Click += NovaPozicijaRadio_Click;
            poznataPozicijaRadio.Click += PoznataPozicijaRadio_Click;
            searchBarcodeBtn.Click += SearchBarcodeBtn_Click;
            searchPozicijaNovaBtn.Click += SearchPozicijaNovaBtn_Click;
            positionBrojInput.TextChanged += PositionBrojOznakaInput_TextChanged;
            positionBrojOznakaInput.TextChanged += PositionBrojOznakaInput_TextChanged;
            barcodeInput.TextChanged += BarcodeInput_TextChanged;
            positionInput.TextChanged += PositionInput_TextChanged;
            searchPositionBtn.Click += SearchPositionBtn_Click;
            odaberPozicijuBtn.Click += OdaberPozicijuBtn_Click;
            odabirKomitentaBtn.Click += OdabirKomitentaBtn_Click;   
            odabirLokacijeBtn.Click += OdabirLokacijeBtn_Click;
            positionBrojOznakaInput.KeyPress += PositionBrojOznakaInput_KeyPress;
            opisPozicije.TextChanged += OpisPozicije_TextChanged;
            lokacijaId = localKomitentLokacija.GetInt("lokacijaId", 0);
            odabirKomitentaBtn.Text = localKomitentLokacija.GetString("nazivKomitenta", null);
            odabirLokacijeBtn.Text = localKomitentLokacija.GetString("lokacijaNaziv", null);
            radniNalog = localRadniNalozi.GetInt("id", 0);
            barcodeReaderNovaBtn.Click += async (sender, e) => {
                MobileBarcodeScanner.Initialize(Application);
                var options = new ZXing.Mobile.MobileBarcodeScanningOptions();
                options.PossibleFormats = new List<ZXing.BarcodeFormat>() {
                    ZXing.BarcodeFormat.EAN_8, ZXing.BarcodeFormat.EAN_13, ZXing.BarcodeFormat.CODE_39
                };
                var scanner = new ZXing.Mobile.MobileBarcodeScanner();
                var result = await scanner.Scan(options);
                if (result != null)
                {
                    barcodeInput.Text = result.ToString();
                    searchBarcodeBtn.PerformClick();
                }
                else
                {
                    Message(messageBarcode, false, "Čitanje barcodea nije uspjelo");
                    messageBarcode.SetTextColor(ContextCompat.GetColorStateList(this, Resource.Color.redPrimary));
                }
                searchPositionBtn.PerformClick();
            };
            barcodeReaderPozBtn.Click += async (sender, e) => {
                MobileBarcodeScanner.Initialize(Application);
                var options = new ZXing.Mobile.MobileBarcodeScanningOptions();
                options.PossibleFormats = new List<ZXing.BarcodeFormat>() {
                    ZXing.BarcodeFormat.EAN_8, ZXing.BarcodeFormat.EAN_13, ZXing.BarcodeFormat.CODE_39
                };

                var scanner = new ZXing.Mobile.MobileBarcodeScanner();
                var result = await scanner.Scan(options);
                if (result != null)
                    positionInput.Text = result.ToString();
                else
                {
                    Message(messageBarcode, false, "Čitanje barcodea nije uspjelo");
                    messageBarcode.SetTextColor(ContextCompat.GetColorStateList(this, Resource.Color.redPrimary));
                }

                searchPositionBtn.PerformClick();
            };
          
            //pamcenje podataka prilikom vracanja na page sa ankete
            if (localPozicija.GetBoolean("pozicije", false))
            {
                if (localPozicija.GetBoolean("novaPozicijaPoznata", false))
                {
                    novaPozicijaRadio.PerformClick();
                    positionBrojOznakaInput.Text = localPozicija.GetString("positionBrojOznaka", null);
                    positionBrojInput.Text = localPozicija.GetString("positionBroj", null);
                    searchPozicijaNovaBtn.PerformClick();
                    barcodeInput.Text = localPozicija.GetString("barcode", null);
                    searchBarcodeBtn.PerformClick();
                }
                else
                {
                    poznataPozicijaRadio.PerformClick();
                    positionInput.Text = localPozicija.GetString("positionInput", null);
                    searchPositionBtn.PerformClick();
                }
                opisPozicije.Text = localPozicija.GetString("opisPozicije", null);
            }
            else
            {
                ShowNextPosition();
                if (sljedecaPozicija)
                {
                    poznataPozicijaRadio.PerformClick();
                    ShowNextPosition();
                }
                else
                    novaPozicijaRadio.PerformClick();
            }
        }

        public override void OnBackPressed()
        {
            intent = new Intent(this, typeof(Activity_Lokacije));
            StartActivity(intent);
        }

        public void PositionBrojOznakaInput_KeyPress(object sender, View.KeyEventArgs e)
        {
            e.Handled = false;
            if (e.KeyCode == Keycode.Enter)
                e.Handled = true;
        }

        public void OdabirKomitentaBtn_Click(object sender, EventArgs e)
        {
            localPozicijaEdit.PutBoolean("novaPozicija", false);
            localPozicijaEdit.Commit();
            intent = new Intent(this, typeof(Activity_Komitenti));
            StartActivity(intent);
        }

        public void OdabirLokacijeBtn_Click(object sender, EventArgs e)
        {
            intent = new Intent(this, typeof(Activity_Lokacije));
            StartActivity(intent);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            global::ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void OpisPozicije_TextChanged(object sender, EventArgs e)
        {
            if (opisPozicije.Text.Length > 100)
            {
                Message(messageOpis, false, "Opis pozicije može sadržavati najviše 100 znakova*");
                messageOpis.SetTextColor(ContextCompat.GetColorStateList(this, Resource.Color.redPrimary));
            }
            else
                messageOpis.Visibility = Android.Views.ViewStates.Invisible;
        }

        public void Check_OpisPozicije()
        {
            if (opisPozicije.Text.Length > 100)
                flag = false;
        }

        public void PoznataPozicijaRadio_Click(object sender, EventArgs e)
        {
            barcodeInput.Text = "";
            poznataLayout.Visibility = Android.Views.ViewStates.Visible;
            messagePoznata.Visibility = Android.Views.ViewStates.Invisible;
            novaLayout.Visibility = Android.Views.ViewStates.Gone;

            PositionClear();
            ShowNextPosition();
        }

        public void NovaPozicijaRadio_Click(object sender, EventArgs e)
        {
            poznataLayout.Visibility = Android.Views.ViewStates.Gone;
            novaLayout.Visibility = Android.Views.ViewStates.Visible;
            messagePosition.Visibility = Android.Views.ViewStates.Invisible;
            barcodeInput.Text = "";
            PositionClear();

            List<DID_LokacijaPozicija> pozicijaList = db.Query<DID_LokacijaPozicija>(
                "SELECT * " +
                "FROM DID_LokacijaPozicija " +
                "INNER JOIN DID_RadniNalog_Lokacija ON DID_LokacijaPozicija.SAN_Id = DID_RadniNalog_Lokacija.Lokacija " +
                "LEFT JOIN DID_Anketa ON DID_LokacijaPozicija.POZ_Id = DID_Anketa.ANK_POZ_Id " +
                "WHERE DID_LokacijaPozicija.SAN_Id = ? " +
                "AND DID_RadniNalog_Lokacija.RadniNalog = ? " +
                "AND DID_Anketa.ANK_POZ_Id IS NULL " +
                "ORDER BY CAST(POZ_Broj AS INTEGER), POZ_BrojOznaka", lokacijaId, radniNalog);

            if (pozicijaList.Count() > 1)
            {
                string zadnjaOznaka = db.Query<DID_LokacijaPozicija>(
                    "SELECT * " +
                    "FROM DID_LokacijaPozicija " +
                    "INNER JOIN DID_RadniNalog_Lokacija ON DID_LokacijaPozicija.SAN_Id = DID_RadniNalog_Lokacija.Lokacija " +
                    "WHERE DID_LokacijaPozicija.SAN_Id = ? " +
                    "AND DID_RadniNalog_Lokacija.RadniNalog = ? " +
                    "AND POZ_Broj = ? " +
                    "ORDER BY CAST(POZ_Broj AS INTEGER)", lokacijaId, radniNalog, pozicijaList.FirstOrDefault().POZ_Broj).LastOrDefault().POZ_BrojOznaka;
                string broj = pozicijaList.FirstOrDefault().POZ_Broj;

                while (true)
                {
                    if (string.IsNullOrEmpty(zadnjaOznaka))
                        zadnjaOznaka = "A";
                    else
                    {
                        //error string must me 1 character long
                        zadnjaOznaka = IncrementCharacter(Convert.ToChar(zadnjaOznaka)).ToString();
                    }

                    List<DID_LokacijaPozicija> nadenaPozicija = db.Query<DID_LokacijaPozicija>(
                        "SELECT * " +
                        "FROM DID_LokacijaPozicija " +
                        "INNER JOIN DID_RadniNalog_Lokacija ON DID_LokacijaPozicija.SAN_Id = DID_RadniNalog_Lokacija.Lokacija " +
                        "WHERE DID_LokacijaPozicija.SAN_Id = ? " +
                        "AND DID_RadniNalog_Lokacija.RadniNalog = ? " +
                        "AND POZ_Broj = ? " +
                        "AND POZ_BrojOznaka = ?", lokacijaId, radniNalog, broj, zadnjaOznaka);

                    if (!nadenaPozicija.Any())
                        break;
                };

                positionBrojInput.Text = broj;
                positionBrojOznakaInput.Text = zadnjaOznaka;
            }
            else
            {
                List<DID_LokacijaPozicija> svePozicije = db.Query<DID_LokacijaPozicija>(
                    "SELECT * " +
                    "FROM DID_LokacijaPozicija " +
                    "INNER JOIN DID_RadniNalog_Lokacija ON DID_LokacijaPozicija.SAN_Id = DID_RadniNalog_Lokacija.Lokacija " +
                    "WHERE DID_LokacijaPozicija.SAN_Id = ? " +
                    "AND DID_RadniNalog_Lokacija.RadniNalog = ? " +
                    "ORDER BY CAST(POZ_Broj AS INTEGER)", lokacijaId, radniNalog);
                if (svePozicije.Any())
                {
                    int zadnjiBroj = Convert.ToInt32(svePozicije.LastOrDefault().POZ_Broj) + 1;
                    positionBrojInput.Text = zadnjiBroj.ToString();
                }
                else
                    positionBrojInput.Text = "1";
            }
            searchPozicijaNovaBtn.PerformClick();
        }

        public char IncrementCharacter(char input)
        {
            return input == 'Z' ? 'A' : (char)(input + 1);
        }

        public void Message(TextView poruka, bool accepted, string text)
        {
            flag = accepted;
            poruka.Text = text;
            poruka.Visibility = Android.Views.ViewStates.Visible;
        }

        public void OdaberPozicijuBtn_Click(object sender, EventArgs e)
        {
            intent = new Intent(this, typeof(Activity_PozicijeList));
            StartActivity(intent);
        }

        public void ShowNextPosition()
        {
            List <DID_LokacijaPozicija> pozicijaList = db.Query<DID_LokacijaPozicija>(
                "SELECT * " +
                "FROM DID_LokacijaPozicija " +
                "INNER JOIN DID_RadniNalog_Lokacija ON DID_LokacijaPozicija.SAN_Id = DID_RadniNalog_Lokacija.Lokacija " +
                "LEFT JOIN DID_Anketa ON DID_LokacijaPozicija.POZ_Id = DID_Anketa.ANK_POZ_Id " +
                "WHERE DID_LokacijaPozicija.SAN_Id = ? " +
                "AND DID_RadniNalog_Lokacija.RadniNalog = ? " +
                "AND DID_Anketa.ANK_POZ_Id IS NULL " +
                "ORDER BY CAST(POZ_Broj AS INTEGER), POZ_BrojOznaka", lokacijaId, radniNalog);

            if (pozicijaList.Any())
            {
                positionInput.Text = pozicijaList.FirstOrDefault().POZ_Broj + pozicijaList.FirstOrDefault().POZ_BrojOznaka;
                sljedecaPozicija = true;
            }
            else
            {
                Message(messagePoznata, false, "Nema poznatih pozicija koje treba obici*");
                messagePoznata.SetTextColor(ContextCompat.GetColorStateList(this, Resource.Color.redPrimary));
                sljedecaPozicija = false;
            }
            searchPositionBtn.PerformClick();
        }

        public void SearchPositionBtn_Click(object sender, EventArgs e) 
        {
            if (!String.IsNullOrEmpty(positionInput.Text))
            {
                pronadenaPozicija = db.Query<DID_LokacijaPozicija>(
                    "SELECT * " +
                    "FROM DID_LokacijaPozicija " +
                    "INNER JOIN DID_RadniNalog_Lokacija ON DID_LokacijaPozicija.SAN_Id = DID_RadniNalog_Lokacija.Lokacija " +
                    "LEFT JOIN DID_Anketa ON DID_LokacijaPozicija.POZ_Id = DID_Anketa.ANK_POZ_Id " +
                    "WHERE DID_LokacijaPozicija.SAN_Id = ? " +
                    "AND DID_RadniNalog_Lokacija.RadniNalog = ? " +
                    "AND DID_Anketa.ANK_POZ_Id IS NULL " +
                    "AND (POZ_Broj || IFNULL(POZ_BrojOznaka, '') = ? " +
                    "OR POZ_Barcode = ?)", lokacijaId, radniNalog, positionInput.Text, positionInput.Text);

                if (pronadenaPozicija.Any())
                {
                    Message(messagePoznata, true, "Pozicija pronadena!");
                    messagePoznata.SetTextColor(ContextCompat.GetColorStateList(this, Resource.Color.green));
                    flag = true;
                }
                else
                {
                    Message(messagePoznata, false, "Pozicija nije pronađena*");
                    messagePoznata.SetTextColor(ContextCompat.GetColorStateList(this, Resource.Color.redPrimary));
                }
            }
        }

        public void PromjeniLokacijuBtn_Click(object sender, EventArgs args)
        {
            intent = new Intent(this, typeof(Activity_Lokacije));
            StartActivity(intent);
        }

        public void SearchPozicijaNovaBtn_Click(object sender, EventArgs args)
        {
            if (!String.IsNullOrEmpty(positionBrojInput.Text) && String.IsNullOrEmpty(positionBrojOznakaInput.Text) && positionBrojInput.Text.Length <= 3 && positionBrojOznakaInput.Text.Length <= 3)
            {
                List<DID_LokacijaPozicija> data = db.Query<DID_LokacijaPozicija>(
                    "SELECT * " +
                    "FROM DID_LokacijaPozicija " +
                    "WHERE SAN_Id = ? " +
                    "AND POZ_Broj = ? " +
                    "AND (POZ_BrojOznaka IS NULL OR POZ_BrojOznaka = '')", lokacijaId, Convert.ToInt32(positionBrojInput.Text));

                if (!data.Any())
                {
                    Message(messagePosition, true, "Unesena pozicija je ispavana*");
                    messagePosition.SetTextColor(ContextCompat.GetColorStateList(this, Resource.Color.green));
                }
                else
                {
                    Message(messagePosition, false, "Unesena pozicija već postoji na odabranoj lokaciji*");
                    messagePosition.SetTextColor(ContextCompat.GetColorStateList(this, Resource.Color.redPrimary));
                }

            }
            else if (!String.IsNullOrEmpty(positionBrojInput.Text) && !String.IsNullOrEmpty(positionBrojOznakaInput.Text) && positionBrojInput.Text.Length <= 3 && positionBrojOznakaInput.Text.Length <= 3)
            {
                List<DID_LokacijaPozicija> data = db.Query<DID_LokacijaPozicija>(
                    "SELECT * " +
                    "FROM DID_LokacijaPozicija " +
                    "WHERE SAN_Id = ? " +
                    "AND POZ_Broj = ? " +
                    "AND POZ_BrojOznaka = ?", lokacijaId, Convert.ToInt32(positionBrojInput.Text), positionBrojOznakaInput.Text);

                if (!data.Any())
                {
                    Message(messagePosition, true, "Unesena pozicija je ispavana!");
                    messagePosition.SetTextColor(ContextCompat.GetColorStateList(this, Resource.Color.green));
                }
                else
                {
                    Message(messagePosition, false, "Unesena pozicija već postoji na odabranoj lokaciji*");
                    messagePosition.SetTextColor(ContextCompat.GetColorStateList(this, Resource.Color.redPrimary));
                }
            }
            else
            {
                Message(messagePosition, false, "Unesite poziciju*");
                messagePosition.SetTextColor(ContextCompat.GetColorStateList(this, Resource.Color.redPrimary));
            }
        }

        public void SearchBarcodeBtn_Click(object sender, EventArgs args)
        {
            if (String.IsNullOrEmpty(barcodeInput.Text))
            {
                messageBarcode.Visibility = Android.Views.ViewStates.Invisible;
            }
            else if(barcodeInput.Text.Length > 15)
            {
                Message(messageBarcode, false, "Barkod može sadržavati maksimalno 15 brojeva*");
                messageBarcode.SetTextColor(ContextCompat.GetColorStateList(this, Resource.Color.redPrimary));
                flagBarcode = false;
            }
            else
            {
                List<DID_LokacijaPozicija> pozicijalist = db.Query<DID_LokacijaPozicija>(
                    "SELECT * " +
                    "FROM DID_LokacijaPozicija " +
                    "WHERE POZ_Barcode = ?", barcodeInput.Text);

                if (pozicijalist.Any())
                {
                    Message(messageBarcode, false, "Uneseni barkod vec postoji u bazi*");
                    messageBarcode.SetTextColor(ContextCompat.GetColorStateList(this, Resource.Color.redPrimary));
                    flagBarcode = false;
                }
                else
                {
                    Message(messageBarcode, true, "Uneseni barkod je ispravan!");
                    messageBarcode.SetTextColor(ContextCompat.GetColorStateList(this, Resource.Color.green));
                    flagBarcode = true;
                }
            }
        }

        public void PositionInput_TextChanged(object sender, EventArgs e)
        {
            flag = false;
            if (string.IsNullOrEmpty(positionInput.Text))
            {
                Message(messagePoznata, false, "Unesite poziciju*");
                messagePoznata.SetTextColor(ContextCompat.GetColorStateList(this, Resource.Color.redPrimary));
            }
            else if (positionInput.Text.Length > 15)
            {
                Message(messagePoznata, false, "Barkod može imati najviše 15 znakova*");
                messagePoznata.SetTextColor(ContextCompat.GetColorStateList(this, Resource.Color.redPrimary));
            }
            else
                messagePoznata.Visibility = Android.Views.ViewStates.Invisible;
        }

        public void PositionBrojOznakaInput_TextChanged(object sender, EventArgs e)
        {
            flag = false;
            if (string.IsNullOrEmpty(positionBrojInput.Text))
            {
                Message(messagePosition, false, "Unesite poziciju*");
                messagePosition.SetTextColor(ContextCompat.GetColorStateList(this, Resource.Color.redPrimary));
            }
            else if (positionBrojInput.Text.Length > 3 || positionBrojOznakaInput.Text.Length > 3)
            {
                Message(messagePosition, false, "Broj ili oznaka pozicije ne smiju imati više od 3 znaka*");
                messagePosition.SetTextColor(ContextCompat.GetColorStateList(this, Resource.Color.redPrimary));
            }
            else
                messagePosition.Visibility = Android.Views.ViewStates.Invisible;
        }

        public void BarcodeInput_TextChanged(object sender, EventArgs e)
        {
            flagBarcode = false;
            if(barcodeInput.Text.Length > 15)
            {
                Message(messageBarcode, false, "Barkod može sadržavati maksimalno 15 brojeva*");
                messageBarcode.SetTextColor(ContextCompat.GetColorStateList(this, Resource.Color.redPrimary));
            }
            else
                messageBarcode.Visibility = Android.Views.ViewStates.Invisible;
        }

        public void PositionClear()
        {
            flag = false;
            positionBrojInput.Text = "";
            positionBrojOznakaInput.Text = "";
            positionInput.Text = "";
            localAnketaEdit.Clear().Commit();
        }

        public void SaveState()
        {
            if (novaPozicijaRadio.Checked)
            {
                localPozicijaEdit.PutBoolean("novaPozicijaPoznata", true);
                localPozicijaEdit.PutString("positionBroj", positionBrojInput.Text);
                localPozicijaEdit.PutString("positionBrojOznaka", positionBrojOznakaInput.Text);
                if (flagBarcode)
                    localPozicijaEdit.PutString("barcode", barcodeInput.Text);
                else
                    localPozicijaEdit.PutString("barcode", null);
            }
            else
            {
                localPozicijaEdit.PutInt("pozicijaId", pronadenaPozicija.LastOrDefault().POZ_Id);
                localPozicijaEdit.PutBoolean("novaPozicijaPoznata", false);
                localPozicijaEdit.PutString("positionInput", positionInput.Text);
                localPozicijaEdit.PutInt("tipKutije", pronadenaPozicija.LastOrDefault().POZ_Tip);
            }
            localPozicijaEdit.PutString("opisPozicije", opisPozicije.Text);
            localPozicijaEdit.PutBoolean("pozicije", true);
            localPozicijaEdit.Commit();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menuNextHome, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            intent = new Intent(this, typeof(Activity_Pozicije));
            if (item.TitleFormatted.ToString() == "Početna")
                intent = new Intent(this, typeof(Activity_Pocetna));
            else if (item.TitleFormatted.ToString() == "naprijed")
            {
                if (poznataPozicijaRadio.Checked)
                    searchPositionBtn.PerformClick();
                else
                {
                    searchPozicijaNovaBtn.PerformClick();
                    if (!string.IsNullOrEmpty(barcodeInput.Text))
                        searchBarcodeBtn.PerformClick();
                }
                Check_OpisPozicije();
                if (flag)
                {
                    SaveState();
                    if (novaPozicijaRadio.Checked)
                        intent = new Intent(this, typeof(Activity_PrvaDer_page1));
                    else
                        intent = new Intent(this, typeof(Activity_Kontola_page1));
                }
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