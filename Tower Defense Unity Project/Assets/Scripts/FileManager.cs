using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class FileManager : MonoBehaviour {

    public class Individual
    {
        public Individual(List<AI.TurrPoint> nodes, int score = NO_SCORE)
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

    public string evaluatedPopFile;
    public string newPopFile;

    private StreamWriter writer;
    private StreamReader reader;
    private const int NO_SCORE = 0;

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
        if(evaluatedPopFile.Length != 0 && evaluatedPopFile.EndsWith(".txt"))
            CheckFile(evaluatedPopFile);
        else
        {
            Debug.Log("Please enter a .txt file name for evaluated population!");
            GameManager.ShutDown();
        }

        if (newPopFile.Length != 0 && newPopFile.EndsWith(".txt"))
            CheckFile(newPopFile);
        else
        {
            Debug.Log("Please enter a .txt file name for new population!");
            GameManager.ShutDown();
        }
    }

    public void SaveCurrentIndividual(int f)
    {
        //Do not save the individual after a presentation
        if (AI.instance.status == AI.Status.Present) {
            return;
        } 

        //Write the current individual to file, creatinga population over time
        writer = new StreamWriter(evaluatedPopFile, true);
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

    private Individual ReadIndividual()
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
                posX = int.Parse(currentInd[i - 2].ToString() + currentInd[i - 1]);
                posY = int.Parse(currentInd[i + 1].ToString() + currentInd[i + 2]);
                i += 3;
            }
            else if (currentInd[i] == '-')
            {
                score = int.Parse(currentInd[i + 1].ToString() + currentInd[i + 2] + currentInd[i + 3] + currentInd[i + 4]);
                i += 5;
            }
            else if (currentInd[i] == 'N')
            {
                oldTurrets.Add(new AI.TurrPoint(null, new AI.Point(posX, posY), score));
                i += 3;
            }
            else if (currentInd[i] == 'E')
            {
                fittness = int.Parse(currentInd[i + 1].ToString() + currentInd[i + 2] + currentInd[i + 3] + currentInd[i + 4]);
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

    public Individual ReadIndividual(int num)
    {
        reader = new StreamReader(newPopFile);
        for (int i = 0; i < num; ++i) {
            if (ReadIndividual() == null)
                Debug.Log("End of file reached!");
        }

        Individual temp = ReadIndividual();
        
        if (reader != null)
            reader.Close();
        return temp;
    }

    public List<Individual> ReadPopulation()
    {
        //Read the entire save file into a list of individuals
        //One individual is a list of turrnodes and an individual score
        List<Individual> population = new List<Individual>();

        //Read all individuals
        reader = new StreamReader(evaluatedPopFile);
        Individual ind = ReadIndividual();
        while(ind != null)
        {
            population.Add(ind);
            ind = ReadIndividual();
        }

        if (reader != null)
            reader.Close();

        return population;
    }

    public void Evolve()
    {
        //Do not save the individual after a presentation
        if (AI.instance.status == AI.Status.Present) {
            return;
        }

        List<Individual> population = ReadPopulation();
        List<Individual> selection = new List<Individual>();
        List<Individual> crossedOver = new List<Individual>();

        /*Debug.Log("Read population: ");
        for (int i = 0; i < population.Count; i++)
        {
            Debug.Log("ind: " + i.ToString());
            for (int t = 0; t < population[i].Turrets().Count; t++)
            {
                Debug.Log(population[i].Turrets()[t].P().X().ToString() + "x" + 
                    population[i].Turrets()[t].P().X().ToString() + "-" + 
                    population[i].Turrets()[t].Fittness().ToString());
            }
            Debug.Log("E" + population[i].Fittness().ToString());
        }*/

        //Selection
        selection = Selection(population);

        //CrossOver
        crossedOver = Crossover(selection);

        //NOTE: Mutation is performed during runtime when the turrets are built

        //Save this new population 
        if(crossedOver.Count < 1) {
            Debug.Log("Code BLUE!");
        }
        else {
            //Overwrite the old saveFile for not evaluated  population
            SaveIndividual(crossedOver[0], newPopFile, false);

            for (int i = 1; i < crossedOver.Count; ++i) {
                SaveIndividual(crossedOver[i], newPopFile);
            }

            //Clear the save file with evaluated population
            writer = new StreamWriter(evaluatedPopFile, false);
            if (writer != null)
                writer.Close();
        }
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

        /*Debug.Log("Individuals after sort: ");
        for (int i = 0; i < popProbability.Count; i++) {
            Debug.Log("ID: " + popProbability[i].Ind().ToString() + " Prob: " + popProbability[i].Probability().ToString());
        }*/

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
                    //Debug.Log("Selected: " + popProbability[j].Ind().ToString() + " rand: " + rand.ToString());
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

    private List<Individual> Crossover(List<Individual> population)
    {
        List<Individual> crossedOver = new List<Individual>();
        
        //Choose the pairs on how they lie in the list, next to each other
        if(population.Count < 2) {
            Debug.Log("To few individuals to preform crossover!");
        }

        int i = 0;
        Individual he, she;
        List<AI.TurrPoint> gTurrets = new List<AI.TurrPoint>(), bTurrets = new List<AI.TurrPoint>();
        float rand;
        while(i < population.Count)
        {
            //Check boundary
            if((i + 1) == population.Count)
            {
                Debug.Log("Uneven number of individuals!");
                //Reached end of list and uneven number of individuals
                //Add the old individual to the new population
                crossedOver.Add(population[i]);
            }
            
            //Select the pair
            she = population[i];
            he = population[i + 1];

            //For every pair there is a probability if crossover should be done or not
            rand = Random.Range(0.0f, 1.0f);
            if(rand < AI.instance.crossProb)
            {
                //Debug.Log("Performing CrossOver for " + i.ToString() + " - " + (i + 1).ToString());
                //Perform the crossover
                int crossPoint = Random.Range(0, Mathf.Min(she.Turrets().Count, he.Turrets().Count));

                //Swap the turrets before the cross Point
                for (int j = 0; j < crossPoint; ++j) {
                    gTurrets.Add(he.Turrets()[j]);
                    bTurrets.Add(she.Turrets()[j]);
                }

                //Filll up with the remaining turrets
                for(int j = crossPoint; j < she.Turrets().Count; ++j) {
                    gTurrets.Add(she.Turrets()[j]);
                }
                for (int j = crossPoint; j < he.Turrets().Count; ++j) {
                    bTurrets.Add(he.Turrets()[j]);
                }

                //Add the new children to the new population
                crossedOver.Add(new Individual(gTurrets));
                crossedOver.Add(new Individual(bTurrets));
            }
            else
            {
                //Debug.Log("NO CrossOver for " + i.ToString() + " - " + (i + 1).ToString());
                crossedOver.Add(she);
                crossedOver.Add(he);
            }
            i += 2;
        }
        return crossedOver;
    }

    private void SaveIndividual(Individual ind, string file, bool append = true)
    {
        //Write the given individual to file, evolving population over time
        writer = new StreamWriter(newPopFile, append);
        List<AI.TurrPoint> turrets = ind.Turrets();
        string nodeText;
        string totalText = "";
        for (int i = 0; i < turrets.Count; ++i)
        {
            string xPos = turrets[i].P().X().ToString();
            string yPos = turrets[i].P().Y().ToString();
            //TODO: Fix the saving of fittness into a turrpoint
            string score = turrets[i].Fittness().ToString();

            xPos = xPos.PadLeft(2, '0');
            yPos = yPos.PadLeft(2, '0');
            score = score.PadLeft(4, '0');

            nodeText = xPos + "x" + yPos + "-" + score + "N";
            totalText += nodeText;
        }

        string fittness = ind.Fittness().ToString();
        fittness = fittness.PadLeft(4, '0');
        totalText += "__E" + fittness;
        writer.WriteLine(totalText);
        if (writer != null)
            writer.Close();
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
