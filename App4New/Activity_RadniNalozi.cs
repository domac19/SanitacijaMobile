using Infobit.DDD.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using SQLite;
using Android.Support.V7.Widget;
using System;
using Android.Content.PM;

namespace App4New
{
    [Activity(Label = "Activity_RadniNalozi", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_RadniNalozi : Activity
    {
        EditText searchInput;
        TextView resultMessage;
        Intent intent;
        RecyclerView radniNaloziListView;
        RecyclerView.LayoutManager mLayoutManager;
        Adapter_RadniNalozi mAdapter;
        Button noviRNBtn;
        List<DID_RadniNalog> radniNalozi, radniNaloziFiltered;

        static readonly ISharedPreferences localUsername = Application.Context.GetSharedPreferences("userLogin", FileCreationMode.Private);
        static readonly ISharedPreferences localPotvrda = Application.Context.GetSharedPreferences("potvrda", FileCreationMode.Private);
        static readonly ISharedPreferences localMaterijali = Application.Context.GetSharedPreferences("localMaterijali", FileCreationMode.Private);
        static readonly ISharedPreferences localTipDeratizacije = Application.Context.GetSharedPreferences("tipDeratizacije", FileCreationMode.Private);
        static readonly ISharedPreferences localOdradeneAnkete = Application.Context.GetSharedPreferences("ankete", FileCreationMode.Private);
        static readonly ISharedPreferences localPozicija = Application.Context.GetSharedPreferences("pozicija", FileCreationMode.Private);
        static readonly ISharedPreferences localAnketa = Application.Context.GetSharedPreferences("anketa", FileCreationMode.Private);
        static readonly ISharedPreferences localKomitentLokacija = Application.Context.GetSharedPreferences("localKomitentLokacija", FileCreationMode.Private);
        static readonly ISharedPreferences localDoneDeratization = Application.Context.GetSharedPreferences("doneDeratization", FileCreationMode.Private);
        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);
        static readonly ISharedPreferences localDeratization = Application.Context.GetSharedPreferences("deratizacija", FileCreationMode.Private);
        static ISharedPreferencesEditor radniNaloziEdit = localRadniNalozi.Edit();

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.radniNalozi);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            radniNaloziListView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            searchInput = FindViewById<EditText>(Resource.Id.searchInput);
            resultMessage = FindViewById<TextView>(Resource.Id.resultMessage);
            noviRNBtn = FindViewById<Button>(Resource.Id.noviRadniNalogBtn);

            SetActionBar(toolbar);
            ActionBar.Title = "Odabir radnog naloga";
            noviRNBtn.Click += NoviRNBtn_Click;
            searchInput.KeyPress += SearchInput_KeyPress;
            searchInput.TextChanged += SearchInput_TextChanged;
            localDoneDeratization.Edit().Clear().Commit();
            localAnketa.Edit().Clear().Commit();
            localDoneDeratization.Edit().Clear().Commit();
            localAnketa.Edit().Clear().Commit();
            localRadniNalozi.Edit().Clear().Commit();
            localPozicija.Edit().Clear().Commit();
            localKomitentLokacija.Edit().Clear().Commit();
            localDeratization.Edit().Clear().Commit();
            localOdradeneAnkete.Edit().Clear().Commit();
            localTipDeratizacije.Edit().Clear().Commit();
            localMaterijali.Edit().Clear().Commit();
            localPotvrda.Edit().Clear().Commit();

            radniNalozi = db.Query<DID_RadniNalog>(
                "SELECT DID_RadniNalog.Id, DID_RadniNalog.Godina, DID_RadniNalog.Godina, DID_RadniNalog.Broj, DID_RadniNalog.Status, " +
                    "DID_RadniNalog.PokretnoSkladiste, DID_RadniNalog.Voditelj, DID_RadniNalog.VoditeljKontakt, DID_RadniNalog.Izdavatelj, " +
                    "DID_RadniNalog.Primatelj, DID_RadniNalog.DatumOd, DID_RadniNalog.DatumDo, DID_RadniNalog.DatumIzrade, DID_RadniNalog.DatumIzvrsenja, DID_RadniNalog.SinhronizacijaStatus, DID_RadniNalog.SinhronizacijaPrivremeniKljuc " +
                "FROM DID_RadniNalog " +
                "INNER JOIN DID_RadniNalog_Djelatnik ON DID_RadniNalog.Id = DID_RadniNalog_Djelatnik.RadniNalog " +
                "WHERE DID_RadniNalog_Djelatnik.Djelatnik = ?", localUsername.GetString("sifraDjelatnika", null));

            PrikazRadnihNaloga();
        }

        public override void OnBackPressed()
        {
            intent = new Intent(this, typeof(Activity_Pocetna));
            StartActivity(intent);
        }

        public void SearchInput_TextChanged(object sender, EventArgs e)
        {
            string input = searchInput.Text.ToLower();
            if (!string.IsNullOrEmpty(input))
            {
                resultMessage.Visibility = Android.Views.ViewStates.Invisible;
                radniNaloziFiltered = radniNalozi.Where(i =>
                    (i.Broj.ToString() != null && i.Broj.ToString().Contains(input)) ||
                    (i.Godina != null && i.Godina.ToLower().Contains(input)) ||
                    (i.Status.ToString() != null && i.Status.ToString().Contains(input)) ||
                    (i.PokretnoSkladiste != null && i.PokretnoSkladiste.Contains(input))).ToList();

                if (radniNaloziFiltered.Any())
                {
                    radniNaloziListView.Visibility = Android.Views.ViewStates.Visible;
                    mLayoutManager = new LinearLayoutManager(this);
                    mAdapter = new Adapter_RadniNalozi(radniNaloziFiltered);
                    radniNaloziListView.SetLayoutManager(mLayoutManager);
                    mAdapter.ItemClick += MAdapter_ItemClick;
                    radniNaloziListView.SetAdapter(mAdapter);
                }
                else
                {
                    radniNaloziListView.Visibility = Android.Views.ViewStates.Gone;
                    resultMessage.Visibility = Android.Views.ViewStates.Visible;
                }
            }
            else
                PrikazRadnihNaloga();
        }

        public void SearchInput_KeyPress(object sender, View.KeyEventArgs e)
        {
            e.Handled = false;
            if (e.KeyCode == Keycode.Enter)
                e.Handled = true;
        }

        public void NoviRNBtn_Click(object sender, EventArgs args)
        {
            intent = new Intent(this, typeof(Activity_NoviRadniNalog));
            StartActivity(intent);
        }

        public void PrikazRadnihNaloga()
        {
            radniNaloziFiltered = radniNalozi;

            if (radniNaloziFiltered.Any())
            {
                radniNaloziListView.Visibility = Android.Views.ViewStates.Visible;
                resultMessage.Visibility = Android.Views.ViewStates.Invisible;
                mLayoutManager = new LinearLayoutManager(this);
                mAdapter = new Adapter_RadniNalozi(radniNaloziFiltered);
                radniNaloziListView.SetLayoutManager(mLayoutManager);
                mAdapter.ItemClick += MAdapter_ItemClick;
                mAdapter.ItemDelete += MAdapter_ItemDelete;
                radniNaloziListView.SetAdapter(mAdapter);
            }
            else
            {
                radniNaloziListView.Visibility = Android.Views.ViewStates.Gone;
                resultMessage.Text = "Nema aktivnih radnih naloga!";
                resultMessage.Visibility = Android.Views.ViewStates.Visible;
            }
        }

        public void MAdapter_ItemDelete(object sender, int e)
        {
            var radniNalog = db.Query<DID_RadniNalog>(
                "SELECT * " +
                "FROM DID_RadniNalog " +
                "WHERE Id = ?", radniNaloziFiltered[e].Id).FirstOrDefault();

            if(radniNalog.Status == 4 || radniNalog.Status == 5)
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Upozorenje!");
                alert.SetMessage("Jeste li sigurni da želite obrisati radni nalog? Ovaj radni nalog je djelomično ili potpuno odrađen!");
                alert.SetPositiveButton("OBRIŠI", (senderAlert, arg) =>
                {
                    List<DID_RadniNalog_Lokacija> radniNalogLokacije = db.Query<DID_RadniNalog_Lokacija>(
                        "SELECT * " +
                        "FROM DID_RadniNalog_Lokacija " +
                        "WHERE RadniNalog = ?", radniNaloziFiltered[e].Id);

                    foreach (var radniNalogLokacija in radniNalogLokacije)
                    {
                        // Ako je lokacija izvrsena, tj. ima ozradenu potvrdu
                        if (radniNalogLokacija.Status == 3)
                        {
                            // Brisanje podataka vezanih uz potvrdu
                            var potvrdaId = db.Query<DID_Potvrda>(
                                "SELECT * " +
                                "FROM DID_Potvrda " +
                                "WHERE RadniNalog = ? " +
                                "AND Lokacija = ?", radniNaloziFiltered[e].Id, radniNalogLokacija.Lokacija).FirstOrDefault().Id;

                            db.Query<DID_Potvrda_Materijal>(
                                "DELETE FROM DID_Potvrda_Materijal " +
                                "WHERE Potvrda = ?", potvrdaId);
                            db.Query<DID_Potvrda_Djelatnost>(
                                "DELETE FROM DID_Potvrda_Djelatnost " +
                                "WHERE Potvrda = ?", potvrdaId);
                            db.Query<DID_Potvrda_Nametnik>(
                                "DELETE FROM DID_Potvrda_Nametnik " +
                                "WHERE Potvrda = ?", potvrdaId);
                            db.Delete<DID_Potvrda>(potvrdaId);
                        }

                        // brisanje anketa
                        db.Query<DID_Anketa>(
                            "DELETE FROM DID_Anketa " +
                            "WHERE ANK_RadniNalog = ?", radniNaloziFiltered[e].Id);

                        db.Query<DID_AnketaMaterijali>(
                            "DELETE FROM DID_AnketaMaterijali " +
                            "WHERE RadniNalog = ?", radniNaloziFiltered[e].Id);
                    }

                    db.Delete<DID_RadniNalog>(radniNaloziFiltered[e].Id);

                    db.Execute(
                        "DELETE FROM DID_RadniNalog_Lokacija " +
                        "WHERE RadniNalog = ?", radniNaloziFiltered[e].Id);

                    db.Execute(
                        "DELETE FROM DID_RadniNalog_Materijal " +
                        "WHERE RadniNalog = ?", radniNaloziFiltered[e].Id);

                    db.Execute(
                        "DELETE FROM DID_RadniNalog_Djelatnik " +
                        "WHERE RadniNalog = ?", radniNaloziFiltered[e].Id);

                    intent = new Intent(this, typeof(Activity_RadniNalozi));
                    StartActivity(intent);
                });

                alert.SetNegativeButton("ODUSTANI", (senderAlert, arg) => { });
                Dialog dialog = alert.Create();
                dialog.Show();
            }
            else
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Upozorenje!");
                alert.SetMessage("Jeste li sigurni da želite obrisati radni nalog?");
                alert.SetPositiveButton("OBRIŠI", (senderAlert, arg) =>
                {
                    db.Execute(
                        "DELETE FROM DID_Anketa " +
                        "WHERE ANK_RadniNalog = ?", radniNaloziFiltered[e].Id);

                    db.Execute(
                        "DELETE FROM DID_AnketaMaterijali " +
                        "WHERE RadniNalog = ?", radniNaloziFiltered[e].Id);

                    db.Delete<DID_RadniNalog>(radniNaloziFiltered[e].Id);

                    db.Execute(
                        "DELETE FROM DID_RadniNalog_Lokacija " +
                        "WHERE RadniNalog = ?", radniNaloziFiltered[e].Id);

                    db.Execute(
                        "DELETE FROM DID_RadniNalog_Materijal " +
                        "WHERE RadniNalog = ?", radniNaloziFiltered[e].Id);

                    db.Execute(
                        "DELETE FROM DID_RadniNalog_Djelatnik " +
                        "WHERE RadniNalog = ?", radniNaloziFiltered[e].Id);

                    intent = new Intent(this, typeof(Activity_RadniNalozi));
                    StartActivity(intent);
                });

                alert.SetNegativeButton("ODUSTANI", (senderAlert, arg) => { });
                Dialog dialog = alert.Create();
                dialog.Show();
            }
        }

        public void MAdapter_ItemClick(object sender, int e)
        {
            radniNaloziEdit.PutBoolean("noviRN", false);
            radniNaloziEdit.PutString("skladiste", radniNaloziFiltered[e].PokretnoSkladiste);
            radniNaloziEdit.PutInt("id", radniNaloziFiltered[e].Id);
            radniNaloziEdit.Commit();

            List <T_KUPDOB> komitenti = db.Query<T_KUPDOB>(
                "SELECT * " +
                "FROM T_KUPDOB " +
                "INNER JOIN DID_Lokacija ON T_KUPDOB.SIFRA = DID_Lokacija.SAN_KD_SIFRA " +
                "INNER JOIN DID_RadniNalog_Lokacija ON DID_Lokacija.SAN_Id = DID_RadniNalog_Lokacija.Lokacija " +
                "WHERE DID_RadniNalog_Lokacija.RadniNalog = ? " +
                "GROUP BY SIFRA, TIP_PARTNERA, NAZIV, OIB", radniNaloziFiltered[e].Id);

            if (komitenti.Any())
                intent = new Intent(this, typeof(Activity_Komitenti));
            else
                intent = new Intent(this, typeof(Activity_NoviKomitent));
            StartActivity(intent);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.TitleFormatted.ToString() == "Početna")
                intent = new Intent(this, typeof(Activity_Pocetna));
            else if (item.TitleFormatted.ToString() == "Radni nalozi")
                intent = new Intent(this, typeof(Activity_RadniNalozi));
            else if (item.TitleFormatted.ToString() == "Prikaz odradenih anketa")
                intent = new Intent(this, typeof(Activity_OdradeneAnkete));
            else if (item.TitleFormatted.ToString() == "Prikaz potrošenih materijala")
                intent = new Intent(this, typeof(Activity_PotroseniMaterijali));

            StartActivity(intent);
            return base.OnOptionsItemSelected(item);
        }
    }
}