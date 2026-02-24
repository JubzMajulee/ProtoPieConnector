// --- IMPORTS ---
// Includes all the standard Unity functions and classes.
using UnityEngine;
// Required to use UnityEvents (the 'onReceive' fields in the Inspector).
using UnityEngine.Events;
// Includes basic .NET functionality (like Uri for the server address).
using System;
// Required for LINQ functions like .ToDictionary() (which we don't use in this version, but good to know).
using System.Linq;
// Required for using Lists and Dictionaries.
using System.Collections.Generic;
// Core classes from the SocketIOUnity package.
using SocketIOClient;
// The JSON handler that the SocketIO package will use.
using SocketIOClient.Newtonsoft.Json;
// Classes for handling raw JSON data (JToken).
using Newtonsoft.Json.Linq;

// --- DATA CLASS 1: MESSAGE MAPPING ---
// '[System.Serializable]' tells Unity to show this class in the Inspector.
// This is not a component; it's a data container.
[System.Serializable]
public class MessageMapping
{
    // A friendly name for the mapping, just for organization in the Inspector.
    public string mappingLabel = "NewLabel";
    // The 'messageId' that ProtoPie will send (e.g., "ChangeSeason", "ChangeView").
    public string messageId;

    // '[Header(...)]' creates a bold title in the Inspector.
    [Header("Action To Trigger (ProtoPie To Unity)")]
    // This is a UnityEvent that takes NO parameters.
    // You can assign functions to this (like ResetToDefault()).
    public UnityEvent onReceive;
    // This is a UnityEvent that takes ONE STRING parameter.
    // You can assign functions to this (like SwitchToEnvironment(string value)).
    public UnityEvent<string> onReceiveWithValue;
}

// --- DATA CLASS 2: PROTOPIE MESSAGE ---
// This class is a template for deserializing the JSON message from ProtoPie.
// It matches the structure { "messageId": "...", "value": "..." }.
public class ProtoPieMessage
{
    // The '[JsonProperty(...)]' attribute tells the JSON parser to match
    // this C# property with the JSON key "messageId".
    [Newtonsoft.Json.JsonProperty("messageId")]
    public string MessageId { get; set; }

    [Newtonsoft.Json.JsonProperty("value")]
    public string Value { get; set; }
}
// Note: The 'MessageDirection' enum has been removed for simplicity.

// --- MAIN CLASS: PROTOPIE CONNECTOR ---
// This is the main component you attach to a GameObject in your scene.
public class ProtoPieConnector : MonoBehaviour
{
    // --- SINGLETON PATTERN ---
    // A 'static' variable is shared by all instances of this class.
    // This 'Instance' allows any other script to easily access this connector
    // without needing a direct reference.
    public static ProtoPieConnector Instance { get; private set; }

    // --- INSPECTOR FIELDS ---
    // '[SerializeField]' makes a private variable visible in the Inspector.
    [SerializeField] private string serverURL = "http://localhost:9981";
    // This is the list of all mappings you will set up in the Inspector.
    [SerializeField] private List<MessageMapping> mappings = new List<MessageMapping>();

    // --- PRIVATE VARIABLES ---
    // We convert the 'mappings' list into a Dictionary for
    // much faster lookups. A dictionary search is instant (O(1)).
    private Dictionary<string, MessageMapping> _mappingLookup;
    // This is the main socket client object that handles the connection.
    private SocketIOUnity socket;

    // --- UNITY LIFECYCLE METHODS ---

    // 'Awake' is called once, right at the start (before 'Start').
    // It's used for initialization.
    void Awake()
    {
        // --- SINGLETON SETUP ---
        // If an 'Instance' already exists (e.g., from another scene)
        // and it's not this one, destroy this new one.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        // This is the one and only 'Instance'.
        Instance = this;
        // This tells Unity not to destroy this GameObject when a new scene loads.
        // This is crucial for a manager script that needs to persist.
        DontDestroyOnLoad(gameObject);

        // Convert our Inspector list into the fast dictionary.
        InitializeMappings();
    }

    // 'Start' is called after 'Awake'.
    void Start()
    {
        // Begin the connection process to the ProtoPie Connect server.
        ConnectToServer();
    }

    // 'OnDestroy' is called when the GameObject is destroyed (e.g., when you stop Play Mode).
    void OnDestroy()
    {
        // Always disconnect the socket to prevent memory leaks or errors.
        if (socket != null) socket.Disconnect();
    }

    // --- CORE LOGIC METHODS ---

    /// <summary>
    /// Converts the public 'mappings' list into the private '_mappingLookup' dictionary
    /// for high-performance lookups.
    /// </summary>
    private void InitializeMappings()
    {
        // Create a new, empty dictionary.
        _mappingLookup = new Dictionary<string, MessageMapping>();
        // Loop through every 'MessageMapping' you created in the Inspector.
        foreach (var mapping in mappings)
        {
            // Make sure the 'messageId' isn't empty and we haven't already added it.
            if (!string.IsNullOrEmpty(mapping.messageId) && !_mappingLookup.ContainsKey(mapping.messageId))
            {
                // Add this mapping to the dictionary, using the 'messageId' as the key.
                _mappingLookup[mapping.messageId] = mapping;
            }
        }
    }

    /// <summary>
    /// Sets up and initiates the connection to the Socket.IO server (ProtoPie Connect).
    /// </summary>
    private void ConnectToServer()
    {
        // 'try-catch' is used for error handling. If anything inside the 'try'
        // block fails, the 'catch' block will run and log an error.
        try
        {
            // Create the socket client object with the server URL and options.
            var uri = new Uri(serverURL);
            socket = new SocketIOUnity(uri, new SocketIOOptions { Transport = SocketIOClient.Transport.TransportProtocol.WebSocket });
            // Tell the socket to use the Newtonsoft JSON parser.
            socket.JsonSerializer = new NewtonsoftJsonSerializer();

            // --- EVENT SUBSCRIPTIONS ---
            // Tell the socket what to do when events happen.

            // On "Connected": Log a success message to the Unity Console.
            socket.OnConnected += (sender, e) => Debug.Log("🔌 ProtoPie Connector: Connected!");

            // On "ppMessage": This is the main event from ProtoPie.
            // 'OnUnityThread' ensures the code runs on Unity's main thread (which is required for UnityEvents).
            socket.OnUnityThread("ppMessage", (response) => ProcessIncomingData(response.GetValue<JToken>()));

            // Start the connection attempt.
            socket.Connect();
        }
        catch (Exception ex)
        {
            // If the setup failed (e.g., bad URL), log the error.
            Debug.LogError($"[ProtoPie] Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Receives the raw JSON data (JToken) from the socket event.
    /// ProtoPie can sometimes send a single message or an array of messages.
    /// This function handles both cases.
    /// </summary>
    private void ProcessIncomingData(JToken token)
    {
        if (token == null) return; // Do nothing if the data is empty.

        try
        {
            // If the data is an array (starts with '['), loop through each item.
            if (token.Type == JTokenType.Array)
                foreach (var item in token.Children()) ProcessMessageToken(item);
            // If the data is a single object (starts with '{'), process it directly.
            else if (token.Type == JTokenType.Object)
                ProcessMessageToken(token);
        }
        catch (Exception ex)
        {
            // Log an error if the JSON is malformed.
            Debug.LogError($"[ProtoPie] Parse Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Deserializes a single message token and executes its mapped actions.
    /// </summary>
    private void ProcessMessageToken(JToken token)
    {
        // Convert the raw JSON token into our clean 'ProtoPieMessage' class.
        ProtoPieMessage msg = token.ToObject<ProtoPieMessage>();

        // If the message is invalid or has no ID, ignore it.
        if (msg == null || string.IsNullOrEmpty(msg.MessageId)) return;

        // --- THE CORE LOGIC ---
        // Try to find a mapping in our dictionary using the 'messageId' as the key.
        if (_mappingLookup.TryGetValue(msg.MessageId, out MessageMapping mapping))
        {
            // SUCCESS: A mapping was found.
            Debug.Log($"[ProtoPie] Executing mapping for '{msg.MessageId}' with value '{msg.Value}'...");

            // Trigger the 'onReceive' event (for functions with NO parameters).
            // The '?' is a null-check; it only invokes if you assigned a function in the Inspector.
            mapping.onReceive?.Invoke();

            // Trigger the 'onReceiveWithValue' event, passing in the 'Value' from the message.
            mapping.onReceiveWithValue?.Invoke(msg.Value);
        }
        // else: No mapping was found for this 'messageId', so nothing happens.
    }
}