// Worked on by - Joshua Furber
using UnityEngine;

public class MeleeWeaponController : MonoBehaviour {
    float damage_;
    bool canDOT;
    DamageStats stats_;

    public void SetWeapon(float damage, bool dot, DamageStats type)
    {
        damage_ = damage;
        canDOT = dot;
        stats_ = type;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;

        IDamage damageCheck = other.GetComponent<IDamage>();
        if (damageCheck != null && !other.CompareTag("Player"))
        {
            damageCheck.TakeDamage(damage_);
            if (canDOT)
                damageCheck.Afflict(stats_);
        }
    }
}
