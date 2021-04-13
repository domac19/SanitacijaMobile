using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace App4New
{
    [Activity(Label = "Activity_Kontola_page1", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_Kontola_page1 : Activity
    {
        CheckBox uginuliStakorZiv, uginuliMisZiv, nemaKutijeZiv, kutijaOstecenaZiv, kutijaUrednaZiv, uginuliMisRot, uginuliStakorRot, ostecenaMekaRot, nemaKutijeLjep,
            pojedenaMekaRot, kutijaUrednaRot, kutijaOstecenaRot, nemaKutijeRot, tragoviGlodanjaRot,izmetRot, kutijaUrednaLjep, kutijaOstecenaLjep, ulovljenStakorLjep, ulovljenMisLjep;
        RadioButton kutijaSRodenticima, kutijaSLjepljivimPodloskama, kutijaZivolovke;
        GridLayout gridLjep, gridRot, gridZiv;
        int tipKutije;
        Intent intent;

        static readonly ISharedPreferences localPozicija = Application.Context.GetSharedPreferences("pozicija", FileCreationMode.Private);
        static readonly ISharedPreferences localAnketa = Application.Context.GetSharedPreferences("anketa", FileCreationMode.Private);
        static ISharedPreferencesEditor localAnketaEdit = localAnketa.Edit();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.kontola_page1);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            gridLjep = FindViewById<GridLayout>(Resource.Id.gridLjep);
            gridRot = FindViewById<GridLayout>(Resource.Id.gridRot);
            gridZiv = FindViewById<GridLayout>(Resource.Id.gridZiv);
            kutijaSRodenticima = FindViewById<RadioButton>(Resource.Id.kutijaSRodenticima);
            kutijaSLjepljivimPodloskama = FindViewById<RadioButton>(Resource.Id.kutijaSLjepljivimPodloskama);
            kutijaZivolovke = FindViewById<RadioButton>(Resource.Id.kutijaZivolovke);

            SetActionBar(toolbar);
            ActionBar.Title = "Anketa";
            tipKutije = localPozicija.GetInt("tipKutije", 0);
            if (tipKutije == 1)
            {
                kutijaUrednaRot = FindViewById<CheckBox>(Resource.Id.kutijaUrednaRot);
                kutijaOstecenaRot = FindViewById<CheckBox>(Resource.Id.kutijaOstecenaRot);
                nemaKutijeRot = FindViewById<CheckBox>(Resource.Id.nemaKutijeRot);
                tragoviGlodanjaRot = FindViewById<CheckBox>(Resource.Id.tragoviGlodanjaRot);
                izmetRot = FindViewById<CheckBox>(Resource.Id.izmetRot);
                pojedenaMekaRot = FindViewById<CheckBox>(Resource.Id.pojedenaMekaRot);
                ostecenaMekaRot = FindViewById<CheckBox>(Resource.Id.ostecenaMekaRot);
                uginuliStakorRot = FindViewById<CheckBox>(Resource.Id.uginuliStakorRot);
                uginuliMisRot = FindViewById<CheckBox>(Resource.Id.uginuliMisRot);
                kutijaSRodenticima.Visibility = Android.Views.ViewStates.Visible;
                kutijaSLjepljivimPodloskama.Visibility = Android.Views.ViewStates.Gone;
                kutijaZivolovke.Visibility = Android.Views.ViewStates.Gone;
                gridRot.Visibility = Android.Views.ViewStates.Visible;
                gridLjep.Visibility = Android.Views.ViewStates.Gone;
                gridZiv.Visibility = Android.Views.ViewStates.Gone;
                kutijaSRodenticima.Checked = true;
            }
            else if (tipKutije == 2)
            {
                kutijaUrednaLjep = FindViewById<CheckBox>(Resource.Id.kutijaUrednaLjep);
                kutijaOstecenaLjep = FindViewById<CheckBox>(Resource.Id.kutijaOstecenaLjep);
                nemaKutijeLjep = FindViewById<CheckBox>(Resource.Id.nemaKutijeLjep);
                ulovljenStakorLjep = FindViewById<CheckBox>(Resource.Id.ulovljenStakorLjep);
                ulovljenMisLjep = FindViewById<CheckBox>(Resource.Id.ulovljenMisLjep);
                kutijaSRodenticima.Visibility = Android.Views.ViewStates.Gone;
                kutijaSLjepljivimPodloskama.Visibility = Android.Views.ViewStates.Visible;
                kutijaZivolovke.Visibility = Android.Views.ViewStates.Gone;
                gridRot.Visibility = Android.Views.ViewStates.Gone;
                gridLjep.Visibility = Android.Views.ViewStates.Visible;
                gridZiv.Visibility = Android.Views.ViewStates.Gone;
                kutijaSLjepljivimPodloskama.Checked = true;
            }
            else
            {
                kutijaUrednaZiv = FindViewById<CheckBox>(Resource.Id.kutijaUrednaZiv);
                kutijaOstecenaZiv = FindViewById<CheckBox>(Resource.Id.kutijaOstecenaZiv);
                nemaKutijeZiv = FindViewById<CheckBox>(Resource.Id.nemaKutijeZiv);
                uginuliMisZiv = FindViewById<CheckBox>(Resource.Id.uginuliMisZiv);
                uginuliStakorZiv = FindViewById<CheckBox>(Resource.Id.uginuliStakorZiv);
                kutijaSRodenticima.Visibility = Android.Views.ViewStates.Gone;
                kutijaSLjepljivimPodloskama.Visibility = Android.Views.ViewStates.Gone;
                kutijaZivolovke.Visibility = Android.Views.ViewStates.Visible;
                gridRot.Visibility = Android.Views.ViewStates.Gone;
                gridLjep.Visibility = Android.Views.ViewStates.Gone;
                gridZiv.Visibility = Android.Views.ViewStates.Visible;
                kutijaZivolovke.Checked = true;
            }
            ClearCheckBox();

            if (localAnketa.GetBoolean("visitedPage1", false))
            {
                if (tipKutije == 1)
                {
                    kutijaUrednaRot.Checked = localAnketa.GetBoolean("kutijaUrednaRot", false);
                    kutijaOstecenaRot.Checked = localAnketa.GetBoolean("kutijaOstecenaRot", false);
                    nemaKutijeRot.Checked = localAnketa.GetBoolean("nemaKutijeRot", false);
                    tragoviGlodanjaRot.Checked = localAnketa.GetBoolean("tragoviGlodanjaRot", false);
                    izmetRot.Checked = localAnketa.GetBoolean("izmetRot", false);
                    pojedenaMekaRot.Checked = localAnketa.GetBoolean("pojedenaMekaRot", false);
                    ostecenaMekaRot.Checked = localAnketa.GetBoolean("ostecenaMekaRot", false);
                    uginuliStakorRot.Checked = localAnketa.GetBoolean("uginuliStakorRot", false);
                    uginuliMisRot.Checked = localAnketa.GetBoolean("uginuliMisRot", false);
                }
                else if (tipKutije == 2)
                {
                    kutijaUrednaLjep.Checked = localAnketa.GetBoolean("kutijaUrednaLjep", false);
                    kutijaOstecenaLjep.Checked = localAnketa.GetBoolean("kutijaOstecenaLjep", false);
                    nemaKutijeLjep.Checked = localAnketa.GetBoolean("nemaKutijeLjep", false);
                    ulovljenStakorLjep.Checked = localAnketa.GetBoolean("ulovljenStakorLjep", false);
                    ulovljenMisLjep.Checked = localAnketa.GetBoolean("ulovljenMisLjep", false);
                }
                else
                {
                    kutijaUrednaZiv.Checked = localAnketa.GetBoolean("kutijaUrednaZiv", false);
                    kutijaOstecenaZiv.Checked = localAnketa.GetBoolean("kutijaOstecenaZiv", false);
                    nemaKutijeZiv.Checked = localAnketa.GetBoolean("nemaKutijeZiv", false);
                    uginuliMisZiv.Checked = localAnketa.GetBoolean("uginuliMisZiv", false);
                    uginuliStakorZiv.Checked = localAnketa.GetBoolean("uginuliStakorZiv", false);
                }
            }
        }

        public void ClearCheckBox()
        {
            if (tipKutije == 1)
            {
                kutijaUrednaRot.Checked = false;
                kutijaOstecenaRot.Checked = false;
                nemaKutijeRot.Checked = false;
                tragoviGlodanjaRot.Checked = false;
                izmetRot.Checked = false;
                pojedenaMekaRot.Checked = false;
                ostecenaMekaRot.Checked = false;
                uginuliMisRot.Checked = false;
                uginuliMisRot.Checked = false;
            }
            else if (tipKutije == 2)
            {
                kutijaUrednaLjep.Checked = false;
                kutijaOstecenaLjep.Checked = false;
                nemaKutijeLjep.Checked = false;
                ulovljenStakorLjep.Checked = false;
                ulovljenMisLjep.Checked = false;
            }
            else
            {
                kutijaUrednaZiv.Checked = false;
                kutijaOstecenaZiv.Checked = false;
                nemaKutijeZiv.Checked = false;
                uginuliStakorZiv.Checked = false;
                uginuliMisZiv.Checked = false;
            }
        }

        public void SaveState()
        {
            if (tipKutije == 1)
            {
                localAnketaEdit.PutBoolean("kutijaUrednaRot", kutijaUrednaRot.Checked);
                localAnketaEdit.PutBoolean("kutijaOstecenaRot", kutijaOstecenaRot.Checked);
                localAnketaEdit.PutBoolean("nemaKutijeRot", nemaKutijeRot.Checked);
                localAnketaEdit.PutBoolean("tragoviGlodanjaRot", tragoviGlodanjaRot.Checked);
                localAnketaEdit.PutBoolean("izmetRot", izmetRot.Checked);
                localAnketaEdit.PutBoolean("pojedenaMekaRot", pojedenaMekaRot.Checked);
                localAnketaEdit.PutBoolean("ostecenaMekaRot", ostecenaMekaRot.Checked);
                localAnketaEdit.PutBoolean("uginuliStakorRot", uginuliStakorRot.Checked);
                localAnketaEdit.PutBoolean("uginuliMisRot", uginuliMisRot.Checked);
            }
            else if (tipKutije == 2)
            {
                localAnketaEdit.PutBoolean("kutijaUrednaLjep", kutijaUrednaLjep.Checked);
                localAnketaEdit.PutBoolean("kutijaOstecenaLjep", kutijaOstecenaLjep.Checked);
                localAnketaEdit.PutBoolean("nemaKutijeLjep", nemaKutijeLjep.Checked);
                localAnketaEdit.PutBoolean("ulovljenStakorLjep", ulovljenStakorLjep.Checked);
                localAnketaEdit.PutBoolean("ulovljenMisLjep", ulovljenMisLjep.Checked);
            }
            else
            {
                localAnketaEdit.PutBoolean("kutijaUrednaZiv", kutijaUrednaZiv.Checked);
                localAnketaEdit.PutBoolean("kutijaOstecenaZiv", kutijaOstecenaZiv.Checked);
                localAnketaEdit.PutBoolean("nemaKutijeZiv", nemaKutijeZiv.Checked);
                localAnketaEdit.PutBoolean("uginuliMisZiv", uginuliMisZiv.Checked);
                localAnketaEdit.PutBoolean("uginuliStakorZiv", uginuliStakorZiv.Checked);
            }
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
                if (localAnketa.GetBoolean("edit", false))
                    intent = new Intent(this, typeof(Activity_OdradeneAnkete));
                else
                    intent = new Intent(this, typeof(Activity_Pozicije));
            }
            else if(item.TitleFormatted.ToString() == "naprijed")
            {
                SaveState();
                intent = new Intent(this, typeof(Activity_Kontola_page2));
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