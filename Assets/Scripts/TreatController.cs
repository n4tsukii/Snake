using UnityEngine;

public class TreatController : MonoBehaviour
{
    private float floorY = 1f;
    private float spawnRangeX = 10f;
    private float spawnRangeZ = 10f;

    void Start()
    {

    }

    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Snake")) {
            TeleportToRandomLocation();
        } else if (collision.gameObject.CompareTag("Tail")) {
            TeleportToRandomLocation();
        }
    }

    void TeleportToRandomLocation()
    {
        Vector3 randomPosition = new Vector3(
            Random.Range(-spawnRangeX, spawnRangeX),
            floorY,
            Random.Range(-spawnRangeZ, spawnRangeZ)
        );
        transform.position = randomPosition;

        Debug.Log($"Treat teleported to: {randomPosition}");
    }
}