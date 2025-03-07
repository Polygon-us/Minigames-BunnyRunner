using System.Collections.Generic;
using FirebaseCore.DTOs;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class HighscoreUI : MonoBehaviour
{
	[SerializeField] private int rank;

	[SerializeField] private Sprite defaultSprite;
	[SerializeField] private List<Sprite> rankSprites;
 	
	[SerializeField] private TMP_Text number;
	[SerializeField] private TMP_Text playerName;
	[SerializeField] private TMP_Text score;

	[SerializeField] private int[] textSizes;
	
	private	Image backgroundImage;

	private void Awake()
	{
		backgroundImage = GetComponent<Image>();
	}

	private void OnValidate()
	{
		if (!gameObject.activeInHierarchy)
			return;
		
		backgroundImage ??= GetComponent<Image>();
		
		backgroundImage.sprite = rank switch
		{
			1 => rankSprites[0],
			2 => rankSprites[1],
			3 => rankSprites[2],
			_ => defaultSprite
		};

		int textSize = rank switch
		{
			1 => textSizes[0],
			2 => textSizes[1],
			3 => textSizes[2],
			_ => textSizes[3]
		};
		
		playerName.enableAutoSizing = true;
		score.enableAutoSizing = true;
		number.enableAutoSizing = true;
		
		number.fontSizeMax = textSize;
		playerName.fontSizeMax = textSize;
		score.fontSizeMax = textSize;
	}

	public void Initialize(int rank, LeaderboardDto responseDto)
	{
		playerName.text = responseDto.username;
		number.text = rank.ToString();
		score.text = responseDto.score.ToString();
		
		this.rank = rank;
		
		OnValidate();
	}
}
