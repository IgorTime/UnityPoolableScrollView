using UnityEngine;

public abstract class ElementView : MonoBehaviour
{
    [SerializeField]
    private int index;

    public RectTransform RectTransform => (RectTransform) transform;
    public Vector2 Size => RectTransform.rect.size;
    public IElementData Data { get; private set; }

    public int Index
    {
        get => index;
        private set => index = value;
    }

    public void Initialize(IElementData data, int index)
    {
        Index = index;
        Data = data;
        UpdateContent(data);
    }

    protected abstract void UpdateContent(IElementData data);
}