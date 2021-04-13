using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Android.App;
using Android.Content;
using Newtonsoft.Json;
using SQLite;


namespace Infobit.DDD.Data
{
    public class Database
    {
        public const int CURRENT_DATABASE_SCHEMA_VERSION = 1;
        static readonly string db_name = "database.sqlite";
        static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static readonly string db_path = Path.Combine(folderPath, db_name);
        static readonly SQLiteConnection db = new SQLiteConnection(db_path);

        static readonly ISharedPreferences localUsername = Application.Context.GetSharedPreferences("userLogin", FileCreationMode.Private);
        static ISharedPreferencesEditor usernameEdit = localUsername.Edit();

        static DeratizacijaSyncData SyncDataGet;

        public static SQLiteConnection GetDBConnection()
        {
            // Postavljanje baze na pocetni stage -> brisanje svih podataka
            //SetDatabaseToVersion(0);

            UpgradeDatabaseIfNecessary();   
            return db;
        }

        private static void SetDatabaseToVersion(int version)
        {
            db.Execute("PRAGMA user_version = " + version);
        }

        private static int GetDatabaseVersion()
        {
            var version = db.ExecuteScalar<int>("PRAGMA user_version");

            if(version == 0)
            {
                usernameEdit.PutBoolean("prvoPokretanje", true);
                usernameEdit.Commit();
            }

            return version;
        }

        private static void UpgradeDatabaseIfNecessary()
        {
            int currentDbVersion = GetDatabaseVersion();
            if (currentDbVersion < CURRENT_DATABASE_SCHEMA_VERSION)
            {
                int startUpgradingFrom = currentDbVersion + 1;

                switch (startUpgradingFrom)
                {
                    //case 1:
                    //    Startposition();
                    //    goto case 2;
                    //case 2:
                    //    UpdateFrom1To2();
                    //    break;

                    case 1:
                        Startposition();
                        break;
                    default:
                        throw new Exception("Greška servera!");
                    
                }
                SetDatabaseToVersion(CURRENT_DATABASE_SCHEMA_VERSION);
            }
        }


        #region Startposition()

        private static void Startposition()
        {
            CreateTables();
            DeleteData();
            GetCredentials();
            UpdateDataBase();
        }

        private static void UpdateDataBase()
        {
            foreach (var djelatnik in SyncDataGet.Djelatnici)
            {
                try
                {
                    db.Execute(
                    "INSERT INTO KE_DJELATNICI ( " +
                        "KE_MBR, " +
                        "KE_IME, " +
                        "KE_PREZIME, " +
                        "KE_MOBITEL, " +
                        "KE_OIB, " +
                        "KE_EMAIL, " +
                        "KE_ADRESA, " +
                        "KE_POSTA, " +
                        "KE_MJESTO) " +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        djelatnik.KE_MBR,
                        djelatnik.KE_IME,
                        djelatnik.KE_PREZIME,
                        djelatnik.KE_MOBITEL,
                        djelatnik.KE_OIB,
                        djelatnik.KE_EMAIL,
                        djelatnik.KE_ADRESA,
                        djelatnik.KE_POSTA,
                        djelatnik.KE_MJESTO);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE KE_DJELATNICI " +
                        "SET KE_IME = ?, " +
                            "KE_PREZIME = ?, " +
                            "KE_MOBITEL = ?, " +
                            "KE_OIB = ?, " +
                            "KE_EMAIL = ?, " +
                            "KE_ADRESA = ?, " +
                            "KE_POSTA = ?, " +
                            "KE_MJESTO = ? " +
                        "WHERE KE_MBR = ?",
                            djelatnik.KE_IME,
                            djelatnik.KE_PREZIME,
                            djelatnik.KE_MOBITEL,
                            djelatnik.KE_OIB,
                            djelatnik.KE_EMAIL,
                            djelatnik.KE_ADRESA,
                            djelatnik.KE_POSTA,
                            djelatnik.KE_MJESTO,
                            djelatnik.KE_MBR);
                }
            }

            foreach (var djelatnikUsername in SyncDataGet.DjelatniciUsername)
            {
                //var password = SecurePasswordHasher.Hash(djelatnikUsername.Password);

                try
                {
                    db.Execute(
                        "INSERT INTO DID_DjelatnikUsername (" +
                            "Djelatnik, " +
                            "Username, " +
                            "Password)" +
                        "VALUES (?, ?, ?)",
                            djelatnikUsername.Djelatnik,
                            djelatnikUsername.Username,
                            djelatnikUsername.Password);
                }
                catch (Exception e)
                {
                    db.Execute(
                        "UPDATE DID_DjelatnikUsername " +
                        "SET Username = ?, " +
                            "Password = ? " +
                        "WHERE Djelatnik = ?",
                            djelatnikUsername.Username,
                            djelatnikUsername.Password,
                            djelatnikUsername.Djelatnik);
                }
            }
        }

        private static void GetCredentials()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 180);
                    string requestUrl = "http://services.infobit.info/DDD_Test/SyncService.svc/get/korisnici";
                    //string requestUrl = "http://services.infobit.info/DDD/SyncService.svc/get/korisnici";
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = client.GetAsync(requestUrl).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string output = response.Content.ReadAsStringAsync().Result;
                        SyncDataGet = JsonConvert.DeserializeObject<DeratizacijaSyncData>(JsonConvert.DeserializeObject<string>(output));
                        if (SyncDataGet.Error != null)
                        {
                            throw new CallServiceExeption("Greska servera!");
                        }
                    }
                    else
                    {
                        throw new CallServiceExeption("Greska servera!");
                    }
                }
            }
            catch (Exception e)
            {
                throw new CallServiceExeption(e.Message);
            }
        }

        public class CallServiceExeption : Exception
        {
            public CallServiceExeption(string message)
               : base(message)
            {
            }
        }

        private static void CreateTables()
        {
            // Kreiranje tablice ako ne postoji
            db.CreateTable<DID_RadniNalog>();
            db.CreateTable<DID_RadniNalog_Djelatnik>();
            db.CreateTable<DID_RadniNalog_Lokacija>();
            db.CreateTable<DID_Anketa>();
            db.CreateTable<DID_AnketaMaterijali>();
            db.CreateTable<DID_RadniNalog_Materijal>();
            db.CreateTable<DID_StanjeSkladista>();
            db.CreateTable<DID_Potvrda>();
            db.CreateTable<DID_Potvrda_Materijal>();
            db.CreateTable<DID_Potvrda_Nametnik>();
            db.CreateTable<DID_Potvrda_Djelatnost>();
            db.CreateTable<RememberCredentials>();
            db.CreateTable<DID_ProvedbeniPlan>();
            db.CreateTable<DID_ProvedbeniPlan_Materijal>();
            db.CreateTable<DID_ProvedbeniPlan_Nametnik>();

            //Matični podaci
            db.CreateTable<KE_DJELATNICI>();
            db.CreateTable<DID_DjelatnikUsername>();
            db.CreateTable<DID_Lokacija>();
            db.CreateTable<DID_LokacijaPozicija>();
            db.CreateTable<T_KUPDOB>();
            db.CreateTable<DID_Djelatnost>();
            db.CreateTable<DID_Nametnik>();
            db.CreateTable<T_SKL>();
            db.CreateTable<T_NAZR>();
            db.CreateTable<T_GRU>();
            db.CreateTable<T_MjerneJedinice>();
            db.CreateTable<DID_TipLokacije>();
            db.CreateTable<DID_TipPosla>();
            db.CreateTable<DID_TipDeratizacijskeKutije>();
            db.CreateTable<DID_RazinaInfestacije>();
            db.CreateTable<DID_RazlogNeizvrsenjaDeratizacije>();
            db.CreateTable<T_ZUPANIJE>();
            db.CreateTable<T_OPCINE>();
            db.CreateTable<T_NASELJA>();
            db.CreateTable<DID_StatusRadnogNaloga>();
            db.CreateTable<DID_StatusLokacije>();
            db.CreateTable<DID_StatusLokacije_RadniNalog>();
        }

        private static void DeleteData()
        {
            // Brisanje svih podataka
            db.DeleteAll<DID_AnketaMaterijali>();
            db.DeleteAll<DID_RadniNalog>();
            db.DeleteAll<DID_RadniNalog_Djelatnik>();
            db.DeleteAll<DID_RadniNalog_Lokacija>();
            db.DeleteAll<DID_Anketa>();
            db.DeleteAll<DID_RadniNalog_Materijal>();
            db.DeleteAll<DID_StanjeSkladista>();
            db.DeleteAll<DID_Potvrda>();
            db.DeleteAll<DID_Potvrda_Materijal>();
            db.DeleteAll<DID_Potvrda_Nametnik>();
            db.DeleteAll<DID_Potvrda_Djelatnost>();
            db.DeleteAll<RememberCredentials>();
            db.DeleteAll<DID_ProvedbeniPlan>();
            db.DeleteAll<DID_ProvedbeniPlan_Materijal>();
            db.DeleteAll<DID_ProvedbeniPlan_Nametnik>();

            //Matični podaci
            db.DeleteAll<KE_DJELATNICI>();
            db.DeleteAll<DID_DjelatnikUsername>();
            db.DeleteAll<DID_Lokacija>();
            db.DeleteAll<DID_LokacijaPozicija>();
            db.DeleteAll<T_KUPDOB>();
            db.DeleteAll<DID_Djelatnost>();
            db.DeleteAll<DID_Nametnik>();
            db.DeleteAll<T_SKL>();
            db.DeleteAll<T_NAZR>();
            db.DeleteAll<T_GRU>();
            db.DeleteAll<T_MjerneJedinice>();
            db.DeleteAll<DID_TipLokacije>();
            db.DeleteAll<DID_TipPosla>();
            db.DeleteAll<DID_TipDeratizacijskeKutije>();
            db.DeleteAll<DID_RazinaInfestacije>();
            db.DeleteAll<DID_RazlogNeizvrsenjaDeratizacije>();
            db.DeleteAll<T_ZUPANIJE>();
            db.DeleteAll<T_OPCINE>();
            db.DeleteAll<T_NASELJA>();
            db.DeleteAll<DID_StatusRadnogNaloga>();
            db.DeleteAll<DID_StatusLokacije>();
            db.DeleteAll<DID_StatusLokacije_RadniNalog>();
        }

        #endregion

        //private static void UpdateFrom1To2()
        //{
        //}
    }
}
