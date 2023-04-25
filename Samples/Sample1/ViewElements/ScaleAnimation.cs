using IgorTime.PoolableScrollView.ItemView;
using UnityEngine;

namespace IgorTime.Samples.Sample1.ViewElements
{
    public class ScaleAnimation : ItemViewAnimation
    {
        [SerializeField]
        private Vector2 minScale = Vector2.zero;

        [SerializeField]
        private Vector2 maxScale = Vector2.one;

        protected override void Animate(float normalizedValue)
        {
            transform.localScale = Vector2.Lerp(minScale, maxScale, normalizedValue);
        }
    }
}