using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class TailController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5;
    [SerializeField] GameObject head;
    [SerializeField] int Gap = 50;
    public List<GameObject> TailParts = new List<GameObject>();
    private List<Vector3> PositionHistory = new List<Vector3>();
    void Start()
    {
        
    }

    void Update()
    {
        Move();
    }

    void Move()
    {
        PositionHistory.Insert(0, transform.position);

        // Trim history to avoid memory leak
        int maxHistory = (TailParts.Count + 1) * Gap;
        if (PositionHistory.Count > maxHistory)
            PositionHistory.RemoveAt(PositionHistory.Count - 1);

        if (PositionHistory.Count < Gap) return;

        int index = 1;
        foreach (var body in TailParts) {
            Vector3 point = PositionHistory[Mathf.Clamp(index * Gap, 0, PositionHistory.Count - 1)];

            Vector3 moveDirection = point - body.transform.position;
            body.transform.position += moveDirection * moveSpeed * Time.deltaTime;

            body.transform.LookAt(point);

            index++;
        }
    }
}