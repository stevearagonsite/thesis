﻿using System;
using System.Collections;
using System.Collections.Generic;
using Logger;
using UnityEngine;
using Debug = Logger.Debug;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioObjectEmitter))]
[RequireComponent(typeof(Collider))]
public class MediumSizeObject : MonoBehaviour, IVacuumObject {
    [HideInInspector] public bool wasShooted;

    public MeshRenderer refRenderer;
    public TrackerModel trackerModel;

    private float _alphaCut;

    private Vector3 _initialPosition;
    public float respawnDistance;
    public float respawnTime;

    private float _respawnTick;

    private float _disolveTimmer = 1;
    private float _disolveTick;
    private bool _disolve;
    private bool _enableSounds;


    protected bool _isAbsorved;
    protected bool _isAbsorvable;
    protected bool _isBeeingAbsorved;
    public static bool drawGizmos = true;

    public bool isAbsorved {
        get => _isAbsorved;
        set => _isAbsorved = value;
    }

    public bool isAbsorvable => _isAbsorvable;

    public bool isBeeingAbsorved {
        get => _isBeeingAbsorved;
        set => _isBeeingAbsorved = value;
    }
    
    protected Rigidbody _rb;

    public Rigidbody rb {
        get {
            if (_rb != null) return _rb;
            
            if (GetComponent<Rigidbody>() != null) {
                _rb = GetComponent<Rigidbody>();
            }

            if (_rb == null) {
                Debug.LogError(this, "No Rigidbody assign component: " + gameObject.name);
            }
            
            return _rb;
        }
        set => _rb = value;
    }
    
    private Collider _collider;
    public Collider collider {
        get {
            if (_collider != null) return _collider;
            
            if (GetComponent<Collider>() != null) {
                _collider = GetComponent<Collider>();
            }

            if (_collider == null) {
                Debug.LogError(this, "No Rigidbody assign component: " + gameObject.name);
            }
            
            return _collider;
        }
        set => _collider = value;
    }

    private AudioObjectEmitter _audioObjectEmitter;

    public bool respawnable;

    protected void Start() {
        _initialPosition = transform.position;
        _isAbsorvable = false;
        _rb = GetComponent<Rigidbody>();
        _audioObjectEmitter = GetComponent<AudioObjectEmitter>();
        _collider = GetComponent<Collider>();
#if UNITY_EDITOR
        if (_collider == null) {
            Debug.LogError(this, "No Collider found: " + gameObject.name);
        }
#endif
#if UNITY_EDITOR
        if (_rb == null) {
            Debug.LogError(this, "No Rigidbody found: " + gameObject.name);
        }
#endif

        SpawnVFXActivate(true);
        StartCoroutine(EnableSounds());
    }

    private IEnumerator EnableSounds() {
        yield return new WaitForSeconds(3f);
        _enableSounds = true;
    }

    private void Execute() {
        var d = Mathf.Abs((transform.position - _initialPosition).magnitude);
        if (!(d > respawnDistance)) return;
        wasShooted = false;
        _disolve = false;
        SpawnVFXActivate(false);
    }

    public void SpawnVFXActivate(bool dir) {
        if (dir) {
            _alphaCut = 1;
            UpdatesManager.instance.AddUpdate(UpdateType.UPDATE, SpawnVFX);
            if (respawnable)
                UpdatesManager.instance.AddUpdate(UpdateType.UPDATE, Execute);
        }
        else {
            _alphaCut = 0;
            UpdatesManager.instance.AddUpdate(UpdateType.UPDATE, DespawnVFX);
            if (respawnable)
                UpdatesManager.instance.RemoveUpdate(UpdateType.UPDATE, Execute);
        }
    }

    private void SpawnVFX() {
        if (_respawnTick >= respawnTime) {
            // material.SetFloat("_DisolveAmount", _alphaCut);
            _alphaCut -= Time.deltaTime;
            _rb.useGravity = true;
            _collider.isTrigger = false;
            if (!(_alphaCut <= 0)) return;
            UpdatesManager.instance.RemoveUpdate(UpdateType.UPDATE, SpawnVFX);
            wasShooted = false;
            _respawnTick = 0;
        }
        else {
            _respawnTick += Time.deltaTime;
        }
    }

    void DespawnVFX() {
        _alphaCut += Time.deltaTime;
        if (!(_alphaCut >= 1)) return;
        UpdatesManager.instance.RemoveUpdate(UpdateType.UPDATE, DespawnVFX);
        RepositionOnSpawn();
    }

    public void RepositionOnSpawn() {
        Debug.LogColor(this,"Respawn: " + gameObject.name, LogColor.RED);
        transform.position = _initialPosition;
        transform.rotation = Quaternion.identity;
        rb.velocity = Vector3.zero;
        rb.useGravity = false;
        collider.isTrigger = true;
        SpawnVFXActivate(true);
        wasShooted = true;

        //for box temperature
        var bt = GetComponent<BoxTemperature>();
        if (bt) {
            bt.ResetBox();
        }
    }

    public void SuckIn(Transform origin, float attractForce) {
        if (wasShooted) return;
        var direction = origin.position - transform.position;
        var distance = direction.magnitude;

        if (distance <= 0.7f) {
            _collider.isTrigger = true;
            rb.isKinematic = true;
            transform.position = origin.position;
            isAbsorved = true;

            //TODO: Pending change implementation from set parent to set position
            transform.SetParent(origin);
        }
        else if (distance < 1f) {
            rb.isKinematic = true;
            var dir = (origin.position - transform.position).normalized;
            transform.position += dir * attractForce / 10 * Time.deltaTime;
        }
        else {
            rb.isKinematic = false;
            var forceMagnitude = (10) * attractForce / Mathf.Pow(distance, 2);
            var force = direction.normalized * forceMagnitude;
            rb.AddForce(force);
        }
    }

    public void BlowUp(Transform origin, float atractForce, Vector3 direction) {
        if (wasShooted) return;
        rb.isKinematic = false;
        isAbsorved = false;
        transform.SetParent(null);
        var distanceRaw = transform.position - origin.position;
        var distance = distanceRaw.magnitude;
        var forceMagnitude = 10 * atractForce * 10 / Mathf.Pow(distance, 2);
        forceMagnitude = Mathf.Clamp(forceMagnitude, 0, 2000);
        var force = direction.normalized * forceMagnitude;
        rb.AddForce(force);
    }

    public void ReachedVacuum() {
    }

    public void Shoot(float shootForce, Vector3 direction) {
        _collider.isTrigger = false;
        wasShooted = true;
        isAbsorved = false;
        rb.isKinematic = false;
        transform.SetParent(null);
        rb.velocity = direction * shootForce / rb.mass;
        _disolveTick = 0;
    }

    public void ViewFX(bool activate) {
        // material.SetFloat("_Alpha", activate ? 0.3f : 1f);
    }

    public void Exit() {
        _collider.isTrigger = false;
        ViewFX(false);
        transform.SetParent(null);
        rb.isKinematic = false;
        isAbsorved = false;
    }

    private void OnCollisionEnter(Collision collision) {
        //TODO: Find a better way to exclude "Player" collision
        if (collision.gameObject.name != "Player") {
            wasShooted = false;
        }

        var relativeVelocity = collision.relativeVelocity;
        var collisionForce = relativeVelocity.magnitude;
        if (!(collisionForce > 5f)) return;
        if (!_enableSounds) return;
        _audioObjectEmitter.PlaySoundHit(0.3f);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, respawnDistance);
    }

    protected void OnDestroy() {
        if (respawnable)
            UpdatesManager.instance.RemoveUpdate(UpdateType.UPDATE, Execute);
    }

    private void OnDrawGizmos() {
        return;
        if (!drawGizmos) return;
        Gizmos.color = new Color(255, 255, 255, 0.5f);
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}