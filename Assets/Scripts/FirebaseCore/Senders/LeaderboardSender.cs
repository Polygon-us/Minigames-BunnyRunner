using System.Collections.Generic;
using FirebaseCore.Utils;
using FirebaseCore.DTOs;
using Newtonsoft.Json;
using UnityEngine;

#if FIREBASE_WEB
using FirebaseWebGL.Scripts.FirebaseBridge;
using FirebaseCore.Receivers;
#else
using Firebase.Extensions;
#endif

namespace FirebaseCore.Senders
{
    public class LeaderboardSender : FirebaseSender<LeaderboardDto>
    {
        protected override string ChildName { get; set; } = "leaderboard";

        private LeaderboardDto _leaderboardDto;
        
        public LeaderboardSender(string room) : base(room)
        {
        }

#if FIREBASE_WEB

        public override void Send(LeaderboardDto leaderboardDto)
        {
            _leaderboardDto = leaderboardDto;
            
            Receiver receiver = ReceiverManager.Instance.Register(GetType());

            FirebaseDatabase.GetJSON
            (
                $"{Room}/{ChildName}",
                receiver.Name,
                receiver.DataFetchedCallback,
                receiver.FailCallback
            );
            receiver.DataFetched += OnDataFetched;
        }

        private void OnDataFetched(string json)
        {
            ReceiverManager.Instance.Unregister(GetType());
            
            List<LeaderboardDto> data = JsonConvert.DeserializeObject<List<LeaderboardDto>>(json);
                  
            string newJson = AddNewEntry(data, _leaderboardDto);
                
            base.Send(newJson);
        }
        
#else
        
        public override void Send(LeaderboardDto leaderboardDto)
        {
            Reference.GetValueAsync().ContinueWithOnMainThread(task =>
            {
                List<LeaderboardDto> data = task.Result.Value.ConvertTo<List<LeaderboardDto>>();
                
                string json = AddNewEntry(data, leaderboardDto);
                
                Reference.SetRawJsonValueAsync(json);
            });
        }
        
#endif

        private static string AddNewEntry(List<LeaderboardDto> data, LeaderboardDto leaderboardDto)
        {
            int index = data.FindIndex(entry => entry.username == leaderboardDto.username);
                
            if (index != -1)
                data[index].score = Mathf.Max(data[index].score, leaderboardDto.score);
            else
                data.Add(leaderboardDto);
                
            return JsonConvert.SerializeObject(data);  
        }
        
    }
}