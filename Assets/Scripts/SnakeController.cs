using UnityEngine;
using System.Collections.Generic;

public class SnakeController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5;
    [SerializeField] float steerSpeed = 180;
    [SerializeField] GameObject Tail;
    [SerializeField] int Gap = 50;
    private List<GameObject> TailParts = new List<GameObject>();
    private List<Vector3> PositionHistory = new List<Vector3>();

    void Start()
    {
        GrowSnake();
        GrowSnake();
        GrowSnake();
        GrowSnake();
    }

    void Update() {
        MoveSnake();

        PositionHistory.Insert(0, transform.position);

        // Trim history to avoid memory leak
        int maxHistory = (TailParts.Count + 1) * Gap;
        if (PositionHistory.Count > maxHistory)
            PositionHistory.RemoveAt(PositionHistory.Count - 1);

        if (PositionHistory.Count < Gap) return;

        int index = 0;
        foreach (var body in TailParts) {
            Vector3 point = PositionHistory[Mathf.Clamp(index * Gap, 0, PositionHistory.Count - 1)];

            Vector3 moveDirection = point - body.transform.position;
            body.transform.position += moveDirection * moveSpeed * Time.deltaTime;

            body.transform.LookAt(point);

            index++;
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
        // Instantiate freely, then parent to Snake root (Head's parent)
        GameObject tail = Instantiate(Tail, transform.position, transform.rotation);
        tail.transform.SetParent(transform.parent, worldPositionStays: true);
        TailParts.Add(tail);
    }
}