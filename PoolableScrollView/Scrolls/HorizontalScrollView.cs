using IgorTime.PoolableScrollView.DataItems;
using IgorTime.PoolableScrollView.Helpers;
using IgorTime.PoolableScrollView.ItemView;
using UnityEngine;
using UnityEngine.UI;

namespace IgorTime.PoolableScrollView.Scrolls
{
    [RequireComponent(typeof(ScrollRect))]
    [AddComponentMenu(MenuConstants.ADD_COMPONENT_MENU_PATH + nameof(HorizontalScrollView))]
    public class HorizontalScrollView : BasePoolableScrollView
    {
        public override int FindClosestItemToCenter()
        {
            var index = -1;
            var closestDistance = float.MaxValue;
            var contentCenter = -Content.anchoredPosition.x + ViewportWidth * 0.5f;
            for (var i = TrailIndex; i <= HeadIndex; i++)
            {
                var distance = Mathf.Abs(ViewsData[i].Position.x - contentCenter);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    index = i;
                }
            }

            return index;
        }

        protected override Vector2 GetAnchoredPositionOfContentForItem(int itemIndex) =>
            new(-ViewsData[itemIndex].Position.x + ViewportWidth * 0.5f, 0);

        protected override bool IsMovingForward(in Vector2 contentDeltaPosition) => contentDeltaPosition.x < 0;

        protected override bool IsFastScrolling(in Vector2 deltaPosition) =>
            Mathf.Abs(deltaPosition.x) > ViewportWidth * 2;

        protected override void InitViewsData(IItemData[] dataElements, out Vector2 contentSize)
        {
            ViewsData = new ItemViewData [dataElements.Length];

            var contentWidth = 0f;
            for (var i = 0; i < dataElements.Length; i++)
            {
                var elementSize = GetElementSize(dataElements[i]);
                var elementPosition = new Vector2(contentWidth + elementSize.x * 0.5f, 0);
                ViewsData[i] = new ItemViewData(elementPosition, elementSize);

                contentWidth += elementSize.x;
            }

            contentSize = new Vector2(contentWidth, Content.sizeDelta.y);
        }

        protected override bool IsOutOfViewportInForwardDirection(int itemIndex, in Vector2 contentAnchoredPosition) =>
            ScrollUtils.IsOnTheRightOfViewport(ViewsData[itemIndex], contentAnchoredPosition.x, ViewportWidth);

        protected override bool IsOutOfViewportInBackwardDirection(int itemIndex, in Vector2 contentAnchoredPosition) =>
            ScrollUtils.IsOnTheLeftOfViewport(ViewsData[itemIndex], contentAnchoredPosition.x);

        protected override Vector2 CalculateItemPositionInContent(in int itemIndex) =>
            new(-ContentRect.width * 0.5f + ViewsData[itemIndex].Position.x, 0);

        protected override bool IsPartiallyVisibleInViewport(in int itemIndex, in Vector2 contentAnchoredPosition) =>
            ScrollUtils.IsPartiallyVisibleInViewportHorizontal(
                ViewsData[itemIndex],
                contentAnchoredPosition.x,
                ViewportWidth);

        protected override int FindFirstItemVisibleInViewport(in Vector2 contentAnchoredPosition) =>
            ScrollUtils.BinarySearchOfFirstItemVisibleInViewportHorizontal(
                ViewsData,
                contentAnchoredPosition.x,
                ViewportWidth);

        protected override void UpdateItemsRelativePosition()
        {
            var viewportPositionX = scrollRect.viewport.position.x;
            var viewportHalfHeight = scrollRect.viewport.rect.width * 0.5f;
            foreach (var activeElement in ActiveElements.Values)
            {
                var d = Mathf.Abs(activeElement.RectTransform.position.x - viewportPositionX);
                var t = Mathf.Clamp01(1f - d / viewportHalfHeight);
                activeElement.UpdateRelativePosition(t);
            }
        }
    }
}