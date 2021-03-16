using System.IO;

public interface IOrderable
{
	void SaveOrders(BinaryWriter stream);

	void LoadOrders(BinaryReader stream);

	void AddOrder(Order order);

	bool RemoveOrder(Order order);

	bool RemoveFirstOrder();

	void ClearOrders();

	bool RemoveLastOrder();

	bool IsLastOrder(Order order);

	void OnOrdersChanged();
}
