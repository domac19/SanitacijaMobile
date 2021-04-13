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
    [Activity(Label = "Activity_Potvrda_page1", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_Potvrda_page1 : SelectedCulture
    {
        EditText opisRadaInput, godinaInput, potvrdaBrInput, brojObjekata1Input, brojObjekata2Input;
        Spinner spinnerInfestacija;
        Intent intent;
        TextView message, messageDate;
        DateTime odabraniDatum;
        DatePicker datePicker;
        LinearLayout tipObjekta1, tipObjekta2;
        DID_Lokacija lokacija;
        bool flag, flagInput;

        static readonly ISharedPreferences localPotvrda = Application.Context.GetSharedPreferences("potvrda", FileCreationMode.Private);
        static ISharedPreferencesEditor localPotvrdaEdit = localPotvrda.Edit();
        static readonly ISharedPreferences localKomitentLokacija = Application.Context.GetSharedPreferences("localKomitentLokacija", FileCreationMode.Private);

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.potvrda_page1);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            opisRadaInput = FindViewById<EditText>(Resource.Id.opisRadaInput);
            godinaInput = FindViewById<EditText>(Resource.Id.godinaInput);
            potvrdaBrInput = FindViewById<EditText>(Resource.Id.potvrdaBrInput);
            opisRadaInput = FindViewById<EditText>(Resource.Id.opisRadaInput);
            spinnerInfestacija = FindViewById<Spinner>(Resource.Id.spinnerInfestacija);
            message = FindViewById<TextView>(Resource.Id.message);
            datePicker = FindViewById<DatePicker>(Resource.Id.datePicker);
            brojObjekata1Input = FindViewById<EditText>(Resource.Id.brojObjekata1Input);
            brojObjekata2Input = FindViewById<EditText>(Resource.Id.brojObjekata2Input);
            tipObjekta1 = FindViewById<LinearLayout>(Resource.Id.tipObjekta1);
            tipObjekta2 = FindViewById<LinearLayout>(Resource.Id.tipObjekta2);

            SetActionBar(toolbar);
            ActionBar.Title = "Potvrda";
            odabraniDatum = DateTime.Now.Date;
            datePicker.UpdateDate(odabraniDatum.Year, odabraniDatum.Month - 1, odabraniDatum.Day);
            godinaInput.Text = "2021";
            brojObjekata1Input.Text = "1";
            brojObjekata2Input.Text = "1";
            potvrdaBrInput.TextChanged += PotvrdaBroj_TextChanged;
            godinaInput.TextChanged += PotvrdaGodina_TextChanged;
            godinaInput.KeyPress += GodinaInput_KeyPress;
            potvrdaBrInput.KeyPress += PotvrdaBrInput_KeyPress;
            int lokacijaId = localKomitentLokacija.GetInt("lokacijaId", 0);
            int radniNalogLokacijaId = localKomitentLokacija.GetInt("radniNalogLokacijaId", 0);

            lokacija = db.Query<DID_Lokacija>(
                "SELECT * " +
                "FROM DID_Lokacija " +
                "WHERE SAN_Id = ?", lokacijaId).FirstOrDefault();

            if(lokacija.SAN_Tip2 == 0)
                tipObjekta2.Visibility = Android.Views.ViewStates.Invisible;

            List<DID_RazinaInfestacije> infestacije = db.Query<DID_RazinaInfestacije>("SELECT * FROM DID_RazinaInfestacije");
            List<string> listInfestacija = new List<string>();
            foreach (var item in infestacije)
                listInfestacija.Add(item.Naziv);
            ArrayAdapter<string> adapterList = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, listInfestacija);
            adapterList.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinnerInfestacija.Adapter = adapterList;
            spinnerInfestacija.SetSelection(listInfestacija.IndexOf("Slaba"));

            if (localPotvrda.GetBoolean("potvrdaPage1", false))
            {
                godinaInput.Text = localPotvrda.GetString("godina", null);
                DateTime date = Convert.ToDateTime(localPotvrda.GetString("datumPotvrde", null));
                datePicker.UpdateDate(date.Year, date.Month - 1, date.Day);
                opisRadaInput.Text = localPotvrda.GetString("opisRadaInput", null);
                potvrdaBrInput.Text = localPotvrda.GetString("potvrdaBrInput", null);
                spinnerInfestacija.SetSelection(listInfestacija.IndexOf(localPotvrda.GetString("razinaInfestacije", null)));
                brojObjekata1Input.Text = localPotvrda.GetInt("lokacijaTipBrojObjekata", 0).ToString();
                brojObjekata2Input.Text = localPotvrda.GetInt("lokacijaTip2BrojObjekata", 0).ToString();
            }
        }

        public void GodinaInput_KeyPress(object sender, View.KeyEventArgs e)
        {
            e.Handled = false;
            if (e.KeyCode == Keycode.Enter)
            {
                potvrdaBrInput.RequestFocus();
                e.Handled = true;
            }
        }

        public void PotvrdaBrInput_KeyPress(object sender, View.KeyEventArgs e)
        {
            e.Handled = false;
            if (e.KeyCode == Keycode.Enter)
            {
                potvrdaBrInput.RequestFocus();
                e.Handled = true;
            }
        }

        public void PotvrdaBroj_TextChanged(object sender, EventArgs e)
        {
            flag = false;
            if (string.IsNullOrEmpty(potvrdaBrInput.Text))
            {
                message.Text = "Unesite broj potvrde*";
                message.Visibility = Android.Views.ViewStates.Visible;
                flagInput = false;
            }
            else if(potvrdaBrInput.Text.Length > 10)
            {
                message.Text = "Broj potvrde može sadržavati najviše 10 znakova*";
                message.Visibility = Android.Views.ViewStates.Visible;
                flagInput = false;
            }
            else
            {
                message.Visibility = Android.Views.ViewStates.Invisible;
                flagInput = true;
            }
        }

        public void VrijemeInput_TextChanged(object sender, EventArgs e)
        {
            flag = false;
            messageDate.Visibility = Android.Views.ViewStates.Invisible;
        }

        public void PotvrdaGodina_TextChanged(object sender, EventArgs e)
        {
            flag = false;
            if (string.IsNullOrEmpty(godinaInput.Text))
            {
                message.Text = "Unesite godinu*";
                message.Visibility = Android.Views.ViewStates.Visible;
                flagInput = false;
            }
            else if (godinaInput.Text.Length > 4)
            {
                message.Text = "Unesite isprvnu godinu*";
                message.Visibility = Android.Views.ViewStates.Visible;
                flagInput = false;
            }
            else
            {
                message.Visibility = Android.Views.ViewStates.Invisible;
                flagInput = true;
            }
        }

        public void SaveState()
        {
            if(lokacija.SAN_Tip2 == 0)
            {
                localPotvrdaEdit.PutInt("lokacijaTip2", 0);
                localPotvrdaEdit.PutInt("lokacijaTip2BrojObjekata", 0);
                localPotvrdaEdit.PutBoolean("lokacijaTip2Odraden", false);
            }
            else
            {
                localPotvrdaEdit.PutInt("lokacijaTip2", lokacija.SAN_Tip2);
                localPotvrdaEdit.PutInt("lokacijaTip2BrojObjekata", Convert.ToInt32(brojObjekata2Input.Text));
                localPotvrdaEdit.PutBoolean("lokacijaTip2Odraden", true);
            }

            localPotvrdaEdit.PutInt("lokacijaTip1", lokacija.SAN_Tip);
            localPotvrdaEdit.PutInt("lokacijaTipBrojObjekata", Convert.ToInt32(brojObjekata1Input.Text));
            localPotvrdaEdit.PutBoolean("lokacijaTip1Odraden", true);
            localPotvrdaEdit.PutString("godina", godinaInput.Text);
            localPotvrdaEdit.PutString("datumPotvrde", datePicker.DateTime.ToString());
            localPotvrdaEdit.PutString("potvrdaBrInput", potvrdaBrInput.Text);
            localPotvrdaEdit.PutString("opisRadaInput", opisRadaInput.Text);
            localPotvrdaEdit.PutString("razinaInfestacije", spinnerInfestacija.SelectedItem.ToString());
            localPotvrdaEdit.PutBoolean("potvrdaPage1", true);
            localPotvrdaEdit.Commit();
        }

        public void CheckPotvrda()
        {
            if (flagInput)
            {
                List<DID_Potvrda> potvrde = db.Query<DID_Potvrda>(
                    "SELECT * " +
                    "FROM DID_Potvrda " +
                    "WHERE Broj = ? " +
                    "AND Godina = ?", potvrdaBrInput.Text, godinaInput.Text);

                if (localPotvrda.GetBoolean("edit", false))
                {
                    string potvrdaBroj = localPotvrda.GetString("potvrdaBrInput", null);
                    if (potvrdaBroj == potvrdaBrInput.Text || !potvrde.Any())
                    {
                        message.Visibility = Android.Views.ViewStates.Invisible;
                        flag = true;
                    }
                    else
                    {
                        message.Text = "Unesite ispravni broj potvrde*";
                        message.Visibility = Android.Views.ViewStates.Visible;
                        flag = false;
                    }
                }
                else
                {
                    if (potvrde.Any())
                    {
                        message.Text = "Kombinacija godine i broja godine već postoje*";
                        message.Visibility = Android.Views.ViewStates.Visible;
                        flag = false;
                    }
                    else
                    {
                        message.Visibility = Android.Views.ViewStates.Invisible;
                        flag = true;
                    }
                }
            }
            else
            {
                message.Visibility = Android.Views.ViewStates.Visible;
                flag = false;
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menuBackNextHome, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.TitleFormatted.ToString() == "Početna")
            {
                intent = new Intent(this, typeof(Activity_Pocetna));
                StartActivity(intent);
            }
            else if (item.TitleFormatted.ToString() == "nazad")
            {
                if (localPotvrda.GetBoolean("fromList", false) || localPotvrda.GetBoolean("edit", false))
                    intent = new Intent(this, typeof(Activity_Lokacije));
                else
                    intent = new Intent(this, typeof(Activity_PotroseniMaterijali_List));

                localPotvrda.Edit().Clear().Commit();
                StartActivity(intent);
            }
            else if (item.TitleFormatted.ToString() == "naprijed")
            {
                CheckPotvrda();
                if (flag)
                {
                    SaveState();
                    intent = new Intent(this, typeof(Activity_Potvrda_page2));
                    StartActivity(intent);
                }
            }
            else if (item.TitleFormatted.ToString() == "Radni nalozi")
            {
                intent = new Intent(this, typeof(Activity_RadniNalozi));
                StartActivity(intent);
            }
            else if (item.TitleFormatted.ToString() == "Prikaz odradenih anketa")
            {
                intent = new Intent(this, typeof(Activity_OdradeneAnkete));
                StartActivity(intent);
            }
            else if (item.TitleFormatted.ToString() == "Prikaz potrošenih materijala")
            {
                intent = new Intent(this, typeof(Activity_PotroseniMaterijali));
                StartActivity(intent);
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}