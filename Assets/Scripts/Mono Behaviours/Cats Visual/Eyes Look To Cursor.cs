using UnityEngine;
using System;

public class EyesLookToCursor : MonoBehaviour
{
    [Serializable]
    public class EyeSetup
    {
        public SkinnedMeshRenderer renderer;           // Рендерер глаза
        public Transform eyePosition;                  // Точная позиция глаза (для конвергенции)
        public Transform reference;                     // Система координат глаза (обычно голова)
        
        [Header("Оси взгляда в локальном пространстве reference")]
        public Axis horizontalAxis = Axis.X;
        public bool invertHorizontal;
        public Axis verticalAxis = Axis.Y;
        public bool invertVertical;
        
        public bool flipHorizontal;
        public bool flipVertical;
        
        [Header("Блендшейпы (индексы или авто-поиск)")]
        public string leftShapeKeyword = "Left";
        public string rightShapeKeyword = "Right";
        public string upShapeKeyword = "Up";
        public string downShapeKeyword = "Down";
        
        // Ручные индексы (если авто-поиск не сработал)
        public int leftShapeIndex = -1;
        public int rightShapeIndex = -1;
        public int upShapeIndex = -1;
        public int downShapeIndex = -1;
        
        [Header("Диапазон весов")]
        public Vector2 blendRange = new Vector2(0, 100);
        
        [Header("Зеркалирование направлений (для правого глаза)")]
        public bool mirrorHorizontal;   // меняет Left<->Right
        public bool mirrorVertical;     // меняет Up<->Down
    }

    [SerializeField] private EyeSetup[] eyes;

    [Header("Общие настройки (если не заданы индивидуально)")]
    [SerializeField] private Transform globalReference;
    [SerializeField] private Vector2 globalBlendRange = new Vector2(0, 100);

    [Header("Конвергенция (использовать eyePosition)")]
    [SerializeField] private bool useConvergence = true;

    private void Start()
    {
        // Автоматический поиск индексов, если они не заданы вручную
        foreach (var eye in eyes)
        {
            if (eye.renderer == null) continue;
            Mesh mesh = eye.renderer.sharedMesh;
            if (mesh == null) continue;

            if (eye.leftShapeIndex < 0)
                eye.leftShapeIndex = FindBlendShape(mesh, eye.leftShapeKeyword);
            if (eye.rightShapeIndex < 0)
                eye.rightShapeIndex = FindBlendShape(mesh, eye.rightShapeKeyword);
            if (eye.upShapeIndex < 0)
                eye.upShapeIndex = FindBlendShape(mesh, eye.upShapeKeyword);
            if (eye.downShapeIndex < 0)
                eye.downShapeIndex = FindBlendShape(mesh, eye.downShapeKeyword);
        }
    }

    private int FindBlendShape(Mesh mesh, string keyword)
    {
        for (int i = 0; i < mesh.blendShapeCount; i++)
            if (mesh.GetBlendShapeName(i).Contains(keyword))
                return i;
        return -1;
    }

    private void Update()
    {
        Vector3 target = EyesHandler.LookPos; // присвойте сюда вашу цель (например, курсор)

        foreach (var eye in eyes)
        {
            if (eye.renderer == null) continue;

            // Определяем систему координат для этого глаза
            Transform space = eye.reference != null ? eye.reference : (globalReference != null ? globalReference : transform);

            // Позиция глаза (для конвергенции)
            Vector3 eyePos;
            if (useConvergence && eye.eyePosition != null)
                eyePos = eye.eyePosition.position;
            else
                eyePos = eye.renderer.bounds.center;

            Vector3 worldDir = (target - eyePos).normalized;
            if (worldDir == Vector3.zero) worldDir = space.forward;

            // Локальное направление в пространстве reference
            Vector3 localDir = space.InverseTransformDirection(worldDir).normalized;

            // Извлекаем оси
            float hor = GetAxisValue(localDir, eye.horizontalAxis) * (eye.invertHorizontal ? -1f : 1f);
            float ver = GetAxisValue(localDir, eye.verticalAxis) * (eye.invertVertical ? -1f : 1f);

            if (eye.flipHorizontal) hor = -hor;
            if (eye.flipVertical) ver = -ver;

            hor = Mathf.Clamp(hor, -1f, 1f);
            ver = Mathf.Clamp(ver, -1f, 1f);

            // Преобразуем в веса блендшейпов
            float right = Mathf.Max(0, hor);
            float left  = Mathf.Max(0, -hor);
            float up    = Mathf.Max(0, ver);
            float down  = Mathf.Max(0, -ver);

            // Применяем диапазон
            Vector2 range = eye.blendRange != Vector2.zero ? eye.blendRange : globalBlendRange;
            right = Mathf.Lerp(range.x, range.y, right);
            left  = Mathf.Lerp(range.x, range.y, left);
            up    = Mathf.Lerp(range.x, range.y, up);
            down  = Mathf.Lerp(range.x, range.y, down);

            // Зеркалирование (для правого глаза, если блендшейпы зеркальны)
            if (eye.mirrorHorizontal) (right, left) = (left, right);
            if (eye.mirrorVertical)   (up, down)   = (down, up);

            // Исключаем одновременную активацию противоположных
            if (right > left) left = 0; else if (left > right) right = 0;
            if (up > down) down = 0; else if (down > up) up = 0;

            // Применяем
            SetWeight(eye.renderer, eye.rightShapeIndex, right);
            SetWeight(eye.renderer, eye.leftShapeIndex,  left);
            SetWeight(eye.renderer, eye.upShapeIndex,    up);
            SetWeight(eye.renderer, eye.downShapeIndex,  down);
        }
    }

    private float GetAxisValue(Vector3 v, Axis axis) =>
        axis switch { Axis.X => v.x, Axis.Y => v.y, Axis.Z => v.z, _ => 0 };

    private void SetWeight(SkinnedMeshRenderer r, int idx, float val)
    {
        if (idx >= 0) r.SetBlendShapeWeight(idx, val);
    }

    public enum Axis { X, Y, Z }
}