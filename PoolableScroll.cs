using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class PoolableScroll : MonoBehaviour
{
    [SerializeField]
    private ScrollRect scrollRect;

    [SerializeField]
    [Range(0, 1)]
    private float normalizedPosition = 1;

    private readonly LinkedList<ElementView> activeElements = new();

    private readonly Dictionary<string, ScrollElementsPool> elementPools = new();

    private IElementData[] itemsData;
    private ElementViewData[] viewsData;

    private Vector2? previousContentPosition;
    private RectTransform Content => scrollRect.content;
    private RectTransform Viewport => scrollRect.viewport;
    private ElementView First => activeElements.First.Value;
    private ElementView Last => activeElements.Last.Value;

    [field: SerializeField]
    private int FirstIndex { get; set; }

    [field: SerializeField]
    private int LastIndex { get; set; }

    private float ViewportHeight => Viewport.rect.height;

    private void OnEnable()
    {
        scrollRect.onValueChanged.AddListener(UpdateScrollItems);
    }

    private void OnDisable()
    {
        scrollRect.onValueChanged.RemoveListener(UpdateScrollItems);
    }

    public void Initialize(IElementData[] itemsData)
    {
        this.itemsData = itemsData;
        viewsData = new ElementViewData[itemsData.Length];

        SetContentSize(itemsData);
        CreateInitialElements(itemsData);
        previousContentPosition = scrollRect.normalizedPosition;
    }

    public bool IsAboveOfViewport(in ElementViewData elementViewData) =>
        elementViewData.Max.y < Content.anchoredPosition.y;

    public bool IsBelowOfViewport(in ElementViewData elementViewData) =>
        elementViewData.Min.y > Content.anchoredPosition.y + Viewport.rect.height;

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

        var newPosition = new Vector2(scrollRect.normalizedPosition.x, normalizedPosition);
        scrollRect.normalizedPosition = newPosition;
    }

    private void CreateInitialElements(ICollection elementsData)
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

    private void SetContentSize(IElementData[] itemsData)
    {
        var height = CalculateFullContentHeightAndViewsData(itemsData);
        Content.sizeDelta = new Vector2(Content.sizeDelta.x, height);
    }

    private float CalculateFullContentHeightAndViewsData(IElementData[] elementsData)
    {
        var contentHeight = 0f;
        for (var i = 0; i < elementsData.Length; i++)
        {
            var elementSize = GetElementSize(elementsData[i]);
            var elementPosition = new float2(0, contentHeight + elementSize.y * 0.5f);
            viewsData[i] = new ElementViewData(elementPosition, elementSize);

            contentHeight += elementSize.y;
        }

        return contentHeight;
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
                HandleMoveUp(contentPosition);
                break;
        }
    }

    private bool IsScrollEmpty() => activeElements.Count == 0 || itemsData.Length == 0;

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

    private void HandleMoveUp(Vector2 contentNormalizedPosition)
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
        if (!IsBelowOfViewport(viewsData[FirstIndex]))
        {
            return false;
        }
        
        ReleaseFirstElement();
        activeElements.RemoveFirst();
        return true;
    }

    private bool TryRemoveTrailItem()
    {
        if (!IsAboveOfViewport(viewsData[LastIndex]))
        {
            return false;
        }

        ReleaseLastElement();
        activeElements.RemoveLast();
        return true;
    }

    private void ReleaseFirstElement()
    {
        FirstIndex--;
        ReleaseElement(First);
    }

    private void ReleaseLastElement()
    {
        LastIndex++;
        ReleaseElement(Last);
    }

    private bool TryCreateNewTrailItem()
    {
        if (IsScrolledToTheStart() || IsAboveOfViewport(viewsData[LastIndex]))
        {
            return false;
        }

        CreateNewTrailElement();
        return true;
    }

    private bool TryCreateNewHeadItem()
    {
        if (IsScrolledToTheEnd() ||
            IsBelowOfViewport(viewsData[FirstIndex]))
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

    private bool IsScrolledToTheEnd() => FirstIndex == itemsData.Length - 1;
    private bool IsScrolledToTheStart() => LastIndex == 0;

    private ElementView CreateVeryFirstElement()
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
            return CreateVeryFirstElement();
        }

        FirstIndex++;
        var elementData = itemsData[FirstIndex];
        var firstElement = activeElements.First.Value;
        var firstElementPosition = firstElement.RectTransform.anchoredPosition.y;
        var firstElementSize = firstElement.Size.y;
        var newElementSize = GetElementSize(elementData).y;
        var newElementPosition = new Vector2(0, firstElementPosition -
                                                firstElementSize / 2 -
                                                newElementSize / 2);

        var newElement = CreateElement(elementData, newElementPosition, FirstIndex);
        activeElements.AddFirst(newElement);
        return newElement;
    }

    private ElementView CreateNewTrailElement()
    {
        if (activeElements.Last == null)
        {
            return CreateVeryFirstElement();
        }

        LastIndex--;
        var elementData = itemsData[LastIndex];
        var lastElement = activeElements.Last.Value;
        var lastElementPosition = lastElement.RectTransform.anchoredPosition.y;
        var lastElementSize = lastElement.Size.y;
        var newElementSize = GetElementSize(elementData).y;
        var newElementPosition = new Vector2(0, lastElementPosition +
                                                lastElementSize / 2 +
                                                newElementSize / 2);

        var newElement = CreateElement(elementData, newElementPosition, LastIndex);
        activeElements.AddLast(newElement);
        return newElement;
    }
}

public struct ElementViewData
{
    private readonly Vector2 position;
    private readonly Vector2 size;

    public ElementViewData(Vector2 position, Vector2 size)
    {
        this.position = position;
        this.size = size;
        Min = position - size * 0.5f;
        Max = position + size * 0.5f;
    }

    public Vector2 Min { get; set; }
    public Vector2 Max { get; set; }
}