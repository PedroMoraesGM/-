using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventsManager : MonoBehaviour
{
    public event Action<Card> OnCardSelected;
    public event Action<bool> OnCardCompared;
    public event Action OnGameover;
    public event Action OnGameLoaded;
    public event Action OnGameStarted;

    public static GameEventsManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
    public void CardSelected(Card card) => OnCardSelected?.Invoke(card);
    public void CardCompared(bool isMatch) => OnCardCompared?.Invoke(isMatch);
    public void GameLoaded() => OnGameLoaded?.Invoke();
    public void GameStarted() => OnGameStarted?.Invoke();
    public void Gameover() => OnGameover?.Invoke();
}
