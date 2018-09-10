using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BlackboardTest
{
    public partial class MainPage : ContentPage
    {
        const string APPKEY = "754ccace-79dc-4e57-ab3f-e0db02435ae3";
        const string SECRET = "M1RR4FdGtXmKLuvTDsGe6QfrmlA6LyMI";

        const string HOSTNAME = "https://unitec.blackboard.com";
        const string AUTH_PATH = "/learn/api/public/v1/oauth2/token";

        Token authToken;

        public MainPage()
        {
            InitializeComponent();
        }

        private void userMail_Completed(object sender, EventArgs e)
        {
            userPass.Focus();
        }

        private void LogIn(object sender, EventArgs e)
        {
            string mail = userMail.Text;
            string pass = userPass.Text;
            if (mail.Length > 0 && pass.Length > 0)
            {
                AppLog("Empezando OAuth2.\nPor favor espere.");
                AuthProcessAsync();
            }
            else
            {
                AppLog("Usuario y contraseña no pueden estar vacios.");
            }
        }

        private async void AuthProcessAsync()
        {
            HttpClient client = new HttpClient();

            var endpoint = new Uri(HOSTNAME + AUTH_PATH);

            var authData = string.Format("{0}:{1}", APPKEY, SECRET);
            var authHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(authData));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);

            var postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            };
            HttpContent body = new FormUrlEncodedContent(postData);

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync(endpoint, body).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    authToken = JsonConvert.DeserializeObject<Token>(content);
                    AppLog("Token: " + authToken.ToString());
                }
                else
                {
                    AppLog("Error: OAuth2: La conexion no respondio exitosamente: " + response.StatusCode);
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (AggregateException agex)
            {
                AppLog("Error: OAuth2: Async: " + agex.Message);
            }
            catch (Exception ex)
            {   
                AppLog("Error: OAuth2: Exepcion al intentar realizar la conexion: " + ex.Message);
            }
        }

        private void AppLog(string message)
        {
            string logMessage = string.Format("[{0}] {1}", DateTime.Now.ToShortTimeString(), message);
            Console.WriteLine(logMessage);
            //result.Text = logMessage;
        }
    }

    public class Token
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
    }
}
