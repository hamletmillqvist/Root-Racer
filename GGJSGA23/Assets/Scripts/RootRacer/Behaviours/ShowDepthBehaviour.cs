using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RootRacer;

public class ShowDepthBehaviour : MonoBehaviour
{
    private Text legacyText;
    private TextMeshProUGUI tmpText;
    bool useLegacyText = false;
    

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
        if (GameManager.instance.isPaused)
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
    string GetDepth()
    {

        return (-GameManager.Depth).ToString("0.#m");
    }
}