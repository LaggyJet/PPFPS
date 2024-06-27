// Worked on by - Joshua Furber
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour {
    public static EnemyManager Instance;

    // Enemy Type, Close Range, Attacking
    readonly List<Tuple<EnemyLimiter, List<int>, List<int>>> currentEnemies = new();
    readonly List<int> enemiesDead;

    void Awake() { Instance = this; }

    public void AddEnemyType(EnemyLimiter type) {
        if (!currentEnemies.Any(tuple => tuple.Item1.Equals(type)))
            currentEnemies.Add(new Tuple<EnemyLimiter, List<int>, List<int>>(type, new List<int>(type.closeRangeAmount), new List<int>(type.attackAmount)));
    }

    public void UpdateKillCounter(EnemyLimiter type) { enemiesDead[GetEnemyIndex(type)]++; }

    public int GetKilledEnemyCount(EnemyLimiter type) { return enemiesDead[GetEnemyIndex(type)]; }

    int GetEnemyIndex(EnemyLimiter type) { return currentEnemies.FindIndex(tuple => tuple.Item1.Equals(type)); }

    public bool IsClose(EnemyLimiter type, int id) { return currentEnemies[GetEnemyIndex(type)].Item2.Contains(id); }

    public bool CanBeClose(EnemyLimiter type) { return (type.closeRangeAmount == 0 || (currentEnemies[GetEnemyIndex(type)].Item2.Count() < type.closeRangeAmount)); }

    public void AddCloseEnemy(EnemyLimiter type, int id) { currentEnemies[GetEnemyIndex(type)].Item2.Add(id); }

    public void RemoveCloseEnemy(EnemyLimiter type, int id) { currentEnemies[GetEnemyIndex(type)].Item2.Remove(id); }

    public bool CanAttack(EnemyLimiter type) { return (type.attackAmount == 0 || currentEnemies[GetEnemyIndex(type)].Item3.Count() < type.attackAmount); }

    public void AddAttackEnemy(EnemyLimiter type, int id) { currentEnemies[GetEnemyIndex(type)].Item3.Add(id); }

    public void RemoveAttackEnemy(EnemyLimiter type, int id) { currentEnemies[GetEnemyIndex(type)].Item3.Remove(id); }    
}