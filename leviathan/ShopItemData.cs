using PTech;

public class ShopItemData
{
	public GDPBackend.GDPShopItem m_gdpItem;

	public ContentPack.Category m_type;

	public string m_name;

	public bool m_owned;

	public string m_price = string.Empty;

	public string m_undiscountedPrice = string.Empty;

	public bool m_newItem;

	public double m_discountPercentage;

	public ShopItemData(GDPBackend.GDPShopItem item, ContentPack.Category type, string name, bool owned, string price, string undiscountedPrice, double discountPercentage, bool newItem)
	{
		m_gdpItem = item;
		m_type = type;
		m_name = name;
		m_price = price;
		m_undiscountedPrice = undiscountedPrice;
		m_owned = owned;
		m_newItem = newItem;
		m_discountPercentage = discountPercentage;
	}
}
