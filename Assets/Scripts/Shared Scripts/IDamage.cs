// Worked on by - Jacob Irvin, Joshua Furber
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamage {
    public void TakeDamage(float damage);

    public void Afflict(DamageStats type);
}