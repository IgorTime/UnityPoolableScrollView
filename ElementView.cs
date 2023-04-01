using UnityEngine;

public abstract class ElementView : MonoBehaviour
{
    [SerializeField]
    private int index;
    public RectTransform RectTransform => (RectTransform) transform;

    public int Index
    {
        get => index;
        private set => index = value;
    }
    
    public abstract Vector2 Size { get; }

    public void Initialize(IElementData data, int index)
    {
        Index = index;
        SetData(data);
    }

    protected abstract void SetData(IElementData data);
}