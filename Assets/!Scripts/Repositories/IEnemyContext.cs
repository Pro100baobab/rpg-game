using UnityEngine;
using UnityEngine.AI;

public interface IEnemyContext
{
    Animator Animator { get; }
    NavMeshAgent Agent { get; }
    Transform Transform { get; }
    Transform PlayerTransform { get; }
    IHealth Health { get; }
    IEnemySettings Settings { get; }
    bool IsPeacefulMode { get; }


    void PerformAttack();
    void PerformStrongAttack();
    void PerformMagicAttack();
    void PerformSummon();
    void OnAttackFinished();
}