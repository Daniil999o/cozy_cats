using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;

public class YG_CameraAnimatorRecorder : EditorWindow
{
    private bool _useSmoothPosition = true;
    private float _smoothPositionSpeed = 5f;
    
    private bool _useSmoothRotation = true;
    private float _smoothRotationSpeed = 6f;
    
    private GameObject _recorderObject;
    private Transform _recorderTransform;
    private Animator _recorderAnimator;

    private float _recordDelay = 0.1f;

    private AnimationClip _clip;
    private bool isRecording;

    private float _currentTime;

    private float _delay;
    
    private const string CameraRecorderName = "Camera";

    [MenuItem("YG Plugin Provider/Animator/Camera Recorder")]
    public static void ShowWindow() {
        GetWindow<YG_CameraAnimatorRecorder>("Camera Recorder");
    }

    private void OnEnable() {
        InitializeRecorder();
    }

    private void InitializeRecorder() {
        _recorderObject = Selection.activeGameObject;

        if (!_recorderObject) CameraRecorderName.TryFindGameObject(out _recorderObject);

        if (!_recorderObject) {
            Transform parent = new GameObject("= CAMERA RECORD =").transform;
            
            _recorderObject = new GameObject($"{CameraRecorderName}");
            _recorderObject.transform.SetParent(parent);

            _recorderObject.transform.position = Vector3.zero;
            _recorderAnimator = _recorderObject.AddComponent<Animator>();

            _recorderObject.AddComponent<Camera>();
        } else {
            if (!_recorderObject.TryGetComponent(out _recorderAnimator))
                _recorderAnimator = _recorderObject.AddComponent<Animator>();
        }

        _recorderTransform = _recorderObject.transform;
        Selection.activeGameObject = _recorderObject;
    }

    private void OnGUI() {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField("Camera Recorder", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (isRecording) {
            if (GUILayout.Button("Stop Recording", GUILayout.Height(30))) StopRecording();
        } else {
            if (GUILayout.Button("Start Recording", GUILayout.Height(30))) StartRecording();
        }
        
        GUILayout.Space(30);
        
        _recordDelay = EditorGUILayout.FloatField("Record Delay", _recordDelay);
        
        GUILayout.Space(30);
        
        _useSmoothPosition = EditorGUILayout.Toggle("Smooth Position", _useSmoothPosition);
        if (_useSmoothPosition) {
            _smoothPositionSpeed = EditorGUILayout.FloatField("Position Smooth Speed", _smoothPositionSpeed);
        
            GUILayout.Space(10);
        }
        
        GUILayout.Space(5);

        _useSmoothRotation = EditorGUILayout.Toggle("Smooth Rotation", _useSmoothRotation);
        if (_useSmoothRotation)
            _smoothRotationSpeed = EditorGUILayout.FloatField("Rotation Smooth Speed", _smoothRotationSpeed);

        EditorGUILayout.EndVertical();
    }


    private void StartRecording() {
        InitializeRecorder();
        
        string folderPath = $"Assets/Animations/Scene Camera Record/{_recorderObject.name}";

        _delay = 0f;
        _currentTime = 0f;

        Directory.CreateDirectory(folderPath);

        _clip = new AnimationClip();
        AssetDatabase.CreateAsset(_clip, Path.Combine(folderPath, $"{_recorderObject.name}_Record.anim"));

        AnimatorController controller =
            AnimatorController.CreateAnimatorControllerAtPath(Path.Combine(folderPath,
                    $"{_recorderObject.name}.controller"
                )
            );

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        AnimatorState state = stateMachine.AddState("Record");
        state.motion = _clip;

        Transform sceneViewCameraTransform = SceneView.lastActiveSceneView.camera.transform;

        _recorderTransform.position = sceneViewCameraTransform.position;
        _recorderTransform.rotation = sceneViewCameraTransform.rotation;

        _recorderAnimator.runtimeAnimatorController = controller;

        isRecording = true;

        AnimationMode.StartAnimationMode();
    }

    private void StopRecording() {
        isRecording = false;
        _currentTime = 0f;
        _delay = 0f;

        AnimationMode.StopAnimationMode();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void Update() {
        if (isRecording && SceneView.lastActiveSceneView && _recorderTransform) {
            if (_delay <= 0f)
                RecordPart();
            else
                _delay -= Time.deltaTime;

            _currentTime += Time.deltaTime;
        }
    }

    private void RecordPart() {
        _delay = _recordDelay;
        
        Transform sceneViewCameraTransform = SceneView.lastActiveSceneView.camera.transform;

        Vector3 viewPos = sceneViewCameraTransform.position;
        Quaternion viewRot = sceneViewCameraTransform.rotation;
            
        _recorderTransform.position = _useSmoothPosition
            ? Vector3.Lerp(_recorderTransform.position, viewPos, _smoothPositionSpeed * 0.1f * Time.deltaTime)
            : viewPos;
            
        _recorderTransform.rotation =  _useSmoothRotation
            ? Quaternion.Lerp(_recorderTransform.rotation, viewRot, _smoothRotationSpeed * 0.1f * Time.deltaTime)
            : viewRot;

        SetKeyframe(_recorderTransform, "localPosition.x", _currentTime);
        SetKeyframe(_recorderTransform, "localPosition.y", _currentTime);
        SetKeyframe(_recorderTransform, "localPosition.z", _currentTime);
        SetKeyframe(_recorderTransform, "localRotation.x", _currentTime);
        SetKeyframe(_recorderTransform, "localRotation.y", _currentTime);
        SetKeyframe(_recorderTransform, "localRotation.z", _currentTime);
        SetKeyframe(_recorderTransform, "localRotation.w", _currentTime);
    }

    private void OnDisable() {
        if (isRecording) StopRecording();
    }

    private void SetKeyframe(Transform target, string propertyPath, float time) {
        AnimationCurve curve = AnimationUtility.GetEditorCurve(_clip,
            EditorCurveBinding.FloatCurve("", typeof(Transform), propertyPath)
        );

        if (curve == null) curve = new AnimationCurve();

        Keyframe key = new Keyframe(time, target.localPosition.x);

        switch (propertyPath) {
            case "localPosition.x":
                key.value = target.localPosition.x;
                break;
            case "localPosition.y":
                key.value = target.localPosition.y;
                break;
            case "localPosition.z":
                key.value = target.localPosition.z;
                break;

            case "localRotation.x":
                key.value = target.localRotation.x;
                break;
            case "localRotation.y":
                key.value = target.localRotation.y;
                break;
            case "localRotation.z":
                key.value = target.localRotation.z;
                break;
            case "localRotation.w":
                key.value = target.localRotation.w;
                break;
        }

        curve.AddKey(key);

        AnimationUtility.SetEditorCurve(_clip, EditorCurveBinding.FloatCurve("", typeof(Transform), propertyPath), curve
        );
    }
}