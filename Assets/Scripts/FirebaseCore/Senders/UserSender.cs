using DTOs.Firebase;
using UnityEngine;

#if FIREBASE_WEB
using FirebaseCore.Receivers;
using FirebaseWebGL.Scripts.FirebaseBridge;
#else
using Firebase.Database;
#endif

namespace FirebaseCore.Senders
{
    public class UserSender : FirebaseSender<RegisterDto>
    {
        protected override string ChildName { get; set; } = "user";

        public UserSender(string room) : base(room)
        {
        }

#if FIREBASE_WEB  
        public override void Send(RegisterDto registerDto)
        {
            Send(JsonUtility.ToJson(registerDto));
        }
#else
        public override void Send(RegisterDto registerDto)
        {
            Reference.SetRawJsonValueAsync(JsonUtility.ToJson(registerDto));
        }
#endif
    }
}