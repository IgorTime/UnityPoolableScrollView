using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PoolableScroll))]
public abstract class PoolableScroll : MonoBehaviour
{
    [SerializeField]
    protected ScrollRect scrollRect;

    [SerializeField]
    protected int firstIndex;

    [SerializeField]
    protected int lastIndex;

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

    protected ElementView First => activeElements?.First?.Value;
    protected ElementView Last => activeElements?.Last?.Value;

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
        viewsData = new ElementViewData[itemsData.Length];
        SetContentSize(itemsData);

        contentRect = scrollRect.content.rect;

        CreateInitialElements(itemsData, content.anchoredPosition);
        previousContentPosition = content.anchoredPosition;
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
            HandleMoveForward(contentAnchoredPosition);
        }
        else
        {
            HandleMoveBackward(contentAnchoredPosition);
        }
    }

    private void HandleMoveBackward(in Vector2 contentAnchoredPosition)
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

    private void HandleMoveForward(in Vector2 contentAnchoredPosition)
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
    
    protected bool IsScrolledToTheEnd() => firstIndex == itemsData.Length - 1;
    protected bool IsScrolledToTheStart() => lastIndex == 0;

    protected abstract bool TryCreateNewTrailItem(in Vector2 contentAnchoredPosition);
    protected abstract bool TryRemoveTrailItem(in Vector2 contentAnchoredPosition);
    protected abstract bool TryRemoveHeadItem(in Vector2 contentAnchoredPosition);
    protected abstract bool TryCreateNewHeadItem(in Vector2 contentAnchoredPosition);
    protected abstract void SetContentSize(IElementData[] dataElements);
    protected abstract bool IsMovingForward(in Vector2 contentDeltaPosition);
    protected abstract bool IsFastScrolling(in Vector2 contentDeltaPosition);
    protected abstract void ReinitAllItems(in Vector2 contentAnchoredPosition);

    private Vector2 GetContentDeltaPosition(Vector2 contentPosition)
    {
        previousContentPosition ??= contentPosition;
        var contentDeltaPosition = contentPosition - previousContentPosition;
        previousContentPosition = contentPosition;
        return contentDeltaPosition.Value;
    }

    private bool IsScrollEmpty() => activeElements.Count == 0 || itemsData.Length == 0;
    
    protected abstract void CreateInitialElements(
        IElementData[] dataElements,
        in Vector2 contentAnchoredPosition);


    private void OnValidate()
    {
        if (!scrollRect)
        {
            scrollRect = GetComponent<ScrollRect>();
        }
    }
    
    protected Vector2 GetElementSize(IElementData data)
    {
        var prefab = PeekElementView(data);
        return prefab.Size;
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
    
    protected ElementView CreateElement(IElementData data, Vector2 position, int index)
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
    
    protected void ReleaseElement(ElementView element)
    {
        var pool = GetElementPool(element.Data.PrefabPath);
        pool.Release(element);
        activeItemsCount--;
    }
    
    protected void ReleaseAllItems()
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
}