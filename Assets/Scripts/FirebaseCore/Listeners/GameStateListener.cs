#if FIREBASE_WEB
#else
using Firebase.Database;
#endif
using System;
using FirebaseCore.DTOs;
using ModernWestern;
using UnityEngine;

namespace FirebaseCore.Listeners
{
    public class GameStateListener : FirebaseListener<GameStateDto>
    {
        protected override string ChildName { get; set; } = "gameState";
        
        public GameStateListener(string room) : base(room)
        {
        }
        
#if FIREBASE_WEB
        protected override void HandleValueChanged(string data)
        {   
            ChangedDataDto changeData = JsonUtility.FromJson<ChangedDataDto>(data);

            GameStateDto gameStateDto = new GameStateDto
            {
                state = changeData.value.ToEnum<GameStates>()
            };
            
            OnDataReceived(gameStateDto);
        }
#else

        protected override void HandleChildChanged(object sender, ChildChangedEventArgs e)
        {
            GameStateDto gameStateDto = new GameStateDto
            {
                state = (GameStates)e.Snapshot.Value
            };
            Debug.Log("Changed: " + e.Snapshot.Value);
            
            OnDataReceived?.Invoke(gameStateDto);
        }
#endif
    }
}