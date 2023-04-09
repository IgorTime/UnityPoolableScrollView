using UnityEngine;

public readonly struct ElementViewData
{
    public readonly Vector2 Min;
    public readonly Vector2 Max;
    public readonly Vector2 Position;

    public ElementViewData(Vector2 position, Vector2 size)
    {
        Position = position;
        Min = position - size * 0.5f;
        Max = position + size * 0.5f;
    }
}