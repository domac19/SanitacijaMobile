using System.Runtime.Serialization;

namespace Infobit.DDD.Data
{
    public class Credentials
    {
        public Credentials() { }

        public Credentials(string username, string password)
        {
            Usename = username;
            Password = password;
        }

        [DataMember]
        public string Usename { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public bool Valid = true;
    }
}
