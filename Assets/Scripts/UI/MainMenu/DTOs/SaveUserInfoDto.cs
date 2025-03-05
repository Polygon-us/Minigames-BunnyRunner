using System;

namespace UI.DTOs
{
    [Serializable]
    public class SaveUserInfoDto
    {
        public string username;
        public string email;
        public int score;
        public int distance;
        public bool tutorial;
        // public string password;
    }
}