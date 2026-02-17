using UnityEngine;

public class TerrainDeformer : MonoBehaviour
{
    private static TerrainDeformer _instance;

    public static TerrainDeformer Instance {
        get {
            if (!_instance) _instance = FindFirstObjectByType<TerrainDeformer>(FindObjectsInactive.Include);

            return _instance;
        }
    }
    
    /*
    
    private void DeformTerrain(Vector3 leftTrackPos, Vector3 rightTrackPos) {
        // Обработка левого трека
        ProcessTrackDeformation(leftTrackPos);

        // Обработка правого трека
        ProcessTrackDeformation(rightTrackPos);
    }

    private void ProcessTrackDeformation(Vector3 trackPos) {
        int terrainX = Mathf.FloorToInt((trackPos.x - _terrain.transform.position.x) * _heightmapToTerrainX);
        int terrainZ = Mathf.FloorToInt((trackPos.z - _terrain.transform.position.z) * _heightmapToTerrainZ);

        // Ограничиваем координаты, чтобы избежать выхода за границы
        terrainX = Mathf.Clamp(terrainX, 0, _terrainData.heightmapResolution - 1);
        terrainZ = Mathf.Clamp(terrainZ, 0, _terrainData.heightmapResolution - 1);

        int deformationSize = Mathf.FloorToInt(trackLength * _heightmapToTerrainX);
        deformationSize = Mathf.Min(deformationSize, 8); // Ограничиваем размер деформации

        // Проверяем границы
        if (terrainX + deformationSize >= _terrainData.heightmapResolution)
            deformationSize = _terrainData.heightmapResolution - terrainX - 1;

        if (terrainZ + deformationSize >= _terrainData.heightmapResolution)
            deformationSize = _terrainData.heightmapResolution - terrainZ - 1;

        if (deformationSize <= 0) return;

        float[,] heights = _terrainData.GetHeights(terrainX, terrainZ, deformationSize, deformationSize);
        bool modified = false;

        float deformAmount = groundPressure * _deformationModifier * updateInterval;
        float minNormalizedHeight = ((_terrain.transform.position.y - maxDeformation) - _terrain.transform.position.y) / _terrainData.size.y;

        for (int i = 0; i < deformationSize; i++) {
            for (int j = 0; j < deformationSize; j++) {
                float newHeight = heights[i, j] - deformAmount / _terrainData.size.y;
                if (newHeight < heights[i, j]) {
                    heights[i, j] = Mathf.Max(newHeight, minNormalizedHeight);
                    modified = true;
                }
            }
        }

        // Применяем изменения только если что-то изменилось
        if (modified) {
            _terrainData.SetHeights(terrainX, terrainZ, heights);
        }
    }

    private void PaintTerrain(Vector3 leftTrackPos, Vector3 rightTrackPos) {
        // Используем буфер для обновления текстур пакетно
        ProcessTrackTexture(leftTrackPos);
        ProcessTrackTexture(rightTrackPos);
    }

    private void ProcessTrackTexture(Vector3 trackPos) {
        int mapX = Mathf.FloorToInt((trackPos.x - _terrain.transform.position.x) * _alphamapToTerrainX);
        int mapZ = Mathf.FloorToInt((trackPos.z - _terrain.transform.position.z) * _alphamapToTerrainZ);

        // Ограничиваем координаты
        mapX = Mathf.Clamp(mapX, 0, _terrainData.alphamapWidth - 1);
        mapZ = Mathf.Clamp(mapZ, 0, _terrainData.alphamapHeight - 1);

        // Используем небольшой размер для обновления текстуры
        int textureSize = 2;

        if (mapX + textureSize >= _terrainData.alphamapWidth)
            textureSize = _terrainData.alphamapWidth - mapX;

        if (mapZ + textureSize >= _terrainData.alphamapHeight)
            textureSize = _terrainData.alphamapHeight - mapZ;

        if (textureSize <= 0) return;

        float[,,] alphaMaps = _terrainData.GetAlphamaps(mapX, mapZ, textureSize, textureSize);
        float alphaIncrement = _deformationModifier * updateInterval;

        for (int x = 0; x < textureSize; x++) {
            for (int z = 0; z < textureSize; z++) {
                // Увеличиваем значение текстуры трека
                alphaMaps[x, z, textureLayer] = 1f;//Mathf.Min(alphaMaps[x, z, textureLayer] + alphaIncrement, 1f);

                // Нормализуем остальные слои, чтобы сумма всегда была 1
                float sum = 0;
                for (int l = 0; l < alphaMaps.GetLength(2); l++) {
                    sum += alphaMaps[x, z, l];
                }

                if (sum > 1f) {
                    float normalizer = 1f / sum;
                    for (int l = 0; l < alphaMaps.GetLength(2); l++) {
                        alphaMaps[x, z, l] *= normalizer;
                    }
                }
            }
        }

        _terrainData.SetAlphamaps(mapX, mapZ, alphaMaps);
    }

    public void CreateExplosionCrater(Vector3 explosionPosition, float radius, float force) {
        if (_terrain == null || _terrainData == null) return;

        force *= 0.0001f;

        force *= 1.2f;
        radius *= 1.2f;
        
        force *= Random.Range(0.9f, 2f);
        radius *= Random.Range(0.9f, 2f);
        
        // Проверяем, находится ли взрыв над террейном
        if (explosionPosition.y > _terrain.SampleHeight(explosionPosition) + _terrain.transform.position.y + radius) {
            return; // Взрыв произошел слишком высоко над землей
        }

        // 1. Деформация ландшафта (создание воронки)
        DeformTerrainCrater(explosionPosition, radius, force);

        // 2. Изменение текстуры в месте взрыва
        PaintExplosionTexture(explosionPosition, radius);
        
        StickToTerrain.UpdateStickObjects();
    }
    
    private void DeformTerrainCrater(Vector3 explosionPosition, float radius, float force) {
        // Переводим мировые координаты в координаты террейна
        float relativeX = explosionPosition.x - _terrain.transform.position.x;
        float relativeZ = explosionPosition.z - _terrain.transform.position.z;

        // Преобразуем в координаты heightmap
        int heightmapX = Mathf.FloorToInt(relativeX * _heightmapToTerrainX);
        int heightmapZ = Mathf.FloorToInt(relativeZ * _heightmapToTerrainZ);

        // Определяем размер деформации в единицах heightmap
        int deformationRadius = Mathf.CeilToInt(radius * _heightmapToTerrainX);

        // Создаем квадратную область для деформации
        int size = deformationRadius * 2 + 1;

        int startX, startZ;

        // Проверяем границы террейна
        if (heightmapX - deformationRadius < 0 || 
            heightmapZ - deformationRadius < 0 ||
            heightmapX + deformationRadius >= _terrainData.heightmapResolution ||
            heightmapZ + deformationRadius >= _terrainData.heightmapResolution) {

            // Корректируем позицию и размер, чтобы они не выходили за границы
            startX = Mathf.Max(0, heightmapX - deformationRadius);
            startZ = Mathf.Max(0, heightmapZ - deformationRadius);
            int endX = Mathf.Min(_terrainData.heightmapResolution - 1, heightmapX + deformationRadius);
            int endZ = Mathf.Min(_terrainData.heightmapResolution - 1, heightmapZ + deformationRadius);

            size = Mathf.Max(endX - startX, endZ - startZ);
            heightmapX = startX + size / 2;
            heightmapZ = startZ + size / 2;

            // Если область слишком мала, выходим
            if (size <= 2) return;
        }

        // Получаем текущие высоты
        startX = Mathf.Max(0, heightmapX - size/2);
        startZ = Mathf.Max(0, heightmapZ - size/2);

        // Окончательная проверка на выход за пределы
        if (startX + size >= _terrainData.heightmapResolution)
            size = _terrainData.heightmapResolution - startX - 1;

        if (startZ + size >= _terrainData.heightmapResolution)
            size = _terrainData.heightmapResolution - startZ - 1;

        if (size <= 0) return;

        float[,] heights = _terrainData.GetHeights(startX, startZ, size, size);
        float[,] originalHeights = new float[size, size];

        // Создаем копию исходных высот
        for (int x = 0; x < size; x++) {
            for (int z = 0; z < size; z++) {
                originalHeights[z, x] = heights[z, x];
            }
        }

        // Центр взрыва в локальных координатах массива heights
        float centerX = heightmapX - startX;
        float centerZ = heightmapZ - startZ;

        // Максимальная глубина воронки (нормализованная)
        float maxDepth = force / _terrainData.size.y;

        // Высота краев кратера - увеличиваем для более выраженных краев
        float rimHeight = maxDepth * 0.25f; // Увеличиваем до 25% от глубины

        // Определяем зоны кратера
        float innerRadius = deformationRadius * 0.65f; // Внутренняя часть (воронка)
        float rimRadius = deformationRadius * 0.85f;   // Расположение пика края
        float outerRadius = deformationRadius * 1.1f;  // Внешняя граница влияния

        float innerRadiusSqr = innerRadius * innerRadius;
        float rimRadiusSqr = rimRadius * rimRadius;
        float outerRadiusSqr = outerRadius * outerRadius;

        // Добавляем небольшое случайное искажение
        float ellipseRatio = Random.Range(0.9f, 1.1f);
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float randomOffsetX = Random.Range(-0.1f, 0.1f) * deformationRadius;
        float randomOffsetZ = Random.Range(-0.1f, 0.1f) * deformationRadius;

        bool modified = false;

        // Проходим по всем точкам
        for (int x = 0; x < size; x++) {
            for (int z = 0; z < size; z++) {
                // Вычисляем расстояние до центра кратера с эллиптическим искажением
                float dx = x - centerX + randomOffsetX;
                float dz = z - centerZ + randomOffsetZ;

                // Применяем поворот
                float rotatedX = dx * Mathf.Cos(randomAngle) - dz * Mathf.Sin(randomAngle);
                float rotatedZ = dx * Mathf.Sin(randomAngle) + dz * Mathf.Cos(randomAngle);

                // Эллиптическое расстояние
                float distSqr = (rotatedX * rotatedX) / (ellipseRatio * ellipseRatio) + (rotatedZ * rotatedZ);

                // Если точка в зоне влияния кратера
                if (distSqr <= outerRadiusSqr) {
                    float deformation = 0f;

                    if (distSqr <= innerRadiusSqr) {
                        // Внутренняя часть кратера (воронка)
                        float normalizedDist = Mathf.Sqrt(distSqr) / innerRadius;

                        // Более выраженный профиль с более плоским дном
                        float depthFactor = Mathf.Pow(normalizedDist, 1.8f);
                        deformation = -maxDepth * (1.0f - depthFactor);

                        // Добавляем немного случайности
                        deformation *= Random.Range(0.95f, 1.05f);
                    } 
                    else if (distSqr <= rimRadiusSqr) {
                        // Зона перехода от впадины к краю - создаем выраженный край
                        float t = (Mathf.Sqrt(distSqr) - innerRadius) / (rimRadius - innerRadius);

                        // Используем синусоидальный профиль для плавного нарастания и более выраженного пика
                        float rimProfile = Mathf.Sin(t * Mathf.PI * 0.5f); // Полусинусоида для нарастания к пику
                        deformation = rimHeight * rimProfile;

                        // Немного случайности для неровных краев
                        deformation *= Random.Range(0.9f, 1.1f);

                        // Проверяем, не находится ли точка в существующем кратере
                        if (heights[z, x] < originalHeights[z, x] - maxDepth * 0.3f) {
                            // Если точка в существующем кратере, уменьшаем поднятие
                            deformation *= 0.3f;
                        }
                    }
                    else {
                        // Внешняя зона - плавное затухание поднятия
                        float t = (Mathf.Sqrt(distSqr) - rimRadius) / (outerRadius - rimRadius);

                        // Плавное затухание от края к обычной поверхности
                        float falloffProfile = Mathf.Cos(t * Mathf.PI * 0.5f); // Косинусоидальное затухание
                        deformation = rimHeight * 0.6f * falloffProfile; // 60% от высоты края

                        // Немного случайности
                        deformation *= Random.Range(0.8f, 1.2f);

                        // Проверка на существующий кратер
                        if (heights[z, x] < originalHeights[z, x] - maxDepth * 0.2f) {
                            deformation *= 0.2f;
                        }
                    }

                    // Применяем деформацию
                    if (deformation < 0) {
                        // Опускаем землю для создания воронки
                        heights[z, x] = Mathf.Max(heights[z, x] + deformation, 0);
                        modified = true;
                    } 
                    else if (deformation > 0) {
                        // Поднимаем землю для краев с учетом существующих кратеров
                        heights[z, x] += deformation;
                        modified = true;
                    }
                }
            }
        }

        // Сглаживаем края для проходимости
        if (modified) {
            // Сглаживаем кратер несколько раз с разной интенсивностью
            // Умеренное сглаживание чтобы сохранить выраженность краев
            ApplySmoothingToHeights(heights, centerX, centerZ, outerRadius, 0.25f);

            // Более интенсивное сглаживание только для внешней зоны въезда/выезда
            SmoothRimEntrance(heights, centerX, centerZ, rimRadius, outerRadius, 0.4f);

            _terrainData.SetHeights(startX, startZ, heights);
        }
    }
    
    private void SmoothRimEntrance(float[,] heights, float centerX, float centerZ, float innerLimit, float outerLimit, float smoothFactor) {
        int size = heights.GetLength(0);
        float[,] smoothedHeights = new float[size, size];
        float innerLimitSqr = innerLimit * innerLimit;
        float outerLimitSqr = outerLimit * outerLimit;
        
        // Копируем высоты
        for (int x = 0; x < size; x++) {
            for (int z = 0; z < size; z++) {
                smoothedHeights[z, x] = heights[z, x];
            }
        }

        // Сглаживаем только переходную зону края (для въезда)
        for (int x = 1; x < size - 1; x++) {
            for (int z = 1; z < size - 1; z++) {
                float distSqr = (x - centerX) * (x - centerX) + (z - centerZ) * (z - centerZ);

                // Применяем только к зоне внешнего края
                if (distSqr > innerLimitSqr && distSqr < outerLimitSqr) {
                    // Градиент сглаживания - сильнее на внешнем крае, слабее на внутреннем
                    float t = (Mathf.Sqrt(distSqr) - innerLimit) / (outerLimit - innerLimit);
                    float localSmoothFactor = smoothFactor * Mathf.SmoothStep(0.5f, 1.0f, t);

                    // Расширенное сглаживание для более плавного съезда
                    float sum = heights[z, x] * 2; // Центральная точка с большим весом
                    float count = 2;

                    // 3x3 окно
                    for (int nx = x-1; nx <= x+1; nx++) {
                        for (int nz = z-1; nz <= z+1; nz++) {
                            if ((nx != x || nz != z) && nx >= 0 && nx < size && nz >= 0 && nz < size) {
                                // Точки дальше от центра получают больший вес
                                float nDistSqr = (nx - centerX) * (nx - centerX) + (nz - centerZ) * (nz - centerZ);
                                float weight = nDistSqr > distSqr ? 1.5f : 1.0f;

                                sum += heights[nz, nx] * weight;
                                count += weight;
                            }
                        }
                    }

                    // Применяем сглаживание
                    smoothedHeights[z, x] = heights[z, x] * (1 - localSmoothFactor) + (sum / count) * localSmoothFactor;
                }
            }
        }

        // Возвращаем результат
        for (int x = 0; x < size; x++) {
            for (int z = 0; z < size; z++) {
                heights[z, x] = smoothedHeights[z, x];
            }
        }
    }

    // Упрощенный метод сглаживания
    private void ApplySmoothingToHeights(float[,] heights, float centerX, float centerZ, float radius, float smoothFactor) {
        int size = heights.GetLength(0);
        float[,] smoothedHeights = new float[size, size];
        float radiusSqr = radius * radius;
    
        // Копируем высоты
        for (int x = 0; x < size; x++) {
            for (int z = 0; z < size; z++) {
                smoothedHeights[z, x] = heights[z, x];
            }
        }
    
        // Сглаживаем
        for (int x = 1; x < size - 1; x++) {
            for (int z = 1; z < size - 1; z++) {
                float distSqr = (x - centerX) * (x - centerX) + (z - centerZ) * (z - centerZ);
            
                if (distSqr <= radiusSqr) {
                    // Базовый фактор сглаживания
                    float localSmoothFactor = smoothFactor;
                
                    // Упрощенное сглаживание 3x3
                    float sum = heights[z, x] * 2; // Больший вес центральной точке
                    float count = 2;
                
                    for (int nx = x-1; nx <= x+1; nx++) {
                        for (int nz = z-1; nz <= z+1; nz++) {
                            if ((nx != x || nz != z) && nx >= 0 && nx < size && nz >= 0 && nz < size) {
                                sum += heights[nz, nx];
                                count++;
                            }
                        }
                    }
                
                    // Применяем сглаживание
                    smoothedHeights[z, x] = heights[z, x] * (1 - localSmoothFactor) + (sum / count) * localSmoothFactor;
                }
            }
        }
    
        // Возвращаем результат
        for (int x = 0; x < size; x++) {
            for (int z = 0; z < size; z++) {
                heights[z, x] = smoothedHeights[z, x];
            }
        }
    }

    private void SmoothCraterOuterEdges(float[,] heights, float centerX, float centerZ, float sqrInnerLimit, float sqrOuterLimit, float smoothFactor) {
        int size = heights.GetLength(0);
        float[,] smoothedHeights = new float[size, size];

        // Копируем исходные высоты
        for (int x = 0; x < size; x++) {
            for (int z = 0; z < size; z++) {
                smoothedHeights[z, x] = heights[z, x];
            }
        }

        // Сглаживаем только внешнюю зону кратера (переходную область)
        for (int x = 2; x < size - 2; x++) {
            for (int z = 2; z < size - 2; z++) {
                float sqrDist = (x - centerX) * (x - centerX) + (z - centerZ) * (z - centerZ);

                // Применяем только к внешней переходной зоне
                if (sqrDist > sqrInnerLimit && sqrDist < sqrOuterLimit) {
                    // Расширенное сглаживание с большим радиусом для плавного перехода
                    float sum = heights[z, x] * 4;
                    float count = 4;

                    // Ближайшие соседи (крест)
                    sum += heights[z-1, x] + heights[z+1, x] + heights[z, x-1] + heights[z, x+1];
                    count += 4;

                    // Диагональные соседи
                    sum += heights[z-1, x-1] + heights[z-1, x+1] + heights[z+1, x-1] + heights[z+1, x+1];
                    count += 4;

                    // Добавляем более дальних соседей для лучшего сглаживания
                    sum += heights[z-2, x] + heights[z+2, x] + heights[z, x-2] + heights[z, x+2];
                    count += 4;

                    // Диагональные соседи второго уровня
                    sum += heights[z-2, x-1] + heights[z-2, x+1] + heights[z+2, x-1] + heights[z+2, x+1];
                    sum += heights[z-1, x-2] + heights[z+1, x-2] + heights[z-1, x+2] + heights[z+1, x+2];
                    count += 8;

                    // Интенсивность сглаживания повышается к краю переходной зоны
                    float t = (sqrDist - sqrInnerLimit) / (sqrOuterLimit - sqrInnerLimit);
                    float localSmoothFactor = smoothFactor * Mathf.SmoothStep(0.5f, 1.0f, t);

                    // Сглаженная высота
                    smoothedHeights[z, x] = heights[z, x] * (1 - localSmoothFactor) + (sum / count) * localSmoothFactor;
                }
            }
        }

        // Возвращаем сглаженные высоты
        for (int x = 0; x < size; x++) {
            for (int z = 0; z < size; z++) {
                heights[z, x] = smoothedHeights[z, x];
            }
        }
    }
    
    // Метод для сглаживания краев кратера
    private void SmoothCraterEdges(float[,] heights, float centerX, float centerZ, float sqrRadius, float smoothFactor) {
        int size = heights.GetLength(0);
        float[,] smoothedHeights = new float[size, size];

        // Копируем исходные высоты
        for (int x = 0; x < size; x++) {
            for (int z = 0; z < size; z++) {
                smoothedHeights[z, x] = heights[z, x];
            }
        }

        // Сглаживаем только в области кратера и вокруг него
        for (int x = 1; x < size - 1; x++) {
            for (int z = 1; z < size - 1; z++) {
                float sqrDist = (x - centerX) * (x - centerX) + (z - centerZ) * (z - centerZ);

                // Сглаживаем только в области кратера и его краев
                if (sqrDist <= sqrRadius) {
                    // Изменяем фактор сглаживания в зависимости от положения
                    float localSmoothFactor = smoothFactor;
                    
                    // Больше сглаживания на краях кратера (где примерно 0.7-1.0 радиуса)
                    float distRatio = Mathf.Sqrt(sqrDist / sqrRadius);
                    if (distRatio > 0.7f) {
                        // Усиливаем сглаживание на краях для более плавного въезда
                        localSmoothFactor *= 1.0f + (distRatio - 0.7f) * 1.5f;
                    }
                    
                    // Ограничиваем максимальный фактор сглаживания
                    localSmoothFactor = Mathf.Min(localSmoothFactor, 0.6f);

                    // Расширенное размытие с разными весами
                    float sum = heights[z, x] * 4.0f; // Центральная точка имеет больший вес
                    float totalWeight = 4.0f;
                    
                    // Основные соседи (крест)
                    sum += heights[z-1, x] * 2.0f + heights[z+1, x] * 2.0f + 
                           heights[z, x-1] * 2.0f + heights[z, x+1] * 2.0f;
                    totalWeight += 8.0f;
                    
                    // Диагональные соседи
                    sum += heights[z-1, x-1] + heights[z-1, x+1] + 
                           heights[z+1, x-1] + heights[z+1, x+1];
                    totalWeight += 4.0f;

                    // Применяем сглаживание с учетом весов
                    smoothedHeights[z, x] = heights[z, x] * (1 - localSmoothFactor) + 
                                           (sum / totalWeight) * localSmoothFactor;
                }
            }
        }

        // Возвращаем сглаженные высоты
        for (int x = 0; x < size; x++) {
            for (int z = 0; z < size; z++) {
                heights[z, x] = smoothedHeights[z, x];
            }
        }
    }
    
    private void PaintExplosionTexture(Vector3 explosionPosition, float radius) {
        // Переводим мировые координаты в координаты splatmap
        float relativeX = explosionPosition.x - _terrain.transform.position.x;
        float relativeZ = explosionPosition.z - _terrain.transform.position.z;

        int alphamapX = Mathf.FloorToInt(relativeX * _alphamapToTerrainX);
        int alphamapZ = Mathf.FloorToInt(relativeZ * _alphamapToTerrainZ);

        // Размер области для текстурирования
        int textureRadius = Mathf.CeilToInt(radius * _alphamapToTerrainX);
        int textureSize = textureRadius * 2 + 1;

        int startX, startZ;

        // Проверяем границы
        if (alphamapX - textureRadius < 0 || 
            alphamapZ - textureRadius < 0 ||
            alphamapX + textureRadius >= _terrainData.alphamapWidth ||
            alphamapZ + textureRadius >= _terrainData.alphamapHeight) {

            // Корректируем размеры, чтобы не выйти за границы
            startX = Mathf.Max(0, alphamapX - textureRadius);
            startZ = Mathf.Max(0, alphamapZ - textureRadius);
            int endX = Mathf.Min(_terrainData.alphamapWidth - 1, alphamapX + textureRadius);
            int endZ = Mathf.Min(_terrainData.alphamapHeight - 1, alphamapZ + textureRadius);

            textureSize = Mathf.Max(endX - startX, endZ - startZ);
            alphamapX = startX + textureSize / 2;
            alphamapZ = startZ + textureSize / 2;

            if (textureSize <= 2) return;
        }

        // Получаем текущие текстуры
        startX = Mathf.Max(0, alphamapX - textureSize/2);
        startZ = Mathf.Max(0, alphamapZ - textureSize/2);

        // Финальная проверка
        if (startX + textureSize >= _terrainData.alphamapWidth)
            textureSize = _terrainData.alphamapWidth - startX - 1;

        if (startZ + textureSize >= _terrainData.alphamapHeight)
            textureSize = _terrainData.alphamapHeight - startZ - 1;

        if (textureSize <= 0) return;

        float[,,] alphaMaps = _terrainData.GetAlphamaps(startX, startZ, textureSize, textureSize);

        // Центр взрыва в локальных координатах
        float centerX = alphamapX - startX;
        float centerZ = alphamapZ - startZ;

        // Квадрат радиуса для оптимизации
        float sqrRadius = textureRadius * textureRadius;

        // Добавляем случайное смещение и эллиптичность
        float offsetX = Random.Range(-0.1f, 0.1f) * textureRadius;
        float offsetZ = Random.Range(-0.1f, 0.1f) * textureRadius;
        float ellipseRatio = Random.Range(0.85f, 1.15f);
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        bool modified = false;

        // Модифицируем текстуры
        for (int x = 0; x < textureSize; x++) {
            for (int z = 0; z < textureSize; z++) {
                // Создаем эллиптическое искажение с поворотом
                float distX = x - centerX + offsetX;
                float distZ = z - centerZ + offsetZ;

                // Применяем поворот
                float rotatedX = distX * Mathf.Cos(randomAngle) - distZ * Mathf.Sin(randomAngle);
                float rotatedZ = distX * Mathf.Sin(randomAngle) + distZ * Mathf.Cos(randomAngle);

                // Вычисляем эллиптическое расстояние
                float sqrDist = (rotatedX * rotatedX) / (ellipseRatio * ellipseRatio) + (rotatedZ * rotatedZ);

                if (sqrDist <= sqrRadius) {
                    // Более плавное распределение текстуры
                    float distRatio = Mathf.Sqrt(sqrDist / sqrRadius);

                    // Создаем градиент текстуры с неравномерностями
                    float intensity;

                    if (distRatio < 0.6f) {
                        // Внутренняя часть кратера - максимальная интенсивность
                        intensity = 1.0f;
                        // Добавляем случайные вариации в центре
                        intensity *= Random.Range(0.95f, 1.0f);
                    } else {
                        // Края кратера - постепенное затухание с шумом
                        intensity = 1.0f - ((distRatio - 0.6f) / 0.4f);
                        // Добавляем шум для неровного края
                        intensity *= Random.Range(0.75f, 1.15f);
                    }

                    // Нелинейный профиль для более естественного вида
                    intensity = Mathf.Pow(intensity, 0.8f);

                    // Добавляем случайные "пятна" более высокой интенсивности
                    if (Random.value < 0.15f) {
                        intensity = Mathf.Min(intensity * 1.2f, 1.0f);
                    }

                    // Увеличиваем значение текстуры кратера с плавным переходом
                    float currentCraterValue = alphaMaps[z, x, craterTextureLayer];
                    // Используем максимум от текущего и нового значения для наложения кратеров
                    alphaMaps[z, x, craterTextureLayer] = Mathf.Max(
                        currentCraterValue, 
                        Mathf.Lerp(currentCraterValue, 1.0f, intensity * 0.9f)
                    );

                    // Нормализуем остальные слои
                    float totalWeight = 0;
                    for (int l = 0; l < alphaMaps.GetLength(2); l++) {
                        totalWeight += alphaMaps[z, x, l];
                    }

                    if (totalWeight > 1.0f) {
                        float normalizer = 1.0f / totalWeight;
                        for (int l = 0; l < alphaMaps.GetLength(2); l++) {
                            alphaMaps[z, x, l] *= normalizer;
                        }
                    }

                    modified = true;
                }
            }
        }

        // Применяем изменения если нужно
        if (modified) {
            _terrainData.SetAlphamaps(startX, startZ, alphaMaps);
        }
    }

    */
}