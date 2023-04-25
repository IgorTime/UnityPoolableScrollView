using UnityEngine;

namespace IgorTime.PoolableScrollView
{
    [RequireComponent(typeof(ItemView))]
    public abstract class ItemViewAnimation : MonoBehaviour
    {
        [SerializeField]
        private ItemView itemView;

        private void OnEnable()
        {
            itemView.onRelativePositionChanged.AddListener(OnRelativePositionChanged);
        }

        private void OnDisable()
        {
            itemView.onRelativePositionChanged.RemoveListener(OnRelativePositionChanged);
        }

        protected abstract void Animate(float normalizedValue);

        private void OnRelativePositionChanged(float relativePosition)
        {
            Animate(relativePosition);
        }

        private void OnValidate()
        {
            if (!itemView)
            {
                itemView = GetComponent<ItemView>();
            }
        }
    }
}