using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class FileManager : MonoBehaviour {

    public class Individual
    {
        public Individual(List<AI.TurrPoint> nodes, int score)
        {
            turrets = nodes;
            fittness = score;
        }
        public List<AI.TurrPoint> Turrets() { return turrets; }
        public int Fittness() { return fittness; }

        private List<AI.TurrPoint> turrets;
        private int fittness; 
    }

    class IndProbability
    {
        public IndProbability(int ind, float score)
        {
            individual = ind;
            probability = score;
        }
        public int Ind() { return individual; }
        public float Probability() { return probability; }

        private  int individual;
        private float probability;
    }

    public string saveFile;

    private StreamWriter writer;
    private StreamReader reader;

    public static FileManager instance;

    void Awake()
    {
        //Singleton
        if (instance != null)
        {
            Debug.LogError("More than one FileManager in scene!");
            Destroy(this);
            return;
        }
        instance = this;
    }

    void Start()
    {
        if(saveFile.Length != 0 && saveFile.EndsWith(".txt"))
            CheckFile(saveFile);
        else
        {
            Debug.Log("Please enter a .txt file name!");
            GameManager.ShutDown();
        }
    }

    public void SaveIndividual(int f)
    {
        //Write the current individual to file, creatinga population over time
        writer = new StreamWriter(saveFile, true);
        List<AI.TurrPoint> turrets = AI.instance.placedTurrets;
        string nodeText;
        string totalText = "";
        for (int i = 0; i < turrets.Count; ++i)
        {
            string xPos = turrets[i].P().X().ToString();
            string yPos = turrets[i].P().Y().ToString();
            //TODO: Fix the saving of fittness into a turrpoint
            string score = turrets[i].Turr().EvaluateTurret().ToString();

            xPos = xPos.PadLeft(2, '0');
            yPos = yPos.PadLeft(2, '0');
            score = score.PadLeft(4, '0');

            nodeText = xPos + "x" + yPos + "-" + score + "N";
            totalText += nodeText;
        }

        string fittness = f.ToString();
        fittness = fittness.PadLeft(4, '0');
        totalText += "__E" + fittness;
        writer.WriteLine(totalText);
        if (writer != null)
            writer.Close();
    }

    public Individual ReadIndividual()
    {
        string currentInd = reader.ReadLine();
        if(currentInd == null || currentInd.Length == 0)
        {
            Debug.Log("Could NOT read individual!");
            return null;
        }
            
        List<AI.TurrPoint> oldTurrets = new List<AI.TurrPoint>();
        int posX = 0, posY = 0, score = 0, fittness = 0;

        int i = 0;
        while (i < currentInd.Length)
        {
            //One char in the individual
            if (currentInd[i] == 'x')
            {
                Debug.Log("Found Point!");
                posX = currentInd[i - 2] * 10 + currentInd[i - 1];
                posY = currentInd[i + 1] * 10 + currentInd[i + 2];
                i += 3;
            }
            else if (currentInd[i] == '-')
            {
                Debug.Log("Found score!");
                score = currentInd[i + 1] * 1000 + currentInd[i + 2] * 100 + currentInd[i + 3] * 10 + currentInd[i + 4];
                i += 5;
            }
            else if (currentInd[i] == 'N')
            {
                Debug.Log("Found node!");
                oldTurrets.Add(new AI.TurrPoint(null, new AI.Point(posX, posY), score));
                i += 3;
            }
            else if (currentInd[i] == 'E')
            {
                Debug.Log("Found fittness!");
                fittness = currentInd[i + 1] * 1000 + currentInd[i + 2] * 100 + currentInd[i + 3] * 10 + currentInd[i + 4];
                break;
            }
            else
            {
                if(i > 1)
                    Debug.Log("Something might be wrong with reading!" + i);
                ++i;
            }     
        }
        return new Individual(oldTurrets, fittness);
    }

    public List<Individual> ReadPopulation()
    {
        //Read the entire save file into a list of individuals
        //One individual is a list of turrnodes and an individual score
        List<Individual> oldPopulation = new List<Individual>();
        reader = new StreamReader(saveFile);

        //Read all individuals
        Individual ind = ReadIndividual();
        while(ind != null)
        {
            oldPopulation.Add(ind);
            ind = ReadIndividual();
        }

        if (reader != null)
            reader.Close();

        return oldPopulation;
    }

    public void Evolve()
    {
        List<Individual> population = ReadPopulation();
        List<Individual> selection = new List<Individual>();

        //Selection
        selection = Selection(population);


        //CrossOver
        //Generate the random pairs

        //Perform recombination for these pairs

        //Mutation
        //Fot every turret in this entire population make a few random turrets of them
    }

    private List<Individual> Selection(List<Individual> population)
    {
        List<Individual> selection = new List<Individual>();
        //Add up all the individulas fittness
        int totalFittness = 0;
        for (int i = 0; i < population.Count; ++i) {
            totalFittness += population[i].Fittness();
        }
        if (totalFittness == 0) {
            Debug.Log("total fittness = 0!");
        }
        //Calculate the probability of one individual to be chosen, Roulette wheel method
        List<IndProbability> popProbability = new List<IndProbability>();
        for (int i = 0; i < population.Count; i++) {
            popProbability.Add(new IndProbability(i, (float)population[i].Fittness() / (float)totalFittness));
        }

        /*Debug.Log("Individuals before sort: ");
        for (int i = 0; i < popProbability.Count; i++) {
            Debug.Log("ID: " + popProbability[i].Ind().ToString() + " Prob: " + popProbability[i].Probability().ToString());
        }*/

        //Sort on propability in decreasing order
        popProbability.Sort(delegate (IndProbability lhs, IndProbability rhs)
        {
            //Sort in decending order of probability
            if (lhs.Probability() == rhs.Probability())
                return 0;
            else if (lhs.Probability() < rhs.Probability())
                return 1;
            else //if (lhs.Probability() > rhs.Probability()
                return -1;
        });

        Debug.Log("Individuals after sort: ");
        for (int i = 0; i < popProbability.Count; i++) {
            Debug.Log("ID: " + popProbability[i].Ind().ToString() + " Prob: " + popProbability[i].Probability().ToString());
        }

        //Select the same amount of individuals as size of population, duplicates may occour
        for (int i = 0; i < popProbability.Count; ++i)
        {
            //Generate a random number between 0, 1
            float rand = Random.Range(0.0f, 1.0f);

            //Important that sorted in decreeasing propability
            float temp = 0.0f;
            for (int j = 0; j < popProbability.Count; ++j)
            {
                //if it is less than probability for A (ex: 0.79), choose A
                if (rand < popProbability[j].Probability() + temp)
                {
                    //Select this individual
                    selection.Add(population[popProbability[j].Ind()]);
                    Debug.Log("Selected: " + popProbability[j].Ind().ToString() + " rand: " + rand.ToString());
                    break;
                }
                //Otherwise if it is less than probability for A + B (ex: 0.79 + 0.15), choose B
                else
                {
                    temp += popProbability[j].Probability();
                }
            }
        }

        return selection;
    }

    private void CheckFile(string file)
    {
        if (!File.Exists(file)) {
            File.Create(file);
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
        if (writer != null)
            writer.Close();
        if (reader != null)
            reader.Close();
    }
}
