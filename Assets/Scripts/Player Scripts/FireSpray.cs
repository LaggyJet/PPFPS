//Worked on by : Jacob Irvin

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSpray : MonoBehaviour
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
        Vector3 randomVec = new Vector3(transform.forward.x, transform.forward.y + Random.Range(-0.2f, 0.2f), transform.forward.z + Random.Range(-0.2f, 0.2f));
        //moves our projectile forward based on its speed
        rb.velocity = randomVec * (speed + Random.Range(-0.2f, 0.2f));
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
        else if (!other.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }

    }
}
