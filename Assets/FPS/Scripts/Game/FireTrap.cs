using UnityEngine;
using System.Collections;
using Unity.FPS.Game; // для Health

public class TimedFireTrap : TrapBase
{
    [Header("Trap Settings")]
    public ParticleSystem FireEffect;
    public AudioClip FireSound;

    [Tooltip("Сколько секунд горит огонь при активации")]
    public float ActiveTime = 2f;

    [Tooltip("Интервал между активациями (секунд)")]
    public float Interval = 3f;

    [Tooltip("Урон за тик горения")]
    public int BurnDamage = 5;

    [Tooltip("Как часто наносить урон (секунд)")]
    public float BurnInterval = 1f;

    private bool isActive;

    void Start()
    {
        StartCoroutine(TrapRoutine());
    }

    IEnumerator TrapRoutine()
    {
        while (true)
        {
            // включаем ловушку
            ActivateTrap();
            yield return new WaitForSeconds(ActiveTime);

            // выключаем эффекты
            DeactivateTrap();
            yield return new WaitForSeconds(Interval);
        }
    }

    void ActivateTrap()
    {
        isActive = true;

        if (FireEffect != null) FireEffect.Play();
        if (FireSound != null) AudioSource.PlayClipAtPoint(FireSound, transform.position);
    }

    void DeactivateTrap()
    {
        isActive = false;

        if (FireEffect != null) FireEffect.Stop();
    }

    void OnTriggerStay(Collider other)
    {
        if (!isActive) return;

        var health = other.GetComponentInParent<Health>();
        if (health != null)
        {
            // наносим периодический урон
            StartCoroutine(DoBurn(health));
        }
    }

    IEnumerator DoBurn(Health health)
    {
        float timer = 0;
        while (timer < ActiveTime && isActive)
        {
            health.TakeDamage(BurnDamage, gameObject);
            yield return new WaitForSeconds(BurnInterval);
            timer += BurnInterval;
        }
    }
}
