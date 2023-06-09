﻿using IgorTime.PoolableScrollView.ItemView;

namespace IgorTime.PoolableScrollView.Helpers
{
    public static class ScrollUtils
    {
        public static bool IsBelowOfViewport(
            in ItemViewData viewData,
            in float contentAnchoredPositionY,
            in float viewportHeight) =>
            viewData.Min.y > contentAnchoredPositionY + viewportHeight;

        public static bool IsAboveOfViewport(
            in ItemViewData viewData,
            in float contentAnchoredPositionY) =>
            viewData.Max.y < contentAnchoredPositionY;

        public static bool IsOnTheRightOfViewport(
            in ItemViewData viewData,
            in float contentAnchoredPositionX,
            in float viewportHeight) =>
            viewData.Min.x > -contentAnchoredPositionX + viewportHeight;

        public static bool IsOnTheLeftOfViewport(
            in ItemViewData viewData,
            in float contentAnchoredPositionX) =>
            viewData.Max.x < -contentAnchoredPositionX;

        public static bool IsPartiallyVisibleInViewportVertical(
            in ItemViewData itemView,
            in float contentAnchoredPositionY,
            in float viewportHeight) =>
            !IsBelowOfViewport(itemView, contentAnchoredPositionY, viewportHeight) &&
            !IsAboveOfViewport(itemView, contentAnchoredPositionY);

        public static bool IsPartiallyVisibleInViewportHorizontal(
            in ItemViewData itemView,
            in float contentAnchoredPositionX,
            in float viewportWidth) =>
            !IsOnTheRightOfViewport(itemView, contentAnchoredPositionX, viewportWidth) &&
            !IsOnTheLeftOfViewport(itemView, contentAnchoredPositionX);

        public static int BinarySearchOfFirstItemVisibleInViewportVertical(
            ItemViewData[] viewsData,
            in float contentAnchoredPositionY,
            in float viewportHeight)
        {
            var startIndex = 0;
            var endIndex = viewsData.Length - 1;
            while (true)
            {
                if (startIndex == endIndex || endIndex < 0)
                {
                    return -1;
                }

                var middleIndex = startIndex + (endIndex - startIndex) / 2;
                if (IsPartiallyVisibleInViewportVertical(
                        viewsData[middleIndex],
                        contentAnchoredPositionY,
                        viewportHeight))
                {
                    return middleIndex;
                }

                var middleElement = viewsData[middleIndex];
                if (middleElement.Position.y > contentAnchoredPositionY)
                {
                    endIndex = middleIndex - 1;
                }
                else
                {
                    startIndex = middleIndex + 1;
                }
            }
        }

        public static int BinarySearchOfFirstItemVisibleInViewportHorizontal(
            ItemViewData[] viewsData,
            in float contentAnchoredPositionX,
            in float viewportWidth)
        {
            var startIndex = 0;
            var endIndex = viewsData.Length - 1;
            while (true)
            {
                if (startIndex == endIndex || endIndex < 0)
                {
                    return -1;
                }

                var middleIndex = startIndex + (endIndex - startIndex) / 2;
                if (IsPartiallyVisibleInViewportHorizontal(
                        viewsData[middleIndex],
                        contentAnchoredPositionX,
                        viewportWidth))
                {
                    return middleIndex;
                }

                var middleElement = viewsData[middleIndex];
                if (middleElement.Position.x > -contentAnchoredPositionX)
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
}