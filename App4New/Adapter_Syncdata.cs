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
    [Activity(Label = "Adapter_Syncdata")]
    public class Adapter_Syncdata : RecyclerView.Adapter
    {
        public List<Adapter_SyncData> mData;
        public Adapter_Syncdata(List<Adapter_SyncData> podatci)
        {
            mData = podatci;
        }
        public override int ItemCount
        {
            get { return mData.Count; }
        }
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            DataViewHolder vh = holder as DataViewHolder;
            vh.Naziv.Text = mData[position].Naziv;
            vh.PrimljeniPodatci.Text = mData[position].Primljeno.ToString();
            vh.PoslaniPodatci.Text = mData[position].Poslano.ToString();
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.adapter_SyncData_row, parent, false);
            DataViewHolder vh = new DataViewHolder(itemView);
            return vh;
        }
    }
    public class DataViewHolder : RecyclerView.ViewHolder
    {
        public TextView Naziv { get; set; }
        public TextView PrimljeniPodatci { get; set; }
        public TextView PoslaniPodatci { get; set; }

        public DataViewHolder(View itemview) : base(itemview)
        {
            Naziv = itemview.FindViewById<TextView>(Resource.Id.naziv);
            PoslaniPodatci = itemview.FindViewById<TextView>(Resource.Id.poslano);
            PrimljeniPodatci = itemview.FindViewById<TextView>(Resource.Id.primljeno);
        }
    }
}
