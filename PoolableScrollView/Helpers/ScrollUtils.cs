namespace IgorTime.PoolableScrollView.Helpers
{
    public static class ScrollUtils
    {
        public static bool IsBelowOfViewport(
            in ElementViewData viewData,
            in float contentAnchoredPositionY,
            in float viewportHeight) =>
            viewData.Min.y > contentAnchoredPositionY + viewportHeight;

        public static bool IsAboveOfViewport(
            in ElementViewData viewData,
            in float contentAnchoredPositionY) =>
            viewData.Max.y < contentAnchoredPositionY;

        public static bool IsPartiallyVisibleInViewportVertical(
            in ElementViewData elementView,
            in float contentAnchoredPositionY,
            in float viewportHeight) =>
            !IsBelowOfViewport(elementView, contentAnchoredPositionY, viewportHeight) &&
            !IsAboveOfViewport(elementView, contentAnchoredPositionY);

        public static int BinarySearchOfFirstItemVisibleInViewportVertical(
            ElementViewData[] viewsData,
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
    }
}