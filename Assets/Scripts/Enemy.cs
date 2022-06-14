using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
    public enum State {Idle, Chasing, Attacking}     
    State currentState;

    public ParticleSystem deathEffect;

    NavMeshAgent pathfinder;
    Transform target;
    LivingEntity targetEntity;                         
    Material skinMaterial;                             
     
    Color originalColor;                              

    float attackDistance = 0.5f;                        
    float timeBetweenAttacks = 1f;                     
    float damage = 1;

    float nextAttackTime;                              
    float myCollisionRadius;                           
    float targetCollisionRadius;                       

    bool hasTarget;

    private void Awake()
    {
        pathfinder = GetComponent<NavMeshAgent>();
        skinMaterial = GetComponent<Renderer>().material;                 
        originalColor = skinMaterial.color;                              

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;   
            targetEntity = target.GetComponent<LivingEntity>();               
           
            myCollisionRadius = GetComponent<CapsuleCollider>().radius;      
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;

        }
    }

    protected override void Start()                                     
    {
        base.Start();                                                    

        if(hasTarget)
        {
            currentState = State.Chasing;                                    
            targetEntity.OnDeath += OnTargetDeath;                           
            StartCoroutine(UpdatePath());                                    
        }
    }

    public void SetCharacteristics(float moveSpeed, int hitsToKillPlayer, float enemyHealth, Color skinColor)
    {
        pathfinder.speed = moveSpeed;

        if (hasTarget)
        {
            damage = Mathf.Ceil (targetEntity.startingHealth / hitsToKillPlayer);  // расчёт урона для каждого врага
        }
        startingHealth = enemyHealth;

        skinMaterial = GetComponent<Renderer>().material;
        skinMaterial.color = skinColor;
        originalColor = skinMaterial.color;
    }

    public override void TakeHit (float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        if( damage >= health)
        {
            Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, deathEffect.startLifetime);
        }
        base.TakeHit(damage, hitPoint, hitDirection);
    }

    void OnTargetDeath()                             
    {
        hasTarget = false;
        currentState = State.Idle;
    }

    void Update()
    {
        if (hasTarget)
        {
            if (Time.time > nextAttackTime)
            {
                float sqrDistanceToTarget = (target.position - transform.position).sqrMagnitude;             
                if (sqrDistanceToTarget < Mathf.Pow(attackDistance + myCollisionRadius + targetCollisionRadius, 2))            
                {
                    nextAttackTime = Time.time + timeBetweenAttacks;               
                    StartCoroutine(Attack());
                }
            }
        }
    }

    IEnumerator Attack()
    {
        currentState = State.Attacking;
        pathfinder.enabled = false;

        Vector3 originalPosition = transform.position;
        Vector3 directionToTarget = (target.position - transform.position).normalized;  
        Vector3 attackPosition = target.position - directionToTarget * (myCollisionRadius);  
    
        float percent = 0;
        float attackSpeed = 3;

        skinMaterial.color = Color.red;      
        bool hasAppliedDamage = false;

        while(percent <= 1)
        {
            if(percent >= 0.5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }

            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2)+percent) *4;         
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

            yield return null;
        }

        skinMaterial.color = originalColor;    
        currentState = State.Chasing;
        pathfinder.enabled = true;
    }

    IEnumerator UpdatePath()            
    {
        float refreshRate = .25f;         

        while (hasTarget)
        {
            if (currentState == State.Chasing)                         
            {
                Vector3 directionToTarget = (target.position - transform.position).normalized;  
                Vector3 targetPosition = target.position - directionToTarget * (myCollisionRadius * targetCollisionRadius + attackDistance / 2);  

                if (!dead)                                             
                {
                    pathfinder.SetDestination(target.position);         
                }
            } 
            yield return new WaitForSeconds(refreshRate);             
        }
    }
}
