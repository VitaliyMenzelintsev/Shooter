using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask collisionMask;
    float speed = 10;
    float damage = 1;
    float lifeTime = 3;
    float skinWidth = 0.1f;                   

    void Start()
    {
        Destroy(gameObject, lifeTime);         

        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, 0.1f, collisionMask); 
        if(initialCollisions.Length > 0)
        {
            OnHitObject(initialCollisions[0], transform.position, transform.forward);   
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
        Ray ray = new Ray(transform.position, transform.forward);  
        RaycastHit hit;


       
        if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide))
        {
            OnHitObject(hit.collider, hit.point, transform.forward);   
        }
    }
 
    void OnHitObject(Collider ñ, Vector3 hitPoint, Vector3 hitDirection)
    {
        IDamageable damageableObject = ñ.GetComponent<IDamageable>();
        if (damageableObject != null)
        {
            damageableObject.TakeHit(damage, hitPoint, transform.forward);   
        }
        GameObject.Destroy(gameObject);
    }
}
