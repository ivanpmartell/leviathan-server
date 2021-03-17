using System;

namespace UnityEngine
{
	[Flags]
	public enum JointDriveMode
	{
		None = 0x0,
		Position = 0x1,
		Velocity = 0x2,
		PositionAndVelocity = 0x3
	}
}
