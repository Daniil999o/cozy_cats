using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class YG_CompassEditor : MonoBehaviour
{
    private static bool showCompass = true; // Переключатель для отображения компаса

    [MenuItem("YG Plugin Provider/Tools/Compass")]
    public static void ToggleCompass()
    {
        showCompass = !showCompass;
        SceneView.RepaintAll(); // Перерисовываем все окна Scene View
    }
    
    private void OnDrawGizmos()
    {
        if (!showCompass) return;

        DrawCompass();
    }

    private void DrawCompass()
    {
        // Получаем текущее направление камеры
        Vector3 north = Vector3.up * 5;   // Север
        Vector3 east = Vector3.right * 5; // Восток
        Vector3 south = Vector3.down * 5; // Юг
        Vector3 west = Vector3.left * 5;  // Запад

        // Рисуем линии компаса
        Gizmos.color = Color.red; // Север
        Gizmos.DrawLine(transform.position, transform.position + north);
        
        Gizmos.color = Color.green; // Восток
        Gizmos.DrawLine(transform.position, transform.position + east);
        
        Gizmos.color = Color.yellow; // Юг
        Gizmos.DrawLine(transform.position, transform.position + south);
        
        Gizmos.color = Color.blue; // Запад
        Gizmos.DrawLine(transform.position, transform.position + west);

        // Рисуем направление камеры
        Vector3 cameraDirection = Camera.current.transform.forward * 5;
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, transform.position + cameraDirection);
    }
}