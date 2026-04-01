using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SnakeController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float steerSpeed = 180f;
    [SerializeField] int tailGap = 100;
    [SerializeField, Min(0f)] float tailDisappearInterval = 2f;
    [SerializeField] ParticleSystem deadParticle;

    List<GameObject> tailSegments = new List<GameObject>();
    Renderer[] cachedHeadRenderers;
    Collider cachedHeadCollider;
    ParticleSystem deadParticleInstance;

    ObjectPooler pooler;
    Transform currentTailTarget;
    
    bool isDead;

    void Start()
    {
        cachedHeadRenderers = GetComponentsInChildren<Renderer>(true);
        cachedHeadCollider = GetComponent<Collider>();
        isDead = false;
        pooler = ObjectPooler.Instance;
        currentTailTarget = transform;
    }

    void Update()
    {
        MoveHead();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Treat"))
        {
            Vector3 spawnPosition = currentTailTarget.position - transform.forward * (tailGap + 1);
            spawnPosition.y += 100f;
            GameObject tailObject = pooler.SpawnFromPool("Tail", spawnPosition, transform.rotation);

            TailController tailController = tailObject.GetComponent<TailController>();

            tailController.Initialize(currentTailTarget, moveSpeed, tailGap);
            currentTailTarget = tailObject.transform;
            tailSegments.Add(tailObject);
        }
        else if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Tail"))
        {
            Dead();
            
        }
    }

    void MoveHead()
    {
        transform.position += moveSpeed * Time.deltaTime * transform.forward * (isDead ? 0 : 1);

        float steerDirection = Input.GetAxis("Horizontal");
        transform.Rotate(steerDirection * steerSpeed * Time.deltaTime * Vector3.up * (isDead ? 0 : 1f));
    }


    void Dead()
    {
        if (isDead) return;

        isDead = true;
        StartCoroutine(HandleDeathSequence());
    }

    IEnumerator HandleDeathSequence()
    {
        SpawnParticle(transform.gameObject);
        HideHead();
        yield return CollapseTailRoutine();
        Invoke(nameof(ReloadScene), 1f);
    }

    void HideHead()
    {
        foreach (Renderer renderer in cachedHeadRenderers)
        {
            renderer.enabled = false;
        }

        cachedHeadCollider.enabled = false;
    }

    void SpawnParticle(GameObject pos)
    {
        deadParticleInstance = Instantiate(deadParticle, pos.transform.position, Quaternion.identity);
    }

    IEnumerator CollapseTailRoutine()
    {
        if (tailSegments.Count == 0)
        {
            currentTailTarget = transform;
            yield break;
        }

        List<GameObject> remainingTails = new List<GameObject>(tailSegments);

        while (remainingTails.Count > 0)
        {
            int randomIndex = Random.Range(0, remainingTails.Count);
            GameObject tail = remainingTails[randomIndex];
            SpawnParticle(tail);
            remainingTails.RemoveAt(randomIndex);
            pooler.ReturnToPool("Tail", tail);

            if (tailDisappearInterval > 0f)
            {
                yield return new WaitForSeconds(tailDisappearInterval);
            }
            else
            {
                yield return null;
            }
        }

        tailSegments.Clear();
        currentTailTarget = transform;
    }

    void ReloadScene() {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}
