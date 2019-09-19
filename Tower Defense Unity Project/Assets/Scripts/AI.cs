using UnityEngine;
using System;

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

    public TurretBlueprint standardTurret;

    BuildManager buildManager;
    const int SIZE = 16;

    void Start ()
    {
        buildManager = BuildManager.instance;
        buildManager.SelectTurretToBuild(standardTurret);

        //Build three random turrets
        //TODO: Taske care of edge case where build turret on same place
        BuildRandomTurret();
        BuildRandomTurret();
        BuildRandomTurret();
    }

    void Update()
    {
        
    }

    //Find random point within game grid. Ranom uses min inclusive but max exclusive, hence +1
    Point FindRandomPoint(int min = 1, int max = SIZE+1)
    {
        return new Point(UnityEngine.Random.Range(min, max), UnityEngine.Random.Range(min, max));
    }

    void BuildRandomTurret()
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
            randomNode = randomObject.GetComponent<Node>();
            if (randomObject == null) 
                Debug.Log("Node: " + nodeName + " NOT found!");
            else if(randomNode.turret != null)
                Debug.Log("Turret: " + nodeName + " already built!");

        } while (randomObject == null || randomNode.turret != null);
        
        //Build turret at random node
        randomNode.BuildTurret(buildManager.GetTurretToBuild());
    }
}
