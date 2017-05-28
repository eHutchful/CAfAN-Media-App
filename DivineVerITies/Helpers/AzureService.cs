using System;
using System.Linq;

using Android.App;
using Android.Content;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using DivineVerITies.Fragments;
using System.Text;

namespace DivineVerITies.Helpers
{
    public class AzureService
    {
        private static AzureService azureServiceInstance = new AzureService();
        public static AzureService DefaultService { get { return azureServiceInstance; } }
        public MobileServiceClient client { get; set; }
        private static MobileServiceUser user;
        public static string tempToken;
        ISharedPreferences pref { get; set; }
        ISharedPreferencesEditor edit { get; set; }
        public MobileServiceUser User { get { return user; } }

        public AzureService()
        {
            CurrentPlatform.Init();
            // Initialize the Mobile Service client with your URL and key
            client = new MobileServiceClient(
                "https://divineverities.azurewebsites.net",
                 new CustomMessageHandler())
            {
                SerializerSettings = new MobileServiceJsonSerializerSettings()
                {
                    CamelCasePropertyNames = true
                }
            };

            //client.CurrentUser = User;
        }

        public async Task<AuthenticationToken> GetAuthenticationToken(string email, string password)
        {
            // define request content
            HttpContent content = new StringContent(
            string.Format("username={0}&password={1}&grant_type=password",
                          Uri.EscapeDataString(email.ToLower()),
                          Uri.EscapeDataString(password)));

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

            if (token != null)
            {
                // authenticate: create and use a mobile service user
                user = new MobileServiceUser(token.UserID);
                user.MobileServiceAuthenticationToken = token.Access_Token;
                client.CurrentUser = user;

                pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
                edit = pref.Edit();

                if (pref.GetBoolean("RememberMe", false))
                {
                    // store settings

                    edit.PutString("UserId", token.UserID.ToString());
                    edit.PutString("Token", token.Access_Token);
                    edit.PutString("TokenExpirationDate", token.Expires.Ticks.ToString());
                    edit.PutString("Username", token.UserName);
                    edit.PutString("Password", 
                        StringCipher.Encrypt(password.Trim(), token.UserName)
                        //password.Trim()
                        );
                    edit.Apply();
                }
                else
                {
                    edit.PutString("Username", token.UserName);
                    tempToken = token.Access_Token;
                    edit.Apply();
                }
            }       
        }

        public async Task<bool> AutoAuthenticate()
        {
            ISharedPreferences pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
            string userName = pref.GetString("Username", string.Empty);
            string password = pref.GetString("Password", string.Empty);
            string userID = pref.GetString("UserId", string.Empty);
            string token = pref.GetString("Token", string.Empty);
            string expirationDateTicks = pref.GetString("TokenExpirationDate", string.Empty);

            if (userID != string.Empty && token != string.Empty && expirationDateTicks != string.Empty && password != string.Empty)
            {

                DateTime expirationDate = new DateTime(long.Parse(expirationDateTicks));

                if (expirationDate < DateTime.Now)
                {
                    try
                    {
                        await Authenticate(userName, 
                            StringCipher.Decrypt(password, userName)
                            //password
                            );
                    }
                    catch (MobileServiceInvalidOperationException)
                    {
                        return false;
                    }
                    catch (NullReferenceException)
                    {
                        return false;
                    }

                }
                else
                {
                    user = new MobileServiceUser(userID);
                    user.MobileServiceAuthenticationToken = token;
                    client.CurrentUser = user;
                }

                return true;
            }
            else if (userID != string.Empty)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> FacebookAuthenticate()
        {
            var success = false;
            try
            {
                // Sign in with Facebook login using a server-managed flow.
                user = await client.LoginAsync(SignIn.SignInContext,
                    MobileServiceAuthenticationProvider.Facebook);

                pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
                edit = pref.Edit();
                
                // store settings
                edit.PutString("UserId", user.UserId);
                edit.PutString("Token", user.MobileServiceAuthenticationToken);

                CreateAndShowDialog(string.Format("you are now logged in - {0}",
                user.UserId), "Logged in!");

                var userinfo = await client.InvokeApiAsync("/api/userinfo/getfacebookuserinfo", HttpMethod.Get, null);
                edit.PutString("Username", (userinfo.ElementAt(0)).ToString());
                edit.Apply();

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
                user = await client.LoginAsync(SignIn.SignInContext,
                    MobileServiceAuthenticationProvider.Twitter);

                pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
                edit = pref.Edit();

                // store settings
                edit.PutString("UserId", user.UserId);
                edit.PutString("Token", user.MobileServiceAuthenticationToken);
                edit.Apply();

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
                user = await client.LoginAsync(SignIn.SignInContext,
                    MobileServiceAuthenticationProvider.Google);

                pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
                edit = pref.Edit();

                // store settings
                edit.PutString("UserId", user.UserId);
                edit.PutString("Token", user.MobileServiceAuthenticationToken);

                CreateAndShowDialog(string.Format("you are now logged in - {0}",
                user.UserId), "Logged in!");

                var userinfo = await client.InvokeApiAsync("/api/userinfo/getgoogleuserinfo", HttpMethod.Get, null);
                edit.PutString("Username", (userinfo.ElementAt(1)).ToString());
                edit.Apply();

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
                user = await client.LoginAsync(SignIn.SignInContext,
                    MobileServiceAuthenticationProvider.MicrosoftAccount);

                // store settings
                edit.PutString("UserId", user.UserId);
                edit.PutString("Token", user.MobileServiceAuthenticationToken);

                CreateAndShowDialog(string.Format("you are now logged in - {0}",
                    user.UserId), "Logged in!");

                var userinfo = await client.InvokeApiAsync("/api/userinfo/getmicrosoftuserinfo", HttpMethod.Get, null);
                edit.PutString("Username", (userinfo.ElementAt(0)).ToString());
                edit.Apply();

                success = true;
            }
            catch (Exception ex)
            {
                CreateAndShowDialog(ex, "Authentication failed");
            }
            return success;
        }

        

        private void CreateAndShowDialog(Exception exception, string title)
        {
            CreateAndShowDialog(exception.Message, title);
        }

        private void CreateAndShowDialog(string message, string title)
        {
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(SignIn.SignInContext);

            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.SetPositiveButton("OKAY", delegate { builder.Dispose(); });
            builder.Create().Show();
        }
    }
}