using UnityEngine;

public static class RectTransformHelper
{
    private static readonly Vector3[] cornersBuffer = new Vector3 [4];

    public static bool IsPartiallyOverlappedBy(
        this RectTransform target,
        RectTransform other)
    {
        var otherRect = other.GetWorldRect();
        target.GetWorldCorners(cornersBuffer);
        return otherRect.Contains(cornersBuffer[0]) ||
               otherRect.Contains(cornersBuffer[1]) ||
               otherRect.Contains(cornersBuffer[2]) ||
               otherRect.Contains(cornersBuffer[3]);
    }

    public static bool IsBelowOf(
        this RectTransform target,
        RectTransform other)
    {
        target.GetWorldMinMax(out _, out var targetMax);
        other.GetWorldMinMax(out var otherMin, out _);
        return targetMax.y < otherMin.y;
    }

    public static bool IsAboveOf(
        this RectTransform target,
        RectTransform other)
    {
        target.GetWorldMinMax(out var targetMin, out _);
        other.GetWorldMinMax(out _, out var otherMax);
        return targetMin.y > otherMax.y;
    }

    public static bool IsOnTheRightOf(
        this RectTransform target,
        RectTransform other)
    {
        target.GetWorldMinMax(out var targetMin, out _);
        other.GetWorldMinMax(out _, out var otherMax);
        return targetMin.x > otherMax.x;
    }

    public static bool IsOnTheLeftOf(
        this RectTransform target,
        RectTransform other)
    {
        target.GetWorldMinMax(out _, out var targetMax);
        other.GetWorldMinMax(out var otherMin, out _);
        return targetMax.x < otherMin.x;
    }

    public static Rect GetWorldRect(this RectTransform rectTransform)
    {
        rectTransform.GetWorldCorners(cornersBuffer);
        return CreateRect(cornersBuffer);
    }

    private static void GetWorldMinMax(this RectTransform rectTransform, out Vector2 min, out Vector2 max)
    {
        rectTransform.GetWorldCorners(cornersBuffer);
        GetMinMax2D(cornersBuffer, out min, out max);
    }

    private static Rect CreateRect(
        Vector3[] corners)
    {
        GetMinMax2D(corners, out var min, out var max);
        var size = max - min;
        return new Rect(min, size);
    }

    private static void GetMinMax2D(
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
}