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
        FinalCheck,
        Completed
    }

    public SessionPhase CurrentPhase { get; private set; }

    // Symbols shown during pre-assessment and training phases
    private List<GHSSymbol> preAssessmentQueue = new List<GHSSymbol>();
    private List<GHSSymbol> trainingQueue = new List<GHSSymbol>();
    private List<GHSSymbol> finalCheckQueue = new List<GHSSymbol>();
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
        else if (CurrentPhase == SessionPhase.Training)
            return trainingQueue[currentIndex];
        else
            return finalCheckQueue[currentIndex];
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
        else if (CurrentPhase == SessionPhase.FinalCheck)
        {
            if (isCorrect)
                KnowledgeStateManager.Instance.SetState(current.id, KnowledgeStateManager.SymbolState.LearnedDuring);

            currentIndex++;

            if (currentIndex >= finalCheckQueue.Count)
                EndSession();
            else
                ShowNextSymbol();
        }
    }

    // Called when the user training and showing flags are finished
    public void AdvanceTeaching()
    {
        currentIndex++;

        if (currentIndex >= trainingQueue.Count)
            ShowFinalCheckTransition();
        else
            ShowNextSymbol();
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

        int knownCount = KnowledgeStateManager.Instance.GetKnownBeforeTraining();
        quizUI.ShowPhaseTransition(knownCount, 7);
    }

    public void BeginFinalCheck()
    {
        StartFinalCheck();
    }

    // Moves from training into the final multiple-choice recheck
    private void StartFinalCheck()
    {
        finalCheckQueue = new List<GHSSymbol>(trainingQueue);
        currentIndex = 0;
        CurrentPhase = SessionPhase.FinalCheck;

        Debug.Log($"Final check phase started. {finalCheckQueue.Count} symbol(s) to verify.");
        ShowNextSymbol();
    }

    private void ShowFinalCheckTransition()
    {
        Debug.Log("Training complete. Awaiting final check confirmation.");
        quizUI.ShowFinalCheckTransition(trainingQueue.Count);
    }

    // Called after transition delay
    public void BeginTraining()
    {
        ShowNextSymbol();
    }

    private void ShowNextSymbol()
    {
        GHSSymbol current = GetCurrentSymbol();
        Debug.Log($"Showing symbol: {current.display_name} | Phase: {CurrentPhase}");

        if (CurrentPhase == SessionPhase.Training)
            quizUI.ShowTraining(current);
        else
            quizUI.ShowQuestion(current);
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
        else if (CurrentPhase == SessionPhase.Training)
            return trainingQueue.Count;
        else
            return finalCheckQueue.Count;
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
