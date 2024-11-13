using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    private Animator _animator;
    public bool IsMatched { get; set; } = false;

    [SerializeField] private Image frontImage; // Reference to the front image component

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Flip()
    {
        if (IsMatched) return; // Skip if already matched
        bool isFlipped = _animator.GetBool("isFlipped");
        _animator.SetBool("isFlipped", !isFlipped);
    }

    public void ClickButton()
    {
        if (!IsMatched)
        {
            Flip();
        }
    }
}
