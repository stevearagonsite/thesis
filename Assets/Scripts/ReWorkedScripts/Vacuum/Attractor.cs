﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attractor : IVacuumAction {

    public List<IVacuumObject> _objectsToInteract;

    private float _atractForce;
    private float _shootSpeed;
    private Transform _vacuumHoleTransform;
    private IHandEffect _aspireParticle;
    private IHandEffect _blowParticle;

    private bool _isStuck;

    private PathCalculate _pc;

    public Attractor(float atractForce, float shootSpeed, Transform vacuumHole, IHandEffect aspireParticle, IHandEffect blowParticle, PathCalculate pc, List<IVacuumObject> objectsToInteract)
    {
        _atractForce = atractForce;
        _shootSpeed = shootSpeed;
        _vacuumHoleTransform = vacuumHole;
        _aspireParticle = aspireParticle;
        _blowParticle = blowParticle;
        _pc = pc;
        _objectsToInteract = objectsToInteract;

    }

    public void Initialize(){}

    public void Execute(params object[] pC)
    {
        _objectsToInteract = (List<IVacuumObject>)pC[0];

        if (_isStuck)
        {
            /*if(_objectsToInteract.Count > 0)
                _pc.SimulatePath(_objectsToInteract[0].rb);*/
            _aspireParticle.StopEffect();
            _blowParticle.StopEffect();
            _isStuck = false;
            if (GameInput.instance.blowUpButton)
            {
                if (_objectsToInteract.Count > 0){
                    _objectsToInteract[0].ViewFX(false);
                    _objectsToInteract[0].Shoot(_shootSpeed, _vacuumHoleTransform.forward);
                }
                //_pc.DeactivatePath();
                _isStuck = false;
            }
            else if (GameInput.instance.absorbButton)
            {
                Attract();
                if(_objectsToInteract.Count > 0)
                    _objectsToInteract[0].ViewFX(true);
            }
            else
            {
                _aspireParticle.StopEffect();
                _blowParticle.StopEffect();
                //_pc.DeactivatePath();
                _isStuck = false;
                foreach (var obj in _objectsToInteract)
                {
                    obj.Exit();
                }
            }
        }
        else
        {
            if (GameInput.instance.blowUpButton)
            {
                _aspireParticle.StopEffect();
                _isStuck = false;
                Reject();
                if (!_blowParticle.IsPlaying())
                    _blowParticle.StartEffect();
            }
            else if (GameInput.instance.absorbButton)
            {
                _blowParticle.StopEffect();
                if (!_aspireParticle.IsPlaying() && !_isStuck)
                    _aspireParticle.StartEffect();
                Attract();
            }
            else
            {
                _aspireParticle.StopEffect();
                //aspireParticle.TerminateEffect();
                _blowParticle.StopEffect();
                //blowParticle.TerminateEffect();
                foreach (var obj in _objectsToInteract)
                {
                    obj.Exit();
                }
            }
           
        }
        
    }

    public void Exit()
    {
        _aspireParticle.StopEffect();
        _aspireParticle.TerminateEffect();
        _blowParticle.StopEffect();
        _blowParticle.TerminateEffect();
        //_pc.DeactivatePath();
        _isStuck = false;
        foreach (var obj in _objectsToInteract)
        {
            obj.Exit();
        }
    }

    void Attract ()
    {
        for (int i = 0; i < _objectsToInteract.Count; i++)
        {
            if (!_isStuck)
            {
                _objectsToInteract[i].SuckIn(_vacuumHoleTransform, _atractForce);
                _objectsToInteract[i].isBeeingAbsorved = true;
            }
            if (_objectsToInteract[i].isAbsorved && _objectsToInteract[i].isAbsorvable)
            {
                _objectsToInteract[i].ReachedVacuum();
                _objectsToInteract.Remove(_objectsToInteract[i]);
            }
            else if (_objectsToInteract[i].isAbsorved && !_objectsToInteract[i].isAbsorvable)
            {
                var aux = _objectsToInteract[i];
                _objectsToInteract = new List<IVacuumObject>();
                _objectsToInteract.Add(aux);
                _isStuck = true;
                _aspireParticle.StopEffect();
            }
        }
    }

    void Reject()
    {
        _isStuck = false;
        if (_objectsToInteract.Count > 0)
        {
            foreach (var obj in _objectsToInteract)
            {
                obj.BlowUp(_vacuumHoleTransform, _atractForce, _vacuumHoleTransform.forward);
                obj.isBeeingAbsorved = true;
            }

        }
        
        
    }
}
