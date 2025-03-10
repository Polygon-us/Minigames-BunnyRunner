using System.Collections.Generic;
using FirebaseCore.Utils;
using FirebaseCore.DTOs;
using Newtonsoft.Json;
using System.Linq;

#if FIREBASE_WEB
using FirebaseCore.Receivers;
using FirebaseWebGL.Scripts.FirebaseBridge;

#else
using Firebase.Extensions;
#endif

namespace FirebaseCore.Senders
{
    public class LeaderboardReader : FirebaseReader<List<LeaderboardDto>>
    {
        protected override string ChildName { get; set; } = "leaderboard";

        public LeaderboardReader(string room) : base(room)
        {
        }
        
#if FIREBASE_WEB
        
        public override void Read()
        {
            Receiver receiver = ReceiverManager.Instance.Register(GetType());

            FirebaseDatabase.GetJSON
            (
                $"{Room}/{ChildName}",
                receiver.Name,
                receiver.DataFetchedCallback,
                receiver.FailCallback
            );
            
            receiver.DataFetched += OnDadaFetched;
        }

        protected override void OnDadaFetched(string json)
        {
            ReceiverManager.Instance.Unregister(GetType());

            List<LeaderboardDto> data = JsonConvert.DeserializeObject<List<LeaderboardDto>>(json);
            
            data = data.OrderByDescending(entry => entry.score).ToList();
            
            OnDataReceived.Invoke(data);
        }

#else

        public override void Read()
        {
            Reference.GetValueAsync().ContinueWithOnMainThread(task =>
            {
                List<LeaderboardDto> data = task.Result.Value.ConvertTo<List<LeaderboardDto>>();

                data = data.Where(entry => entry != null).ToList();
                data = data.OrderByDescending(entry => entry.score).ToList();
                
                OnDataReceived.Invoke(data);
            });
        }
        
#endif
        
    }
}