using Infobit.DDD.Data;
using System;
using System.Collections.Generic;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Android.Content;

namespace App4New
{
    [Activity(Label = "Adapter_Materijali_ProvedbeniPlan")]
    public class Adapter_Materijali_ProvedbeniPlan : RecyclerView.Adapter
    {
        static readonly ISharedPreferences localProvedbeniPlan = Application.Context.GetSharedPreferences("localProvedbeniPlan", FileCreationMode.Private);

        public event EventHandler<int> ItemClick;
        public event EventHandler<int> CheckboxClick;
        public List<T_NAZR> materijali;
        public Adapter_Materijali_ProvedbeniPlan(List<T_NAZR> materijal)
        {
            materijali = materijal;
        }
        public override int ItemCount
        {
            get { return materijali.Count; }
        }
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            Materijali_ProvedbeniPlanViewHolder vh = holder as Materijali_ProvedbeniPlanViewHolder;
            // Provjera jeli odabran materijal te ga oznacuje ako je
            if (localProvedbeniPlan.GetString("materijal" + materijali[position].NAZR_SIFRA, null) != null)
                vh.MaterijalCheckBox.Checked = true;
            else
                vh.MaterijalCheckBox.Checked = false;

            // Disablanje checkboxa ako provedbeni plan postoji -> moguc samo pregled
            if (localProvedbeniPlan.GetInt("provedbeniPlanId", 0) == 0)
                vh.MaterijalCheckBox.Enabled = true;
            else
                vh.MaterijalCheckBox.Enabled = false;

            vh.MaterijalCheckBox.Text = materijali[position].NAZR_NAZIV;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.adapter_Materijal_ProvedbeniPlan_row, parent, false);
            Materijali_ProvedbeniPlanViewHolder vh = new Materijali_ProvedbeniPlanViewHolder(itemView, OnClickItem, OnCheckboxClick);
            return vh;
        }

        private void OnClickItem(int obj)
        {
            ItemClick?.Invoke(this, obj);
        }
        private void OnCheckboxClick(int obj)
        {
            CheckboxClick?.Invoke(this, obj);
        }

    }
    public class Materijali_ProvedbeniPlanViewHolder : RecyclerView.ViewHolder
    {
        public CheckBox MaterijalCheckBox { get; set; }
        

        public Materijali_ProvedbeniPlanViewHolder(View itemview, Action<int> listener, Action<int> listenerCheckBoxClick) : base(itemview)
        {
            MaterijalCheckBox = itemview.FindViewById<CheckBox>(Resource.Id.checkbox);
            MaterijalCheckBox.Click += (sender, e) => listenerCheckBoxClick(Position);
            itemview.Click += (sender, e) => listener(Position);
        }
    }
}
