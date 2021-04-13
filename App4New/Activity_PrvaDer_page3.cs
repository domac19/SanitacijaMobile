using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Content.PM;

namespace App4New
{
    [Activity(Label = "Activity_PrvaDer_page3", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_PrvaDer_page3 : Activity
    {
        RadioButton kutijaSLjepljivimPodloskama, kutijaSRodenticima, kutijaZivolovke;
        CheckBox noviMamciUcinjenoRot, novaKutijaUcinjenoRot, nadopunjenaMekomUcinjenoRot, novaLjepPodUcinjenoLjep, novaKutijaUcinjenoLjep, novaKutijaUcinjenoZiv;
        LinearLayout checkboxRot, checkboxLjep, checkboxZiv;
        Intent intent;

        static readonly ISharedPreferences localAnketa = Application.Context.GetSharedPreferences("anketa", FileCreationMode.Private);
        static ISharedPreferencesEditor localAnketaEdit = localAnketa.Edit();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.prvaDer_page3);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            kutijaSLjepljivimPodloskama = FindViewById<RadioButton>(Resource.Id.kutijaSLjepljivimPodloskama);
            kutijaSRodenticima = FindViewById<RadioButton>(Resource.Id.kutijaSRodenticima);
            kutijaZivolovke = FindViewById<RadioButton>(Resource.Id.kutijaZivolovke);
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
            kutijaSLjepljivimPodloskama.Click += KutijaSLjepljivimPodloskama_Click;
            kutijaSRodenticima.Click += KutijaSRodenticima_Click;
            kutijaZivolovke.Click += KutijaZivolovke_Click;
            
            if (localAnketa.GetBoolean("visitedPage3", false))
            {
                if (localAnketa.GetBoolean("ANK_Kutija_Ljep", false))
                {
                    kutijaSLjepljivimPodloskama.PerformClick();
                    novaLjepPodUcinjenoLjep.Checked = localAnketa.GetBoolean("novaLjepPodUcinjenoLjep", false);
                    novaKutijaUcinjenoLjep.Checked = localAnketa.GetBoolean("novaKutijaUcinjenoLjep", false);
                }
                else if (localAnketa.GetBoolean("ANK_Kutija_Rot", false))
                {
                    kutijaSRodenticima.PerformClick();
                    noviMamciUcinjenoRot.Checked = localAnketa.GetBoolean("noviMamciUcinjenoRot", false);
                    novaKutijaUcinjenoRot.Checked = localAnketa.GetBoolean("novaKutijaUcinjenoRot", false);
                    nadopunjenaMekomUcinjenoRot.Checked = localAnketa.GetBoolean("nadopunjenaMekomUcinjenoRot", false);
                }
                else if (localAnketa.GetBoolean("ANK_Kutija_Ziv", false))
                {
                    kutijaZivolovke.PerformClick();
                    novaKutijaUcinjenoZiv.Checked = localAnketa.GetBoolean("novaKutijaUcinjenoZiv", false);
                }
            }
        }

        public void KutijaSLjepljivimPodloskama_Click(object sender, EventArgs e)
        {
            checkboxLjep.Visibility = Android.Views.ViewStates.Visible;
            checkboxRot.Visibility = Android.Views.ViewStates.Gone;
            checkboxZiv.Visibility = Android.Views.ViewStates.Gone;
            kutijaSLjepljivimPodloskama.Checked = true;
            ClearCheckBox();
        }

        public void KutijaSRodenticima_Click(object sender, EventArgs e)
        {
            checkboxLjep.Visibility = Android.Views.ViewStates.Gone;
            checkboxRot.Visibility = Android.Views.ViewStates.Visible;
            checkboxZiv.Visibility = Android.Views.ViewStates.Gone;
            kutijaSRodenticima.Checked = true;
            ClearCheckBox();
        }

        public void KutijaZivolovke_Click(object sender, EventArgs e)
        {
            checkboxLjep.Visibility = Android.Views.ViewStates.Gone;
            checkboxRot.Visibility = Android.Views.ViewStates.Gone;
            checkboxZiv.Visibility = Android.Views.ViewStates.Visible;
            kutijaZivolovke.Checked = true;
            ClearCheckBox();
        }

        public void ClearCheckBox()
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
            localAnketaEdit.PutBoolean("ANK_Kutija_Ljep", false);
            localAnketaEdit.PutBoolean("novaLjepPodUcinjenoLjep", false);
            localAnketaEdit.PutBoolean("novaKutijaUcinjenoLjep", false);
            localAnketaEdit.PutBoolean("ANK_Kutija_Rot", false);
            localAnketaEdit.PutBoolean("noviMamciUcinjenoRot", false);
            localAnketaEdit.PutBoolean("novaKutijaUcinjenoRot", false);
            localAnketaEdit.PutBoolean("nadopunjenaMekomUcinjenoRot", false);
            localAnketaEdit.PutBoolean("ANK_Kutija_Ziv", false);
            localAnketaEdit.PutBoolean("novaKutijaUcinjenoZiv", false);
            if (kutijaSLjepljivimPodloskama.Checked)
            {
                localAnketaEdit.PutBoolean("ANK_Kutija_Ljep", true);
                localAnketaEdit.PutBoolean("novaLjepPodUcinjenoLjep", novaLjepPodUcinjenoLjep.Checked);
                localAnketaEdit.PutBoolean("novaKutijaUcinjenoLjep", novaKutijaUcinjenoLjep.Checked);
            }
            else if (kutijaSRodenticima.Checked)
            {
                localAnketaEdit.PutBoolean("ANK_Kutija_Rot", true);
                localAnketaEdit.PutBoolean("noviMamciUcinjenoRot", noviMamciUcinjenoRot.Checked);
                localAnketaEdit.PutBoolean("novaKutijaUcinjenoRot", novaKutijaUcinjenoRot.Checked);
                localAnketaEdit.PutBoolean("nadopunjenaMekomUcinjenoRot", nadopunjenaMekomUcinjenoRot.Checked);
            }
            else
            {
                localAnketaEdit.PutBoolean("ANK_Kutija_Ziv", true);
                localAnketaEdit.PutBoolean("novaKutijaUcinjenoZiv", novaKutijaUcinjenoZiv.Checked);
            }
            localAnketaEdit.PutBoolean("visitedPage3", true);
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
                intent = new Intent(this, typeof(Activity_PrvaDer_page2));
            else if (item.TitleFormatted.ToString() == "naprijed")
            {
                SaveState();
                if (localAnketa.GetBoolean("edit", false))
                {
                    intent = new Intent(this, typeof(Activity_PrvaDer_page4_Edit));
                    StartActivity(intent);
                }
                else
                {
                    intent = new Intent(this, typeof(Activity_PrvaDer_page4));
                    StartActivity(intent);
                }
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