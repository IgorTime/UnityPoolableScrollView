﻿using IgorTime.PoolableScrollView;
using IgorTime.Samples.Sample_1.ElementData;
using TMPro;
using UnityEngine;

namespace IgorTime.Samples.Sample_1.ViewElements
{
    public class TextItemView : ItemViewTyped<TextData>
    {
        [SerializeField]
        private TextMeshProUGUI text;
    
        [SerializeField]
        private CanvasGroup canvasGroup;

        protected override void UpdateContent(TextData textData)
        {
            text.text = textData.Text;
        }

        public override void SetVisibility(bool isVisible)
        {
            canvasGroup.alpha = isVisible ? 1 : 0;
        }
    }
}