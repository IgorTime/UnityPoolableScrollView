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

    public static bool IsPartiallyOverlappedBy(
        this RectTransform target,
        RectTransform other) =>
        target.IsPartiallyOverlappedBy(other, out _, out _);

    public static Rect CreateRect(
        Vector3[] corners)
    {
        var min = new Vector2();
        var max = new Vector2();
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

        var size = max - min;
        return new Rect(min, size);
    }

    public static Rect GetWorldRect(this RectTransform rectTransform)
    {
        rectTransform.GetWorldCorners(cornersBuffer);
        return CreateRect(cornersBuffer);
    }
}