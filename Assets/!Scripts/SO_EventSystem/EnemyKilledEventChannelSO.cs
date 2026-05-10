using UnityEngine;
using UnityEngine.Events;

public struct KillData
{
    public bool IsBoss;
    public int ScoreValue;
}

[CreateAssetMenu(menuName = "Events/Enemy Killed Event Channel")]
public class EnemyKilledEventChannelSO : ScriptableObject
{
    public UnityAction<KillData> OnEventRaised;
    public void RaiseEvent(KillData data) => OnEventRaised?.Invoke(data);
}