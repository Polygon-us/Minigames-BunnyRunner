using FirebaseCore.DTOs;
using Newtonsoft.Json;
using DTOs.Firebase;
using UnityEngine;

#if FIREBASE_WEB
using FirebaseCore.Receivers;
using FirebaseWebGL.Scripts.FirebaseBridge;
#else
using Firebase.Database;
#endif

namespace FirebaseCore.Senders
{
    public class GameStateSender : FirebaseSender<GameStateDto>
    {
        protected override string ChildName { get; set; } = "gameState";

        public GameStateSender(string room) : base(room)
        {
        }

#if FIREBASE_WEB  
        public override void Send(GameStateDto stateDto)
        {
            Send(JsonConvert.SerializeObject(stateDto));
        }
#else
        public override void Send(GameStateDto stateDto)
        {
            string json = JsonConvert.SerializeObject(stateDto);
            Reference.SetRawJsonValueAsync(json);
        }
#endif
    }
}