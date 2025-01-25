using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraClamp : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _virtualCamera;
    [SerializeField] private float _minX = -2.0f;
    [SerializeField] private float _maxX = 2.0f;
    [SerializeField] private float _minY = 0.0f;

    private Transform _followTarget;

    private void Start()
    {
        if (_virtualCamera.Follow == null)
        {
            Debug.LogError("Cinemachine Virtual Camera requires a Follow target for clamping to work.");
            enabled = false;
            return;
        }

        _followTarget = _virtualCamera.Follow;
    }

    private void LateUpdate()
    {
        Vector3 targetPosition = _followTarget.position;
        targetPosition.x = Mathf.Clamp(targetPosition.x, _minX, _maxX);
        targetPosition.y = Mathf.Max(targetPosition.y, _minY);
        _followTarget.position = targetPosition;

        /*Vector3 targetPosition = _followTarget.position;
        targetPosition.x = Mathf.Clamp(targetPosition.x, _minX, _maxX);
        _followTarget.position = targetPosition;*/
    }
}
