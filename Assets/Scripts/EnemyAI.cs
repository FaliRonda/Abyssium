using UnityEngine;
using UnityEngine.Serialization;

public class EnemyAI : MonoBehaviour
{
    // Reference to the player
    [FormerlySerializedAs("player")] public Transform playerTransform;

    public float patrolSpeed = 1f;
    public float chaseSpeed = 1f;
    // Visibility radius
    public float visibilityRadius = 10f;

    // Attack distance
    public float attackDistance = 2f;

    public Transform[] waypoints;

    // Behavior tree root node
    private BTNode rootNode;

    private void Start()
    {
        // Create the behavior tree
        rootNode = new BTSelector(
            new BTAttackNode(transform, playerTransform, attackDistance),
            new BTChaseNode(transform, playerTransform, chaseSpeed),
            new BTPatrolNode(transform, waypoints, patrolSpeed)
        );
    }

    private void Update()
    {
        // Update the behavior tree
        rootNode.Execute();
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw the visibility radius gizmo
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visibilityRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}