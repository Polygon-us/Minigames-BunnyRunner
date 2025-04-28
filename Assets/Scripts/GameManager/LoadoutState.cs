using System.Collections.Generic;
using FirebaseCore.Listeners;
using FirebaseCore.Senders;
using FirebaseCore.DTOs;
using UnityEngine;

/// <summary>
/// State pushed on the GameManager during the Loadout, when player select player, theme and accessories
/// Take care of init the UI, load all the data used for it etc.
/// </summary>
public class LoadoutState : AState
{
    [SerializeField] private GameObject character;
    
    [Header("Char UI")]
    public Canvas canvas;

	public RectTransform charSelect;
	public Transform charPosition;
	
	public AudioClip menuTheme;
    
    [Header("Prefabs")]
    public ConsumableIcon consumableIcon;

    protected List<int> m_OwnedAccesories = new List<int>();
    protected bool m_IsLoadingCharacter;

	protected Modifier m_CurrentModifier = new Modifier();

    protected const float k_CharacterRotationSpeed = 45f;
    protected const float k_OwnedAccessoriesCharacterOffset = -0.1f;
    protected int k_UILayer;
    protected readonly Quaternion k_FlippedYAxisRotation = Quaternion.Euler (0f, 180f, 0f);

    private UserListener userListener;
    private GameStateSender gameStateSender;
    
    public override void Enter(AState from)
    {
        canvas.gameObject.SetActive(true);
        
        k_UILayer = LayerMask.NameToLayer("UI");

        gameStateSender = new GameStateSender(roomConfig.roomName);
        
        userListener = new UserListener(roomConfig.roomName);
        userListener.OnDataReceived += OnDataReceived;
        
        // Reseting the global blinking value. Can happen if the game unexpectedly exited while still blinking
        Shader.SetGlobalFloat("_BlinkingValue", 0.0f);

        if (MusicPlayer.instance.GetStem(0) != menuTheme)
		{
            MusicPlayer.instance.SetStem(0, menuTheme);
            StartCoroutine(MusicPlayer.instance.RestartAllStems());
        }

        character.SetActive(true);
    }

    private void OnDataReceived(UserDataDto userData)
    {
        roomConfig.username = userData.username;

        GameStateDto gameStateDto = new GameStateDto
        {
            state = GameStates.Game
        };
        gameStateSender.Send(gameStateDto);
    }

    public override void Exit(AState to)
    {
        userListener.Disconnect();
        
        character.SetActive(false);

        GameState gs = to as GameState;

        if (gs != null)
        {
			gs.currentModifier = m_CurrentModifier;
			
            // We reset the modifier to a default one, for next run (if a new modifier is applied, it will replace this default one before the run starts)
			m_CurrentModifier = new Modifier();
        }
        
        gameObject.SetActive(false);
    }

    public override string GetName()
    {
        return "Loadout";
    }

    public override void Tick()
    {
        if (character != null)
        {
            character.transform.Rotate(0, k_CharacterRotationSpeed * Time.deltaTime, 0, Space.Self);
        }

		charSelect.gameObject.SetActive(PlayerData.instance.characters.Count > 1);
    }
}
