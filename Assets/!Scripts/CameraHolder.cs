using UnityEngine;

public class CameraHolder : MonoBehaviour
{
    [Header("TargetPoint")]
    [SerializeField] private Transform targetPos;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private Vector3 offset;

    private void Start()
    {
        if (offset == Vector3.zero)
        {
            offset = transform.position - targetPos.position;
        }
    }

    private void LateUpdate()
    {
        if (targetPos == null) return;

        Vector3 desiredPosition = targetPos.position + offset;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}