using System.Collections.Generic;
using System.Linq;
using Firebase.Extensions;
using FirebaseCore.DTOs;
using FirebaseCore.Utils;

namespace FirebaseCore.Senders
{
    public class LeaderboardReader : FirebaseReader<List<LeaderboardDto>>
    {
        protected override string ChildName { get; set; } = "leaderboard";
        
        public LeaderboardReader(string room) : base(room)
        {
        }

        public override void Read()
        {
            Reference.GetValueAsync().ContinueWithOnMainThread(task =>
            {
                List<LeaderboardDto> data = task.Result.Value.ConvertTo<List<LeaderboardDto>>();
                
                data = data.OrderByDescending(entry => entry.score).ToList();
                
                OnDataReceived.Invoke(data);
            });
        }
    }
}