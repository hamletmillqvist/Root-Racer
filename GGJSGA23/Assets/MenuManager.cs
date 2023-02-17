using System;
using System.Linq;
using RootRacer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
	[SerializeField] private Canvas menuCanvas;
	[SerializeField] private Canvas creditsCanvas;
	[SerializeField] private Canvas placingsCanvas;

	public Image logo;
	public Sprite[] logos;
	public int players = 2;
	public Image[] placingsImages;

	[SerializeField] private string nextScene;
	[SerializeField] private string twoPlayerScene;
	[SerializeField] private string threePlayerScene;
	[SerializeField] private string fourPlayerScene;

	private void Start()
	{
		DontDestroyOnLoad(this.gameObject);
	}

	public void LoadNextScene()
	{
		SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
	}

	public void StartGame()
	{
		placingsCanvas.gameObject.SetActive(false);
		menuCanvas.gameObject.SetActive(false);

		var playerScene = players switch
		{
			2 => twoPlayerScene,
			3 => threePlayerScene,
			4 => fourPlayerScene,
			_ => throw new IndexOutOfRangeException("Invalid player count.")
		};

		SceneManager.LoadScene(nextScene);
		SceneManager.LoadScene(playerScene, LoadSceneMode.Additive);

		placingsCanvas.gameObject.SetActive(false);
		menuCanvas.gameObject.SetActive(false);
	}

	public void QuitGame()
	{
		Application.Quit();
	}

	[Obsolete]
	public void RestartGame()
	{
		GameManager.Instance.StartGame();
	}

	public void ShowCredits()
	{
		creditsCanvas.gameObject.SetActive(true);
		menuCanvas.gameObject.SetActive(false);
	}

	public void GoBack()
	{
		SceneManager.LoadScene("MainMenu");
		placingsCanvas.gameObject.SetActive(false);
		Destroy(gameObject);
	}

	public void ShowGameOver(PlayerController playerController)
	{
		SetPlace(0, playerController);
		Debug.Log(GameManager.Instance.playerDeaths.Count);

		for (var i = 1; i <= 3 && GameManager.Instance.playerDeaths.Any(); i++)
		{
			SetPlace(i);
		}

		placingsCanvas.gameObject.SetActive(true);
	}

	private void SetPlace(int i, PlayerController player)
	{
		placingsImages[i].sprite = player.winFaces[i];
		SetPlaceColorVisible(i);
	}

	private void SetPlace(int i)
	{
		placingsImages[i].sprite = GameManager.Instance.playerDeaths.Pop().Player.winFaces[i];
		SetPlaceColorVisible(i);
	}

	private void SetPlaceColorVisible(int i)
	{
		var color = placingsImages[i].color;
		color.a = 1f;
		placingsImages[i].color = color;
	}

	public void SelectPlayers(int players)
	{
		logo.sprite = logos[players - 2];
		this.players = players;
	}
}