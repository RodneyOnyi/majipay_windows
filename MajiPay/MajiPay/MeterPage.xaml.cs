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
    public sealed partial class MeterPage : Page
    {
        private account ac;
        double units;
        double cost;
        

        public MeterPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter!= null)
            {
                ac = (account)e.Parameter;
                txtmeterno.Text = ac.meterno;
                txtname.Text = ac.name;
                txtlastreading.Text = ac.reading;
                txtcompliance.Text = ac.compliance;
            }
                      

        }

        private async void btnsubmit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtmeterreading.Text))
            {
                await UIHelper.ShowAlert("Please enter meter number");
            }else
            {
                if(double.Parse(txtlastreading.Text) >= double.Parse(txtmeterreading.Text))
                {
                    ac.compliance = "Non-compliant";
                    txtcompliance.Text = ac.compliance;
                    await UIHelper.ShowAlert("Your meter is non compliant","Error");
                }
                else
                {
                    units = double.Parse(txtmeterreading.Text.Trim()) - double.Parse(txtlastreading.Text);
                    cost = units * 20;
                    ac.reading = txtmeterreading.Text.Trim();
                    ac.compliance = "Compliant";
                    await UpdateAccount();
                }                
            }
        }

        //call to server
        private async Task UpdateAccount()
        {
            btnsubmit.IsEnabled = false;
            
            string jsonstr = "{\"id\":" + ac.id + ",\"reading\":\""+ac.reading+ "\",\"units\":\"" + units + "\",\"compliance\":\"" + ac.compliance + "\"}";
                        
            StringContent content = new StringContent(jsonstr, System.Text.Encoding.UTF8, "application/json");

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            //no caching
            client.DefaultRequestHeaders.IfModifiedSince = DateTimeOffset.Now;

            HttpResponseMessage response = await client.PostAsync(UIHelper.baseUrl + "updatereading.php", content);

            string outputJson = await response.Content.ReadAsStringAsync();

            JObject output = JObject.Parse(outputJson);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string status = (string)output["status"];
                if (status == "success")
                {   
                    if (ac.compliance == "Compliant")                
                    await UIHelper.ShowAlert(string.Format("Meter reading submitted.You have consumed {0} units and it costs Ksh {1}",units,cost),"MajiPay");
                    txtcompliance.Text = ac.compliance;
                    txtlastreading.Text = ac.reading;
                }
                else
                {
                    string error = (string)output["message"];
                    await UIHelper.ShowAlert(error);
                }
                btnsubmit.IsEnabled = true;
            }
            else
            {
                string error = (string)output["message"];
                await UIHelper.ShowAlert(error);
                btnsubmit.IsEnabled = true;
            }
        }


        private void ValidateReading(ErrorBucket errors)
        {
            if (string.IsNullOrEmpty(txtmeterreading.Text))
                errors.AddError("Please enter meter number");      
        }


        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values.Remove("username");
            Frame.Navigate(typeof(LoginPage));
        }
    }
}
