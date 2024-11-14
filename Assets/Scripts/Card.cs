using UnityEditor.Hardware;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    public bool IsMatched { get; set; } = false;
    public bool IsBeingCompared { get; set; } // To track during cards compare delay

    [SerializeField] private Image frontImage; // Reference to the front image component
    private Sprite cardImage; // Unique image for this card

    private void Start()
    {
        if (IsMatched) // When loading a already matched card
        {
            _animator.SetBool("isFlipped", true);
        }
    }

    public void SetCardImage(Sprite image)
    {
        cardImage = image;
        frontImage.sprite = cardImage;        
    }

    public Sprite GetCardImage()
    {
        return cardImage;
    }

    public void Flip()
    {
        if (IsMatched) return; // Skip if already matched
        bool isFlipped = _animator.GetBool("isFlipped");
        _animator.SetBool("isFlipped", !isFlipped);
    }

    public void ClickButton()
    {
        if (IsBeingCompared) return;

        if (!IsMatched)
        {
            IsBeingCompared = true;
            Flip();
            GameEventsManager.Instance.CardSelected(this);
        }
    }
}
