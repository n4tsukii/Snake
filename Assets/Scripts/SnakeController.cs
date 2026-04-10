using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SnakeController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float steerSpeed = 180f;
    [SerializeField] int tailGap = 100;
    [SerializeField] ParticleSystem deadParticle;
    float despawnDelay = 0.5f;
    Vector3 inputDirection = new Vector3(0, 0, 1);

    List<GameObject> tailSegments = new List<GameObject>();
    Renderer[] cachedHeadRenderers;
    Collider cachedHeadCollider;
    ParticleSystem deadParticleInstance;

    ObjectPooler pooler;
    Transform currentTailTarget;
    
    bool isDead;

    void Start()
    {
        // cachedHeadRenderers = GetComponentsInChildren<Renderer>(true);
        // cachedHeadCollider = GetComponent<Collider>();
        // pooler = ObjectPooler.Instance;
        // currentTailTarget = transform;

        isDead = false;
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
            StartCoroutine(Dead());
        }
    }

    void MoveHead()
    {
        if (Input.GetKeyDown(KeyCode.W)) {
            inputDirection = new Vector3(0, 0, 1);
        } else if (Input.GetKeyDown(KeyCode.S)) {
            inputDirection = new Vector3(0, 0, -1);
        } else if (Input.GetKeyDown(KeyCode.A)) {
            inputDirection = new Vector3(-1, 0, 0);
        } else if (Input.GetKeyDown(KeyCode.D)) {
            inputDirection = new Vector3(1, 0, 0);
        }
        this.transform.position =  new Vector3(
            this.transform.position.x + inputDirection.x * (isDead ? 0 : 1f), 
            this.transform.position.y + inputDirection.y * (isDead ? 0 : 1f), 
            this.transform.position.z + inputDirection.z * (isDead ? 0 : 1f));

        float steerDirection = Input.GetAxis("Horizontal");
        transform.Rotate(steerDirection * steerSpeed * Time.deltaTime * Vector3.up * (isDead ? 0 : 1f));
    }

    void StopSnake() {
        isDead = true;
        foreach (GameObject tail in tailSegments)
        {
            TailController tailController = tail.GetComponent<TailController>();
            if (tailController != null)
            {
                tailController.StopTail();
            }
        }
    }

    IEnumerator Dead()
    {
        StopSnake();
        SpawnParticle(transform.gameObject);
        HideHead();
        yield return CollapseTailRoutine(despawnDelay);
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
        Destroy(deadParticleInstance, deadParticleInstance.main.duration);
    }

    IEnumerator CollapseTailRoutine(float despawnDelay)
    {
        if (tailSegments.Count == 0)
        {
            tailSegments.Clear();
            currentTailTarget = transform;
            yield break;
        }

        List<GameObject> remainingTails = new List<GameObject>(tailSegments);

        while (remainingTails.Count > 0)
        {
            int randomIndex = Random.Range(0, remainingTails.Count);
            GameObject tail = remainingTails[randomIndex];
            if (tail != null)
                continue;
            SpawnParticle(tail);
            remainingTails.RemoveAt(randomIndex);
            pooler.ReturnToPool("Tail", tail);

            yield return new WaitForSeconds(despawnDelay);
        }
    }

    void ReloadScene() {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}
