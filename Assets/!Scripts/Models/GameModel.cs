using System;
using System.Collections.Generic;
using UnityEngine;

public class GameModel
{
    public static GameModel Instance { get; private set; }

    public GameObject Player { get; set; }
    public List<GameObject> Enemies { get; set; } = new List<GameObject>();

    private bool _isPeacefulMode;
    public bool IsPeacefulMode
    {
        get => _isPeacefulMode;
        set
        {
            if (_isPeacefulMode != value)
            {
                _isPeacefulMode = value;
                OnPeacefulModeChanged?.Invoke(value);
            }
        }
    }
    public event Action<bool> OnPeacefulModeChanged;

    public GameModel()
    {
        Instance = this;
    }
}