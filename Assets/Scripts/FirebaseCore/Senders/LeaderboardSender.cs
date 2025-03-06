using Firebase.Extensions;
using FirebaseCore.DTOs;
using Newtonsoft.Json;

#if FIREBASE_WEB
using FirebaseCore.Receivers;
using FirebaseWebGL.Scripts.FirebaseBridge;
#else
using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;
#endif

namespace FirebaseCore.Senders
{
    public class LeaderboardSender : FirebaseSender<LeaderboardDto>
    {
        protected override string ChildName { get; set; } = "leaderboard";

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
            Reference.GetValueAsync().ContinueWithOnMainThread(task =>
            {
                List<LeaderboardDto> data =
                    JsonConvert.DeserializeObject<List<LeaderboardDto>>(JsonConvert.SerializeObject(task.Result.Value));
                
                int index = data.FindIndex(entry => entry.username == leaderboardDto.username);
                
                if (index != -1)
                    data[index].score = Mathf.Max(data[index].score, leaderboardDto.score);
                else
                    data.Add(leaderboardDto);
                
                string json = JsonConvert.SerializeObject(data);  
                
                Reference.SetRawJsonValueAsync(json);
            });
        }
#endif
    }
}