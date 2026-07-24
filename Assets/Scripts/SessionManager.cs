using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls the overall session flow: pre-assessment,training,end screen
public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance;
    private QuizUIManager quizUI;

    public enum SessionPhase
    {
        PreAssessment,
        Training,
        Completed
    }

    public SessionPhase CurrentPhase { get; private set; }

    // Symbols shown during pre-assessment and training phases
    private List<GHSSymbol> preAssessmentQueue = new List<GHSSymbol>();
    private List<GHSSymbol> trainingQueue = new List<GHSSymbol>();
    // Tracks the current position in the active queue
    private int currentIndex = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        quizUI = FindObjectOfType<QuizUIManager>();         
    }

    // Initialises the session and begins pre-assessment, Session starts when user clicks Start on intro screen
    public void StartSession()
    {
        // Reset all symbols to Untested before the session begins
        KnowledgeStateManager.Instance.Initialise();

        // Load all symbols into pre-assessment queue
        preAssessmentQueue = new List<GHSSymbol>(GHSDataLoader.Database.symbols);
        currentIndex = 0;
        CurrentPhase = SessionPhase.PreAssessment;

        Debug.Log("Session started. Pre-assessment phase beginning.");
        ShowNextSymbol();
    }

    // Returns the current symbol to display
    public GHSSymbol GetCurrentSymbol()
    {
        if (CurrentPhase == SessionPhase.PreAssessment)
            return preAssessmentQueue[currentIndex];
        else
            return trainingQueue[currentIndex];
    }

    // Returns the current position in the active queue
    public int GetCurrentIndex()
    {
        return currentIndex;
    }

    public int GetTotalCount()
    {
        if (CurrentPhase == SessionPhase.PreAssessment)
            return preAssessmentQueue.Count;
        else
            return trainingQueue.Count;
    }

    // Called by the UI when the user selects an answer
    public void SubmitAnswer(bool isCorrect)
    {
        GHSSymbol current = GetCurrentSymbol();

        if (CurrentPhase == SessionPhase.PreAssessment)
        {
            if (isCorrect)
                KnowledgeStateManager.Instance.SetState(current.id, KnowledgeStateManager.SymbolState.KnownBefore);
            else
                KnowledgeStateManager.Instance.SetState(current.id, KnowledgeStateManager.SymbolState.Struggling);

            currentIndex++;

            if (currentIndex >= preAssessmentQueue.Count)
                StartTrainingPhase();
            else
                ShowNextSymbol();
        }
        else if (CurrentPhase == SessionPhase.Training)
        {
            if (isCorrect)
                KnowledgeStateManager.Instance.SetState(current.id, KnowledgeStateManager.SymbolState.LearnedDuring);

            currentIndex++;

            if (currentIndex >= trainingQueue.Count)
                EndSession();
            else
                ShowNextSymbol();
        }
    }

    // Builds training queue from unknown symbols only
    private void StartTrainingPhase()
    {
        trainingQueue = KnowledgeStateManager.Instance.GetUnknownSymbols();
        currentIndex = 0;
        CurrentPhase = SessionPhase.Training;

        if (trainingQueue.Count == 0)
        {
            Debug.Log("User knows all symbols. Skipping training.");
            EndSession();
            return;
        }

        Debug.Log($"Training phase started. {trainingQueue.Count} symbol(s) to train.");
        ShowNextSymbol();
    }

    private void ShowNextSymbol()
    {
        GHSSymbol current = GetCurrentSymbol();
        Debug.Log($"Showing symbol: {current.display_name} | Phase: {CurrentPhase}");

        // Tell the UI to display the current symbol
        quizUI.ShowQuestion(current);
    }

    private void EndSession()
    {
        CurrentPhase = SessionPhase.Completed;
        Debug.Log("Session completed.");

        // Calculate KPIs
        var knownBefore = KnowledgeStateManager.Instance.GetKnownBeforeTraining();
        var learned = KnowledgeStateManager.Instance.GetLearnedDuringTraining();
        var struggling = KnowledgeStateManager.Instance.GetStillStruggling();

        quizUI.ShowEndScreen(knownBefore, learned, struggling, 7);
    }
}
