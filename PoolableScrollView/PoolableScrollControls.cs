using IgorTime.PoolableScrollView.Helpers;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace IgorTime.PoolableScrollView
{
    [RequireComponent(typeof(BasePoolableScrollView))]
    [AddComponentMenu("IgorTime/PoolableScrollView/PoolableScrollControls")]
    public class PoolableScrollControls : MonoBehaviour
    {
        [FormerlySerializedAs("poolableScrollView")]
        [SerializeField]
        private BasePoolableScrollView basePoolableScrollView;

        [SerializeField]
        private Button buttonNext;

        [SerializeField]
        private Button buttonPrevious;

        [Header("Animation:")]
        [SerializeField]
        [Min(0)]
        private float scrollDuration = 0.5f;

        [SerializeField]
        private AnimationCurve scrollNextCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [SerializeField]
        private AnimationCurve scrollPreviousCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private void OnEnable()
        {
            buttonNext.SubscribeOnClick(OnNextClick);
            buttonPrevious.SubscribeOnClick(OnPreviousClick);
        }

        private void OnDisable()
        {
            buttonNext.UnsubscribeOnClick(OnNextClick);
            buttonPrevious.UnsubscribeOnClick(OnPreviousClick);
        }

        private void OnPreviousClick()
        {
            basePoolableScrollView.ScrollToPrevious(scrollDuration, scrollPreviousCurve);
        }

        private void OnNextClick()
        {
            basePoolableScrollView.ScrollToNext(scrollDuration, scrollNextCurve);
        }

        private void OnValidate()
        {
            if (!basePoolableScrollView)
            {
                basePoolableScrollView = GetComponent<BasePoolableScrollView>();
            }
        }
    }
}