using IgorTime.PoolableScrollView.ItemView;
using IgorTime.Samples.Sample1.ElementData;
using TMPro;
using UnityEngine;

namespace IgorTime.Samples.Sample1.ViewElements
{
    public class TextItemView : ItemViewTyped<TextData>
    {
        [Header("Item specific:")]
        [SerializeField]
        private TextMeshProUGUI indexField;

        [SerializeField]
        private TextMeshProUGUI messageField;

        [SerializeField]
        private CanvasGroup canvasGroup;

        public override void SetVisibility(bool isVisible)
        {
            canvasGroup.alpha = isVisible ? 1 : 0;
        }

        protected override void UpdateContent(TextData textData)
        {
            messageField.text = textData.Message;
            indexField.text = Index.ToString();
        }
    }
}