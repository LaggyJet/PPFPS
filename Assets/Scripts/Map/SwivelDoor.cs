using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public class SwivelDoor : MonoBehaviour
{
    [SerializeField] float openAngle;
    [SerializeField] float closeAngle;
    [SerializeField] float openSpeed;
    [SerializeField] float closeSpeed;
    [SerializeField] int limit;
    int count;
    public bool close = false;
    public bool test;
    private bool isOpen = false;
    private bool isMoving = false;
    private bool closeSoundPlayed = false;


    private void Update() {
        if (count >= limit|| test)
        {
            // If door is closed 
            if(!isOpen && !isMoving)
            {
               OpenDoorSound();
            }

            OpenDoor();
        }   
        // If door is open
        else if(close)
        {
            if(closeSoundPlayed == false)
            {
               CloseDoorSound();
               closeSoundPlayed = true;
            }
            
            CloseDoor();
        }
    }

    void CloseDoor()
    {
        Quaternion rotation = Quaternion.AngleAxis(closeAngle, transform.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, closeSpeed);

        // Need this check for sound
        isMoving = false;
        closeSoundPlayed = true;
    }
    void OpenDoor()
    {
        close = false;
        Quaternion rotation = Quaternion.AngleAxis(openAngle, transform.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, openSpeed);

        // Need this check for sound
        isMoving = false;
    }

    public void SwivelTo(float angle_,  float speed_)
    {
        Quaternion rotation = Quaternion.AngleAxis(angle_, transform.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, speed_);
    }

    public void Increment(int amount)
    {
        count += amount;
    }

    // Needed these for checks & sound not replaying on doors
    void CloseDoorSound()
    {
      if(!PlayerController.instance.isPlayingDoorCloseAud && !PlayerController.instance.isPlayingDoorOpenAud)
        {
            PlayerController.instance.PlayDoorCloseAud();
        }
        
        isOpen = true;
        isMoving = true;
    }

    void OpenDoorSound()
    {
        if(!PlayerController.instance.isPlayingDoorCloseAud && !PlayerController.instance.isPlayingDoorOpenAud)
        {
            PlayerController.instance.PlayDoorOpenAud();
        }
        
       isOpen = true;
       isMoving = true;
    }
}
