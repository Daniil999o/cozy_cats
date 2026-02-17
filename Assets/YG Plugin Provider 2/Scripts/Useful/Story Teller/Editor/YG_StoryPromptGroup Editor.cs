using UnityEngine;
using UnityEditor;
using System.Collections;
using Unity.EditorCoroutines.Editor;

[CustomEditor(typeof(YG_StoryPromptGroup))]
[CanEditMultipleObjects]
public class YG_StoryPromptGroupEditor : Editor
{
    private YG_StoryPromptGroup _promptGroup;
    private int _currentPromptIndex = -1;

    private EditorCoroutine _typingCoroutine;
    private EditorCoroutine _typingAllCoroutine;

    private bool _nextAllTyping;

    private void OnEnable() {
        _promptGroup = (YG_StoryPromptGroup)target;
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if (!_promptGroup.character) {
            EditorGUILayout.HelpBox("Assign a character to see the preview", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Dialog Preview", EditorStyles.boldLabel);

        if (GUILayout.Button(_typingAllCoroutine != null ? "Is Playing" : _nextAllTyping ? "Paused" : "Play All")) {
            if (_typingAllCoroutine != null || _nextAllTyping) {
                StopTypingCoroutine();
                YG_EditorAudioUtility.StopAllClips();
            } else {
                PlayAllPrompts();
            }
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(5);

        if (_promptGroup.prompts == null || _promptGroup.prompts.Length == 0) {
            EditorGUILayout.LabelField("There are no phrases to display", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        for (int i = 0; i < _promptGroup.prompts.Length; i++) {
            YG_StoryPromptGroupText prompt = _promptGroup.prompts[i];
            EditorGUILayout.BeginVertical(i == _currentPromptIndex ? EditorStyles.helpBox : GUI.skin.box);

            EditorGUILayout.BeginHorizontal();

            if (_promptGroup.character.avatar) {
                Rect avatarRect = GUILayoutUtility.GetRect(64, 64, GUILayout.Width(64), GUILayout.Height(64));
                DrawSpritePreview(avatarRect, _promptGroup.character.avatar);
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(_promptGroup.character.name, EditorStyles.boldLabel);

            string displayedText = prompt.displayedText;
            if (displayedText.IsEmptyOrWhiteSpaces()) displayedText = prompt.text;

            EditorGUILayout.LabelField(displayedText, EditorStyles.wordWrappedLabel);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            if (prompt.clip || prompt.fixedDuration > 0) {
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();

                bool playing = _typingCoroutine != null && i == _currentPromptIndex;
                string playButtonText = playing ? "❚❚" : "▶️";

                if (GUILayout.Button(playButtonText, GUILayout.Height(25))) {
                    if (playing) {
                        YG_EditorAudioUtility.PauseClip(_promptGroup.prompts[i].clip);
                        PauseTypingCoroutine(i);
                    } else
                        StartTypingCoroutine(i);
                }

                if (GUILayout.Button("■", GUILayout.Width(120), GUILayout.Height(25))) {
                    YG_EditorAudioUtility.StopAllClips();
                    StopTypingCoroutine();
                    _currentPromptIndex = -1;
                    Repaint();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
    }

    private void PlayAllPrompts(int index = 0, bool stopAll = true) {
        if (stopAll) {
            StopTypingCoroutine();
            YG_EditorAudioUtility.StopAllClips();
        }

        _currentPromptIndex = index;

        if (_typingAllCoroutine != null) EditorCoroutineUtility.StopCoroutine(_typingAllCoroutine);

        _typingAllCoroutine = EditorCoroutineUtility.StartCoroutine(PlayAllPromptsCoroutine(index), this);
    }

    private IEnumerator PlayAllPromptsCoroutine(int index = 0) {
        for (int i = index; i < _promptGroup.prompts.Length; i++) {
            StartTypingCoroutine(i, false);
            yield return new WaitUntil(() => _typingCoroutine == null);
            yield return new WaitForSecondsRealtime(_promptGroup.delayAfterPrinting);
        }

        _typingAllCoroutine = null;
        _typingCoroutine = null;
    }

    private void StartTypingCoroutine(int index, bool stopAll = true) {
        if (_nextAllTyping) {
            _nextAllTyping = false;

            PlayAllPrompts(index, false);
            return;
        }

        if (_currentPromptIndex != index) {
            YG_EditorAudioUtility.StopAllClips();
            StopTypingCoroutine(stopAll);
        }

        _currentPromptIndex = index;

        _typingCoroutine = EditorCoroutineUtility.StartCoroutine(IE_TypeText(index), this);
    }

    private void StopTypingCoroutine(bool stopAll = true) {
        if (_typingCoroutine != null) {
            EditorCoroutineUtility.StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }

        if (stopAll && _typingAllCoroutine != null) {
            EditorCoroutineUtility.StopCoroutine(_typingAllCoroutine);
            _typingAllCoroutine = null;
        }

        if (_promptGroup.prompts.InLength(_currentPromptIndex))
            _promptGroup.prompts[_currentPromptIndex].displayedText = "";

        _currentPromptIndex = -1;

        Repaint();
    }

    private void PauseTypingCoroutine(int index) {
        _currentPromptIndex = index;

        if (_typingCoroutine != null) {
            EditorCoroutineUtility.StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;

            if (_typingAllCoroutine != null) {
                EditorCoroutineUtility.StopCoroutine(_typingAllCoroutine);
                _typingAllCoroutine = null;

                _nextAllTyping = true;
            }
        }
    }

    private IEnumerator IE_TypeText(int index) {
        YG_StoryPromptGroupText prompt = _promptGroup.prompts[index];

        float delay = prompt.fixedDuration;

        if (prompt.clip) {
            delay = prompt.clip.length / prompt.text.Length;

            YG_EditorAudioUtility.PlayClip(prompt.clip);
        }

        int oldIndex = prompt.displayedText.Length;

        if (!prompt.text.InLength(oldIndex)) {
            prompt.displayedText = "";
            oldIndex = 0;
        }

        for (int i = oldIndex; i < prompt.text.Length; i++) {
            prompt.displayedText += prompt.text[i];
            Repaint();

            if (i < prompt.text.Length) yield return new WaitForSecondsRealtime(delay);
        }

        StopTypingCoroutine(false);
    }


    private void OnDisable() {
        YG_EditorAudioUtility.StopAllClips();
        StopTypingCoroutine();
    }

    private void DrawSpritePreview(Rect position, Sprite sprite) {
        if (!sprite) return;

        Vector2 fullSize = new Vector2(sprite.texture.width, sprite.texture.height);
        Vector2 size = new Vector2(sprite.textureRect.width, sprite.textureRect.height);

        Rect coords = sprite.textureRect;
        coords.x /= fullSize.x;
        coords.width /= fullSize.x;
        coords.y /= fullSize.y;
        coords.height /= fullSize.y;

        Vector2 ratio;
        ratio.x = position.width / size.x;
        ratio.y = position.height / size.y;
        float minRatio = Mathf.Min(ratio.x, ratio.y);

        Vector2 center = position.center;
        position.width = size.x * minRatio;
        position.height = size.y * minRatio;
        position.center = center;

        GUI.DrawTextureWithTexCoords(position, sprite.texture, coords);
    }
}