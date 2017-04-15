using Newtonsoft.Json;

namespace RedmineUserImport
{
    [JsonObject("user")]
    public class User
    {
        [JsonProperty("user")]
        public UserDetail _userDetail { get; set; } = new UserDetail();

        public User (UserDetail deatil)
        {
            this._userDetail.Login = deatil.Login;
            this._userDetail.Password = deatil.Password;
            this._userDetail.FirstName = deatil.FirstName;
            this._userDetail.LastName = deatil.LastName;
            this._userDetail.Mail = deatil.Mail;
            this._userDetail.MustChangePasswd = deatil.MustChangePasswd;
        }
    }

    public class UserDetail
    {
        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("firstname")]
        public string FirstName { get; set; }

        [JsonProperty("lastname")]
        public string LastName { get; set; }

        [JsonProperty("mail")]
        public string Mail { get; set; }

        [JsonProperty("must_change_passwd")]
        public bool MustChangePasswd { get; set; }
    }

    public sealed class UserDetailMap : CsvHelper.Configuration.CsvClassMap<UserDetail>
    {
        public UserDetailMap()
        {
            Map(x => x.Login).Index(0);
            Map(x => x.Password).Index(1);
            Map(x => x.FirstName).Index(2);
            Map(x => x.LastName).Index(3);
            Map(x => x.Mail).Index(4);
            Map(x => x.MustChangePasswd).Index(5);
        }
    }
}
