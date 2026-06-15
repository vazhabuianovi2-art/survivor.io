using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Follows the camera so the background always fills the screen.
    /// Scale is set large enough to stay fully visible at any camera position.
    /// </summary>
    public class InfiniteBackground : MonoBehaviour
    {
        private Camera _cam;

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void LateUpdate()
        {
            if (_cam == null) return;
            var p = _cam.transform.position;
            transform.position = new Vector3(p.x, p.y, transform.position.z);
        }
    }
}
