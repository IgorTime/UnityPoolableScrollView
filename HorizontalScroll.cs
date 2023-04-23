using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class HorizontalScroll : PoolableScroll
{
    protected override bool IsMovingForward(in Vector2 contentDeltaPosition) => contentDeltaPosition.x > 0;

    protected override bool IsFastScrolling(in Vector2 deltaPosition) =>
        Mathf.Abs(deltaPosition.x) > ViewportHeight * 2;

    protected override void InitViewsData(IElementData[] dataElements, out Vector2 contentSize)
    {
        ViewsData = new ElementViewData [dataElements.Length];

        var contentWidth = 0f;
        for (var i = 0; i < dataElements.Length; i++)
        {
            var elementSize = GetElementSize(dataElements[i]);
            var elementPosition = new Vector2(contentWidth + elementSize.y * 0.5f, 0);
            ViewsData[i] = new ElementViewData(elementPosition, elementSize);

            contentWidth += elementSize.x;
        }

        contentSize = new Vector2(contentWidth, ContentRect.height);
    }

    protected override bool IsOutOfViewportInForwardDirection(int itemIndex, in Vector2 anchoredPosition) =>
        ViewsData[itemIndex].Min.x > anchoredPosition.x + ViewportWidth; // IsOnTheRightOfViewport

    protected override bool IsOutOfViewportInBackwardDirection(int itemIndex, in Vector2 anchoredPosition) =>
        ViewsData[itemIndex].Max.x < anchoredPosition.x; // IsOnTheLeftOfViewport

    protected override Vector2 CalculateItemPositionInContent(in int itemIndex) =>
        new(ContentRect.width * 0.5f - ViewsData[itemIndex].Position.x, 0);

    protected override bool IsPartiallyVisibleInViewport(in int elementIndex, in Vector2 anchoredPosition) =>
        !IsOutOfViewportInBackwardDirection(elementIndex, anchoredPosition) &&
        !IsOutOfViewportInForwardDirection(elementIndex, anchoredPosition);

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
            if (middleElement.Position.x > contentAnchoredPosition.x)
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