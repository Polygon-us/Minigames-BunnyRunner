using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using UnityEngine;

#if FIREBASE_WEB
using FirebaseCore.Receivers;
using FirebaseWebGL.Scripts.FirebaseBridge;
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
            Reference = FirebaseDatabase.DefaultInstance.GetReference($"{Room}/{ChildName}");
            
            ListenToChanges();
        }

        private void ListenToChanges()
        {
            Reference.ChildAdded += HandleChildChanged;
            Reference.ChildChanged += HandleChildChanged;
        }

        protected abstract void HandleChildChanged(object sender, ChildChangedEventArgs e);

        public void Disconnect()
        {
            if (Reference == null)
                return;
            
            Reference.ChildAdded -= HandleChildChanged;
            Reference.ChildChanged -= HandleChildChanged;
        }
#endif

        protected static T ConvertTo<T>(Dictionary<string, object> data)
        {
            return ConvertTo<T>(JsonConvert.SerializeObject(data));
        }
        
        protected static T ConvertTo<T>(string obj)
        {
            return JsonConvert.DeserializeObject<T>(obj);
        }

    }
}