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

        private UserInputDto _lastInput;

        protected override void HandleValueChanged(string data)
        {
            Debug.Log(data);
            
            ChangedDataDto changeData = JsonUtility.FromJson<ChangedDataDto>(data);

            switch (changeData.key)
            {
                case nameof(UserInputDto.direction):
                    _lastInput.direction = int.Parse(changeData.value);
                    break;
                case nameof(UserInputDto.count):
                    _lastInput.count = int.Parse(changeData.value);
                    break;
            }

            OnDataReceived(_lastInput);
        }
#else

        // protected override void HandleChildChanged(UserInputDto data)
        // {
        //     OnDataReceived?.Invoke(data);
        // }

#endif
        
    }
}