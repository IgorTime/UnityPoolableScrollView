using Unity.Mathematics;

public struct ElementViewData
{
    public float2 Position;
    public float2 Size;
    public int Index;
    public float2 Min;
    public float2 Max;
    
    public ElementViewData(float2 position, float2 size, int index)
    {
        Position = position;
        Size = size;
        Index = index;
        Min = position - size * 0.5f;
        Max = position + size * 0.5f;
    }
}