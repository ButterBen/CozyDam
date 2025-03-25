using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SimpleGoalNavigationScript : MonoBehaviour
{
    public GameObject targetGO;
    public Vector3 targetTransform;
    private NavMeshAgent agent;
    private Animator animator;
    [SerializeField] private GameObject unit;

    private Vector3 lastPosition;
    public bool isMovingRight = true;
    
    // Variables for stuck detection
    private float stuckCheckTimer = 0f;
    private float stuckCheckInterval = 0.6f; // Check every 0.2 seconds
    private float minMovementThreshold = 0.05f; // Minimum movement required
    private float unstuckRadius = 2f; // Radius for random point when stuck
    private Vector3 positionAtLastCheck;
    public float normalSpeed = 5f;
    public float slowSpeed = 3f;
    public bool inMenu = false;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        lastPosition = transform.position;
        positionAtLastCheck = transform.position;
        if(inMenu)
        {
            StartCoroutine(menuRoam());
        }
    }

    void FixedUpdate()
    {
        AdjustSpeedBasedOnNavMeshArea();

        // Check if the agent has reached its destination
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            // Either no path or very close to the destination
            if (!agent.pathPending)
            {
                animator.SetBool("walking", false);
            }
        }
        
        // Check for stuck agents
        CheckIfStuck();

        // Check movement direction
        CheckMovementDirection();
    }
    void AdjustSpeedBasedOnNavMeshArea()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 1f, NavMesh.AllAreas))
        {
            int areaMask = hit.mask; // Gets the area type
            
            if (areaMask == (1 << 0)) // NavMesh area 0
            {
                agent.speed = normalSpeed;
            }
            else if (areaMask == (1 << 4)) // NavMesh area 4
            {
                agent.speed = slowSpeed;
            }
        }
    }

    void CheckIfStuck()
    {
        // Only check for stuck units if we're actively navigating
        if (!agent.pathPending && agent.remainingDistance > agent.stoppingDistance)
        {
            stuckCheckTimer += Time.fixedDeltaTime;
            
            // Check if it's time to see if we're stuck
            if (stuckCheckTimer >= stuckCheckInterval)
            {
                // Calculate how far we've moved since last check
                float distanceMoved = Vector3.Distance(transform.position, positionAtLastCheck);
                
                // If we haven't moved enough, we might be stuck
                if (distanceMoved < minMovementThreshold)
                {
                    Debug.Log("Unit is stuck! Finding alternate path...");
                    FindAlternatePath();
                }
                
                // Reset for next check
                positionAtLastCheck = transform.position;
                stuckCheckTimer = 0f;
            }
        }
    }
    
    void FindAlternatePath()
    {
        // Find a random point within unstuckRadius
        Vector2 randomCirclePoint = Random.insideUnitCircle * unstuckRadius;
        Vector3 randomPoint = new Vector3(
            transform.position.x + randomCirclePoint.x,
            transform.position.y + randomCirclePoint.y,
            0f
        );
        
        // Set the new target
        SetTargetTransform(randomPoint);
    }
    

    void CheckMovementDirection()
    {
        // Get current position
        Vector3 currentPosition = transform.position;

        // Calculate movement direction
        Vector3 movementDirection = currentPosition - lastPosition;

        // For isometric 2D, we'll check both X and Y movement
        if (movementDirection.magnitude > 0.01f) // Small threshold to avoid tiny movements
        {
            // Determine if moving more horizontally or vertically
            bool movingMoreHorizontally = Mathf.Abs(movementDirection.x) > Mathf.Abs(movementDirection.y);

            if (movingMoreHorizontally)
            {
                // Horizontal movement
                bool newIsMovingRight = movementDirection.x > 0;
                if (newIsMovingRight != isMovingRight)
                {
                    // Flip the unit
                    isMovingRight = newIsMovingRight;
                    RotateUnit();
                }
            }
            else
            {
                // Vertical movement - you might want to adjust this based on your specific isometric setup
                bool newIsMovingRight = movementDirection.y > 0;
                if (newIsMovingRight != isMovingRight)
                {
                    // Flip the unit
                    isMovingRight = newIsMovingRight;
                    RotateUnit();
                }
            }
        }

        // Update last position for next frame
        lastPosition = currentPosition;
    }

    public void RotateUnit()
    {
        // Rotate 180 degrees around Y axis
        unit.transform.rotation = Quaternion.Euler(0f, isMovingRight ? 0f : 180f, 0f);
    }

    public void SetTargetGO(GameObject newTarget)
    {
        animator.SetBool("walking", true);
        
        targetGO = newTarget;
        Vector3 targetPosition = targetGO.transform.position;
        targetPosition.z = 0;
       // Debug.Log("Setting target GO to: " + targetPosition);
        agent.SetDestination(targetPosition);
    }

    public void SetTargetTransform(Vector3 newTarget)
    {
        animator.SetBool("walking", true);
        newTarget.z = 0;
        //Debug.Log("Setting target to: " + newTarget);
        targetTransform = newTarget;
        agent.SetDestination(targetTransform);
    }

    IEnumerator menuRoam()
    {
        float minX = 4f, maxX = 30f; // Set your X bounds
        float minY = 0f, maxY = 12f; // Set your Y bounds

        while (true)
        {
            // Generate a random point within the defined rectangle
            float randomX = Random.Range(minX, maxX);
            float randomY = Random.Range(minY, maxY);
            
            Vector3 randomPoint = new Vector3(randomX, randomY, 0f);
            
            SetTargetTransform(randomPoint);

            yield return new WaitForSeconds(10);
        }
    }

}