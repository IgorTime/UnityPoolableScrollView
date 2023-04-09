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

    public bool IsAboveOfViewport(in int elementIndex) => viewsData[elementIndex].Max.y < Content.anchoredPosition.y;

    public bool IsBelowOfViewport(in int elementIndex) =>
        viewsData[elementIndex].Min.y > Content.anchoredPosition.y + Viewport.rect.height;

    public bool IsPartiallyVisibleInViewport(in int elementIndex) =>
        !IsAboveOfViewport(elementIndex) &&
        !IsBelowOfViewport(elementIndex);

    public bool IsFullyVisibleInViewport(in int elementIndex)
    {
        var anchoredPosition = Content.anchoredPosition;
        return viewsData[elementIndex].Min.y >= anchoredPosition.y &&
               viewsData[elementIndex].Max.y <= anchoredPosition.y + Viewport.rect.height;
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
        return pool.Get();
    }

    private ScrollElementsPool GetElementPool(string prefabPath)
    {
        if (!elementPools.TryGetValue(prefabPath, out var pool))
        {
            elementPools[prefabPath] = pool = new ScrollElementsPool(prefabPath, Content);
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
            CreateNewFirstElement();
            if (IsBelowOfViewport(FirstIndex))
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
                HandleMoveUp();
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

        while (TryCreateNewTrailItem() |
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

        while (TryCreateNewHeadItem() |
               TryRemoveTrailItem())
        {
        }
    }

    private bool TryRemoveHeadItem()
    {
        if (!IsBelowOfViewport(FirstIndex))
        {
            return false;
        }

        ReleaseFirstElement();
        return true;
    }

    private bool TryRemoveTrailItem()
    {
        if (!IsAboveOfViewport(LastIndex))
        {
            return false;
        }

        ReleaseLastElement();
        return true;
    }

    private void ReleaseFirstElement()
    {
        FirstIndex--;
        ReleaseElement(First);
        activeElements.RemoveFirst();
    }

    private void ReleaseLastElement()
    {
        LastIndex++;
        ReleaseElement(Last);
        activeElements.RemoveLast();
    }

    private bool TryCreateNewTrailItem()
    {
        if (IsScrolledToTheStart() || IsAboveOfViewport(LastIndex))
        {
            return false;
        }

        CreateNewTrailElement();
        return true;
    }

    private bool TryCreateNewHeadItem()
    {
        if (IsScrolledToTheEnd() ||
            IsBelowOfViewport(FirstIndex))
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

    private void CreateVeryFirstElement()
    {
        var elementData = itemsData[0];
        var startPosition = Content.rect.height / 2;
        var elementHeightHalf = GetElementSize(elementData).y / 2;
        var elementPositionY = startPosition - elementHeightHalf;

        var elementCenterPosition = new Vector2(0, elementPositionY);
        var element = CreateElement(elementData, elementCenterPosition, 0);
        activeElements.AddFirst(element);
    }

    private void CreateNewFirstElement()
    {
        if (activeElements.First == null)
        {
            CreateVeryFirstElement();
            return;
        }

        FirstIndex++;
        var newElement = CreateElement(
            itemsData[FirstIndex],
            CalculateVerticalPositionInContent(FirstIndex),
            FirstIndex);

        activeElements.AddFirst(newElement);
    }

    private Vector2 CalculateVerticalPositionInContent(int itemIndex) =>
        new(0, Content.rect.height * 0.5f - viewsData[itemIndex].Position.y);

    private void CreateNewTrailElement()
    {
        if (activeElements.Last == null)
        {
            CreateVeryFirstElement();
            return;
        }

        LastIndex--;
        var newElement = CreateElement(
            itemsData[LastIndex],
            CalculateVerticalPositionInContent(LastIndex),
            LastIndex);

        activeElements.AddLast(newElement);
    }
}

public readonly struct ElementViewData
{
    public Vector2 Min { get; }
    public Vector2 Max { get; }
    public Vector2 Position { get; }

    public ElementViewData(Vector2 position, Vector2 size)
    {
        Position = position;
        Min = position - size * 0.5f;
        Max = position + size * 0.5f;
    }
}