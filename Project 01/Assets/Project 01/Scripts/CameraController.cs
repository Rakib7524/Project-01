using UnityEngine;
using UnityEngine.Events;

public class CameraController : MonoBehaviour
{
    public static UnityAction<Vector3> leftClickOnGround;
    
    private Ray _ray;
    private int _groundLayer;
    private RaycastHit _hit;

    private void Awake()
    {
        _groundLayer = 1 << LayerMask.NameToLayer("Ground");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 0 = Mouse left button.
        {
            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(_ray, out _hit, 50f, _groundLayer))
            {
                Debug.Log("Left click on ground.");
                leftClickOnGround?.Invoke(_hit.point);
            }
        }
    }

    private void OnDrawGizmos()
    {
        //Debug.DrawRay(_ray.origin, _ray.direction * 25f, Color.red, 1f);
        Gizmos.DrawRay(_ray.origin, _ray.direction * 50f);
        Gizmos.DrawSphere(_hit.point, 0.5f);
    }
}
