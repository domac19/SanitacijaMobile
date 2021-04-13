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
    [Activity(Label = "Adapter_LokacijeRecycleView", Icon = "@mipmap/cancelBtn")]
    public class Adapter_LokacijeRecycleView : RecyclerView.Adapter
    {
        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);

        public event EventHandler<int> ItemZakljucaj;
        public event EventHandler<int> ItemPotvrda;
        public event EventHandler<int> ItemClick;
        public event EventHandler<int> ItemProvedbeniPlan;
        public event EventHandler<int> ItemPostavke;
        public List<DID_Lokacija> mDID_Lokacije;
        public Adapter_LokacijeRecycleView(List<DID_Lokacija> lokacije)
        {
            mDID_Lokacije = lokacije;
        }
        public override int ItemCount
        {
            get { return mDID_Lokacije.Count; }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            LokacijaViewHolder vh = holder as LokacijaViewHolder;
            DID_Lokacija lokacija = db.Query<DID_Lokacija>(
                "SELECT * " +
                "FROM DID_Lokacija " +
                "WHERE SAN_Id = ?", mDID_Lokacije[position].SAN_Id).FirstOrDefault();

            int radniNalog = localRadniNalozi.GetInt("id", 0);
            DID_RadniNalog_Lokacija radniNalogLokacija = db.Query<DID_RadniNalog_Lokacija>(
                "SELECT * " +
                "FROM DID_RadniNalog_Lokacija " +
                "WHERE Lokacija = ? " +
                "AND RadniNalog = ?", mDID_Lokacije[position].SAN_Id, radniNalog).FirstOrDefault();
            int tipAkcije = radniNalogLokacija.TipAkcije;

            List<DID_Potvrda> potvrda = db.Query<DID_Potvrda>(
                "SELECT * " +
                "FROM DID_Potvrda " +
                "WHERE Lokacija = ? " +
                "AND RadniNalog = ?", mDID_Lokacije[position].SAN_Id, radniNalog);

            if(radniNalogLokacija.Status == 4)
            {
                vh.ItemView.SetBackgroundResource(Resource.Color.redLight);
                vh.Potvrda.Visibility = Android.Views.ViewStates.Gone;
                vh.Zakljucaj.Visibility = Android.Views.ViewStates.Gone;
                vh.Otkljucaj.Visibility = Android.Views.ViewStates.Gone;
            }
            else if(radniNalogLokacija.Status == 3)
            {
                vh.Potvrda.Visibility = Android.Views.ViewStates.Visible;
                vh.Potvrda.Text = "prikaži potvrdu";
                vh.Zakljucaj.Visibility = Android.Views.ViewStates.Gone;
                vh.Otkljucaj.Visibility = Android.Views.ViewStates.Visible;

                if (radniNalogLokacija.SinhronizacijaStatus == 2)
                {
                    vh.ItemView.SetBackgroundResource(Resource.Color.greenDark);
                    vh.Potvrda.SetTextColor(Android.Graphics.Color.White);
                }
                else
                {
                    vh.ItemView.SetBackgroundResource(Resource.Color.colorPrimary);
                    vh.Potvrda.SetTextColor(Android.Graphics.Color.ParseColor("#ff669900"));
                }
            }
            else if (potvrda.Any())
            {
                vh.Potvrda.Visibility = Android.Views.ViewStates.Visible;
                vh.Potvrda.Text = "prikaži potvrdu";
                vh.Zakljucaj.Visibility = Android.Views.ViewStates.Visible;
                vh.Otkljucaj.Visibility = Android.Views.ViewStates.Gone;
                vh.ItemView.SetBackgroundColor(Android.Graphics.Color.Transparent);
            }
            else
            {
                List<DID_LokacijaPozicija> pozicijeOdradene = db.Query<DID_LokacijaPozicija>(
                    "SELECT * " +
                    "FROM DID_LokacijaPozicija " +
                    "INNER JOIN DID_Anketa ON DID_LokacijaPozicija.POZ_Id = DID_Anketa.ANK_POZ_Id " +
                    "WHERE DID_Anketa.ANK_RadniNalog = ? " +
                    "AND DID_LokacijaPozicija.SAN_Id = ?", radniNalog, mDID_Lokacije[position].SAN_Id);

                List<DID_LokacijaPozicija> pozicijeUkupno = db.Query<DID_LokacijaPozicija>(
                    "SELECT * " +
                    "FROM DID_LokacijaPozicija " +
                    "INNER JOIN DID_RadniNalog_Lokacija ON DID_LokacijaPozicija.SAN_Id = DID_RadniNalog_Lokacija.Lokacija " +
                    "WHERE DID_RadniNalog_Lokacija.RadniNalog = ? " +
                    "AND DID_LokacijaPozicija.SAN_Id = ?", radniNalog, mDID_Lokacije[position].SAN_Id);

                vh.Zakljucaj.Visibility = Android.Views.ViewStates.Gone;
                vh.Otkljucaj.Visibility = Android.Views.ViewStates.Gone;

                if (!mDID_Lokacije[position].SAN_AnketePoPozicijama)
                {
                    vh.ItemView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    vh.Potvrda.Text = "izdaj potvrdu";
                }
                else if (pozicijeOdradene.Count == pozicijeUkupno.Count && pozicijeOdradene.Count > 0)
                {
                    vh.ItemView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    vh.Potvrda.Text = "izdaj potvrdu";
                }
                else
                {
                    vh.ItemView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    vh.Potvrda.Visibility = Android.Views.ViewStates.Invisible;
                }
            }

            vh.Naziv.Text = lokacija.SAN_Naziv;
            vh.Mjesto.Text = lokacija.SAN_Mjesto;
            vh.Adresa.Text = lokacija.SAN_UlicaBroj;
            vh.TipAkcije.Text = lokacija.SAN_Tip.ToString();
            if(tipAkcije == 1)
                vh.TipAkcije.Text = "Prvi dolazak";
            else
                vh.TipAkcije.Text = "Kontrola";   
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.adapter_Lokacija_row, parent, false);
            LokacijaViewHolder vh = new LokacijaViewHolder(itemView, OnClickItem, OnClickPotvrda, OnClickZakljucaj, onClickProvedbeniPlan, onClickPostavke);
            return vh;
        }
        private void OnClickItem(int obj)
        {
            ItemClick?.Invoke(this, obj);
        }
        private void OnClickPotvrda(int obj)
        {
            ItemPotvrda?.Invoke(this, obj);
        }
        private void onClickPostavke(int obj)
        {
            ItemPostavke?.Invoke(this, obj);
        }
        private void OnClickZakljucaj(int obj)
        {
            ItemZakljucaj?.Invoke(this, obj);
        }
        private void onClickProvedbeniPlan(int obj)
        {
            ItemProvedbeniPlan?.Invoke(this, obj);
        }
    }
    public class LokacijaViewHolder : RecyclerView.ViewHolder
    {
        public TextView Naziv { get; set; }
        public TextView Mjesto { get; set; }
        public TextView Adresa { get; set; }
        public TextView TipAkcije { get; set; }
        public Button Potvrda { get; set; }
        public Button Zakljucaj { get; set; }
        public Button Otkljucaj { get; set; }
        public Button ProvedbeniPlanBtn { get; set; }
        public Button PostavkeBtn { get; set; }

        public LokacijaViewHolder(View itemview, Action<int> listener, Action<int> listenerPotvrda, Action<int> listenerZakljucaj, Action<int> listenerProvedbeniPlan, Action<int> listenerPostavke) : base(itemview)
        {
            Naziv = itemview.FindViewById<TextView>(Resource.Id.naziv);
            Mjesto = itemview.FindViewById<TextView>(Resource.Id.mjesto);
            Adresa = itemview.FindViewById<TextView>(Resource.Id.adresa);
            TipAkcije = itemview.FindViewById<TextView>(Resource.Id.tipAkcije);
            Potvrda = itemview.FindViewById<Button>(Resource.Id.potvrdaBtn);
            Zakljucaj = itemview.FindViewById<Button>(Resource.Id.zakljucajBtn);
            Otkljucaj = itemview.FindViewById<Button>(Resource.Id.otkljucajBtn);
            ProvedbeniPlanBtn = itemview.FindViewById<Button>(Resource.Id.provedbeniPlanBtn);
            PostavkeBtn = itemview.FindViewById<Button>(Resource.Id.postavkeBtn);
            PostavkeBtn.Click += (sender, e) => listenerPostavke(Position);
            ProvedbeniPlanBtn.Click += (sender, e) => listenerProvedbeniPlan(Position);
            Potvrda.Click += (sender, e) => listenerPotvrda(Position);
            Zakljucaj.Click += (sender, e) => listenerZakljucaj(Position);
            Otkljucaj.Click += (sender, e) => listenerZakljucaj(Position);
            itemview.Click += (sender, e) => listener(Position);
        }
    }
}
