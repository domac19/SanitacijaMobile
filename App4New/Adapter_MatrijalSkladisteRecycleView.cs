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
    [Activity(Label = "Adapter_MatrijalSkladisteRecycleView", Icon = "@mipmap/cancelBtn")]
    public class Adapter_MatrijalSkladisteRecycleView : RecyclerView.Adapter
    {
        public event EventHandler<int> ItemClick;
        public List<DID_StanjeSkladista> mDID_StanjeSkladista;
        public Adapter_MatrijalSkladisteRecycleView(List<DID_StanjeSkladista> skladista)
        {
            mDID_StanjeSkladista = skladista;
        }
        public override int ItemCount
        {
            get { return mDID_StanjeSkladista.Count; }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            SkladisteViewHolder vh = holder as SkladisteViewHolder;

            if (mDID_StanjeSkladista[position].Kolicina < 0)
                vh.ItemView.SetBackgroundResource(Resource.Color.redLight);
            else
                vh.ItemView.SetBackgroundColor(Android.Graphics.Color.Transparent);

            vh.Naziv.Text = mDID_StanjeSkladista[position].MaterijalNaziv;
            vh.Kolicina.Text = mDID_StanjeSkladista[position].Kolicina.ToString("F3").Replace('.', ',');
            vh.MjernaJedinica.Text = mDID_StanjeSkladista[position].MjernaJedinicaOznaka;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.adapter_PotroseniMaterijali_List_row, parent, false);
            SkladisteViewHolder vh = new SkladisteViewHolder(itemView, OnClickItem);
            return vh;
        }
        private void OnClickItem(int obj)
        {
            ItemClick?.Invoke(this, obj);
        }
      
    }
    public class SkladisteViewHolder : RecyclerView.ViewHolder
    {
        public TextView Naziv { get; set; }
        public TextView Kolicina { get; set; }
        public TextView MjernaJedinica { get; set; }

        public SkladisteViewHolder(View itemview, Action<int> listener) : base(itemview)
        {
            Naziv = itemview.FindViewById<TextView>(Resource.Id.naziv);
            Kolicina = itemview.FindViewById<TextView>(Resource.Id.kolicina);
            MjernaJedinica = itemview.FindViewById<TextView>(Resource.Id.mjernaJedinica);
            itemview.Click += (sender, e) => listener(Position);
        }
    }
}
