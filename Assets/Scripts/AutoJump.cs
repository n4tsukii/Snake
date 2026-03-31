using UnityEngine;

public class AutoJump : MonoBehaviour
{
    float jumpForce = 3f;
    Rigidbody rb;
    float jumpInterval = 0.40f;
    float jumpTimer = 0f;
    int isGrounded = 0;

    void OnCollisionEnter(Collision collision)
    {
        isGrounded = 0;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        jumpTimer += Time.deltaTime;
        if (jumpTimer >= jumpInterval && isGrounded == 0)
        {
            Jump();
            jumpTimer = 0f;
        }
    }

    void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = 1;
    }
}
