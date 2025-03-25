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
    private float stuckCheckInterval = 0.6f; 
    private float minMovementThreshold = 0.05f; // Minimum movement required
    private float unstuckRadius = 2f; 
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
            int areaMask = hit.mask; 
            
            if (areaMask == (1 << 0)) 
            {
                agent.speed = normalSpeed;
            }
            else if (areaMask == (1 << 4)) 
            {
                agent.speed = slowSpeed;
            }
        }
    }

    void CheckIfStuck()
    {
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
        Vector3 currentPosition = transform.position;

        Vector3 movementDirection = currentPosition - lastPosition;

        if (movementDirection.magnitude > 0.01f)
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
                bool newIsMovingRight = movementDirection.y > 0;
                if (newIsMovingRight != isMovingRight)
                {
                    // Flip the unit
                    isMovingRight = newIsMovingRight;
                    RotateUnit();
                }
            }
        }

        lastPosition = currentPosition;
    }

    public void RotateUnit()
    {
        unit.transform.rotation = Quaternion.Euler(0f, isMovingRight ? 0f : 180f, 0f);
    }

    public void SetTargetGO(GameObject newTarget)
    {
        animator.SetBool("walking", true);
        
        targetGO = newTarget;
        Vector3 targetPosition = targetGO.transform.position;
        targetPosition.z = 0;
        agent.SetDestination(targetPosition);
    }

    public void SetTargetTransform(Vector3 newTarget)
    {
        animator.SetBool("walking", true);
        newTarget.z = 0;
        targetTransform = newTarget;
        agent.SetDestination(targetTransform);
    }

    IEnumerator menuRoam()
    {
        float minX = 4f, maxX = 30f;
        float minY = 0f, maxY = 12f;

        while (true)
        {
            float randomX = Random.Range(minX, maxX);
            float randomY = Random.Range(minY, maxY);
            
            Vector3 randomPoint = new Vector3(randomX, randomY, 0f);
            
            SetTargetTransform(randomPoint);

            yield return new WaitForSeconds(10);
        }
    }

}