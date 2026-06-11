using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DebugPanelExtention
{
    public static class GameObjectExtension
    {
        public static void SetActiveSafe(this GameObject gameObejct, bool active = true)
        {
            if (gameObejct == null)
                return;
            gameObejct.SetActive(active);
        }

        public static void SetActiveSafe(this Component component, bool active = true)
        {
            if (component == null)
                return;
            component.gameObject.SetActive(active);
        }

        public static void SetSpriteSafe(this Image image, Sprite sprite = null)
        {
            if (image == null)
                return;
            image.sprite = sprite;
        }

        public static void SetFillAmountSafe(this Image image, float value = 0)
        {
            if (image == null)
                return;
            image.fillAmount = value;
        }

        public static void SetColorSafe(this Image image, Color color)
        {
            if (image == null)
                return;
            image.color = color;
        }

        public static void OnClickAddListenerSafe(this Button button, UnityAction callback)
        {
            if (button == null)
                return;
            button.onClick.AddListener(callback);
        }

        public static void OnClickRemoveListenerSafe(this Button button, UnityAction callback)
        {
            if (button == null)
                return;
            button.onClick.RemoveListener(callback);
        }

        public static void OnClickRemoveAllListenersSafe(this Button button)
        {
            if (button == null)
                return;
            button.onClick.RemoveAllListeners();
        }
    }
}