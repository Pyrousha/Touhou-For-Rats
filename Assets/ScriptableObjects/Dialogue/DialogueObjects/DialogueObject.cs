using System;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/DialogueObject")]
public class DialogueObject : ScriptableObject
{
    //  public CharacterObject charObjTest;

    [field: SerializeField, PreviewSprite] public Dialogue[] DialogueLines { get; private set; }
    [field: SerializeField] public DialogueObject NextDialogueObject { get; private set; }
    [field: SerializeField] public Response[] Responses { get; private set; }

    public bool HasResponses => ((Responses != null) && (Responses.Length > 0));

#if UNITY_EDITOR
    public void OnValidate()
    {
        if ((NextDialogueObject != null) && (Responses != null && Responses.Length > 0))
            Debug.LogError("ERROR ON \"" + name + ".asset\" : NextDialogueObject set while also using responses.\nPlease set the Responses length to 0, or remove the NextDialogueObject\n");
    }
#endif
}

[Serializable]
public struct Dialogue
{
    [SerializeField, PreviewSprite] public CharacterObject Speaker;
    [SerializeField][TextArea] public string Text;
    [SerializeField] public DialogueOverride OptionalOverrides;

    [Serializable]
    public struct DialogueOverride
    {
        [SerializeField, PreviewSprite] public Sprite PortraitImageOverride;
        [SerializeField] public AudioClip VoiceClipOverride;
    }
}


// // DialogueDrawer
// [CustomPropertyDrawer(typeof(Dialogue))]
// public class DialogueDrawer : PropertyDrawer
// {

//     const float imageHeight = 100;

//     public override float GetPropertyHeight(SerializedProperty property,
//                                             GUIContent label)
//     {
//         if (property.propertyType == SerializedPropertyType.ObjectReference &&
//             (property.objectReferenceValue as Sprite) != null)
//         {
//             return EditorGUI.GetPropertyHeight(property, label, true) + imageHeight + 10;
//         }
//         return EditorGUI.GetPropertyHeight(property, label, true);
//     }

//     static string GetPath(SerializedProperty property)
//     {
//         string path = property.propertyPath;
//         int index = path.LastIndexOf(".");
//         return path.Substring(0, index + 1);
//     }

//     public override void OnGUI(Rect position,
//                                 SerializedProperty property,
//                                 GUIContent label)
//     {
//         //Draw the normal property field
//         EditorGUI.PropertyField(position, property, label, true);

//         if (property.propertyType == SerializedPropertyType.ObjectReference)
//         {
//             var sprite = property.objectReferenceValue as Sprite;
//             if (sprite != null)
//             {
//                 position.y += EditorGUI.GetPropertyHeight(property, label, true) + 5;
//                 position.height = imageHeight;
//                 //EditorGUI.DrawPreviewTexture(position, sprite.texture, null, ScaleMode.ScaleToFit, 0);
//                 GUI.DrawTexture(position, sprite.texture, ScaleMode.ScaleToFit);
//             }
//         }
//     }


















// private static GUIStyle s_TempStyle = new GUIStyle();
// private SerializedProperty sprite;

// // Draw the property inside the given rect
// public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
// {
//     sprite = property.FindPropertyRelative("test");

//     EditorGUILayout.BeginHorizontal();
//     EditorGUILayout.PrefixLabel("Source Image");
//     EditorGUILayout.ObjectField(sprite as Sprite, typeof(Sprite), allowSceneObjects: true);
//     EditorGUILayout.EndHorizontal();
//     return;


//     // Using BeginProperty / EndProperty on the parent property means that
//     // prefab override logic works on the entire property.
//     EditorGUI.BeginProperty(position, label, property);

//     // Draw label
//     position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

//     // Don't make child fields be indented
//     var indent = EditorGUI.indentLevel;
//     EditorGUI.indentLevel = 0;







//     // Calculate rects
//     var imageRect = new Rect(25, position.y, 64, 64);
//     var unitRect = new Rect(position.x + 35, position.y, 50, position.height);
//     var nameRect = new Rect(position.x + 90, position.y, position.width - 90, position.height);

//     // EditorGUI.ObjectField(imageRect, )
//     // EditorGUI.LabelField(amountRect, "test");

//     // // Draw fields - pass GUIContent.none to each so they are drawn without labels
//     // EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("amount"), GUIContent.none);
//     // EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("unit"), GUIContent.none);
//     // EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"), GUIContent.none);

//     // Set indent back to what it was
//     EditorGUI.indentLevel = indent;

//     EditorGUI.EndProperty();

// }
// }