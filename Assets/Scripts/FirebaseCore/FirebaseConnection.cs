using FirebaseCore.DTOs;
using Newtonsoft.Json;
using UnityEngine;
using System;

#if UNITY_WEBGL && !UNITY_EDITOR
using FirebaseWebGL.Scripts.FirebaseBridge;
#else
using Firebase.Extensions;
using Firebase.Database;
using Firebase;
#endif

namespace FirebaseCore
{
    public class FirebaseConnection : MonoBehaviour
    {
        private const string Room = "A1B1";

        public static Action<int> OnMovementInput;
        public static Action OnSubmitInput;

#if UNITY_WEBGL && !UNITY_EDITOR
        private void Start()
        {
            ListenToDatabaseChanges();
        }

        public void ListenToDatabaseChanges()
        {
            FirebaseDatabase.ListenForChildChanged(Room, gameObject.name, nameof(HandleValueChanged), nameof(HandleError));
        }

        private void HandleValueChanged(string data)
        {
            ChangedDataDto dataDto = ConvertTo<ChangedDataDto>(data);
            
            switch (dataDto.key)
            {
                case nameof(UserInputDto.direction):
                    OnMovementInput?.Invoke(int.Parse(dataDto.value));
                    break;
                case nameof(UserInputDto.submit):
                    OnSubmitInput?.Invoke();
                    break;
            }

        }

        private void HandleError(string error)
        {
            Debug.LogError(error);
        }

        private void OnDisable()
        {
            FirebaseDatabase.StopListeningForChildChanged(Room, gameObject.name, nameof(HandleValueChanged),
                nameof(HandleError));
        }

        private static T ConvertTo<T>(string obj) where T : class
        {
            return JsonConvert.DeserializeObject<T>(obj);
        }
#else
        DatabaseReference reference;

        private void Start()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(_ => {
                reference = FirebaseDatabase.DefaultInstance.GetReference(Room);

                // Listening for changes to child properties of Room
                reference.ChildChanged += HandleChildChanged;
            });
        }

        // Handle child changes under the Room node
        private void HandleChildChanged(object sender, ChildChangedEventArgs e)
        {
            if (e.DatabaseError != null)
            {
                Debug.LogError("Error: " + e.DatabaseError.Message);
                return;
            }

            // Log the changed child key and its new value
            Debug.Log("Child changed: " + e.Snapshot.Key + " -> " + e.Snapshot.Value);
        }

        private void OnDisable()
        {
            reference.ChildChanged -= HandleChildChanged;
        }
#endif
    }
}