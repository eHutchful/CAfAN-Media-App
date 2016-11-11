using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.WindowsAzure.MobileServices;
using ModernHttpClient;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using DivineVerITies.Fragments;

namespace DivineVerITies.Helpers
{
    public class AzureService
    {
        private static AzureService azureServiceInstance = new AzureService();
        public static AzureService DefaultService { get { return azureServiceInstance; } }
        public MobileServiceClient client { get; set; }
        private static MobileServiceUser user;
        public MobileServiceUser User { get { return user; } }

        public AzureService()
        {
            CurrentPlatform.Init();
            // Initialize the Mobile Service client with your URL and key
            client = new MobileServiceClient(
                "https://divineveritiestrial.azurewebsites.net",
                new NativeMessageHandler())
            {
                SerializerSettings = new MobileServiceJsonSerializerSettings()
                {
                    CamelCasePropertyNames = true
                }
            };

            client.CurrentUser = User;
        }

        public async Task<AuthenticationToken> GetAuthenticationToken(string email, string password)
        {
            // define request content
            HttpContent content = new StringContent(
            string.Format("username={0}&password={1}&grant_type=password",
                          email.ToLower(),
                          password));

            // set header
            content.Headers.ContentType
            = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            // invoke Api
            HttpResponseMessage response
                = await client.InvokeApiAsync("/oauth/token",
                                               content,
                                                HttpMethod.Post,
                                               null,
                                               null);

            // read and parse token
            string flatToken = await response.Content.ReadAsStringAsync();
            return JsonConvert
                    .DeserializeObject<AuthenticationToken>(flatToken);
        }

        public async Task Authenticate(string email, string password)
        {
            // get the token
            var token = await GetAuthenticationToken(email, password);

            // authenticate: create and use a mobile service user
            user = new MobileServiceUser(token.UserID);
            user.MobileServiceAuthenticationToken = token.Access_Token;
        }

        public async Task<bool> FacebookAuthenticate()
        {
            var success = false;
            try
            {
                // Sign in with Facebook login using a server-managed flow.
                user = await client.LoginAsync(Fragment2.SignInContext,
                    MobileServiceAuthenticationProvider.Facebook);
                CreateAndShowDialog(string.Format("you are now logged in - {0}",
                    user.UserId), "Logged in!");

                success = true;
            }
            catch (Exception ex)
            {
                CreateAndShowDialog(ex, "Authentication failed");
            }
            return success;
        }

        public async Task<bool> TwitterAuthenticate()
        {
            var success = false;
            try
            {
                // Sign in with Facebook login using a server-managed flow.
                user = await client.LoginAsync(Fragment2.SignInContext,
                    MobileServiceAuthenticationProvider.Twitter);
                CreateAndShowDialog(string.Format("you are now logged in - {0}",
                    user.UserId), "Logged in!");

                success = true;
            }
            catch (Exception ex)
            {
                CreateAndShowDialog(ex, "Authentication failed");
            }
            return success;
        }

        public async Task<bool> GoogleAuthenticate()
        {
            var success = false;
            try
            {
                // Sign in with Facebook login using a server-managed flow.
                user = await client.LoginAsync(Fragment2.SignInContext,
                    MobileServiceAuthenticationProvider.Google);
                CreateAndShowDialog(string.Format("you are now logged in - {0}",
                    user.UserId), "Logged in!");

                success = true;
            }
            catch (Exception ex)
            {
                CreateAndShowDialog(ex, "Authentication failed");
            }
            return success;
        }

        public async Task<bool> MicrosoftAuthenticate()
        {
            var success = false;
            try
            {
                // Sign in with Facebook login using a server-managed flow.
                user = await client.LoginAsync(Fragment2.SignInContext,
                    MobileServiceAuthenticationProvider.MicrosoftAccount);
                CreateAndShowDialog(string.Format("you are now logged in - {0}",
                    user.UserId), "Logged in!");
                
                success = true;
            }
            catch (Exception ex)
            {
                CreateAndShowDialog(ex, "Authentication failed");
            }
            return success;
        }

        private void CreateAndShowDialog(Exception exception, String title)
        {
            CreateAndShowDialog(exception.Message, title);
        }

        private void CreateAndShowDialog(string message, string title)
        {
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(Fragment2.SignInContext);

            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.Create().Show();
        }
    }
}