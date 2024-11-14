using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private Card _firstSelectedCard, _secondSelectedCard;
    private List<Card> _allCards = new List<Card>();
    private List<CardState> _cardStates = new List<CardState>(); // To track state and position for save/load

    [SerializeField] private GameSettings _settings; // All the game settings
    [SerializeField] private Card _cardPrefab; // Prefab of a single card
    [Header("UI")]
    [SerializeField] private RectTransform _gridContainer; // Container for card layout
    [SerializeField] private GameObject _loadGameButton;

    private string saveFilePath;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");

        SetLoadGameButton();
    }

    private void SetLoadGameButton()
    {
        // Display load button if there is saved data
        _loadGameButton.SetActive(File.Exists(saveFilePath));
    }

    public void InitializeGameButton()
    {
        File.Delete(saveFilePath); // Resets save file

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

    private void CleanCardsGridContainer()
    {
        // Clear any existing cards in the grid
        foreach (Transform child in _gridContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void InstantiateCards(List<Sprite> images, int rows, int cols)
    {
        _allCards.Clear();
        _cardStates.Clear();
        CleanCardsGridContainer();

        float cardWidth = _gridContainer.rect.width / cols;
        float cardHeight = _gridContainer.rect.height / rows;

        for (int i = 0; i < images.Count; i++)
        {
            int row = i / cols;
            int col = i % cols;

            Card newCard = Instantiate(_cardPrefab, _gridContainer);
            newCard.transform.localScale = Vector3.one;

            RectTransform cardRect = newCard.GetComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(cardWidth, cardHeight);

            float posX = col * cardWidth - (_gridContainer.rect.width / 2) + (cardWidth / 2);
            float posY = row * cardHeight - (_gridContainer.rect.height / 2) + (cardHeight / 2);
            cardRect.anchoredPosition = new Vector2(posX, posY);

            newCard.SetCardImage(images[i]);
            newCard.IsMatched = false;

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
        else if (_firstSelectedCard == selectedCard) // Checks for unselecting card
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

        firstSelect.IsBeingCompared = true;
        secondSelect.IsBeingCompared = true;

        _firstSelectedCard = null;
        _secondSelectedCard = null;

        yield return new WaitForSeconds(0.5f);

        if (firstSelect.GetCardImage() == secondSelect.GetCardImage())
        {
            firstSelect.IsMatched = true;
            secondSelect.IsMatched = true;

            UpdateCardState(firstSelect, true);
            UpdateCardState(secondSelect, true);

            // Check for game over condition
            if (AllCardsMatched())
            {
                GameOver();
                yield break; // Exit coroutine if the game is over
            }
        }
        else
        {
            firstSelect.Flip();
            secondSelect.Flip();
        }

        firstSelect.IsBeingCompared = false;
        secondSelect.IsBeingCompared = false;

        SaveGame(); // Save the game state after every card selection
    }

    private bool AllCardsMatched()
    {
        // Check if every card in the list is matched
        return _allCards.All(card => card.IsMatched);
    }

    private void GameOver()
    {
        Debug.Log("Game Over! You've matched all cards.");

        foreach (var card in _allCards)
        {
            card.transform.DOShakePosition(1.5f);
        }               

        File.Delete(saveFilePath); // Resets save file
    }

    private void UpdateCardState(Card card, bool isMatched)
    {
        foreach (CardState state in _cardStates)
        {
            if (state.cardImage == card.GetCardImage() && state.position == ((RectTransform)card.transform).anchoredPosition)
            {
                state.isMatched = isMatched;
                card.transform.DOShakePosition(0.75f);
                break;
            }
        }
    }

    private void SaveGame()
    {
        // Serialize the game state to JSON
        string json = JsonUtility.ToJson(new GameData(_cardStates));
        File.WriteAllText(saveFilePath, json);
    }
    public void LoadGameButton()
    {
        // Load the game state from JSON
        string json = File.ReadAllText(saveFilePath);
        GameData gameData = JsonUtility.FromJson<GameData>(json);

        // Calculate card dimensions based on grid layout
        int rows = _settings.Rows;
        int cols = _settings.Cols;
        float cardWidth = _gridContainer.rect.width / cols;
        float cardHeight = _gridContainer.rect.height / rows;

        CleanCardsGridContainer();

        _allCards.Clear();
        _cardStates.Clear();

        // Restore game from the saved card states
        foreach (CardState state in gameData.CardStates)
        {
            Card newCard = Instantiate(_cardPrefab, _gridContainer);
            newCard.IsMatched = state.isMatched;
            newCard.SetCardImage(state.cardImage);

            // Adjust card size based on container and grid layout
            RectTransform cardRect = newCard.GetComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(cardWidth, cardHeight);

            // Position the card based on its saved position
            cardRect.anchoredPosition = state.position;

            _allCards.Add(newCard);
            _cardStates.Add(state);
        }
    }
}

[System.Serializable]
public class GameData
{
    public List<CardState> CardStates;

    public GameData(List<CardState> cardStates)
    {
        CardStates = cardStates;
    }
}

[System.Serializable]
public class CardState
{
    public Sprite cardImage;
    public Vector2 position;
    public bool isMatched;
}
