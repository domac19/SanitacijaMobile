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
    [Activity(Label = "Adapter_MaterijaliPotvrda")]
    public class Adapter_MaterijaliPotvrda : BaseAdapter<DID_AnketaMaterijali>
    {
        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        readonly List<DID_AnketaMaterijali> items;
        readonly Activity context;
        public Adapter_MaterijaliPotvrda(Activity context, List<DID_AnketaMaterijali> items)
            : base()
        {
            this.context = context;
            this.items = items;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override DID_AnketaMaterijali this[int position]
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
                view = context.LayoutInflater.Inflate(Resource.Layout.adapter_PotroseniMaterijali_Potvrda_row, null);

            string mjernaJedinica = db.Query<T_MjerneJedinice>(
                "SELECT * " +
                "FROM T_MjerneJedinice " +
                "WHERE Id = ?", item.MjernaJedinica).FirstOrDefault().Oznaka;

            view.FindViewById<TextView>(Resource.Id.textView1).Text = (GetItemId(position) + 1).ToString();
            view.FindViewById<TextView>(Resource.Id.textView2).Text = item.MaterijalSifra;
            view.FindViewById<TextView>(Resource.Id.textView3).Text = item.MaterijalNaziv;
            view.FindViewById<TextView>(Resource.Id.textView4).Text = item.Kolicina.ToString("F3").Replace('.', ',') + " " + mjernaJedinica;
            view.FindViewById<TextView>(Resource.Id.textView5).Text = item.Cijena.ToString("F2").Replace('.', ',') + " kn";
            view.FindViewById<TextView>(Resource.Id.textView6).Text = item.Iznos.ToString("F2").Replace('.', ',') + " kn";
            return view;
        }
    }
}
