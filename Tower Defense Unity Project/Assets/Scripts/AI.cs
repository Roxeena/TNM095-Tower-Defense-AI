using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor;

public class AI : MonoBehaviour {

    public struct Point
    {
        public Point(int inX, int inY) {
            xCord = inX;
            yCord = inY;
        }
        public int X() { return xCord; }
        public int Y() { return yCord; }

        private int xCord, yCord;
    }

    public struct TurrPoint
    {
        public TurrPoint(Turret turr, Point p, int value = 0) {
            turret = turr;
            point = p;
            fittness = value;
        }
        public Point P() { return point; }
        public Turret Turr() { return turret; }
        public float Fittness() { return fittness; }
        public void SetFittness(int value) { fittness = value; }

        //NOTE: Turret can be null if this is a read turrpoint from file
        private Turret turret;
        private Point point;
        private int fittness;
    }

    public enum Status
    {
        Learn, 
        Random, 
        Present
    };

    public TurretBlueprint standardTurret;
    public Status status;
    public List<TurrPoint> placedTurrets;
    public float crossProb;
    public float mutationProb;
    public const int populationSize = 10;
    public const int maxIterations = 15;

    private BuildManager buildManager;
    private const int SIZE = 16;
    private FileManager.Individual commandInduvidual;
    private int currentTurret = 0;
    private int notLearnedCounter = 0;
    private int oldScore = 0;
    private int notLearnedItterations = 5;

    public static AI instance;
    public static int indNr = 0;
    public static int itteration = 0;
    public static int maxTurretsPlaced = 0;

    void Awake()
    {
        //Singleton
        if (instance != null)
        {
            Debug.LogError("More than one AI in scene!");
            Destroy(this);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
        Debug.Log("Start ind: " + indNr.ToString());
        SceneManager.LoadScene("Level01");
    }

    void Start ()
    {
        buildManager = BuildManager.instance;
        buildManager.SelectTurretToBuild(standardTurret);
        placedTurrets = new List<TurrPoint>();

        if (status == Status.Learn)
            SetComands();
        if (status == Status.Present){
            //Read the entire population of previous generation
            List<FileManager.Individual> population = FileManager.instance.ReadPopulation();

            //Create a heat map
            HeatMap(population);

            //Find the best individual in the previous generation 
            int score = 0;
            int bestInd = -1;
            for(int i = 0; i < population.Count; ++i) {
                if (population[i].Fittness() > score) {
                    score = population[i].Fittness();
                    bestInd = i;
                }   
            }

            //Follow the comands given by the best individual
            Debug.Log("Best: " + bestInd);
            SetComands(bestInd);
        }
    }

    private void HeatMap(List<FileManager.Individual> population)
    {
        //For every node that has had a turret placed on it, 
        //increment the number of turrets placed on it
        Point p;
        string pointName;
        GameObject pointObject;
        Node pointNode;
        for (int i = 0; i < population.Count; ++i)
        {

            //Find the nodes that had turrets placed on them
            for (int t = 0; t < population[i].Turrets().Count; ++t)
            {
                p = population[i].Turrets()[t].P();

                pointName = "Node" + p.X() + "x" + p.Y();
                pointObject = GameObject.Find(pointName);
                if (pointObject == null)
                {
                    Debug.Log("WEIRED! Node: " + pointName + " NOT found!");
                    continue;
                }
                else
                {
                    //Increment the number of turrets placed on it
                    pointNode = pointObject.GetComponent<Node>();
                    pointNode.numTurrets++;

                    //Find the node with the max number of turrets placed, normalizes color
                    if (pointNode.numTurrets > maxTurretsPlaced)
                        maxTurretsPlaced = pointNode.numTurrets;
                }
            }
        }

        //Color the nodes based on the occurance of turrets on it
        for (int i = 1; i < SIZE + 1; ++i)
        {
            for (int j = 1; j < SIZE + 1; ++j)
            {
                pointName = "Node" + i + "x" + j;
                pointObject = GameObject.Find(pointName);
                if (pointObject == null)
                {
                    //Debug.Log("WEIRED! 2! Node: " + pointName + " NOT found!");
                    continue;
                }
                else
                {
                    pointNode = pointObject.GetComponent<Node>();
                    //Debug.Log("Node: " + pointName + " Turrets: " + pointNode.numTurrets.ToString());
                    pointNode.CalculateHeatMapColor();
                }
            }
        }
    }

    public void ResetTurrets()
    {
        placedTurrets = new List<TurrPoint>();
        currentTurret = 0;
    }

    //When a new wave is spawned, spend money on turrets
    public IEnumerator PrepareForWave()
    {
        //For every new round the AI is allowed to place only one turret
        //Except at the start of the game, then it can build three turrets

        //Start with random behaviour
        if (status == Status.Random)
        {
            //Random new indivudal
            //Place new random turrets

            //In first round place three turrets
            if (WaveSpawner.waveIndex == 0) {
                for (int i = 0; i < 3; i++) {
                    BuildRandomTurret();
                }
            }
            //The other rounds only place one turret
            else
                BuildRandomTurret();

        }
        //Both learning and present follows a comand individual
        else 
        {
            //Follow the comands in the file of this individual
            //In the end this individual will be evaluated
            if (commandInduvidual == null) {
                Debug.Log("Comand individual not set!");
            }
            else
            {
                //In first round place three turrets
                if (WaveSpawner.waveIndex == 0) {
                    for (int i = 0; i < 3 && currentTurret < commandInduvidual.Turrets().Count; i++) {
                        PlaceTurret();
                        ++currentTurret;
                    }
                }
                //The other rounds only place one turret
                else {
                    if(currentTurret < commandInduvidual.Turrets().Count) {
                        PlaceTurret();
                        ++currentTurret;
                    }
                    else
                        BuildRandomTurret();
                }
            } 
        }

        yield return new WaitForSeconds(1.0f);
    }

    private void PlaceTurret()
    {
        //Do not mutate during presentation
        //Add mutation
        float rand = UnityEngine.Random.Range(0.0f, 1.0f);
        if (status == AI.Status.Learn && rand < mutationProb) {
            //Mutate this turret
            BuildRandomTurret();
        }
        else {
            //Build predefined turret
            BuildTurret(commandInduvidual.Turrets()[currentTurret].P().X(),
                commandInduvidual.Turrets()[currentTurret].P().Y());
        }
    }

    public void SetComands(int ind = -1) {
        if (ind == -1)
            ind = indNr;
        commandInduvidual = FileManager.instance.ReadIndividual(ind);
        if(commandInduvidual == null)
        {
            Debug.Log("Comand individual NOT set!");
        }
    }

    //Evaluate the fitness function for this individual
    public int EvaluateIndividual()
    {
        int result = 0;

        //Loop throught all the turrets and calculate the average fittness over them
        int turretScore = 0;
        for(int i = 0; i < placedTurrets.Count; ++i)
        {
            turretScore = placedTurrets[i].Turr().EvaluateTurret();
            //TODO: Replace the turrpoint with a new turrpoint with the fittness
            placedTurrets[i].SetFittness(turretScore);
            result += turretScore;
        }

        //Add the number of rounds survived
        result += WaveSpawner.waveIndex * 2;
        return result;
    }

    public bool TerminateCheck()
    {
        //Read the individuals for this generation
        List<FileManager.Individual> population = FileManager.instance.ReadPopulation();

        //Calculate the total score for this generation
        int newScore = 0;
        for (int i = 0; i < population.Count; i++) {
            newScore += population[i].Fittness();
        }

        //If the AI have not improved anything for 3 itterations then terminate
        if(newScore < oldScore) {
            Debug.Log("NOT Improved: " + (newScore - oldScore));
            notLearnedCounter++;
        }
        else {
            Debug.Log("Improved: " + (newScore - oldScore));
            FileUtil.DeleteFileOrDirectory("BestSoFar.txt");
            FileUtil.CopyFileOrDirectory(FileManager.instance.evaluatedPopFile, "BestSoFar.txt");
            oldScore = newScore;
            notLearnedCounter = 0;
        }

        //Otherwise continue to learn
        if (notLearnedCounter < notLearnedItterations) {
            
            return false;
        }

        //Terminate
        Debug.Log("Im the best!");
        return true;
    }

    //Find random point within game grid. Ranom uses min inclusive but max exclusive, hence +1
    Point FindRandomPoint(int min = 1, int max = SIZE+1)
    {
        return new Point(UnityEngine.Random.Range(min, max), UnityEngine.Random.Range(min, max));
    }

    private void BuildRandomTurret()
    {
        GameObject randomObject;
        Point randomPoint;
        string nodeName;
        Node randomNode;

        //Loop until a sucessfull node is found. In case it tries to pick a node on the path
        do
        {
            randomPoint = FindRandomPoint();
            nodeName = "Node" + randomPoint.X() + "x" + randomPoint.Y();
            randomObject = GameObject.Find(nodeName);
            if (randomObject == null) {
                //Debug.Log("Node: " + nodeName + " NOT found!");
                continue;
            }
            else {
                randomNode = randomObject.GetComponent<Node>();
                if (randomNode.turret != null) {
                    //Debug.Log("Turret: " + nodeName + " already built!");
                    continue;
                }
                else
                    break;
            }
        } while (true);

        //Build turret at random node
        randomNode.BuildTurret(buildManager.GetTurretToBuild());
        placedTurrets.Add(new TurrPoint(randomNode.turret.GetComponent<Turret>(), randomPoint));

    }

    bool BuildTurret(int x, int y)
    {
        Point plannedPoint = new Point(x, y);
        string nodeName = "Node" + plannedPoint.X() + "x" + plannedPoint.Y();
        GameObject plannedObject = GameObject.Find(nodeName);
        Node plannedNode;

        if (plannedObject == null) {
            //Debug.Log("Node: " + nodeName + " NOT found!");
        }
        else {
            plannedNode = plannedObject.GetComponent<Node>();
            if (plannedNode.turret != null) {
                //Debug.Log("Turret: " + nodeName + " already built!");
                
            }
            else
            {
                plannedNode.BuildTurret(buildManager.GetTurretToBuild());
                placedTurrets.Add(new TurrPoint(plannedNode.turret.GetComponent<Turret>(), plannedPoint));
            }
        }
        return false;
    }
}
