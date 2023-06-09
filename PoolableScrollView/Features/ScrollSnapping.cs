﻿using IgorTime.PoolableScrollView.Scrolls;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IgorTime.PoolableScrollView.Features
{
    [RequireComponent(typeof(BasePoolableScrollView))]
    public class ScrollSnapping : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField]
        private BasePoolableScrollView poolableScrollView;

        [Header("Settings:")]
        [SerializeField]
        private float speedThreshold = 100f;

        [SerializeField]
        private float snappingDuration = 0.05f;

        [SerializeField]
        private AnimationCurve snappingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private bool isDragging;
        private float sqrThreshold;

        private void Awake()
        {
            sqrThreshold = speedThreshold * speedThreshold;
        }

        private void OnEnable()
        {
            poolableScrollView.onValueChanged.AddListener(OnScrollValueChanged);
        }

        private void OnDisable()
        {
            poolableScrollView.onValueChanged.RemoveListener(OnScrollValueChanged);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
            HandleSnapping();
        }

        private void OnScrollValueChanged(Vector2 position)
        {
            if (poolableScrollView.IsAnimated || isDragging)
            {
                return;
            }

            HandleSnapping();
        }

        private void HandleSnapping()
        {
            if (Mathf.Abs(poolableScrollView.velocity.sqrMagnitude) > sqrThreshold)
            {
                return;
            }

            var centeredItem = poolableScrollView.FindClosestItemToCenter();
            poolableScrollView.ScrollToItem(centeredItem, snappingDuration, snappingCurve);
        }

        private void OnValidate()
        {
            if (!poolableScrollView)
            {
                poolableScrollView = GetComponent<BasePoolableScrollView>();
            }
        }
    }
}