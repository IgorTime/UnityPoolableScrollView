using UnityEngine;
using UnityEngine.Events;

namespace IgorTime.PoolableScrollView
{
    public abstract class ElementView : MonoBehaviour
    {
        [SerializeField]
        private int index;

        public UnityEvent<float> onRelativePositionChanged = new();

        public int Index
        {
            get => index;
            private set => index = value;
        }

        public IElementData Data { get; private set; }
        public RectTransform RectTransform => (RectTransform) transform;
        public Vector2 Size => RectTransform.rect.size;

        public void Initialize(IElementData data, int index)
        {
            Index = index;
            Data = data;
            UpdateContent(data);
        }

        public abstract void SetVisibility(bool isVisible);

        protected abstract void UpdateContent(IElementData data);

        internal void UpdateRelativePosition(float relativePosition)
        {
            onRelativePositionChanged.Invoke(relativePosition);
        }
    }
}