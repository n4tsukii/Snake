using UnityEngine;

public class TreatController : MonoBehaviour
{
    private float floorY = 1f;
    private float spawnRangeX = 18f;
    private float spawnRangeZ = 18f;

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