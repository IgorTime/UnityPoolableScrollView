using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IgorTime.PoolableScrollView
{
    [RequireComponent(typeof(ScrollRect))]
    [RequireComponent(typeof(ViewItemProvider))]
    public abstract class BasePoolableScrollView : MonoBehaviour
    {
        [SerializeField]
        protected ScrollRect scrollRect;

        [SerializeField]
        protected ViewItemProvider itemViewProvider;

        private readonly Dictionary<int, ElementView> activeElements = new();

        protected ElementViewData[] ViewsData;
        protected Rect ContentRect;
        protected RectTransform Content;
        protected float ViewportHeight;
        protected float ViewportWidth;

        protected int HeadIndex;
        protected int TrailIndex;

        private IElementData[] itemsData;
        private Vector2? previousContentPosition;
        private int activeItemsCount;
        private Coroutine scrollCoroutine;

        private ElementView Head
        {
            get
            {
                if (HeadIndex == -1)
                {
                    return null;
                }

                return activeElements.TryGetValue(HeadIndex, out var element)
                    ? element
                    : null;
            }
        }

        private ElementView Trail
        {
            get
            {
                if (TrailIndex == -1)
                {
                    return null;
                }

                return activeElements.TryGetValue(TrailIndex, out var element)
                    ? element
                    : null;
            }
        }

        private void Awake()
        {
            Content = scrollRect.content;

            var viewportRect = scrollRect.viewport.rect;
            ViewportHeight = viewportRect.height;
            ViewportWidth = viewportRect.width;
        }

        private void OnEnable()
        {
            scrollRect.onValueChanged.AddListener(UpdateScrollItems);
        }

        private void OnDisable()
        {
            scrollRect.onValueChanged.RemoveListener(UpdateScrollItems);
        }

        public void Initialize(IElementData[] itemsData)
        {
            this.itemsData = itemsData;
            InitViewsData(this.itemsData, out var contentSize);
            Content.sizeDelta = contentSize;
            ContentRect = scrollRect.content.rect;
            previousContentPosition = Content.anchoredPosition;
            CreateInitialElements(itemsData, Content.anchoredPosition);
        }

        public void ScrollToItem(int itemIndex)
        {
            scrollRect.velocity = Vector2.zero;
            Content.anchoredPosition = GetAnchoredPositionOfContentForItem(itemIndex);
        }

        public void ScrollToItem(int itemIndex, float duration, AnimationCurve easingCurve = null)
        {
            scrollRect.velocity = Vector2.zero;

            if (scrollCoroutine != null)
            {
                StopCoroutine(scrollCoroutine);
            }

            var targetPosition = GetAnchoredPositionOfContentForItem(itemIndex);
            scrollCoroutine = StartCoroutine(ScrollCoroutine(targetPosition, duration, easingCurve));
        }

        public void ScrollToNext()
        {
            var index = FindClosestItemToCenter();
            var nextIndex = Mathf.Clamp(index + 1, 0, ViewsData.Length - 1);
            ScrollToItem(nextIndex);
        }

        public void ScrollToPrevious()
        {
            var index = FindClosestItemToCenter();
            var nextIndex = Mathf.Clamp(index - 1, 0, ViewsData.Length - 1);
            ScrollToItem(nextIndex);
        }

        public void ScrollToNext(float duration, AnimationCurve easingCurve = null)
        {
            var index = FindClosestItemToCenter();
            var nextIndex = Mathf.Clamp(index + 1, 0, ViewsData.Length - 1);

            if (duration > 0)
            {
                ScrollToItem(nextIndex, duration, easingCurve);
            }
            else
            {
                ScrollToItem(nextIndex);
            }
        }

        public void ScrollToPrevious(float duration, AnimationCurve easingCurve = null)
        {
            var index = FindClosestItemToCenter();
            var previousIndex = Mathf.Clamp(index - 1, 0, ViewsData.Length - 1);

            if (duration > 0)
            {
                ScrollToItem(previousIndex, duration, easingCurve);
            }
            else
            {
                ScrollToItem(previousIndex);
            }
        }

        protected abstract void InitViewsData(IElementData[] dataElements, out Vector2 contentSize);
        protected abstract bool IsMovingForward(in Vector2 contentDeltaPosition);
        protected abstract bool IsFastScrolling(in Vector2 contentDeltaPosition);
        protected abstract Vector2 CalculateItemPositionInContent(in int itemIndex);
        protected abstract bool IsOutOfViewportInForwardDirection(int itemIndex, in Vector2 contentAnchoredPosition);
        protected abstract bool IsOutOfViewportInBackwardDirection(int itemIndex, in Vector2 contentAnchoredPosition);
        protected abstract bool IsPartiallyVisibleInViewport(in int itemIndex, in Vector2 contentAnchoredPosition);
        protected abstract int FindFirstItemVisibleInViewport(in Vector2 contentAnchoredPosition);
        protected abstract int FindClosestItemToCenter();
        protected abstract Vector2 GetAnchoredPositionOfContentForItem(int itemIndex);

        protected Vector2 GetElementSize(IElementData data)
        {
            var prefab = itemViewProvider.Peek(data);
            return prefab.Size;
        }

        private IEnumerator ScrollCoroutine(
            Vector2 targetPosition,
            float duration,
            AnimationCurve easingCurve = null)
        {
            var elapsedTime = 0f;
            var startPosition = Content.anchoredPosition;
            while (elapsedTime < duration)
            {
                var t = elapsedTime / duration;
                if (easingCurve != null)
                {
                    t = easingCurve.Evaluate(t);
                }

                Content.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            Content.anchoredPosition = targetPosition;
        }

        private bool IsScrolledToTheEnd() => HeadIndex == itemsData.Length - 1;
        private bool IsScrolledToTheStart() => TrailIndex == 0;

        private ElementView CreateElement(IElementData data, Vector2 position, int index)
        {
            var elementView = itemViewProvider.Provide(data);
            elementView.Initialize(data, index);
            elementView.RectTransform.anchoredPosition = position;
            activeItemsCount++;
            return elementView;
        }

        private void ReleaseFirstElement()
        {
            if (activeItemsCount > 0)
            {
                ReleaseElement(Head);
                activeElements.Remove(HeadIndex);
            }
            
            HeadIndex--;
        }

        private void ReleaseLastElement()
        {
            if (activeItemsCount > 0)
            {
                ReleaseElement(Trail);
                activeElements.Remove(TrailIndex);
            }

            TrailIndex++;
        }

        private void ReleaseAllItems()
        {
            foreach (var active in activeElements.Values)
            {
                ReleaseElement(active);
            }

            HeadIndex = 0;
            TrailIndex = 0;
            activeItemsCount = 0;
            activeElements.Clear();
        }

        private void CreateHeadItem(in int itemIndex)
        {
            var newElement = CreateElement(
                itemsData[itemIndex],
                CalculateItemPositionInContent(itemIndex),
                itemIndex);

            activeElements[itemIndex] = newElement;
        }

        private void CreateTrailItem(in int itemIndex)
        {
            var newElement = CreateElement(
                itemsData[itemIndex],
                CalculateItemPositionInContent(itemIndex),
                itemIndex);

            activeElements[itemIndex] = newElement;
        }

        private void UpdateScrollItems(Vector2 contentPosition)
        {
            if (IsScrollEmpty())
            {
                return;
            }

            var contentAnchoredPosition = Content.anchoredPosition;
            var contentDeltaPosition = GetContentDeltaPosition(contentAnchoredPosition);
            if (IsFastScrolling(contentDeltaPosition))
            {
                ReinitAllItems(contentAnchoredPosition);
                return;
            }

            if (IsMovingForward(contentDeltaPosition))
            {
                HandleMovementForward(contentAnchoredPosition);
            }
            else
            {
                HandleMovementBackward(contentAnchoredPosition);
            }
            
            UpdateItemsRelativePosition();
        }

        private void UpdateItemsRelativePosition()
        {
            var viewportPositionY = scrollRect.viewport.position.y;
            var viewportHalfHeight = scrollRect.viewport.rect.height * 0.5f;
            foreach (var activeElement in activeElements.Values)
            {
                var d = Mathf.Abs(activeElement.RectTransform.position.y - viewportPositionY);
                var t = Mathf.Clamp01(1f - d / viewportHalfHeight);
                activeElement.UpdateRelativePosition(t);
            }
        }

        private void HandleMovementBackward(in Vector2 contentAnchoredPosition)
        {
            if (IsScrolledToTheStart())
            {
                return;
            }

            while (TryRemoveHeadItem(contentAnchoredPosition))
            {
            }

            while (TryCreateNewTrailItem(contentAnchoredPosition))
            {
            }
        }

        private void ReinitAllItems(in Vector2 contentAnchoredPosition)
        {
            ReleaseAllItems();
            var index = FindFirstItemVisibleInViewport(contentAnchoredPosition);
            HeadIndex = index;
            TrailIndex = index;

            activeElements[index] = CreateElement(
                itemsData[index],
                CalculateItemPositionInContent(index),
                index);

            while (TryCreateNewTrailItem(contentAnchoredPosition))
            {
            }

            while (TryCreateNewHeadItem(contentAnchoredPosition))
            {
            }
        }

        private bool TryRemoveHeadItem(in Vector2 anchoredPosition)
        {
            if (!IsOutOfViewportInForwardDirection(HeadIndex, anchoredPosition))
            {
                return false;
            }

            ReleaseFirstElement();
            return true;
        }

        private bool TryRemoveTrailItem(in Vector2 anchoredPosition)
        {
            if (!IsOutOfViewportInBackwardDirection(TrailIndex, anchoredPosition))
            {
                return false;
            }

            ReleaseLastElement();
            return true;
        }

        private bool TryCreateNewTrailItem(in Vector2 anchoredPosition)
        {
            if (IsScrolledToTheStart() ||
                IsOutOfViewportInBackwardDirection(TrailIndex, anchoredPosition))
            {
                return false;
            }

            CreateNewTrailElement(anchoredPosition);
            return true;
        }

        private bool TryCreateNewHeadItem(in Vector2 anchoredPosition)
        {
            if (IsScrolledToTheEnd() ||
                IsOutOfViewportInForwardDirection(HeadIndex, anchoredPosition))
            {
                return false;
            }

            CreateNewHeadElement(anchoredPosition);
            return true;
        }

        private void HandleMovementForward(in Vector2 contentAnchoredPosition)
        {
            if (IsScrolledToTheEnd())
            {
                return;
            }

            while (TryRemoveTrailItem(contentAnchoredPosition))
            {
            }

            while (TryCreateNewHeadItem(contentAnchoredPosition))
            {
            }
        }

        private Vector2 GetContentDeltaPosition(Vector2 contentPosition)
        {
            previousContentPosition ??= contentPosition;
            var contentDeltaPosition = contentPosition - previousContentPosition;
            previousContentPosition = contentPosition;
            return contentDeltaPosition.Value;
        }

        private bool IsScrollEmpty() => activeElements.Count == 0 || itemsData.Length == 0;

        private void OnValidate()
        {
            if (!scrollRect)
            {
                scrollRect = GetComponent<ScrollRect>();
            }

            if (!itemViewProvider)
            {
                itemViewProvider = GetComponent<ViewItemProvider>();
            }
        }

        private void ReleaseElement(ElementView element)
        {
            itemViewProvider.Release(element);
            activeItemsCount--;
        }

        private void CreateNewHeadElement(in Vector2 anchoredPosition)
        {
            HeadIndex++;
            if (IsPartiallyVisibleInViewport(HeadIndex, anchoredPosition) ||
                IsOutOfViewportInForwardDirection(HeadIndex, anchoredPosition))
            {
                CreateHeadItem(HeadIndex);
            }
        }

        private void CreateNewTrailElement(in Vector2 anchoredPosition)
        {
            TrailIndex--;
            if (IsPartiallyVisibleInViewport(TrailIndex, anchoredPosition) ||
                IsOutOfViewportInBackwardDirection(TrailIndex, anchoredPosition))
            {
                CreateTrailItem(TrailIndex);
            }
        }

        private void CreateInitialElements(IElementData[] elementsData, in Vector2 anchoredPosition)
        {
            HeadIndex = -1;
            for (var i = 0; i < elementsData.Length; i++)
            {
                CreateNewHeadElement(anchoredPosition);
                if (!IsPartiallyVisibleInViewport(HeadIndex, anchoredPosition))
                {
                    break;
                }
            }
        }
    }
}