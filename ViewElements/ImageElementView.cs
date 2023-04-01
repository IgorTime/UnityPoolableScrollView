using UnityEngine;
using UnityEngine.UI;

namespace PoolableScroll.ViewElements
{
    public class ImageElementView : ElementView
    {
        [SerializeField]
        private Image image;

        public override Vector2 Size => GetComponent<RectTransform>().rect.size;

        public override void Initialize(IElementData data)
        {
            var spriteData = (SpriteData) data;
            image.sprite = spriteData.Sprite;
        }
    }
}