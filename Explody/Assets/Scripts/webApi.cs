using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Serializable]
public class Scores
{
    public string score;
    public string username;

    public string level;

      public string SaveToString()
    {
        return JsonUtility.ToJson(this);
    }
}

[System.Serializable]
public class ScoresWrapper
{
    public Scores[] allScores;
}



public class webApi : MonoBehaviour
{
    public Text feedbackMsg;
    public InputField username;
    public InputField score;
    public InputField level;

    readonly string getScoresURL = "http://dpollan.ngrok.io/getScores";
    readonly string postScoreURL = "http://dpollan.ngrok.io/postScores";


    // Start is called before the first frame update
    void Start()
    {
        feedbackMsg.text = "press 'Get Scores' or 'Put Score' button";
    }

    public void onButtonGetScore() 
    {
        feedbackMsg.text = "Loading High Scores...";
        StartCoroutine(FetchScores());
    }

    IEnumerator FetchScores() {
        UnityWebRequest fetchScoresReq = UnityWebRequest.Get(getScoresURL);
        yield return fetchScoresReq.SendWebRequest();

        if(fetchScoresReq.isNetworkError || fetchScoresReq.isHttpError) {
            Debug.Log("Error: " + fetchScoresReq.error);
        } else {
            ScoresWrapper wrappedScores = JsonUtility.FromJson<ScoresWrapper>("{\"allScores\":" + fetchScoresReq.downloadHandler.text + "}");
            Debug.Log(wrappedScores.allScores[0].username);
            string result = "High Scores\n";
            foreach (Scores entry in wrappedScores.allScores)
            {
                result += entry.username + "               " + entry.score + "\n";
            }
            feedbackMsg.text = result;
        }
    }

    public void onButtonPostScore() 
    {
        if ((score.text == string.Empty) || (username.text == string.Empty)) {
            feedbackMsg.text = "Not all required fields have been provided";
        } else {
            feedbackMsg.text = "Sending Score Data..."; 

            //Create a user entry based on what the user entered
            Scores userEntry = new Scores();
            userEntry.username = username.text;
            userEntry.score = score.text;
            userEntry.level = level.text;

            //Now make it a Json object
            string json = userEntry.SaveToString();
            
            StartCoroutine(postScore(postScoreURL, json));

        }

    }

    IEnumerator postScore(string url, string json) 
    {
        var postScoreReq = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        postScoreReq.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        postScoreReq.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        postScoreReq.SetRequestHeader("Content-Type", "application/json");
        Debug.Log("here is json: " + json);
    
        yield return postScoreReq.SendWebRequest();

        if (postScoreReq.isNetworkError || postScoreReq.isHttpError) {
            Debug.LogError("Error: " + postScoreReq.error);
        }

        else 
        {
            //unpack json file
            ScoresWrapper wrappedScores = JsonUtility.FromJson<ScoresWrapper>("{\"allScores\":" + postScoreReq.downloadHandler.text + "}");
            Debug.Log(wrappedScores.allScores[0].username);
            string result = "High Scores\n";
            //iterate through each score in order and put them on screen
            foreach (Scores entry in wrappedScores.allScores)
            {
                result += entry.username + "               " + entry.score + "\n";
            }
            feedbackMsg.text = result;
        }
    }
}

public static class JsonHelper
{
	public static Scores[] FromJson<Scores>(string json)
	{
		Wrapper<Scores> wrapper = JsonUtility.FromJson<Wrapper<Scores>>(json);
		return wrapper.Items;
	}

	public static string ToJson<Scores>(Scores[] array)
	{
		Wrapper<Scores> wrapper = new Wrapper<Scores>();
		wrapper.Items = array;
		return JsonUtility.ToJson(wrapper);
	}

	public static string ToJson<Scores>(Scores[] array, bool prettyPrint)
	{
		Wrapper<Scores> wrapper = new Wrapper<Scores>();
		wrapper.Items = array;
		return JsonUtility.ToJson(wrapper, prettyPrint);
	}

	[System.Serializable]
	private class Wrapper<Scores>
	{
		public Scores[] Items;
	}
}

