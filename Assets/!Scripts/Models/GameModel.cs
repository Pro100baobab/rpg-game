using System.Collections.Generic;
using UnityEngine;

public class GameModel
{
    public GameObject Player { get; set; }
    public List<GameObject> Enemies { get; set; } = new List<GameObject>();
}