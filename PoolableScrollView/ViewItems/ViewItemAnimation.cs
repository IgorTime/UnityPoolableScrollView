using UnityEngine;

namespace IgorTime.PoolableScrollView
{
    [RequireComponent(typeof(ElementView))]
    public abstract class ViewItemAnimation : MonoBehaviour
    {
        [SerializeField]
        private ElementView elementView;

        private void OnEnable()
        {
            elementView.onRelativePositionChanged.AddListener(OnRelativePositionChanged);
        }

        private void OnDisable()
        {
            elementView.onRelativePositionChanged.RemoveListener(OnRelativePositionChanged);
        }

        protected abstract void Animate(float normalizedValue);

        private void OnRelativePositionChanged(float relativePosition)
        {
            Animate(relativePosition);
        }

        private void OnValidate()
        {
            if (!elementView)
            {
                elementView = GetComponent<ElementView>();
            }
        }
    }
}