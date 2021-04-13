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
    [Activity(Label = "Adapter_Nametnici_ProvedbeniPlan")]
    public class Adapter_Nametnici_ProvedbeniPlan : RecyclerView.Adapter
    {
        static readonly ISharedPreferences localProvedbeniPlan = Application.Context.GetSharedPreferences("localProvedbeniPlan", FileCreationMode.Private);
        static readonly ISharedPreferences localPotvrda = Application.Context.GetSharedPreferences("potvrda", FileCreationMode.Private);

        public event EventHandler<int> ItemClick;
        public event EventHandler<int> CheckboxClick;
        public List<DID_Nametnik> nametnici;
        public Adapter_Nametnici_ProvedbeniPlan(List<DID_Nametnik> nametnik)
        {
            nametnici = nametnik;
        }
        public override int ItemCount
        {
            get { return nametnici.Count; }
        }
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            Materijali_ProvedbeniPlanViewHolder vh = holder as Materijali_ProvedbeniPlanViewHolder;

            // Provjera jeli odabran namentik
            var dsd = localPotvrda.GetString("nametnik" + nametnici[position].Sifra, null) != null;
            if (localProvedbeniPlan.GetString("nametnik" + nametnici[position].Sifra, null) != null || localPotvrda.GetString("nametnik" + nametnici[position].Sifra, null) != null)
                vh.MaterijalCheckBox.Checked = true;
            else
                vh.MaterijalCheckBox.Checked = false;

            // Disablanje checkboxa ako provedbeni plan postoji -> moguc samo pregled
            if (localProvedbeniPlan.GetInt("provedbeniPlanId", 0) == 0)
                vh.MaterijalCheckBox.Enabled = true;
            else
                vh.MaterijalCheckBox.Enabled = false;


            vh.MaterijalCheckBox.Text = nametnici[position].Naziv;
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
    public class Nametnici_ProvedbeniPlanViewHolder : RecyclerView.ViewHolder
    {
        public CheckBox MaterijalCheckBox { get; set; }
        

        public Nametnici_ProvedbeniPlanViewHolder(View itemview, Action<int> listener, Action<int> listenerCheckBoxClick) : base(itemview)
        {
            MaterijalCheckBox = itemview.FindViewById<CheckBox>(Resource.Id.checkbox);
            MaterijalCheckBox.Click += (sender, e) => listenerCheckBoxClick(Position);
            itemview.Click += (sender, e) => listener(Position);
        }
    }
}
