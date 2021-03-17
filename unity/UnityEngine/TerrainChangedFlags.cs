using System;

namespace UnityEngine
{
	[Flags]
	internal enum TerrainChangedFlags
	{
		Heightmap = 0x1,
		TreeInstances = 0x2,
		DelayedHeightmapUpdate = 0x4,
		FlushEverythingImmediately = 0x8,
		RemoveDirtyDetailsImmediately = 0x10,
		WillBeDestroyed = 0x100
	}
}
