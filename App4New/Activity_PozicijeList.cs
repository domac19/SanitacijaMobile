using Infobit.DDD.Data;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using Android.Content.PM;

namespace App4New
{
    [Activity(Label = "Activity_PozicijeList", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_PozicijeList : Activity
    {
        TextView resultMessage;
        ListView pozicijaListView;
        EditText searchInput;
        Intent intent;
        List<DID_LokacijaPozicija> pozicijaList, pozicijaListFiltered;
        int radniNalog;

        static readonly ISharedPreferences localPozicija = Application.Context.GetSharedPreferences("pozicija", FileCreationMode.Private);
        static ISharedPreferencesEditor localPozicijaEdit = localPozicija.Edit();
        static readonly ISharedPreferences localKomitentLokacija = Application.Context.GetSharedPreferences("localKomitentLokacija", FileCreationMode.Private);
        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.pozicijeList);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            pozicijaListView = FindViewById<ListView>(Resource.Id.pozicijaListView);
            searchInput = FindViewById<EditText>(Resource.Id.searchInput);
            resultMessage = FindViewById<TextView>(Resource.Id.resultMessage);

            SetActionBar(toolbar);
            ActionBar.Title = "Odabir pozicije";
            pozicijaListView.ItemClick += LokacijaListView_ItemClick;
            searchInput.KeyPress += SearchInput_KeyPress;
            searchInput.TextChanged += SearchInput_TextChanged;
            int lokacijaId = localKomitentLokacija.GetInt("lokacijaId", 0);
            radniNalog = localRadniNalozi.GetInt("id", 0);
            pozicijaList = db.Query<DID_LokacijaPozicija>(
               "SELECT * " +
               "FROM DID_LokacijaPozicija " +
               "INNER JOIN DID_RadniNalog_Lokacija ON DID_LokacijaPozicija.SAN_Id = DID_RadniNalog_Lokacija.Lokacija " +
               "LEFT JOIN DID_Anketa ON DID_LokacijaPozicija.POZ_Id = DID_Anketa.ANK_POZ_Id " +
               "WHERE DID_LokacijaPozicija.SAN_Id = ? " +
               "AND DID_RadniNalog_Lokacija.RadniNalog = ? " +
               "AND DID_Anketa.ANK_POZ_Id IS NULL " +
               "ORDER BY CAST(POZ_Broj AS INTEGER), POZ_BrojOznaka", lokacijaId, radniNalog);

            pozicijaListFiltered = pozicijaList;

            if (pozicijaListFiltered.Any())
                pozicijaListView.Adapter = new Adapter_Pozicija(this, pozicijaListFiltered);
            else
                resultMessage.Visibility = Android.Views.ViewStates.Visible;
        }

        public override void OnBackPressed()
        {
            intent = new Intent(this, typeof(Activity_Pozicije));
            StartActivity(intent);
        }

        public void SearchInput_TextChanged(object sender, EventArgs e)
        {
            string input = searchInput.Text.ToLower();
            if (!string.IsNullOrEmpty(input))
            {
                resultMessage.Visibility = Android.Views.ViewStates.Invisible;
                pozicijaListFiltered = pozicijaList.Where(i =>
                    (i.POZ_Broj != null && i.POZ_Broj.Contains(input)) ||
                    (i.POZ_BrojOznaka != null && i.POZ_BrojOznaka.ToLower().Contains(input)) ||
                    (i.POZ_Barcode != null && i.POZ_Barcode.Contains(input))).ToList();

                if (pozicijaListFiltered.Any())
                {
                    pozicijaListView.Adapter = new Adapter_Pozicija(this, pozicijaListFiltered);
                }
                else
                {
                    pozicijaListView.Adapter = new Adapter_Pozicija(this, pozicijaListFiltered);
                    resultMessage.Text = "Nije pronađena pozicija sa unesenim pojmom!";
                    resultMessage.Visibility = Android.Views.ViewStates.Visible;
                }
            }
            else
            {
                pozicijaListFiltered = pozicijaList;

                if (pozicijaListFiltered.Any())
                {
                    pozicijaListView.Adapter = new Adapter_Pozicija(this, pozicijaListFiltered);
                    resultMessage.Visibility = Android.Views.ViewStates.Gone;
                }
                else
                {
                    resultMessage.Text = "Nema pronađena pozicija!";
                    resultMessage.Visibility = Android.Views.ViewStates.Visible;
                    Window.SetSoftInputMode(SoftInput.StateAlwaysHidden);
                    pozicijaListView.Adapter = new Adapter_Pozicija(this, pozicijaListFiltered);
                }
            }
        }

        public void SearchInput_KeyPress(object sender, View.KeyEventArgs e)
        {
            e.Handled = false;
            if (e.KeyCode == Keycode.Enter)
                e.Handled = true;
        }

        private void LokacijaListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            localPozicijaEdit.PutBoolean("pozicije", true);
            localPozicijaEdit.PutBoolean("novaPozicije", false);
            localPozicijaEdit.PutString("positionInput", pozicijaListFiltered[e.Position].POZ_Broj + pozicijaListFiltered[e.Position].POZ_BrojOznaka);
            localPozicijaEdit.PutString("barcode", pozicijaListFiltered[e.Position].POZ_Barcode);
            localPozicijaEdit.PutString("positionBroj", pozicijaListFiltered[e.Position].POZ_Broj);
            localPozicijaEdit.PutString("positionBrojOznaka", pozicijaListFiltered[e.Position].POZ_BrojOznaka);
            localPozicijaEdit.PutInt("pozicijaId", pozicijaListFiltered[e.Position].POZ_Id);
            localPozicijaEdit.PutInt("tipKutije", pozicijaListFiltered[e.Position].POZ_Tip);
            localPozicijaEdit.Commit();

            if(localPozicija.GetBoolean("pozicijeNovaDer", false))
                intent = new Intent(this, typeof(Activity_PrvaDer_page1));
            else
                intent = new Intent(this, typeof(Activity_Kontola_page1));
            StartActivity(intent);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menuClose, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.TitleFormatted.ToString() == "close")
                base.OnBackPressed();
            return base.OnOptionsItemSelected(item);
        }
    }
}