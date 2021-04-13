using Infobit.DDD.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using SQLite;

namespace App4New
{
    [Activity(Label = "Adapter_RadniNalozi")]
    public class Adapter_RadniNalozi : RecyclerView.Adapter
    {
        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        public event EventHandler<int> ItemClick;
        public event EventHandler<int> ItemDelete;
        public List<DID_RadniNalog> mDID_RadniNalozi;
        public Adapter_RadniNalozi(List<DID_RadniNalog> radniNalog)
        {
            mDID_RadniNalozi = radniNalog;
        }
        public override int ItemCount
        {
            get { return mDID_RadniNalozi.Count; }
        }
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            RadniNalogViewHolder vh = holder as RadniNalogViewHolder;
            string nazivSkladista = db.Query<T_SKL>(
                "SELECT * " +
                "FROM T_SKL " +
                "WHERE SKL_SIFRA = ?", mDID_RadniNalozi[position].PokretnoSkladiste).FirstOrDefault().SKL_NAZIV;

            if(mDID_RadniNalozi[position].Status == 5 && mDID_RadniNalozi[position].SinhronizacijaStatus == 2)
            {
                vh.ItemView.SetBackgroundResource(Resource.Color.greenDark);
                vh.Status.Text = "izvršeno";
            }
            else if (mDID_RadniNalozi[position].Status == 5)
            {
                vh.ItemView.SetBackgroundResource(Resource.Color.colorPrimary);
                vh.Status.Text = "izvršeno";
            }
            else if(mDID_RadniNalozi[position].Status == 4)
            {
                vh.ItemView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                vh.Status.Text = "djelomično izvršeno";
            }
            else
            {
                vh.ItemView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                vh.Status.Text = "neizvršeno";
            }

            vh.Broj.Text = mDID_RadniNalozi[position].Broj.ToString();
            vh.Godina.Text = mDID_RadniNalozi[position].Godina.ToString();
            vh.Skladiste.Text = nazivSkladista;
            vh.DatumOd.Text = mDID_RadniNalozi[position].DatumOd.ToShortDateString();
            vh.DatumDo.Text = mDID_RadniNalozi[position].DatumDo.ToShortDateString();
            vh.Broj.Text = mDID_RadniNalozi[position].Broj.ToString();
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.adapter_RadniNalog_row, parent, false);
            RadniNalogViewHolder vh = new RadniNalogViewHolder(itemView, OnClickItem, OnClickDelete);
            return vh;
        }

        private void OnClickItem(int obj)
        {
            ItemDelete?.Invoke(this, obj);
        }
        private void OnClickDelete(int obj)
        {
            ItemClick?.Invoke(this, obj);
        }
    }
    public class RadniNalogViewHolder : RecyclerView.ViewHolder
    {
        public TextView Broj { get; set; }
        public TextView Godina { get; set; }
        public TextView Skladiste { get; set; }
        public TextView DatumOd { get; set; }
        public TextView DatumDo { get; set; }
        public TextView Status { get; set; }
        public Button Obrisi { get; set; }

        public RadniNalogViewHolder(View itemview, Action<int> listenerObrisi, Action<int> listener) : base(itemview)
        {
            Broj = itemview.FindViewById<TextView>(Resource.Id.broj);
            Godina = itemview.FindViewById<TextView>(Resource.Id.godina);
            Skladiste = itemview.FindViewById<TextView>(Resource.Id.skladiste);
            DatumOd = itemview.FindViewById<TextView>(Resource.Id.datumOd);
            DatumDo = itemview.FindViewById<TextView>(Resource.Id.datumDo);
            Status = itemview.FindViewById<TextView>(Resource.Id.status);
            Obrisi = itemview.FindViewById<Button>(Resource.Id.deleteBtn);
            itemview.Click += (sender, e) => listener(Position);
            Obrisi.Click += (sender, e) => listenerObrisi(Position);
        }
    }
}
