using Med.SafeValue;
using UI.DTOs;

namespace Source.Handlers
{
    public class BaseHandler
    {
        private const string UserInfoKey = "UserInfo";

        protected const string GameType = "fanta_runner";

        public static SaveUserInfoDto SaveUserInfo => PlayerSaves.DecryptClass<SaveUserInfoDto>(UserInfoKey);

        protected static string[] Args(params string[] args) => args;

        public static void SaveInfoToPrefs(string username, string email, int score, int distance,
            bool tutorial /*, string password*/)
        {
            SaveUserInfoDto saveUserInfoDto = new SaveUserInfoDto
            {
                username = username,
                email = email,
                score = score,
                distance = distance,
                tutorial = tutorial
                // password = password
            };

            PlayerSaves.EncryptClass(saveUserInfoDto, UserInfoKey);
        }

        public static void ResetInfoFromPrefs()
        {
            SaveUserInfoDto saveUserInfoDto = new SaveUserInfoDto {tutorial = true};

            PlayerSaves.EncryptClass(saveUserInfoDto, UserInfoKey);
        }
    }
}