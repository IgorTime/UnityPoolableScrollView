using IgorTime.PoolableScrollView;
using IgorTime.Samples.Sample_1.ElementData;
using UnityEngine;
using UnityEngine.UI;

namespace IgorTime.Samples.Sample_1.ViewElements
{
    public class ImageItemView : ItemView
    {
        [SerializeField]
        private Image image;

        protected override void UpdateContent(IItemData data)
        {
            var spriteData = (SpriteData) data;
            image.sprite = spriteData.Sprite;
        }

        public override void SetVisibility(bool isVisible)
        {
            image.enabled = isVisible;
        }
    }
}