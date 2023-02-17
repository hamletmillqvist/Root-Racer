using RootRacer.Utils;
using Sonity;
using UnityEngine;

namespace RootRacer
{
	/* Todo: This behaviour is growing to become quite complex.
	 * Let's try refactor it if more methods are added.
	 * This is to prevent the GOD-CLASS code smell!
	 */

	[RequireComponent(typeof(CircleCollider2D))]
	public class PlayerController : MonoBehaviour
	{
		public KeyCode moveLeft;
		public KeyCode moveRight;
		public Color playerColor;
		public float horizontalMoveSpeed;
		public float downSpeed;
		public float boostReduceAmount;
		public float baseEatAnimationSpeed = 3;
		public float minDistanceForLineUpdate = 0.1f;
		public int linePositions = 50;

		[Header("Player Effects")] public float invertTime = 5;
		public float shieldTime = 5;
		public float sizeMultiplier = 2;
		public float sizeTime = 5;
		public bool hasGodMode = false;
		public Sprite[] winFaces;
		[Header("Sounds")] [SerializeField] private SoundEvent footstepsSoundEvent;
		[SerializeField] private SoundEvent deathSoundEvent;

		private Animator headAnimator;
		private new Camera camera;
		private Vector2 screenSize;
		private GameManager gameManager;
		private float invertTimer;
		private float shieldTimer;
		private float sizeTimer;
		private Vector3 baseSizeScale;
		private bool invertControls;
		private bool hasShield;
		private bool hasSizeUp;
		private Vector3 startPosition;
		private LineRenderer lineRenderer;
		private GameObject shieldObject;
		public GameObject shieldPop;

		public CircleCollider2D CircleCollider2D { get; set; }

		private static readonly int PlayerColor = Shader.PropertyToID("_PlayerColor");
		private static readonly int AnimationMultiplier = Animator.StringToHash("AnimationMultiplier");

		private void Awake()
		{
			var transformReference = transform;
			startPosition = transformReference.position;
			baseSizeScale = transformReference.localScale;

			camera = FindObjectOfType<Camera>();
			gameManager = FindObjectOfType<GameManager>();
			headAnimator = GetComponentInChildren<Animator>();
			lineRenderer = GetComponentInChildren<LineRenderer>();

			GameManager.Instance.AddPlayer(this);

			CircleCollider2D = GetComponent<CircleCollider2D>();
			CollisionSystemUtil.RegisterPlayer(CircleCollider2D);

			RegisterPauseEvents();
		}

		private void Start()
		{
			downSpeed = gameManager.GetTargetSpeed();
			lineRenderer.positionCount = linePositions;
			lineRenderer.material.SetColor(PlayerColor, playerColor);
			ResetPlayer();
		}

		private void OnPause()
		{
			footstepsSoundEvent.Stop(transform);
		}

		private void OnUnPause()
		{
			footstepsSoundEvent.Play(transform);
		}

		private void Update()
		{
			if (gameManager.isPaused)
			{
				return;
			}

			var deltaTime = Time.deltaTime;

			EffectTimers(deltaTime);

			var downSpeed = gameManager.GetTargetSpeed();
			var aMulti = (downSpeed + baseEatAnimationSpeed) / baseEatAnimationSpeed;
			headAnimator.SetFloat(AnimationMultiplier, aMulti);

			HandleHorizontalMovement(deltaTime);
			HandleVerticalMovement(deltaTime);

			UpdateLine(deltaTime);

			NormalizeDownSpeed(deltaTime);

			HandleTouchedItems();

			if (GetIsOutsideOfScreen())
			{
				KillPlayer();
			}
		}

		public void KillPlayer()
		{
			deathSoundEvent?.Play(gameManager.transform);
			footstepsSoundEvent.Stop(transform);
			GameManager.RemovePlayer(this);
			Destroy(gameObject);
		}

		private void EffectTimers(float deltaTime)
		{
			// Todo: Effects should be turned into a list of objects that handle themselves.
			if (invertControls)
			{
				invertTimer -= deltaTime;
				if (invertTimer <= 0)
				{
					invertControls = false;
				}
			}

			if (hasShield)
			{
				shieldTimer -= deltaTime;
				if (shieldTimer <= 0)
				{
					DestroyShield();
				}
			}

			if (hasSizeUp)
			{
				sizeTimer -= deltaTime;
				if (sizeTimer <= 0)
				{
					hasSizeUp = false;
					transform.localScale = baseSizeScale;
				}
			}
		}

		private void OnDestroy()
		{
			CollisionSystemUtil.UnregisterPlayer(this);
			UnregisterPauseEvents();
		}

		private void RegisterPauseEvents()
		{
			gameManager.OnGamePause += OnPause;
			gameManager.OnGameUnPause += OnUnPause;
		}

		private void UnregisterPauseEvents()
		{
			gameManager.OnGamePause -= OnPause;
			gameManager.OnGameUnPause -= OnUnPause;
		}

		private void HandleTouchedItems()
		{
			var touchedItems = CollisionSystemUtil.GetTouchedItems(CircleCollider2D);
			foreach (var touchedItem in touchedItems)
			{
				touchedItem.TriggerEffect(this);
			}
		}

		public void ResetPlayer()
		{
			ResetTransform();
			ResetSpeed();
			ResetStatusEffects();
			ResetLineRenderers();
		}

		private void ResetLineRenderers()
		{
			var pos = transform.position;
			for (var i = 0; i < lineRenderer.positionCount; i++)
			{
				lineRenderer.SetPosition(i, pos);
			}
		}

		private void ResetTransform()
		{
			var transformReference = transform;
			transformReference.position = startPosition;
			transformReference.localScale = baseSizeScale;
		}

		private void ResetSpeed()
		{
			downSpeed = gameManager.GetTargetSpeed();
		}

		private void ResetStatusEffects()
		{
			invertControls = false;
			hasSizeUp = false;

			hasShield = false;
			if (shieldObject)
			{
				Destroy(shieldObject);
			}
		}

		[ContextMenu("Stun Player")]
		public void StunPlayer()
		{
			if (hasGodMode)
			{
				return;
			}

			if (hasShield)
			{
				DestroyShield();
				return;
			}

			downSpeed = gameManager.GetTargetSpeed() * 100;
		}

		[ContextMenu("Speed Player")]
		public void SpeedUp(float amount)
		{
			downSpeed -= amount;
		}

		[ContextMenu("Invert Controls")]
		public void InvertControls()
		{
			if (hasGodMode)
			{
				return;
			}

			invertTimer = invertTime;
			invertControls = true;
		}

		public void Shield()
		{
			shieldTimer = shieldTime;
			if (!hasShield)
			{
				shieldObject = Instantiate(GameManager.ShieldPrefab, transform);
			}

			hasShield = true;
		}

		private void DestroyShield()
		{
			Destroy(shieldObject);
			Destroy(Instantiate(shieldPop, transform), 1f);
			hasShield = false;
		}

		public void SizeUp()
		{
			if (hasGodMode)
			{
				return;
			}

			transform.localScale = baseSizeScale * sizeMultiplier;
			hasSizeUp = true;
			sizeTimer = sizeTime;
		}

		private void NormalizeDownSpeed(float deltaTime)
		{
			var targetSpeed = gameManager.GetTargetSpeed();

			var isCloseEnough = downSpeed.IsCloseEnough(targetSpeed, acceptedDifference: 0.001f);
			if (isCloseEnough)
			{
				downSpeed = targetSpeed;
				return;
			}

			downSpeed = Mathf.MoveTowards(downSpeed, targetSpeed, boostReduceAmount * deltaTime);
		}

		private void UpdateLine(float deltaTime)
		{
			for (var i = 0; i < lineRenderer.positionCount; i++)
			{
				lineRenderer.SetPosition(
					index: i,
					position: lineRenderer.GetPosition(i) +
					          new Vector3(0, gameManager.GetTargetSpeed() * 100 * deltaTime, 0));
			}

			var lastPoint = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
			if (Vector3.Distance(lastPoint, transform.position) < minDistanceForLineUpdate)
			{
				return;
			}

			for (var i = 0; i < lineRenderer.positionCount - 1; i++)
			{
				lineRenderer.SetPosition(i, lineRenderer.GetPosition(i + 1));
			}

			lineRenderer.SetPosition((lineRenderer.positionCount - 1), transform.position);
		}

		private void HandleVerticalMovement(float deltaTime)
		{
			var deltaY = downSpeed - gameManager.GetTargetSpeed();

			if (deltaY == 0)
			{
				return;
			}

			transform.position += new Vector3(0, deltaY * deltaTime, 0);
			ClampVerticalPosition();
		}

		private void ClampVerticalPosition()
		{
			var position = transform.position;
			var minScreenBounds = camera.ScreenToWorldPoint(Vector3.zero);
			var maxScreenBounds = camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
			var radius = CircleCollider2D.radius * transform.localScale.y;
			position.y = Mathf.Clamp(position.y, minScreenBounds.y + radius, maxScreenBounds.y + radius * 2);
			transform.position = position;
		}

		private void HandleHorizontalMovement(float deltaTime)
		{
			float movementX = 0;
			if (Input.GetKey(moveLeft))
			{
				movementX -= invertControls ? -1 : 1;
			}

			if (Input.GetKey(moveRight))
			{
				movementX += invertControls ? -1 : 1;
			}

			UpdateHorizontalPosition(deltaTime * movementX);
		}

		private void UpdateHorizontalPosition(float movementDirectionDelta)
		{
			var position = transform.position;

			position += new Vector3(horizontalMoveSpeed * movementDirectionDelta, 0, 0);

			var minScreenBounds = camera.ScreenToWorldPoint(Vector3.zero);
			var maxScreenBounds = camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
			var radius = CircleCollider2D.radius * transform.localScale.x;
			position.x = Mathf.Clamp(position.x, minScreenBounds.x + radius, maxScreenBounds.x - radius);

			transform.position = position;
		}

		private bool GetIsOutsideOfScreen()
		{
			var screenPoint = camera.WorldToScreenPoint(transform.position);
			return screenPoint.y > Screen.height;
		}
	}
}