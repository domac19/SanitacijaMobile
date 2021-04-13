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

namespace App4New
{
    [Activity(Label = "Adapter_AnketeRecycleView")]
    public class Adapter_AnketeRecycleView : RecyclerView.Adapter
    {
        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        public event EventHandler<int> ItemDelete;
        public event EventHandler<int> ItemClick;
        public event EventHandler<int> ItemEdit;
        public List<DID_Anketa> mDID_AnketaOriginal;
        public Adapter_AnketeRecycleView(List<DID_Anketa> ankete)
        {
            mDID_AnketaOriginal = ankete;
        }
        public override int ItemCount
        {
            get { return mDID_AnketaOriginal.Count; }
        }
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            AnketeViewHolder vh = holder as AnketeViewHolder;
            DID_LokacijaPozicija brojOznaka = db.Query<DID_LokacijaPozicija>(
                "SELECT * " +
                "FROM DID_LokacijaPozicija " +
                "INNER JOIN DID_Anketa ON DID_LokacijaPozicija.POZ_Id = DID_Anketa.ANK_POZ_Id " +
                "WHERE Id = ?", mDID_AnketaOriginal[position].Id).LastOrDefault();

            DID_LokacijaPozicija pozicija = db.Query<DID_LokacijaPozicija>(
                "SELECT * " +
                "FROM DID_LokacijaPozicija " +
                "WHERE POZ_Id = ?", mDID_AnketaOriginal[position].ANK_POZ_Id).FirstOrDefault();

            List<DID_Potvrda> potvrda = db.Query<DID_Potvrda>(
                "SELECT * " +
                "FROM DID_Potvrda " +
                "WHERE Lokacija = ? " +
                "AND RadniNalog = ?", pozicija.SAN_Id, mDID_AnketaOriginal[position].ANK_RadniNalog);

            if (potvrda.Any() && potvrda.FirstOrDefault().SinhronizacijaPrivremeniKljuc == null)
            {
                vh.DeleteBtn.Visibility = Android.Views.ViewStates.Gone;
            }   

            if (mDID_AnketaOriginal[position].ANK_RazlogNeizvrsenja > 0)
            {
                vh.UnCheckedBtn.Visibility = Android.Views.ViewStates.Visible;
                vh.CheckedBtn.Visibility = Android.Views.ViewStates.Gone;
                vh.EditBtn.Visibility = Android.Views.ViewStates.Invisible;
            }
            else
            {
                vh.UnCheckedBtn.Visibility = Android.Views.ViewStates.Gone;
                vh.CheckedBtn.Visibility = Android.Views.ViewStates.Visible;
            }

            vh.BrojOznaka.Text = brojOznaka.POZ_Broj + brojOznaka.POZ_BrojOznaka;
            vh.LastEditDate.Text = mDID_AnketaOriginal[position].LastEditDate.ToString();
            vh.OpisPozicije.Text = brojOznaka.POZ_Opis;
        }
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.adapter_OdradeneAnkete_row, parent, false);
            AnketeViewHolder vh = new AnketeViewHolder(itemView, OnClickDelete, OnClickItem, OnClickEdit);
            return vh;
        }

        private void OnClickDelete(int obj)
        {
            ItemDelete?.Invoke(this, obj);
        }

        private void OnClickEdit(int obj)
        {
            ItemClick?.Invoke(this, obj);
        }

        private void OnClickItem(int obj)
        {
            ItemEdit?.Invoke(this, obj);
        }
    }
    public class AnketeViewHolder : RecyclerView.ViewHolder
    {
        public TextView BrojOznaka { get; set; }
        public TextView LastEditDate { get; set; }
        public TextView OpisPozicije { get; set; }
        public Button DeleteBtn { get; set; }
        public Button EditBtn { get; set; }
        public Button CheckedBtn { get; set; }
        public Button UnCheckedBtn { get; set; }

        public AnketeViewHolder(View itemview, Action<int> listenerDelete, Action<int> listenerEdit, Action<int> listener) : base(itemview)
        {
            BrojOznaka = itemview.FindViewById<TextView>(Resource.Id.textView1);
            OpisPozicije = itemview.FindViewById<TextView>(Resource.Id.textView2);
            LastEditDate = itemview.FindViewById<TextView>(Resource.Id.textView3);
            CheckedBtn = itemview.FindViewById<Button>(Resource.Id.checkedBtn);
            UnCheckedBtn = itemview.FindViewById<Button>(Resource.Id.uncheckedBtn);
            DeleteBtn = itemview.FindViewById<Button>(Resource.Id.deleteBtn);
            EditBtn = itemview.FindViewById<Button>(Resource.Id.editBtn);
            DeleteBtn.Click += (sender, e) => listenerDelete(Position);
            EditBtn.Click += (sender, e) => listenerEdit(Position);
            itemview.Click += (sender, e) => listener(Position);
        }
    }
}
