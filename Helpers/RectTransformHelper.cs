using UnityEngine;

public static class RectTransformHelper
{
    private static readonly Vector3[] targetCorners = new Vector3 [4];
    private static readonly Vector3[] otherCorners = new Vector3 [4];

    public static bool IsOverlappedBy(
        this RectTransform target,
        RectTransform other)
    {
        CalculateMinMaxOfRects(
            target,
            other,
            out var targetMin,
            out var targetMax,
            out var otherMin,
            out var otherMax);

        return otherMin.x <= targetMin.x &&
               otherMin.y <= targetMin.y &&
               otherMax.x >= targetMax.x &&
               otherMax.y >= targetMax.y;
    }

    public static bool IsPartiallyOverlappedBy(
        this RectTransform target,
        RectTransform other)
    {
        CalculateMinMaxOfRects(
            target,
            other,
            out var targetMin,
            out var targetMax,
            out var otherMin,
            out var otherMax);

        return (targetMin.x <= otherMin.x && targetMin.x >= otherMin.x &&
               targetMin.y <= otherMin.y && targetMin.y >= otherMin.y) ||
               (targetMax.x <= otherMax.x && targetMax.x >= otherMax.x &&
               targetMax.y <= otherMax.y && targetMax.y >= otherMax.y);
    }

    public static void GetMinMax2D(
        Vector3[] points,
        out Vector2 min,
        out Vector2 max)
    {
        min = new Vector2();
        max = new Vector2();
        for (var i = 0; i < points.Length; i++)
        {
            var corner = points[i];
            if (i == 0)
            {
                min = corner;
                max = corner;
            }
            else
            {
                min.x = Mathf.Min(min.x, corner.x);
                min.y = Mathf.Min(min.y, corner.y);
                max.x = Mathf.Max(max.x, corner.x);
                max.y = Mathf.Max(max.y, corner.y);
            }
        }
    }

    private static void CalculateMinMaxOfRects(
        RectTransform target,
        RectTransform other,
        out Vector2 targetMin,
        out Vector2 targetMax,
        out Vector2 otherMin,
        out Vector2 otherMax)
    {
        target.GetWorldCorners(targetCorners);
        other.GetWorldCorners(otherCorners);

        GetMinMax2D(targetCorners, out targetMin, out targetMax);
        GetMinMax2D(otherCorners, out otherMin, out otherMax);
    }
}