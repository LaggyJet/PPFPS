// Worked on by - Joshua Furber
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpiderController : MonoBehaviour, IDamage {
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator anim;
    [SerializeField] float hp;
    [SerializeField] int faceTargetSpeed, distanceFromPlayer, spitCooldown;
    [SerializeField] GameObject spitEffectPS, acidPuddle;
    [SerializeField] DamageStats type;
    [SerializeField] GameObject spider;
    [SerializeField] int spawnRate, spawnAmount;

    public bool isAttacking, wasKilled, isSpawningSpiders, onCooldown, isDOT;
    DamageStats status;
    Vector3 playerDirection;
    float currentAngle;

    void Start() {
        isAttacking = wasKilled = isSpawningSpiders = onCooldown = isDOT = false;
        acidPuddle.AddComponent<AcidPuddle>().SetDamageType(type);
        GameManager.instance.updateEnemy(1);
        agent.speed = distanceFromPlayer * 0.1875f;
        StartCoroutine(CirclePlayer());
    }

    void Update() {
        playerDirection = GameManager.instance.player.transform.position - transform.position;

        if (!wasKilled) {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(playerDirection), Time.deltaTime * faceTargetSpeed);

            if (!isSpawningSpiders && !isAttacking)
                StartCoroutine(SpawnSpiders());
        }
        
    }

    IEnumerator SpawnSpiders() {
        isSpawningSpiders = true;
        for (int i = 0; i < spawnAmount; i++) {
            Instantiate(spider, transform.position, Quaternion.LookRotation(playerDirection) * Quaternion.Euler(0, 180, 0));
            yield return new WaitForSeconds(1f);
        }
        yield return new WaitForSeconds(spawnRate);
        isSpawningSpiders = false;
    }

    IEnumerator CirclePlayer() {
        float angleAdjustment = (Mathf.PI * 2) / 360;
        float thresh = 1f;
        while (true) {
            if (isAttacking) {
                agent.SetDestination(transform.position);
                yield return null;
            }
            else
            {
                float xOffset = Mathf.Sin(currentAngle) * distanceFromPlayer;
                float zOffset = Mathf.Cos(currentAngle) * distanceFromPlayer;
                var target = new Vector3(GameManager.instance.player.transform.position.x + xOffset, GameManager.instance.player.transform.position.y, GameManager.instance.player.transform.position.z + zOffset);
                agent.SetDestination(target);
                while (Vector3.Distance(agent.transform.position, target) > thresh)
                    yield return null;
                currentAngle += angleAdjustment * Time.deltaTime;
                if (currentAngle > Mathf.PI * 2)
                    currentAngle -= Mathf.PI * 2;
            }
        }
    }

    IEnumerator Spit() {
        isAttacking = onCooldown = true;
        yield return new WaitForSeconds(0.1f);
        acidPuddle.GetComponent<Collider>().enabled = true;
        anim.enabled = false;
        spitEffectPS.GetComponent<ParticleSystem>().Play();
        yield return new WaitForSeconds(4);
        anim.enabled = true;
        acidPuddle.GetComponent<Collider>().enabled = false;
        anim.SetTrigger("Circle");
        isAttacking = false;
        yield return new WaitForSeconds(spitCooldown);
        onCooldown = false;
    }

    public void TakeDamage(float amount) {
        hp -= amount;
        if (hp > 0)
            StartCoroutine(FlashDamage());

        if (hp <= 0 && !wasKilled) {
            GameManager.instance.updateEnemy(-1);
            gameObject.GetComponent<Collider>().enabled = false;
            StartCoroutine(DeathAnimation());
            wasKilled = true;
            Invoke("Win", 2); //done to prevent glitchy menu
        }

        if (!isAttacking && !onCooldown)
            StartCoroutine(Spit());
    }

    public void Afflict(DamageStats type) { 
        status = type;
        if (!isDOT)
            StartCoroutine(DamageOverTime());
    }

    IEnumerator DamageOverTime() {
        isDOT = true;
        for (int i = 0; i < status.length; i++) {
            TakeDamage(status.damage);
            yield return new WaitForSeconds(1);
        }
        isDOT = false;
    }

    bool IsSpitParticles(Transform transform) {
        while (transform != null) {
            if (transform.name == "SpitPosition")
                return true;
            transform = transform.parent;
        }
        return false;
    }

    IEnumerator DeathAnimation() {
        yield return new WaitForSeconds(0.2f);
        agent.isStopped = true;
        agent.SetDestination(transform.position);
        agent.radius = 0;
        anim.SetTrigger("Death");
        var renderers = new List<Renderer>();
        Renderer[] childRenders = transform.GetComponentsInChildren<Renderer>();
        renderers.AddRange(childRenders);
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length * 2);
        foreach (Renderer render in renderers) {
            if (IsSpitParticles(render.transform))
                continue;
            float newAlpha = render.material.GetFloat("_Alpha");
            while (newAlpha > 0) {
                newAlpha -= 0.5f * Time.deltaTime;
                render.material.SetFloat("_Alpha", newAlpha);
                yield return null;
            }
        }
        Destroy(gameObject);
    }

    IEnumerator FlashDamage() {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = new Color(0.5f, 0.5f, 0.5f, 1);
    }

    private void Win()
    {
        GameManager.instance.gameWon();
    }
}