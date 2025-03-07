using FirebaseCore.DTOs;
using Newtonsoft.Json;
using UnityEngine;
using System;

#if FIREBASE_WEB
using FirebaseCore.Receivers;
using FirebaseWebGL.Scripts.FirebaseBridge;
#else
using Firebase.Database;
#endif


namespace FirebaseCore.Senders
{
    public abstract class FirebaseSender<T>
    {
        protected readonly string Room;
        protected abstract string ChildName { get; set; }

#if FIREBASE_WEB
        protected FirebaseSender(string room)
        {
            Room = room;
        }

        protected void Send(string json)
        {
            Receiver receiver = ReceiverManager.Instance.Register(GetType());
            
            FirebaseDatabase.UpdateJSON
            (
                $"{Room}/{ChildName}",
                json,
                receiver.Name,
                receiver.SuccessCallback,
                receiver.FailCallback
            );
        }

        public void Delete()
        {
            Receiver receiver = ReceiverManager.Instance.Register(GetType());

            FirebaseDatabase.DeleteJSON
            (
                $"{Room}/{ChildName}",
                receiver.Name,
                receiver.SuccessCallback,
                receiver.FailCallback
            );
        }
#else
        protected DatabaseReference Reference;

        protected FirebaseSender(string room)
        {
            Room = room;

            GetReference();
        }

        private void GetReference()
        {
            Reference = FirebaseDatabase.DefaultInstance.GetReference(Room).Child(ChildName);
        }
        
        public void Delete()
        {
            Reference.RemoveValueAsync();
        }
#endif

        public abstract void Send(T obj);

    }
}