using UnityEngine;

namespace UVAnimation
{
    public class UVScrolling
    {
        public static Vector2 UVScroll(Vector2 uv, float speed, Vector2 direction)
        {
            var scrollDelta = uv;
            scrollDelta += direction * (speed * Time.deltaTime);
            scrollDelta.x = Mathf.Repeat(0f, 1f);
            scrollDelta.y = Mathf.Repeat(0f, 1f);
            
            return scrollDelta;
        }
    }
}
