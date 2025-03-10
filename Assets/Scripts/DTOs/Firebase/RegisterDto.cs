using System;

namespace DTOs.Firebase
{
    [Serializable]
    public struct RegisterDto
    {
        public string name;
        public string username;
        public string email;
        public string phone;
    }
}