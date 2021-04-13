using Infobit.DDD.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Android.App;
using Android.Views;
using Android.Widget;
using SQLite;
using Android.Content;

namespace App4New
{
    [Activity(Label = "Adapter_MaterijaliPozicija")]
    public class Adapter_MaterijaliPozicija : BaseAdapter<DID_LokacijaPozicija>
    {
        static readonly ISharedPreferences localMaterijali = Application.Context.GetSharedPreferences("localMaterijali", FileCreationMode.Private);
        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        readonly List<DID_LokacijaPozicija> items;
        readonly Activity context;
        public Adapter_MaterijaliPozicija(Activity context, List<DID_LokacijaPozicija> items)
            : base()
        {
            this.context = context;
            this.items = items;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override DID_LokacijaPozicija this[int position]
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
                view = context.LayoutInflater.Inflate(Resource.Layout.adapter_potroseniMaterijali_PrikazPozicija_row, null);

            string materijalSifra = localMaterijali.GetString("sifra", null);
            int radniNalog = localRadniNalozi.GetInt("id", 0);

            List<DID_AnketaMaterijali> filtriranePotrosnje = db.Query<DID_AnketaMaterijali>(
                "SELECT mat.Id, mat.Cijena, mat.LokacijaId, TOTAL(mat.Iznos) AS Iznos, mat.RadniNalog, mat.PozicijaId, mat.MaterijalSifra, mat.MaterijalNaziv, mat.MjernaJedinica, TOTAL(mat.Kolicina) AS Kolicina " +
                "FROM DID_AnketaMaterijali mat " +
                "WHERE mat.PozicijaId = ? " +
                "AND mat.RadniNalog = ? " +
                "AND mat.MaterijalSifra = ? " +
                "GROUP BY mat.MaterijalNaziv, mat.MaterijalSifra", item.POZ_Id, radniNalog, materijalSifra);

            view.FindViewById<TextView>(Resource.Id.pozicija).Text = item.POZ_Broj + " " + item.POZ_BrojOznaka;
            view.FindViewById<TextView>(Resource.Id.naziv).Text = filtriranePotrosnje.FirstOrDefault().Kolicina.ToString("F3").Replace('.', ',');
            return view;
        }
    }   
}