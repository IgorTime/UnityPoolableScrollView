using UnityEngine;

public abstract class ElementView : MonoBehaviour
{
    public RectTransform RectTransform => (RectTransform) transform;
    public abstract Vector2 Size { get; }

    public abstract void Initialize(IElementData data);
}