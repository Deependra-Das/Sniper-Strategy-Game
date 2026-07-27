using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

public abstract class BaseEnemy : MonoBehaviour
{
    [SerializeField] protected float behaviourLoopInterval = 0.1f;
    protected Animator animator;
    protected NavMeshAgent agent;
    private Coroutine _behaviourLoopCoroutine;
    private bool _isPaused;

    private float timer = 0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        SetPaused(false);
        _behaviourLoopCoroutine = StartCoroutine(BehaviourLoop());
    }

    private IEnumerator BehaviourLoop()
    {
        while (true)
        {
            if (!_isPaused)
                ExecuteBehaviour();

            yield return new WaitForSeconds(behaviourLoopInterval);
        }
    }

    private void SetPaused(bool paused)
    {
        _isPaused = paused;

        agent.isStopped = paused;
        animator.speed = paused ? 0f : 1f;
    }

    protected abstract void ExecuteBehaviour();
}
