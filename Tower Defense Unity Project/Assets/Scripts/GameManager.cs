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
        AI.instance.EvaluateIndividual();
		gameOverUI.SetActive(true);

        //Save what learned in file
        //Restart game and learn more
        sceneFader.FadeTo(SceneManager.GetActiveScene().name);
    }

	public void WinLevel ()
	{
		GameIsOver = true;
        AI.instance.EvaluateIndividual();
        completeLevelUI.SetActive(true);

        //Save what learned in file
        //Restart game and learn more
        sceneFader.FadeTo(SceneManager.GetActiveScene().name);
    }

}
