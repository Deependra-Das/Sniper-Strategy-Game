using UnityEngine;

public class GuardEnemy : BaseEnemy
{
    protected override void ExecuteBehaviour()
    {
        agent.ResetPath();
    }
}
