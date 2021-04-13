using Infobit.DDD.Data;
using System;
using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

namespace App4New
{
    [Activity(Label = "Adapter_Opcina")]
    public class Adapter_Opcina : BaseAdapter<T_OPCINE>
    {
        readonly List<T_OPCINE> items;
        readonly List<T_ZUPANIJE> zupanije;
        readonly Activity context;
        string nazivZupanije;
        public Adapter_Opcina(Activity context, List<T_OPCINE> items, List<T_ZUPANIJE> zupanije)
            : base()
        {
            this.zupanije = zupanije;
            this.context = context;
            this.items = items;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override T_OPCINE this[int position]
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
                view = context.LayoutInflater.Inflate(Resource.Layout.adapter_Opcina_row, null);

            foreach (var zupanija in zupanije)
            {
                if (zupanija.Sifra == item.Zupanija)
                    nazivZupanije = zupanija.Naziv;
            }

            view.FindViewById<TextView>(Resource.Id.zupanija).Text = nazivZupanije;
            view.FindViewById<TextView>(Resource.Id.opcina).Text = item.Naziv;
            return view;
        }
    }
}
