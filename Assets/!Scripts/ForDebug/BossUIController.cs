using TMPro;
using UnityEngine;
using UnityEngine.UI;


// Этот класс предназначен для отладки и позволяет менять тип атаки и элемент босса через UI
// (поэтому для упрощения не использует MVC)
public class BossUIController : MonoBehaviour
{
    [SerializeField] private Button meleeButton;
    [SerializeField] private Button rangedButton;
    [SerializeField] private Button elementsButton;
    [SerializeField] private TextMeshProUGUI elementText;

    [SerializeField] private BossEnemy boss;

    private int index = 0;

    private void Start()
    {
        meleeButton.onClick.AddListener(() => boss.SetAttackType(BossAttackType.Melee));
        rangedButton.onClick.AddListener(() => boss.SetAttackType(BossAttackType.Ranged));

        elementsButton.onClick.AddListener(() => {
            index = (index + 1) % 5;
            ChangeText(index);
            boss.SetElement((IElement.Elements)index);
            });
    }

    private void ChangeText(int index)
    {
        elementText.text = index switch
        {
            0 => "Ground",
            1 => "Rock",
            2 => "Lava",
            3 => "Ice",
            4 => "Snow",
            _ => "Unknown",
        };
    }
}