using System;
using UnityEngine;

namespace Game
{
    public class CursorSpriteController : MonoBehaviour
    {
        [SerializeField] private Texture2D releasedCursor;
        [SerializeField] private Texture2D pressedCursor;

        private void Start()
        {
            Cursor.SetCursor(releasedCursor, Vector2.zero, CursorMode.Auto);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.SetCursor(pressedCursor, Vector2.zero, CursorMode.Auto);
            }

            if (Input.GetMouseButtonUp(0))
            {
                Cursor.SetCursor(releasedCursor, Vector2.zero, CursorMode.Auto);
            }
        }
    }
}