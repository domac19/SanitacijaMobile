using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Infobit.DDD.Data;
using SQLite;
using System.IO;
using System.Linq;
using Android.Content.PM;

namespace App4New
{
    [Activity(Label = "Activity_Profile", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_Profile : Activity
    {
        Intent intent;
        TextView adresa, oib, email, phone, username;
        KE_DJELATNICI user;
        string sifraDjelatnika;

        static readonly ISharedPreferences localUsername = Application.Context.GetSharedPreferences("userLogin", FileCreationMode.Private);

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.profile);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            adresa = FindViewById<TextView>(Resource.Id.adresa);
            oib = FindViewById<TextView>(Resource.Id.oib);
            email = FindViewById<TextView>(Resource.Id.email);
            phone = FindViewById<TextView>(Resource.Id.phone);
            username = FindViewById<TextView>(Resource.Id.username);

            SetActionBar(toolbar);
            ActionBar.Title = "Korisnički podaci";
            sifraDjelatnika = localUsername.GetString("sifraDjelatnika", null);
            user = db.Query<KE_DJELATNICI>(
                "SELECT * " +
                "FROM KE_DJELATNICI " +
                "WHERE KE_MBR = ?", sifraDjelatnika).FirstOrDefault();

            username.Text = user.KE_IME + " " + user.KE_PREZIME;
            adresa.Text = user.KE_ADRESA + ", " + user.KE_POSTA + ", " + user.KE_MJESTO;
            email.Text = user.KE_EMAIL;
            oib.Text = user.KE_OIB;
            phone.Text = user.KE_MOBITEL;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menuClose, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.TitleFormatted.ToString() == "close")
            {
                intent = new Intent(this, typeof(Activity_Pocetna));
                StartActivity(intent);
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}