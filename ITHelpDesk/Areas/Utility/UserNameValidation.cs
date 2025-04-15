using System.Text.RegularExpressions;

namespace ITHelpDesk.Areas.Utility
{
    public class UserNameValidation
    {
        public static bool IsValidUserName(string username)
        {
            // Only allow letters and spaces
            var regex = new Regex(@"^[a-zA-Z\s]+$");
            return regex.IsMatch(username);
        }
    }
}
