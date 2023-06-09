﻿using System;
using System.Collections;
using System.Collections.Generic;
using IgorTime.PoolableScrollView.DataItems;
using IgorTime.PoolableScrollView.ItemView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IgorTime.PoolableScrollView.Scrolls
{
    [RequireComponent(typeof(ItemViewProvider))]
    public abstract class BasePoolableScrollView : ScrollRect
    {
        [SerializeField]
        protected ItemViewProvider itemViewProvider;

        [SerializeField]
        private bool interactable = true;

        protected readonly Dictionary<int, ItemView.ItemView> ActiveElements = new();

        protected ItemViewData[] ViewsData;
        protected Rect ContentRect;
        protected RectTransform Content;
        protected float ViewportHeight;
        protected float ViewportWidth;

        protected int HeadIndex;
        protected int TrailIndex;

        private IItemData[] itemsData;
        private Vector2? previousContentPosition;
        private int activeItemsCount;
        private Coroutine scrollCoroutine;

        public bool IsAnimated => scrollCoroutine != null;

        private ItemView.ItemView Head
        {
            get
            {
                if (HeadIndex == -1)
                {
                    return null;
                }

                return ActiveElements.TryGetValue(HeadIndex, out var element)
                    ? element
                    : null;
            }
        }

        private ItemView.ItemView Trail
        {
            get
            {
                if (TrailIndex == -1)
                {
                    return null;
                }

                return ActiveElements.TryGetValue(TrailIndex, out var element)
                    ? element
                    : null;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            Content = content;

            var viewportRect = viewport.rect;
            ViewportHeight = viewportRect.height;
            ViewportWidth = viewportRect.width;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            onValueChanged.AddListener(UpdateScrollItems);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            onValueChanged.RemoveListener(UpdateScrollItems);
        }

        public void Initialize(IItemData[] itemsData)
        {
            this.itemsData = itemsData;
            InitViewsData(this.itemsData, out var contentSize);
            Content.sizeDelta = contentSize;
            ContentRect = content.rect;
            previousContentPosition = Content.anchoredPosition;
            CreateInitialElements(itemsData, Content.anchoredPosition);
            UpdateItemsRelativePosition();
        }

        public void ScrollToItem(int itemIndex)
        {
            velocity = Vector2.zero;
            Content.anchoredPosition = GetAnchoredPositionOfContentForItem(itemIndex);
        }

        public void ScrollToItem(
            int itemIndex,
            float duration,
            AnimationCurve easingCurve = null,
            Action onCompleted = null)
        {
            if (scrollCoroutine != null)
            {
                return;
            }

            velocity = Vector2.zero;

            var targetPosition = GetAnchoredPositionOfContentForItem(itemIndex);
            scrollCoroutine = StartCoroutine(ScrollCoroutine(targetPosition, duration, easingCurve, onCompleted));
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

        public void ScrollToNext(
            float duration,
            AnimationCurve easingCurve = null,
            Action onCompleted = null)
        {
            var index = FindClosestItemToCenter();
            var nextIndex = Mathf.Clamp(index + 1, 0, ViewsData.Length - 1);

            if (duration > 0)
            {
                ScrollToItem(nextIndex, duration, easingCurve, onCompleted);
            }
            else
            {
                ScrollToItem(nextIndex);
            }
        }

        public void ScrollToPrevious(
            float duration,
            AnimationCurve easingCurve = null,
            Action onCompleted = null)
        {
            var index = FindClosestItemToCenter();
            var previousIndex = Mathf.Clamp(index - 1, 0, ViewsData.Length - 1);

            if (duration > 0)
            {
                ScrollToItem(previousIndex, duration, easingCurve, onCompleted);
            }
            else
            {
                ScrollToItem(previousIndex);
            }
        }

        public abstract int FindClosestItemToCenter();

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (!interactable)
            {
                return;
            }

            base.OnBeginDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (!interactable)
            {
                return;
            }

            base.OnEndDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (!interactable)
            {
                return;
            }

            base.OnDrag(eventData);
        }

        protected abstract void InitViewsData(IItemData[] dataElements, out Vector2 contentSize);
        protected abstract bool IsMovingForward(in Vector2 contentDeltaPosition);
        protected abstract bool IsFastScrolling(in Vector2 contentDeltaPosition);
        protected abstract Vector2 CalculateItemPositionInContent(in int itemIndex);
        protected abstract bool IsOutOfViewportInForwardDirection(int itemIndex, in Vector2 contentAnchoredPosition);
        protected abstract bool IsOutOfViewportInBackwardDirection(int itemIndex, in Vector2 contentAnchoredPosition);
        protected abstract bool IsPartiallyVisibleInViewport(in int itemIndex, in Vector2 contentAnchoredPosition);
        protected abstract int FindFirstItemVisibleInViewport(in Vector2 contentAnchoredPosition);
        protected abstract Vector2 GetAnchoredPositionOfContentForItem(int itemIndex);

        protected Vector2 GetElementSize(IItemData data)
        {
            var prefab = itemViewProvider.Peek(data);
            return prefab.Size;
        }

        protected abstract void UpdateItemsRelativePosition();

        private IEnumerator ScrollCoroutine(
            Vector2 targetPosition,
            float duration,
            AnimationCurve easingCurve = null,
            Action onCompleted = null)
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

            yield return null;
            scrollCoroutine = null;
            onCompleted?.Invoke();
        }

        private bool IsScrolledToTheEnd() => HeadIndex == itemsData.Length - 1;
        private bool IsScrolledToTheStart() => TrailIndex == 0;

        private ItemView.ItemView CreateElement(IItemData data, Vector2 position, int index)
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
                ActiveElements.Remove(HeadIndex);
            }

            HeadIndex--;
        }

        private void ReleaseLastElement()
        {
            if (activeItemsCount > 0)
            {
                ReleaseElement(Trail);
                ActiveElements.Remove(TrailIndex);
            }

            TrailIndex++;
        }

        private void ReleaseAllItems()
        {
            foreach (var active in ActiveElements.Values)
            {
                ReleaseElement(active);
            }

            HeadIndex = 0;
            TrailIndex = 0;
            activeItemsCount = 0;
            ActiveElements.Clear();
        }

        private void CreateHeadItem(in int itemIndex)
        {
            var newElement = CreateElement(
                itemsData[itemIndex],
                CalculateItemPositionInContent(itemIndex),
                itemIndex);

            ActiveElements[itemIndex] = newElement;
        }

        private void CreateTrailItem(in int itemIndex)
        {
            var newElement = CreateElement(
                itemsData[itemIndex],
                CalculateItemPositionInContent(itemIndex),
                itemIndex);

            ActiveElements[itemIndex] = newElement;
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

            HandleMovement(contentDeltaPosition, contentAnchoredPosition);
            UpdateItemsRelativePosition();
        }

        private void HandleMovement(
            in Vector2 contentDeltaPosition,
            in Vector2 contentAnchoredPosition)
        {
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

            ActiveElements[index] = CreateElement(
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

        private bool IsScrollEmpty() => ActiveElements.Count == 0 || itemsData.Length == 0;

        private void OnValidate()
        {
            if (!itemViewProvider)
            {
                itemViewProvider = GetComponent<ItemViewProvider>();
            }
        }

        private void ReleaseElement(ItemView.ItemView item)
        {
            itemViewProvider.Release(item);
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

        private void CreateInitialElements(IItemData[] elementsData, in Vector2 anchoredPosition)
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