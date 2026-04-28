using UnityEngine;
using UnityEngine.AI;

public interface IEnemyContext
{
    Animator Animator { get; }
    Animator BeforeSpawnAnimator { get; }
    NavMeshAgent Agent { get; }
    Transform Transform { get; }
    Transform PlayerTransform { get; }
    IHealth Health { get; }
    IEnemySettings Settings { get; }
    bool IsPeacefulMode { get; }
    Transform[] PatrolPoints { get; }


    void PerformAttack();
    void PerformStrongAttack();
    void PerformMagicAttack();
    void PerformSummon();
    void OnAttackFinished();

    void SwitchToMonsterAnimator();
    void SwitchToRuinsAnimator();
}