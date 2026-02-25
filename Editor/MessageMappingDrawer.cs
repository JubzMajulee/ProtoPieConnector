using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using System.Reflection;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(MessageMapping))]
public class MessageMappingDrawer : PropertyDrawer
{
    // Caching heights for layout calculation
    private float lineHeight = EditorGUIUtility.singleLineHeight;
    private float spacing = EditorGUIUtility.standardVerticalSpacing;
    private float sectionSpacing = 12f;
    private float headerSpacing = 8f;
    private float mappingSpacing = 16f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw an outline/box for the array element
        // We subtract mappingSpacing from the height so there's a visual gap before the next element
        Rect boxRect = new Rect(position.x, position.y + (mappingSpacing / 2f), position.width, GetPropertyHeight(property, label) - mappingSpacing);
        GUI.Box(boxRect, GUIContent.none, EditorStyles.helpBox);

        // Calculate initial rect inside the box
        Rect drawRect = new Rect(position.x + 5, position.y + (mappingSpacing / 2f) + 5, position.width - 10, lineHeight);

        // References to all base properties
        SerializedProperty mappingLabel = property.FindPropertyRelative("mappingLabel");
        SerializedProperty messageId = property.FindPropertyRelative("messageId");
        SerializedProperty direction = property.FindPropertyRelative("direction");

        // References to Receive properties
        SerializedProperty receiveMode = property.FindPropertyRelative("receiveMode");
        SerializedProperty onReceive = property.FindPropertyRelative("onReceive");
        SerializedProperty onReceiveWithValue = property.FindPropertyRelative("onReceiveWithValue");

        // References to Send properties
        SerializedProperty targetObject = property.FindPropertyRelative("targetObject");
        SerializedProperty eventToListenTo = property.FindPropertyRelative("eventToListenTo");
        SerializedProperty sendMode = property.FindPropertyRelative("sendMode");
        SerializedProperty payloadType = property.FindPropertyRelative("payloadType");
        SerializedProperty staticPayloadValue = property.FindPropertyRelative("staticPayloadValue");
        SerializedProperty variableSourceObject = property.FindPropertyRelative("variableSourceObject");
        SerializedProperty variableName = property.FindPropertyRelative("variableName");

        // Header: Strong Label at the top
        EditorGUI.LabelField(drawRect, mappingLabel.stringValue, EditorStyles.boldLabel);
        drawRect.y += lineHeight + spacing;

        // Draw generic properties
        EditorGUI.PropertyField(drawRect, mappingLabel);
        drawRect.y += lineHeight + spacing;

        EditorGUI.PropertyField(drawRect, messageId);
        drawRect.y += lineHeight + spacing;

        EditorGUI.PropertyField(drawRect, direction);
        drawRect.y += lineHeight + spacing;

        MessageDirection currentDir = (MessageDirection)direction.enumValueIndex;

        // --- DRAW RECEIVE FIELDS ---
        if (currentDir == MessageDirection.Receive)
        {
            drawRect.y += sectionSpacing;

            EditorGUI.LabelField(drawRect, "Action To Trigger (ProtoPie To Unity)", EditorStyles.boldLabel);
            drawRect.y += lineHeight + headerSpacing;

            // Receive Mode Configuration
            EditorGUI.PropertyField(drawRect, receiveMode);
            drawRect.y += lineHeight + headerSpacing; // Re-use headerSpacing for 8px padding

            ReceiveMode currentReceiveMode = (ReceiveMode)receiveMode.enumValueIndex;

            if (currentReceiveMode == ReceiveMode.TriggerOnly)
            {
                float onReceiveHeight = EditorGUI.GetPropertyHeight(onReceive);
                Rect onReceiveRect = new Rect(drawRect.x, drawRect.y, drawRect.width, onReceiveHeight);
                EditorGUI.PropertyField(onReceiveRect, onReceive);
                drawRect.y += onReceiveHeight + spacing;
            }
            else if (currentReceiveMode == ReceiveMode.WithValue)
            {
                float onReceiveWithValueHeight = EditorGUI.GetPropertyHeight(onReceiveWithValue);
                Rect onReceiveWithValueRect = new Rect(drawRect.x, drawRect.y, drawRect.width, onReceiveWithValueHeight);
                EditorGUI.PropertyField(onReceiveWithValueRect, onReceiveWithValue);
                drawRect.y += onReceiveWithValueHeight + spacing;
            }
        }

        // --- DRAW SEND FIELDS ---
        if (currentDir == MessageDirection.Send)
        {
            drawRect.y += sectionSpacing;

            EditorGUI.LabelField(drawRect, "Action To Listen To (Unity To ProtoPie)", EditorStyles.boldLabel);
            drawRect.y += lineHeight + headerSpacing;

            EditorGUI.PropertyField(drawRect, targetObject);
            drawRect.y += lineHeight + spacing;

            // Target Object Event Dropdown Logic
            if (targetObject.objectReferenceValue != null)
            {
                DrawEventDropdown(drawRect, targetObject, eventToListenTo);
                drawRect.y += lineHeight + spacing;
            }
            else
            {
                EditorGUI.HelpBox(new Rect(drawRect.x, drawRect.y, drawRect.width, lineHeight * 2), "Assign a Target Object to pick an event.", MessageType.Info);
                drawRect.y += (lineHeight * 2) + spacing;
            }

            // Send Mode Configuration
            EditorGUI.PropertyField(drawRect, sendMode);
            drawRect.y += lineHeight + spacing;

            SendMode currentMode = (SendMode)sendMode.enumValueIndex;

            if (currentMode == SendMode.WithValue)
            {
                EditorGUI.PropertyField(drawRect, payloadType);
                drawRect.y += lineHeight + spacing;

                PayloadType currentType = (PayloadType)payloadType.enumValueIndex;

                if (currentType == PayloadType.StaticString)
                {
                    EditorGUI.PropertyField(drawRect, staticPayloadValue, new GUIContent("Payload Value"));
                    drawRect.y += lineHeight + spacing;
                }
                else if (currentType == PayloadType.DynamicVariable)
                {
                    EditorGUI.PropertyField(drawRect, variableSourceObject, new GUIContent("Variable Source Obj"));
                    drawRect.y += lineHeight + spacing;

                    // Variable Data Source Dropdown Logic
                    if (variableSourceObject.objectReferenceValue != null)
                    {
                        DrawVariableDropdown(drawRect, variableSourceObject, variableName);
                        drawRect.y += lineHeight + spacing;
                    }
                    else
                    {
                        EditorGUI.HelpBox(new Rect(drawRect.x, drawRect.y, drawRect.width, lineHeight * 2), "Assign a Source Object to pick a variable.", MessageType.Info);
                        drawRect.y += (lineHeight * 2) + spacing;
                    }
                }
            }
        }

        EditorGUI.EndProperty();
    }

    /// <summary>
    /// Computes the exact height needed to render the CustomDrawer, ensuring proper layout inside lists/arrays.
    /// </summary>
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float totalHeight = 10; // Internal padding
        totalHeight += mappingSpacing; // External spacing between mappings
        
        // Header, mappingLabel, messageId, direction
        totalHeight += (lineHeight + spacing) * 4;

        SerializedProperty direction = property.FindPropertyRelative("direction");
        MessageDirection currentDir = (MessageDirection)direction.enumValueIndex;

        if (currentDir == MessageDirection.Receive)
        {
            totalHeight += sectionSpacing;
            totalHeight += lineHeight + headerSpacing; // Header Label
            totalHeight += lineHeight + headerSpacing; // Receive Mode dropdown (with 8px padding)
            
            SerializedProperty receiveMode = property.FindPropertyRelative("receiveMode");
            if ((ReceiveMode)receiveMode.enumValueIndex == ReceiveMode.TriggerOnly)
            {
                totalHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("onReceive")) + spacing;
            }
            else
            {
                totalHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("onReceiveWithValue")) + spacing;
            }
        }

        if (currentDir == MessageDirection.Send)
        {
            totalHeight += sectionSpacing;

            totalHeight += lineHeight + headerSpacing; // Header Label
            totalHeight += lineHeight + spacing; // Target object field
            
            SerializedProperty targetObject = property.FindPropertyRelative("targetObject");
            if (targetObject.objectReferenceValue != null) totalHeight += lineHeight + spacing; // Dropdown
            else totalHeight += (lineHeight * 2) + spacing; // Helpbox

            totalHeight += lineHeight + spacing; // SendMode
            
            SerializedProperty sendMode = property.FindPropertyRelative("sendMode");
            if ((SendMode)sendMode.enumValueIndex == SendMode.WithValue)
            {
                totalHeight += lineHeight + spacing; // PayloadType
                
                SerializedProperty payloadType = property.FindPropertyRelative("payloadType");
                if ((PayloadType)payloadType.enumValueIndex == PayloadType.StaticString)
                {
                    totalHeight += lineHeight + spacing; // staticPayloadValue
                }
                else
                {
                    totalHeight += lineHeight + spacing; // variableSourceObject
                    SerializedProperty variableSourceObject = property.FindPropertyRelative("variableSourceObject");
                    if (variableSourceObject.objectReferenceValue != null) totalHeight += lineHeight + spacing; // Dropdown
                    else totalHeight += (lineHeight * 2) + spacing; // Helpbox
                }
            }
        }

        return totalHeight;
    }

    /// <summary>
    /// Uses reflection to list all UnityEvents on the selected target GameObject.
    /// </summary>
    private void DrawEventDropdown(Rect position, SerializedProperty targetObjProp, SerializedProperty eventNameProp)
    {
        GameObject obj = targetObjProp.objectReferenceValue as GameObject;
        if (obj == null) return;

        List<string> options = new List<string> { "None" };
        List<string> displayOptions = new List<string> { "None" };

        MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>();
        foreach (var comp in components)
        {
            if (comp == null) continue;
            string compName = comp.GetType().Name;

            var fields = comp.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                // We are looking for UnityEvents specifically to trigger our sends
                if (typeof(UnityEventBase).IsAssignableFrom(field.FieldType)) 
                {
                    options.Add($"{compName}.{field.Name}");
                    
                    // Format display name natively so it's designer-friendly
                    displayOptions.Add(field.Name);
                }
            }
        }

        int currentIndex = Mathf.Max(0, options.IndexOf(eventNameProp.stringValue));
        
        int newIndex = EditorGUI.Popup(position, "Event To Listen To", currentIndex, displayOptions.ToArray());
        if (newIndex > 0)
        {
            eventNameProp.stringValue = options[newIndex];
        }
        else
        {
            eventNameProp.stringValue = "";
        }
    }

    /// <summary>
    /// Uses reflection to list all fields and properties on the selected source GameObject.
    /// </summary>
    private void DrawVariableDropdown(Rect position, SerializedProperty sourceObjProp, SerializedProperty varNameProp)
    {
        GameObject obj = sourceObjProp.objectReferenceValue as GameObject;
        if (obj == null) return;

        List<string> options = new List<string> { "None" };
        List<string> displayOptions = new List<string> { "None" };

        MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>();
        foreach (var comp in components)
        {
            if (comp == null) continue;
            string compName = comp.GetType().Name;

            // Search Fields
            var fields = comp.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                // Exclude delegates and UnityEvents from variable dropdowns
                if (!typeof(System.Delegate).IsAssignableFrom(field.FieldType) && !typeof(UnityEventBase).IsAssignableFrom(field.FieldType))
                {
                    options.Add($"{compName}.{field.Name}");
                    displayOptions.Add(field.Name);
                }
            }

            // Search Properties
            var props = comp.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var prop in props)
            {
                if (prop.CanRead && !typeof(System.Delegate).IsAssignableFrom(prop.PropertyType) && !typeof(UnityEventBase).IsAssignableFrom(prop.PropertyType))
                {
                    options.Add($"{compName}.{prop.Name}");
                    displayOptions.Add(prop.Name);
                }
            }
        }

        int currentIndex = Mathf.Max(0, options.IndexOf(varNameProp.stringValue));
        
        int newIndex = EditorGUI.Popup(position, "Variable Data Source", currentIndex, displayOptions.ToArray());
        if (newIndex > 0)
        {
            varNameProp.stringValue = options[newIndex];
        }
        else
        {
            varNameProp.stringValue = "";
        }
    }
}
