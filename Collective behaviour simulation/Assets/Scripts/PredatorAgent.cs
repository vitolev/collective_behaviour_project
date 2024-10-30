using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class PredatorAgent : MonoBehaviour
{
    // Predator parameters
    public float detectionRadius = 80.0f;
    public float killRadius = 3.0f;
    public float separationForceMultiplier = 5.0f;
    public float frictionCoefficient = 0.1f;
    public float predatorDiameter = 0.5f;

    public float beta = 1.0f;
    public float beta_escape = 20.0f;
    public float alpha = 0.1f;
    public float gamma = 0.1f;

    public LayerMask preyLayer;
    public LayerMask predatorLayer;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Collider[] predatorInSeparationRadius = Physics.OverlapSphere(transform.position, killRadius, predatorLayer);
        

        Vector3 separationForce = ComputeSeparationForce(predatorInSeparationRadius);
        Vector3 frictionForce = ComputeFrictionForce();

        Vector3 totalForce = separationForce + frictionForce;

        // Apply force and rotate towards velocity vector.
        rb.AddForce(totalForce, ForceMode.Force);
        RotateTowardsVelocity();
    }

    // Separation Force
    Vector3 ComputeSeparationForce(Collider[] nearbyPredator)
    {
        Vector3 separation = Vector3.zero;

        foreach (Collider predator in nearbyPredator)
        {
            if (predator.gameObject != gameObject) // Avoid self
            {
                // Pairwise separation force according to article.
                Vector3 difference = predator.transform.position - transform.position;
                separation += difference.normalized * (difference.magnitude - 2 * predatorDiameter);
            }
        }
        separation.z = 0;
        return separation * separationForceMultiplier;
    }

    // Propulsion Force (basic forward movement)
    /*
    Vector3 ComputePropulsionForce()
    {
        Vector3 propulsion;
        Collider[] nearbyPredators = Physics.OverlapSphere(transform.position, attractionRadius, predatorLayer);

        if (nearbyPredators.Length > 0)
        {
            Vector3 escapeDirection = Vector3.zero;
            foreach (Collider predator in nearbyPredators)
            {
                escapeDirection += (transform.position - predator.transform.position);
            }

            propulsion = (beta_escape - gamma * rb.velocity.magnitude * rb.velocity.magnitude) * escapeDirection.normalized;
        }
        else
        {
            propulsion = (beta - alpha * rb.velocity.magnitude) * rb.velocity;
        }
        propulsion.z = 0;
        return propulsion;
    }
    */

    // Frictional Force
    Vector3 ComputeFrictionForce()
    {
        Vector3 friction = -frictionCoefficient * rb.velocity.magnitude * rb.velocity;
        friction.z = 0;
        return friction;
    }

    // Rotate towards velocity direction
    void RotateTowardsVelocity()
    {
        Vector3 direction = rb.velocity;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // Convert radians to degrees
        angle = angle % 360; // Wrap the angle to the range 0-360
        Quaternion rotation = Quaternion.Euler(0, 0, angle - 90); // Create a Quaternion from the angle
        transform.rotation = rotation;
    }

    void OnDrawGizmosSelected()
    {
        // Visualize zones in 2D plane (xz)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, killRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
