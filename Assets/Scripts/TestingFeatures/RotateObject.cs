using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField] private Transform _pivot = null;
    [SerializeField] private Vector3 _rotateAxis = Vector3.zero;
    [SerializeField] private float _rotationSpeed = 10;

    private void Start()
    {
        if (!_pivot)
            _pivot = transform;

        if (_rotateAxis == Vector3.zero)
            _rotateAxis = Vector3.up;

    }

    void Update()
    {
        transform.RotateAround(_pivot.position, _rotateAxis, _rotationSpeed * Time.deltaTime);
    }
}
