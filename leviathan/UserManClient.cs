using System.Collections.Generic;
using PTech;

public abstract class UserManClient
{
	public delegate void UpdatedHandler();

	public UpdatedHandler m_onUpdated;

	public List<ShipDef> GetShipListFromFleet(string fleetName, int campaignID)
	{
		return GetFleet(fleetName, campaignID)?.m_ships;
	}

	public abstract List<ShipDef> GetShipDefs(int campaignID);

	public abstract List<FleetDef> GetFleetDefs(int campaignID);

	public abstract List<string> GetAvailableMaps();

	public abstract List<string> GetUnlockedCampaignMaps(string campaign);

	public abstract List<string> GetAvailableCampaigns();

	public abstract List<string> GetAvailableShips(int campaignID);

	public abstract List<string> GetAvailableSections(int campaignID);

	public abstract List<string> GetAvailableHPModules(int campaignID);

	public abstract List<int> GetAvailableFlags();

	public abstract void AddShip(ShipDef ship);

	public abstract void AddFleet(FleetDef fleet);

	public abstract void RemoveShip(string name);

	public abstract void RemoveFleet(string fleet);

	public abstract FleetDef GetFleet(string name, int campaignID);

	public abstract void SetFlag(int flag);

	public abstract void UnlockAchievement(int id);

	public abstract void AddShipyardTime(float time);

	public abstract void BuyPackage(string packageName);

	public abstract void SetOwnedPackages(List<string> owned);

	protected void UpdateShipAvailability(List<ShipDef> shipDefs)
	{
		ComponentDB instance = ComponentDB.instance;
		List<string> availableShips = GetAvailableShips(0);
		List<string> availableSections = GetAvailableSections(0);
		List<string> availableHPModules = GetAvailableHPModules(0);
		foreach (ShipDef shipDef in shipDefs)
		{
			if (shipDef.m_campaignID == 0)
			{
				shipDef.UpdateAvailability(instance, availableShips, availableSections, availableHPModules);
			}
		}
	}

	protected void UpdateFleetAvailability(List<FleetDef> fleetDefs)
	{
		ComponentDB instance = ComponentDB.instance;
		List<string> availableShips = GetAvailableShips(0);
		List<string> availableSections = GetAvailableSections(0);
		List<string> availableHPModules = GetAvailableHPModules(0);
		foreach (FleetDef fleetDef in fleetDefs)
		{
			if (fleetDef.m_campaignID == 0)
			{
				fleetDef.UpdateAvailability(instance, availableShips, availableSections, availableHPModules);
			}
		}
	}
}
