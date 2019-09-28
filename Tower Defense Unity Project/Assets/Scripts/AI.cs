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
        public TurrPoint(Turret turr, Point p) {
            turret = turr;
            point = p;
            fittness = 0.0f;
        }
        public Point P() { return point; }
        public Turret Turr() { return turret; }
        public float Fittness() { Debug.Log("Ret Fittness: " + fittness);  return fittness; }
        public void SetFittness(float value)
        {
            fittness = value;
            Debug.Log("Set Fittness: " + fittness);    
        }

        private Turret turret;
        private Point point;
        private float fittness;
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

    private BuildManager buildManager;
    private const int SIZE = 16;
    public const int maxIndividuals = 2;

    public static AI instance;
    public static int numIndividuals = 0;

    void Awake()
    {
        //Singleton
        if (instance != null)
        {
            Debug.LogError("More than one AI in scene!");
            return;
        }
        instance = this;
        Debug.Log("Ind: " + numIndividuals);
    }

    void Start ()
    {
        buildManager = BuildManager.instance;
        buildManager.SelectTurretToBuild(standardTurret);

        //If learn then open file
        //Learn from that data or present what you learned
        placedTurrets = new List<TurrPoint>();
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
        float sumTurretEval = 0.0f;
        float turretScore = 0.0f;
        for(int i = 0; i < placedTurrets.Count; ++i)
        {
            turretScore = placedTurrets[i].Turr().EvaluateTurret();
            placedTurrets[i].SetFittness(turretScore);
            Debug.Log("Score: " + placedTurrets[i].Fittness());
            sumTurretEval += turretScore;
        }
        result = sumTurretEval;

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
