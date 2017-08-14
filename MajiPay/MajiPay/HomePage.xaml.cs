using MajiPay.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MajiPay
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
         public HomePage()
        {
            this.InitializeComponent();
        }

        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Frame.BackStack.Clear();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (meterno.Text.Length < 6)
            {
                await UIHelper.ShowAlert("Invalid meter number");
            }
            else
            {
                await VerifyMeter();
            }
        }

        private async Task VerifyMeter()
        {
            btnsubmit.IsEnabled = false;            

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.IfModifiedSince = DateTimeOffset.Now;
            HttpResponseMessage response = await client.GetAsync(UIHelper.baseUrl+"verify.php?meterno="+ meterno.Text.Trim());

            string outputJson = await response.Content.ReadAsStringAsync();

            JObject output = JObject.Parse(outputJson);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string status = (string)output["status"];
                if (status == "success")
                {
                    account ac = new account();
                    ac.id = (int)output["id"];
                    ac.name = (string)output["name"];
                    ac.reading = (string)output["reading"];
                    ac.meterno = (string)output["meterno"];
                    ac.compliance = (string)output["compliance"];                    
                    
                    Frame.Navigate(typeof(MeterPage),ac);
                }
                else
                {
                    string error = (string)output["message"];
                    await UIHelper.ShowAlert(error);
                }
            }
            else
            {
                string error = (string)output["message"];
                await UIHelper.ShowAlert(error);
            }
            btnsubmit.IsEnabled = true;
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values.Remove("username");
            Frame.Navigate(typeof(LoginPage));
        }
    }
}
