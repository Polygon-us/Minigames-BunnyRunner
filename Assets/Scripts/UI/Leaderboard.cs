using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using FirebaseCore.DTOs;
using FirebaseCore.Senders;
using UnityEngine;

// Prefill the info on the player data, as they will be used to populate the leadboard.
public class Leaderboard : MonoBehaviour
{
    [SerializeField] private RoomConfig roomConfig;
    
    public RectTransform entriesRoot;

    public HighscoreUI playerEntry;

    private List<LeaderboardDto> _records;

    private LeaderboardReader _leaderboardReader;
 
    public void Open()
    {
        gameObject.SetActive(true);

        _leaderboardReader = new LeaderboardReader(roomConfig.roomName);
        _leaderboardReader.OnDataReceived += OnDataReceived;
        
        Populate().Forget();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public async UniTaskVoid Populate()
    {
        for (int i = 0; i < entriesRoot.childCount; ++i)
        {
            entriesRoot.GetChild(i).gameObject.SetActive(false);
        }

        _leaderboardReader.Read();
    }
    
    private void OnDataReceived(LeaderboardListDto data)
    {
        _leaderboardReader.OnDataReceived -= OnDataReceived;

        _records = data.records;
        _records = _records.OrderByDescending(x => x.score).ToList();

        int index = data.records.FindIndex(record => record.username == roomConfig.username);

        if (index != -1)
        {
            LeaderboardDto playerRecord = _records.FirstOrDefault();
            int playerPlace = index;
            
            int lastIndex = Mathf.Min(entriesRoot.childCount, playerPlace) - 1;
            
            playerEntry.transform.SetSiblingIndex(lastIndex);
            
            if (playerPlace >= entriesRoot.childCount)
                _records.Insert(lastIndex, playerRecord);
        }
        
        for (int i = 0; i < entriesRoot.childCount && i < _records.Count; ++i)
        {
            HighscoreUI hs = entriesRoot.GetChild(i).GetComponent<HighscoreUI>();
            
            hs.gameObject.SetActive(true);

            hs.Initialize(i + 1, _records[i]);
        }
    }
}