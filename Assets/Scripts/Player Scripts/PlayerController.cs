//Worked on by : Jacob Irvin, Natalie Lubahn, Kheera, Emily Underwood

using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IDamage, IDataPersistence
{
     public static PlayerController instance; 

    //this sets up our player controller variable to handle collision
    public CharacterController controller;

    //these variables are game function variables that may likely be changed
    
    [SerializeField] float speed;
    [SerializeField] int sprintMod;
    [SerializeField] int gravity;
    [SerializeField] int jumpMax;
    [SerializeField] int jumpSpeed;
    [SerializeField] public GameObject shootPosition;
    [SerializeField] public GameObject[] combatObjects;

    [Header("------- HP -------")]
    
    [Range(0f, 10f)] public float hp; 
    float hpBase;

    // Health bar gradual fill 
    [SerializeField] Color fullHealth; 
    [SerializeField] Color midHealth; 
    [SerializeField] Color criticalHealth;

    // HP bar shake
    [Range(0f, 10f)] public float hpShakeDuration;  

    [Header("------- Stamina -------")]

    [Range(0f, 10f)] public float stamina; 
    [Range(0f, 50f)] public float staminaRegenerate;  
    float staminaBase; 
    public Coroutine staminaCor = null;
    
    
     // stamina bar gradual fill 
    [SerializeField] Color fullstamina; 
    [SerializeField] Color midstamina; 
    [SerializeField] Color criticalstamina;

    // stamina bar shake
    [Range(0f, 10f)] public float stamShakeDuration;   

    //these are animation variables
    [SerializeField] public Animator animate;
    [SerializeField] float animationTransSpeed;

    //these are variables used explicitly in functions
    DamageStats status;
    int jumpCount;
    bool isDead;
    bool isDOT;

    Vector3 moveDir;
    Vector3 playerV;

    [SerializeField] Sprite sprite;

    //variables used for save/load
    public static Vector3 spawnLocation;
    public static Quaternion spawnRotation;
    public static float spawnHp;
    public static float spawnStamina;


    [Header("------ Audio ------")]

    //Audio variables
    [SerializeField] public AudioSource audioSource;
    [SerializeField] AudioClip[] footsteps;
    [SerializeField] float footstepsVol;
    [SerializeField] AudioClip[] hurt;
    [SerializeField] float hurtVol;
    [SerializeField] public AudioClip[] attack;
    [SerializeField] float attackVol;

    [Header("------ Sprint Audio ------")]
    [SerializeField] public AudioSource sprintAudioSource;
    [SerializeField] public AudioClip sprintSound;
    [SerializeField] float sprintVol;
    [SerializeField] public AudioClip[] noSprint;
    [SerializeField] float noSprintVol; 

    [Header("------ Stamina HP Audio ------")]
    [SerializeField] public AudioSource staminaAudioSource;
    [SerializeField] public AudioClip[] noHP;
    [SerializeField] public float noHPvol;
    [SerializeField] public AudioClip[] noAttack;
    [SerializeField] public float noAttackVol;
    
    bool isPlayingSteps;
    bool isSprinting;
    public bool isPlayingStamina;  
    public bool isPlayingNoSprinting;
    public bool isPlayingNoHP = false;
    private bool isRegenerating = false;

    [Header("------ Classes ------")]
    //class variables
    public Class_Mage mage;
    public Class_Warrior warrior;
    public Class_Archer archer;

    private void Start()
    {
        mage = this.GetComponent<Class_Mage>();
        warrior = this.GetComponent<Class_Warrior>();
        archer = this.GetComponent<Class_Archer>();
        mage.enabled = false;
        warrior.enabled = false;
        archer.enabled = false;

        //tracks our base hp and the current hp that will update as our player takes damage or gets health
        hpBase = hp;
        staminaBase = stamina;
        this.transform.position = Vector3.zero;
        this.transform.rotation = Quaternion.identity;

        if (spawnLocation == Vector3.zero)
        {
            this.transform.position = GameManager.playerLocation;
            this.transform.rotation = GameManager.instance.player.transform.rotation ;
            //updates our ui to accurately show the player hp and other information
            updatePlayerUI();
        }
        else
        {
            
            GameManager.playerLocation = spawnLocation;
            this.transform.position = spawnLocation;
            this.transform.rotation = spawnRotation;
            hp = spawnHp;
            stamina = spawnStamina;
            Physics.SyncTransforms();
            //updates our ui to accurately show the player hp / stamina and other information
            updatePlayerUI();
            spawnLocation = Vector3.zero;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //runs our movement function to determine the player velocity each frame
        Movement();
        // Regenerating over time ( can be adjusted in unity )
        RegenerateStamina();

    }

    //methods for key binding/controls
    public void OnMove(InputAction.CallbackContext ctxt)
    {
        Vector2 newMoveDir = ctxt.ReadValue<Vector2>();
        moveDir.x = newMoveDir.x;
        moveDir.z = newMoveDir.y;
       
    }
    public void OnJump(InputAction.CallbackContext ctxt)
    {
        if (ctxt.performed && GameManager.instance.canJump)
        {
            if (jumpCount < jumpMax)
            {
                jumpCount++;
                playerV.y = jumpSpeed;
            }
        }
        controller.Move(moveDir * speed * Time.deltaTime);
        playerV.y -= gravity * Time.deltaTime;
        controller.Move(playerV * Time.deltaTime);

    }
    public void OnSprint(InputAction.CallbackContext ctxt)
    {
        if(stamina > 0)
        {
           if(ctxt.performed)
        {
            if (!isSprinting)
            {
                isSprinting = true;
                speed *= sprintMod; 
                SubtractStamina(0.5f);
                sprintAudioSource.clip = sprintSound;
                sprintAudioSource.volume = sprintVol;
                sprintAudioSource.Play();
            }
        }
        else if(ctxt.canceled)
        {
            if (isSprinting)
            {
                isSprinting = false;
                speed /= sprintMod;
                
            }
        }
        }
        else 
        { 
            StopSprinting();

            // Checking for audio ( preventing looping on sounds )
            if(!sprintAudioSource.isPlaying)
            {
                // Playing out of stamina if sprinting is not allowed 
             sprintAudioSource.PlayOneShot(noSprint[Random.Range(0, noSprint.Length)], noSprintVol);
             isPlayingNoSprinting = true;
            }
            
            isPlayingNoSprinting = sprintAudioSource.isPlaying;
            Debug.Log("No Stamina poo :(");    
        }
        
    }
    public void OnAbility1(InputAction.CallbackContext ctxt)
    {
        if (ctxt.performed)
        {
            Debug.Log("stayc girls its going down!! (testing)");
        }
    }
    public void OnPrimaryFire(InputAction.CallbackContext ctxt)
    {
        if (mage.enabled)
        {
            mage.OnPrimaryFire(ctxt);
        }
        else if (warrior.enabled)
        {
            warrior.OnPrimaryFire(ctxt);
        }
        else if (archer.enabled)
        {
            archer.OnPrimaryFire(ctxt);
        }
    }
    public void OnSecondaryFire(InputAction.CallbackContext ctxt)
    {
        if (mage.enabled)
        {
            mage.OnSecondaryFire(ctxt);
        }
        else if (warrior.enabled)
        {
            warrior.OnSecondaryFire(ctxt);
        }
        else if (archer.enabled)
        {
            archer.OnSecondaryFire(ctxt);
        }
    }

    //calculates the player movement
    void Movement()
    {
        //makes sure gravity doesn't build up on a grounded player
        if (controller.isGrounded)
        {
            playerV = Vector3.zero;
            jumpCount = 0;
        }
        //gets our input and adjusts the players position using a velocity formula
        Vector3 movement = moveDir.x * transform.right + moveDir.z * transform.forward;
        controller.Move(movement * speed * Time.deltaTime);
        playerV.y -= gravity * Time.deltaTime;
        controller.Move(playerV * Time.deltaTime);

        if (controller.isGrounded && movement.magnitude > 0.3 && !isPlayingSteps)
        {
            StartCoroutine(playSteps());
        }
    }

    


// // Added to fix sprinting not stopping on button up 
private void StopSprinting()
{
    if (isSprinting)
    {
        isSprinting = false;
        speed /= sprintMod;
    }
}
    

    public void SetAnimationTrigger(string triggerName)
    {
        animate.SetTrigger(triggerName);
    }
    public void SetAnimationBool(string boolName, bool state)
    {
        animate.SetBool(boolName, state);
    }

    public void PlaySound(char context) // A for attack, 
    {
        switch (context)
        {
            case 'A':
                audioSource.PlayOneShot(attack[Random.Range(0, attack.Length)], attackVol);
                break;

        }
    }
    IEnumerator playSteps() //playing footsteps sounds
    {
        isPlayingSteps = true;
        audioSource.PlayOneShot(footsteps[Random.Range(0, footsteps.Length)], footstepsVol);

        if (!isSprinting)
        {
            yield return new WaitForSeconds(0.3f);
        }
        else
        {
            yield return new WaitForSeconds(0.1f);
        }
        isPlayingSteps = false;
    }

    private void OnParticleCollision(GameObject other)
    {
        int damage = 1;
        //when encountering a collision trigger it checks for component IDamage
        IDamage dmg = other.GetComponent<IDamage>();

        //if there is an IDamage component we run the inside code
        if (dmg != null && !other.gameObject.CompareTag("Player"))
        {
            //deal damage to the object hit
            dmg.TakeDamage(damage);
            //destroy our projectile
            Destroy(gameObject);
        }
    }


    public void Afflict(DamageStats type)
    {
        status = type;
        if (!isDOT)
            StartCoroutine(DamageOverTime());
    }

    IEnumerator DamageOverTime()
    {
        isDOT = true;
        for (int i = 0; i < status.length; i++)
        {
            TakeDamage(status.damage);
            yield return new WaitForSeconds(1);
        }
        isDOT = false;
    }

    //this function happens when the player is called to take damage
    public void TakeDamage(float amount)
    {
        //subtract the damage from the player
        hp -= 0.5f; 

        if (!isPlayingSteps) //plays hurt sounds
        {
            audioSource.PlayOneShot(hurt[Random.Range(0, hurt.Length)], hurtVol);
        }

        //updates our ui to accurately show the players health / stamina 
        updatePlayerUI();
        //if health drops below zero run our lose condition
        if(hp <= 0 && !isDead)
        {
            isDead = true;
            GameManager.instance.gameLost();
            isDead = false;
        }
    }

    // The function for updating HP bar
    //called when player picks up a health potion
    public void AddHP(float amount)
    {
        if (hp + amount > hpBase) { //added amount would exceed max hp
            hp = hpBase; //set to max hp
        } else
        {
            hp += amount; //add amount to hp
        }
        updatePlayerUI();
    }

    // Subtract & add function for stamina
    public void AddStamina(float amount)
    {
        if (stamina + amount > staminaBase) 
        { 
            stamina = staminaBase; 
        }
         else if(stamina + amount > 10) // Not going above ten 
        {
            stamina = 10;
        }
        else
        {
            stamina += amount; 
        }
        updatePlayerUI();
    }

    public void SubtractStamina(float amount) 
    {
        if (stamina - amount > staminaBase) 
        { 
            stamina = staminaBase; 
        } 
        else if(stamina - amount < 0) // Not going below zero
        {
            stamina = 0;
        }
        else
        {
            stamina -= amount; 
        }

        updatePlayerUI();
    }


    // Regenerate stamina 
    private void RegenerateStamina()
    {
       if(stamina < staminaBase && !isRegenerating)
       {
           StartCoroutine(RegenStaminaDelay());
       }
    }

    // Preventing stamina from regenerating too fast
    private IEnumerator RegenStaminaDelay()
    {
       isRegenerating = true; 

       yield return new WaitForSeconds(5);

       if(stamina < staminaBase)
       {
          stamina += staminaRegenerate * Time.deltaTime;

          if(stamina > staminaBase)
          {
            stamina = staminaBase;
          }

          updatePlayerUI();
          yield return null;
       }

       isRegenerating = false;

    }

    
    //called when player runs into spiderwebs
    public void Slow()
    {
        speed = speed / 7;
    }

    //called when player escapes spiderwebs
    public void UnSlow()
    {
        speed = speed * 7;
    }

    //the function for updating our ui
    public void updatePlayerUI()
    {
        // Variable for filling bar 
        float healthRatio = (float)hp / hpBase;
        float staminaRatio = (float)stamina / staminaBase; 

        // Storing 
        GameManager.instance.playerHPBar.fillAmount = healthRatio; 
        GameManager.instance.staminaBar.fillAmount = staminaRatio;

      
       
            // If health is more than 50% full
            if (healthRatio > 0.5f || GameManager.instance.playerHPBar.color != midHealth) 
            {
                GameManager.instance.playerHPBar.color = Color.Lerp(midHealth, fullHealth, (healthRatio - 0.5f) * 2);
                isPlayingNoHP = false;
            }
            else // If the health is less than 50%
            {
                GameManager.instance.playerHPBar.color = Color.Lerp(criticalHealth, midHealth, healthRatio * 2);

                if(!isPlayingNoHP)
                {
                    if(!staminaAudioSource.isPlaying)
                    {
                        // Playing heart beat for low HP 
                        staminaAudioSource.PlayOneShot(noHP[Random.Range(0, noHP.Length)], noHPvol);
                        isPlayingNoHP = true;
                    }
                }

                isPlayingNoHP = staminaAudioSource.isPlaying;
                Debug.Log("No HP :(");
               
                // if(healthRatio <= 0.5f )
                // {
                //    Shake.instance.Shaking(hpShakeDuration); 
                // }
                
            }
       
       
       
            // If stamina is more than 50% full 
            if (staminaRatio > 0.5f || GameManager.instance.staminaBar.color != midstamina) 
            {
                GameManager.instance.staminaBar.color = Color.Lerp(midstamina, fullstamina, (staminaRatio - 0.5f) * 2); 
            }
            else // If the stamina is less than 50%
            {
                GameManager.instance.staminaBar.color = Color.Lerp(criticalstamina, midstamina, staminaRatio * 2);
                if(staminaRatio <= 0.5f){
                   Shake.instance.Shaking(stamShakeDuration); 
                }
                
            }
       
    }
    
    
    public void Respawn()
    {
        this.transform.position = GameManager.playerLocation;
        hp = hpBase;
        stamina = staminaBase;
        updatePlayerUI(); 
    }
    public void LoadData(GameData data)
    {
        spawnLocation = data.playerPos;
        spawnRotation = data.playerRot;
        spawnHp = data.playerHp; 
        spawnStamina = data.playerStamina;
    }
    public void SaveData(ref GameData data)
    {
        data.playerPos = this.transform.position;
        data.playerRot = this.transform.rotation;
        data.playerHp = hp;
        data.playerStamina = stamina;
    }
}
