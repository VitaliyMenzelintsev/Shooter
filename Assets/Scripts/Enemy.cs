using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
    public enum State {Idle, Chasing, Attacking}      // �������� ������������ ��������� �����, ����� �� �� ����� ����, ����� ��� �������, � �������
    State currentState;

    NavMeshAgent pathfinder;
    Transform target;
    LivingEntity targetEntity;                         // ������ �� ������� ��������
    Material skinMaterial;                             //���������� ��������� �����
     
    Color originalColor;                               // ���������� ���������� ����� �����

    float attackDistance = 0.5f;                        // ��������� �����
    float timeBetweenAttacks = 1f;                      // ����� ����� �������
    float damage = 1;

    float nextAttackTime;                               // ����� ��������� �����
    float myCollisionRadius;                            // ������ ������������ �����
    float targetCollisionRadius;                       // ������ ������������ ���� (������)

    bool hasTarget;

    protected override void Start()                                       // ���������� ������ Start �� LivingEntity.cs
    {
        base.Start();                                                     // ���������� ����������� Start �� LivingEntity.cs
        pathfinder = GetComponent<NavMeshAgent>();
        skinMaterial = GetComponent<Renderer>().material;                 //������������� ��������� ��� ����, ��� ��� �� �������
        originalColor = skinMaterial.color;                               // �������� ���� - ��� ���� ���������

        if(GameObject.FindGameObjectWithTag("Player") != null)
        {
            currentState = State.Chasing;                                     // �������������� ��������� - �������������
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;    // ���� - ������� ������ � ����� Player
            targetEntity = target.GetComponent<LivingEntity>();               // ������������� ����� ��������
            targetEntity.OnDeath += OnTargetDeath;                            // �������� �� �������

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;       // ������ ������������ ����� ������� ����������
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;

            StartCoroutine(UpdatePath());                                     // �� ������� ��������� ���� ���������� ����
        }
    }

    void OnTargetDeath()                             // ������� ������ ����
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
                float sqrDistanceToTarget = (target.position - transform.position).sqrMagnitude;             // ���������� ����������� ���������� �� ����
                if (sqrDistanceToTarget < Mathf.Pow(attackDistance + myCollisionRadius + targetCollisionRadius, 2))            // ���� ������� ���������� �� ���� ������, ��� ������� ��������� �����
                {
                    nextAttackTime = Time.time + timeBetweenAttacks;               // ����� ��������� ����� = ������� ����� + ����� ����� �������
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
        Vector3 directionToTarget = (target.position - transform.position).normalized;  // ������ ����������� � ����
        Vector3 attackPosition = target.position - directionToTarget * (myCollisionRadius);  // ������� ����
    
        float percent = 0;
        float attackSpeed = 3;

        skinMaterial.color = Color.red;      // ���� ��������, ����� �������
        bool hasAppliedDamage = false;

        while(percent <= 1)
        {
            if(percent >= 0.5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }

            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2)+percent) *4;         // ��������� ������ ��������� ��� ������������� ����� �����
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

            yield return null;
        }

        skinMaterial.color = originalColor;     // ����������� �������� ����� �����
        currentState = State.Chasing;
        pathfinder.enabled = true;
    }

    IEnumerator UpdatePath()              // ���������� Update, �������� ������ � ���� �� ������ ����
    {
        float refreshRate = .25f;         // ������� ����������

        while (hasTarget)
        {
            if (currentState == State.Chasing)                         // ���� ����������� ������, ���� ��������� Chasing
            {
                Vector3 directionToTarget = (target.position - transform.position).normalized;  // ������ ����������� � ����
                Vector3 targetPosition = target.position - directionToTarget * (myCollisionRadius * targetCollisionRadius + attackDistance / 2);  // ������� ����

                if (!dead)                                              // ����� ���� ��� �������, ��� ������ �� ����
                {
                    pathfinder.SetDestination(target.position);         // ����� ���������� ��� ������������ - ������� ���� (������)
                }
            } 
            yield return new WaitForSeconds(refreshRate);               // ��������� ���� ������ �������� refreshRate 
        }
    }
}
