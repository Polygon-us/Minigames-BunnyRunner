using System;
using UnityEngine;
using System.Collections.Generic;
using FirebaseCore;
using FirebaseCore.DTOs;
using Med.SafeValue;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;

/// <summary>
/// Handle everything related to controlling the character. Interact with both the Character (visual, animation) and CharacterCollider
/// </summary>
public class CharacterInputController : MonoBehaviour
{
    static int s_DeadHash = Animator.StringToHash ("Dead");
	static int s_RunStartHash = Animator.StringToHash("runStart");
	static int s_MovingHash = Animator.StringToHash("Moving");
	static int s_JumpingHash = Animator.StringToHash("Jumping");
	static int s_JumpingSpeedHash = Animator.StringToHash("JumpSpeed");
	static int s_SlidingHash = Animator.StringToHash("Sliding");

	public TrackManager trackManager;
	public Character character;
	public CharacterCollider characterCollider;
	public GameObject blobShadow;
	public float laneChangeSpeed = 1.0f;

	private SafeInt m_maxLife = new (3);

	public Consumable inventory;

	public int maxLife => m_maxLife.Value;
	public int coins { get { return m_Coins.Value; } set { m_Coins.Value = value; } }
	public int premium { get { return m_Premium.Value; } set { m_Premium.Value = value; } }
	public int currentLife { get { return m_CurrentLife.Value; } set { m_CurrentLife.Value = value; } }
	public List<Consumable> consumables { get { return m_ActiveConsumables; } }
	public bool isJumping { get { return m_Jumping; } }
	public bool isSliding { get { return m_Sliding; } }

	[Header("Controls")]
	public float jumpLength = 2.0f;     // Distance jumped
	public float jumpHeight = 1.2f;

	public float slideLength = 2.0f;

	[Header("Sounds")]
	public AudioClip slideSound;
	public AudioClip powerUpUseSound;
	public AudioSource powerupSource;

    public int currentTutorialLevel;
    [HideInInspector] public bool tutorialWaitingForValidation;

    protected SafeInt m_Coins;
    protected SafeInt m_Premium;
    protected SafeInt m_CurrentLife;

    protected List<Consumable> m_ActiveConsumables = new List<Consumable>();

    protected int m_ObstacleLayer;

	protected bool m_IsInvincible;
	protected bool m_IsRunning;
	
    protected float m_JumpStart;
    protected bool m_Jumping;

	protected bool m_Sliding;
	protected float m_SlideStart;

	protected AudioSource m_Audio;

    protected int m_CurrentLane = k_StartingLane;
    protected Vector3 m_TargetPosition = Vector3.zero;

    protected readonly Vector3 k_StartingPosition = Vector3.forward * 2f;

    protected Vector2 m_StartingTouch;
	protected bool m_IsSwiping = false;
	private InputAction moveAction;
	private InputAction touchAction;
	private InputAction swipeAction;
	
    protected const int k_StartingLane = 1;
    protected const float k_GroundingSpeed = 80f;
    protected const float k_ShadowRaycastDistance = 100f;
    protected const float k_ShadowGroundOffset = 0.01f;
    protected const float k_TrackSpeedToJumpAnimSpeedRatio = 0.6f;
    protected const float k_TrackSpeedToSlideAnimSpeedRatio = 0.9f;

    protected void Awake ()
    {
        m_Premium = new SafeInt(0);
        m_CurrentLife = new SafeInt(0);
        m_Sliding = false;
        m_SlideStart = 0.0f;
	    m_IsRunning = false;
	    
	    moveAction = InputSystem.actions.FindAction("Move");
	    touchAction = InputSystem.actions.FindAction("Touch");
	    swipeAction = InputSystem.actions.FindAction("Swipe");
    }

    private void OnDisable()
    {
	    FirebaseConnection.OnMovementInput -= OnMovementInput;
    }

    // Cheating functions, use for testing
	public void CheatInvincible(bool invincible)
	{
		m_IsInvincible = invincible;
    }

	public bool IsCheatInvincible()
	{
		return m_IsInvincible;
	}

    public void Init()
    {
        transform.position = k_StartingPosition;
		m_TargetPosition = Vector3.zero;

		m_CurrentLane = k_StartingLane;
		characterCollider.transform.localPosition = Vector3.zero;

        currentLife = maxLife;

		m_Audio = GetComponent<AudioSource>();

		m_ObstacleLayer = 1 << LayerMask.NameToLayer("Obstacle");
    }

	// Called at the beginning of a run or rerun
	public void Begin()
	{
		m_IsRunning = false;
        character.animator.SetBool(s_DeadHash, false);

		characterCollider.Init();

		m_ActiveConsumables.Clear();
	    
		FirebaseConnection.OnMovementInput += OnMovementInput;
	}

	public void End()
	{
		FirebaseConnection.OnMovementInput -= OnMovementInput;
		
        CleanConsumable();
    }

    public void CleanConsumable()
    {
	    foreach (var consumable in m_ActiveConsumables)
	    {
		    consumable.Ended(this);
		    Addressables.ReleaseInstance(consumable.gameObject);
	    }

	    m_ActiveConsumables.Clear();
    }

    public void StartRunning()
    {   
	    StartMoving();
	    
	    if (!character.animator)
		    return;
	    
	    character.animator.Play(s_RunStartHash);
        character.animator.SetBool(s_MovingHash, true);
    }

	public void StartMoving()
	{
		m_IsRunning = true;
		
		m_IsSwiping = false;
	}

    public void StopMoving()
    {
	    m_IsRunning = false;
        trackManager.StopMove();
     
        if (character.animator)
            character.animator.SetBool(s_MovingHash, false);
    }

    protected bool TutorialMoveCheck(int tutorialLevel)
    {
        tutorialWaitingForValidation = currentTutorialLevel != tutorialLevel;

        return !TrackManager.instance.isTutorial || currentTutorialLevel >= tutorialLevel;
    }

    private void OnMovementInput(int direction)
    {
	    // 1 -> jump
	    // 2 -> slide
	    // 3 -> left
	    // 4 -> right
	    
	    switch (direction)
	    {
		    case 1 when TutorialMoveCheck(1):
			    Jump();
			    break;
		    case 2 when TutorialMoveCheck(2):
			    if (!m_Sliding)
				    Slide();
			    break;
		    case 3 when TutorialMoveCheck(0):
			    ChangeLane(-1);
			    break;
		    case 4 when TutorialMoveCheck(0):
			    ChangeLane(1);
			    break;
	    }
    }
    
	protected void Update()
	{
		// Disabled if it's tutorial and not thec urrent right tutorial level (see func TutorialMoveCheck)
		Vector2 moveValue = moveAction.ReadValue<Vector2>();

		if (moveAction.WasPressedThisFrame())
		{
			if (Mathf.Abs(moveValue.x) > Mathf.Abs(moveValue.y))
			{
				switch (moveValue.x)
				{
					case < 0 when TutorialMoveCheck(0):
						ChangeLane(-1);
						break;
					case > 0 when TutorialMoveCheck(0):
						ChangeLane(1);
						break;
				}
			}
			else
			{
				switch (moveValue.y)
				{
					case > 0 when TutorialMoveCheck(1):
						Jump();
						break;
					case < 0 when TutorialMoveCheck(2):
					{
						if (!m_Sliding)
							Slide();
						break;
					}
				}
			}
		}

		// Use touch input on mobile
		if (touchAction.IsInProgress())
		{
			// Input check is AFTER the swipe test, that way if TouchPhase.Ended happen a single frame after the Began Phase
			// a swipe can still be registered (otherwise, m_IsSwiping will be set to false and the test wouldn't happen for that began-Ended pair)

			if (touchAction.WasPerformedThisFrame())
			{
				m_StartingTouch = swipeAction.ReadValue<Vector2>();
				m_IsSwiping = true;
			}
			else if (touchAction.WasReleasedThisFrame())
			{
				m_IsSwiping = false;
			}

			if (m_IsSwiping)
			{
				Vector2 diff = swipeAction.ReadValue<Vector2>() - m_StartingTouch;

				// Put difference in Screen ratio, but using only width, so the ratio is the same on both
				// axes (otherwise we would have to swipe more vertically...)
				diff = new Vector2(diff.x / Screen.width, diff.y / Screen.width);

				if (diff.magnitude > 0.05f) //we set the swip distance to trigger movement to 5% of the screen width
				{
					if (Mathf.Abs(diff.y) > Mathf.Abs(diff.x))
					{
						if (TutorialMoveCheck(2) && diff.y < 0)
						{
							Slide();
						}
						else if (TutorialMoveCheck(1))
						{
							Jump();
						}
					}
					else if (TutorialMoveCheck(0))
					{
						if (diff.x < 0)
						{
							ChangeLane(-1);
						}
						else
						{
							ChangeLane(1);
						}
					}

					m_IsSwiping = false;
				}
			}
		}
		
		Vector3 verticalTargetPosition = m_TargetPosition;

		UpdateSliding();
		UpdateJump(ref verticalTargetPosition);
		
		characterCollider.transform.localPosition = Vector3.MoveTowards(characterCollider.transform.localPosition, verticalTargetPosition, laneChangeSpeed * Time.deltaTime);

		SetBlobShadow();
	}
	
// #elif UNITY_WEBGL
//     protected void Update ()
//     {
//         Vector3 verticalTargetPosition = m_TargetPosition;
//
// 		UpdateSliding();
// 		UpdateJump(ref verticalTargetPosition);
// 		
//         characterCollider.transform.localPosition = Vector3.MoveTowards(characterCollider.transform.localPosition, verticalTargetPosition, laneChangeSpeed * Time.deltaTime);
//
// 		SetBlobShadow();
// 	}
// #endif
	    
	private void UpdateSliding()
	{
		if (!m_Sliding)
			return;
		
		// Slide time isn't constant but the slide length is (even if slightly modified by speed, to slide slightly further when faster).
		// This is for gameplay reason, we don't want the character to drasticly slide farther when at max speed.
		float correctSlideLength = slideLength * (1.0f + trackManager.speedRatio);
		float ratio = (trackManager.worldDistance - m_SlideStart) / correctSlideLength;
		
		if (ratio >= 1.0f)
		{
			// We slid to (or past) the required length, go back to running
			StopSliding();
		}
	}

	private void UpdateJump(ref Vector3 verticalTargetPosition)
	{
		if (!m_Jumping)
			return;
		
		if (trackManager.isMoving)
		{
			// Same as with the sliding, we want a fixed jump LENGTH not fixed jump TIME. Also, just as with sliding,
			// we slightly modify length with speed to make it more playable.
			float correctJumpLength = jumpLength * (1.0f + trackManager.speedRatio);
			float ratio = (trackManager.worldDistance - m_JumpStart) / correctJumpLength;
			if (ratio >= 1.0f)
			{
				m_Jumping = false;
				character.animator.SetBool(s_JumpingHash, false);
			}
			else
			{
				verticalTargetPosition.y = Mathf.Sin(ratio * Mathf.PI) * jumpHeight;
			}
		}
		else if(!AudioListener.pause)//use AudioListener.pause as it is an easily accessible singleton & it is set when the app is in pause too
		{
			verticalTargetPosition.y = Mathf.MoveTowards (verticalTargetPosition.y, 0, k_GroundingSpeed * Time.deltaTime);
			if (Mathf.Approximately(verticalTargetPosition.y, 0f))
			{
				character.animator.SetBool(s_JumpingHash, false);
				m_Jumping = false;
			}
		}
	}

	private void SetBlobShadow()
	{
		// Put blob shadow under the character.
		RaycastHit hit;
		if(Physics.Raycast(characterCollider.transform.position + Vector3.up, Vector3.down, out hit, k_ShadowRaycastDistance, m_ObstacleLayer))
		{
			blobShadow.transform.position = hit.point + Vector3.up * k_ShadowGroundOffset;
		}
		else
		{
			Vector3 shadowPosition = characterCollider.transform.position;
			shadowPosition.y = k_ShadowGroundOffset;
			blobShadow.transform.position = shadowPosition;
		}
	}
	
    public void Jump()
    {
	    if (!m_IsRunning)
		    return;

	    if (m_Jumping)
		    return;
	    
	    if (m_Sliding)
	        StopSliding();

        float correctJumpLength = jumpLength * (1.0f + trackManager.speedRatio);
        m_JumpStart = trackManager.worldDistance;
        float animSpeed = k_TrackSpeedToJumpAnimSpeedRatio * (trackManager.speed / correctJumpLength);

        character.animator.SetFloat(s_JumpingSpeedHash, animSpeed);
        character.animator.SetBool(s_JumpingHash, true);
        character.Source.PlayOneShot(character.jumpSound);
        m_Jumping = true;
    }

    public void StopJumping()
    {
	    if (!m_Jumping)
		    return;
	    
	    character.animator.SetBool(s_JumpingHash, false);
	    m_Jumping = false;
    }

	public void Slide()
	{
		if (!m_IsRunning)
			return;

		if (m_Sliding) 
			return;
		
		if (m_Jumping)
			StopJumping();

		float correctSlideLength = slideLength * (1.0f + trackManager.speedRatio); 
		m_SlideStart = trackManager.worldDistance;
		float animSpeed = k_TrackSpeedToJumpAnimSpeedRatio * (trackManager.speed / correctSlideLength);

		character.animator.SetFloat(s_JumpingSpeedHash, animSpeed);
		character.animator.SetBool(s_SlidingHash, true);
		m_Audio.PlayOneShot(slideSound);
		m_Sliding = true;

		characterCollider.Slide(true);
	}

	public void StopSliding()
	{
		if (!m_Sliding) 
			return;
		
		character.animator.SetBool(s_SlidingHash, false);
		m_Sliding = false;

		characterCollider.Slide(false);
	}

	public void ChangeLane(int direction)
    {
		if (!m_IsRunning)
			return;

        int targetLane = m_CurrentLane + direction;

        if (targetLane is < 0 or > 2)
            // Ignore, we are on the borders.
            return;

        m_CurrentLane = targetLane;
        m_TargetPosition = new Vector3((m_CurrentLane - 1) * trackManager.laneOffset, 0, 0);
    }

    public void UseInventory()
    {
	    if (!inventory || !inventory.CanBeUsed(this))
		    return;
	    
	    UseConsumable(inventory);
	    
	    inventory = null;
    }

    public void UseConsumable(Consumable c)
    {
		characterCollider.audio.PlayOneShot(powerUpUseSound);

        foreach (var consumable in m_ActiveConsumables)
        {
	        if (consumable.GetType() != c.GetType())
		        continue;
	        
	        // If we already have an active consumable of that type, we just reset the time
	        consumable.ResetTime();
	        Addressables.ReleaseInstance(c.gameObject);
	        
	        return;
        }

        // If we didn't had one, activate that one 
        c.transform.SetParent(transform, false);
        c.gameObject.SetActive(false);

        m_ActiveConsumables.Add(c);
        StartCoroutine(c.Started(this));
    }
}
