using FirebaseCore.DTOs;
using Newtonsoft.Json;
using UnityEngine;
using System;
using System.Collections.Generic;
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
    public abstract class FirebaseReader<TDto>
    {
        protected readonly string Room;
        protected abstract string ChildName { get; set; }

        public Action<TDto> OnDataReceived;
        
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

        public abstract void Read();
#endif
    }
}