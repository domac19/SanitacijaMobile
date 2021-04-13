using System;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Infobit.DDD.Data;
using SQLite;
using Android.Content.PM;

namespace App4New
{
    [Activity(Label = "Activity_Potvrda_page2", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_Potvrda_page2 : Activity
    {
        Intent intent;
        CheckBox deratizacijaBtn, dezinsekcijaBtn, dezinfekcijaBtn, dezodorizacijaBtn, zastitaBiljaBtn, suzbijanjeStetnikaBtn, KIInsektiBtn,
            KIGlodavciBtn, uzimanjeBrisevaBtn, suzbijanjeKorovaBtn, kosnjaTraveBtn, tretman2, tretman, postavljanjeLovki, kontrola, postavljanjeMaterijala;
        EditText brObjekataDeratizacija, brObjekataDezinsekcija, brObjekataDezinfekcija, brObjekataZastitaBilja, brObjekataDezodorizacija, brObjekataSuzbijanjeStetnika,
                brObjekataKIInsekti, brObjekataKIGlodavci, brObjekataUzimanjeBriseva, brObjekataSuzbijanjeKorova, brObjekatakosnjaTrave, brObjekataDeratizacija2, brObjekataDezinsekcija2, brObjekataDezinfekcija2, brObjekataZastitaBilja2, brObjekataDezodorizacija2, brObjekataSuzbijanjeStetnika2,
                brObjekataKIInsekti2, brObjekataKIGlodavci2, brObjekataUzimanjeBriseva2, brObjekataSuzbijanjeKorova2, brObjekatakosnjaTrave2;
        LinearLayout brObjekataDeratizacijaLayout, brObjekataDezinsekcijaLayout, brObjekataDezinfekcijaLayout, brObjekataZastitaBiljaLayout, brObjekataDezodorizacijaLayout, brObjekataSuzbijanjeStetnikaLayout,
                brObjekataKIInsektiLayout, brObjekataKIGlodavciLayout, brObjekataUzimanjeBrisevaLayout, brObjekataSuzbijanjeKorovaLayout, brObjekatakosnjaTraveLayout;
        TextView brObjekataDeratizacija2TV, brObjekataDezinsekcija2TV, brObjekataDezinfekcija2TV, brObjekataZastitaBilja2TV, brObjekataDezodorizacija2TV,
                brObjekataSuzbijanjeStetnika2TV, brObjekataKIInsekti2TV, brObjekataKIGlodavci2TV, brObjekataUzimanjeBriseva2TV, brObjekataSuzbijanjeKorova2TV, brObjekatakosnjaTrave2TV;
        DID_Lokacija lokacija;

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
            SetContentView(Resource.Layout.potvrda_page2);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);

            brObjekataDeratizacija2TV = FindViewById<TextView>(Resource.Id.brObjekataDeratizacija2TV);
            brObjekataDezinsekcija2TV = FindViewById<TextView>(Resource.Id.brObjekataDezinsekcija2TV);
            brObjekataDezinfekcija2TV = FindViewById<TextView>(Resource.Id.brObjekataDezinfekcija2TV);
            brObjekataZastitaBilja2TV = FindViewById<TextView>(Resource.Id.brObjekataZastitaBilja2TV);
            brObjekataDezodorizacija2TV = FindViewById<TextView>(Resource.Id.brObjekataDezodorizacija2TV);
            brObjekataSuzbijanjeStetnika2TV = FindViewById<TextView>(Resource.Id.brObjekataSuzbijanjeStetnika2TV);
            brObjekataKIInsekti2TV = FindViewById<TextView>(Resource.Id.brObjekataKIInsekti2TV);
            brObjekataKIGlodavci2TV = FindViewById<TextView>(Resource.Id.brObjekataKIGlodavci2TV);
            brObjekataUzimanjeBriseva2TV = FindViewById<TextView>(Resource.Id.brObjekataUzimanjeBriseva2TV);
            brObjekataSuzbijanjeKorova2TV = FindViewById<TextView>(Resource.Id.brObjekataSuzbijanjeKorova2TV);
            brObjekatakosnjaTrave2TV = FindViewById<TextView>(Resource.Id.brObjekatakosnjaTrave2TV);
            brObjekataDeratizacija = FindViewById<EditText>(Resource.Id.brObjekataDeratizacija);
            brObjekataDezinsekcija = FindViewById<EditText>(Resource.Id.brObjekataDezinsekcija);
            brObjekataDezinfekcija = FindViewById<EditText>(Resource.Id.brObjekataDezinfekcija);
            brObjekataZastitaBilja = FindViewById<EditText>(Resource.Id.brObjekataZastitaBilja);
            brObjekataDezodorizacija = FindViewById<EditText>(Resource.Id.brObjekataDezodorizacija);
            brObjekataSuzbijanjeStetnika = FindViewById<EditText>(Resource.Id.brObjekataSuzbijanjeStetnika);
            brObjekataKIInsekti = FindViewById<EditText>(Resource.Id.brObjekataKIInsekti);
            brObjekataKIGlodavci = FindViewById<EditText>(Resource.Id.brObjekataKIGlodavci);
            brObjekataUzimanjeBriseva = FindViewById<EditText>(Resource.Id.brObjekataUzimanjeBriseva);
            brObjekataSuzbijanjeKorova = FindViewById<EditText>(Resource.Id.brObjekataSuzbijanjeKorova);
            brObjekatakosnjaTrave = FindViewById<EditText>(Resource.Id.brObjekatakosnjaTrave);
            brObjekataDeratizacija2 = FindViewById<EditText>(Resource.Id.brObjekataDeratizacija2);
            brObjekataDezinsekcija2 = FindViewById<EditText>(Resource.Id.brObjekataDezinsekcija2);
            brObjekataDezinfekcija2 = FindViewById<EditText>(Resource.Id.brObjekataDezinfekcija2);
            brObjekataZastitaBilja2 = FindViewById<EditText>(Resource.Id.brObjekataZastitaBilja2);
            brObjekataDezodorizacija2 = FindViewById<EditText>(Resource.Id.brObjekataDezodorizacija2);
            brObjekataSuzbijanjeStetnika2 = FindViewById<EditText>(Resource.Id.brObjekataSuzbijanjeStetnika2);
            brObjekataKIInsekti2 = FindViewById<EditText>(Resource.Id.brObjekataKIInsekti2);
            brObjekataKIGlodavci2 = FindViewById<EditText>(Resource.Id.brObjekataKIGlodavci2);
            brObjekataUzimanjeBriseva2 = FindViewById<EditText>(Resource.Id.brObjekataUzimanjeBriseva2);
            brObjekataSuzbijanjeKorova2 = FindViewById<EditText>(Resource.Id.brObjekataSuzbijanjeKorova2);
            brObjekatakosnjaTrave2 = FindViewById<EditText>(Resource.Id.brObjekatakosnjaTrave2);
            brObjekataDeratizacijaLayout = FindViewById<LinearLayout>(Resource.Id.brObjekataDeratizacijaLayout);
            brObjekataDezinsekcijaLayout = FindViewById<LinearLayout>(Resource.Id.brObjekataDezinsekcijaLayout);
            brObjekataDezinfekcijaLayout = FindViewById<LinearLayout>(Resource.Id.brObjekataDezinfekcijaLayout);
            brObjekataZastitaBiljaLayout = FindViewById<LinearLayout>(Resource.Id.brObjekataZastitaBiljaLayout);
            brObjekataDezodorizacijaLayout = FindViewById<LinearLayout>(Resource.Id.brObjekataDezodorizacijaLayout);
            brObjekataSuzbijanjeStetnikaLayout = FindViewById<LinearLayout>(Resource.Id.brObjekataSuzbijanjeStetnikaLayout);
            brObjekataKIInsektiLayout = FindViewById<LinearLayout>(Resource.Id.brObjekataKIInsektiLayout);
            brObjekataKIGlodavciLayout = FindViewById<LinearLayout>(Resource.Id.brObjekataKIGlodavciLayout);
            brObjekataUzimanjeBrisevaLayout = FindViewById<LinearLayout>(Resource.Id.brObjekataUzimanjeBrisevaLayout);
            brObjekataSuzbijanjeKorovaLayout = FindViewById<LinearLayout>(Resource.Id.brObjekataSuzbijanjeKorovaLayout);
            brObjekatakosnjaTraveLayout = FindViewById<LinearLayout>(Resource.Id.brObjekatakosnjaTraveLayout);
            deratizacijaBtn = FindViewById<CheckBox>(Resource.Id.deratizacijaBtn);
            dezinsekcijaBtn = FindViewById<CheckBox>(Resource.Id.dezinsekcijaBtn);
            dezinfekcijaBtn = FindViewById<CheckBox>(Resource.Id.dezinfekcijaBtn);
            zastitaBiljaBtn = FindViewById<CheckBox>(Resource.Id.zastitaBiljaBtn);
            dezodorizacijaBtn = FindViewById<CheckBox>(Resource.Id.dezodorizacijaBtn);
            suzbijanjeStetnikaBtn = FindViewById<CheckBox>(Resource.Id.suzbijanjeStetnikaBtn);
            KIInsektiBtn = FindViewById<CheckBox>(Resource.Id.KIInsektiBtn);
            KIGlodavciBtn = FindViewById<CheckBox>(Resource.Id.KIGlodavciBtn);
            uzimanjeBrisevaBtn = FindViewById<CheckBox>(Resource.Id.uzimanjeBrisevaBtn);
            suzbijanjeKorovaBtn = FindViewById<CheckBox>(Resource.Id.suzbijanjeKorovaBtn);
            kosnjaTraveBtn = FindViewById<CheckBox>(Resource.Id.kosnjaTraveBtn);
            tretman2 = FindViewById<CheckBox>(Resource.Id.tretman2);
            tretman = FindViewById<CheckBox>(Resource.Id.tretman);
            postavljanjeLovki = FindViewById<CheckBox>(Resource.Id.postavljanjeLovki);
            kontrola = FindViewById<CheckBox>(Resource.Id.kontrola);
            postavljanjeMaterijala = FindViewById<CheckBox>(Resource.Id.postavljanjeMaterijala);

            SetActionBar(toolbar);
            ActionBar.Title = "Potvrda - djelatnost";
            deratizacijaBtn.Click += DeratizacijaBtn_Click;
            dezinsekcijaBtn.Click += DezinsekcijaBtn_Click;
            dezinfekcijaBtn.Click += DezinfekcijaBtn_Click;
            zastitaBiljaBtn.Click += ZastitaBiljaBtn_Click;
            dezodorizacijaBtn.Click += DezodorizacijaBtn_Click;
            suzbijanjeStetnikaBtn.Click += SuzbijanjeStetnikaBtn_Click;
            KIInsektiBtn.Click += KIInsektiBtn_Click;
            KIGlodavciBtn.Click += KIGlodavciBtn_Click;
            uzimanjeBrisevaBtn.Click += UzimanjeBrisevaBtn_Click;
            suzbijanjeKorovaBtn.Click += SuzbijanjeKorovaBtn_Click;
            kosnjaTraveBtn.Click += KosnjaTraveBtn_Click;

            int lokacijaId = localKomitentLokacija.GetInt("lokacijaId", 0);
            lokacija = db.Query<DID_Lokacija>(
                "SELECT * " +
                "FROM DID_Lokacija " +
                "WHERE SAN_Id = ?", lokacijaId).FirstOrDefault();

            if(lokacija.SAN_Tip2 == 0)
            {
                brObjekataDeratizacija2TV.Visibility = Android.Views.ViewStates.Gone;
                brObjekataDezinsekcija2TV.Visibility = Android.Views.ViewStates.Gone;
                brObjekataDezinfekcija2TV.Visibility = Android.Views.ViewStates.Gone;
                brObjekataZastitaBilja2TV.Visibility = Android.Views.ViewStates.Gone;
                brObjekataDezodorizacija2TV.Visibility = Android.Views.ViewStates.Gone;
                brObjekataSuzbijanjeStetnika2TV.Visibility = Android.Views.ViewStates.Gone;
                brObjekataKIInsekti2TV.Visibility = Android.Views.ViewStates.Gone;
                brObjekataKIGlodavci2TV.Visibility = Android.Views.ViewStates.Gone;
                brObjekataUzimanjeBriseva2TV.Visibility = Android.Views.ViewStates.Gone;
                brObjekataSuzbijanjeKorova2TV.Visibility = Android.Views.ViewStates.Gone;
                brObjekatakosnjaTrave2TV.Visibility = Android.Views.ViewStates.Gone;
                brObjekataDeratizacija2.Visibility = Android.Views.ViewStates.Gone;
                brObjekataDezinsekcija2.Visibility = Android.Views.ViewStates.Gone;
                brObjekataDezinfekcija2.Visibility = Android.Views.ViewStates.Gone;
                brObjekataZastitaBilja2.Visibility = Android.Views.ViewStates.Gone;
                brObjekataDezodorizacija2.Visibility = Android.Views.ViewStates.Gone;
                brObjekataSuzbijanjeStetnika2.Visibility = Android.Views.ViewStates.Gone;
                brObjekataKIInsekti2.Visibility = Android.Views.ViewStates.Gone;
                brObjekataKIGlodavci2.Visibility = Android.Views.ViewStates.Gone;
                brObjekataUzimanjeBriseva2.Visibility = Android.Views.ViewStates.Gone;
                brObjekataSuzbijanjeKorova2.Visibility = Android.Views.ViewStates.Gone;
                brObjekatakosnjaTrave2.Visibility = Android.Views.ViewStates.Gone;
            }


            if (localPotvrda.GetBoolean("potvrdaPage2", false))
            {
                if (localPotvrda.GetBoolean("deratizacijaBtn", false))
                    deratizacijaBtn.PerformClick();
                if (localPotvrda.GetBoolean("dezinsekcijaBtn", false))
                    dezinsekcijaBtn.PerformClick();
                if (localPotvrda.GetBoolean("dezinfekcijaBtn", false))
                    dezinfekcijaBtn.PerformClick();
                if (localPotvrda.GetBoolean("dezodorizacijaBtn", false))
                    dezodorizacijaBtn.PerformClick();
                if (localPotvrda.GetBoolean("zastitaBiljaBtn", false))
                    zastitaBiljaBtn.PerformClick();
                if (localPotvrda.GetBoolean("suzbijanjeStetnikaBtn", false))
                    suzbijanjeStetnikaBtn.PerformClick();
                if (localPotvrda.GetBoolean("KIInsektiBtn", false))
                    KIInsektiBtn.PerformClick();
                if (localPotvrda.GetBoolean("KIGlodavciBtn", false))
                    KIGlodavciBtn.PerformClick();
                if (localPotvrda.GetBoolean("uzimanjeBrisevaBtn", false))
                    uzimanjeBrisevaBtn.PerformClick();
                if (localPotvrda.GetBoolean("suzbijanjeKorovaBtn", false))
                    suzbijanjeKorovaBtn.PerformClick();
                if (localPotvrda.GetBoolean("kosnjaTraveBtn", false))
                    kosnjaTraveBtn.PerformClick();
                if (localPotvrda.GetBoolean("postavljanjeMaterijala", false))
                    postavljanjeMaterijala.PerformClick();
                if (localPotvrda.GetBoolean("kontrola", false))
                    kontrola.PerformClick();
                if (localPotvrda.GetBoolean("postavljanjeLovki", false))
                    postavljanjeLovki.PerformClick();
                if (localPotvrda.GetBoolean("tretman", false))
                    tretman.PerformClick();
                if (localPotvrda.GetBoolean("tretman2", false))
                    tretman2.PerformClick();

                brObjekataDeratizacija.Text = localPotvrda.GetString("brObjekataDeratizacija", null);
                brObjekataDezinsekcija.Text = localPotvrda.GetString("brObjekataDezinsekcija", null);
                brObjekataDezinfekcija.Text = localPotvrda.GetString("brObjekataDezinfekcija", null);
                brObjekataZastitaBilja.Text = localPotvrda.GetString("brObjekataZastitaBilja", null);
                brObjekataDezodorizacija.Text = localPotvrda.GetString("brObjekataDezodorizacija", null);
                brObjekataSuzbijanjeStetnika.Text = localPotvrda.GetString("brObjekataSuzbijanjeStetnika", null);
                brObjekataKIInsekti.Text = localPotvrda.GetString("brObjekataKIInsekti", null);
                brObjekataKIGlodavci.Text = localPotvrda.GetString("brObjekataKIGlodavci", null);
                brObjekataUzimanjeBriseva.Text = localPotvrda.GetString("brObjekataUzimanjeBriseva", null);
                brObjekataSuzbijanjeKorova.Text = localPotvrda.GetString("brObjekataSuzbijanjeKorova", null);
                brObjekatakosnjaTrave.Text = localPotvrda.GetString("brObjekatakosnjaTrave", null);
                brObjekataDeratizacija2.Text = localPotvrda.GetString("brObjekataDeratizacija2", null);
                brObjekataDezinsekcija2.Text = localPotvrda.GetString("brObjekataDezinsekcija2", null);
                brObjekataDezinfekcija2.Text = localPotvrda.GetString("brObjekataDezinfekcija2", null);
                brObjekataZastitaBilja2.Text = localPotvrda.GetString("brObjekataZastitaBilja2", null);
                brObjekataDezodorizacija2.Text = localPotvrda.GetString("brObjekataDezodorizacija2", null);
                brObjekataSuzbijanjeStetnika2.Text = localPotvrda.GetString("brObjekataSuzbijanjeStetnika2", null);
                brObjekataKIInsekti2.Text = localPotvrda.GetString("brObjekataKIInsekti2", null);
                brObjekataKIGlodavci2.Text = localPotvrda.GetString("brObjekataKIGlodavci2", null);
                brObjekataUzimanjeBriseva2.Text = localPotvrda.GetString("brObjekataUzimanjeBriseva2", null);
                brObjekataSuzbijanjeKorova2.Text = localPotvrda.GetString("brObjekataSuzbijanjeKorova2", null);
                brObjekatakosnjaTrave2.Text = localPotvrda.GetString("brObjekatakosnjaTrave2", null);
            }
        }

        public void DezodorizacijaBtn_Click(object sender, EventArgs args)
        {
            if (dezodorizacijaBtn.Checked)
                brObjekataDezodorizacijaLayout.Visibility = Android.Views.ViewStates.Visible;
            else
                brObjekataDezodorizacijaLayout.Visibility = Android.Views.ViewStates.Gone;
        }
        public void SuzbijanjeStetnikaBtn_Click(object sender, EventArgs args)
        {
            if (suzbijanjeStetnikaBtn.Checked)
                brObjekataSuzbijanjeStetnikaLayout.Visibility = Android.Views.ViewStates.Visible;
            else
                brObjekataSuzbijanjeStetnikaLayout.Visibility = Android.Views.ViewStates.Gone;
        }
        public void KIInsektiBtn_Click(object sender, EventArgs args)
        {
            if (KIInsektiBtn.Checked)
                brObjekataKIInsektiLayout.Visibility = Android.Views.ViewStates.Visible;
            else
                brObjekataKIInsektiLayout.Visibility = Android.Views.ViewStates.Invisible;
        }
        public void KIGlodavciBtn_Click(object sender, EventArgs args)
        {
            if (KIGlodavciBtn.Checked)
                brObjekataKIGlodavciLayout.Visibility = Android.Views.ViewStates.Visible;
            else
                brObjekataKIGlodavciLayout.Visibility = Android.Views.ViewStates.Invisible;
        }
        public void UzimanjeBrisevaBtn_Click(object sender, EventArgs args)
        {
            if (uzimanjeBrisevaBtn.Checked)
                brObjekataUzimanjeBriseva.Visibility = Android.Views.ViewStates.Visible;
            else
                brObjekataUzimanjeBriseva.Visibility = Android.Views.ViewStates.Invisible;
        }
        public void SuzbijanjeKorovaBtn_Click(object sender, EventArgs args)
        {
            if (suzbijanjeKorovaBtn.Checked)
                brObjekataSuzbijanjeKorova.Visibility = Android.Views.ViewStates.Visible;
            else
                brObjekataSuzbijanjeKorova.Visibility = Android.Views.ViewStates.Invisible;
        }
        public void KosnjaTraveBtn_Click(object sender, EventArgs args)
        {
            if (kosnjaTraveBtn.Checked)
                brObjekatakosnjaTraveLayout.Visibility = Android.Views.ViewStates.Visible;
            else
                brObjekatakosnjaTraveLayout.Visibility = Android.Views.ViewStates.Invisible;
        }

        public void ZastitaBiljaBtn_Click(object sender, EventArgs args)
        {
            if (zastitaBiljaBtn.Checked)
            {
                tretman2.Visibility = Android.Views.ViewStates.Visible;
                brObjekataZastitaBiljaLayout.Visibility = Android.Views.ViewStates.Visible;
            }
            else
            {
                brObjekataZastitaBiljaLayout.Visibility = Android.Views.ViewStates.Invisible;
                tretman2.Visibility = Android.Views.ViewStates.Invisible;
                tretman2.Checked = false;
            }
        }

        public void DezinfekcijaBtn_Click(object sender, EventArgs args)
        {
            if (dezinfekcijaBtn.Checked)
            {
                tretman.Visibility = Android.Views.ViewStates.Visible;
                brObjekataDezinfekcijaLayout.Visibility = Android.Views.ViewStates.Visible;
            }
            else
            {
                brObjekataDezinfekcijaLayout.Visibility = Android.Views.ViewStates.Invisible;
                tretman.Visibility = Android.Views.ViewStates.Invisible;
                tretman.Checked = false;
            }
        }

        public void DezinsekcijaBtn_Click(object sender, EventArgs args)
        {
            if (dezinsekcijaBtn.Checked)
            {
                postavljanjeLovki.Visibility = Android.Views.ViewStates.Visible;
                brObjekataDezinsekcijaLayout.Visibility = Android.Views.ViewStates.Visible;
            }
            else
            {               
                brObjekataDezinsekcijaLayout.Visibility = Android.Views.ViewStates.Invisible;
                postavljanjeLovki.Visibility = Android.Views.ViewStates.Invisible;
                dezinsekcijaBtn.Checked = false;
            }
        }

        public void DeratizacijaBtn_Click(object sender, EventArgs args)
        {
            if (deratizacijaBtn.Checked)
            {

                postavljanjeMaterijala.Visibility = Android.Views.ViewStates.Visible;
                kontrola.Visibility = Android.Views.ViewStates.Visible;
                brObjekataDeratizacijaLayout.Visibility = Android.Views.ViewStates.Visible;
            }
            else
            {
                brObjekataDeratizacijaLayout.Visibility = Android.Views.ViewStates.Invisible;
                postavljanjeMaterijala.Visibility = Android.Views.ViewStates.Invisible;
                kontrola.Visibility = Android.Views.ViewStates.Invisible;
                postavljanjeMaterijala.Checked = false;
                kontrola.Checked = false;
            }
        }

        public void SaveState()
        {
            localPotvrdaEdit.PutString("brObjekataDeratizacija2", brObjekataDeratizacija2.Text);
            localPotvrdaEdit.PutString("brObjekataDezinsekcija2", brObjekataDezinsekcija2.Text);
            localPotvrdaEdit.PutString("brObjekataDezinfekcija2", brObjekataDezinfekcija2.Text);
            localPotvrdaEdit.PutString("brObjekataZastitaBilja2", brObjekataZastitaBilja2.Text);
            localPotvrdaEdit.PutString("brObjekataDezodorizacija2", brObjekataDezodorizacija2.Text);
            localPotvrdaEdit.PutString("brObjekataSuzbijanjeStetnika2", brObjekataSuzbijanjeStetnika2.Text);
            localPotvrdaEdit.PutString("brObjekataKIInsekti2", brObjekataKIInsekti2.Text);
            localPotvrdaEdit.PutString("brObjekataKIGlodavci2", brObjekataKIGlodavci2.Text);
            localPotvrdaEdit.PutString("brObjekataUzimanjeBriseva2", brObjekataUzimanjeBriseva2.Text);
            localPotvrdaEdit.PutString("brObjekataSuzbijanjeKorova2", brObjekataSuzbijanjeKorova2.Text);
            localPotvrdaEdit.PutString("brObjekatakosnjaTrave2", brObjekatakosnjaTrave2.Text);
            localPotvrdaEdit.PutString("brObjekataDeratizacija", brObjekataDeratizacija.Text);
            localPotvrdaEdit.PutString("brObjekataDezinsekcija", brObjekataDezinsekcija.Text);
            localPotvrdaEdit.PutString("brObjekataDezinfekcija", brObjekataDezinfekcija.Text);
            localPotvrdaEdit.PutString("brObjekataZastitaBilja", brObjekataZastitaBilja.Text);
            localPotvrdaEdit.PutString("brObjekataDezodorizacija", brObjekataDezodorizacija.Text);
            localPotvrdaEdit.PutString("brObjekataSuzbijanjeStetnika", brObjekataSuzbijanjeStetnika.Text);
            localPotvrdaEdit.PutString("brObjekataKIInsekti", brObjekataKIInsekti.Text);
            localPotvrdaEdit.PutString("brObjekataKIGlodavci", brObjekataKIGlodavci.Text);
            localPotvrdaEdit.PutString("brObjekataUzimanjeBriseva", brObjekataUzimanjeBriseva.Text);
            localPotvrdaEdit.PutString("brObjekataSuzbijanjeKorova", brObjekataSuzbijanjeKorova.Text);
            localPotvrdaEdit.PutString("brObjekatakosnjaTrave", brObjekatakosnjaTrave.Text);
            localPotvrdaEdit.PutBoolean("deratizacijaBtn", deratizacijaBtn.Checked);
            localPotvrdaEdit.PutBoolean("dezinsekcijaBtn", dezinsekcijaBtn.Checked);
            localPotvrdaEdit.PutBoolean("dezinfekcijaBtn", dezinfekcijaBtn.Checked);
            localPotvrdaEdit.PutBoolean("dezodorizacijaBtn", dezodorizacijaBtn.Checked);
            localPotvrdaEdit.PutBoolean("zastitaBiljaBtn", zastitaBiljaBtn.Checked);
            localPotvrdaEdit.PutBoolean("suzbijanjeStetnikaBtn", suzbijanjeStetnikaBtn.Checked);
            localPotvrdaEdit.PutBoolean("KIInsektiBtn", KIInsektiBtn.Checked);
            localPotvrdaEdit.PutBoolean("KIGlodavciBtn", KIGlodavciBtn.Checked);
            localPotvrdaEdit.PutBoolean("uzimanjeBrisevaBtn", uzimanjeBrisevaBtn.Checked);
            localPotvrdaEdit.PutBoolean("suzbijanjeKorovaBtn", suzbijanjeKorovaBtn.Checked);
            localPotvrdaEdit.PutBoolean("kosnjaTraveBtn", kosnjaTraveBtn.Checked);
            localPotvrdaEdit.PutBoolean("tretman2", tretman2.Checked);
            localPotvrdaEdit.PutBoolean("tretman", tretman.Checked);
            localPotvrdaEdit.PutBoolean("postavljanjeLovki", postavljanjeLovki.Checked);
            localPotvrdaEdit.PutBoolean("kontrola", kontrola.Checked);
            localPotvrdaEdit.PutBoolean("postavljanjeMaterijala", postavljanjeMaterijala.Checked);
            localPotvrdaEdit.PutBoolean("potvrdaPage2", true);
            localPotvrdaEdit.Commit();
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
                intent = new Intent(this, typeof(Activity_Potvrda_page1));
            }
            else if (item.TitleFormatted.ToString() == "naprijed")
            {
                SaveState();
                intent = new Intent(this, typeof(Activity_Potvrda_page3));
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