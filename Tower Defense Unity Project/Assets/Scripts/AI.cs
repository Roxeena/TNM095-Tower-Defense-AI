using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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

    private BuildManager buildManager;
    private const int SIZE = 16;
    public const int populationSize = 10;
    public const int maxIterations = 10;
    private FileManager.Individual commandInduvidual;
    private int currentTurret = 0;

    public static AI instance;
    public static int indNr = 0;
    public static int itteration = 0;

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
    }

    public void ResetTurrets()
    {
        placedTurrets = new List<TurrPoint>();
        currentTurret = 0;
    }

    //When a new wave is spawned, spend money on turrets
    public IEnumerator PrepareForWave()
    {
        if(status == Status.Random)
        {
            //Random new indivudal
            //Place new random turrets
            while (BuildRandomTurret())
                ;
            yield return new WaitForSeconds(1.0f);
        }
        else if(status == Status.Learn)
        {
            //Follow the comands in the file of this individual
            //In the end this individual will be evaluated
            bool success = true;
            if (commandInduvidual == null) {
                Debug.Log("Comand individual not set!");
            }
            else
            {
                while (success && currentTurret < commandInduvidual.Turrets().Count)
                {
                    //Add mutation
                    float rand = UnityEngine.Random.Range(0.0f, 1.0f);
                    if (rand < mutationProb)
                    {
                        //Mutate this turret
                        Debug.Log("Mutate!");
                        success = BuildRandomTurret();
                    }
                    else
                    {
                        //Build predefined turret
                        Debug.Log("Follow the comands");
                        success = BuildTurret(commandInduvidual.Turrets()[currentTurret].P().X(),
                        commandInduvidual.Turrets()[currentTurret].P().Y());
                    }
                    if (success)
                        ++currentTurret;
                }
            } 
        }
        else if(status == Status.Present)
        {
            //Best individual
            //Build the best turret
            //TODO!
        }     
    }

    public void SetComands() {
        commandInduvidual = FileManager.instance.ReadIndividual(indNr);
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

    //Find random point within game grid. Ranom uses min inclusive but max exclusive, hence +1
    Point FindRandomPoint(int min = 1, int max = SIZE+1)
    {
        return new Point(UnityEngine.Random.Range(min, max), UnityEngine.Random.Range(min, max));
    }

    bool BuildRandomTurret()
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
        if (randomNode.BuildTurret(buildManager.GetTurretToBuild()))
        {
            placedTurrets.Add(new TurrPoint(randomNode.turret.GetComponent<Turret>(), randomPoint));
            return true;
        }
        else
            return false;
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
                if(plannedNode.BuildTurret(buildManager.GetTurretToBuild()))
                {
                    placedTurrets.Add(new TurrPoint(plannedNode.turret.GetComponent<Turret>(), plannedPoint));
                    return true;
                }
                else
                    return false;
            }
        }
        return false;
    }
}
