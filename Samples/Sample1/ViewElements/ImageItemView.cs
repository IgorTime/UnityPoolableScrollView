using IgorTime.PoolableScrollView;
using IgorTime.Samples.Sample_1.ElementData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IgorTime.Samples.Sample_1.ViewElements
{
    public class ImageItemView : ItemViewTyped<SpriteData>
    {
        [Header("Item specific:")]
        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private TextMeshProUGUI indexField;

        [SerializeField]
        private Image image;

        public override void SetVisibility(bool isVisible)
        {
            canvasGroup.alpha = isVisible ? 1 : 0;
        }

        protected override void UpdateContent(SpriteData spriteData)
        {
            image.sprite = spriteData.Sprite;
            indexField.text = Index.ToString();
        }
    }
}