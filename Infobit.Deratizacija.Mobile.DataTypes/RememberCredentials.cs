using System.Runtime.Serialization;

namespace Infobit.DDD.Data
{
    public class RememberCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public RememberCredentials() { }
    }
}
