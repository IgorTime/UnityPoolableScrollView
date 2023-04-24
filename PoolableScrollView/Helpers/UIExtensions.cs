using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IgorTime.PoolableScrollView.Helpers
{
    public static class UIExtensions
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
        
        public static void SubscribeOnSubmit(this TMP_InputField inputField, UnityAction<string> action)
        {
            if (inputField)
            {
                inputField.onSubmit.AddListener(action);
            }
        }

        public static void UnsubscribeOnSubmit(this TMP_InputField inputField, UnityAction<string> action)
        {
            if (inputField)
            {
                inputField.onSubmit.RemoveListener(action);
            }
        }
    }
}