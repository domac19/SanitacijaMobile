using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Infobit.DDD.Data;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Content.PM;

namespace App4New
{
    [Activity(Label = "Activity_NoviRadniNalog", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_NoviRadniNalog : SelectedCulture
    {
        EditText godinaInput;
        Button spremiBtn, odustaniBtn;
        Intent intent;
        Spinner spinnerVoditelj, spinnerSkladiste;
        CalendarView kalendarDo, kalendarOd;
        DateTime datumOd, datumDo;
        TextView messageKalendar, messageGodina;
        List<string> djelatniciSifreList, skladistaSifreList;
        string sifraDjelatnika, sifraSkladista, sifraUlogiranogDjelatnika;
        bool flag = true;

        static readonly ISharedPreferences localUsername = Application.Context.GetSharedPreferences("userLogin", FileCreationMode.Private);
        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);
        static ISharedPreferencesEditor radniNaloziEdit = localRadniNalozi.Edit();

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.noviRadniNalog);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            godinaInput = FindViewById<EditText>(Resource.Id.godinaInput);
            spremiBtn = FindViewById<Button>(Resource.Id.spremiBtn);
            odustaniBtn = FindViewById<Button>(Resource.Id.odustaniBtn);
            spinnerVoditelj = FindViewById<Spinner>(Resource.Id.spinnerVoditelj);
            spinnerSkladiste = FindViewById<Spinner>(Resource.Id.spinnerSkladiste);
            kalendarOd = FindViewById<CalendarView>(Resource.Id.kalendarOd);
            kalendarDo = FindViewById<CalendarView>(Resource.Id.kalendarDo);
            messageKalendar = FindViewById<TextView>(Resource.Id.messageKalendar);
            messageGodina = FindViewById<TextView>(Resource.Id.messageGodina);

            SetActionBar(toolbar);
            ActionBar.Title = "Novi radni nalog";
            spremiBtn.Click += SpremiBtn_Click;
            odustaniBtn.Click += OdustaniBtn_Click;
            spinnerVoditelj.ItemSelected += SpinnerVoditelj_ItemSelected;
            spinnerSkladiste.ItemSelected += SpinnerSkladiste_ItemSelected;
            kalendarOd.DateChange += KalendarOdOnDateChange;
            kalendarDo.DateChange += KalendarDoOnDateChange;
            godinaInput.TextChanged += GodinaInput_TextChanged;
            godinaInput.Text = DateTime.Now.Year.ToString();
            datumOd = DateTime.Now.Date;
            datumDo = DateTime.Now.Date;
            sifraUlogiranogDjelatnika = localUsername.GetString("sifraDjelatnika", null);

            List<KE_DJELATNICI> djelatnici = db.Query<KE_DJELATNICI>(
                "SELECT * " +
                "FROM KE_DJELATNICI");

            List<string> djelatniciList = new List<string>();
            djelatniciSifreList = new List<string>();
            foreach (var item in djelatnici)
            {
                djelatniciList.Add(item.KE_IME + ' ' + item.KE_PREZIME);
                djelatniciSifreList.Add(item.KE_MBR);
            }
            ArrayAdapter<string> adapterDjelatnici = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, djelatniciList);
            adapterDjelatnici.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinnerVoditelj.Adapter = adapterDjelatnici;
            spinnerVoditelj.SetSelection(djelatniciSifreList.IndexOf(sifraUlogiranogDjelatnika.ToString()));

            List<T_SKL> skladista = db.Query<T_SKL>(
                "SELECT * " +
                "FROM T_SKL " +
                "WHERE SKL_SIFRA != 1000");

            List<string> skladistaList = new List<string>();
            skladistaSifreList = new List<string>();
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
        }

        public void GodinaInput_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(godinaInput.Text) || godinaInput.Text.Length > 4)
            {
                flag = false;
                messageGodina.Visibility = Android.Views.ViewStates.Visible;
            }
            else
            {
                flag = true;
                messageGodina.Visibility = Android.Views.ViewStates.Invisible;
            }
        }

        private void KalendarOdOnDateChange(object sender, CalendarView.DateChangeEventArgs args)
        {
            datumOd = new DateTime(args.Year, args.Month + 1, args.DayOfMonth);

            int result = DateTime.Compare(datumOd.Date, DateTime.Now.Date);
            if (result < 0)
            {
                messageKalendar.Visibility = Android.Views.ViewStates.Visible;
                flag = false;
            }
            else
            {
                int result2 = DateTime.Compare(datumDo, datumOd);
                if (result2 < 0)
                {
                    messageKalendar.Visibility = Android.Views.ViewStates.Visible;
                    flag = false;
                }
                else
                {
                    messageKalendar.Visibility = Android.Views.ViewStates.Invisible;
                    flag = true;
                }
            }
        }

        private void KalendarDoOnDateChange(object sender, CalendarView.DateChangeEventArgs args)
        {
            datumDo = new DateTime(args.Year, args.Month + 1, args.DayOfMonth);

            int result2 = DateTime.Compare(datumDo.Date, datumOd.Date);
            if (result2 < 0)
            {
                messageKalendar.Visibility = Android.Views.ViewStates.Visible;
                flag = false;
            }
            else
            {
                messageKalendar.Visibility = Android.Views.ViewStates.Invisible;
                flag = true;
            }
        }

        private void SpinnerVoditelj_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            sifraDjelatnika = djelatniciSifreList[e.Position];
        }

        private void SpinnerSkladiste_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            sifraSkladista = skladistaSifreList[e.Position];
        }

        public void OdustaniBtn_Click(object sender, EventArgs e)
        {
            intent = new Intent(this, typeof(Activity_Pocetna));
            StartActivity(intent);
        }

        public void SpremiBtn_Click(object sender, EventArgs args)
        {
            if (flag)
            {
                int id = -1;
                string mob = db.Query<KE_DJELATNICI>(
                    "SELECT * " +
                    "FROM KE_DJELATNICI " +
                    "WHERE KE_MBR = ?", sifraDjelatnika).FirstOrDefault().KE_MOBITEL;
                

                List<DID_RadniNalog> radniNalozi = db.Query<DID_RadniNalog>(
                    "SELECT * " +
                    "FROM DID_RadniNalog " +
                    "WHERE Id < 0");

                if (radniNalozi.Any())
                    id = radniNalozi.FirstOrDefault().Id - 1;

                db.Query<DID_RadniNalog>(
                    "INSERT INTO DID_RadniNalog (" +
                        "Id, " +
                        "Broj, " +
                        "Godina, " +
                        "Status, " +
                        "DatumOd, " +
                        "DatumDo, " +
                        "PokretnoSkladiste, " +
                        "Voditelj, " +
                        "VoditeljKontakt, " +
                        "Izdavatelj, " +
                        "DatumIzrade, " +
                        "SinhronizacijaPrivremeniKljuc, " +
                        "SinhronizacijaStatus)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        id,
                        id,
                        godinaInput.Text,
                        3,
                        datumOd,
                        datumDo,
                        sifraSkladista,
                        sifraDjelatnika,
                        mob,
                        sifraUlogiranogDjelatnika,
                        DateTime.Now,
                        id,
                        0
                    );

                // DID_RadniNalog_Djelatnik
                int radniNalogDjelatnikId = -1;

                List<DID_RadniNalog_Djelatnik> radniNaloziDjeltnik = db.Query<DID_RadniNalog_Djelatnik>(
                    "SELECT * " +
                    "FROM DID_RadniNalog_Djelatnik " +
                    "WHERE Id < 0");

                if (radniNaloziDjeltnik.Any())
                    radniNalogDjelatnikId = radniNaloziDjeltnik.FirstOrDefault().Id - 1;

                db.Query<DID_RadniNalog_Djelatnik>(
                    "INSERT INTO DID_RadniNalog_Djelatnik (" +
                        "Id, " +
                        "RadniNalog, " +
                        "Djelatnik, " +
                        "SinhronizacijaPrivremeniKljuc, " +
                        "SinhronizacijaStatus)" +
                    "VALUES (?, ?, ?, ?, ?)",
                        radniNalogDjelatnikId,
                        id,
                        sifraUlogiranogDjelatnika,
                        radniNalogDjelatnikId,
                        0
                    );

                radniNaloziEdit.PutBoolean("noviRN", true);
                radniNaloziEdit.PutString("skladiste", sifraSkladista);
                radniNaloziEdit.PutInt("id", id);
                radniNaloziEdit.Commit();
                intent = new Intent(this, typeof(Activity_NoviKomitent));
                StartActivity(intent);
            }
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