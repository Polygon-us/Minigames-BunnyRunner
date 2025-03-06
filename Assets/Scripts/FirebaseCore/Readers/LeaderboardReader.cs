using System.Collections.Generic;
using FirebaseCore.DTOs;

namespace FirebaseCore.Senders
{
    public class LeaderboardReader : FirebaseReader<List<LeaderboardDto>>
    {
        protected override string ChildName { get; set; } = "leaderboard";
        
        public LeaderboardReader(string room) : base(room)
        {
        }

    }
}