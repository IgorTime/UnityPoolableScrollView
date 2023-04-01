using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class Scroll : MonoBehaviour
{
    [SerializeField]
    private ScrollRect scrollRect;

    private IEnumerable<IElementData> itemsData;

    public void Start()
    {
        var data = new IElementData[]
        {
            new TextData {Text = "Zalupa1"}, // 100
            new TextData {Text = "Zalupa2"}, // 100
            new SpriteData {Sprite = null}, // 200
            new SpriteData {Sprite = null}, // 200
            new SpriteData {Sprite = null}, // 200
            new SpriteData {Sprite = null}, // 200
            new SpriteData {Sprite = null}, // 200
            new SpriteData {Sprite = null}, // 200
            new SpriteData {Sprite = null}, // 200
            new SpriteData {Sprite = null}, // 200
            new SpriteData {Sprite = null}, // 200
            new SpriteData {Sprite = null}, // 200
            new TextData {Text = "Zalupa3"}, //
            new TextData {Text = "Zalupa4"},
            new TextData {Text = "Zalupa4"},
            new TextData {Text = "Zalupa4"},
            new TextData {Text = "Zalupa4"},
            new TextData {Text = "Zalupa4"},
            new TextData {Text = "Zalupa4"},
            new TextData {Text = "Zalupa4"},
            new TextData {Text = "Zalupa4"},
            new TextData {Text = "Zalupa4"},
            new SpriteData {Sprite = null},
            new SpriteData {Sprite = null},
            new SpriteData {Sprite = null},
            new SpriteData {Sprite = null},
            new SpriteData {Sprite = null},
            new SpriteData {Sprite = null},
            new SpriteData {Sprite = null},
            new SpriteData {Sprite = null},
            new SpriteData {Sprite = null},
            new SpriteData {Sprite = null},
            new SpriteData {Sprite = null},
        };

        Initialize(data);
    }

    private void OnEnable()
    {
        scrollRect.onValueChanged.AddListener(UpdateScrollItems);
    }

    private void OnDisable()
    {
        scrollRect.onValueChanged.RemoveListener(UpdateScrollItems);
    }

    public void Initialize(IEnumerable<IElementData> itemsData)
    {
        this.itemsData = itemsData;

        SetContentSize(itemsData);
        CreateInitialElements(itemsData);
    }

    public ElementView CreateElement(IElementData data, Vector2 position)
    {
        var prefab = Resources.Load<ElementView>(data.PrefabPath);
        var elementView = Instantiate(prefab, scrollRect.content);
        elementView.Initialize(data);
        elementView.GetComponent<RectTransform>().anchoredPosition = position;
        return elementView;
    }

    public Vector2 GetElementSize(IElementData data)
    {
        var prefab = Resources.Load<ElementView>(data.PrefabPath);
        return prefab.Size;
    }

    private void OnValidate()
    {
        if (!scrollRect)
        {
            scrollRect = GetComponent<ScrollRect>();
        }
    }

    private void CreateInitialElements(IEnumerable<IElementData> elementDatas)
    {
        var startPosition = scrollRect.content.rect.height / 2;
        foreach (var elementData in elementDatas)
        {
            var elementHeightHalf = GetElementSize(elementData).y / 2;
            var elementPositionY = startPosition - elementHeightHalf;
            var elementCenterPosition = new Vector2(0, elementPositionY);
            var elementTopPosition = new Vector2(0, elementPositionY + elementHeightHalf);
            var elementDownPosition = new Vector2(0, elementPositionY - elementHeightHalf);

            if (IsVisible(elementCenterPosition, elementHeightHalf))
            {
                var element = CreateElement(elementData, elementCenterPosition);
                startPosition = elementPositionY - elementHeightHalf;
            }
        }
    }

    private bool IsVisible(Vector2 elementCenterPosition, float elementHeightHalf) =>
        throw new NotImplementedException();

    private void SetContentSize(IEnumerable<IElementData> itemsData)
    {
        var height = CalculateFullContentHeight(itemsData);
        scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, height);
    }

    private float CalculateFullContentHeight(IEnumerable<IElementData> elementDatas)
    {
        var height = 0f;
        foreach (var elementData in elementDatas)
        {
            height += GetElementSize(elementData).y;
        }

        return height;
    }

    private void UpdateScrollItems(Vector2 arg0)
    {
    }
}