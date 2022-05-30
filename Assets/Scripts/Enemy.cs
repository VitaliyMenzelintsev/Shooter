using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
    public enum State {Idle, Chasing, Attacking}      // введение перечислени€ состо€ний врага, чтобы он не искал путь, когда уже атакует, к примеру
    State currentState;

    NavMeshAgent pathfinder;
    Transform target;
    LivingEntity targetEntity;                         // ссылка на целевую сущность
    Material skinMaterial;                             //обь€вление материала врага
     
    Color originalColor;                               // обь€вление исходногго цвета врага

    float attackDistance = 0.5f;                        // дистанци€ атаки
    float timeBetweenAttacks = 1f;                      // врем€ между атаками
    float damage = 1;

    float nextAttackTime;                               // врем€ следующей атаки
    float myCollisionRadius;                            // радиус столкновени€ ¬рага
    float targetCollisionRadius;                       // радиус столкновени€ цели (»грока)

    bool hasTarget;

    protected override void Start()                                       // перезапись метода Start из LivingEntity.cs
    {
        base.Start();                                                     // выполнение функционала Start от LivingEntity.cs
        pathfinder = GetComponent<NavMeshAgent>();
        skinMaterial = GetComponent<Renderer>().material;                 //инициализаци€ материала как того, что уже на обьекте
        originalColor = skinMaterial.color;                               // исходный цвет - это цвет материала

        if(GameObject.FindGameObjectWithTag("Player") != null)
        {
            currentState = State.Chasing;                                     // первоначальное состо€ние - преследование
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;    // цель - игровой обьект с тэгом Player
            targetEntity = target.GetComponent<LivingEntity>();               // инициализаци€ живой сущности
            targetEntity.OnDeath += OnTargetDeath;                            // подписка на событие

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;       // радиус столкновени€ равен радиусу коллайдера
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;

            StartCoroutine(UpdatePath());                                     // со стартом запускаем цикл обновлени€ пути
        }
    }

    void OnTargetDeath()                             // событие смерть цели
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
                float sqrDistanceToTarget = (target.position - transform.position).sqrMagnitude;             // вычисление квадратного рассто€ни€ до цели
                if (sqrDistanceToTarget < Mathf.Pow(attackDistance + myCollisionRadius + targetCollisionRadius, 2))            // если квадрат рассто€ни€ до цели меньше, чем квадрат дистанции атаки
                {
                    nextAttackTime = Time.time + timeBetweenAttacks;               // врем€ следующей атаки = текущее врем€ + врем€ между атаками
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
        Vector3 directionToTarget = (target.position - transform.position).normalized;  // вектор направлен€и к цели
        Vector3 attackPosition = target.position - directionToTarget * (myCollisionRadius);  // позици€ цели
    
        float percent = 0;
        float attackSpeed = 3;

        skinMaterial.color = Color.red;      // враг краснеет, когда атакует
        bool hasAppliedDamage = false;

        while(percent <= 1)
        {
            if(percent >= 0.5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }

            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2)+percent) *4;         // описываем график гиперболы дл€ моделировани€ атаки врага
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

            yield return null;
        }

        skinMaterial.color = originalColor;     // возвращение прежнего цвета врагу
        currentState = State.Chasing;
        pathfinder.enabled = true;
    }

    IEnumerator UpdatePath()              // разгружаем Update, обновл€€ данные о цели не каждый кадр
    {
        float refreshRate = .25f;         // частота обновлени€

        while (hasTarget)
        {
            if (currentState == State.Chasing)                         // путь обновл€етс€ только, если состо€ние Chasing
            {
                Vector3 directionToTarget = (target.position - transform.position).normalized;  // вектор направлен€и к цели
                Vector3 targetPosition = target.position - directionToTarget * (myCollisionRadius * targetCollisionRadius + attackDistance / 2);  // позици€ цели

                if (!dead)                                              // поиск пути при условии, что обьект не мЄртв
                {
                    pathfinder.SetDestination(target.position);         // точка назначание дл€ Ќавћешјгента - позици€ цели (игрока)
                }
            } 
            yield return new WaitForSeconds(refreshRate);               // обновл€ем цель каждое значение refreshRate 
        }
    }
}
