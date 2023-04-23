using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class VerticalScroll : PoolableScroll
{
    public override void ScrollToItem(int itemIndex)
    {
        Content.anchoredPosition = new Vector2(0, ViewsData[itemIndex].Position.y - ViewportHeight * 0.5f);
    }

    protected override int FindClosestItemToCenter()
    {
        var index = -1;
        var closestDistance = float.MaxValue;
        var contentCenter = Content.anchoredPosition.y + ViewportHeight * 0.5f;
        for (var i = trailIndex; i <= headIndex; i++)
        {
            var distance = Mathf.Abs(ViewsData[i].Position.y - contentCenter);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                index = i;
            }
        }

        return index;
    }

    protected override bool IsMovingForward(in Vector2 contentDeltaPosition) => contentDeltaPosition.y > 0;

    protected override bool IsFastScrolling(in Vector2 deltaPosition) =>
        Mathf.Abs(deltaPosition.y) > ViewportHeight * 2;

    protected override void InitViewsData(IElementData[] dataElements, out Vector2 contentSize)
    {
        ViewsData = new ElementViewData [dataElements.Length];

        var contentHeight = 0f;
        for (var i = 0; i < dataElements.Length; i++)
        {
            var elementSize = GetElementSize(dataElements[i]);
            var elementPosition = new Vector2(0, contentHeight + elementSize.y * 0.5f);
            ViewsData[i] = new ElementViewData(elementPosition, elementSize);

            contentHeight += elementSize.y;
        }

        contentSize = new Vector2(Content.sizeDelta.x, contentHeight);
    }

    protected override bool IsOutOfViewportInForwardDirection(int itemIndex, in Vector2 contentAnchoredPosition) =>
        ViewsData[itemIndex].Min.y > contentAnchoredPosition.y + ViewportHeight; // IsBelowOfViewport

    protected override bool IsOutOfViewportInBackwardDirection(int itemIndex, in Vector2 contentAnchoredPosition) =>
        ViewsData[itemIndex].Max.y < contentAnchoredPosition.y; // IsAboveOfViewport

    protected override Vector2 CalculateItemPositionInContent(in int itemIndex) =>
        new(0, ContentRect.height * 0.5f - ViewsData[itemIndex].Position.y);

    protected override bool IsPartiallyVisibleInViewport(in int itemIndex, in Vector2 contentAnchoredPosition) =>
        !IsOutOfViewportInBackwardDirection(itemIndex, contentAnchoredPosition) &&
        !IsOutOfViewportInForwardDirection(itemIndex, contentAnchoredPosition);

    protected override int FindFirstItemVisibleInViewport(in Vector2 contentAnchoredPosition)
    {
        var startIndex = 0;
        var endIndex = ViewsData.Length - 1;
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

            var middleElement = ViewsData[middleIndex];
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