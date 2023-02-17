using System.Collections.Generic;
using UnityEngine;

namespace RootRacer
{
	public class GameManager : MonoBehaviour
	{
		public delegate void GamePauseDelegate();

		public event GamePauseDelegate OnGamePause;
		public event GamePauseDelegate OnGameUnPause;

		public static GameManager Instance;
		public static Camera MainCamera;

		public static IReadOnlyList<PlayerController> Players => Instance.players;
		public static float Depth => Instance.depth;
		public static GameObject ShieldPrefab => Instance.shieldPrefab;

		public DepthMusicSO gameDepthMusic;
		public MeshRenderer worldMeshRenderer;
		public bool isPaused;

		[SerializeField] private float startSpeed = 0.05f;
		[SerializeField] private float speedIncrease = 0.1f;
		[SerializeField] private GameObject shieldPrefab;

		private Material worldMaterial;
		private float depth;
		private float currentSpeed = 0.5f;
		private int shaderPropID;
		private List<PlayerController> players;
		public Stack<PlayerDeathInfo> playerDeaths;
		private MenuManager menuManager;
		private int currentlyPlayingDepthMusic;

		private void Awake()
		{
			MainCamera = FindObjectOfType<Camera>();
			players = new List<PlayerController>();
			playerDeaths = new Stack<PlayerDeathInfo>();
			menuManager = FindObjectOfType<MenuManager>();

			if (menuManager == null)
			{
				Debug.LogError("No menuManager in scene");
			}

			Instance = this;
			isPaused = true;
			Time.timeScale = 0;

			worldMaterial = worldMeshRenderer.material;
		}

		public void AddPlayer(PlayerController playerController)
		{
			if (!players.Contains(playerController))
			{
				players.Add(playerController);
			}
		}

		private void Start()
		{
			shaderPropID = worldMaterial.shader.GetPropertyNameId(worldMaterial.shader.FindPropertyIndex("_Position"));

			StartGame();
		}

		private void Update()
		{
			if (isPaused)
			{
				return;
			}

			ScrollWorld(Time.deltaTime);
			CheckDepthMusic(depth);
			CollisionSystemUtil.UpdateCollisions();
		}

		private void CheckDepthMusic(float depth)
		{
			var selectedIndex = currentlyPlayingDepthMusic;

			for (var i = currentlyPlayingDepthMusic; i < gameDepthMusic.gameDepthMusic.Length; i++)
			{
				var depthMusic = gameDepthMusic.gameDepthMusic[i];
				if (depth > depthMusic.depth)
				{
					selectedIndex = i;
				}
			}

			if (selectedIndex == currentlyPlayingDepthMusic)
			{
				return;
			}

			gameDepthMusic.gameDepthMusic[currentlyPlayingDepthMusic].music.Stop2D();
			gameDepthMusic.gameDepthMusic[selectedIndex].music.Play2D();
			currentlyPlayingDepthMusic = selectedIndex;
		}

		public float GetTargetSpeed() => currentSpeed;

		private void ScrollWorld(float deltaTime)
		{
			depth -= deltaTime * currentSpeed;
			currentSpeed += speedIncrease * deltaTime;
			worldMaterial.SetVector(shaderPropID, new Vector2(0, depth));
		}

		[ContextMenu("Start")]
		public void StartGame()
		{
			ResetGame();
			UnPauseGame();
		}

		private void UnPauseGame()
		{
			Time.timeScale = 1;
			isPaused = false;
			OnGameUnPause?.Invoke();
			gameDepthMusic.gameDepthMusic[currentlyPlayingDepthMusic].music.Play2D();
		}

		private void PauseGame()
		{
			OnGamePause?.Invoke();
			gameDepthMusic.gameDepthMusic[currentlyPlayingDepthMusic].music.Stop2D();
			isPaused = true;
			Time.timeScale = 0;
		}

		public void ResetGame()
		{
			currentSpeed = startSpeed;
			depth = 0;
			var players = FindObjectsOfType<PlayerController>();
			foreach (var player in players)
			{
				player.ResetPlayer();
			}
		}

		public static void RemovePlayer(PlayerController playerController)
		{
			Instance.players.Remove(playerController);
			Instance.playerDeaths.Push(new PlayerDeathInfo
			{
				Depth = Instance.depth,
				Player = playerController,
			});

			CollisionSystemUtil.UnregisterPlayer(playerController);

			if (Instance.players.Count == 1)
			{
				Instance.GameOver(Instance.players[0]);
			}
		}

		public void GameOver(PlayerController playerWin)
		{
			PauseGame();
			menuManager.ShowGameOver(playerWin);
		}
	}
}