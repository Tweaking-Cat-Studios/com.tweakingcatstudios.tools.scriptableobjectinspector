#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TCSTools.Scripts
{
    public class ScriptableObjectInspectorWindow : EditorWindow
    {
        private GameObject currentSelection;
        private List<SOFieldInfo> scriptableObjects = new();
        private int selectedIndex = 0;
        private bool isLocked = false;
        private Vector2 scrollPos; // Add this field

        [MenuItem("TCS/Scriptable Object Inspector")]
        public static void ShowWindow()
        {
            GetWindow<ScriptableObjectInspectorWindow>("SO Inspector");
        }

        private void OnSelectionChange()
        {
            if (isLocked) return;

            currentSelection = Selection.activeGameObject;
            RebuildSOList();
            selectedIndex = 0;
            Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scriptable Object Inspector", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            isLocked = GUILayout.Toggle(isLocked, "🔒 Lock", "Button", GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (currentSelection == null)
            {
                EditorGUILayout.LabelField("Select a GameObject in the scene.");
                return;
            }

            if (scriptableObjects.Count == 0)
            {
                EditorGUILayout.LabelField("No ScriptableObject fields found.");
                return;
            }

            string[] displayNames = scriptableObjects.ConvertAll(so =>
                $"{so.ComponentName}.{so.FieldName}").ToArray();

            selectedIndex = EditorGUILayout.Popup("Select ScriptableObject", selectedIndex, displayNames);
            var selectedSO = scriptableObjects[selectedIndex].Value;

            // Begin scroll view here
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            if (selectedSO != null)
            {
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Inspector", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("📌 Find In Project", GUILayout.Width(120)))
                {
                    EditorGUIUtility.PingObject(selectedSO);
                }
                EditorGUILayout.EndHorizontal();

                Editor editor = Editor.CreateEditor(selectedSO);
                if (editor != null)
                {
                    editor.OnInspectorGUI();
                }
            }
            else
            {
                EditorGUILayout.LabelField("Selected field is null.");
            }

            EditorGUILayout.EndScrollView(); // End scroll view

        }

        private void RebuildSOList()
        {
            scriptableObjects.Clear();

            if (currentSelection == null) return;

            var components = currentSelection.GetComponents<MonoBehaviour>();
            foreach (var comp in components)
            {
                if (comp == null) continue;

                var fields = comp.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    if (typeof(ScriptableObject).IsAssignableFrom(field.FieldType))
                    {
                        var value = field.GetValue(comp) as ScriptableObject;
                        if (value != null)
                        {
                            scriptableObjects.Add(new SOFieldInfo
                            {
                                ComponentName = comp.GetType().Name,
                                FieldName = field.Name,
                                Value = value
                            });
                        }
                    }
                }
            }
        }

        private class SOFieldInfo
        {
            public string ComponentName;
            public string FieldName;
            public ScriptableObject Value;
        }
    }
}
#endif
