using FirebaseCore.DTOs;

namespace FirebaseCore.Senders
{
    public class LeaderboardReader : FirebaseReader<LeaderboardListDto>
    {
        protected override string ChildName { get; set; } = "leaderboard";
        
        public LeaderboardReader(string room) : base(room)
        {
        }

    }
}