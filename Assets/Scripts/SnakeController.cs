using UnityEngine;

public class SnakeController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float steerSpeed = 180f;
    [SerializeField] int tailGap = 100;
    [SerializeField] float tailFollowSpeed = 5f;

    ObjectPooler pooler;
    Transform currentTailTarget;

    void Start()
    {
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
            GameObject tailObject = pooler.SpawnFromPool("Tail", transform.position, transform.rotation);
            if (tailObject == null) return;

            TailController tailController = tailObject.GetComponent<TailController>();
            if (tailController == null) return;

            tailController.Initialize(currentTailTarget, tailFollowSpeed, tailGap);
            currentTailTarget = tailObject.transform;
        }
    }

    void MoveHead()
    {
        transform.position += moveSpeed * Time.deltaTime * transform.forward;

        float steerDirection = Input.GetAxis("Horizontal");
        transform.Rotate(steerDirection * steerSpeed * Time.deltaTime * Vector3.up);
    }
}
