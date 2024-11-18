using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    private float speed = 4.0f;
    private float rotationSpeed = 5.0f;
    public Vector3 startPosition;
    public Transform stackPoint;
    private StackSystem stackSystem; 

    private float dropInterval = 0.15f; 
    private float nextDropTime = 0.0f; 
    
    public float jumpForce = 1.5f;
    private bool hasJumped;
    
    private bool isOnWater = false; 
    private float odunYuksekligi = 0.15f;

    private Transform waterSurface;  // Su yüzeyi

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        stackSystem = GetComponent<StackSystem>();
        waterSurface = GameObject.FindGameObjectWithTag("Water").transform;  // Su yüzeyini al
        Debug.Log("Player initialized at position: " + startPosition);
    }

    void FixedUpdate()
    {
        Vector3 forwardMovement = transform.forward * speed * Time.deltaTime;
        rb.MovePosition(rb.position + forwardMovement); 
        Debug.Log("Player moving forward. Current position: " + rb.position);

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                float deltaX = touch.deltaPosition.x * rotationSpeed * Time.deltaTime;
                Quaternion rotation = Quaternion.Euler(0, deltaX, 0);
                rb.MoveRotation(rb.rotation * rotation); 
                Debug.Log("Player rotated. New rotation: " + rb.rotation.eulerAngles);
            }
        }
    }

    private void Update()
    {
        if (!IsGrounded()) 
        {
            Debug.Log("Player is not grounded.");

            if (stackSystem != null && stackSystem.GetWoodCount() > 0) 
            {
                FreezeYPosition(true);
                Debug.Log("Freezing Y position.");
                
                if (Time.time >= nextDropTime)
                {
                    GameObject wood = stackSystem.RemoveWood();
                    if (wood != null)
                    {
                        wood.transform.position = new Vector3(transform.position.x, waterSurface.position.y + odunYuksekligi, transform.position.z); // Su yüzeyine yakın yerleştir
                        wood.transform.rotation = Quaternion.identity; 
                        wood.SetActive(true);
                        Debug.Log("Wood dropped. Position: " + wood.transform.position);
                    }
                    nextDropTime = Time.time + dropInterval; 
                }
            }
            else
            {
                FreezeYPosition(false);
                Debug.Log("No wood left. Allowing Y position movement.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Player triggered: " + other.gameObject.name);

        if (other.CompareTag("Finish"))
        {
            Debug.Log("Player reached finish line!");
            GameManager.Instance.GameOver();
        }
        else if (other.CompareTag("Collectible"))
        {
            Destroy(other.gameObject); 
            if (stackSystem != null)
            {
                stackSystem.AddWood(1); 
                Debug.Log("Collected wood. Total wood: " + stackSystem.GetWoodCount());
            }
        }
        else if (other.CompareTag("Booster"))
        {
            Debug.Log("Player picked up booster.");
            Jump(); 
        }
        else if (other.CompareTag("WaterTrigger"))
        {
            Debug.Log("Player fell into the water! Game Over.");
            GameManager.Instance.GameOver();
        }
    }

    private void Jump()
    {
        if (!hasJumped)
        {
            Vector3 jumpDirection = (transform.forward + new Vector3(0, 0.5f, 0)).normalized;
            rb.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);
            hasJumped = true;
            Debug.Log("Player jumped with force: " + jumpDirection * jumpForce);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Player collided with: " + other.gameObject.name);

        if (other.gameObject.CompareTag("Ground"))
        {
            hasJumped = false;  
            Debug.Log("Player grounded.");
        }
        else if (other.gameObject.CompareTag("Obstacle"))
        {
            rb.rotation = Quaternion.Euler(0, rb.rotation.eulerAngles.y, 0); 
            Debug.Log("Player hit an obstacle. Resetting rotation.");
        }
    }

    private bool IsGrounded()
    {
        bool grounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
        Debug.Log("Is player grounded? " + grounded);
        return grounded;
    }

    private void FreezeYPosition(bool freeze)
    {
        if (freeze)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ; 
            Debug.Log("Y position frozen.");
        }
        else
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            Debug.Log("Y position unfrozen.");
        }
    }
}