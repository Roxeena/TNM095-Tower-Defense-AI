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
        EndLevel();
        //gameOverUI.SetActive(true);
        sceneFader.FadeTo(SceneManager.GetActiveScene().name);
    }

	public void WinLevel ()
	{
        EndLevel();
        //completeLevelUI.SetActive(true);
        sceneFader.FadeTo(SceneManager.GetActiveScene().name);
    }

    private void EndLevel()
    {
        GameIsOver = true;

        //If presented game, shut down
        if (AI.instance.status == AI.Status.Present) {
            Debug.Log("Presentation done!");
            ShutDown();
        }   

        //Evaluate this individual
        int fittness = AI.instance.EvaluateIndividual();

        //Save what this individual learned in file
        FileManager.instance.SaveCurrentIndividual(fittness);
        AI.instance.ResetTurrets();

        //Restart game and let new individual play and be evaluated
        ++AI.indNr;
        Debug.Log("Individual: " + AI.indNr.ToString() + " fit: " + fittness.ToString());

        //In learning phase set a new comander
        if (AI.instance.status == AI.Status.Learn)
            AI.instance.SetComands();

        //Check if time to reset or stop
        if (AI.indNr == AI.populationSize)
        {
            AI.itteration++;
            Debug.Log("Itteration: " + AI.itteration.ToString());
            //Stop the AI after a number of iterations or other demand
            //TODO: Set a demand for when to stop based on the learning
            if (AI.itteration == AI.maxIterations || AI.instance.TerminateCheck()) {
                ShutDown();
            }
            //If a new population has been created, evolve that population
            else
            {
                AI.instance.status = AI.Status.Learn;
                FileManager.instance.Evolve();
                AI.indNr = 0;
                AI.instance.SetComands();
            }
        }
    }

    public static void ShutDown()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

}
