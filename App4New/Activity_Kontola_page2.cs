using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace App4New
{
    [Activity(Label = "Activity_Kontola_page2", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_Kontola_page2 : Activity
    {
        RadioButton rotKutijaSRodenticima, rotKutijasLjepljivimPodloskom, rotKutijaZivolovka;
        CheckBox noviMamciUcinjenoRot, novaKutijaUcinjenoRot, nadopunjenaMekomUcinjenoRot, novaLjepPodUcinjenoLjep, novaKutijaUcinjenoLjep, novaKutijaUcinjenoZiv;
        LinearLayout checkboxRot, checkboxLjep, checkboxZiv;
        int tipKutije;
        Intent intent;

        static readonly ISharedPreferences localPozicija = Application.Context.GetSharedPreferences("pozicija", FileCreationMode.Private);
        static readonly ISharedPreferences localAnketa = Application.Context.GetSharedPreferences("anketa", FileCreationMode.Private);
        static ISharedPreferencesEditor localAnketaEdit = localAnketa.Edit();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.kontola_page2);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            rotKutijaZivolovka = FindViewById<RadioButton>(Resource.Id.kutijaZivolovka);
            rotKutijasLjepljivimPodloskom = FindViewById<RadioButton>(Resource.Id.kutijasLjepljivimPodloskom);
            rotKutijaSRodenticima = FindViewById<RadioButton>(Resource.Id.kutijaSRodenticima);
            checkboxLjep = FindViewById<LinearLayout>(Resource.Id.checkboxLjep);
            checkboxRot = FindViewById<LinearLayout>(Resource.Id.checkboxRot);
            checkboxZiv = FindViewById<LinearLayout>(Resource.Id.checkboxZiv);
            noviMamciUcinjenoRot = FindViewById<CheckBox>(Resource.Id.noviMamciUcinjenoRot);
            novaKutijaUcinjenoRot = FindViewById<CheckBox>(Resource.Id.novaKutijaUcinjenoRot);
            nadopunjenaMekomUcinjenoRot = FindViewById<CheckBox>(Resource.Id.nadopunjenaMekomUcinjenoRot);
            novaLjepPodUcinjenoLjep = FindViewById<CheckBox>(Resource.Id.novaLjepPodUcinjenoLjep);
            novaKutijaUcinjenoLjep = FindViewById<CheckBox>(Resource.Id.novaKutijaUcinjenoLjep);
            novaKutijaUcinjenoZiv = FindViewById<CheckBox>(Resource.Id.novaKutijaUcinjenoZiv);

            SetActionBar(toolbar);
            ActionBar.Title = "Anketa";
            ClearCheckbox();
            tipKutije = localPozicija.GetInt("tipKutije", 0);
            if (tipKutije == 1)
            {
                rotKutijaSRodenticima.Visibility = Android.Views.ViewStates.Visible;
                rotKutijaSRodenticima.Checked = true;
                checkboxRot.Visibility = Android.Views.ViewStates.Visible;
            }
            else if (tipKutije == 2)
            {
                rotKutijasLjepljivimPodloskom.Visibility = Android.Views.ViewStates.Visible;
                rotKutijasLjepljivimPodloskom.Checked = true;
                checkboxLjep.Visibility = Android.Views.ViewStates.Visible;
            }
            else
            {
                rotKutijaZivolovka.Visibility = Android.Views.ViewStates.Visible;
                rotKutijaZivolovka.Checked = true;
                checkboxZiv.Visibility = Android.Views.ViewStates.Visible;
            }

            if (localAnketa.GetBoolean("visitedPage2", false))
            {
                noviMamciUcinjenoRot.Checked = localAnketa.GetBoolean("noviMamciUcinjenoRot", false);
                novaKutijaUcinjenoRot.Checked = localAnketa.GetBoolean("novaKutijaUcinjenoRot", false);
                nadopunjenaMekomUcinjenoRot.Checked = localAnketa.GetBoolean("nadopunjenaMekomUcinjenoRot", false);
                novaLjepPodUcinjenoLjep.Checked = localAnketa.GetBoolean("novaLjepPodUcinjenoLjep", false);
                novaKutijaUcinjenoLjep.Checked = localAnketa.GetBoolean("novaKutijaUcinjenoLjep", false);
                novaKutijaUcinjenoZiv.Checked = localAnketa.GetBoolean("novaKutijaUcinjenoZiv", false);
            }
        }

        public void ClearCheckbox()
        {
            noviMamciUcinjenoRot.Checked = false;
            novaKutijaUcinjenoRot.Checked = false;
            nadopunjenaMekomUcinjenoRot.Checked = false;
            novaLjepPodUcinjenoLjep.Checked = false;
            novaKutijaUcinjenoLjep.Checked = false;
            novaKutijaUcinjenoZiv.Checked = false;
        }

        public void SaveState()
        {
            if (tipKutije == 1)
            {
                localAnketaEdit.PutBoolean("noviMamciUcinjenoRot", noviMamciUcinjenoRot.Checked);
                localAnketaEdit.PutBoolean("novaKutijaUcinjenoRot", novaKutijaUcinjenoRot.Checked);
                localAnketaEdit.PutBoolean("nadopunjenaMekomUcinjenoRot", nadopunjenaMekomUcinjenoRot.Checked);
            }
            else if (tipKutije == 2)
            {
                localAnketaEdit.PutBoolean("novaLjepPodUcinjenoLjep", novaLjepPodUcinjenoLjep.Checked);
                localAnketaEdit.PutBoolean("novaKutijaUcinjenoLjep", novaKutijaUcinjenoLjep.Checked);
            }
            else
                localAnketaEdit.PutBoolean("novaKutijaUcinjenoZiv", novaKutijaUcinjenoZiv.Checked);
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
            switch (item.TitleFormatted.ToString())
            {
                case "Početna":
                    intent = new Intent(this, typeof(Activity_Pocetna));
                    break;
                case "nazad":
                    SaveState();
                    intent = new Intent(this, typeof(Activity_Kontola_page1));
                    break;
                case "naprijed":
                    SaveState();
                    if (localAnketa.GetBoolean("edit", false))
                        intent = new Intent(this, typeof(Activity_Kontola_page3_Edit));
                    else
                        intent = new Intent(this, typeof(Activity_Kontola_page3));

                    break;
            }


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