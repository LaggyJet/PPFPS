//worked on by - natalie lubahn
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        DataPersistenceManager.Instance.SaveGame();
        GameManager.instance.playerLocation = other.transform.position;
    }
}
