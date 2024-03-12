using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using UnityEngine;

public class BTPatrolNode : BTNode
{
    private Enemies.CODE_NAMES enemyCode;
    private bool isWalking;
    private Vector3 nextDestination;
    private PatrolNodeParametersSO patrolNodeParameters;
    private bool calculatingNextDestination;
    private EventInstance stepFMODAudio;

    public BTPatrolNode(Enemies.CODE_NAMES enemyCode)
    {
        this.enemyCode = enemyCode;
    }

    public override BTNodeState Execute()
    {
        if (!enemyAI.enemyStunned || !patrolNodeParameters.stoppedByStun)
        {
            if (isWalking)
            {
                // Move towards the current waypoint
                Vector3 direction = nextDestination - enemyTransform.position;
                float distance = direction.magnitude;

                if (distance < 0.1f)
                {
                    isWalking = false;
                    stepFMODAudio.stop(STOP_MODE.ALLOWFADEOUT);
                    return BTNodeState.NextTree;
                }

                // Move towards the waypoint
                enemyTransform.Translate(direction.normalized * (Time.deltaTime * patrolNodeParameters.patrolSpeed));
                enemySprite.flipX = direction.x > 0;
                enemyAnimator.Play("Enemy_walk");

                return BTNodeState.Running;
            }
            else if(!calculatingNextDestination)
            {
                calculatingNextDestination = true;
                WaitAndCalculateDestination();
                return BTNodeState.Running;
            }
            else
            {
                return BTNodeState.Running;
            }
        }

        enemyAnimator.Play("Enemy_idle");
        return BTNodeState.Running;
    }

    private void WaitAndCalculateDestination()
    {
        enemyAnimator.Play("Enemy_idle");
        Sequence calculateSequence = DOTween.Sequence();
        calculateSequence
            .AppendInterval(Random.Range(patrolNodeParameters.minWait, patrolNodeParameters.maxWait))
            .AppendCallback(CalculateNextDestination);
    }

    void CalculateNextDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere;
        randomDirection.y = 0; // Asegurar que el punto esté en el mismo plano que el suelo
        randomDirection.Normalize(); // Normalizar el vector para asegurarse de que tenga una longitud de 1
        Vector3 randomPoint = enemyTransform.position + randomDirection * Random.Range(patrolNodeParameters.innerRadius, patrolNodeParameters.outerRadius);

        RaycastHit[] hits = Physics.RaycastAll(enemyTransform.position, randomPoint - enemyTransform.position, patrolNodeParameters.outerRadius);
        
        bool foundWall = false;
        foreach (RaycastHit hit in hits)
        {
            // Comprobar si alguno de los hits golpea una pared
            if (hit.collider != null && hit.collider.gameObject.layer == Layers.WALL_LAYER)
            {
                foundWall = true;
                break;
            }
        }

        if (foundWall)
        {
            // Si alguno de los rayos golpea una pared, recalcula el punto
            CalculateNextDestination();
            return;
        }
        else
        {
            nextDestination = randomPoint;
            isWalking = true;
            stepFMODAudio = Core.Audio.PlayFMODAudio("event:/Characters/Enemies/Stalker/Steps", enemyTransform);
            calculatingNextDestination = false;
        }
    }

    public override void DrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(enemyTransform.position, patrolNodeParameters.innerRadius);
        Gizmos.DrawWireSphere(enemyTransform.position, patrolNodeParameters.outerRadius);
    }
    
    public override void InitializeNode(Dictionary<string, object> parameters)
    {
        base.InitializeNode(parameters);
        AssignNodeParameters();
    }
    
    private void AssignNodeParameters()
    {
        patrolNodeParameters =
            Resources.Load<PatrolNodeParametersSO>(Enemies.EnemiesParametersPathDictionary(enemyCode, "Patrol"));

        // Check if the ScriptableObject was loaded successfully.
        if (patrolNodeParameters == null)
        {
            Debug.LogError("EnemyParemeters not found in Resources folder.");
        }
    }

    public override void ResetNode()
    {
        isWalking = false;
        stepFMODAudio.stop(STOP_MODE.ALLOWFADEOUT);
    }
}