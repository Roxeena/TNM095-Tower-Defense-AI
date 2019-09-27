using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	public static bool GameIsOver;

	public GameObject gameOverUI;
	public GameObject completeLevelUI;
    public SceneFader sceneFader;

    void Start ()
	{
        GameIsOver = false;
	}

	// Update is called once per frame
	void Update () {
		if (GameIsOver)
			return;

		if (PlayerStats.Lives <= 0)
		{
			EndGame();
		}
	}

	void EndGame ()
	{
		GameIsOver = true;
        float fittness = AI.instance.EvaluateIndividual();
		gameOverUI.SetActive(true);

        //Save what learned in file
        FileManager.instance.SaveProgress(fittness);
        //Restart game and learn more
        AI.numIndividuals++;
        if (AI.numIndividuals == AI.maxIndividuals)
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        sceneFader.FadeTo(SceneManager.GetActiveScene().name);
    }

	public void WinLevel ()
	{
		GameIsOver = true;
        float fittness = AI.instance.EvaluateIndividual();
        completeLevelUI.SetActive(true);

        //Save what learned in file
        FileManager.instance.SaveProgress(fittness);
        //Restart game and learn more
        AI.numIndividuals++;
        Debug.Log("Quit Ind: " + AI.numIndividuals);
        if (AI.numIndividuals == AI.maxIndividuals)
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        sceneFader.FadeTo(SceneManager.GetActiveScene().name);
    }

}
