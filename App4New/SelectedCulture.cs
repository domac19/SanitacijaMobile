using System.Globalization;
using System.Threading;
using Android.App;

namespace App4New
{
    public class SelectedCulture : Activity
    {
        protected override void OnResume()
        {
            base.OnResume();

            var userSelectedCulture = new CultureInfo("hr-HR");
            Thread.CurrentThread.CurrentCulture = userSelectedCulture;
        }
    }
}