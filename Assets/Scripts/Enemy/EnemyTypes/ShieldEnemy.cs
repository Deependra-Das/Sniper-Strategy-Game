using UnityEngine;

public class ShieldEnemy : BaseEnemy
{
    protected override void ExecuteBehaviour()
    {
        agent.ResetPath();
    }
}
