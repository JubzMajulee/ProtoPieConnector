using UnityEngine;

/// <summary>
/// A simple script to test receiving messages from ProtoPie Connect.
/// Attach this to a GameObject and link its methods in the ProtoPieConnector Inspector.
/// </summary>
public class ProtoPieReceiveTester : MonoBehaviour
{
    private Renderer _renderer;

    void Awake()
    {
        // Get the renderer so we can change colors dynamically
        _renderer = GetComponent<Renderer>();
    }

    /// <summary>
    /// Test method for 'onReceive' (no value).
    /// </summary>
    public void OnMessageReceived()
    {
        Debug.Log("✅ [ReceiveTester] Action Triggered without a value!");
    }

    /// <summary>
    /// Test method for 'onReceiveWithValue' (string value).
    /// </summary>
    public void OnMessageWithValueReceived(string val)
    {
        Debug.Log($"✅ [ReceiveTester] Action Triggered with value: '{val}'");

        if (_renderer != null)
        {
            // Simple visual feedback: Change color based on the string received
            Color newColor;
            if (ColorUtility.TryParseHtmlString(val, out newColor) || TryParseColorWord(val, out newColor))
            {
                _renderer.material.color = newColor;
                Debug.Log($"   -> Changed color to {val}");
            }
        }
    }

    // Helper to parse basic color words since Unity's HtmlString parser mostly expects hex codes for words
    private bool TryParseColorWord(string word, out Color color)
    {
        word = word.ToLower().Trim();
        color = Color.white;
        switch (word)
        {
            case "red": color = Color.red; return true;
            case "green": color = Color.green; return true;
            case "blue": color = Color.blue; return true;
            case "yellow": color = Color.yellow; return true;
            case "black": color = Color.black; return true;
            case "white": color = Color.white; return true;
            default: return false;
        }
    }
}
