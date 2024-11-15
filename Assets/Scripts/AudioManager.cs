using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip _flipSound;
    [SerializeField] private AudioClip _matchSound;
    [SerializeField] private AudioClip _mismatchSound;
    [SerializeField] private AudioClip _gameOverSound;

    [SerializeField] private AudioSource _soundAudioSource;

    private void Start()
    {
        GameEventsManager.Instance.OnCardSelected += PlayFlipSound;
        GameEventsManager.Instance.OnCardCompared += PlayMatchOrMismatchSound;
        GameEventsManager.Instance.OnGameover += PlayGameOverSound;
    }

    private void PlayFlipSound(Card card)
    {
        _soundAudioSource.PlayOneShot(_flipSound);
    }

    private void PlayMatchOrMismatchSound(bool isMatch)
    {
        _soundAudioSource.PlayOneShot(isMatch ? _matchSound : _mismatchSound);
    }

    private void PlayGameOverSound()
    {
        _soundAudioSource.PlayOneShot(_gameOverSound);
    }

    private void OnDestroy()
    {
        GameEventsManager.Instance.OnCardSelected -= PlayFlipSound;
        GameEventsManager.Instance.OnCardCompared -= PlayMatchOrMismatchSound;
        GameEventsManager.Instance.OnGameover -= PlayGameOverSound;
    }
}
