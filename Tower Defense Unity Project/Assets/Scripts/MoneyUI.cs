using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MoneyUI : MonoBehaviour {

	public Text moneyText;

    private static int ind = 1;
    private static int ittr = 1;
    private static int fitt = 0;

    public void UpdateText(int inInd, int inIttr, int inFitt) {
        moneyText.text = "Individual: " + inInd + " Itteration: " + inIttr + " Fit: " + inFitt;
        ind = inInd;
        ittr = inIttr;
        fitt = inFitt;
    }

    //Update is called once per frame
    void Update()
    {
        moneyText.text = "Individual: " + ind + " Itteration: " + ittr + " Fit: " + fitt;
    }
}
