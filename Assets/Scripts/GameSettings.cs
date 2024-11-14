using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/GameSettings", order = 1)]
public class GameSettings : ScriptableObject
{
    [Header("Grid Settings")]
    [Tooltip("Number of rows in the game grid.")]
    public int Rows = 4;
    [Tooltip("Number of columns in the game grid.")]
    public int Cols = 4;
    [Header("Card Settings")]
    [Tooltip("List of images for the cards.")]
    public List<Sprite> CardImages = new List<Sprite>();
}
