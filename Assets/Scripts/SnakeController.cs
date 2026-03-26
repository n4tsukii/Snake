using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class SnakeController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5;
    [SerializeField] float steerSpeed = 180;
    [SerializeField] GameObject Tail;
    ObjectPooler pooler;

    void Start()
    {
        pooler = ObjectPooler.Instance;
    }

    void Update() {
        MoveSnake();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Treat")) {
            GrowSnake();
        }
    }

    void MoveSnake() 
    {
        transform.position += moveSpeed * Time.deltaTime * transform.forward;

        float steerDirection = Input.GetAxis("Horizontal");
        transform.Rotate(steerDirection * steerSpeed * Time.deltaTime * Vector3.up);
    }

    private void GrowSnake()
    {
        GameObject tail = pooler.SpawnFromPool("Tail", transform.position, transform.rotation);
        if (tail != null)
        {
            tail.transform.SetParent(transform.parent, worldPositionStays: true);
            TailController tailController = tail.GetComponent<TailController>();
            if (tailController != null)            {
                tailController.enabled = true;
                tailController.TailParts.Add(tail);
            }
        }
    }
}