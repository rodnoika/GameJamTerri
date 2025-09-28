using UnityEngine;
using Unity.FPS.AI;
using Unity.FPS.Game; // Health, Actor

[RequireComponent(typeof(EnemyController))]
public class EnemyMelee : MonoBehaviour
{
    [Header("Melee")]
    public float AttackStopDistance = 1.8f;   // дистанция, на которой начинаем бить
    public int Damage = 20;
    public float AttackCooldown = 0.8f;

    [Header("Animation & Sound")]
    public Animator Animator;
    public string AttackTriggerName = "Attack"; // имя триггера в аниматоре
    public AudioClip[] AttackSounds;                 // можно задать несколько звуков
    [Range(0.8f, 1.2f)] public float PitchVariation = 0.1f;

    EnemyController m_EnemyController;
    float m_LastAttackTime;

    void Start()
    {
        m_EnemyController = GetComponent<EnemyController>();

        if (m_EnemyController.NavMeshAgent != null)
            m_EnemyController.NavMeshAgent.stoppingDistance = AttackStopDistance * 0.9f;
    }

    void Update()
    {
        var targetGO = m_EnemyController.KnownDetectedTarget;
        if (targetGO != null)
        {
            var target = targetGO.transform;
            float dist = Vector3.Distance(transform.position, target.position);

            if (dist > AttackStopDistance)
            {
                m_EnemyController.SetNavDestination(target.position);
                m_EnemyController.OrientTowards(target.position);
            }
            else
            {
                m_EnemyController.SetNavDestination(transform.position);
                m_EnemyController.OrientTowards(target.position);

                if (Time.time - m_LastAttackTime >= AttackCooldown)
                {
                    DoMeleeHit(target);
                    m_LastAttackTime = Time.time;
                }
            }
        }
        else
        {
            // патруль
            m_EnemyController.UpdatePathDestination();
            m_EnemyController.SetNavDestination(m_EnemyController.GetDestinationOnPath());
        }
    }

    void DoMeleeHit(Transform target)
    {
        // АНИМАЦИЯ
        if (Animator != null && !string.IsNullOrEmpty(AttackTriggerName))
        {
            Animator.SetTrigger(AttackTriggerName);
        }

        // УРОН
        var health = target.GetComponentInParent<Health>();
        if (health != null)
        {
            health.TakeDamage(Damage, gameObject);
        }

        // ЗВУК
        if (AttackSounds != null && AttackSounds.Length > 0)
        {
            var clip = AttackSounds[Random.Range(0, AttackSounds.Length)];
            float pitch = Random.Range(1f - PitchVariation, 1f + PitchVariation);
            AudioUtility.CreateSFX(clip, transform.position, AudioUtility.AudioGroups.WeaponShoot, 1f, pitch);
        }
    }
}
