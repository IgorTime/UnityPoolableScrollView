﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PoolableScroll))]
public abstract class PoolableScroll : MonoBehaviour
{
    [SerializeField]
    protected ScrollRect scrollRect;

    [SerializeField]
    protected int headIndex;

    [SerializeField]
    protected int trailIndex;

    protected readonly LinkedList<ElementView> activeElements = new();
    protected readonly Dictionary<string, ScrollElementsPool> elementPools = new();
    protected IElementData[] itemsData;
    protected ElementViewData[] viewsData;
    protected Vector2? previousContentPosition;
    protected int activeItemsCount;
    protected Rect contentRect;
    protected RectTransform content;
    protected float viewportHeight;
    protected float viewportWidth;

    private ElementView First => activeElements?.First?.Value;
    private ElementView Last => activeElements?.Last?.Value;

    private void Awake()
    {
        content = scrollRect.content;

        var viewportRect = scrollRect.viewport.rect;
        viewportHeight = viewportRect.height;
        viewportWidth = viewportRect.width;
    }

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
        InitViewsData(this.itemsData, out var contentSize);
        content.sizeDelta = contentSize;
        contentRect = scrollRect.content.rect;
        previousContentPosition = content.anchoredPosition;
        CreateInitialElements(itemsData, content.anchoredPosition);
    }

    protected bool IsScrolledToTheEnd() => headIndex == itemsData.Length - 1;
    protected bool IsScrolledToTheStart() => trailIndex == 0;

    protected abstract void InitViewsData(IElementData[] dataElements, out Vector2 contentSize);
    protected abstract bool TryCreateNewTrailItem(in Vector2 contentAnchoredPosition);
    protected abstract bool TryCreateNewHeadItem(in Vector2 contentAnchoredPosition);
    protected abstract bool IsMovingForward(in Vector2 contentDeltaPosition);
    protected abstract bool IsFastScrolling(in Vector2 contentDeltaPosition);
    protected abstract void ReinitAllItems(in Vector2 contentAnchoredPosition);
    protected abstract Vector2 CalculateItemPositionInContent(in int itemIndex);
    protected abstract bool IsOutOfViewportInForwardDirection(int itemIndex, in Vector2 anchoredPosition);
    protected abstract bool IsOutOfViewportInBackwardDirection(int itemIndex, in Vector2 anchoredPosition);


    protected abstract void CreateInitialElements(
        IElementData[] dataElements,
        in Vector2 contentAnchoredPosition);

    protected Vector2 GetElementSize(IElementData data)
    {
        var prefab = PeekElementView(data);
        return prefab.Size;
    }

    protected ElementView CreateElement(IElementData data, Vector2 position, int index)
    {
        var elementView = GetElementView(data);
        elementView.Initialize(data, index);
        elementView.RectTransform.anchoredPosition = position;
        activeItemsCount++;
        return elementView;
    }

    protected void ReleaseFirstElement()
    {
        headIndex--;

        if (activeItemsCount > 0)
        {
            ReleaseElement(First);
            activeElements.RemoveFirst();
        }
    }

    protected void ReleaseLastElement()
    {
        trailIndex++;

        if (activeItemsCount > 0)
        {
            ReleaseElement(Last);
            activeElements.RemoveLast();
        }
    }

    protected void ReleaseAllItems()
    {
        while (activeElements.Count > 0)
        {
            ReleaseElement(activeElements.First.Value);
            activeElements.RemoveFirst();
        }

        headIndex = 0;
        trailIndex = 0;
        activeItemsCount = 0;
    }

    protected void CreateHeadItem(in int itemIndex)
    {
        var newElement = CreateElement(
            itemsData[itemIndex],
            CalculateItemPositionInContent(itemIndex),
            itemIndex);

        activeElements.AddFirst(newElement);
    }

    protected void CreateTrailItem(in int itemIndex)
    {
        var newElement = CreateElement(
            itemsData[itemIndex],
            CalculateItemPositionInContent(itemIndex),
            itemIndex);

        activeElements.AddLast(newElement);
    }

    private void UpdateScrollItems(Vector2 contentPosition)
    {
        if (IsScrollEmpty())
        {
            return;
        }

        var contentAnchoredPosition = content.anchoredPosition;
        var contentDeltaPosition = GetContentDeltaPosition(contentAnchoredPosition);
        if (IsFastScrolling(contentDeltaPosition))
        {
            ReinitAllItems(contentAnchoredPosition);
            return;
        }

        if (IsMovingForward(contentDeltaPosition))
        {
            HandleMovementForward(contentAnchoredPosition);
        }
        else
        {
            HandleMovementBackward(contentAnchoredPosition);
        }
    }

    private void HandleMovementBackward(in Vector2 contentAnchoredPosition)
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
    
    private bool TryRemoveHeadItem(in Vector2 anchoredPosition)
    {
        if (!IsOutOfViewportInForwardDirection(headIndex, anchoredPosition))
        {
            return false;
        }

        ReleaseFirstElement();
        return true;
    }
    
    private bool TryRemoveTrailItem(in Vector2 anchoredPosition)
    {
        if (!IsOutOfViewportInBackwardDirection(trailIndex, anchoredPosition))
        {
            return false;
        }

        ReleaseLastElement();
        return true;
    }

    
    private void HandleMovementForward(in Vector2 contentAnchoredPosition)
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

    private Vector2 GetContentDeltaPosition(Vector2 contentPosition)
    {
        previousContentPosition ??= contentPosition;
        var contentDeltaPosition = contentPosition - previousContentPosition;
        previousContentPosition = contentPosition;
        return contentDeltaPosition.Value;
    }

    private bool IsScrollEmpty() => activeElements.Count == 0 || itemsData.Length == 0;

    private void OnValidate()
    {
        if (!scrollRect)
        {
            scrollRect = GetComponent<ScrollRect>();
        }
    }

    private ElementView PeekElementView(IElementData data)
    {
        var pool = GetElementPool(data.PrefabPath);
        return pool.Peek();
    }

    private ScrollElementsPool GetElementPool(string prefabPath)
    {
        if (!elementPools.TryGetValue(prefabPath, out var pool))
        {
            elementPools[prefabPath] = pool = new ScrollElementsPool(prefabPath, content);
        }

        return pool;
    }

    private ElementView GetElementView(IElementData data)
    {
        var pool = GetElementPool(data.PrefabPath);
        return pool.Get();
    }

    private void ReleaseElement(ElementView element)
    {
        var pool = GetElementPool(element.Data.PrefabPath);
        pool.Release(element);
        activeItemsCount--;
    }
}