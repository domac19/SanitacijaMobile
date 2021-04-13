using Infobit.DDD.Data;
using System;
using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

namespace App4New
{
    [Activity(Label = "Adapter_Pozicija")]
    public class Adapter_Pozicija : BaseAdapter<DID_LokacijaPozicija>
    {
        readonly List<DID_LokacijaPozicija> items;
        readonly Activity context;
        public Adapter_Pozicija(Activity context, List<DID_LokacijaPozicija> items)
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
                view = context.LayoutInflater.Inflate(Resource.Layout.adapter_Pozicija_row, null);

            if(!String.IsNullOrEmpty(item.POZ_Broj))
                view.FindViewById<TextView>(Resource.Id.broj).Text = item.POZ_Broj;
            else
                view.FindViewById<TextView>(Resource.Id.broj).Text = " - ";

            if(!String.IsNullOrEmpty(item.POZ_BrojOznaka))
                view.FindViewById<TextView>(Resource.Id.brojOznaka).Text = item.POZ_BrojOznaka;
            else
                view.FindViewById<TextView>(Resource.Id.brojOznaka).Text = " - ";

            if (!String.IsNullOrEmpty(item.POZ_Barcode))
                view.FindViewById<TextView>(Resource.Id.barcode).Text = item.POZ_Barcode.ToString();
            else
                view.FindViewById<TextView>(Resource.Id.barcode).Text = " - ";

            return view;
        }
    }
}
