
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml.Controls;

namespace MajiPay
{
    
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ErrorBucket errors = new ErrorBucket();
            ValidateSignUp(errors);
            if (!(errors.HasErrors))           {
                
                if (NetworkInformation.GetInternetConnectionProfile() == null)
                {
                    await UIHelper.ShowAlert("Check your internet connection", "No internet connectivity");
                }
                else
                {
                    await LoginAsync();
                }
            }
            else
            {
                await UIHelper.ShowAlert(errors.GetErrorsAsString());
                errors.ClearErrors();
            }
        }

        //call to server
        private async Task LoginAsync()
        {
            btnlogin.IsEnabled = false;            

            string jsonstr = "{\"username\":\"" + username.Text.Trim() + "\",\"password\":\"" + password.Password.Trim() + "\"}";

            StringContent content = new StringContent(jsonstr, System.Text.Encoding.UTF8, "application/json");

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            //no caching
            client.DefaultRequestHeaders.IfModifiedSince = DateTimeOffset.Now;
            //problem is here
            HttpResponseMessage response = await client.PostAsync(UIHelper.baseUrl + "login.php", content);
                        
            string outputJson = await response.Content.ReadAsStringAsync();
            
            JObject output = JObject.Parse(outputJson);
            await UIHelper.ShowAlert(jsonstr);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string status = (string)output["status"];
                if (status != "error")
                {
                    string message = (string)output["message"];
                    await UIHelper.ShowAlert(message);                    
                    Windows.Storage.ApplicationData.Current.LocalSettings.Values["username"] = username.Text;
                    Frame.Navigate(typeof(HomePage));                    
                }
                else
                {
                    string error = (string)output["message"];
                    await UIHelper.ShowAlert(error);
                }
                btnlogin.IsEnabled = true;
            }
            else
            {
                string error = (string)output["message"];
                await UIHelper.ShowAlert(error);
                btnlogin.IsEnabled = true;
            }
            
        }


        private void ValidateSignUp(ErrorBucket errors)
        {

            if (string.IsNullOrEmpty(username.Text))
                errors.AddError("Enter username");

            if (string.IsNullOrEmpty(password.Password))
                errors.AddError("Enter password");

            if (!string.IsNullOrEmpty(password.Password) && password.Password.Length<6)
                errors.AddError("Password cannot be less than 6 characters");

        }

        
    }
}
