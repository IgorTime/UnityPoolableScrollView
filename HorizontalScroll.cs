using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class HorizontalScroll : PoolableScroll
{
    [SerializeField]
    [Range(0, 1)]
    private float normalizedPosition = 1;

    [SerializeField]
    private int firstIndex;

    [SerializeField]
    private int lastIndex;

    private readonly LinkedList<ElementView> activeElements = new();
    private readonly Dictionary<string, ScrollElementsPool> elementPools = new();
    private IElementData[] itemsData;
    private ElementViewData[] viewsData;
    private Vector2? previousContentPosition;
    private int activeItemsCount;
    private float viewportHeight;
    private Rect contentRect;
    private RectTransform content;
    private ElementView First => activeElements?.First?.Value;
    private ElementView Last => activeElements?.Last?.Value;

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
        content = scrollRect.content;
        SetContentSize(itemsData);

        contentRect = scrollRect.content.rect;
        viewportHeight = scrollRect.viewport.rect.height;

        CreateInitialElements(itemsData, content.anchoredPosition);
        previousContentPosition = content.anchoredPosition;
    }

    private bool IsAboveOfViewport(in int elementIndex, in Vector2 anchoredPosition) =>
        viewsData[elementIndex].Max.y < anchoredPosition.y;

    private bool IsBelowOfViewport(in int elementIndex, in Vector2 anchoredPosition) =>
        viewsData[elementIndex].Min.y > anchoredPosition.y + viewportHeight;

    private bool IsPartiallyVisibleInViewport(in int elementIndex, in Vector2 anchoredPosition) =>
        !IsAboveOfViewport(elementIndex, anchoredPosition) &&
        !IsBelowOfViewport(elementIndex, anchoredPosition);

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
        elementView.RectTransform.anchoredPosition = position;
        activeItemsCount++;
        return elementView;
    }

    private ElementView GetElementView(IElementData data)
    {
        var pool = GetElementPool(data.PrefabPath);
        return pool.Get();
    }

    private ScrollElementsPool GetElementPool(string prefabPath)
    {
        if (!elementPools.TryGetValue(prefabPath, out var pool))
        {
            elementPools[prefabPath] = pool = new ScrollElementsPool(prefabPath, content);
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

    private void CreateInitialElements(ICollection elementsData, in Vector2 anchoredPosition)
    {
        for (var i = 0; i < elementsData.Count; i++)
        {
            if (i == 0)
            {
                CreateVeryFirstElement();
                continue;
            }

            CreateNewFirstElement(anchoredPosition);
            if (IsBelowOfViewport(firstIndex, anchoredPosition))
            {
                break;
            }
        }
    }

    private void SetContentSize(IElementData[] itemsData)
    {
        var height = CalculateFullContentHeightAndViewsData(itemsData);
        content.sizeDelta = new Vector2(content.sizeDelta.x, height);
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

        var contentAnchoredPosition = content.anchoredPosition;
        var contentDeltaPosition = GetContentDeltaPosition(contentAnchoredPosition);
        if (IsFastVerticalScrolling(contentDeltaPosition))
        {
            ReinitAllItems(contentAnchoredPosition);
            return;
        }

        switch (contentDeltaPosition.y)
        {
            case < 0:
                HandleMoveDown(contentAnchoredPosition);
                break;
            case > 0:
                HandleMoveUp(contentAnchoredPosition);
                break;
        }
    }

    private bool IsFastVerticalScrolling(in Vector2 deltaPosition) => Mathf.Abs(deltaPosition.y) > viewportHeight * 2;

    private void ReinitAllItems(Vector2 contentAnchoredPosition)
    {
        ReleaseAllItems();
        var index = FindFirstItemVisibleInViewportVertical(contentAnchoredPosition);
        firstIndex = index;
        lastIndex = index;

        activeElements.AddFirst(CreateElement(
            itemsData[index],
            CalculateVerticalPositionInContent(index),
            index));

        while (TryCreateNewTrailItem(contentAnchoredPosition))
        {
        }

        while (TryCreateNewHeadItem(contentAnchoredPosition))
        {
        }
    }

    private void ReleaseAllItems()
    {
        while (activeElements.Count > 0)
        {
            ReleaseElement(activeElements.First.Value);
            activeElements.RemoveFirst();
        }

        firstIndex = 0;
        lastIndex = 0;
        activeItemsCount = 0;
    }

    private bool IsScrollEmpty() => activeElements.Count == 0 || itemsData.Length == 0;

    private Vector2 GetContentDeltaPosition(Vector2 contentPosition)
    {
        previousContentPosition ??= contentPosition;
        var contentDeltaPosition = contentPosition - previousContentPosition;
        previousContentPosition = contentPosition;
        return contentDeltaPosition.Value;
    }

    private void HandleMoveDown(Vector2 contentAnchoredPosition)
    {
        if (IsScrolledToTheStart())
        {
            return;
        }

        while (TryRemoveHeadItem(contentAnchoredPosition))
        {
        }

        while (TryCreateNewTrailItem(contentAnchoredPosition))
        {
        }
    }

    private void HandleMoveUp(Vector2 contentAnchoredPosition)
    {
        if (IsScrolledToTheEnd())
        {
            return;
        }

        while (TryRemoveTrailItem(contentAnchoredPosition))
        {
        }

        while (TryCreateNewHeadItem(contentAnchoredPosition))
        {
        }
    }

    private bool TryRemoveHeadItem(in Vector2 anchoredPosition)
    {
        if (!IsBelowOfViewport(firstIndex, anchoredPosition))
        {
            return false;
        }

        ReleaseFirstElement();
        return true;
    }

    private bool TryRemoveTrailItem(in Vector2 anchoredPosition)
    {
        if (!IsAboveOfViewport(lastIndex, anchoredPosition))
        {
            return false;
        }

        ReleaseLastElement();
        return true;
    }

    private void ReleaseFirstElement()
    {
        firstIndex--;

        if (activeItemsCount > 0)
        {
            ReleaseElement(First);
            activeElements.RemoveFirst();
        }
    }

    private void ReleaseLastElement()
    {
        lastIndex++;

        if (activeItemsCount > 0)
        {
            ReleaseElement(Last);
            activeElements.RemoveLast();
        }
    }

    private bool TryCreateNewTrailItem(in Vector2 anchoredPosition)
    {
        if (IsScrolledToTheStart() ||
            IsAboveOfViewport(lastIndex, anchoredPosition))
        {
            return false;
        }

        CreateNewTrailElement(anchoredPosition);
        return true;
    }

    private bool TryCreateNewHeadItem(in Vector2 anchoredPosition)
    {
        if (IsScrolledToTheEnd() ||
            IsBelowOfViewport(firstIndex, anchoredPosition))
        {
            return false;
        }

        CreateNewFirstElement(anchoredPosition);
        return true;
    }

    private void ReleaseElement(ElementView element)
    {
        var pool = GetElementPool(element.Data.PrefabPath);
        pool.Release(element);
        activeItemsCount--;
    }

    private bool IsScrolledToTheEnd() => firstIndex == itemsData.Length - 1;
    private bool IsScrolledToTheStart() => lastIndex == 0;

    private void CreateVeryFirstElement()
    {
        var elementData = itemsData[0];
        var startPosition = contentRect.height / 2;
        var elementHeightHalf = GetElementSize(elementData).y / 2;
        var elementPositionY = startPosition - elementHeightHalf;

        var elementCenterPosition = new Vector2(0, elementPositionY);
        var element = CreateElement(elementData, elementCenterPosition, 0);
        activeElements.AddFirst(element);
    }

    private void CreateNewFirstElement(in Vector2 anchoredPosition)
    {
        firstIndex++;
        if (IsPartiallyVisibleInViewport(firstIndex, anchoredPosition) ||
            IsBelowOfViewport(firstIndex, anchoredPosition))
        {
            var newElement = CreateElement(
                itemsData[firstIndex],
                CalculateVerticalPositionInContent(firstIndex),
                firstIndex);

            activeElements.AddFirst(newElement);
        }
    }

    private Vector2 CalculateVerticalPositionInContent(int itemIndex) =>
        new(0, contentRect.height * 0.5f - viewsData[itemIndex].Position.y);

    private void CreateNewTrailElement(in Vector2 anchoredPosition)
    {
        lastIndex--;
        if (IsPartiallyVisibleInViewport(lastIndex, anchoredPosition) ||
            IsAboveOfViewport(lastIndex, anchoredPosition))
        {
            var newElement = CreateElement(
                itemsData[lastIndex],
                CalculateVerticalPositionInContent(lastIndex),
                lastIndex);

            activeElements.AddLast(newElement);
        }
    }

    private int FindFirstItemVisibleInViewportVertical(in Vector2 contentAnchoredPosition)
    {
        var startIndex = 0;
        var endIndex = viewsData.Length - 1;
        while (true)
        {
            if (startIndex == endIndex)
            {
                return -1;
            }

            var middleIndex = startIndex + (endIndex - startIndex) / 2;
            if (IsPartiallyVisibleInViewport(middleIndex, contentAnchoredPosition))
            {
                return middleIndex;
            }

            var middleElement = viewsData[middleIndex];
            if (middleElement.Position.y > contentAnchoredPosition.y)
            {
                endIndex = middleIndex - 1;
            }
            else
            {
                startIndex = middleIndex + 1;
            }
        }
    }
}