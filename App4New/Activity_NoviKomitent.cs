using System;
using System.Collections.Generic;
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
    [Activity(Label = "Activity_NoviKomitent", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_NoviKomitent : Activity
    {
        RadioButton pravnaOsobaRBtn, fizickaOsobaRBtn, poznatiKomBtn, noviKomBtn;
        EditText nazivInput, OIBInput, adresaInput, postanskiBrojInput, mjestoInput, telefonInput, imeInput, prezimeInput, OIB1Input, OIB2Input;
        Button spremiBtn, odustaniBtn;
        LinearLayout linearLayout1, linearLayout2, poznatiKomitentLayout;
        Intent intent;
        ScrollView noviKomitentLayout;
        ListView partneriListView;
        List<T_KUPDOB> partnerList, partnerListFiltered;
        EditText searchInput;
        TextView resultMessage, msgPartnera, msgPartnera2, msgOIB, msgUlica, msgPostanskiBroj, msgMjesto, msgTelefon;
        bool flag = false;
        int radniNalogId;
        string sifra;

        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);
        static readonly ISharedPreferences localKomitentLokacija = Application.Context.GetSharedPreferences("localKomitentLokacija", FileCreationMode.Private);
        static ISharedPreferencesEditor localKomitentLokacijaEdit = localKomitentLokacija.Edit();

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.noviKomitent);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            linearLayout1 = FindViewById<LinearLayout>(Resource.Id.linearLayout1);
            linearLayout2 = FindViewById<LinearLayout>(Resource.Id.linearLayout2);
            poznatiKomitentLayout = FindViewById<LinearLayout>(Resource.Id.poznatiKomitentLayout);
            noviKomitentLayout = FindViewById<ScrollView>(Resource.Id.noviKomitentLayout);
            nazivInput = FindViewById<EditText>(Resource.Id.nazivInput);
            OIBInput = FindViewById<EditText>(Resource.Id.OIBInput);
            OIB1Input = FindViewById<EditText>(Resource.Id.OIB1Input);
            OIB2Input = FindViewById<EditText>(Resource.Id.OIB2Input);
            adresaInput = FindViewById<EditText>(Resource.Id.adresaInput);
            postanskiBrojInput = FindViewById<EditText>(Resource.Id.postanskiBrojInput);
            mjestoInput = FindViewById<EditText>(Resource.Id.mjestoInput);
            telefonInput = FindViewById<EditText>(Resource.Id.telefonInput);
            imeInput = FindViewById<EditText>(Resource.Id.imeInput);
            prezimeInput = FindViewById<EditText>(Resource.Id.prezimeInput);
            pravnaOsobaRBtn = FindViewById<RadioButton>(Resource.Id.pravnaOsobaRBtn);
            fizickaOsobaRBtn = FindViewById<RadioButton>(Resource.Id.fizickaOsobaRBtn);
            spremiBtn = FindViewById<Button>(Resource.Id.spremiBtn);
            odustaniBtn = FindViewById<Button>(Resource.Id.odustaniBtn);
            partneriListView = FindViewById<ListView>(Resource.Id.partneriListView);
            searchInput = FindViewById<EditText>(Resource.Id.searchInput);
            resultMessage = FindViewById<TextView>(Resource.Id.resultMessage);
            poznatiKomBtn = FindViewById<RadioButton>(Resource.Id.poznatiKomBtn);
            noviKomBtn = FindViewById<RadioButton>(Resource.Id.noviKomBtn);
            msgPartnera = FindViewById<TextView>(Resource.Id.msgPartnera);
            msgPartnera2 = FindViewById<TextView>(Resource.Id.msgPartnera2);
            msgOIB = FindViewById<TextView>(Resource.Id.msgOIB);
            msgUlica = FindViewById<TextView>(Resource.Id.msgUlica);
            msgPostanskiBroj = FindViewById<TextView>(Resource.Id.msgPostanskiBroj);
            msgMjesto = FindViewById<TextView>(Resource.Id.msgMjesto);
            msgTelefon = FindViewById<TextView>(Resource.Id.msgTelefon);

            SetActionBar(toolbar);
            ActionBar.Title = "Novi komitent";
            odustaniBtn.Click += OdustaniBtn_Click;
            radniNalogId = localRadniNalozi.GetInt("id", 0);
            noviKomBtn.Click += NoviKomBtn_Click;
            poznatiKomBtn.Click += PoznatiKomBtn_Click;
            nazivInput.TextChanged += NazivInput_TextChanged;
            imeInput.TextChanged += NazivInput_TextChanged;
            prezimeInput.TextChanged += NazivInput_TextChanged;
            OIB1Input.TextChanged += OIBInput_TextChanged;
            OIB2Input.TextChanged += OIBInput_TextChanged;
            OIBInput.TextChanged += OIBInput_TextChanged;
            adresaInput.TextChanged += AdresaInput_TextChanged;
            postanskiBrojInput.TextChanged += PostanskiBrojInput_TextChanged;
            mjestoInput.TextChanged += MjestoInput_TextChanged;
            telefonInput.TextChanged += TelefonInput_TextChanged;
            spremiBtn.Click += SpremiBtn_Click;
            OIB1Input.KeyPress += OIB1Input_KeyPress;
            imeInput.KeyPress += ImeInput_KeyPress;

            if (localRadniNalozi.GetBoolean("noviRN", false))
                partnerList = db.Query<T_KUPDOB>(
                    "SELECT * " +
                    "FROM T_KUPDOB " +
                    "LEFT JOIN DID_Lokacija ON T_KUPDOB.SIFRA = DID_Lokacija.SAN_KD_Sifra " +
                    "LEFT JOIN DID_RadniNalog_Lokacija ON DID_Lokacija.SAN_Id = DID_RadniNalog_Lokacija.Lokacija " +
                    "WHERE IFNULL(DID_RadniNalog_Lokacija.RadniNalog, 0) != ? " +
                    "GROUP BY NAZIV, OIB", radniNalogId);
            else if (!localRadniNalozi.GetBoolean("noviRN", false) && localKomitentLokacija.GetString("sifraKomitenta", null) == null)
                partnerList = db.Query<T_KUPDOB>(
                    "SELECT * " +
                    "FROM T_KUPDOB " +
                    "LEFT JOIN DID_Lokacija ON T_KUPDOB.SIFRA = DID_Lokacija.SAN_KD_Sifra " +
                    "LEFT JOIN DID_RadniNalog_Lokacija ON DID_Lokacija.SAN_Id = DID_RadniNalog_Lokacija.Lokacija " +
                    "WHERE IFNULL(DID_RadniNalog_Lokacija.RadniNalog, 0) != ? " +
                    "GROUP BY NAZIV, OIB", radniNalogId);
            else
                partnerList = db.Query<T_KUPDOB>(
                    "SELECT * " +
                    "FROM T_KUPDOB " +
                    "LEFT JOIN DID_Lokacija ON T_KUPDOB.SIFRA = DID_Lokacija.SAN_KD_Sifra " +
                    "LEFT JOIN DID_RadniNalog_Lokacija ON DID_Lokacija.SAN_Id = DID_RadniNalog_Lokacija.Lokacija " +
                    "WHERE IFNULL(DID_RadniNalog_Lokacija.RadniNalog, 0) != ? " +
                    "AND SIFRA != ? " +
                    "GROUP BY NAZIV, OIB", radniNalogId, localKomitentLokacija.GetString("sifraKomitenta", null));

            partnerListFiltered = partnerList;

            if (partnerListFiltered.Any())
                poznatiKomBtn.PerformClick();
            else
                noviKomBtn.PerformClick();
        }

        public void NoviKomBtn_Click(object sender, EventArgs e)
        {
            noviKomitentLayout.Visibility = Android.Views.ViewStates.Visible;
            poznatiKomitentLayout.Visibility = Android.Views.ViewStates.Gone;

            pravnaOsobaRBtn.Click += delegate
            {
                linearLayout2.Visibility = Android.Views.ViewStates.Visible;
                linearLayout1.Visibility = Android.Views.ViewStates.Gone;
                OIBInput.Visibility = Android.Views.ViewStates.Gone;
                nazivInput.Visibility = Android.Views.ViewStates.Visible;
                msgPartnera.Visibility = Android.Views.ViewStates.Invisible;
                msgPartnera2.Visibility = Android.Views.ViewStates.Gone;
                msgOIB.Visibility = Android.Views.ViewStates.Invisible;
                nazivInput.RequestFocus();
            };
            fizickaOsobaRBtn.Click += delegate
            {
                linearLayout2.Visibility = Android.Views.ViewStates.Gone;
                linearLayout1.Visibility = Android.Views.ViewStates.Visible;
                nazivInput.Visibility = Android.Views.ViewStates.Gone;
                OIBInput.Visibility = Android.Views.ViewStates.Visible;
                msgOIB.Visibility = Android.Views.ViewStates.Invisible;
                msgPartnera2.Visibility = Android.Views.ViewStates.Invisible;
                msgPartnera.Visibility = Android.Views.ViewStates.Gone;
                imeInput.RequestFocus();
            };
        }

        public void PoznatiKomBtn_Click(object sender, EventArgs e)
        {
            noviKomitentLayout.Visibility = Android.Views.ViewStates.Gone;
            poznatiKomitentLayout.Visibility = Android.Views.ViewStates.Visible;
            localKomitentLokacijaEdit.PutBoolean("noviKomitent", true);
            localKomitentLokacijaEdit.Commit();

            PrikazKomitenta();

            searchInput.KeyPress += (object senderSearch, View.KeyEventArgs eSearch) =>
            {
                eSearch.Handled = false;
                if (eSearch.KeyCode == Keycode.Enter)
                    eSearch.Handled = true;
            };

            searchInput.TextChanged += delegate
            {
                string input = searchInput.Text.ToLower();
                if (!string.IsNullOrEmpty(input))
                {
                    resultMessage.Visibility = Android.Views.ViewStates.Invisible;
                    partnerListFiltered = partnerList.Where(i =>
                        (i.SIFRA != null && i.SIFRA.ToLower().Contains(input)) ||
                        (i.OIB != null && i.OIB.ToLower().Contains(input)) ||
                        (i.OIB2 != null && i.OIB2.ToLower().Contains(input)) ||
                        (i.NAZIV != null && i.NAZIV.ToLower().Contains(input)) ||
                        (i.UL_BROJ != null && i.UL_BROJ.ToLower().Contains(input)) ||
                        (i.GRAD != null && i.GRAD.ToLower().Contains(input)) ||
                        (i.POSTA != null && i.POSTA.ToLower().Contains(input))).ToList();

                    partneriListView.Adapter = new Adapter_Komitent(this, partnerListFiltered);
                    if (!partnerListFiltered.Any())
                    {
                        partneriListView.Adapter = new Adapter_Komitent(this, partnerListFiltered);
                        resultMessage.Text = "Nije pronađen komitnet sa unesenim pojmom!";
                        resultMessage.Visibility = Android.Views.ViewStates.Visible;
                    }
                }
                else
                    PrikazKomitenta();
            };
        }

        public void OIB1Input_KeyPress(object sender, View.KeyEventArgs e)
        {
            e.Handled = false;
            if (e.KeyCode == Keycode.Enter)
            {
                OIB2Input.RequestFocus();
                e.Handled = true;
            }
        }

        public void ImeInput_KeyPress(object sender, View.KeyEventArgs e)
        {
            e.Handled = false;
            if (e.KeyCode == Keycode.Enter)
            {
                prezimeInput.RequestFocus();
                e.Handled = true;
            }
        }

        public override void OnBackPressed()
        {
            SaveState();
            intent = new Intent(this, typeof(Activity_RadniNalozi));
            StartActivity(intent);
        }

        public void NazivInput_TextChanged(object sender, EventArgs e)
        {
            if (pravnaOsobaRBtn.Checked)
            {
                if (String.IsNullOrEmpty(nazivInput.Text))
                    msgPartnera.Visibility = Android.Views.ViewStates.Visible;
                else if (nazivInput.Text.Length > 200)
                {
                    msgPartnera.Text = "Naziv ne smije imati više od 200 znakova*";
                    msgPartnera.Visibility = Android.Views.ViewStates.Visible;
                }
                else
                    msgPartnera.Visibility = Android.Views.ViewStates.Invisible;
            }
            else
            {
                if (String.IsNullOrEmpty(imeInput.Text) || String.IsNullOrEmpty(prezimeInput.Text))
                    msgPartnera2.Visibility = Android.Views.ViewStates.Visible;
                else if (imeInput.Text.Length > 50 || prezimeInput.Text.Length > 50)
                {
                    msgPartnera2.Text = "Ime i prezime ne smije imati više od 50 znakova*";
                    msgPartnera2.Visibility = Android.Views.ViewStates.Visible;
                }
                else
                    msgPartnera2.Visibility = Android.Views.ViewStates.Invisible;
            }
        }

        public void OIBInput_TextChanged(object sender, EventArgs e)
        {
            if (pravnaOsobaRBtn.Checked)
            {
                if (String.IsNullOrEmpty(OIB1Input.Text))
                {
                    msgOIB.Text = "Unesite OIB1*";
                    msgOIB.Visibility = Android.Views.ViewStates.Visible;
                }
                else if (OIB1Input.Text.Length != 11)
                {
                    msgOIB.Text = "OIB1 mora imati točno 11 brojeva*";
                    msgOIB.Visibility = Android.Views.ViewStates.Visible;
                }
                else if (!String.IsNullOrEmpty(OIB2Input.Text) && OIB2Input.Text.Length != 3)
                {
                    msgOIB.Text = "OIB2 mora imati točno 3 broja*";
                    msgOIB.Visibility = Android.Views.ViewStates.Visible;
                }
                else
                    msgOIB.Visibility = Android.Views.ViewStates.Invisible;
            }
            else
            {
                if (String.IsNullOrEmpty(OIBInput.Text))
                {
                    msgOIB.Text = "Unesite OIB*";
                    msgOIB.Visibility = Android.Views.ViewStates.Visible;
                }
                else if (OIBInput.Text.Length != 11)
                {
                    msgOIB.Text = "OIB mora imati točno 11 brojeva*";
                    msgOIB.Visibility = Android.Views.ViewStates.Visible;
                }
                else
                    msgOIB.Visibility = Android.Views.ViewStates.Invisible;
            }
        }

        public void AdresaInput_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(adresaInput.Text))
                msgUlica.Visibility = Android.Views.ViewStates.Visible;
            else if (adresaInput.Text.Length > 200)
            {
                msgUlica.Text = "Adresa može sadržavati maksimalno 200 znakova*";
                msgUlica.Visibility = Android.Views.ViewStates.Visible;
            }
            else
                msgUlica.Visibility = Android.Views.ViewStates.Invisible;
        }

        public void PostanskiBrojInput_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(postanskiBrojInput.Text))
                msgPostanskiBroj.Visibility = Android.Views.ViewStates.Visible;
            else if (postanskiBrojInput.Text.Length != 5)
            {
                msgPostanskiBroj.Text = "Poštanski broj se sastoji od 5 brojeva*";
                msgPostanskiBroj.Visibility = Android.Views.ViewStates.Visible;
            }
            else
                msgPostanskiBroj.Visibility = Android.Views.ViewStates.Invisible;
        }

        public void MjestoInput_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(mjestoInput.Text))
                msgMjesto.Visibility = Android.Views.ViewStates.Visible;
            else if (mjestoInput.Text.Length > 100)
            {
                msgMjesto.Text = "Mjesto može sadržavati najviše 100 znakova*";
                msgMjesto.Visibility = Android.Views.ViewStates.Visible;
            }
            else
                msgMjesto.Visibility = Android.Views.ViewStates.Invisible;
        }

        public void TelefonInput_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(telefonInput.Text))
                msgTelefon.Visibility = Android.Views.ViewStates.Visible;
            else if (telefonInput.Text.Length > 25)
            {
                msgTelefon.Text = "Telefonski broj može sadržavati maksimalno 25 brojeva*";
                msgTelefon.Visibility = Android.Views.ViewStates.Visible;
            }
            else
                msgTelefon.Visibility = Android.Views.ViewStates.Invisible;
        }

        public void PrikazKomitenta()
        {
            partnerListFiltered = partnerList;

            if (partnerListFiltered.Any())
            {
                partneriListView.Adapter = new Adapter_Komitent(this, partnerListFiltered);
                partneriListView.ItemClick += PartneriListView_ItemClick;
                resultMessage.Visibility = Android.Views.ViewStates.Gone;
            }
            else
            {
                resultMessage.Text = "Nema postojećih komitenata!";
                resultMessage.Visibility = Android.Views.ViewStates.Visible;
            }
        }

        private void PartneriListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            localKomitentLokacijaEdit.PutString("sifraKomitenta", partnerListFiltered[e.Position].SIFRA);
            localKomitentLokacijaEdit.PutString("nazivKomitenta", partnerListFiltered[e.Position].NAZIV);
            localKomitentLokacijaEdit.Commit();
            SaveState();
            intent = new Intent(this, typeof(Activity_NovaLokacija));
            StartActivity(intent);
        }

        public void OdustaniBtn_Click(object sender, EventArgs e)
        {
            SaveState();
            if (localRadniNalozi.GetBoolean("noviRN", false))
            {
                intent = new Intent(this, typeof(Activity_Pocetna));
                StartActivity(intent);
            }
            else
                base.OnBackPressed();
        }

        public void Message(TextView poruka, bool accepted, string text, EditText input)
        {
            flag = accepted;
            poruka.Text = text;
            poruka.Visibility = Android.Views.ViewStates.Visible;
            input.RequestFocus();
        }

        public void SpremiBtn_Click(object sender, EventArgs e)
        {
            flag = true;

            if (string.IsNullOrEmpty(telefonInput.Text))
                Message(msgTelefon, false, "Unesite telefon*", telefonInput);
            else if (telefonInput.Text.Length > 25)
                Message(msgTelefon, false, "Telefonski broj može sadržavati maksimalno 25 brojeva*", telefonInput);

            if (string.IsNullOrEmpty(mjestoInput.Text))
                Message(msgMjesto, false, "Unesite mjesto*", mjestoInput);
            else if (mjestoInput.Text.Length > 100)
                Message(msgMjesto, false, "Mjesto može sadržavati najviše 100 znakova*", mjestoInput);

            if (string.IsNullOrEmpty(postanskiBrojInput.Text))
                Message(msgPostanskiBroj, false, "Unesite poštanski broj*", postanskiBrojInput);
            else if (postanskiBrojInput.Text.Length != 5)
                Message(msgPostanskiBroj, false, "Poštanski broj se sastoji od 5 brojeva*", postanskiBrojInput);

            if (string.IsNullOrEmpty(adresaInput.Text))
                Message(msgUlica, false, "Unesite adresu*", adresaInput);
            else if (adresaInput.Text.Length > 200)
                Message(msgUlica, false, "Adresa može sadržavati maksimalno 200 znakova*", adresaInput);

            if (fizickaOsobaRBtn.Checked)
            {
                if (string.IsNullOrEmpty(OIBInput.Text))
                    Message(msgOIB, false, "Unesite OIB*", OIBInput);
                else if (OIBInput.Text.Length != 11)
                    Message(msgOIB, false, "OIB mora imati točno 11 brojeva*", OIBInput);
                else
                {
                    List<T_KUPDOB> partneri = db.Query<T_KUPDOB>(
                        "SELECT * " +
                        "FROM T_KUPDOB " +
                        "WHERE OIB = ?", OIBInput.Text);

                    if (partneri.Any())
                        Message(msgOIB, false, "Unjeli ste OIB koji već postoji*", OIBInput);
                }

                if (string.IsNullOrEmpty(prezimeInput.Text))
                    Message(msgPartnera2, false, "Unesite Ime i prezime*", prezimeInput);
                else if (prezimeInput.Text.Length > 50)
                    Message(msgPartnera2, false, "Ime i prezime ne smije imati više od 50 znakova*", prezimeInput);

                if (string.IsNullOrEmpty(imeInput.Text))
                    Message(msgPartnera2, false, "Unesite Ime i prezime*", imeInput);
                else if (imeInput.Text.Length > 50)
                    Message(msgPartnera2, false, "Ime i prezime ne smije imati više od 50 znakova*", imeInput);
            }
            else
            {
                if (string.IsNullOrEmpty(OIB1Input.Text))
                    Message(msgOIB, false, "Unesite OIB*", OIB1Input);
                else if (OIB1Input.Text.Length != 11)
                    Message(msgOIB, false, "OIB mora imati točno 11 brojeva*", OIB1Input);

                if (!string.IsNullOrEmpty(OIB2Input.Text) && OIB2Input.Text.Length != 3)
                    Message(msgOIB, false, "OIB2 mora imati točno 3 broja*", OIB2Input);
                else
                {
                    List<T_KUPDOB> partneri = db.Query<T_KUPDOB>(
                        "SELECT * " +
                        "FROM T_KUPDOB " +
                        "WHERE (OIB = ? AND OIB2 = null) " +
                        "OR (OIB = ? AND OIB2 = ?)", OIB1Input.Text, OIB1Input.Text, OIB2Input.Text);

                    if (partneri.Any())
                        Message(msgOIB, false, "Unjeli ste OIB koji već postoji*", OIB1Input);
                }

                if (string.IsNullOrEmpty(nazivInput.Text))
                    Message(msgPartnera, false, "Unesite naziv*", nazivInput);
                else if (nazivInput.Text.Length > 200)
                    Message(msgPartnera, false, "Naziv ne smije imati više od 200 znakova*", nazivInput);
            }

            if (flag)
            {
                if (pravnaOsobaRBtn.Checked)
                {
                    db.Query<T_KUPDOB>(
                        "INSERT INTO T_KUPDOB (" +
                            "SIFRA, " +
                            "NAZIV, " +
                            "OIB, " +
                            "OIB2, " +
                            "ULICA, " +
                            "POSTA, " +
                            "GRAD, " +
                            "TELEFON, " +
                            "TIP_PARTNERA, " +
                            "SinhronizacijaPrivremeniKljuc)" +
                        "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                            OIB1Input.Text + OIB2Input.Text,
                            nazivInput.Text,
                            OIB1Input.Text,
                            OIB2Input.Text,
                            adresaInput.Text,
                            postanskiBrojInput.Text,
                            mjestoInput.Text,
                            telefonInput.Text,
                            1,
                            OIB1Input.Text + OIB2Input.Text
                        );
                    sifra = OIB1Input.Text + OIB2Input.Text;
                }
                else
                {
                    db.Query<T_KUPDOB>(
                        "INSERT INTO T_KUPDOB (" +
                            "SIFRA, " +
                            "NAZIV, " +
                            "IME, " +
                            "PREZIME, " +
                            "OIB, " +
                            "ULICA, " +
                            "POSTA, " +
                            "GRAD, " +
                            "TELEFON," +
                            "TIP_PARTNERA, " +
                            "SinhronizacijaPrivremeniKljuc) " +
                        "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                            OIBInput.Text,
                            imeInput.Text + ' ' + prezimeInput.Text,
                            imeInput.Text,
                            prezimeInput.Text,
                            OIBInput.Text,
                            adresaInput.Text,
                            postanskiBrojInput.Text,
                            mjestoInput.Text,
                            telefonInput.Text,
                            2,
                            OIBInput.Text
                        );
                    sifra = OIBInput.Text;
                }
                T_KUPDOB komitent = db.Query<T_KUPDOB>(
                    "SELECT * " +
                    "FROM T_KUPDOB " +
                    "WHERE SIFRA = ?", sifra).FirstOrDefault();

                localKomitentLokacijaEdit.PutString("sifraKomitenta", komitent.SIFRA);
                localKomitentLokacijaEdit.PutString("nazivKomitenta", komitent.NAZIV);
                localKomitentLokacijaEdit.Commit();
                SaveState();
                intent = new Intent(this, typeof(Activity_NovaLokacija));
                StartActivity(intent);
            }
        }

        public void SaveState()
        {
            localKomitentLokacijaEdit.PutBoolean("noviKomitent", true);
            localKomitentLokacijaEdit.Commit();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menuBackHome, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            SaveState();
            if (item.TitleFormatted.ToString() == "Početna")
                intent = new Intent(this, typeof(Activity_Pocetna));
            else if (item.TitleFormatted.ToString() == "nazad")
                intent = new Intent(this, typeof(Activity_RadniNalozi));
            else if (item.TitleFormatted.ToString() == "Radni nalozi")
                intent = new Intent(this, typeof(Activity_RadniNalozi));
            else if (item.TitleFormatted.ToString() == "Prikaz odradenih anketa")
                intent = new Intent(this, typeof(Activity_OdradeneAnkete));
            else if (item.TitleFormatted.ToString() == "Prikaz potrošenih materijala")
                intent = new Intent(this, typeof(Activity_PotroseniMaterijali));

            StartActivity(intent);
            Finish();
            return base.OnOptionsItemSelected(item);
        }
    }
}