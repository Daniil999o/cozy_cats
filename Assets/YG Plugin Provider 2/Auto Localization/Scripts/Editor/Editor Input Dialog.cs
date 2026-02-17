#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AutoLocalization.Editor
{
    public class EditorInputDialog : EditorWindow
    {
        private string inputText = "";
        private System.Action<string> onOkCallback;
        private System.Action onCancelCallback;

        public static void Show(string title,
            string defaultValue,
            System.Action<string> onOk,
            System.Action onCancel = null) {
            EditorInputDialog window = GetWindow<EditorInputDialog>(true, title);
            window.inputText = defaultValue;
            window.onOkCallback = onOk;
            window.onCancelCallback = onCancel;
            window.minSize = new Vector2(300, 120);
            window.maxSize = new Vector2(300, 120);
            window.Show();
        }

        private void OnGUI() {
            Event evt = Event.current;

            if (evt.type == EventType.KeyDown) {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter) {
                    if (!string.IsNullOrEmpty(inputText)) {
                        onOkCallback?.Invoke(inputText);
                        Close();
                    }

                    evt.Use();
                    return;
                } else if (evt.keyCode == KeyCode.Escape) {
                    onCancelCallback?.Invoke();
                    Close();
                    evt.Use();
                    return;
                }
            }

            GUILayout.Space(10);

            GUI.SetNextControlName("GroupNameField");
            inputText = EditorGUILayout.TextField("Group Name:", inputText);

            if (Event.current.type == EventType.Layout) EditorGUI.FocusTextInControl("GroupNameField");

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Cancel", GUILayout.Width(80))) {
                onCancelCallback?.Invoke();
                Close();
            }

            bool disabled = string.IsNullOrEmpty(inputText);
            EditorGUI.BeginDisabledGroup(disabled);
            if (GUILayout.Button("OK", GUILayout.Width(80))) {
                onOkCallback?.Invoke(inputText);
                Close();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }
    }
}

#endif