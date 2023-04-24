using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IgorTime.PoolableScrollView
{
    [RequireComponent(typeof(ScrollRect))]
    public abstract class PoolableScroll : MonoBehaviour
    {
        [SerializeField]
        protected ScrollRect scrollRect;

        private readonly LinkedList<ElementView> activeElements = new();
        private readonly Dictionary<string, ScrollElementsPool> elementPools = new();

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

        private ElementView First => activeElements?.First?.Value;
        private ElementView Last => activeElements?.Last?.Value;

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
            Content.anchoredPosition = GetAnchoredPositionOfContentForItem(itemIndex);
        }

        public void ScrollToItem(int itemIndex, float duration, AnimationCurve easingCurve = null)
        {
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
            ScrollToItem(nextIndex, duration, easingCurve);
        }

        public void ScrollToPrevious(float duration, AnimationCurve easingCurve = null)
        {
            var index = FindClosestItemToCenter();
            var nextIndex = Mathf.Clamp(index - 1, 0, ViewsData.Length - 1);
            ScrollToItem(nextIndex, duration, easingCurve);
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
            var prefab = PeekElementView(data);
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
                if(easingCurve != null)
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
            var elementView = GetElementView(data);
            elementView.Initialize(data, index);
            elementView.RectTransform.anchoredPosition = position;
            activeItemsCount++;
            return elementView;
        }

        private void ReleaseFirstElement()
        {
            HeadIndex--;

            if (activeItemsCount > 0)
            {
                ReleaseElement(First);
                activeElements.RemoveFirst();
            }
        }

        private void ReleaseLastElement()
        {
            TrailIndex++;

            if (activeItemsCount <= 0)
            {
                return;
            }

            ReleaseElement(Last);
            activeElements.RemoveLast();
        }

        private void ReleaseAllItems()
        {
            while (activeElements.Count > 0)
            {
                ReleaseElement(activeElements.First.Value);
                activeElements.RemoveFirst();
            }

            HeadIndex = 0;
            TrailIndex = 0;
            activeItemsCount = 0;
        }

        private void CreateHeadItem(in int itemIndex)
        {
            var newElement = CreateElement(
                itemsData[itemIndex],
                CalculateItemPositionInContent(itemIndex),
                itemIndex);

            activeElements.AddFirst(newElement);
        }

        private void CreateTrailItem(in int itemIndex)
        {
            var newElement = CreateElement(
                itemsData[itemIndex],
                CalculateItemPositionInContent(itemIndex),
                itemIndex);

            activeElements.AddLast(newElement);
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

            activeElements.AddFirst(CreateElement(
                itemsData[index],
                CalculateItemPositionInContent(index),
                index));

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
        }

        private ElementView PeekElementView(IElementData data)
        {
            var pool = GetElementPool(data.PrefabPath);
            return pool.Peek();
        }

        private ScrollElementsPool GetElementPool(string prefabPath)
        {
            if (!elementPools.TryGetValue(prefabPath, out var pool))
            {
                elementPools[prefabPath] = pool = new ScrollElementsPool(prefabPath, Content);
            }

            return pool;
        }

        private ElementView GetElementView(IElementData data)
        {
            var pool = GetElementPool(data.PrefabPath);
            return pool.Get();
        }

        private void ReleaseElement(ElementView element)
        {
            var pool = GetElementPool(element.Data.PrefabPath);
            pool.Release(element);
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