using UnityEngine;

public abstract class ElementView : MonoBehaviour
{
    [SerializeField]
    private int index;
    public RectTransform RectTransform => (RectTransform) transform;
    public IElementData Data { get; private set; }

    public int Index
    {
        get => index;
        private set => index = value;
    }
    
    public abstract Vector2 Size { get; }

    public void Initialize(IElementData data, int index)
    {
        Index = index;
        Data = data;
        SetData(data);
    }

    protected abstract void SetData(IElementData data);
}