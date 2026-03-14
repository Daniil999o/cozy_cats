using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Graphic))]
[AddComponentMenu("UI/Effects/True UI Effects")]
public class MultiUIEffects : BaseMeshEffect
{
    public enum EffectType { Shadow, Outline }

    [System.Serializable]
    public class UIEffect
    {
        public bool active = true;
        public EffectType type = EffectType.Outline;
        public Color color = new Color(1f, 1f, 1f, 1f);
        public Vector2 offset = new Vector2(0f, 0f);
        [Range(1f, 20f)] public float thickness = 3f;
    }

    public List<UIEffect> effects = new List<UIEffect>();

    // Глобальный материал, чтобы не создавать копии в памяти
    private static Material sharedEffectMaterial;

    protected override void OnEnable()
    {
        base.OnEnable();
        AutoSetupMaterial();
    }

    #if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        AutoSetupMaterial();
        if (graphic != null) graphic.SetVerticesDirty();
    }
    #endif

    private void AutoSetupMaterial()
    {
        if (graphic == null) return;

        // 1. Включаем передачу скрытого канала (UV1) в Canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            if ((canvas.additionalShaderChannels & AdditionalCanvasShaderChannels.TexCoord1) == 0)
            {
                canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
            }
        }

        // 2. АВТОМАТИЧЕСКАЯ УСТАНОВКА МАТЕРИАЛА
        if (graphic.material == null || graphic.material.name == "Default UI Material")
        {
            if (sharedEffectMaterial == null)
            {
                Shader shader = Shader.Find("UI/TrueEffects");
                if (shader != null)
                {
                    sharedEffectMaterial = new Material(shader);
                    sharedEffectMaterial.hideFlags = HideFlags.HideAndDontSave;
                }
                else
                {
                    Debug.LogWarning("MultiUIEffects: Не найден шейдер 'UI/TrueEffects'. Убедитесь, что вы создали его.");
                    return;
                }
            }
            graphic.material = sharedEffectMaterial;
        }
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || effects == null || effects.Count == 0) return;

        List<UIVertex> originalVerts = new List<UIVertex>();
        vh.GetUIVertexStream(originalVerts);
        if (originalVerts.Count == 0) return;

        // Помечаем оригинальные вершины картинки как "1" (для шейдера)
        for (int i = 0; i < originalVerts.Count; i++)
        {
            UIVertex vt = originalVerts[i];
            vt.uv1 = new Vector2(1f, 0f); 
            originalVerts[i] = vt;
        }

        List<UIVertex> newVerts = new List<UIVertex>();

        // Сначала генерируем эффекты (снизу вверх)
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            var effect = effects[i];
            if (!effect.active) continue;

            if (effect.type == EffectType.Shadow)
            {
                ApplyEffect(originalVerts, newVerts, effect.color, effect.offset);
            }
            else if (effect.type == EffectType.Outline)
            {
                float t = effect.thickness;
                Vector2 o = effect.offset;
                
                ApplyEffect(originalVerts, newVerts, effect.color, o + new Vector2(t, t));
                ApplyEffect(originalVerts, newVerts, effect.color, o + new Vector2(t, -t));
                ApplyEffect(originalVerts, newVerts, effect.color, o + new Vector2(-t, t));
                ApplyEffect(originalVerts, newVerts, effect.color, o + new Vector2(-t, -t));
                ApplyEffect(originalVerts, newVerts, effect.color, o + new Vector2(t, 0));
                ApplyEffect(originalVerts, newVerts, effect.color, o + new Vector2(-t, 0));
                ApplyEffect(originalVerts, newVerts, effect.color, o + new Vector2(0, t));
                ApplyEffect(originalVerts, newVerts, effect.color, o + new Vector2(0, -t));
            }
        }

        // В САМЫЙ КОНЕЦ списка добавляем оригинальные вершины
        // (Unity отрисовывает последние вершины поверх остальных)
        newVerts.AddRange(originalVerts);

        vh.Clear();
        vh.AddUIVertexTriangleStream(newVerts);
    }

    private void ApplyEffect(List<UIVertex> original, List<UIVertex> target, Color effectColor, Vector2 offset)
    {
        for (int i = 0; i < original.Count; i++)
        {
            UIVertex vt = original[i];
            
            vt.color = effectColor;           // Назначаем чистый цвет
            vt.uv1 = new Vector2(0f, 0f);     // Помечаем как "0" (эффект для шейдера)
            vt.position.x += offset.x;        // Сдвигаем
            vt.position.y += offset.y;
            
            target.Add(vt);
        }
    }
}