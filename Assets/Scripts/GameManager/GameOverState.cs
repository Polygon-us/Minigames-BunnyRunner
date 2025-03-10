using System.Globalization;
using FirebaseCore.DTOs;
using FirebaseCore.Senders;
using TMPro;
using UnityEngine;
#if UNITY_ANALYTICS
using UnityEngine.Analytics;
#endif

/// <summary>
/// state pushed on top of the GameManager when the player dies.
/// </summary>
public class GameOverState : AState
{
    public TrackManager trackManager;
    public Canvas canvas;
    // public MissionUI missionPopup;
    public AudioClip gameOverTheme;
    
	public Leaderboard fullLeaderboard;

    [SerializeField] private TMP_Text countDownText;
    [SerializeField] private int countDown = 5;
    
    private GameStateSender gameStateSender;
    private UserSender userSender;

    private int countDownTween;
    
    public override void Enter(AState from)
    {
        canvas.gameObject.SetActive(true);

        gameStateSender = new GameStateSender(roomConfig.roomName);
        userSender = new UserSender(roomConfig.roomName);
        
        fullLeaderboard.Open();

        countDownTween = LeanTween.value(countDown, 0, countDown).setOnUpdate(
            value => countDownText.text = Mathf.CeilToInt(value).ToString()
        ).setOnComplete(OnCountDown).uniqueId;
        
		CreditCoins();
    }

	public override void Exit(AState to)
    {
        LeanTween.cancel(countDownTween);
        
        canvas.gameObject.SetActive(false);
        FinishRun();
    }

    private void OnCountDown()
    {
        userSender.Delete();
        
        GameStateDto gameStateDto = new GameStateDto
        {
            state = GameStates.Register
        };
        gameStateSender.Send(gameStateDto);
    }
    
    public override string GetName()
    {
        return "GameOver";
    }

    public override void Tick()
    {
        
    }

    protected void CreditCoins()
	{
		PlayerData.instance.Save();

#if UNITY_ANALYTICS // Using Analytics Standard Events v0.3.0
        var transactionId = System.Guid.NewGuid().ToString();
        var transactionContext = "gameplay";
        var level = PlayerData.instance.rank.ToString();
        var itemType = "consumable";
        
        if (trackManager.characterController.coins > 0)
        {
            AnalyticsEvent.ItemAcquired(
                AcquisitionType.Soft, // Currency type
                transactionContext,
                trackManager.characterController.coins,
                "fishbone",
                PlayerData.instance.coins,
                itemType,
                level,
                transactionId
            );
        }

        if (trackManager.characterController.premium > 0)
        {
            AnalyticsEvent.ItemAcquired(
                AcquisitionType.Premium, // Currency type
                transactionContext,
                trackManager.characterController.premium,
                "anchovies",
                PlayerData.instance.premium,
                itemType,
                level,
                transactionId
            );
        }
#endif 
	}
    
	protected void FinishRun()
    {
        CharacterCollider.DeathEvent de = trackManager.characterController.characterCollider.deathData;
        //register data to analytics
#if UNITY_ANALYTICS
        AnalyticsEvent.GameOver(null, new Dictionary<string, object> {
            { "coins", de.coins },
            { "premium", de.premium },
            { "score", de.score },
            { "distance", de.worldDistance },
            { "obstacle",  de.obstacleType },
            { "theme", de.themeUsed },
            { "character", de.character },
        });
#endif

        PlayerData.instance.Save();
        
        trackManager.End();
    }

    //----------------
}
