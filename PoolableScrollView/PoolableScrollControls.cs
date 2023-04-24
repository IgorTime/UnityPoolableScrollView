using IgorTime.PoolableScrollView.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IgorTime.PoolableScrollView
{
    [RequireComponent(typeof(BasePoolableScrollView))]
    [AddComponentMenu(MenuConstants.ADD_COMPONENT_MENU_PATH + nameof(PoolableScrollControls))]
    public class PoolableScrollControls : MonoBehaviour
    {
        [SerializeField]
        private BasePoolableScrollView poolableScrollView;

        [Header("Buttons:")]
        [SerializeField]
        private Button buttonNext;

        [SerializeField]
        private Button buttonPrevious;

        [Header("Search:")]
        [SerializeField]
        private TMP_InputField searchField;

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
            searchField.SubscribeOnSubmit(OnSearchSubmit);
        }

        private void OnDisable()
        {
            buttonNext.UnsubscribeOnClick(OnNextClick);
            buttonPrevious.UnsubscribeOnClick(OnPreviousClick);
            searchField.UnsubscribeOnSubmit(OnSearchSubmit);
        }

        private void OnSearchSubmit(string searchValue)
        {
            if (int.TryParse(searchValue, out var index))
            {
                poolableScrollView.ScrollToItem(index, scrollDuration, scrollNextCurve);
            }
        }

        private void OnPreviousClick()
        {
            poolableScrollView.ScrollToPrevious(scrollDuration, scrollPreviousCurve);
        }

        private void OnNextClick()
        {
            poolableScrollView.ScrollToNext(scrollDuration, scrollNextCurve);
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