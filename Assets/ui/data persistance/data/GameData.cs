//worked on by - natalie lubahn
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class GameData
{
    //public variables for serializing data from the other scripts
  //  public int enemyCount = 0;
    public Vector3 playerPos;
    public Quaternion playerRot;
    public float playerHp;

    public GameData()  //constructor - values are the initial values when starting a game
    {
            //this.enemyCount = GameManager.enemyCount;
            this.playerPos = Vector3.zero;
            this.playerRot = Quaternion.identity;
            this.playerHp = 0;
    }
}