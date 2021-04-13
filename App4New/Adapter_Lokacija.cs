using Infobit.DDD.Data;
using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;

namespace App4New
{
    [Activity(Label = "Adapter_Lokacija")]
    public class Adapter_Lokacija : BaseAdapter<DID_Lokacija>
    {
        readonly List<DID_Lokacija> items;
        readonly Activity context;
        public Adapter_Lokacija(Activity context, List<DID_Lokacija> items)
            : base()
        {
            this.context = context;
            this.items = items;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override DID_Lokacija this[int position]
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
                view = context.LayoutInflater.Inflate(Resource.Layout.adapter_NovaLokacija_row, null);

            view.FindViewById<TextView>(Resource.Id.naziv).Text = item.SAN_Naziv;
            view.FindViewById<TextView>(Resource.Id.mjesto).Text = item.SAN_Mjesto;
            view.FindViewById<TextView>(Resource.Id.adresa).Text = item.SAN_UlicaBroj;
            return view;
        }
    }
}