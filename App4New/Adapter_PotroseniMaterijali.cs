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
using Android.Content;

namespace App4New
{
    [Activity(Label = "Adapter_PotroseniMaterijali")]
    public class Adapter_PotroseniMaterijali : RecyclerView.Adapter
    {
        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        public int redniBroj = 0;
        public event EventHandler<int> ItemClick;
        public event EventHandler<int> ItemDelete;
        public List<DID_AnketaMaterijali> mDID_AnketaMaterijali;
        public Adapter_PotroseniMaterijali(List<DID_AnketaMaterijali> materijali)
        {
            mDID_AnketaMaterijali = materijali;
        }
        public override int ItemCount
        {
            get { return mDID_AnketaMaterijali.Count; }
        }
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            MaterijaliViewHolder vh = holder as MaterijaliViewHolder;

            string mjernaJedinica = db.Query<T_MjerneJedinice>(
                "SELECT * " +
                "FROM T_MjerneJedinice " +
                "WHERE Id = ?", mDID_AnketaMaterijali[position].MjernaJedinica).FirstOrDefault().Oznaka;

            redniBroj++;
            vh.RedniBroj.Text = redniBroj.ToString();
            vh.Sifra.Text = mDID_AnketaMaterijali[position].MaterijalSifra;
            vh.Naziv.Text = mDID_AnketaMaterijali[position].MaterijalNaziv;
            vh.Kolicina.Text = mDID_AnketaMaterijali[position].Kolicina.ToString("F3").Replace('.', ',') + " " + mjernaJedinica;
            vh.Cijena.Text = mDID_AnketaMaterijali[position].Cijena.ToString("F2").Replace('.', ',') + " kn";
            vh.Iznos.Text = mDID_AnketaMaterijali[position].Iznos.ToString("F2").Replace('.', ',') + " kn";
        }
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.adapter_PotroseniMaterijali_Button_row, parent, false);
            MaterijaliViewHolder vh = new MaterijaliViewHolder(itemView, OnClickDelete, OnClickItem);           
            return vh;
        }

        private void OnClickDelete(int obj)
        {
            ItemDelete?.Invoke(this, obj);
        }

        private void OnClickItem(int obj)
        {
            ItemClick?.Invoke(this, obj);
        }
    }
    public class MaterijaliViewHolder : RecyclerView.ViewHolder
    {
        public TextView RedniBroj { get; set; }
        public TextView Sifra { get; set; }
        public TextView Naziv { get; set; }
        public TextView Kolicina { get; set; }
        public TextView Cijena { get; set; }
        public TextView Iznos { get; set; }
        public Button DeleteBtn { get; set; }

        public MaterijaliViewHolder(View itemview, Action<int> listenerDelete, Action<int> listener) : base(itemview)
        {
            RedniBroj = itemview.FindViewById<TextView>(Resource.Id.textView1);
            Sifra = itemview.FindViewById<TextView>(Resource.Id.textView2);
            Naziv = itemview.FindViewById<TextView>(Resource.Id.textView3);
            Kolicina = itemview.FindViewById<TextView>(Resource.Id.textView4);
            Cijena = itemview.FindViewById<TextView>(Resource.Id.textView5);
            Iznos = itemview.FindViewById<TextView>(Resource.Id.textView6);
            DeleteBtn = itemview.FindViewById<Button>(Resource.Id.deleteBtn);
            DeleteBtn.Click += (sender, e) => listenerDelete(Position);
            itemview.Click += (sender, e) => listener(Position);
        }
    }
}
