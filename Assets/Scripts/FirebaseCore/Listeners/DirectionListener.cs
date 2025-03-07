#if FIREBASE_WEB

#else
using Firebase.Database;
#endif
using FirebaseCore.DTOs;
using UnityEngine;

namespace FirebaseCore.Listeners
{
    public class DirectionListener : FirebaseListener<UserInputDto>
    {
        protected override string ChildName { get; set; } = "movement";

        public DirectionListener(string room) : base(room)
        {
        }
        
#if FIREBASE_WEB
        protected override void HandleValueChanged(string data)
        {   
            Debug.Log(data);
        }
#else

        // protected override void HandleChildChanged(UserInputDto data)
        // {
        //     OnDataReceived?.Invoke(data);
        // }

#endif
        
    }
}