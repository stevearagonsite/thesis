﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TPCamera;

namespace Player
{
    public class AimingState : IState<Inputs>
    {
        Dictionary<Inputs, IState<Inputs>> _transitions;

        Transform transform;
        Transform _mainCamera;
        Animator _anim;

        float _horizontal;
        float _vertical;

        float _horizontalSpeed;
        float _verticalSpeed;

        PlayerController _pc;

        public AimingState(Transform t, CameraFSM mainCamera, Animator anim, float speed, PlayerController pc)
        {
            transform = t;
            _mainCamera = mainCamera.transform;
            _anim = anim;
            _horizontalSpeed = speed/6;
            _verticalSpeed = speed/3;
            _pc = pc;
        }

        public void Enter()
        {
            _anim.SetBool("isAbsorbing", true);
            _anim.SetFloat("speed", 1);
            _pc.forwardCheck.SetCollider(ForwardChecker.FowardSizes.ABSORBING);
        }

        public void Execute()
        {

            _horizontal = GameInput.instance.horizontalMove;
            _vertical = GameInput.instance.verticalMove;

            var moveVector = new Vector2(_horizontal, _vertical).normalized;

            _anim.SetFloat("horizontalSpeed", moveVector.x * 90);
            _anim.SetFloat("verticalSpeed", moveVector.y * 90);

            if (!_pc.forwardCheck.isForwardObstructed)
            {
                transform.position += transform.forward * moveVector.y * _verticalSpeed * Time.deltaTime;
                transform.position += transform.right * moveVector.x * _horizontalSpeed * Time.deltaTime;
            }

            var camYRotation = Quaternion.Euler(0, _mainCamera.eulerAngles.y, 0);
            transform.rotation = camYRotation;
        }

        public void Exit()
        {
            _anim.SetBool("isAbsorbing", false);
            _anim.SetFloat("speed", 0);
        }

        public Dictionary<Inputs, IState<Inputs>> Transitions
        {
            get { return _transitions; }
            set { _transitions = value; }
        }
    }

}
