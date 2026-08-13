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
    public GameObject answerGroup;
    public GameObject endPanel;
    public GameObject feedbackPanel;

    [Header("UI References")]
    public Image symbolImage;
    public TextMeshProUGUI questionText;
    public List<Button> answerButtons;
    public TextMeshProUGUI feedbackText;    
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI scoreText;


    private GHSSymbol currentSymbol;

    void Start()
    {
        introPanel.SetActive(true);
        questionPanel.SetActive(false);
        endPanel.SetActive(false);
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
        // Stop any running coroutines from previous question
        StopAllCoroutines();
        
        // Re-enable and show buttons for new question
        foreach (Button btn in answerButtons)
            btn.interactable = true;
        answerGroup.SetActive(true);

        // Hide feedback panel for new question
        feedbackText.gameObject.SetActive(false);
        feedbackPanel.SetActive(false);

        currentSymbol = symbol;

        // Update progress counter
        int current = SessionManager.Instance.GetCurrentIndex() + 1;
        int total = SessionManager.Instance.GetTotalCount();
        progressText.text = $"{SessionManager.Instance.CurrentPhase} | Symbol {current} of {total}";

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
            string capturedOption = options[i];
            TextMeshProUGUI buttonText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = capturedOption;

            // Clear previous listeners and assign new one
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(capturedOption));
        }
    }

    private void OnAnswerSelected(string selectedOption)
    {

        // Hide buttons to show feedback prominently
        answerGroup.SetActive(false);
        feedbackText.gameObject.SetActive(true);
        feedbackPanel.SetActive(true);

        bool isCorrect = selectedOption == currentSymbol.correct_option;

        if (SessionManager.Instance.CurrentPhase == SessionManager.SessionPhase.PreAssessment)
        {
            if (isCorrect)
            {
                feedbackText.text = $"Correct!\n{currentSymbol.display_name} — {currentSymbol.correct_meaning}";
                feedbackText.color = Color.green;
                StartCoroutine(SubmitAfterDelay(isCorrect, 3f));
            }
            else
            {
                feedbackText.text = "Not quite. You will practise this in training.";
                feedbackText.color = new Color(1f, 0.8f, 0f);
                StartCoroutine(SubmitAfterDelay(isCorrect, 2f));
            }
        }
        else
        {
            if (isCorrect)
            {
                feedbackText.text = $"Correct!\n{currentSymbol.display_name} — {currentSymbol.correct_meaning}";
                feedbackText.color = Color.green;
                StartCoroutine(SubmitAfterDelay(isCorrect, 3f));
            }
            else
            {
                feedbackText.text = "Not quite. Keep going.";
                feedbackText.color = new Color(1f, 0.8f, 0f);
                StartCoroutine(SubmitAfterDelay(isCorrect, 1.5f));
            }
        }
    }

    private IEnumerator SubmitAfterDelay(bool isCorrect, float delay)
    {
        yield return new WaitForSeconds(delay);
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

    // Briefly shows a transition message between pre-assessment and training
    public void ShowPhaseTransition(int knownCount, int totalCount)
    {
        // Hide question content, show only transition message
        answerGroup.SetActive(false);
        symbolImage.gameObject.SetActive(false);
        questionText.gameObject.SetActive(false);
        progressText.gameObject.SetActive(false);

        // Show a simple message then start training
        feedbackText.gameObject.SetActive(true);
        feedbackPanel.SetActive(true);
        feedbackText.fontSize = 26;
        feedbackText.text = $"Pre-Assessment Complete!\n\nYou already knew {knownCount} out of {totalCount} symbols.\n\nNow let's practise the ones you missed.";
        feedbackText.color = Color.white;
        StartCoroutine(TransitionToTraining());
    }

    private IEnumerator TransitionToTraining()
    {
        yield return new WaitForSeconds(5f);
        // Restore question elements
        symbolImage.gameObject.SetActive(true);
        questionText.gameObject.SetActive(true);
        progressText.gameObject.SetActive(true);
        SessionManager.Instance.BeginTraining();

    }
    // Hides the question panel and displays the KPI end screen with session results
    public void ShowEndScreen(int knownBefore, int learned, int stillStruggling, int total)
    {
        questionPanel.SetActive(false);
        endPanel.SetActive(true);

        int totalKnown = knownBefore + learned;
        float accuracy = (totalKnown / (float)total) * 100f;
        string passOrFail = accuracy >= 80f ? "PASS" : "FAIL";

        scoreText.text =
            $"Symbols already known:      {knownBefore} / {total}\n" +
            $"Learned during training:       {learned} / {total}\n" +
            $"Still needs practice:              {stillStruggling} / {total}\n\n" +
            $"Final accuracy:                      {accuracy:0}%\n\n" +
            $"Result:                                   {passOrFail}";
    }

    public void OnRestartButtonClicked()
    {
        endPanel.SetActive(false);
        introPanel.SetActive(true);
    }
}
