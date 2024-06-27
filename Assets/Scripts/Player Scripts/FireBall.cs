//Worked on by : Jacob Irvin

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : MonoBehaviour
{
    //the body of our projectile that will handle our physics
    [SerializeField] Rigidbody rb;

    //game variables that may be tweaked
    [SerializeField] float damage;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;
    [SerializeField] DamageStats type;


    // Start is called before the first frame update
    void Start()
    {
        //moves our projectile forward based on its speed
        rb.velocity = transform.forward * speed;
        //after being alive so long our projectile will die
        Destroy(gameObject, destroyTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        //when encountering a collision trigger it checks for component IDamage
        IDamage dmg = other.GetComponent<IDamage>();

        //if there is an IDamage component we run the inside code
        if (dmg != null && !other.gameObject.CompareTag("Player"))
        {
            //deal damage to the object hit
            dmg.TakeDamage(damage);
            dmg.Afflict(type);
            //destroy our projectile
            Destroy(gameObject);
        }
        else if (!other.gameObject.CompareTag("Player") && !other.isTrigger)
        {
            Destroy(gameObject);
        }

    }
}
