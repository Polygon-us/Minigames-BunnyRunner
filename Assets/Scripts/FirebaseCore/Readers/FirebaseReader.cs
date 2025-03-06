using FirebaseCore.DTOs;
using Newtonsoft.Json;
using UnityEngine;
using System;
using Firebase.Extensions;

#if FIREBASE_WEB
using FirebaseCore.Receivers;
using FirebaseWebGL.Scripts.FirebaseBridge;
#else
using Cysharp.Threading.Tasks;
using Firebase.Database;
#endif


namespace FirebaseCore.Senders
{
    public abstract class FirebaseReader<T>
    {
        protected readonly string Room;
        protected abstract string ChildName { get; set; }

        public Action<T> OnDataReceived;
        
#if FIREBASE_WEB
        protected FirebaseSender(string room)
        {
            Room = room;
        }

        protected void Read()
        {
            Receiver receiver = ReceiverManager.Instance.Register(GetType());
            FirebaseDatabase.ListenForChildChanged($"{Room}/{ChildName}", receiver.Name, receiver.ChildChangedCallback, receiver.FailCallback);
            FirebaseDatabase.ListenForChildAdded($"{Room}/{ChildName}", receiver.Name, receiver.ChildAddedCallback, receiver.FailCallback);
            receiver.ChildAdded += HandleValueChanged;
            receiver.ChildChanged += HandleValueChanged;
            Receiver receiver = ReceiverManager.Instance.Register(GetType());
            
            // FirebaseDatabase.UpdateJSON
            // (
            //     $"{Room}/{ChildName}",
            //     json,
            //     receiver.Name,
            //     receiver.SuccessCallback,
            //     receiver.FailCallback
            // );
        }

#else
        protected DatabaseReference Reference;

        protected FirebaseReader(string room)
        {
            Room = room;

            GetReference();
        }

        private void GetReference()
        {
            Reference = FirebaseDatabase.DefaultInstance.GetReference(Room).Child(ChildName);
        }
        
        public void Read()
        {
            Reference.GetValueAsync().ContinueWithOnMainThread(request =>
            {   
                OnDataReceived.Invoke((T)request.Result.Value);
            });
        }
#endif


    }
}