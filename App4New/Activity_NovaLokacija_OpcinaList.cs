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
using Android.Content.PM;

namespace App4New
{
    [Activity(Label = "Activity_NovaLokacija_OpcinaList", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_NovaLokacija_OpcinaList : Activity
    {
        TextView resultMessage;
        ListView opcinaListView;
        List<T_OPCINE> opcinaList, opcinaListFiltered;
        Intent intent;
        EditText searchInput;       

        static readonly ISharedPreferences localKomitentLokacija = Application.Context.GetSharedPreferences("localKomitentLokacija", FileCreationMode.Private);
        static ISharedPreferencesEditor localKomitentLokacijaEdit = localKomitentLokacija.Edit();

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.opcinaList);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            opcinaListView = FindViewById<ListView>(Resource.Id.opcinaListView);
            resultMessage = FindViewById<TextView>(Resource.Id.resultMessage);
            searchInput = FindViewById<EditText>(Resource.Id.searchInput);

            SetActionBar(toolbar);
            ActionBar.Title = "Odabir Općine/Županije";
            opcinaListView.ItemClick += OpcinaListView_ItemClick;
            int lokacijaId = localKomitentLokacija.GetInt("lokacijaId", 0);
            List<T_NASELJA> naselja = db.Query<T_NASELJA>("SELECT * FROM T_NASELJA");
            List<T_ZUPANIJE> zupanije = db.Query<T_ZUPANIJE>("SELECT * FROM T_ZUPANIJE");
            opcinaList = db.Query<T_OPCINE>(
                "SELECT * " +
                "FROM T_OPCINE");

            opcinaListFiltered = opcinaList;

            if (opcinaListFiltered.Any())
                opcinaListView.Adapter = new Adapter_Opcina(this, opcinaListFiltered, zupanije);
            else
                resultMessage.Visibility = Android.Views.ViewStates.Visible;

            searchInput.KeyPress += SearchInput_KeyPress;
            searchInput.TextChanged += delegate
            {
                string input = searchInput.Text.ToLower();
                if (!string.IsNullOrEmpty(input))
                {
                    resultMessage.Visibility = Android.Views.ViewStates.Invisible;
                    opcinaListFiltered = opcinaList.Where(i =>
                        (i.Naziv != null && i.Naziv.ToLower().Contains(input)) ||
                        (i.Zupanija != null && i.Zupanija.ToLower().Contains(input))).ToList();

                    if (opcinaListFiltered.Any())
                        opcinaListView.Adapter = new Adapter_Opcina(this, opcinaListFiltered, zupanije);
                    else
                    {
                        opcinaListView.Adapter = new Adapter_Opcina(this, opcinaListFiltered, zupanije);
                        resultMessage.Text = "Nije pronađena županija sa unesenim pojmom!";
                        resultMessage.Visibility = Android.Views.ViewStates.Visible;
                    }
                }
                else
                {
                    opcinaListFiltered = opcinaList;
                    if (opcinaListFiltered.Any())
                    {
                        opcinaListView.Adapter = new Adapter_Opcina(this, opcinaListFiltered, zupanije);
                        resultMessage.Visibility = Android.Views.ViewStates.Gone;
                    }
                    else
                    {
                        resultMessage.Text = "Nema dostupnih materijala na skladištu!";
                        resultMessage.Visibility = Android.Views.ViewStates.Visible;
                        Window.SetSoftInputMode(SoftInput.StateAlwaysHidden);
                        opcinaListView.Adapter = new Adapter_Opcina(this, opcinaListFiltered, zupanije);
                    }
                }
            };
        }

        public void SearchInput_KeyPress(object sender, View.KeyEventArgs e)
        {
            e.Handled = false;
            if (e.KeyCode == Keycode.Enter)
                e.Handled = true;
        }

        private void OpcinaListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            string nazivZupanije = db.Query<T_ZUPANIJE>(
                "SELECT * " +
                "FROM T_ZUPANIJE " +
                "WHERE Sifra = ?", opcinaListFiltered[e.Position].Zupanija).FirstOrDefault().Naziv;

            localKomitentLokacijaEdit.PutBoolean("prikazNovaLokacija", true);
            localKomitentLokacijaEdit.PutBoolean("opcineVisited", true);
            localKomitentLokacijaEdit.PutString("nazivOpcine", opcinaListFiltered[e.Position].Naziv + " / " + nazivZupanije);
            localKomitentLokacijaEdit.PutString("sifraOpcine", opcinaListFiltered[e.Position].Sifra);
            localKomitentLokacijaEdit.Commit();
            intent = new Intent(this, typeof(Activity_NovaLokacija));
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