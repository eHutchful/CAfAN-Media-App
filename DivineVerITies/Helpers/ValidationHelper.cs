using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Java.Util.Regex;
using Android.Support.Design.Widget;

namespace DivineVerITies.Helpers
{
    public class ValidationHelper
    {
        private Context context;

        /// <summary>
        /// constructor </summary>
        /// <param name="context"> </param>
        public ValidationHelper(Context context)
        {
            this.context = context;
        }

        /// <summary>
        /// method to check EditText filled . </summary>
        /// <param name="textInputLayout.EditText"> </param>
        /// <param name="textInputLayout"> </param>
        /// <param name="message">
        /// @return </param>
        public virtual bool? isEditTextFilled(TextInputLayout textInputLayout, string message)
        {
            string value = textInputLayout.EditText.Text.ToString().Trim();
            if (value.Length == 0)
            {
                textInputLayout.Error = message;
                hideKeyboardFrom(textInputLayout.EditText);
                return false;
            }
            else
            {
                textInputLayout.ErrorEnabled = false;
            }

            return true;
        }

        /// <summary>
        /// method to check valid email inEditText . </summary>
        /// <param name="textInputLayout.EditText"> </param>
        /// <param name="textInputLayout"> </param>
        /// <param name="message">
        /// @return </param>
        public virtual bool? isEditTextEmail(TextInputLayout textInputLayout, string message)
        {
            string value = textInputLayout.EditText.Text.ToString().Trim();
            if (value.Length == 0 || !Android.Util.Patterns.EmailAddress.Matcher(value).Matches())
            {
                textInputLayout.Error = message;
                hideKeyboardFrom(textInputLayout.EditText);
                return false;
            }
            else
            {
                textInputLayout.ErrorEnabled = false;
            }
            return true;
        }



        /// <summary>
        /// method to check valid password inEditText.
        /// </summary>
        /// <param name="textInputLayout.EditText"></param>
        /// <param name="textInputLayout"></param>
        /// <param name="pass"></param>
        /// <param name="username"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public virtual bool validatePassword(TextInputLayout textInputLayout, string pass, string username)
        {


            if (pass.Length < 6)
            {
                textInputLayout.Error = "Password must be at least 6 characters";
                //hideKeyboardFrom(textInputLayout.EditText);
                return false;
            }
            if (username.Length < 3)
            {
                textInputLayout.Error = "Username must be at least 3 characters";
                //hideKeyboardFrom(textInputLayout.EditText);
                return false;
            }
            if (!isValidPassword(pass, ".*\\d.*"))
            {
                textInputLayout.Error = "Password must contain at least one digit";
                //hideKeyboardFrom(textInputLayout.EditText);
                return false;
            }

            if (!isValidPassword(pass, ".*[a-z].*"))
            {
                textInputLayout.Error = "Password must contain at least one lowercase letter";
                //hideKeyboardFrom(textInputLayout.EditText);
                return false;
            }
            if (!isValidPassword(pass, ".*[.!@#$%^&*+=?-].*"))
            {
                textInputLayout.Error = "Password must contain at least one special character";
                //hideKeyboardFrom(textInputLayout.EditText);
                return false;
            }
            if (containsPartOf(pass, username))
            {
                textInputLayout.Error = "Password cannot contain substring of username";
                //hideKeyboardFrom(textInputLayout.EditText);
                return false;
            }
            //if (containsPartOf(pass, email))
            //{
            //    textInputLayout.Error = "Password cannot contain substring of email";
            //    hideKeyboardFrom(textInputLayout.EditText);
            //    return false;
            //}
            textInputLayout.ErrorEnabled = false;
            return true;
        }

        /// <summary>
        /// method to validate password.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="password_pattern"></param>
        /// <returns></returns>
        public static bool isValidPassword(string password, string password_pattern)
        {

            Pattern pattern;
            Matcher matcher;

            pattern = Pattern.Compile(password_pattern);
            matcher = pattern.Matcher(password);

            return matcher.Matches();
        }

        /// <summary>
        /// method to check if password contains subset of username or password.
        /// </summary>
        /// <param name="pass"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        private static bool containsPartOf(string pass, string username)
        {
            int requiredMin = 3; for (int i = 0; (i + requiredMin) < username.Length; i++)
            {
                if (pass.Contains(username.Substring(i, requiredMin)))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// method to Hide keyboard </summary>
        /// <param name="view"> </param>
        private void hideKeyboardFrom(View view)
        {
            InputMethodManager imm = (InputMethodManager)context.GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(view.WindowToken, HideSoftInputFlags.None);
        }

    }
}