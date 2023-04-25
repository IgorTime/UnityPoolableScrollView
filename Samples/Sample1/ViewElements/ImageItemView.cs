using IgorTime.PoolableScrollView;
using IgorTime.Samples.Sample_1.ElementData;
using UnityEngine;
using UnityEngine.UI;

namespace IgorTime.Samples.Sample_1.ViewElements
{
    public class ImageItemView : ItemViewTyped<SpriteData>
    {
        [SerializeField]
        private Image image;

        protected override void UpdateContent(SpriteData spriteData)
        {
            image.sprite = spriteData.Sprite;
        }

        public override void SetVisibility(bool isVisible)
        {
            image.enabled = isVisible;
        }
    }
}