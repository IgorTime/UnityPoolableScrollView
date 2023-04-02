using UnityEngine;

public static class RectTransformHelper
{
    private static readonly Vector3[] cornersBuffer = new Vector3 [4];

    public static bool IsPartiallyOverlappedBy(
        this RectTransform target,
        RectTransform other,
        out Rect rectA,
        out Rect rectB)
    {
        rectA = target.GetWorldRect();
        rectB = other.GetWorldRect();
        target.GetWorldCorners(cornersBuffer);
        return rectB.Contains(cornersBuffer[0]) ||
               rectB.Contains(cornersBuffer[1]) ||
               rectB.Contains(cornersBuffer[2]) ||
               rectB.Contains(cornersBuffer[3]);
    }

    public static bool IsBelowOf(
        this RectTransform target,
        RectTransform other)
    {
        target.GetWorldCorners(cornersBuffer);
        GetMinMax2D(cornersBuffer, out _, out var targetMax);

        other.GetWorldCorners(cornersBuffer);
        GetMinMax2D(cornersBuffer, out var otherMin, out _);
        return targetMax.y < otherMin.y;
    }

    public static bool IsAboveOf(
        this RectTransform target,
        RectTransform other)
    {
        target.GetWorldCorners(cornersBuffer);
        GetMinMax2D(cornersBuffer, out var targetMin, out _);

        other.GetWorldCorners(cornersBuffer);
        GetMinMax2D(cornersBuffer, out _, out var otherMax);
        return targetMin.y > otherMax.y;
    }

    public static bool IsPartiallyOverlappedBy(
        this RectTransform target,
        RectTransform other) =>
        target.IsPartiallyOverlappedBy(other, out _, out _);

    public static Rect CreateRect(
        Vector3[] corners)
    {
        GetMinMax2D(corners, out var min, out var max);
        var size = max - min;
        return new Rect(min, size);
    }

    public static void GetMinMax2D(
        Vector3[] corners,
        out Vector2 min,
        out Vector2 max)
    {
        min = new Vector2();
        max = new Vector2();
        for (var i = 0; i < corners.Length; i++)
        {
            var corner = corners[i];
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

    public static Rect GetWorldRect(this RectTransform rectTransform)
    {
        rectTransform.GetWorldCorners(cornersBuffer);
        return CreateRect(cornersBuffer);
    }
}