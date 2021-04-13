using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Content.PM;

namespace App4New
{
    [Activity(Label = "Activity_PrvaDer_page1", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_PrvaDer_page1 : Activity
    {
        CheckBox rupeBox, legloBox, tragoviNoguBox, videnZiviGlodavacBox, izmetBox, videnUginuliGlodavacBox, stetaBox;
        Intent intent;

        static readonly ISharedPreferences localPozicija = Application.Context.GetSharedPreferences("pozicija", FileCreationMode.Private);
        static readonly ISharedPreferences localAnketa = Application.Context.GetSharedPreferences("anketa", FileCreationMode.Private);
        static ISharedPreferencesEditor localAnketaEdit = localAnketa.Edit();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.prvaDer_page1);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            rupeBox = FindViewById<CheckBox>(Resource.Id.rupeBox);
            legloBox = FindViewById<CheckBox>(Resource.Id.legloBox);
            izmetBox = FindViewById<CheckBox>(Resource.Id.izmetBox);
            stetaBox = FindViewById<CheckBox>(Resource.Id.stetaBox);
            tragoviNoguBox = FindViewById<CheckBox>(Resource.Id.tragoviNoguBox);
            videnZiviGlodavacBox = FindViewById<CheckBox>(Resource.Id.videnZiviGlodavacBox);
            videnUginuliGlodavacBox = FindViewById<CheckBox>(Resource.Id.videnUginuliGlodavacBox);

            SetActionBar(toolbar);
            ActionBar.Title = "Anketa";
            if (localAnketa.GetBoolean("visitedPage1", false))
            {
                rupeBox.Checked = localAnketa.GetBoolean("rupeBox", false);
                legloBox.Checked = localAnketa.GetBoolean("legloBox", false);
                tragoviNoguBox.Checked = localAnketa.GetBoolean("tragoviNoguBox", false);
                videnZiviGlodavacBox.Checked = localAnketa.GetBoolean("videnZiviGlodavacBox", false);
                izmetBox.Checked = localAnketa.GetBoolean("izmetBox", false);
                videnUginuliGlodavacBox.Checked = localAnketa.GetBoolean("videnUginuliGlodavacBox", false);
                stetaBox.Checked = localAnketa.GetBoolean("stetaBox", false);
            }
        }

        public void SaveState()
        {
            localAnketaEdit.PutBoolean("rupeBox", rupeBox.Checked);
            localAnketaEdit.PutBoolean("legloBox", legloBox.Checked);
            localAnketaEdit.PutBoolean("tragoviNoguBox", tragoviNoguBox.Checked);
            localAnketaEdit.PutBoolean("videnZiviGlodavacBox", videnZiviGlodavacBox.Checked);
            localAnketaEdit.PutBoolean("izmetBox", izmetBox.Checked);
            localAnketaEdit.PutBoolean("videnUginuliGlodavacBox", videnUginuliGlodavacBox.Checked);
            localAnketaEdit.PutBoolean("stetaBox", stetaBox.Checked);
            localAnketaEdit.PutBoolean("visitedPage1", true);
            localAnketaEdit.Commit();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menuBackNextHome, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.TitleFormatted.ToString() == "Početna")
                intent = new Intent(this, typeof(Activity_Pocetna));
            else if (item.TitleFormatted.ToString() == "nazad")
            {
                SaveState();
                if (localPozicija.GetBoolean("pozicije", false))
                    intent = new Intent(this, typeof(Activity_Pozicije));
                else if (localAnketa.GetBoolean("edit", false))
                    intent = new Intent(this, typeof(Activity_OdradeneAnkete));
                else
                    intent = new Intent(this, typeof(Activity_Pozicije));
            }
            else if (item.TitleFormatted.ToString() == "naprijed")
            {
                SaveState();
                intent = new Intent(this, typeof(Activity_PrvaDer_page2));
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