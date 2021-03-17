namespace UnityEngine
{
	public enum RigidbodyConstraints
	{
		None = 0,
		FreezePositionX = 2,
		FreezePositionY = 4,
		FreezePositionZ = 8,
		FreezeRotationX = 0x10,
		FreezeRotationY = 0x20,
		FreezeRotationZ = 0x40,
		FreezePosition = 14,
		FreezeRotation = 112,
		FreezeAll = 126
	}
}
