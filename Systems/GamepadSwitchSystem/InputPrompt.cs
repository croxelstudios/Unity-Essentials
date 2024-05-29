using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Croxel Scriptables/InputPrompt")]
public class InputPrompt : ScriptableObject
{
    public InputPromptTexture keyboardTexture = new InputPromptTexture(0, 0.1f);
    public InputPromptTexture[] buttonTextures = new InputPromptTexture[] { new InputPromptTexture(0, 0.1f) };
    public int gamepadDefault = 0;

    [Serializable]
    public struct InputPromptTexture
    {
        public Texture2D texture;
        public Sprite[] sprites;
        public int startFrame;
        [Min(0)]
        public float frameTime;

        public InputPromptTexture(int startFrame, float frameTime, Texture2D texture = null, Sprite[] sprites = null)
        {
            this.texture = texture;
            this.sprites = sprites;
            this.startFrame = startFrame;
            this.frameTime = frameTime;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InputPromptTexture))]
    class InputPromptTextureDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
            {
                SerializedProperty prop = property;
                SerializedProperty endProp = property.GetEndProperty();
                if (prop.NextVisible(true) && prop.NextVisible(true))
                {
                    do
                    {
                        float height = EditorGUI.GetPropertyHeight(prop, null, true)
                            + EditorGUIUtility.standardVerticalSpacing;
                        totalHeight += height;
                    }
                    while (prop.NextVisible(false) && (!SerializedProperty.EqualContents(prop, endProp)));
                }
            }
            return totalHeight;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUIContent guiContent = new GUIContent(property.displayName);
            Rect foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, guiContent, true);

            Rect indentedPosition = EditorGUI.IndentedRect(position);
            float indentOffset = indentedPosition.x - position.x;
            Rect propertyRect = new Rect(position.x + (EditorGUIUtility.labelWidth - indentOffset),
                position.y, position.width - (EditorGUIUtility.labelWidth - indentOffset),
                EditorGUIUtility.singleLineHeight);

            SerializedProperty texProp = property.FindPropertyRelative("texture");
            EditorGUI.BeginChangeCheck();
            EditorGUI.ObjectField(propertyRect, texProp, typeof(Texture2D), GUIContent.none);
            if (EditorGUI.EndChangeCheck())
                UpdateSpritesProperty(texProp, property.FindPropertyRelative("sprites"));

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                SerializedProperty prop = property;
                SerializedProperty endProp = property.GetEndProperty();
                float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                if (prop.NextVisible(true) && prop.NextVisible(true))
                {
                    do
                    {
                        float height = EditorGUI.GetPropertyHeight(prop, new GUIContent(prop.displayName), true);
                        EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), prop, true);
                        y += height + EditorGUIUtility.standardVerticalSpacing;
                    }
                    while (prop.NextVisible(false) && (!SerializedProperty.EqualContents(prop, endProp)));
                }
                EditorGUI.indentLevel--;
            }
        }

        void UpdateSpritesProperty(SerializedProperty texProp, SerializedProperty sprProp)
        {
            Sprite[] spritesArray = ((Texture2D)texProp.objectReferenceValue).GetSpritesArray();
            if (spritesArray != null)
            {
                sprProp.ClearArray();
                for (int i = 0; i < spritesArray.Length; i++)
                {
                    sprProp.InsertArrayElementAtIndex(i);
                    SerializedProperty indexProp = sprProp.GetArrayElementAtIndex(i);
                    indexProp.objectReferenceValue = spritesArray[i];
                }
            }
        }
    }
#endif
}
