using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Infobit.DDD.Data;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace App4New
{
    [Activity(Label = "Activity_Komitenti", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_Komitenti : Activity
    {
        ListView partneriListView;
        Android.Widget.Toolbar toolbar;
        List<T_KUPDOB> partnerList, partnerListFiltered;
        EditText searchInput;
        TextView resultMessage;
        int radniNalog;
        Intent intent;
        Button noviKomitentBtn;

        static readonly ISharedPreferences localKomitentLokacija = Application.Context.GetSharedPreferences("localKomitentLokacija", FileCreationMode.Private);
        static ISharedPreferencesEditor localKomitentLokacijaEdit = localKomitentLokacija.Edit();
        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.komitenti);
            toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            partneriListView = FindViewById<ListView>(Resource.Id.partneriListView);
            searchInput = FindViewById<EditText>(Resource.Id.searchInput);
            resultMessage = FindViewById<TextView>(Resource.Id.resultMessage);
            noviKomitentBtn = FindViewById<Button>(Resource.Id.noviKomitentBtn);

            SetActionBar(toolbar);
            ActionBar.Title = "Odabir partnera";
            radniNalog = localRadniNalozi.GetInt("id", 0);
            partneriListView.ItemClick += PartneriListView_ItemClick;
            noviKomitentBtn.Click += NoviKomitentBtn_Click;
            searchInput.TextChanged += SearchInput_TextChanged;
            searchInput.KeyPress += SearchInput_KeyPress;
            partnerList = db.Query<T_KUPDOB>(
                "SELECT * " +
                "FROM T_KUPDOB " +
                "INNER JOIN DID_Lokacija ON T_KUPDOB.SIFRA = DID_Lokacija.SAN_KD_SIFRA " +
                "INNER JOIN DID_RadniNalog_Lokacija ON DID_Lokacija.SAN_Id = DID_RadniNalog_Lokacija.Lokacija " +
                "WHERE DID_RadniNalog_Lokacija.RadniNalog = ? " +
                "GROUP BY SIFRA, TIP_PARTNERA, NAZIV, OIB", radniNalog);

            partnerListFiltered = partnerList;
            partneriListView.Adapter = new Adapter_Komitent(this, partnerListFiltered);
        }

        public override void OnBackPressed()
        {
            intent = new Intent(this, typeof(Activity_RadniNalozi));
            StartActivity(intent);
        }

        public void SearchInput_KeyPress(object sender, View.KeyEventArgs e)
        {
            e.Handled = false;
            if (e.KeyCode == Keycode.Enter)
                e.Handled = true;
        }

        public void SearchInput_TextChanged(object sender, EventArgs e)
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

                if (partnerListFiltered.Any())
                    partneriListView.Adapter = new Adapter_Komitent(this, partnerListFiltered);
                else
                {
                    partneriListView.Adapter = new Adapter_Komitent(this, partnerListFiltered);
                    resultMessage.Visibility = Android.Views.ViewStates.Visible;
                }
            }
            else
            {
                partnerListFiltered = partnerList;
                partneriListView.Adapter = new Adapter_Komitent(this, partnerListFiltered);
                resultMessage.Visibility = Android.Views.ViewStates.Invisible;
            }
        }

        public void NoviKomitentBtn_Click(object sender, EventArgs e)
        {
            intent = new Intent(this, typeof(Activity_NoviKomitent));
            StartActivity(intent);
        }

        private void PartneriListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            localKomitentLokacijaEdit.PutString("sifraKomitenta", partnerListFiltered[e.Position].SIFRA);
            localKomitentLokacijaEdit.PutString("nazivKomitenta", partnerListFiltered[e.Position].NAZIV);
            localKomitentLokacijaEdit.Commit();
            List<DID_Lokacija >lokacijeList = db.Query<DID_Lokacija>(
                "SELECT * " +
                "FROM DID_Lokacija " +
                "INNER JOIN T_KUPDOB ON DID_Lokacija.SAN_KD_SIFRA = T_KUPDOB.SIFRA " +
                "INNER JOIN DID_RadniNalog_Lokacija ON DID_Lokacija.SAN_Id = DID_RadniNalog_Lokacija.Lokacija " +
                "WHERE DID_Lokacija.SAN_KD_Sifra = ? " +
                "AND DID_RadniNalog_Lokacija.RadniNalog = ?", partnerListFiltered[e.Position].SIFRA, radniNalog);

            if(lokacijeList.Any())
                intent = new Intent(this, typeof(Activity_Lokacije));
            else
                intent = new Intent(this, typeof(Activity_NovaLokacija));

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
                intent = new Intent(this, typeof(Activity_RadniNalozi));
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