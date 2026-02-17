using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class BallPhysicsEditor : EditorWindow
{
    [System.Serializable]
    class Ball
    {
        public Vector2 position;
        public Vector2 velocity;
        public float radius;
        public Color color;
        public float mass = 1f;
        
        public Ball(Vector2 pos, float r)
        {
            position = pos;
            velocity = Random.insideUnitCircle * 50f;
            radius = r;
            color = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.7f, 1f);
            mass = Random.Range(0.8f, 1.2f);
        }
    }
    
    // Настройки
    private List<Ball> balls = new List<Ball>();
    private const int BallCount = 15;
    private const float BallRadius = 12f;
    private Vector2 gravity = new Vector2(0f, -400f);
    private float friction = 0.995f;
    private float bounceFactor = 0.9f;
    
    // Стаканчик
    private Rect cupRect;
    private float cupPadding = 20f;
    private Color cupColor = new Color(0.8f, 0.6f, 0.4f, 0.8f);
    private Color cupBorderColor = new Color(0.5f, 0.3f, 0.1f, 1f);
    
    // Для отслеживания тряски окна
    private Vector2 lastWindowPosition;
    private Vector2 windowVelocity;
    private Vector2 windowShakeAccumulator;
    
    // Физические константы
    private const float ShakeForceMultiplier = 0.5f;
    private const float WindowShakeDamping = 0.9f;

    private int ballsCount = 1;
    
    [MenuItem("YG Plugin Provider/Fun/Ball Physics")]
    static void ShowWindow()
    {
        var window = GetWindow<BallPhysicsEditor>();
        window.titleContent = new GUIContent("Cup Physics");
        window.minSize = new Vector2(500, 600);
        window.Show();
        
        // Инициализируем позицию окна для отслеживания
        window.lastWindowPosition = window.position.position;
    }
    
    void OnEnable()
    {
        // Инициализируем шарики
        InitializeCup();
        InitializeBalls();
        
        // Подписываемся на обновление
        EditorApplication.update += UpdatePhysics;
        
        // Запоминаем начальную позицию окна
        lastWindowPosition = position.position;
    }
    
    void OnDisable()
    {
        EditorApplication.update -= UpdatePhysics;
    }
    
    void InitializeCup()
    {
        // Стаканчик занимает 80% окна по центру
        float cupWidth = position.width * 0.8f;
        float cupHeight = position.height * 0.7f;
        float cupX = (position.width - cupWidth) / 2f;
        float cupY = (position.height - cupHeight) / 2f;
        
        cupRect = new Rect(cupX, cupY, cupWidth, cupHeight);
    }
    
    void InitializeBalls()
    {
        balls.Clear();
        
        // Создаем шарики ТОЛЬКО внутри стаканчика
        for (int i = 0; i < BallCount; i++)
        {
            // Случайная позиция внутри стаканчика с отступом от краев
            float safeX = Random.Range(
                cupRect.x + cupPadding + BallRadius, 
                cupRect.x + cupRect.width - cupPadding - BallRadius
            );
            float safeY = Random.Range(
                cupRect.y + cupPadding + BallRadius, 
                cupRect.y + cupRect.height - cupPadding - BallRadius
            );
            
            balls.Add(new Ball(new Vector2(safeX, safeY), BallRadius));
        }
    }
    
    void UpdatePhysics()
    {
        if (balls.Count == 0) return;
        
        float deltaTime = Time.deltaTime;
        
        // 1. Сначала обновляем "тряску" от движения окна
        UpdateWindowShake(deltaTime);
        
        // 2. Обновляем физику каждого шарика
        for (int i = 0; i < balls.Count; i++)
        {
            Ball ball = balls[i];
            
            // Применяем гравитацию
            ball.velocity += gravity * deltaTime;
            
            // Добавляем силу от тряски окна
            ball.velocity += windowShakeAccumulator * ball.mass;
            
            // Применяем трение (воздух в стаканчике)
            ball.velocity *= Mathf.Pow(friction, deltaTime * 60f);
            
            // Обновляем позицию
            ball.position += ball.velocity * deltaTime;
            
            // Проверяем столкновения со стенками стаканчика
            HandleCupCollision(ball);
            
            // Проверяем столкновения между шариками
            for (int j = i + 1; j < balls.Count; j++)
            {
                HandleBallCollision(ball, balls[j]);
            }
        }
        
        // 3. Гасим накопленную силу тряски
        windowShakeAccumulator *= WindowShakeDamping;
        
        // 4. Перерисовываем окно
        Repaint();
    }
    
    void UpdateWindowShake(float deltaTime)
    {
        // Получаем текущую позицию окна редактора
        Vector2 currentWindowPos = position.position;
        
        // Вычисляем скорость перемещения окна
        Vector2 delta = currentWindowPos - lastWindowPosition;
        windowVelocity = delta / deltaTime;
        
        // Если окно двигалось быстро - это тряска!
        if (windowVelocity.magnitude > 10f)
        {
            // Накопление силы тряски (чем быстрее двигали окно, тем сильнее тряска)
            float shakeForce = Mathf.Clamp(windowVelocity.magnitude * ShakeForceMultiplier, 0f, 200f);
            Vector2 shakeDirection = windowVelocity.normalized;
            
            windowShakeAccumulator += shakeDirection * shakeForce * deltaTime;
        }
        
        // Сохраняем позицию для следующего кадра
        lastWindowPosition = currentWindowPos;
    }
    
    void HandleCupCollision(Ball ball)
    {
        bool collisionOccurred = false;
        
        // Левая стенка стаканчика
        if (ball.position.x - ball.radius < cupRect.x + cupPadding)
        {
            ball.position.x = cupRect.x + cupPadding + ball.radius;
            ball.velocity.x = Mathf.Abs(ball.velocity.x) * bounceFactor;
            collisionOccurred = true;
        }
        
        // Правая стенка стаканчика
        if (ball.position.x + ball.radius > cupRect.x + cupRect.width - cupPadding)
        {
            ball.position.x = cupRect.x + cupRect.width - cupPadding - ball.radius;
            ball.velocity.x = -Mathf.Abs(ball.velocity.x) * bounceFactor;
            collisionOccurred = true;
        }
        
        // Верхняя стенка стаканчика
        if (ball.position.y - ball.radius < cupRect.y + cupPadding)
        {
            ball.position.y = cupRect.y + cupPadding + ball.radius;
            ball.velocity.y = Mathf.Abs(ball.velocity.y) * bounceFactor;
            collisionOccurred = true;
        }
        
        // Нижняя стенка стаканчика
        if (ball.position.y + ball.radius > cupRect.y + cupRect.height - cupPadding)
        {
            ball.position.y = cupRect.y + cupRect.height - cupPadding - ball.radius;
            ball.velocity.y = -Mathf.Abs(ball.velocity.y) * bounceFactor;
            collisionOccurred = true;
            
            // Немного дополнительного трения при ударе о дно
            ball.velocity *= 0.95f;
        }
        
        // Если было столкновение, добавляем немного трения
        if (collisionOccurred)
        {
            ball.velocity *= 0.98f;
        }
    }
    
    void HandleBallCollision(Ball a, Ball b)
    {
        Vector2 delta = b.position - a.position;
        float distance = delta.magnitude;
        float minDistance = a.radius + b.radius;
        
        if (distance < minDistance && distance > 0.001f)
        {
            // Нормаль столкновения
            Vector2 normal = delta / distance;
            
            // Отталкиваем шарики, чтобы они не пересекались
            float overlap = minDistance - distance;
            Vector2 separation = normal * (overlap * 0.5f);
            a.position -= separation;
            b.position += separation;
            
            // Вычисляем относительную скорость
            Vector2 relativeVelocity = a.velocity - b.velocity;
            float velocityAlongNormal = Vector2.Dot(relativeVelocity, normal);
            
            // Если шарики удаляются друг от друга, не обрабатываем столкновение
            if (velocityAlongNormal > 0) return;
            
            // Коэффициент упругости
            float e = bounceFactor * 0.8f;
            
            // Импульс столкновения
            float j = -(1 + e) * velocityAlongNormal;
            j /= (1 / a.mass + 1 / b.mass);
            
            // Применяем импульс
            Vector2 impulse = j * normal;
            a.velocity += impulse / a.mass;
            b.velocity -= impulse / b.mass;
            
            // Немного трения при столкновении
            a.velocity *= 0.99f;
            b.velocity *= 0.99f;
        }
    }
    
    void OnGUI()
    {
        // Обновляем размер стаканчика при изменении размера окна
        InitializeCup();
        
        // Фон окна
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), 
                          new Color(0.15f, 0.15f, 0.15f));
        
        // Рисуем стаканчик
        DrawCup();
        
        // Рисуем шарики
        DrawBalls();
        
        // UI панель управления
        DrawControlPanel();
        
        // Информация о тряске
        DrawShakeInfo();
    }
    
    void DrawCup()
    {
        // Внешняя граница стаканчика
        EditorGUI.DrawRect(cupRect, cupBorderColor);
        
        // Внутренняя часть стаканчика (с отступом)
        Rect innerCup = new Rect(
            cupRect.x + cupPadding,
            cupRect.y + cupPadding,
            cupRect.width - cupPadding * 2,
            cupRect.height - cupPadding * 2
        );
        
        // Градиент для стаканчика (светлее внизу)
        Texture2D cupTexture = new Texture2D(1, 2);
        cupTexture.SetPixels(new Color[] {
            new Color(cupColor.r * 1.2f, cupColor.g * 1.2f, cupColor.b * 1.2f, cupColor.a),
            new Color(cupColor.r * 0.8f, cupColor.g * 0.8f, cupColor.b * 0.8f, cupColor.a)
        });
        cupTexture.Apply();
        
        GUI.DrawTexture(innerCup, cupTexture);
        
        // Обводка стаканчика
        Handles.BeginGUI();
        Handles.color = new Color(0.3f, 0.2f, 0.1f, 1f);
        Handles.DrawSolidRectangleWithOutline(innerCup, Color.clear, new Color(0.4f, 0.3f, 0.2f, 1f));
        Handles.EndGUI();
    }
    
    void DrawBalls()
    {
        foreach (var ball in balls)
        {
            // Основной круг шарика
            Rect ballRect = new Rect(
                ball.position.x - ball.radius,
                ball.position.y - ball.radius,
                ball.radius * 2,
                ball.radius * 2
            );
            
            // Рисуем тень
            Rect shadowRect = new Rect(
                ballRect.x + 2,
                ballRect.y + 2,
                ballRect.width,
                ballRect.height
            );
            EditorGUI.DrawRect(shadowRect, new Color(0, 0, 0, 0.3f));
            
            // Основной цвет шарика
            EditorGUI.DrawRect(ballRect, ball.color);
            
            // Блик
            float highlightSize = ball.radius * 0.4f;
            Rect highlightRect = new Rect(
                ball.position.x - ball.radius * 0.7f,
                ball.position.y - ball.radius * 0.7f,
                highlightSize,
                highlightSize
            );
            EditorGUI.DrawRect(highlightRect, new Color(1, 1, 1, 0.6f));
        }
    }
    
    void DrawControlPanel()
    {
        GUILayout.BeginArea(new Rect(10, 10, 200, 200));
        
        GUILayout.Label("🍶 Cup Physics", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        if (GUILayout.Button("🔄 Reset Balls"))
        {
            InitializeBalls();
        }


        GUILayout.Space(20);
        
        ballsCount = (int)GUILayout.HorizontalSlider(ballsCount, 1, 10000);
        GUILayout.Label($"Balls Count: {ballsCount}");
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("➕ Add Ball"))
        {
            // Добавляем шарик в случайное место внутри стаканчика
            float safeX = Random.Range(
                cupRect.x + cupPadding + BallRadius, 
                cupRect.x + cupRect.width - cupPadding - BallRadius
            );
            float safeY = Random.Range(
                cupRect.y + cupPadding + BallRadius, 
                cupRect.y + cupRect.height - cupPadding - BallRadius
            );
            
            for (int i = ballsCount; i > 0; i--)
                balls.Add(new Ball(new Vector2(safeX, safeY), BallRadius));
        }
        
        if (GUILayout.Button("🗑️ Clear All"))
        {
            balls.Clear();
        }
        
        GUILayout.Space(10);
        
        // Настройки гравитации
        GUILayout.Label("Gravity Settings:");
        float newGravity = -gravity.y;
        newGravity = GUILayout.HorizontalSlider(newGravity, -800f, 800f);
        gravity.y = -newGravity;
        GUILayout.Label($"Strength: {newGravity:F0}");
        
        GUILayout.Space(10);
        
        // Информация
        GUILayout.Label($"Balls: {balls.Count}");
        GUILayout.Label($"FPS: {1f/Time.deltaTime:F1}");
        
        GUILayout.EndArea();
    }
    
    void DrawShakeInfo()
    {
        GUILayout.BeginArea(new Rect(position.width - 210, 10, 200, 100));
        
        GUILayout.Label("Window Shake Info:", EditorStyles.boldLabel);
        
        // Индикатор силы тряски
        float shakePower = windowShakeAccumulator.magnitude;
        Rect shakeBarBg = GUILayoutUtility.GetRect(150, 20);
        EditorGUI.DrawRect(shakeBarBg, new Color(0.3f, 0.3f, 0.3f));
        
        Rect shakeBar = new Rect(
            shakeBarBg.x,
            shakeBarBg.y,
            Mathf.Clamp(shakePower * 0.5f, 0, shakeBarBg.width),
            shakeBarBg.height
        );
        
        Color shakeColor = Color.Lerp(Color.green, Color.red, shakePower / 100f);
        EditorGUI.DrawRect(shakeBar, shakeColor);
        
        GUILayout.Label($"Shake Power: {shakePower:F1}");
        GUILayout.Label($"Window Speed: {windowVelocity.magnitude:F1} px/s");
        
        GUILayout.EndArea();
    }
    
    void OnInspectorUpdate()
    {
        // Гарантируем перерисовку
        Repaint();
    }
    
    // Переопределяем метод для отслеживания изменения размера окна
    void OnRectUpdated()
    {
        InitializeCup();
        Repaint();
    }
}