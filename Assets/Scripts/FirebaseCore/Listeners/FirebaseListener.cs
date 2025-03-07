using System.Collections.Generic;
using FirebaseCore.Utils;
using Newtonsoft.Json;
using UnityEngine;
using System;

#if FIREBASE_WEB
using FirebaseWebGL.Scripts.FirebaseBridge;
using FirebaseCore.Receivers;
#else
using Cysharp.Threading.Tasks;
using Firebase.Database;
using Firebase;
#endif

namespace FirebaseCore.Listeners
{
    public abstract class FirebaseListener<TDto> where TDto : struct 
    {
        protected readonly string Room;
        protected abstract string ChildName { get; set; }

        public Action<TDto> OnDataReceived;
        
#if FIREBASE_WEB
        protected FirebaseListener(string room)
        {
            Room = room;
            
            ListenToDatabaseChanges();
        }

        public void ListenToDatabaseChanges()
        {
            Receiver receiver = ReceiverManager.Instance.Register(GetType());
            FirebaseDatabase.ListenForChildChanged($"{Room}/{ChildName}", receiver.Name, receiver.ChildChangedCallback, receiver.FailCallback);
            FirebaseDatabase.ListenForChildAdded($"{Room}/{ChildName}", receiver.Name, receiver.ChildAddedCallback, receiver.FailCallback);
            receiver.ChildAdded += HandleValueChanged;
            receiver.ChildChanged += HandleValueChanged;
        }

        protected abstract void HandleValueChanged(string data);
        
        public void Disconnect()
        {
            Receiver receiver = ReceiverManager.Instance.GetByType(GetType());
            FirebaseDatabase.StopListeningForChildChanged($"{Room}/{ChildName}", receiver.Name, receiver.ChildChangedCallback, receiver.FailCallback);
            FirebaseDatabase.StopListeningForChildAdded($"{Room}/{ChildName}", receiver.Name, receiver.ChildAddedCallback, receiver.FailCallback);
            
            ReceiverManager.Instance.Unregister(GetType());
        }
        
#else

        protected DatabaseReference Reference;

        protected FirebaseListener(string room)
        {
            Room = room;

            GetReference();
        }

        private void GetReference()
        {
            Reference = FirebaseDatabase.DefaultInstance.GetReference($"{Room}");
            
            ListenToChanges();
        }

        private void ListenToChanges()
        {
            Reference.ChildAdded += HandleChildChanged;
            Reference.ChildChanged += HandleChildChanged;
        }

        private void HandleChildChanged(object sender, ChildChangedEventArgs e)
        {
            if (e.Snapshot.Key == ChildName)
                // HandleChildChanged(e.Snapshot.Value.ConvertTo<TDto>());
                OnDataReceived?.Invoke(e.Snapshot.Value.ConvertTo<TDto>());
        }
        
        // protected abstract void HandleChildChanged(TDto value);

        public void Disconnect()
        {
            if (Reference == null)
                return;
            
            Reference.ChildAdded -= HandleChildChanged;
            Reference.ChildChanged -= HandleChildChanged;
        }
#endif
    }
}