using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.FPS.Game;   
using Unity.FPS.AI;    

public class BossController : MonoBehaviour
{
    [Header("Refs")]
    public Transform Player;        
    public Animator Animator;       
    public NavMeshAgent Agent;     
    public Health Health;

    [Header("Movement")]
    public float DetectRange = 25f;
    public float AttackRange = 6f;
    public float StopDistanceBuffer = 0.25f;

    [Header("Slam (ближняя)")]
    public float SlamCooldown = 5f;
    public float SlamWindup = 0.6f;
    public float SlamRadius = 5f;
    public int   SlamDamage = 20;
    public LayerMask DamageLayers = ~0;

    [Header("Volley (дальняя/средняя через EnemyController)")]
    public float VolleyCooldown = 7f;
    public float VolleyWindup = 0.7f;
    public int   VolleyCount = 5;          // сколько “нажатий” стрельбы
    public float VolleyShotInterval = 0.08f;

    [Header("Phase speeds")]
    [Range(0.1f, 2f)] public float P1_Speed = 3.2f;
    [Range(0.1f, 2f)] public float P2_Speed = 4.0f;
    [Range(0.1f, 2f)] public float Rage_Speed = 4.8f;

    float _nextSlam, _nextVolley;
    bool _dead;
    EnemyController Enemy;


    enum Phase { P1, P2, Rage }
    Phase _phase = Phase.P1;

    void Awake()
    {
        Enemy = GetComponent<EnemyController>();
        if (!Agent) Agent = GetComponent<NavMeshAgent>();
        if (!Health) Health = GetComponent<Health>();
        if (!Animator) Animator = GetComponentInChildren<Animator>();

        if (!Player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) Player = p.transform;
        }

        if (Health)
        {
            Health.OnDie += OnBossDie;
            Health.OnDamaged += OnBossDamaged;
        }
    }

    void OnDestroy()
    {
        if (Health)
        {
            Health.OnDie -= OnBossDie;
            Health.OnDamaged -= OnBossDamaged;
        }
    }

    void Update()
    {
        if (_dead) return;

        UpdatePhaseAndSpeed();

        Vector3 targetPos = GetTargetPosition();
        float dist = Vector3.Distance(transform.position, targetPos);

        if (dist <= DetectRange && dist > AttackRange + StopDistanceBuffer)
        {
            if (Agent && Agent.enabled)
            {
                Agent.isStopped = false;
                Enemy?.SetNavDestination(targetPos); 
            }
            SetAnimMove(true);
        }
        else
        {
            if (Agent) Agent.isStopped = true;
            SetAnimMove(false);
        }

        TryAttacks(dist, targetPos);
    }

    Vector3 GetTargetPosition()
    {
        if (Enemy && Enemy.KnownDetectedTarget)
            return Enemy.KnownDetectedTarget.transform.position;
        if (Player) return Player.position;
        return transform.position;
    }

    void TryAttacks(float dist, Vector3 targetPos)
    {
        float t = Time.time;

        if (dist <= AttackRange && t >= _nextSlam)
        {
            _nextSlam = t + SlamCooldown;
            StartCoroutine(DoSlam());
            return; 
        }

        if (dist <= DetectRange && t >= _nextVolley)
        {
            _nextVolley = t + VolleyCooldown;
            StartCoroutine(DoVolley(targetPos));
        }
    }

    IEnumerator DoSlam()
    {
        if (Animator) Animator.SetTrigger("Slam");
        yield return new WaitForSeconds(SlamWindup);

        var hits = Physics.OverlapSphere(transform.position, SlamRadius, DamageLayers, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            var hp = h.GetComponentInParent<Health>();
            if (hp && hp != Health)
                hp.TakeDamage(SlamDamage, gameObject);
        }
    }

    IEnumerator DoVolley(Vector3 targetPos)
    {
        if (Animator) Animator.SetTrigger("Shoot");
        yield return new WaitForSeconds(VolleyWindup);

        if (!Enemy) yield break;

        for (int i = 0; i < Mathf.Max(1, VolleyCount); i++)
        {
            Enemy.OrientTowards(targetPos);
            Enemy.OrientWeaponsTowards(targetPos);
            Enemy.TryAtack(targetPos); 

            if (VolleyShotInterval > 0f)
                yield return new WaitForSeconds(VolleyShotInterval);
        }
    }

    void UpdatePhaseAndSpeed()
    {
        if (!Health || !Agent) return;

        float hp01 = Mathf.Clamp01(Health.CurrentHealth / Health.MaxHealth);
        Phase p = hp01 <= 0.35f ? Phase.Rage : (hp01 <= 0.70f ? Phase.P2 : Phase.P1);
        if (p != _phase)
        {
            _phase = p;
            if (Animator) Animator.SetInteger("Phase", (int)_phase);
        }

        Agent.speed = _phase switch
        {
            Phase.P1 => P1_Speed,
            Phase.P2 => P2_Speed,
            _ => Rage_Speed
        };
    }

    void SetAnimMove(bool moving)
    {
        
    }

    void OnBossDamaged(float dmg, GameObject source)
    {
    }

    void OnBossDie()
    {
        _dead = true;
        if (Agent) Agent.isStopped = true;
        if (Animator) Animator.SetTrigger("Die");
        enabled = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, DetectRange);
        Gizmos.DrawWireSphere(transform.position, AttackRange);
        Gizmos.DrawWireSphere(transform.position, SlamRadius);
    }
}
