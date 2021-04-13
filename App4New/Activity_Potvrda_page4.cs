using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Infobit.DDD.Data;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Content.PM;

namespace App4New
{
    [Activity(Label = "Activity_Potvrda_page4", Theme = "@style/Base.Theme.DesignDemo", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Activity_Potvrda_page4 : Activity
    {
        Intent intent;
        ListView materijaliListView;
        TextView ukupanIznosTextView;
        Button izdajPotvrdu;
        List<DID_AnketaMaterijali> filtriranePotrosnje;
        decimal ukupanIznos;
        int lokacijaId, radniNalog, radniNalogLokacijaId;

        static readonly ISharedPreferences localKomitentLokacija = Application.Context.GetSharedPreferences("localKomitentLokacija", FileCreationMode.Private);
        static readonly ISharedPreferences localMaterijali = Application.Context.GetSharedPreferences("localMaterijali", FileCreationMode.Private);
        static readonly ISharedPreferences localPotvrda = Application.Context.GetSharedPreferences("potvrda", FileCreationMode.Private);
        static readonly ISharedPreferences localRadniNalozi = Application.Context.GetSharedPreferences("radniNalozi", FileCreationMode.Private);

        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.potvrda_page4);
            Android.Widget.Toolbar toolbar = FindViewById<Android.Widget.Toolbar>(Resource.Id.toolbarHomePage);
            materijaliListView = FindViewById<ListView>(Resource.Id.materijaliListView);
            ukupanIznosTextView = FindViewById<TextView>(Resource.Id.ukupanIznosTextView);
            izdajPotvrdu = FindViewById<Button>(Resource.Id.izdajPotvrdu);

            SetActionBar(toolbar);
            ActionBar.Title = "Potvrda";
            izdajPotvrdu.Click += IzdajPotvrdu_Click;
            lokacijaId = localKomitentLokacija.GetInt("lokacijaId", 0);
            ukupanIznos = 0;
            radniNalog = localRadniNalozi.GetInt("id", 0);
            radniNalogLokacijaId = localKomitentLokacija.GetInt("radniNalogLokacijaId", 0);
            List<DID_Potvrda> potvrde = db.Query<DID_Potvrda>(
                "SELECT * " +
                "FROM DID_Potvrda " +
                "WHERE RadniNalog = ? " +
                "AND Lokacija = ?", radniNalog, lokacijaId);

            //odi triba prikazati materijale pojedinacno te zbrojiti kolicine
            filtriranePotrosnje = db.Query<DID_AnketaMaterijali>(
                "SELECT mat.Id, mat.Cijena, mat.LokacijaId, TOTAL(mat.Iznos) AS Iznos, mat.RadniNalog, mat.PozicijaId, mat.MaterijalSifra, mat.MaterijalNaziv, mat.MjernaJedinica, TOTAL(mat.Kolicina) AS Kolicina " +
                "FROM DID_AnketaMaterijali mat " +
                "INNER JOIN DID_LokacijaPozicija poz ON mat.PozicijaId = poz.POZ_Id " +
                "WHERE poz.SAN_Id = ? " +
                "AND mat.RadniNalog = ? " +
                "GROUP BY mat.MaterijalNaziv", lokacijaId, radniNalog);

            foreach (var materijal in filtriranePotrosnje)
                ukupanIznos += Math.Round(materijal.Iznos, 2);
            ukupanIznosTextView.Text = Math.Round(ukupanIznos, 2).ToString("F2").Replace('.', ',') + " kn";
            materijaliListView.Adapter = new Adapter_MaterijaliPotvrda(this, filtriranePotrosnje);
        }

        public void IzdajPotvrdu_Click(object sender, EventArgs args)
        {
            // DEFAULT DATA
            int id = localPotvrda.GetInt("id", 0);
            int radniNalog = localRadniNalozi.GetInt("id", 0);
            bool edit = localPotvrda.GetBoolean("edit", false);

            // PAGE 1
            string godina = localPotvrda.GetString("godina", null);
            DateTime datum = Convert.ToDateTime(localPotvrda.GetString("datumPotvrde", null));
            string potvrdaBrInput = localPotvrda.GetString("potvrdaBrInput", null);
            string opisRadaInput = localPotvrda.GetString("opisRadaInput", null);
            string razinaInfestacije = localPotvrda.GetString("razinaInfestacije", null);

            int lokacijaTip2 = localPotvrda.GetInt("lokacijaTip2", 0);
            int lokacijaTip2BrojObjekata = localPotvrda.GetInt("lokacijaTip2BrojObjekata", 0);
            bool lokacijaTip2Odraden = localPotvrda.GetBoolean("lokacijaTip2Odraden", false);
            int lokacijaTip1 = localPotvrda.GetInt("lokacijaTip1", 0);
            int lokacijaTipBrojObjekata = localPotvrda.GetInt("lokacijaTipBrojObjekata", 0);
            bool lokacijaTip1Odraden = localPotvrda.GetBoolean("lokacijaTip1Odraden", false);

            // PAGE 2
            bool deratizacijaBtn = localPotvrda.GetBoolean("deratizacijaBtn", false);
            bool dezinsekcijaBtn = localPotvrda.GetBoolean("dezinsekcijaBtn", false);
            bool dezinfekcijaBtn = localPotvrda.GetBoolean("dezinfekcijaBtn", false);
            bool dezodorizacijaBtn = localPotvrda.GetBoolean("dezodorizacijaBtn", false);
            bool zastitaBiljaBtn = localPotvrda.GetBoolean("zastitaBiljaBtn", false);
            bool suzbijanjeStetnikaBtn = localPotvrda.GetBoolean("suzbijanjeStetnikaBtn", false);
            bool KIInsektiBtn = localPotvrda.GetBoolean("KIInsektiBtn", false);
            bool KIGlodavciBtn = localPotvrda.GetBoolean("KIGlodavciBtn", false);
            bool uzimanjeBrisevaBtn = localPotvrda.GetBoolean("uzimanjeBrisevaBtn", false);
            bool suzbijanjeKorovaBtn = localPotvrda.GetBoolean("suzbijanjeKorovaBtn", false);
            bool kosnjaTraveBtn = localPotvrda.GetBoolean("kosnjaTraveBtn", false);
            bool tretman2 = localPotvrda.GetBoolean("tretman2", false);
            bool tretman = localPotvrda.GetBoolean("tretman", false);
            bool postavljanjeLovki = localPotvrda.GetBoolean("postavljanjeLovki", false);
            bool kontrola = localPotvrda.GetBoolean("kontrola", false);
            bool postavljanjeMaterijala = localPotvrda.GetBoolean("postavljanjeMaterijala", false);

            int brObjekataDeratizacija = !String.IsNullOrEmpty(localPotvrda.GetString("brObjekataDeratizacija", null)) ? Convert.ToInt32(localPotvrda.GetString("brObjekataDeratizacija", null)) : 0;
            int brObjekataDezinsekcija = !String.IsNullOrEmpty(localPotvrda.GetString("brObjekataDezinsekcija", null)) ? Convert.ToInt32(localPotvrda.GetString("brObjekataDezinsekcija", null)) : 0;
            int brObjekataDezinfekcija = !String.IsNullOrEmpty(localPotvrda.GetString("brObjekataDezinfekcija", null)) ? Convert.ToInt32(localPotvrda.GetString("brObjekataDezinfekcija", null)) : 0;
            int brObjekataZastitaBilja = !String.IsNullOrEmpty(localPotvrda.GetString("brObjekataZastitaBilja", null)) ? Convert.ToInt32(localPotvrda.GetString("brObjekataZastitaBilja", null)) : 0;
            int brObjekataDezodorizacija = !String.IsNullOrEmpty(localPotvrda.GetString("brObjekataDezodorizacija", null)) ? Convert.ToInt32(localPotvrda.GetString("brObjekataDezodorizacija", null)) : 0;
            int brObjekataSuzbijanjeStetnika = !String.IsNullOrEmpty(localPotvrda.GetString("brObjekataSuzbijanjeStetnika", null)) ? Convert.ToInt32(localPotvrda.GetString("brObjekataSuzbijanjeStetnika", null)) : 0;
            int brObjekataKIInsekti = !String.IsNullOrEmpty(localPotvrda.GetString("brObjekataKIInsekti", null)) ? Convert.ToInt32(localPotvrda.GetString("brObjekataKIInsekti", null)) : 0;
            int brObjekataKIGlodavci = !String.IsNullOrEmpty(localPotvrda.GetString("brObjekataKIGlodavci", null)) ? Convert.ToInt32(localPotvrda.GetString("brObjekataKIGlodavci", null)) : 0;
            int brObjekataUzimanjeBriseva = !String.IsNullOrEmpty(localPotvrda.GetString("brObjekataUzimanjeBriseva", null)) ? Convert.ToInt32(localPotvrda.GetString("brObjekataUzimanjeBriseva", null)) : 0;
            int brObjekataSuzbijanjeKorova = !String.IsNullOrEmpty(localPotvrda.GetString("brObjekataSuzbijanjeKorova", null)) ? Convert.ToInt32(localPotvrda.GetString("brObjekataSuzbijanjeKorova", null)) : 0;
            int brObjekatakosnjaTrave = !String.IsNullOrEmpty(localPotvrda.GetString("brObjekatakosnjaTrave", null)) ? Convert.ToInt32(localPotvrda.GetString("brObjekatakosnjaTrave", null)) : 0;
            int brObjekataDeratizacija2 = !String.IsNullOrEmpty(localPotvrda.GetString("brObjekataDeratizacija2", null)) ? Convert.ToInt32(localPotvrda.GetString("brObjekataDeratizacija2", null)) : 0;
            int brObjekataDezinsekcija2 = !String.IsNullOrEmpty(localPotvrda.GetString("brObjekataDezinsekcija2", null)) ? Convert.ToInt32(localPotvrda.GetString("brObjekataDezinsekcija2", null)) : 0;
            int brObjekataDezinfekcija2 = !String.IsNullOrEmpty(localPotvrda.GetString("brObjekataDezinfekcija2", null)) ? Convert.ToInt32(localPotvrda.GetString("brObjekataDezinfekcija2", null)) : 0;
            int brObjekataZastitaBilja2 = !String.IsNullOrEmpty(localPotvrda.GetString("brObjekataZastitaBilja2", null)) ? Convert.ToInt32(localPotvrda.GetString("brObjekataZastitaBilja2", null)) : 0;
            int brObjekataDezodorizacija2 = !String.IsNullOrEmpty(localPotvrda.GetString("brObjekataDezodorizacija2", null)) ? Convert.ToInt32(localPotvrda.GetString("brObjekataDezodorizacija2", null)) : 0;
            int brObjekataSuzbijanjeStetnika2 = !String.IsNullOrEmpty(localPotvrda.GetString("brObjekataSuzbijanjeStetnika2", null)) ? Convert.ToInt32(localPotvrda.GetString("brObjekataSuzbijanjeStetnika2", null)) : 0;
            int brObjekataKIInsekti2 = !String.IsNullOrEmpty(localPotvrda.GetString("brObjekataKIInsekti2", null)) ? Convert.ToInt32(localPotvrda.GetString("brObjekataKIInsekti2", null)) : 0;
            int brObjekataKIGlodavci2 = !String.IsNullOrEmpty(localPotvrda.GetString("brObjekataKIGlodavci2", null)) ? Convert.ToInt32(localPotvrda.GetString("brObjekataKIGlodavci2", null)) : 0;
            int brObjekataUzimanjeBriseva2 = !String.IsNullOrEmpty(localPotvrda.GetString("brObjekataUzimanjeBriseva2", null)) ? Convert.ToInt32(localPotvrda.GetString("brObjekataUzimanjeBriseva2", null)) : 0;
            int brObjekataSuzbijanjeKorova2 = !String.IsNullOrEmpty(localPotvrda.GetString("brObjekataSuzbijanjeKorova2", null)) ? Convert.ToInt32(localPotvrda.GetString("brObjekataSuzbijanjeKorova2", null)) : 0;
            int brObjekatakosnjaTrave2 = !String.IsNullOrEmpty(localPotvrda.GetString("brObjekatakosnjaTrave2", null)) ? Convert.ToInt32(localPotvrda.GetString("brObjekatakosnjaTrave2", null)) : 0;

        #region nesto 

            // PAGE 3
            int infestacija = db.Query<DID_RazinaInfestacije>(
                "SELECT * " +
                "FROM DID_RazinaInfestacije " +
                "WHERE Naziv = ?", razinaInfestacije).FirstOrDefault().Sifra;

            //Update Statusa Radnog naloga i lokacije te statusa sinkronizacije
            db.Execute(
                "UPDATE DID_RadniNalog_Lokacija " +
                "SET SinhronizacijaStatus = 0, " +
                "Status = 2 " +
                "WHERE Id = ?", radniNalogLokacijaId);

            List<DID_RadniNalog_Lokacija> izvrseneLokacije = db.Query<DID_RadniNalog_Lokacija>(
                "SELECT * " +
                "FROM DID_RadniNalog_Lokacija " +
                "WHERE RadniNalog = ? " +
                "AND Status = 3", radniNalog);

            if (izvrseneLokacije.Any())
                db.Query<DID_RadniNalog>(
                    "UPDATE DID_RadniNalog " +
                    "SET Status = 4, " +
                    "SinhronizacijaStatus = 1 " +
                    "WHERE Id = ?", radniNalog);
            else
                db.Query<DID_RadniNalog>(
                    "UPDATE DID_RadniNalog " +
                    "SET Status = 3, " +
                    "SinhronizacijaStatus = 0 " +
                    "WHERE Id = ?", radniNalog);

            if(id == 0)
            {
                List<DID_Potvrda> potvrde = db.Query<DID_Potvrda>(
                    "SELECT * " +
                    "FROM DID_Potvrda " +
                    "WHERE Id < 0");

                if (potvrde.Any())
                    id = potvrde.FirstOrDefault().Id - 1;
                else
                    id = -1;
            }
           
            if (edit)
            {
                //Update potvrde
                db.Query<DID_Potvrda>(
                    "UPDATE DID_Potvrda " +
                    "SET Godina = ?, " +
                        "Broj = ?, " +
                        "RadniNalog = ?, " +
                        "Lokacija = ?, " +
                        "DatumVrijeme = ?, " +
                        "Infestacija = ?, " +
                        "OpisRada = ?, " +
                        "RadniNalogLokacijaId = ?, " +
                        "LokacijaTip = ?, " +
                        "LokacijaTipOdradjen = ?, " +
                        "LokacijaTipBrojObjekata = ?, " +
                        "LokacijaTip2 = ?, " +
                        "LokacijaTip2Odradjen = ?, " +
                        "LokacijaTip2BrojObjekata = ? " +
                    "WHERE Id = ?",
                        godina,
                        potvrdaBrInput,
                        radniNalog,
                        lokacijaId,
                        datum,
                        infestacija,
                        opisRadaInput,
                        radniNalogLokacijaId,
                        lokacijaTip1,
                        lokacijaTip1Odraden,
                        lokacijaTipBrojObjekata,
                        lokacijaTip2,
                        lokacijaTip2Odraden,
                        lokacijaTip2BrojObjekata,
                        id
                    );

                //izbrisi sve potvrde djelatnosti i nametnike (ovo trenutno funcionira da
                //se moze za lokaciju samo jednom odraditi potvrda)
                db.Query<DID_Potvrda_Djelatnost>(
                   "DELETE FROM DID_Potvrda_Djelatnost " +
                   "WHERE Potvrda = ?", id);

                db.Query<DID_Potvrda_Nametnik>(
                   "DELETE FROM DID_Potvrda_Nametnik " +
                   "WHERE Potvrda = ?", id);
            }
            else
            {
                //spremanje nove potvrde
                db.Query<DID_Potvrda>(
                    "INSERT INTO DID_Potvrda (" +
                        "Id, " +
                        "Godina, " +
                        "Broj, " +
                        "RadniNalog, " +
                        "Lokacija, " +
                        "DatumVrijeme, " +
                        "Infestacija, " +
                        "OpisRada, " +
                        "RadniNalogLokacijaId, " +
                        "LokacijaTip, " +
                        "LokacijaTipOdradjen, " +
                        "LokacijaTipBrojObjekata, " +
                        "LokacijaTip2, " +
                        "LokacijaTip2Odradjen, " +
                        "LokacijaTip2BrojObjekata, " +
                        "SinhronizacijaPrivremeniKljuc)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        id,
                        godina,
                        potvrdaBrInput,
                        radniNalog,
                        lokacijaId,
                        datum,
                        infestacija,
                        opisRadaInput,
                        radniNalogLokacijaId,
                        lokacijaTip1,
                        lokacijaTip1Odraden,
                        lokacijaTipBrojObjekata,
                        lokacijaTip2,
                        lokacijaTip2Odraden,
                        lokacijaTip2BrojObjekata,
                        id
                    );
            }

            //odi promini da se pogleda filteredMaterijali
            db.Execute(
                "DELETE FROM DID_Potvrda_Materijal " +
                "WHERE Potvrda = ?", id);

            db.Execute(
                "INSERT INTO DID_Potvrda_Materijal (Potvrda, Materijal, Utroseno, MaterijalNaziv) " +
                "SELECT pot.Id, mat.MaterijalSifra, TOTAL(mat.Kolicina), mat.MaterijalNaziv " +
                "FROM DID_AnketaMaterijali mat " +
                "INNER JOIN DID_LokacijaPozicija poz ON poz.POZ_Id = mat.PozicijaId " +
                "INNER JOIN DID_Potvrda pot ON pot.RadniNalog = mat.RadniNalog " +
                "AND pot.Lokacija = poz.SAN_Id " +
                "WHERE pot.Id = ? " +
                "GROUP BY mat.MaterijalSifra, mat.MjernaJedinica", id);

            var listaMaterijalaPotvrda = db.Query<DID_Potvrda_Materijal>("SELECT * FROM DID_Potvrda_Materijal WHERE Potvrda = ?", id);
            foreach (var materijal in listaMaterijalaPotvrda)
            {
                db.Execute(
                    "UPDATE DID_Potvrda_Materijal " +
                    "SET SinhronizacijaPrivremeniKljuc = ? " +
                    "WHERE Id = ?", materijal.Id, materijal.Id);
            }

        #endregion

        #region comment

            int djelatnostId = -1;
            List<DID_Potvrda_Djelatnost> djelatnosti = db.Query<DID_Potvrda_Djelatnost>(
                "SELECT * " +
                "FROM DID_Potvrda_Djelatnost " +
                "WHERE Id < 0");
            if (djelatnosti.Any())
                djelatnostId = djelatnosti.FirstOrDefault().Id - 1;


            if (deratizacijaBtn && !postavljanjeMaterijala && !kontrola)
            {
                db.Query<DID_Potvrda_Djelatnost>(
                    "INSERT INTO DID_Potvrda_Djelatnost (" +
                        "Id, " +
                        "Potvrda, " +
                        "Djelatnost, " +
                        "BrojObjekata, " +
                        "ObjektiTip1, " +
                        "ObjektiTip2, " +
                        "BrojObjekataTip2, " +
                        "SinhronizacijaPrivremeniKljuc)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                        djelatnostId,
                        id,
                        1,
                        brObjekataDeratizacija,
                        brObjekataDeratizacija > 0 ? true : false,
                        brObjekataDeratizacija2 > 0 ? true : false,
                        brObjekataDeratizacija2,
                        djelatnostId--
                    );
            }
            else
            {
                if (deratizacijaBtn && postavljanjeMaterijala)
                {
                    db.Query<DID_Potvrda_Djelatnost>(
                        "INSERT INTO DID_Potvrda_Djelatnost (" +
                            "Id, " +
                            "Potvrda, " +
                            "Djelatnost," +
                            "TipPosla, " +
                            "BrojObjekata, " +
                            "ObjektiTip1, " +
                            "ObjektiTip2, " +
                            "BrojObjekataTip2, " +
                            "SinhronizacijaPrivremeniKljuc)" +
                        "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)",
                            djelatnostId,
                            id,
                            1,
                            1,
                            brObjekataDeratizacija,
                            brObjekataDeratizacija > 0 ? true : false,
                            brObjekataDeratizacija2 > 0 ? true : false,
                            brObjekataDeratizacija2,
                            djelatnostId--
                        );
                }
                if (deratizacijaBtn && kontrola)
                {
                    db.Query<DID_Potvrda_Djelatnost>(
                        "INSERT INTO DID_Potvrda_Djelatnost (" +
                            "Id, " +
                            "Potvrda, " +
                            "Djelatnost," +
                            "TipPosla, " +
                            "BrojObjekata, " +
                            "ObjektiTip1, " +
                            "ObjektiTip2, " +
                            "BrojObjekataTip2, " +
                            "SinhronizacijaPrivremeniKljuc)" +
                        "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)",
                            djelatnostId,
                            id,
                            1,
                            2,
                            brObjekataDeratizacija,
                            brObjekataDeratizacija > 0 ? true : false,
                            brObjekataDeratizacija2 > 0 ? true : false,
                            brObjekataDeratizacija2,
                            djelatnostId--
                        );
                }
            }
            if (dezinsekcijaBtn && !postavljanjeLovki)
            {
                db.Query<DID_Potvrda_Djelatnost>(
                    "INSERT INTO DID_Potvrda_Djelatnost (" +
                        "Id, " +
                        "Potvrda, " +
                        "Djelatnost, " +
                        "BrojObjekata, " +
                        "ObjektiTip1, " +
                        "ObjektiTip2, " +
                        "BrojObjekataTip2, " +
                        "SinhronizacijaPrivremeniKljuc)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                        djelatnostId,
                        id,
                        2,
                        brObjekataDezinsekcija,
                        brObjekataDezinsekcija > 0 ? true : false,
                        brObjekataDezinsekcija2 > 0 ? true : false,
                        brObjekataDezinsekcija2,
                        djelatnostId--
                    );
            }
            else if (dezinsekcijaBtn && postavljanjeLovki)
            {
                db.Query<DID_Potvrda_Djelatnost>(
                    "INSERT INTO DID_Potvrda_Djelatnost (" +
                        "Id, " +
                        "Potvrda, " +
                        "Djelatnost," +
                        "TipPosla, " +
                        "BrojObjekata, " +
                        "ObjektiTip1, " +
                        "ObjektiTip2, " +
                        "BrojObjekataTip2, " +
                        "SinhronizacijaPrivremeniKljuc)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        djelatnostId,
                        id,
                        2,
                        3,
                        brObjekataDezinsekcija,
                        brObjekataDezinsekcija > 0 ? true : false,
                        brObjekataDezinsekcija2 > 0 ? true : false,
                        brObjekataDezinsekcija2,
                        djelatnostId--
                    );
            }
            if (dezinfekcijaBtn && !tretman)
            {
                db.Query<DID_Potvrda_Djelatnost>(
                     "INSERT INTO DID_Potvrda_Djelatnost (" +
                        "Id, " +
                        "Potvrda, " +
                        "Djelatnost, " +
                        "BrojObjekata, " +
                        "ObjektiTip1, " +
                        "ObjektiTip2, " +
                        "BrojObjekataTip2, " +
                        "SinhronizacijaPrivremeniKljuc)" +
                     "VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                        djelatnostId,
                        id,
                        3,
                        brObjekataDezinfekcija,
                        brObjekataDezinfekcija > 0 ? true : false,
                        brObjekataDezinfekcija2 > 0 ? true : false,
                        brObjekataDezinfekcija2,
                        djelatnostId--
                     );
            }
            else if (dezinfekcijaBtn && tretman)
            {
                db.Query<DID_Potvrda_Djelatnost>(
                    "INSERT INTO DID_Potvrda_Djelatnost (" +
                        "Id, " +
                        "Potvrda, " +
                        "Djelatnost," +
                        "TipPosla, " +
                        "BrojObjekata, " +
                        "ObjektiTip1, " +
                        "ObjektiTip2, " +
                        "BrojObjekataTip2, " +
                        "SinhronizacijaPrivremeniKljuc)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        djelatnostId,
                        id,
                        3,
                        4,
                        brObjekataDezinfekcija,
                        brObjekataDezinfekcija > 0 ? true : false,
                        brObjekataDezinfekcija2 > 0 ? true : false,
                        brObjekataDezinfekcija2,
                        djelatnostId--
                    );
            }
            if (dezodorizacijaBtn)
            {
                db.Query<DID_Potvrda_Djelatnost>(
                    "INSERT INTO DID_Potvrda_Djelatnost (" +
                        "Id, " +
                        "Potvrda, " +
                        "Djelatnost, " +
                        "BrojObjekata, " +
                        "ObjektiTip1, " +
                        "ObjektiTip2, " +
                        "BrojObjekataTip2, " +
                        "SinhronizacijaPrivremeniKljuc)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                        djelatnostId,
                        id,
                        4,
                        brObjekataDezodorizacija,
                        brObjekataDezodorizacija > 0 ? true : false,
                        brObjekataDezodorizacija2 > 0 ? true : false,
                        brObjekataDezodorizacija2,
                        djelatnostId--
                    );
            }
            if (zastitaBiljaBtn && !tretman2)
            {
                db.Query<DID_Potvrda_Djelatnost>(
                    "INSERT INTO DID_Potvrda_Djelatnost (" +
                        "Id, " +
                        "Potvrda, " +
                        "Djelatnost, " +
                        "BrojObjekata, " +
                        "ObjektiTip1, " +
                        "ObjektiTip2, " +
                        "BrojObjekataTip2, " +
                        "SinhronizacijaPrivremeniKljuc)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                        djelatnostId,
                        id,
                        5,
                        brObjekataZastitaBilja,
                        brObjekataZastitaBilja > 0 ? true : false,
                        brObjekataZastitaBilja2 > 0 ? true : false,
                        brObjekataZastitaBilja2,
                        djelatnostId--
                    );
            }
            else if (zastitaBiljaBtn && tretman2)
            {
                db.Query<DID_Potvrda_Djelatnost>(
                    "INSERT INTO DID_Potvrda_Djelatnost (" +
                        "Id, " +
                        "Potvrda, " +
                        "Djelatnost," +
                        "TipPosla, " +
                        "BrojObjekata, " +
                        "ObjektiTip1, " +
                        "ObjektiTip2, " +
                        "BrojObjekataTip2, " +
                        "SinhronizacijaPrivremeniKljuc)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        djelatnostId,
                        id,
                        5,
                        6,
                        brObjekataZastitaBilja,
                        brObjekataZastitaBilja > 0 ? true : false,
                        brObjekataZastitaBilja2 > 0 ? true : false,
                        brObjekataZastitaBilja2,
                        djelatnostId--
                    );
            }
            if (suzbijanjeStetnikaBtn)
            {
                db.Query<DID_Potvrda_Djelatnost>(
                    "INSERT INTO DID_Potvrda_Djelatnost (" +
                        "Id, " +
                        "Potvrda, " +
                        "Djelatnost, " +
                        "BrojObjekata, " +
                        "ObjektiTip1, " +
                        "ObjektiTip2, " +
                        "BrojObjekataTip2, " +
                        "SinhronizacijaPrivremeniKljuc)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                        djelatnostId,
                        id,
                        6,
                        brObjekataSuzbijanjeStetnika,
                        brObjekataSuzbijanjeStetnika > 0 ? true : false,
                        brObjekataSuzbijanjeStetnika2 > 0 ? true : false,
                        brObjekataSuzbijanjeStetnika2,
                        djelatnostId--
                    );
            }
            if (KIInsektiBtn)
            {
                db.Query<DID_Potvrda_Djelatnost>(
                    "INSERT INTO DID_Potvrda_Djelatnost (" +
                        "Id, " +
                        "Potvrda, " +
                        "Djelatnost, " +
                        "BrojObjekata, " +
                        "ObjektiTip1, " +
                        "ObjektiTip2, " +
                        "BrojObjekataTip2, " +
                        "SinhronizacijaPrivremeniKljuc)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                        djelatnostId,
                        id,
                        7,
                        brObjekataKIInsekti,
                        brObjekataKIInsekti > 0 ? true : false,
                        brObjekataKIInsekti2 > 0 ? true : false,
                        brObjekataKIInsekti2,
                        djelatnostId--
                    );
            }
            if (KIGlodavciBtn)
            {
                db.Query<DID_Potvrda_Djelatnost>(
                    "INSERT INTO DID_Potvrda_Djelatnost (" +
                        "Id, " +
                        "Potvrda, " +
                        "Djelatnost, " +
                        "BrojObjekata, " +
                        "ObjektiTip1, " +
                        "ObjektiTip2, " +
                        "BrojObjekataTip2, " +
                        "SinhronizacijaPrivremeniKljuc)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                        djelatnostId,
                        id,
                        8,
                        brObjekataKIGlodavci,
                        brObjekataKIGlodavci > 0 ? true : false,
                        brObjekataKIGlodavci2 > 0 ? true : false,
                        brObjekataKIGlodavci2,
                        djelatnostId--
                    );
            }
            if (uzimanjeBrisevaBtn)
            {
                db.Query<DID_Potvrda_Djelatnost>(
                    "INSERT INTO DID_Potvrda_Djelatnost (" +
                        "Id, " +
                        "Potvrda, " +
                        "Djelatnost, " +
                        "BrojObjekata, " +
                        "ObjektiTip1, " +
                        "ObjektiTip2, " +
                        "BrojObjekataTip2, " +
                        "SinhronizacijaPrivremeniKljuc)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                        djelatnostId,
                        id,
                        9,
                        brObjekataUzimanjeBriseva,
                        brObjekataUzimanjeBriseva > 0 ? true : false,
                        brObjekataUzimanjeBriseva2 > 0 ? true : false,
                        brObjekataUzimanjeBriseva2,
                        djelatnostId--
                    );
            }
            if (suzbijanjeKorovaBtn)
            {
                db.Query<DID_Potvrda_Djelatnost>(
                    "INSERT INTO DID_Potvrda_Djelatnost (" +
                        "Id, " +
                        "Potvrda, " +
                        "Djelatnost, " +
                        "BrojObjekata, " +
                        "ObjektiTip1, " +
                        "ObjektiTip2, " +
                        "BrojObjekataTip2, " +
                        "SinhronizacijaPrivremeniKljuc)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                        djelatnostId,
                        id,
                        10,
                        brObjekataSuzbijanjeKorova,
                        brObjekataSuzbijanjeKorova > 0 ? true : false,
                        brObjekataSuzbijanjeKorova2 > 0 ? true : false,
                        brObjekataSuzbijanjeKorova2,
                        djelatnostId--
                    );
            }
            if (kosnjaTraveBtn)
            {
                db.Query<DID_Potvrda_Djelatnost>(
                    "INSERT INTO DID_Potvrda_Djelatnost (" +
                        "Id, " +
                        "Potvrda, " +
                        "Djelatnost, " +
                        "BrojObjekata, " +
                        "ObjektiTip1, " +
                        "ObjektiTip2, " +
                        "BrojObjekataTip2, " +
                        "SinhronizacijaPrivremeniKljuc)" +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                        djelatnostId,
                        id,
                        11,
                        brObjekatakosnjaTrave,
                        brObjekatakosnjaTrave > 0 ? true : false,
                        brObjekatakosnjaTrave2 > 0 ? true : false,
                        brObjekatakosnjaTrave2,
                        djelatnostId--
                    );
            }
            #endregion


            // DODAVANJE NAMETNIKA NA POTVRDU NAMETNICI
            var nametniciList = db.Query<DID_Nametnik>("SELECT * FROM DID_Nametnik");
            foreach (var nametnik in nametniciList)
            {
                if (!String.IsNullOrEmpty(localPotvrda.GetString("nametnik" + nametnik.Sifra, null)))
                {
                    int nametnikId = -1;
                    var zadnjiId = db.Query<DID_Potvrda_Nametnik>(
                        "SELECT * " +
                        "FROM DID_Potvrda_Nametnik " +
                        "WHERE Id < 0 " +
                        "ORDER BY Id");

                    if (zadnjiId.Count > 0)
                        nametnikId = zadnjiId.FirstOrDefault().Id - 1;

                    db.Execute(
                       "INSERT INTO DID_Potvrda_Nametnik (" +
                           "Id, " +
                           "Potvrda, " +
                           "Nametnik, " +
                           "SinhronizacijaPrivremeniKljuc)" +
                       "VALUES (?, ?, ?, ?)",
                           nametnikId,
                           id,
                           nametnik.Sifra,
                           nametnikId);
                }
            }


            localPotvrda.Edit().Clear().Commit();
            localMaterijali.Edit().Clear().Commit();
            intent = new Intent(this, typeof(Activity_LokacijaZavrsena));
            StartActivity(intent);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menuBackHome, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.TitleFormatted.ToString() == "Početna")
                intent = new Intent(this, typeof(Activity_Pocetna));
            else if (item.TitleFormatted.ToString() == "nazad")
                intent = new Intent(this, typeof(Activity_Potvrda_page3));
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