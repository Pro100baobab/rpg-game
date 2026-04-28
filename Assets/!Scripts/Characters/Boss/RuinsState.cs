using UnityEngine;

public class RuinsState : EnemyState
{
    private bool assembling;
    private float assembleTimer;
    private float assembleDuration = 7.15f; // врем€ анимации сбора

    public RuinsState(EnemyStateMachine sm) : base(sm)
    {
        // ƒлительность можно получать из BossEnemy через контекст,
        // но дл€ упрощени€ оставим константу или добавим поле в настройки.
    }

    public override void Enter()
    {
        assembling = false;
        //Context.SwitchToRuinsAnimator();
    }

    public override void Update()
    {
        if (Context.Health.CurrentHealth <= 0) return;

        if (!assembling && PlayerInDetectionRange())
        {
            assembling = true;
            assembleTimer = 0f;
            Context.BeforeSpawnAnimator.SetTrigger("Assemble");
        }

        if (assembling)
        {
            assembleTimer += Time.deltaTime;
            if (assembleTimer >= assembleDuration)
            {
                Context.SwitchToMonsterAnimator();
                StateMachine.ChangeState(new BossIdleState(StateMachine));
                Debug.LogWarning("ћонстр перешел в Idle из Ruins");
            }
        }
    }

    private bool PlayerInDetectionRange()
    {
        if (Context.PlayerTransform == null) return false;
        float dist = Vector3.Distance(Context.Transform.position, Context.PlayerTransform.position);
        return dist <= Context.Settings.DetectionRange;
    }
}