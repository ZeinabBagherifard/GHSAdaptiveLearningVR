using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Manages the quiz UI — displays symbols, answer buttons, and feedback
public class QuizUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject introPanel;
    public GameObject questionPanel; 
    
    [Header("UI References")]
    public Image symbolImage;
    public TextMeshProUGUI questionText;
    public List<Button> answerButtons;
    public TextMeshProUGUI feedbackText;

    private GHSSymbol currentSymbol;

    void Start()
    {
        // Show intro, hide quiz on startup
        introPanel.SetActive(true);
        questionPanel.SetActive(false);
    }

    public void OnStartButtonClicked()
    {
        introPanel.SetActive(false);
        questionPanel.SetActive(true);
        SessionManager.Instance.StartSession();
    }

    // Display a symbol and its shuffled answer options
    public void ShowQuestion(GHSSymbol symbol)
    {
        currentSymbol = symbol;

        // Load and display the symbol image from Resources
        Sprite sprite = Resources.Load<Sprite>(symbol.image_resource);
        if (sprite != null)
            symbolImage.sprite = sprite;
        else
            Debug.LogWarning($"Image not found: {symbol.image_resource}");

        questionText.text = "What does this symbol mean?";
        feedbackText.text = "";

        // Combine correct and wrong options then shuffle to randomise button order
        List<string> options = new List<string>(symbol.wrong_options);
        options.Add(symbol.correct_option);
        Shuffle(options);

        // Assign each option to a button
        for (int i = 0; i < answerButtons.Count; i++)
        {
            string option = options[i];
            TextMeshProUGUI buttonText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = option;

            // Clear previous listeners and assign new one
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(option));
        }
    }

    private void OnAnswerSelected(string selectedOption)
    {
        bool isCorrect = selectedOption == currentSymbol.correct_option;

        if (isCorrect)
        {
            feedbackText.text = "Correct!";
            feedbackText.color = Color.green;
        }
        else
        {
            feedbackText.text = "Incorrect. Try again.";
            feedbackText.color = Color.red;
        }

        SessionManager.Instance.SubmitAnswer(isCorrect);
    }

    // Fisher-Yates shuffle to randomise answer order
    private void Shuffle(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            string temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
