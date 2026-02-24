using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A simple script to test sending messages to ProtoPie Connect.
/// Attach this to a GameObject, set the payload string, and link the UnityEvent in the ProtoPieConnector Inspector.
/// </summary>
public class ProtoPieSendTester : MonoBehaviour
{
    [Header("Testing Configuration")]
    [Tooltip("The key to press to trigger sending the message.")]
    public KeyCode triggerKey = KeyCode.Space;
    
    [Tooltip("The string data that will be pulled by ProtoPieConnector using reflection.")]
    public string payloadData = "Hello from Unity!";

    [Header("Events (Link to ProtoPieConnector)")]
    public UnityEvent OnSendTriggered;

    void Update()
    {
        // Simple input detection to trigger the event
        if (Input.GetKeyDown(triggerKey))
        {
            Debug.Log($"📤 [SendTester] Key '{triggerKey}' pressed. Invoking send event.");
            
            // This invokes the UnityEvent.
            // ProtoPieConnector (if configured correctly) is listening to this event via reflection!
            OnSendTriggered?.Invoke();
        }
    }

    /// <summary>
    /// Optional: A method to update the payload data via UI buttons or other scripts before sending.
    /// </summary>
    public void SetPayload(string newPayload)
    {
        payloadData = newPayload;
        Debug.Log($"[SendTester] Payload updated to: '{payloadData}'");
    }
}
