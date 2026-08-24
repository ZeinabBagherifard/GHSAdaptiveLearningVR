using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GeminiProxyClient : MonoBehaviour
{
    [SerializeField] private string baseUrl = "http://127.0.0.1:8000";

    [System.Serializable]
    private class ExplanationRequest
    {
        public string symbol;
        public string wrong_answer;
        public string correct_meaning;
        public string safety_tip;
        public string confused_symbol;
        public string confused_meaning;
    }

    [System.Serializable]
    private class ExplanationResponse
    {
        public string symbol;
        public string explanation;
    }

    public void RequestExplanation(
        string symbol,
        string wrongAnswer,
        string correctMeaning,
        string safetyTip,
        string confusedSymbol,
        string confusedMeaning,
        System.Action<string> onSuccess,
        System.Action<string> onError)
    {
        StartCoroutine(
            SendExplanationRequest(
                symbol, wrongAnswer, correctMeaning, safetyTip,
                confusedSymbol, confusedMeaning, onSuccess, onError
            )
        );
    }

    private IEnumerator SendExplanationRequest(
        string symbol,
        string wrongAnswer,
        string correctMeaning,
        string safetyTip,
        string confusedSymbol,
        string confusedMeaning,
        System.Action<string> onSuccess,
        System.Action<string> onError)
    {
        var reqObj = new ExplanationRequest
        {
            symbol = symbol,
            wrong_answer = wrongAnswer,
            correct_meaning = correctMeaning,
            safety_tip = safetyTip,
            confused_symbol = confusedSymbol ?? "",
            confused_meaning = confusedMeaning ?? ""
        };

        string json = JsonUtility.ToJson(reqObj);

        UnityWebRequest request = new UnityWebRequest(baseUrl + "/explain", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"Gemini proxy request failed: {request.error}");
            onError?.Invoke(request.error);
        }
        else
        {
            ExplanationResponse response = JsonUtility.FromJson<ExplanationResponse>(request.downloadHandler.text);
            onSuccess?.Invoke(response.explanation);
        }
    }
}
