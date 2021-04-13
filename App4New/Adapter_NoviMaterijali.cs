using Infobit.DDD.Data;
using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

namespace App4New
{
    [Activity(Label = "Adapter_NoviMaterijali")]
    public class Adapter_NoviMaterijali : BaseAdapter<T_NAZR>
    {
        readonly List<T_NAZR> items;
        readonly Activity context;

        public Adapter_NoviMaterijali(Activity context, List<T_NAZR> items)
            : base()
        {
            this.context = context;
            this.items = items;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override T_NAZR this[int position]
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
                view = context.LayoutInflater.Inflate(Resource.Layout.adapter_NoviMaterijal_row, null);

            view.FindViewById<TextView>(Resource.Id.naziv).Text = item.NAZR_NAZIV;
            view.FindViewById<TextView>(Resource.Id.cijena).Text = item.NAZR_CIJENA_ART.ToString() + " kn";
            view.FindViewById<TextView>(Resource.Id.grupa).Text = "grupa";
            return view;
        }
    }
}
