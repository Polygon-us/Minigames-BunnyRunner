using FirebaseCore.DTOs;
using Newtonsoft.Json;

#if FIREBASE_WEB
using FirebaseCore.Receivers;
using FirebaseWebGL.Scripts.FirebaseBridge;
#else
using Firebase.Database;
#endif

namespace FirebaseCore.Senders
{
    public class LeaderboardSender : FirebaseSender<LeaderboardDto>
    {
        protected override string ChildName { get; set; } = "gameState";

        public LeaderboardSender(string room) : base(room)
        {
        }

#if FIREBASE_WEB  
        public override void Send(LeaderboardDto leaderboardDto)
        {
            Send(JsonConvert.SerializeObject(leaderboardDto));
        }
#else
        public override void Send(LeaderboardDto leaderboardDto)
        {
            string json = JsonConvert.SerializeObject(leaderboardDto);
            Reference.SetRawJsonValueAsync(json);
        }
#endif
    }
}