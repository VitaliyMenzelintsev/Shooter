using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask collisionMask;
    float speed = 10;
    float damage = 1;
    float lifeTime = 3;
    float skinWidth = 0.1f;                   // ширина пули

    void Start()
    {
        Destroy(gameObject, lifeTime);         // уничтожить пулю после окончания lifeTime

        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, 0.1f, collisionMask); // массив коллайдеров в которые попала пуля
        if(initialCollisions.Length > 0) // если массив насчитывает более 0, то уничтожается пуля и запускается метод попадания
        {
            OnHitObject(initialCollisions[0], transform.position, transform.forward);     // передача в метод OnHitObject первого коллайдера с которым столкнётся пуля
        }
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    void Update()
    {
        float moveDistance = speed * Time.deltaTime;
        CheckCollisions(moveDistance);
        transform.Translate(Vector3.forward * moveDistance);
    }

    void CheckCollisions(float moveDistance)
    {
        Ray ray = new Ray(transform.position, transform.forward);  // луч от позиции в прямом направлении
        RaycastHit hit;


        // если луч попадает в тригеррный коллайдер
        if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide))
        {
            OnHitObject(hit.collider, hit.point, transform.forward);    // запуск метода попадания по обьекту
        }
    }
 
    void OnHitObject(Collider с, Vector3 hitPoint, Vector3 hitDirection)
    {
        IDamageable damageableObject = с.GetComponent<IDamageable>();
        if (damageableObject != null)
        {
            damageableObject.TakeHit(damage, hitPoint, transform.forward);   // повреждаемому обьекту наносится урон
        }
        GameObject.Destroy(gameObject);
    }
}
