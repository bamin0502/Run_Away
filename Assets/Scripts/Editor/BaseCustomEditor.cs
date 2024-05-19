using UnityEditor;
using UnityEngine;

public class BaseCustomEditor : Editor
{
    protected GUIStyle guiStyle;
    protected Font customFont;

    protected virtual void OnEnable()
    {
        customFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/NeoDunggeunmoPro-Regular (2).ttf");

        guiStyle = new GUIStyle();
        if (customFont != null)
        {
            guiStyle.font = customFont;
        }
        else
        {
            Debug.LogWarning("Failed to load custom font.");
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var property = serializedObject.GetIterator();
        if (property.NextVisible(true))
        {
            do
            {
                EditorGUILayout.BeginHorizontal();
                if (guiStyle != null && customFont != null)
                {
                    EditorGUILayout.LabelField(new GUIContent(property.displayName), guiStyle, GUILayout.MaxWidth(150));
                }
                else
                {
                    EditorGUILayout.LabelField(new GUIContent(property.displayName), GUILayout.MaxWidth(150));
                }

                if (property.propertyType == SerializedPropertyType.String)
                {
                    // 문자열 속성을 Text Area로 표시
                    property.stringValue = EditorGUILayout.TextArea(property.stringValue, GUILayout.Height(60));
                }
                else
                {
                    EditorGUILayout.PropertyField(property, GUIContent.none, true);
                }
                EditorGUILayout.EndHorizontal();

                // 간격을 줄이기 위해 다음 줄 추가
                EditorGUILayout.Space(2);
            }
            while (property.NextVisible(false));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
