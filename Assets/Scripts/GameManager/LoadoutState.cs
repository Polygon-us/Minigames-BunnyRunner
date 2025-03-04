using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Source.Handlers;
using TMPro;
using UI.DTOs;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// State pushed on the GameManager during the Loadout, when player select player, theme and accessories
/// Take care of init the UI, load all the data used for it etc.
/// </summary>
public class LoadoutState : AState
{
    [Header("Char UI")]
	public RectTransform charSelect;
	public Transform charPosition;
	
	public AudioClip menuTheme;

	[Header("Settings")] 
	[SerializeField] private TMP_Text maxScoreTxt;
	[SerializeField] private TMP_Text maxDistanceTxt;

    [Header("Prefabs")]
    public ConsumableIcon consumableIcon;

    protected GameObject m_Character;
    protected List<int> m_OwnedAccesories = new List<int>();
    protected bool m_IsLoadingCharacter;

	protected Modifier m_CurrentModifier = new Modifier();

    protected const float k_CharacterRotationSpeed = 45f;
    protected const float k_OwnedAccessoriesCharacterOffset = -0.1f;
    protected int k_UILayer;
    protected readonly Quaternion k_FlippedYAxisRotation = Quaternion.Euler (0f, 180f, 0f);

    public override void Enter(AState from)
    {
        k_UILayer = LayerMask.NameToLayer("UI");

        // Reseting the global blinking value. Can happen if the game unexpectedly exited while still blinking
        Shader.SetGlobalFloat("_BlinkingValue", 0.0f);

        if (MusicPlayer.instance.GetStem(0) != menuTheme)
		{
            MusicPlayer.instance.SetStem(0, menuTheme);
            StartCoroutine(MusicPlayer.instance.RestartAllStems());
        }

        Refresh();
    }

    public override void Exit(AState to)
    {
        if (m_Character)
	        // Addressables.ReleaseInstance(m_Character);
			m_Character.gameObject.SetActive(false);

        GameState gs = to as GameState;

        if (gs != null)
        {
			gs.currentModifier = m_CurrentModifier;
			
            // We reset the modifier to a default one, for next run (if a new modifier is applied, it will replace this default one before the run starts)
			m_CurrentModifier = new Modifier();
        }
    }

    public void Refresh()
    {
        StartCoroutine(PopulateCharacters());
    }

    public override string GetName()
    {
        return "Loadout";
    }

    public override void Tick()
    {
        if (m_Character != null)
        {
            m_Character.transform.Rotate(0, k_CharacterRotationSpeed * Time.deltaTime, 0, Space.Self);
        }

		charSelect.gameObject.SetActive(PlayerData.instance.characters.Count > 1);
    }

    public IEnumerator PopulateCharacters()
    {
        PlayerData.instance.usedAccessory = -1;

        if (!m_IsLoadingCharacter)
        {
            m_IsLoadingCharacter = true;
            GameObject newChar = null;
            while (newChar == null)
            {
                Character c = CharacterDatabase.GetCharacter(PlayerData.instance.characters[PlayerData.instance.usedCharacter]);

                if (c != null)
                {
                    m_OwnedAccesories.Clear();
                    for (int i = 0; i < c.accessories.Length; ++i)
                    {
						// Check which accessories we own.
                        string compoundName = c.characterName + ":" + c.accessories[i].accessoryName;
                        if (PlayerData.instance.characterAccessories.Contains(compoundName))
                        {
                            m_OwnedAccesories.Add(i);
                        }
                    }

                    Vector3 pos = charPosition.transform.position;
                    if (m_OwnedAccesories.Count > 0)
                    {
                        pos.x = k_OwnedAccessoriesCharacterOffset;
                    }
                    else
                    {
                        pos.x = 0.0f;
                    }
                    charPosition.transform.position = pos;
                    
                    AsyncOperationHandle op = Addressables.InstantiateAsync(c.characterName);
                    yield return op;
                    if (op.Result == null || !(op.Result is GameObject))
                    {
                        Debug.LogWarning(string.Format("Unable to load character {0}.", c.characterName));
                        yield break;
                    }
                    newChar = op.Result as GameObject;
                    Helpers.SetRendererLayerRecursive(newChar, k_UILayer);
					newChar.transform.SetParent(charPosition, false);
                    newChar.transform.rotation = k_FlippedYAxisRotation;

                    if (m_Character != null)
                        Addressables.ReleaseInstance(m_Character);

                    m_Character = newChar;

                    m_Character.transform.localPosition = Vector3.right * 1000;
                    //animator will take a frame to initialize, during which the character will be in a T-pose.
                    //So we move the character off screen, wait that initialised frame, then move the character back in place.
                    //That avoid an ugly "T-pose" flash time
                    yield return new WaitForEndOfFrame();
                    m_Character.transform.localPosition = Vector3.zero;

                    SetupAccessory();
                }
                else
                    yield return new WaitForSeconds(1.0f);
            }
            m_IsLoadingCharacter = false;
        }
	}

    void SetupAccessory()
    {
        Character c = m_Character.GetComponent<Character>();
        c.SetupAccesory(PlayerData.instance.usedAccessory);
    }

    public void StartGame()
    {
	    SaveUserInfoDto saveUserInfoDto = BaseHandler.SaveUserInfo;
        if (saveUserInfoDto.tutorial)
        {
            if (PlayerData.instance.ftueLevel == 1)
            {
                PlayerData.instance.ftueLevel = 2;
                PlayerData.instance.Save();
            }
        }

        LeanTween.delayedCall(0.3f,
	        () => manager.SwitchState("Game")
	    );
    }

}
