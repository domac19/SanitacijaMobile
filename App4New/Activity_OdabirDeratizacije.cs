
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace App4New
{
    [Activity(Label = "Activity_OdabirDeratizacije", Theme = "@style/AppTheme")]
    public class Activity_OdabirDeratizacije : Activity
    {
        Button radniNalogBtn, bezRadnogNalogaBtn;
        Toolbar toolbar;
        TextView naslov2TextView;
        Intent intent;

        static readonly ISharedPreferences localKomitentLokacija = Application.Context.GetSharedPreferences("localKomitentLokacija", FileCreationMode.Private);
        static readonly ISharedPreferences localNewPartner = Application.Context.GetSharedPreferences("partner", FileCreationMode.Private);
        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);
        static ISharedPreferencesEditor radniNaloziEdit = localRadniNalozi.Edit();
        static readonly ISharedPreferences localPozicija = Application.Context.GetSharedPreferences("pozicija", FileCreationMode.Private);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.odabirDeratizacije);
            toolbar = FindViewById<Toolbar>(Resource.Id.toolbarLogin);
            radniNalogBtn = FindViewById<Button>(Resource.Id.radniNalogBtn);
            bezRadnogNalogaBtn = FindViewById<Button>(Resource.Id.bezRadnogNalogaBtn);
            naslov2TextView = FindViewById<TextView>(Resource.Id.naslov2TextView);

            naslov2TextView.Text = "nove Deratizacije";
            SetActionBar(toolbar);
            ActionBar.Title = "Nova Deratizacija";
            localKomitentLokacija.Edit().Clear().Commit();
            localNewPartner.Edit().Clear().Commit();
            localRadniNalozi.Edit().Clear().Commit();
            localPozicija.Edit().Clear().Commit();

            radniNalogBtn.Click += delegate
            {
                radniNaloziEdit.PutBoolean("visitedRadniNalozi", true);
                radniNaloziEdit.Commit();
                intent = new Intent(this, typeof(Activity_RadniNalozi));
                StartActivity(intent);
            };

            bezRadnogNalogaBtn.Click += delegate
            {
                radniNaloziEdit.PutBoolean("visitedRadniNalozi", false);
                radniNaloziEdit.Commit();
                intent = new Intent(this, typeof(Activity_NoviRadniNalog));
                StartActivity(intent);
            };
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menuHome, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.TitleFormatted.ToString() == "Početna")
            {
                intent = new Intent(this, typeof(Activity_Pocetna));
                StartActivity(intent);
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}