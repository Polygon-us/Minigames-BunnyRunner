using System.Collections.Generic;
using UnityEngine;
using System;

namespace FirebaseCore.Receivers
{
    public class ReceiverManager : MonoBehaviour
    {
        private static ReceiverManager _instance;

        public static ReceiverManager Instance
        {
            get
            {
                if (_instance)
                    return _instance;

                _instance = FindAnyObjectByType<ReceiverManager>();

                if (!_instance)
                    _instance = new GameObject("ReceiverManager").AddComponent<ReceiverManager>();

                return _instance;
            }
        }

        private readonly Dictionary<Type, Receiver> receivers = new();

        public Receiver Register(Type type)
        {
            Receiver receiver = GetByType(type);
            
            if (receiver)
                return receiver;
            
            receiver = new GameObject(type.Name).AddComponent<Receiver>();

            receiver.transform.SetParent(transform);

            receivers.Add(type, receiver);
            return receiver;
        }

        public Receiver GetByType(Type type)
        {
            return receivers.GetValueOrDefault(type);
        }

        public void Unregister(Type type)
        {
            Receiver receiver = receivers[type];

            receiver.ChildAdded = null;
            receiver.ChildChanged = null;

            receivers.Remove(type);

            Destroy(receiver.gameObject);
        }
    }
}