using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class VerticalScroll : PoolableScroll
{
    protected override void CreateInitialElements(IElementData[] elementsData, in Vector2 anchoredPosition)
    {
        for (var i = 0; i < elementsData.Length; i++)
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

    protected override void SetContentSize(IElementData[] itemsData)
    {
        var height = CalculateFullContentHeightAndViewsData(itemsData);
        content.sizeDelta = new Vector2(content.sizeDelta.x, height);
    }

    protected override bool IsMovingForward(in Vector2 contentDeltaPosition) => contentDeltaPosition.y > 0;

    protected override bool IsFastScrolling(in Vector2 deltaPosition) =>
        Mathf.Abs(deltaPosition.y) > viewportHeight * 2;

    protected override void ReinitAllItems(in Vector2 contentAnchoredPosition)
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

    protected override void HandleMoveBackward(in Vector2 contentAnchoredPosition)
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

    protected override void HandleMoveForward(in Vector2 contentAnchoredPosition)
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