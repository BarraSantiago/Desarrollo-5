using UnityEngine.EventSystems;

namespace UI
{
    public static class UIHelper
    {
        private static bool isPointerOverUI;

        public static void UpdatePointerOverUIState()
        {
            isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
        }

        public static bool IsPointerOverUIElement()
        {
            return isPointerOverUI;
        }
    }
}