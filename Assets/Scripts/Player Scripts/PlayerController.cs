//Worked on by : Jacob Irvin, Natalie Lubahn, Kheera, Emily Underwood

using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IDamage, IDataPersistence
{
    //this sets up our player controller variable to handle collision
    public CharacterController controller;

    //these variables are game function variables that may likely be changed
    [SerializeField] bool shootProjectile;
    
    [SerializeField] float hp;
    [SerializeField] int speed;
    [SerializeField] int sprintMod;
    [SerializeField] int gravity;
    [SerializeField] int jumpMax;
    [SerializeField] int jumpSpeed;

    [Header("------- HP -------")]
    
    [Range(0f, 10f)] public float hp; 
    float hpBase;

    // Health bar gradual fill 
    [SerializeField] Color fullHealth; 
    [SerializeField] Color midHealth; 
    [SerializeField] Color criticalHealth;

    // HP bar shake
    [Range(0f, 10f)] public float hpDuration;  

    [Header("------- Stamina -------")]

    [Range(0f, 10f)] public float stamina; 
    float staminaBase; 
    
     // stamina bar gradual fill 
    [SerializeField] Color fullstamina; 
    [SerializeField] Color midstamina; 
    [SerializeField] Color criticalstamina;

    // stamina bar shake
    [Range(0f, 10f)] public float stamDuration;   

   

    //these are combat variables
    [SerializeField] float shootDamage;
    [SerializeField] float shootRate;
    [SerializeField] int shootDistance;
    [SerializeField] GameObject shootPosition;
    [SerializeField] GameObject projectile;

    //these are animation variables
    [SerializeField] Animator animate;
    [SerializeField] float animationTransSpeed;


    //these are variables used explicitly in functions
    DamageStats status;
    int jumpCount;
    bool isShooting;
    bool isDead;
    bool isDOT;

    Vector3 moveDir;
    Vector3 playerV;

    [SerializeField] Sprite sprite;

    //variables used for save/load
    public static Vector3 spawnLocation;
    public static Quaternion spawnRotation;
    public static float spawnHp;

    //Audio variables
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] footsteps;
    [SerializeField] float footstepsVol;
    [SerializeField] AudioClip[] hurt;
    [SerializeField] float hurtVol;
    [SerializeField] AudioClip[] attack;
    [SerializeField] float attackVol;
    bool isPlayingSteps;
    bool isSprinting;

    private void Start()
    {
        UnityEngine.UI.Image.DontDestroyOnLoad(GameManager.instance.playerHPBar);
        //tracks our base hp and the current hp that will update as our player takes damage or gets health
        hpBase = hp;
        this.transform.position = Vector3.zero;
        this.transform.rotation = Quaternion.identity;

        if (spawnLocation == Vector3.zero)
        {
            this.transform.position = new Vector3(-18.7507896f, 0.108012557f, 81.6620026f);
            this.transform.rotation = new Quaternion(0, 180.513367f, 0, 0);
            //updates our ui to accurately show the player hp and other information
            updatePlayerUI();
        }
        else
        {
            this.transform.position = spawnLocation;
            this.transform.rotation = spawnRotation;
            hp = spawnHp;
            //updates our ui to accurately show the player hp and other information
            updatePlayerUI();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //runs our movement function to determine the player velocity each frame
        Movement();
        Sprint();

        if (Input.GetButton("Fire1") && !isShooting && !GameManager.instance.isPaused && SceneManager.GetActiveScene().name != "title menu")
        {
            //plays our shooting animation
            animate.SetTrigger("Shoot Fire");
        }

        SecondaryFireCheck();
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
        //runs a check for if player jumps
        if (Input.GetButtonDown("Jump") && GameManager.instance.canJump)
        {
            if (jumpCount < jumpMax)
            {
                jumpCount++;
                playerV.y = jumpSpeed;
            }
        }
        //gets our input and adjusts the players position using a velocity formula
        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * speed * Time.deltaTime);
        playerV.y -= gravity * Time.deltaTime;
        controller.Move(playerV * Time.deltaTime);

        if (controller.isGrounded && moveDir.magnitude > 0.3 && !isPlayingSteps)
        {
            StartCoroutine(playSteps());
        }
    }

    //calculates our speed if the player is sprinting
    void Sprint()
    {
        //when Sprint is pressed apply the sprint modifier variable to our speed variable
        if (Input.GetButtonDown("Sprint"))
        {
            isSprinting = true;
            speed *= sprintMod;
        }
        //when sprint is no longer being pressed we remove the sprint modifier from the speed variable
        else if (Input.GetButtonUp("Sprint"))
        {
            isSprinting = false;
            speed /= sprintMod;
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

    //this function handles everything to do with the player shooting
    void ShootFire()
    {
        //sets shootings variable to true so we can only fire once at a time
        isShooting = true;

        audioSource.PlayOneShot(attack[Random.Range(0, attack.Length)], attackVol);

        //plays our shooting animation
        animate.SetTrigger("Shoot Fire");
        //sets up our collision detection
        if(!shootProjectile)
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDistance))
            {
                Debug.Log(hit.transform.name);

                IDamage dmg = hit.collider.GetComponent<IDamage>();

                if (hit.transform != transform && dmg != null)
                {
                    dmg.TakeDamage(shootDamage);
                }
            }
        }
        //spawns our projectile
        Instantiate(projectile, shootPosition.transform.position, shootPosition.transform.rotation);
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
        hp -= amount;

        if (!isPlayingSteps) //plays hurt sounds
        {
            audioSource.PlayOneShot(hurt[Random.Range(0, hurt.Length)], hurtVol);
        }

        //updates our ui to accurately show the players health
        updatePlayerUI();
        //if health drops below zero run our lose condition
        if(hp <= 0 && !isDead)
        {
            isDead = true;
            GameManager.instance.gameLost();
        }
    }

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

    //called when player runs into spiderwebs
    public void Slow()
    {
        speed = speed / 2;
    }

    //called when player escapes spiderwebs
    public void UnSlow()
    {
        speed = speed * 2;
    }

    //the function for updating our ui
    public void updatePlayerUI()
    {
        if(GameManager.instance.playerHPBar == null)
        {
            Debug.LogError("HELPEE AFJI IM GOING INSANE");
        }

        // Variable for filling bar 
        float healthRatio = (float)hp / hpBase;

        // Storing 
        GameManager.instance.playerHPBar.fillAmount = healthRatio;

            if (healthRatio > 0.5f || GameManager.instance.playerHPBar.color != midHealth) // If health is more than 50% full
            {
                GameManager.instance.playerHPBar.color = Color.Lerp(midHealth, fullHealth, (healthRatio - 0.5f) * 2);
            }
            else // If the health is less than 50%
            {
                GameManager.instance.playerHPBar.color = Color.Lerp(criticalHealth, midHealth, healthRatio * 2);
                Shake.instance.Shaking(hpDuration);  
            }
    }
    
    public void Respawn()
    {
        this.transform.position = GameManager.instance.playerLocation;
        hp = hpBase;
        updatePlayerUI();
    }
    public void LoadData(GameData data)
    {
        spawnLocation = data.playerPos;
        spawnRotation = data.playerRot;
        spawnHp = data.playerHp;
        
    }
    public void SaveData(ref GameData data)
    {
        data.playerPos = this.transform.position;
        data.playerRot = this.transform.rotation;
        data.playerHp = hp;
    }
}
