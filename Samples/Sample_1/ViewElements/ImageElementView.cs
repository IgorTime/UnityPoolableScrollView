using UnityEngine;
using UnityEngine.UI;

public class ImageElementView : ElementView
{
    [SerializeField]
    private Image image;

    protected override void UpdateContent(IElementData data)
    {
        var spriteData = (SpriteData) data;
        image.sprite = spriteData.Sprite;
    }

    public override void SetVisibility(bool isVisible)
    {
        image.enabled = isVisible;
    }
}