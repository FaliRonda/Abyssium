using UnityEngine;

public class EnemyAttackTrigger : MonoBehaviour
{
    public EnemyAI enemyAI;
    
    public void ActiveAttackTrigger()
    {
        enemyAI.ActiveAttackTrigger();
    }
    
    public void DeactiveAttackTrigger()
    {
        enemyAI.DeactiveAttackTrigger();
    }
}