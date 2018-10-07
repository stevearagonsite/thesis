﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAnimation : MonoBehaviour {

    public Weight pesimeter;
    private Animator _animator;

    void OnEnable() {

        _animator = GetComponent<Animator>();
        if (pesimeter != null && _animator != null)
        {
            Debug.Log("1");
            pesimeter.AddOnWeightEnterEvent(() => {if (pesimeter.isActiveAndEnabled) _animator.Play("Animation");  });
            pesimeter.AddOnWeightExitEvent(() => { if (pesimeter.isActiveAndEnabled) _animator.Play("Animation"); });
            pesimeter.AddOnWeightEvent(() => { if (pesimeter.isActiveAndEnabled) { _animator.speed = 1; } else { _animator.speed = 0; } });
            _animator.speed = 0;
        }
    }

    public void StopAnimator(){
        if (pesimeter != null && _animator != null)
        {
            Debug.Log("hey");
            _animator.speed = 0;
        }
    }
}
