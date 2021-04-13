using Infobit.DDD.Data;
using System.Collections.Generic;
using System.IO;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using SQLite;
using System.Linq;

namespace App4New
{
    [Activity(Label = "Adapter_Komitent")]
    public class Adapter_Komitent : BaseAdapter<T_KUPDOB>
    {
        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);
        static readonly ISharedPreferences localKomitentLokacija = Application.Context.GetSharedPreferences("localKomitentLokacija", FileCreationMode.Private);

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        readonly List<T_KUPDOB> items;
        readonly Activity context;
        public Adapter_Komitent(Activity context, List<T_KUPDOB> items)
            : base()
        {
            this.context = context;
            this.items = items;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override T_KUPDOB this[int position]
        {
            get { return items[position]; }
        }
        public override int Count
        {
            get { return items.Count; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];
            View view = convertView;
            if (view == null)
                view = context.LayoutInflater.Inflate(Resource.Layout.adapter_Komitent_row, null);

            if(!localKomitentLokacija.GetBoolean("noviKomitent", false))
            {
                int radniNalog = localRadniNalozi.GetInt("id", 0);
                List<DID_RadniNalog_Lokacija> sveLokacije = db.Query<DID_RadniNalog_Lokacija>(
                    "SELECT * " +
                    "FROM DID_RadniNalog_Lokacija " +
                    "INNER JOIN DID_Lokacija ON DID_RadniNalog_Lokacija.Lokacija = DID_Lokacija.SAN_Id " +
                    "WHERE RadniNalog = ? " +
                    "AND SAN_KD_Sifra = ?", radniNalog, item.SIFRA);

                List<DID_RadniNalog_Lokacija> izvrseneLokacije = db.Query<DID_RadniNalog_Lokacija>(
                    "SELECT * " +
                    "FROM DID_RadniNalog_Lokacija " +
                    "INNER JOIN DID_Lokacija ON DID_RadniNalog_Lokacija.Lokacija = DID_Lokacija.SAN_Id " +
                    "WHERE RadniNalog = ? " +
                    "AND SAN_KD_Sifra = ? " +
                    "AND DID_RadniNalog_Lokacija.Status = 3", radniNalog, item.SIFRA);

                List<DID_RadniNalog_Lokacija> sinkroniziraneLokacije = db.Query<DID_RadniNalog_Lokacija>(
                    "SELECT * " +
                    "FROM DID_RadniNalog_Lokacija " +
                    "INNER JOIN DID_Lokacija ON DID_RadniNalog_Lokacija.Lokacija = DID_Lokacija.SAN_Id " +
                    "WHERE RadniNalog = ? " +
                    "AND SAN_KD_Sifra = ? " +
                    "AND DID_RadniNalog_Lokacija.Status = 3 " +
                    "AND DID_RadniNalog_Lokacija.SinhronizacijaStatus = 2", radniNalog, item.SIFRA);

                if(sveLokacije.Count == sinkroniziraneLokacije.Count)
                    view.SetBackgroundResource(Resource.Color.greenDark);
                else if (sveLokacije.Count == izvrseneLokacije.Count)
                    view.SetBackgroundResource(Resource.Color.colorPrimary);
                else
                    view.SetBackgroundColor(Android.Graphics.Color.Transparent);
            }
            
            view.FindViewById<TextView>(Resource.Id.kontakt).Text = item.TELEFON;
            view.FindViewById<TextView>(Resource.Id.naziv).Text = item.NAZIV;
            return view;
        }
    }
}
