public class FleetSizes
{
	public static FleetSize[] sizes = new FleetSize[6]
	{
		new FleetSize(1, 3000),
		new FleetSize(1, 6000),
		new FleetSize(1, 8000),
		new FleetSize(1, 6000),
		new FleetSize(0, 0),
		new FleetSize(0, 0)
	};

	public static FleetSizeClass GetSizeClass(int points)
	{
		if (sizes[0].ValidSize(points))
		{
			return FleetSizeClass.Small;
		}
		if (sizes[1].ValidSize(points))
		{
			return FleetSizeClass.Medium;
		}
		if (sizes[2].ValidSize(points))
		{
			return FleetSizeClass.Heavy;
		}
		return FleetSizeClass.Custom;
	}

	public static string GetSizeClassName(int points)
	{
		return GetSizeClass(points) switch
		{
			FleetSizeClass.Small => "fleetsize_small", 
			FleetSizeClass.Medium => "fleetsize_medium", 
			FleetSizeClass.Heavy => "fleetsize_large", 
			_ => "fleetsize_invalid", 
		};
	}

	public static FleetSizeClass GetSizeClass(FleetSize size)
	{
		if (sizes[0].IsEqual(size))
		{
			return FleetSizeClass.Small;
		}
		if (sizes[1].IsEqual(size))
		{
			return FleetSizeClass.Medium;
		}
		if (sizes[2].IsEqual(size))
		{
			return FleetSizeClass.Heavy;
		}
		return FleetSizeClass.Custom;
	}

	public static string GetSizeClassName(FleetSize size)
	{
		PLog.Log("GetSizeClassName " + size.min + " / " + size.max);
		if (sizes[0].IsEqual(size))
		{
			return "fleetsize_small";
		}
		if (sizes[1].IsEqual(size))
		{
			return "fleetsize_medium";
		}
		if (sizes[2].IsEqual(size))
		{
			return "fleetsize_large";
		}
		return string.Empty;
	}

	public static string GetSizeLimit(FleetSize fleetSize, int size)
	{
		if (size < fleetSize.min)
		{
			return "fleetsize_minimum";
		}
		if (size > fleetSize.max)
		{
			return "fleetsize_exceeded";
		}
		if (sizes[0].ValidSize(size))
		{
			return "fleetsize_small";
		}
		if (sizes[1].ValidSize(size))
		{
			return "fleetsize_medium";
		}
		if (sizes[2].ValidSize(size))
		{
			return "fleetsize_large";
		}
		return "fleetsize_invalid";
	}
}
