using UnityEngine;
using System;
using System.IO;
using System.Collections;
using UnityEngine.SceneManagement;

public class AI : MonoBehaviour {

    struct Point
    {
        public Point(int inX, int inY){
            xCord = inX;
            yCord = inY;
        }
        public int X() { return xCord; }
        public int Y() { return yCord; }
        public void SetX(int inX) { xCord = inX; }
        public void SetY(int inY) { yCord = inY; }

        private int xCord, yCord;
    }

    public enum Status
    {
        Learn, 
        Random, 
        Present
    };

    public TurretBlueprint standardTurret;
    public Status status;

    private BuildManager buildManager;
    private const int SIZE = 16;

    public static AI instance;

    void Awake()
    {
        //Singleton
        if (instance != null)
        {
            Debug.LogError("More than one AI in scene!");
            return;
        }
        instance = this;
    }

    void Start ()
    {
        buildManager = BuildManager.instance;
        buildManager.SelectTurretToBuild(standardTurret);

        //If learn then open file
        //Learn from that data or present what you learned
        
    }

    //When a new wave is spawned, spend money on turrets
    public IEnumerator PrepareForWave()
    {
        if(status == Status.Random)
        {
            //Create a new individual with random behavior
            while (BuildRandomTurret())
                ;
            yield return new WaitForSeconds(1.0f);
        }
        else if(status == Status.Learn)
        {
            //Learn form what has been done before
        }
        else if(status == Status.Present)
        {
            //Present the best solution
        }     
    }

    //Evaluate the fitness function for this individual
    public float EvaluateIndividual()
    {
        float result = 0.0f;

        //Loop throught all the turrets and calculate the average fittness over them
        Turret[] turrets = FindObjectsOfType<Turret>();
        float sumTurretEval = 0.0f;
        for(int i = 0; i < turrets.Length; ++i)
        {
            sumTurretEval += turrets[i].EvaluateTurret();
        }
        result = sumTurretEval / turrets.Length;

        //Add the number of rounds survived
        result += WaveSpawner.waveIndex;
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
                Debug.Log("Node: " + nodeName + " NOT found!");
                continue;
            }
            else {
                randomNode = randomObject.GetComponent<Node>();
                if (randomNode.turret != null) {
                    Debug.Log("Turret: " + nodeName + " already built!");
                    continue;
                }
                else
                    break;
            }
        } while (true);
        
        //Build turret at random node
        return randomNode.BuildTurret(buildManager.GetTurretToBuild()); 
    }

    bool BuildTurret(int x, int y)
    {
        Point plannedPoint = new Point(x, y);
        string nodeName = "Node" + plannedPoint.X() + "x" + plannedPoint.Y();
        GameObject plannedObject = GameObject.Find(nodeName);
        Node plannedNode;

        if (plannedObject == null) {
            Debug.Log("Node: " + nodeName + " NOT found!");
        }
        else {
            plannedNode = plannedObject.GetComponent<Node>();
            if (plannedNode.turret != null) {
                Debug.Log("Turret: " + nodeName + " already built!");
            }
            else
                return plannedNode.BuildTurret(buildManager.GetTurretToBuild());
        }
        return false;
    }
}
