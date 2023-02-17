using RootRacer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowDepthBehaviour : MonoBehaviour
{
	private Text legacyText;
	private TextMeshProUGUI tmpText;
	private bool useLegacyText;

	private void Awake()
	{
		tmpText = GetComponent<TextMeshProUGUI>();
		if (tmpText == null)
		{
			useLegacyText = true;
			legacyText = GetComponent<Text>();
			if (legacyText == null)
			{
				Debug.LogError($"No text component found on {gameObject.name}");
			}
		}
	}

	private void LateUpdate()
	{
		if (GameManager.Instance.isPaused)
		{
			return;
		}

		if (useLegacyText)
		{
			legacyText.text = GetDepth();
			return;
		}

		tmpText.text = GetDepth();
	}

	private string GetDepth()
	{
		return (-GameManager.Depth).ToString("0.#m");
	}
}