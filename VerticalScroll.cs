using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class VerticalScroll : PoolableScroll
{
    protected override void CreateInitialElements(IElementData[] elementsData, in Vector2 anchoredPosition)
    {
        headIndex = -1;
        for (var i = 0; i < elementsData.Length; i++)
        {
            CreateNewHeadElement(anchoredPosition);
            if (IsBelowOfViewport(headIndex, anchoredPosition))
            {
                break;
            }
        }
    }

    protected override void SetContentSize(IElementData[] itemsData)
    {
        var height = CalculateFullContentHeightAndViewsData(itemsData);
        content.sizeDelta = new Vector2(content.sizeDelta.x, height);
    }

    protected override bool IsMovingForward(in Vector2 contentDeltaPosition) => 
        contentDeltaPosition.y > 0;

    protected override bool IsFastScrolling(in Vector2 deltaPosition) =>
        Mathf.Abs(deltaPosition.y) > viewportHeight * 2;

    protected override void ReinitAllItems(in Vector2 contentAnchoredPosition)
    {
        ReleaseAllItems();
        var index = FindFirstItemVisibleInViewportVertical(contentAnchoredPosition);
        headIndex = index;
        trailIndex = index;

        activeElements.AddFirst(CreateElement(
            itemsData[index],
            CalculateItemPositionInContent(index),
            index));

        while (TryCreateNewTrailItem(contentAnchoredPosition))
        {
        }

        while (TryCreateNewHeadItem(contentAnchoredPosition))
        {
        }
    }

    protected override bool TryRemoveHeadItem(in Vector2 anchoredPosition)
    {
        if (!IsBelowOfViewport(headIndex, anchoredPosition))
        {
            return false;
        }

        ReleaseFirstElement();
        return true;
    }

    protected override bool TryRemoveTrailItem(in Vector2 anchoredPosition)
    {
        if (!IsAboveOfViewport(trailIndex, anchoredPosition))
        {
            return false;
        }

        ReleaseLastElement();
        return true;
    }

    protected override bool TryCreateNewTrailItem(in Vector2 anchoredPosition)
    {
        if (IsScrolledToTheStart() ||
            IsAboveOfViewport(trailIndex, anchoredPosition))
        {
            return false;
        }

        CreateNewTrailElement(anchoredPosition);
        return true;
    }

    protected override bool TryCreateNewHeadItem(in Vector2 anchoredPosition)
    {
        if (IsScrolledToTheEnd() ||
            IsBelowOfViewport(headIndex, anchoredPosition))
        {
            return false;
        }

        CreateNewHeadElement(anchoredPosition);
        return true;
    }

    private bool IsAboveOfViewport(in int elementIndex, in Vector2 anchoredPosition) =>
        viewsData[elementIndex].Max.y < anchoredPosition.y;

    private bool IsBelowOfViewport(in int elementIndex, in Vector2 anchoredPosition) =>
        viewsData[elementIndex].Min.y > anchoredPosition.y + viewportHeight;

    private bool IsPartiallyVisibleInViewport(in int elementIndex, in Vector2 anchoredPosition) =>
        !IsAboveOfViewport(elementIndex, anchoredPosition) &&
        !IsBelowOfViewport(elementIndex, anchoredPosition);

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

    private void CreateNewHeadElement(in Vector2 anchoredPosition)
    {
        headIndex++;
        if (IsPartiallyVisibleInViewport(headIndex, anchoredPosition) ||
            IsBelowOfViewport(headIndex, anchoredPosition))
        {
            CreateHeadItem(headIndex);
        }
    }

    protected override Vector2 CalculateItemPositionInContent(in int itemIndex) =>
        new(0, contentRect.height * 0.5f - viewsData[itemIndex].Position.y);

    private void CreateNewTrailElement(in Vector2 anchoredPosition)
    {
        trailIndex--;
        if (IsPartiallyVisibleInViewport(trailIndex, anchoredPosition) ||
            IsAboveOfViewport(trailIndex, anchoredPosition))
        {
            CreateTrailItem(trailIndex);
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