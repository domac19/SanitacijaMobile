using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Content.PM;

namespace App4New
{
    [Activity(Label = "Activity_PrvaDer_page2", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_PrvaDer_page2 : Activity
    {
        CheckBox stakoriBox, miseviBox, drugiGlodavciBox, okolisBox, hranaBox, odvodnjaBox;
        Intent intent;

        static readonly ISharedPreferences localAnketa = Application.Context.GetSharedPreferences("anketa", FileCreationMode.Private);
        static ISharedPreferencesEditor localAnketaEdit = localAnketa.Edit();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.prvaDer_page2);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            stakoriBox = FindViewById<CheckBox>(Resource.Id.stakoriBox);
            miseviBox = FindViewById<CheckBox>(Resource.Id.miseviBox);
            drugiGlodavciBox = FindViewById<CheckBox>(Resource.Id.drugiGlodavciBox);
            okolisBox = FindViewById<CheckBox>(Resource.Id.okolisBox);
            hranaBox = FindViewById<CheckBox>(Resource.Id.hranaBox);
            odvodnjaBox = FindViewById<CheckBox>(Resource.Id.odvodnjaBox);
           
            SetActionBar(toolbar);
            ActionBar.Title = "Anketa";
            if(localAnketa.GetBoolean("visitedPage2", false))
            {
                stakoriBox.Checked = localAnketa.GetBoolean("stakoriBox", false);
                miseviBox.Checked = localAnketa.GetBoolean("miseviBox", false);
                drugiGlodavciBox.Checked = localAnketa.GetBoolean("drugiGlodavciBox", false);
                okolisBox.Checked = localAnketa.GetBoolean("okolisBox", false);
                hranaBox.Checked = localAnketa.GetBoolean("hranaBox", false);
                odvodnjaBox.Checked = localAnketa.GetBoolean("odvodnjaBox", false);
            }
        }

        public void SaveState()
        {
            localAnketaEdit.PutBoolean("stakoriBox", stakoriBox.Checked);
            localAnketaEdit.PutBoolean("miseviBox", miseviBox.Checked);
            localAnketaEdit.PutBoolean("drugiGlodavciBox", drugiGlodavciBox.Checked);
            localAnketaEdit.PutBoolean("okolisBox", okolisBox.Checked);
            localAnketaEdit.PutBoolean("hranaBox", hranaBox.Checked);
            localAnketaEdit.PutBoolean("odvodnjaBox", odvodnjaBox.Checked);
            localAnketaEdit.PutBoolean("visitedPage2", true);
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
                intent = new Intent(this, typeof(Activity_PrvaDer_page1));
            }
            else if (item.TitleFormatted.ToString() == "naprijed")
            {
                SaveState();
                intent = new Intent(this, typeof(Activity_PrvaDer_page3));
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