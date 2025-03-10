using FirebaseCore.Listeners;
using FirebaseCore.Senders;
using FirebaseCore.DTOs;
using UnityEngine;

public class GameFlowController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AState initialState;
    [SerializeField] private AState gameState;
    [SerializeField] private AState gameOverState;
    [SerializeField] private RoomConfig roomConfig;
    
    private GameStateListener gameStateListener;
    private GameStateSender gameStateSender;
    
    private void Start()
    {
        gameStateSender = new GameStateSender(roomConfig.roomName);
        gameStateListener = new GameStateListener(roomConfig.roomName);

        gameStateListener.OnDataReceived += OnStateChanged;

        gameManager.Initialize(roomConfig);
        
        GameStateDto gameStateDto = new GameStateDto
        {
            state = GameStates.Register
        };
            
        gameStateSender.Send(gameStateDto);
    }
    
    private void OnStateChanged(GameStateDto state)
    {
        switch (state.state)
        {
            case GameStates.Register:
                gameManager.PushState(initialState.GetName());
                break;
            case GameStates.Game:
                gameManager.PushState(gameState.GetName());
                break;
            case GameStates.End:
                gameManager.PushState(gameOverState.GetName());
                break;
        }
    }

    private void OnDestroy()
    {
        gameStateListener.Disconnect();
    }
}
