using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    private Animator _animator;
    public bool IsMatched { get; set; } = false;
    public bool IsBeingCompared { get; set; } // To track during cards compare delay

    [SerializeField] private Image frontImage; // Reference to the front image component
    private Sprite cardImage; // Unique image for this card

    private void Awake()
    {
        _animator = GetComponent<Animator>();
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
            Flip();
            GameManager.Instance.CardSelected(this);
        }
    }
}
