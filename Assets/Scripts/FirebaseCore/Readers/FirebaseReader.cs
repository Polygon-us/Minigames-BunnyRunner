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
        protected FirebaseReader(string room)
        {
            Room = room;
        }

        protected void Read()
        {
            Receiver receiver = ReceiverManager.Instance.Register(GetType());
            
            FirebaseDatabase.GetJSON
            (
                $"{Room}/{ChildName}",
                receiver.Name,
                receiver.DataFetchedCallback,
                receiver.FailCallback
            );
            
            receiver.DataFetched += OnDadaFetched;
        }

        private void OnDadaFetched(string json)
        {
            OnDataReceived?.Invoke(JsonConvert.DeserializeObject<T>(json));
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