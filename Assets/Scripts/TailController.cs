using UnityEngine;
using System.Collections.Generic;

public class TailController : MonoBehaviour, IPooledObject
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] int gap = 100;

    private readonly List<Vector3> positionHistory = new List<Vector3>();
    private Transform followTarget;
    private bool isFollowing;

    public void OnObjectSpawn()
    {
        positionHistory.Clear();
        isFollowing = false;
    }

    public void Initialize(Transform targetToFollow, float followSpeed, int followGap)
    {
        followTarget = targetToFollow;
        moveSpeed = followSpeed;
        gap = followGap;
        positionHistory.Clear();

        if (followTarget != null)
        {
            for (int i = 0; i <= gap; i++)
            {
                positionHistory.Add(followTarget.position);
            }
        }

        isFollowing = followTarget != null;
    }

    void Update()
    {
        MoveTail();
    }

    void MoveTail()
    {
        if (!isFollowing || followTarget == null) return;

        positionHistory.Insert(0, followTarget.position);

        int maxHistory = Mathf.Max(gap + 1, 256);
        if (positionHistory.Count > maxHistory)
        {
            positionHistory.RemoveRange(maxHistory, positionHistory.Count - maxHistory);
        }

        int historyIndex = Mathf.Min(gap, positionHistory.Count - 1);
        if (historyIndex < 0) return;

        Vector3 targetPosition = positionHistory[historyIndex];
        transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        Vector3 lookDirection = followTarget.position - transform.position;
        if (lookDirection.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
}
