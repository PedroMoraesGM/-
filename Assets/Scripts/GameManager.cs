using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private Card _firstSelectedCard, _secondSelectedCard;
    private List<Card> _allCards = new List<Card>();
    private List<CardState> _cardStates = new List<CardState>(); // To track state and position for save/load

    [SerializeField] private GameSettings _settings; // All the game settings
    [SerializeField] private RectTransform _gridContainer; // Container for card layout
    [SerializeField] private Card _cardPrefab; // Prefab of a single card

    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void InitializeGame()
    {
        int totalCards = _settings.Rows * _settings.Cols;
        List<Sprite> randomImages = GenerateRandomCardImages(totalCards);

        // Instantiate and assign images to cards
        InstantiateCards(randomImages, _settings.Rows, _settings.Cols);
    }

    private List<Sprite> GenerateRandomCardImages(int totalCards)
    {
        List<Sprite> images = new List<Sprite>();

        // Shuffle the available images and take as many as needed for unique pairs
        List<Sprite> shuffledImages = new List<Sprite>(_settings.CardImages);
        Shuffle(shuffledImages);

        int pairsNeeded = totalCards / 2;
        for (int i = 0; i < pairsNeeded; i++)
        {
            images.Add(shuffledImages[i]); // Add one image for the pair
            images.Add(shuffledImages[i]); // Add the matching pair
        }

        // Shuffle the resulting list of paired images to randomize card positions
        Shuffle(images);
        return images;
    }

    private void Shuffle(List<Sprite> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Sprite temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private void InstantiateCards(List<Sprite> images, int rows, int cols)
    {
        // Calculate the size of each card based on container and grid layout
        float cardWidth = _gridContainer.rect.width / cols;
        float cardHeight = _gridContainer.rect.height / rows;

        for (int i = 0; i < images.Count; i++)
        {
            int row = i / cols;
            int col = i % cols;

            // Instantiate card prefab and set its parent to gridContainer
            Card newCard = Instantiate(_cardPrefab, _gridContainer);
            newCard.transform.localScale = Vector3.one;

            // Set the card's size
            RectTransform cardRect = newCard.GetComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(cardWidth, cardHeight);

            // Position card based on row and column within the grid
            float posX = col * cardWidth - (_gridContainer.rect.width / 2) + (cardWidth / 2);
            float posY = row * cardHeight - (_gridContainer.rect.height / 2) + (cardHeight / 2);
            cardRect.anchoredPosition = new Vector2(posX, posY);

            // Set the card's image
            newCard.SetCardImage(images[i]);
            newCard.IsMatched = false;

            // Add the card to the list of all cards and track its state for save/load
            _allCards.Add(newCard);
            _cardStates.Add(new CardState { cardImage = images[i], position = new Vector2(posX, posY), isMatched = false });
        }
    }

    public void CardSelected(Card selectedCard)
    {
        if (_firstSelectedCard == null)
        {
            _firstSelectedCard = selectedCard;
        }
        else if (_firstSelectedCard == selectedCard) //Checks for unselecting card
        {
            _firstSelectedCard = null;
        }
        else if (_secondSelectedCard == null)
        {
            _secondSelectedCard = selectedCard;
            StartCoroutine(CheckForMatch());
        }
    }

    private IEnumerator CheckForMatch()
    {

        Card firstSelect = _firstSelectedCard;
        Card secondSelect = _secondSelectedCard;

        // Track these cards being compared to lock input on then
        firstSelect.IsBeingCompared = true;
        secondSelect.IsBeingCompared = true;

        // Reset selections
        _firstSelectedCard = null;
        _secondSelectedCard = null;

        yield return new WaitForSeconds(0.5f); // Small delay for viewing

        if (firstSelect.GetCardImage() == secondSelect.GetCardImage())
        {
            firstSelect.IsMatched = true;
            secondSelect.IsMatched = true;

            UpdateCardState(firstSelect, true);
            UpdateCardState(secondSelect, true);
        }
        else
        {
            firstSelect.Flip();
            secondSelect.Flip();
        }

        firstSelect.IsBeingCompared = false;
        secondSelect.IsBeingCompared = false;
    }

    private void UpdateCardState(Card card, bool isMatched)
    {
        foreach (CardState state in _cardStates)
        {
            if (state.cardImage == card.GetCardImage() && state.position == ((RectTransform)card.transform).anchoredPosition)
            {
                state.isMatched = isMatched;
                break;
            }
        }
    }
}

// Structure to track each card’s position and match state for save/load
[System.Serializable]
public class CardState
{
    public Sprite cardImage;
    public Vector2 position;
    public bool isMatched;
}
