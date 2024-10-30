using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PreyAgent : MonoBehaviour
{
    // Prey parameters
    public float separationRadius = 2.0f;
    public float alignmentRadius = 15.0f;
    public float attractionRadius = 80.0f;
    public float separationForceMultiplier = 5.0f;
    public float alignmentForceMultiplier = 1.0f;
    public float attractionForceMultiplier = 0.5f;
    public float frictionCoefficient = 0.1f;
    public float preyDiameter = 0.5f;

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
        Collider[] preyInSeparationRadius = Physics.OverlapSphere(transform.position, separationRadius, preyLayer);
        Collider[] preyInAlignmentRadius = Physics.OverlapSphere(transform.position, alignmentRadius, preyLayer);
        Collider[] preyInAttractionRadius = Physics.OverlapSphere(transform.position, attractionRadius, preyLayer);

        Vector3 separationForce = ComputeSeparationForce(preyInSeparationRadius);
        Vector3 alignmentForce = ComputeAlignmentForce(preyInAlignmentRadius);
        Vector3 attractionForce = ComputeAttractionForce(preyInAttractionRadius);
        Vector3 propulsionForce = ComputePropulsionForce();
        Vector3 frictionForce = ComputeFrictionForce();

        Vector3 totalForce = separationForce + alignmentForce + attractionForce + propulsionForce + frictionForce;

        // Apply force and rotate towards velocity vector.
        rb.AddForce(totalForce, ForceMode.Force);
        RotateTowardsVelocity();
    }

    // Separation Force
    Vector3 ComputeSeparationForce(Collider[] nearbyPrey)
    {
        Vector3 separation = Vector3.zero;
        
        foreach (Collider prey in nearbyPrey)
        {
            if (prey.gameObject != gameObject) // Avoid self
            {
                // Pairwise separation force according to article.
                Vector3 difference = transform.position - prey.transform.position;
                separation += difference.normalized * (difference.magnitude - 2 * preyDiameter);
            }
        }
        separation.z = 0;
        return separation * separationForceMultiplier;
    }

    // Alignment Force
    Vector3 ComputeAlignmentForce(Collider[] nearbyPrey)
    {
        Vector3 alignment = Vector3.zero;
        int count = 0;

        foreach (Collider prey in nearbyPrey)
        {
            if((prey.transform.position - transform.position).magnitude > separationRadius)
            {
                if (prey.gameObject != gameObject)
                {
                    alignment += prey.GetComponent<Rigidbody>().velocity;
                    count++;
                }
            }
        }

        if (count > 0)
        {
            alignment /= count;
            alignment -= rb.velocity;
        }
        alignment.z = 0;
        return alignment * alignmentForceMultiplier;
    }

    // Attraction Force
    Vector3 ComputeAttractionForce(Collider[] nearbyPrey)
    {
        Vector3 attraction = Vector3.zero;
        int count = 0;

        foreach (Collider prey in nearbyPrey)
        {
            if ((prey.transform.position - transform.position).magnitude > alignmentRadius)
            {
                if (prey.gameObject != gameObject)
                {
                    Vector3 difference = prey.transform.position - rb.transform.position;
                    attraction += Mathf.Sqrt(1 - Mathf.Pow((1 - difference.magnitude / attractionRadius), 2)) * difference.normalized;
                    count++;
                }
            }
        }

        if (count > 0)
        {
            attraction /= count;
        }
        attraction.z = 0;
        return attraction * attractionForceMultiplier;
    }

    // Propulsion Force (basic forward movement)
    Vector3 ComputePropulsionForce()
    {
        Vector3 propulsion;
        Collider[] nearbyPredators = Physics.OverlapSphere(transform.position, attractionRadius, predatorLayer);

        if (nearbyPredators.Length > 0)
        {
            Vector3 escapeDirection = Vector3.zero;
            foreach (Collider predator in nearbyPredators)
            {
                Vector3 direction = transform.position - predator.transform.position;
                escapeDirection += direction.normalized / direction.magnitude;
            }
            
            propulsion = (beta_escape - gamma * rb.velocity.magnitude*rb.velocity.magnitude)*escapeDirection.normalized;
        }
        else
        {
            propulsion = (beta - alpha * rb.velocity.magnitude) * rb.velocity;
        }
        propulsion.z = 0;
        return propulsion;
    }

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
        Quaternion rotation = Quaternion.Euler(0, 0, angle-90); // Create a Quaternion from the angle
        transform.rotation = rotation;
    }

    void OnDrawGizmosSelected()
    {
        // Visualize zones in 2D plane (xz)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, separationRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, alignmentRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);
    }
}
