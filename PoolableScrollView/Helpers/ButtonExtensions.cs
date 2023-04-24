using UnityEngine.Events;
using UnityEngine.UI;

namespace IgorTime.PoolableScrollView.Helpers
{
    public static class ButtonExtensions
    {
        public static void SubscribeOnClick(this Button button, UnityAction action)
        {
            if (button)
            {
                button.onClick.AddListener(action);
            }
        }

        public static void UnsubscribeOnClick(this Button button, UnityAction action)
        {
            if (button)
            {
                button.onClick.RemoveListener(action);
            }
        }
    }
}