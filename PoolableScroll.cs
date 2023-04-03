using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class PoolableScroll : MonoBehaviour
{
    [SerializeField]
    private ScrollRect scrollRect;

    private readonly LinkedList<ElementView> activeElements = new();

    private readonly Dictionary<string, ScrollElementsPool> elementPools = new();

    private List<IElementData> itemsData;
    private Vector2? previousContentPosition;
    private RectTransform Content => scrollRect.content;
    private RectTransform Viewport => scrollRect.viewport;
    private ElementView First => activeElements.First.Value;
    private ElementView Last => activeElements.Last.Value;

    private void OnEnable()
    {
        scrollRect.onValueChanged.AddListener(UpdateScrollItems);
    }

    private void OnDisable()
    {
        scrollRect.onValueChanged.RemoveListener(UpdateScrollItems);
    }

    public void Initialize(List<IElementData> itemsData)
    {
        this.itemsData = itemsData;

        SetContentSize(itemsData);
        CreateInitialElements(itemsData);
        previousContentPosition = scrollRect.normalizedPosition;
    }

    private Vector2 GetElementSize(IElementData data)
    {
        var prefab = PeekElementView(data);
        return prefab.Size;
    }

    private ElementView PeekElementView(IElementData data)
    {
        var pool = GetElementPool(data.PrefabPath);
        return pool.Peek();
    }

    private ElementView CreateElement(IElementData data, Vector2 position, int index)
    {
        var elementView = GetElementView(data);
        elementView.Initialize(data, index);
        elementView.GetComponent<RectTransform>().anchoredPosition = position;
        return elementView;
    }

    private ElementView GetElementView(IElementData data)
    {
        var pool = GetElementPool(data.PrefabPath);
        return pool.Get(Content);
    }

    private ScrollElementsPool GetElementPool(string prefabPath)
    {
        if (!elementPools.TryGetValue(prefabPath, out var pool))
        {
            elementPools[prefabPath] = pool = new ScrollElementsPool(prefabPath, transform);
        }

        return pool;
    }

    private void OnValidate()
    {
        if (!scrollRect)
        {
            scrollRect = GetComponent<ScrollRect>();
        }
    }

    private void CreateInitialElements(List<IElementData> elementsData)
    {
        for (var i = 0; i < elementsData.Count; i++)
        {
            var element = CreateNewFirstElement();
            if (!element.RectTransform.IsPartiallyOverlappedBy(Viewport))
            {
                break;
            }
        }
    }

    private void SetContentSize(IEnumerable<IElementData> itemsData)
    {
        var height = CalculateFullContentHeight(itemsData);
        Content.sizeDelta = new Vector2(Content.sizeDelta.x, height);
    }

    private float CalculateFullContentHeight(IEnumerable<IElementData> elementsData)
    {
        var height = 0f;
        foreach (var elementData in elementsData)
        {
            height += GetElementSize(elementData).y;
        }

        return height;
    }

    private void UpdateScrollItems(Vector2 contentPosition)
    {
        if (IsScrollEmpty())
        {
            return;
        }

        var contentDeltaPosition = GetContentDeltaPosition(contentPosition);
        switch (contentDeltaPosition.y)
        {
            case > 0:
                HandleMoveDown();
                break;
            case < 0:
                HandleMoveUp();
                break;
        }
    }

    private bool IsScrollEmpty() => activeElements.Count == 0 || itemsData.Count == 0;

    private Vector2 GetContentDeltaPosition(Vector2 contentPosition)
    {
        previousContentPosition ??= contentPosition;
        var contentDeltaPosition = contentPosition - previousContentPosition;
        previousContentPosition = contentPosition;
        return contentDeltaPosition.Value;
    }

    private void HandleMoveDown()
    {
        if (IsScrolledToTheStart())
        {
            return;
        }

        while (TryCreateNewTrailItem() & 
               TryRemoveHeadItem())
        {
        }
    }

    private void HandleMoveUp()
    {
        if (IsScrolledToTheEnd())
        {
            return;
        }

        while (TryCreateNewHeadItem() & 
               TryRemoveTrailItem())
        {
        }
    }

    private bool TryRemoveHeadItem()
    {
        if (!First.RectTransform.IsBelowOf(Viewport))
        {
            return false;
        }

        ReleaseElement(First);
        activeElements.RemoveFirst();
        return true;
    }

    private bool TryRemoveTrailItem()
    {
        if (!Last.RectTransform.IsAboveOf(Viewport))
        {
            return false;
        }

        ReleaseElement(Last);
        activeElements.RemoveLast();
        return true;
    }

    private bool TryCreateNewTrailItem()
    {
        if (IsScrolledToTheStart() || Last.RectTransform.IsAboveOf(Viewport))
        {
            return false;
        }

        CreateNewTrailElement();
        return true;
    }

    private bool TryCreateNewHeadItem()
    {
        if (IsScrolledToTheEnd() || First.RectTransform.IsBelowOf(Viewport))
        {
            return false;
        }

        CreateNewFirstElement();
        return true;
    }

    private void ReleaseElement(ElementView element)
    {
        var pool = GetElementPool(element.Data.PrefabPath);
        pool.Release(element);
    }

    private bool IsScrolledToTheEnd() => First.Index == itemsData.Count - 1;
    private bool IsScrolledToTheStart() => Last.Index == 0;

    private ElementView CreateFirstElement()
    {
        var elementData = itemsData[0];
        var startPosition = Content.rect.height / 2;
        var elementHeightHalf = GetElementSize(elementData).y / 2;
        var elementPositionY = startPosition - elementHeightHalf;

        var elementCenterPosition = new Vector2(0, elementPositionY);
        var element = CreateElement(elementData, elementCenterPosition, 0);
        activeElements.AddFirst(element);
        return element;
    }

    private ElementView CreateNewFirstElement()
    {
        if (activeElements.First == null)
        {
            return CreateFirstElement();
        }

        var elementData = itemsData[First.Index + 1];
        var firstElement = activeElements.First.Value;
        var firstElementPosition = firstElement.RectTransform.anchoredPosition.y;
        var firstElementSize = firstElement.Size.y;
        var newElementSize = GetElementSize(elementData).y;
        var newElementPosition = new Vector2(0, firstElementPosition -
                                                firstElementSize / 2 -
                                                newElementSize / 2);

        var newElementIndex = firstElement.Index + 1;
        var newElement = CreateElement(elementData, newElementPosition, newElementIndex);
        activeElements.AddFirst(newElement);
        return newElement;
    }

    private ElementView CreateNewTrailElement()
    {
        if (activeElements.Last == null)
        {
            return CreateFirstElement();
        }

        var elementData = itemsData[Last.Index - 1];
        var lastElement = activeElements.Last.Value;
        var lastElementPosition = lastElement.RectTransform.anchoredPosition.y;
        var lastElementSize = lastElement.Size.y;
        var newElementSize = GetElementSize(elementData).y;
        var newElementPosition = new Vector2(0, lastElementPosition +
                                                lastElementSize / 2 +
                                                newElementSize / 2);

        var newElementIndex = lastElement.Index - 1;
        var newElement = CreateElement(elementData, newElementPosition, newElementIndex);
        activeElements.AddLast(newElement);
        return newElement;
    }
}