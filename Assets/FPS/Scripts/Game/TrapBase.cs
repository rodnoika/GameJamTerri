using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.FPS.Game;

public abstract class TrapBase : MonoBehaviour
{
    [Header("Параметры ловушки")]
    public int Damage = 20;
    public float Cooldown = 0.75f; 
    public LayerMask DamageLayers = ~0; 

    readonly Dictionary<Health, float> _lastHitTime = new Dictionary<Health, float>();

    protected virtual void Awake()
    {
        var rb = GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void TryHit(Collider other)
    {
        Debug.Log($"Trap trigger enter with: {other.name} (layer {other.gameObject.layer})", other);

        if (((1 << other.gameObject.layer) & DamageLayers) == 0)
            return;

        var health = other.GetComponentInParent<Health>();
        if (!health) return;

        float t = Time.time;
        float last;
        if (_lastHitTime.TryGetValue(health, out last) && (t - last) < Cooldown)
            return; 

        health.TakeDamage(Damage, gameObject);
        _lastHitTime[health] = t;

        ActivateTrap(other.transform);
    }

    protected virtual void OnTriggerEnter(Collider other) => TryHit(other);

    protected virtual void OnTriggerStay(Collider other) => TryHit(other);

    protected virtual void ActivateTrap(Transform target) { }
}
