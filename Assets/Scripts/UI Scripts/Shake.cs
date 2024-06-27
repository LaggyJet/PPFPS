// Kheera 

using System.Collections; 
using System.Collections.Generic;
using UnityEngine;

public class Shake : MonoBehaviour
{
    // Determines the Position, Rotation, and Scale of said GameObject in the scene
    Transform targetObject;
    Vector3 initialPosition;
    float shakeDuration = 0f;
    bool isShaking = false;

    // Implementing my 1-10 system 
    [Range(0f, 10f)]public float intensity; 

    public static Shake instance;

    // Start is called before the first frame update
    void Start()
    {
        // In case anyone else would like to use this for other objects
        instance = this;

        // Storing position, rotate, scale in target
        targetObject = GetComponent<Transform>(); 

        // Get Local position of object 
        initialPosition = targetObject.localPosition; 
    }

    
    public void Shaking(float duration)
    {
        
        if (duration > 0)
        {
          // Increase duration 
          shakeDuration += duration;

          if (!isShaking)
          {
           // Begin shaking frames 
           StartCoroutine(DoShake());
          }
        }
       
    }

    IEnumerator DoShake()
    {
        // Start shaking 
        isShaking = true;

        // Stores the current time since the last frame 
        float seconds = Time.time;
        while (Time.time < seconds + shakeDuration)
        {
            if (GameManager.instance != null && !GameManager.instance.isPaused)  
            { 
            // Generate random position, keep local position ( DO NOT ADJUST OBJECT LOCATION COMPLETELY !!!)
            var random = new Vector3(Random.Range(-1f, 1f) * intensity, Random.Range(-1f, 1f) * intensity, 0f);
            // Set random postion for object 
            targetObject.localPosition = initialPosition + random;
            
            }
            // Wait for Next frame 
            yield return null;
        }

        // Stop shaking - routine is finished 
        // Reset 
        targetObject.localPosition = initialPosition;
        isShaking = false;
        shakeDuration = 0f; 
    }

}
