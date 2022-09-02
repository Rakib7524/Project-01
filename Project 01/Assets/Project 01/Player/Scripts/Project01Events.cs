using UnityEngine;
using UnityEngine.Events;

namespace Project01
{
    public class Project01Events : MonoBehaviour
    {
        public static UnityAction<Vector3> groundLeftClicked;

        private void Awake()
        {
            CameraController.leftClickOnGround += OnLeftClickOnGround;
        }

        private void OnLeftClickOnGround(Vector3 p_position)
        {
            Debug.Log("Event: OnGroundClicked on position: " + p_position);
            groundLeftClicked?.Invoke(p_position);
        }
    }
}