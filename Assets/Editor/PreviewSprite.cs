using UnityEngine;
namespace UnityEditor
{
    [CustomPropertyDrawer(typeof(PreviewSpriteAttribute))]
    public class PreviewSpriteDrawer : PropertyDrawer
    {
        const float imageHeight = 32;

        public override float GetPropertyHeight(SerializedProperty property,
                                                GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference &&
                (property.objectReferenceValue as Sprite) != null)
            {
                return EditorGUI.GetPropertyHeight(property, label, true) + imageHeight + 10;
            }
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        static string GetPath(SerializedProperty property)
        {
            string path = property.propertyPath;
            int index = path.LastIndexOf(".");
            return path.Substring(0, index + 1);
        }

        public override void OnGUI(Rect position,
                                    SerializedProperty property,
                                    GUIContent label)
        {
            //Draw the normal property field
            EditorGUI.PropertyField(position, property, label, true);

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                Sprite sprite;

                var charObj = property.objectReferenceValue as CharacterObject;
                if (charObj != null)
                    sprite = charObj.DefaultPortraitSprite;
                else
                    sprite = property.objectReferenceValue as Sprite;

                if (sprite != null)
                // {
                //     position.y += EditorGUI.GetPropertyHeight(property, label, true) + 5;
                //     position.height = imageHeight;
                //     //EditorGUI.DrawPreviewTexture(position, sprite.texture, null, ScaleMode.ScaleToFit, 0);
                //     GUI.DrawTexture(position, sprite.texture, ScaleMode.ScaleToFit);
                // }
                {
                    // position.y += EditorGUI.GetPropertyHeight(property, label, true) + 5;
                    position.y -= 8;
                    position.height = 32;
                    position.x = -100;
                    //EditorGUI.DrawPreviewTexture(position, sprite.texture, null, ScaleMode.ScaleToFit, 0);
                    GUI.DrawTexture(position, sprite.texture, ScaleMode.ScaleToFit);
                }
            }
        }
    }
}
