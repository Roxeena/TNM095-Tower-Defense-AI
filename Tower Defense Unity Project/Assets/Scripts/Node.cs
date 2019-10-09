using UnityEngine;
using UnityEngine.EventSystems;

public class Node : MonoBehaviour {

	public Color hoverColor;
	public Color notEnoughMoneyColor;
    public Vector3 positionOffset;

	[HideInInspector]
	public GameObject turret;
	[HideInInspector]
	public TurretBlueprint turretBlueprint;
	[HideInInspector]
	public bool isUpgraded = false;

	private Renderer rend;
	private Color startColor;
    public int numTurrets = 0;

	BuildManager buildManager;

	void Start ()
	{
		rend = GetComponent<Renderer>();
		startColor = rend.material.color;

		buildManager = BuildManager.instance;
    }

	public Vector3 GetBuildPosition ()
	{
		return transform.position + positionOffset;
	}

	void OnMouseDown ()
	{
        //NOTE: DISABLED!
        return;
        if (EventSystem.current.IsPointerOverGameObject())
			return;

		if (turret != null)
		{
			buildManager.SelectNode(this);
			return;
		}

		if (!buildManager.CanBuild)
			return;

		BuildTurret(buildManager.GetTurretToBuild());
	}

    public void CalculateHeatMapColor()
    {
        float ratio = (float)numTurrets / (float)AI.maxTurretsPlaced;
        if (rend == null)
            rend = GetComponent<Renderer>();
        rend.material.color = new Color(1.0f - ratio, 1.0f - ratio, 1.0f);
    }

	public bool BuildTurret (TurretBlueprint blueprint)
	{
        if (blueprint == null)
            Debug.Log("No turret selected yet!");

        if (PlayerStats.Money < blueprint.cost)
		{
			//Debug.Log("Not enough money to build that!");
			return false;
		}

		PlayerStats.Money -= blueprint.cost;

		GameObject _turret = (GameObject)Instantiate(blueprint.prefab, GetBuildPosition(), Quaternion.identity);
		turret = _turret;

		turretBlueprint = blueprint;

		GameObject effect = (GameObject)Instantiate(buildManager.buildEffect, GetBuildPosition(), Quaternion.identity);
		Destroy(effect, 5f);

        //Debug.Log("Turret build!");
        return true;
	}

	public void UpgradeTurret ()
	{
		if (PlayerStats.Money < turretBlueprint.upgradeCost)
		{
			Debug.Log("Not enough money to upgrade that!");
			return;
		}

		PlayerStats.Money -= turretBlueprint.upgradeCost;

		//Get rid of the old turret
		Destroy(turret);

		//Build a new one
		GameObject _turret = (GameObject)Instantiate(turretBlueprint.upgradedPrefab, GetBuildPosition(), Quaternion.identity);
		turret = _turret;

		GameObject effect = (GameObject)Instantiate(buildManager.buildEffect, GetBuildPosition(), Quaternion.identity);
		Destroy(effect, 5f);

		isUpgraded = true;

		Debug.Log("Turret upgraded!");
	}

	public void SellTurret ()
	{
		PlayerStats.Money += turretBlueprint.GetSellAmount();

		GameObject effect = (GameObject)Instantiate(buildManager.sellEffect, GetBuildPosition(), Quaternion.identity);
		Destroy(effect, 5f);

		Destroy(turret);
		turretBlueprint = null;
	}

	void OnMouseEnter ()
	{
        //NOTE: DISABLED!
        return;
        if (EventSystem.current.IsPointerOverGameObject())
			return;

		if (!buildManager.CanBuild)
			return;

		if (buildManager.HasMoney)
		{
			rend.material.color = hoverColor;
		} else
		{
			rend.material.color = notEnoughMoneyColor;
		}

	}

	void OnMouseExit ()
	{
        //NOTE: DISABLED!
        return;
        rend.material.color = startColor;
    }

}
