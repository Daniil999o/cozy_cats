using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class YG_DividedImage : MonoBehaviour
{
    [SerializeField] private Sprite _sprite;
    [SerializeField] private int _divisionsX = 2;
    [SerializeField] private int _divisionsY = 2;

    private GameObject[,] _imageGrid;

    private void OnGUI()
    {
        UpdateDividedImage();
    }

    private void UpdateDividedImage()
    {
        // Очистка предыдущих изображений
        if (_imageGrid != null)
        {
            for (int x = 0; x < _imageGrid.GetLength(0); x++)
            {
                for (int y = 0; y < _imageGrid.GetLength(1); y++)
                {
                    if (_imageGrid[x, y] != null)
                        DestroyImmediate(_imageGrid[x, y]);
                }
            }
        }

        if (_sprite == null)
        {
            return;
        }

        _imageGrid = new GameObject[_divisionsX, _divisionsY];

        float partWidth = _sprite.rect.width / _divisionsX;
        float partHeight = _sprite.rect.height / _divisionsY;

        for (int x = 0; x < _divisionsX; x++)
        {
            for (int y = 0; y < _divisionsY; y++)
            {
                // Создаем новый GameObject для Image
                GameObject imagePart = new GameObject("ImagePart_" + x + "_" + y);
                imagePart.transform.SetParent(transform);

                // Добавляем компонент Image
                Image image = imagePart.AddComponent<Image>();
                RectTransform partRectTransform = image.rectTransform;

                image.type = Image.Type.Sliced;
                partRectTransform.sizeDelta = new Vector2(partWidth, partHeight);

                // Настраиваем anchorMin и anchorMax
                partRectTransform.anchorMin = new Vector2(x / (float)_divisionsX, y / (float)_divisionsY);
                partRectTransform.anchorMax = new Vector2((x + 1) / (float)_divisionsX, (y + 1) / (float)_divisionsY);
                partRectTransform.offsetMin = Vector2.zero;
                partRectTransform.offsetMax = Vector2.zero;


                // Создаем новый спрайт, указывая область исходной текстуры
                Rect partRect = new Rect(
                    _sprite.textureRect.xMin + x * partWidth,
                    _sprite.textureRect.yMin + y * partHeight,
                    partWidth,
                    partHeight);


                Sprite partSprite = Sprite.Create(_sprite.texture, partRect, new Vector2(0.5f, 0.5f), _sprite.pixelsPerUnit);
                image.sprite = partSprite;
                _imageGrid[x,y] = imagePart;

            }
        }
    }
}