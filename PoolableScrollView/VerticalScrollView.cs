using IgorTime.PoolableScrollView.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace IgorTime.PoolableScrollView
{
    [RequireComponent(typeof(ScrollRect))]
    [AddComponentMenu(MenuConstants.ADD_COMPONENT_MENU_PATH + nameof(VerticalScrollView))]
    public class VerticalScrollView : BasePoolableScrollView
    {
        protected override Vector2 GetAnchoredPositionOfContentForItem(int itemIndex) =>
            new(0, ViewsData[itemIndex].Position.y - ViewportHeight * 0.5f);

        public override int FindClosestItemToCenter()
        {
            var index = -1;
            var closestDistance = float.MaxValue;
            var contentCenter = Content.anchoredPosition.y + ViewportHeight * 0.5f;
            for (var i = TrailIndex; i <= HeadIndex; i++)
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

        protected override void InitViewsData(IItemData[] dataElements, out Vector2 contentSize)
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
            ScrollUtils.IsBelowOfViewport(ViewsData[itemIndex], contentAnchoredPosition.y, ViewportHeight);

        protected override bool IsOutOfViewportInBackwardDirection(int itemIndex, in Vector2 contentAnchoredPosition) =>
            ScrollUtils.IsAboveOfViewport(ViewsData[itemIndex], contentAnchoredPosition.y);

        protected override Vector2 CalculateItemPositionInContent(in int itemIndex) =>
            new(0, ContentRect.height * 0.5f - ViewsData[itemIndex].Position.y);

        protected override bool IsPartiallyVisibleInViewport(in int itemIndex, in Vector2 contentAnchoredPosition) =>
            !IsOutOfViewportInBackwardDirection(itemIndex, contentAnchoredPosition) &&
            !IsOutOfViewportInForwardDirection(itemIndex, contentAnchoredPosition);

        protected override int FindFirstItemVisibleInViewport(in Vector2 contentAnchoredPosition) =>
            ScrollUtils.BinarySearchOfFirstItemVisibleInViewportVertical(
                ViewsData,
                contentAnchoredPosition.y,
                ViewportHeight);
        
        protected override void UpdateItemsRelativePosition()
        {
            var viewportPositionY = scrollRect.viewport.position.y;
            var viewportHalfHeight = scrollRect.viewport.rect.height * 0.5f;
            foreach (var activeElement in ActiveElements.Values)
            {
                var d = Mathf.Abs(activeElement.RectTransform.position.y - viewportPositionY);
                var t = Mathf.Clamp01(1f - d / viewportHalfHeight);
                activeElement.UpdateRelativePosition(t);
            }
        }
    }
}