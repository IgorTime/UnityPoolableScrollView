using UnityEngine;

public static class RectTransformHelper
{
    private static readonly Vector3[] targetCorners = new Vector3 [4];
    private static readonly Vector3[] otherCorners = new Vector3 [4];

    public static bool IsOverlappedBy(
        this RectTransform target,
        RectTransform other)
    {
        target.GetWorldCorners(targetCorners);
        other.GetWorldCorners(otherCorners);

        GetMinMax2D(targetCorners, out var targetMin, out var targetMax);
        GetMinMax2D(otherCorners, out var otherMin, out var otherMax);

        return otherMin.x <= targetMin.x &&
               otherMin.y <= targetMin.y &&
               otherMax.x >= targetMax.x &&
               otherMax.y >= targetMax.y;
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
}