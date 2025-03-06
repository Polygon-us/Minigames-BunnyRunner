﻿using UnityEngine;
using System;

namespace FirebaseCore.Receivers
{
    public class Receiver : MonoBehaviour
    {
        public string Name => gameObject.name;

        public string SuccessCallback => nameof(OnRequestSuccess);
        public string FailCallback => nameof(OnRequestFail);
        
        public string DataFetchedCallback => nameof(OnDataFetched);
        
        public string ChildChangedCallback => nameof(OnChildChanged);
        public string ChildAddedCallback => nameof(OnChildAdded);

        public Action<string> ChildAdded;
        public Action<string> ChildChanged;

        public Action<string> DataFetched;
        
        private void OnRequestSuccess(string message)
        {
            Debug.Log(message);
        }

        private void OnRequestFail(string message)
        {
            Debug.LogError(message);
        }
        
        private void OnDataFetched(string data) => DataFetched?.Invoke(data);

        private void OnChildChanged(string data) => ChildChanged?.Invoke(data);

        private void OnChildAdded(string data) => ChildAdded?.Invoke(data);
    }
}