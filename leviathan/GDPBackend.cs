using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class GDPBackend
{
	public class GDPShopItem
	{
		public int m_id;

		public string m_presentationKey = string.Empty;

		public string m_price = string.Empty;

		public bool m_discounted;

		public double m_discountPercentage;

		public string m_undiscountedPrice = string.Empty;

		public int m_dateYear;

		public int m_dateMonth;

		public int m_dateDay;

		public string m_packName;
	}

	public class GDPOwnedItem : IEquatable<GDPOwnedItem>
	{
		public string m_itemType = string.Empty;

		public int m_instance;

		public string m_packName = string.Empty;

		public bool Equals(GDPOwnedItem other)
		{
			return m_itemType == other.m_itemType && m_packName == other.m_packName;
		}
	}

	public Action<string> m_onBoughtItem;

	public Action<bool, string> m_onRedeemRespons;

	public Action<string> m_onOrderFailed;

	public Action<bool, string> m_onRestoreOwnedFinished;

	public Action<List<GDPShopItem>> m_onGotOfferList;

	public void UnlockAchievement(int id)
	{
		UnlockAchievement("achievement_" + id);
	}

	protected void AddPacks(ref List<GDPOwnedItem> items, string[] packNames)
	{
		foreach (string packName in packNames)
		{
			GDPOwnedItem gDPOwnedItem = new GDPOwnedItem();
			gDPOwnedItem.m_itemType = string.Empty;
			gDPOwnedItem.m_instance = 0;
			gDPOwnedItem.m_packName = packName;
			items.Add(gDPOwnedItem);
		}
	}

	public virtual void Close()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void RestoreOwned()
	{
	}

	public virtual bool CanPlaceOrders()
	{
		return true;
	}

	public virtual bool IsBackendOnline()
	{
		return true;
	}

	public abstract void RequestOffers();

	public abstract List<GDPOwnedItem> RequestOwned();

	public abstract void PlaceOrder(GDPShopItem item, string description);

	public virtual void UnlockAchievement(string name)
	{
	}

	public virtual void RedeemCode(string code)
	{
	}

	public virtual void OpenWebUrl(string url)
	{
		Application.OpenURL(url);
	}
}
