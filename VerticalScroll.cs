using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class VerticalScroll : PoolableScroll
{
    protected override bool IsMovingForward(in Vector2 contentDeltaPosition) => contentDeltaPosition.y > 0;

    protected override bool IsFastScrolling(in Vector2 deltaPosition) =>
        Mathf.Abs(deltaPosition.y) > viewportHeight * 2;

    protected override void InitViewsData(IElementData[] dataElements, out Vector2 contentSize)
    {
        viewsData = new ElementViewData [dataElements.Length];

        var contentHeight = 0f;
        for (var i = 0; i < dataElements.Length; i++)
        {
            var elementSize = GetElementSize(dataElements[i]);
            var elementPosition = new Vector2(0, contentHeight + elementSize.y * 0.5f);
            viewsData[i] = new ElementViewData(elementPosition, elementSize);

            contentHeight += elementSize.y;
        }

        contentSize = new Vector2(contentRect.width, contentHeight);
    }

    protected override bool IsOutOfViewportInForwardDirection(int itemIndex, in Vector2 anchoredPosition) =>
        viewsData[itemIndex].Min.y > anchoredPosition.y + viewportHeight; // IsBelowOfViewport

    protected override bool IsOutOfViewportInBackwardDirection(int itemIndex, in Vector2 anchoredPosition) =>
        viewsData[itemIndex].Max.y < anchoredPosition.y; // IsAboveOfViewport

    protected override Vector2 CalculateItemPositionInContent(in int itemIndex) =>
        new(0, contentRect.height * 0.5f - viewsData[itemIndex].Position.y);

    protected override bool IsPartiallyVisibleInViewport(in int elementIndex, in Vector2 anchoredPosition) =>
        !IsOutOfViewportInBackwardDirection(elementIndex, anchoredPosition) &&
        !IsOutOfViewportInForwardDirection(elementIndex, anchoredPosition);

    protected override int FindFirstItemVisibleInViewport(in Vector2 contentAnchoredPosition)
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