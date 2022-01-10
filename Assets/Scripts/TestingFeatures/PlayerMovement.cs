using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] private CharacterController _controller;
    // Start is called before the first frame update
    [SerializeField] private float _speed=12.0f;
    [SerializeField] private float _gravity=9.81f;

    [SerializeField]
    private Transform _groundCheck=null;
    [SerializeField]
    private float _groundDistance = 0.4f;
    [SerializeField]
    private LayerMask _groundMask;
    private Vector3 _velocity;

    private bool _isGrounded;
    // Update is called once per frame
    void Update()
    {
        _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundDistance, _groundMask);

        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        _controller.Move(move * _speed*Time.deltaTime);
        _velocity.y -= _gravity * Time.deltaTime;
        _controller.Move(_velocity * _speed*Time.deltaTime);

    }
}
