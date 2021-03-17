public class FleetSize
{
	public int min;

	public int max;

	public FleetSize(int min, int max)
	{
		this.min = min;
		this.max = max;
	}

	public bool IsEqual(FleetSize size)
	{
		if (min != size.min)
		{
			return false;
		}
		if (max != size.max)
		{
			return false;
		}
		return true;
	}

	public bool ValidSize(int size, bool dubble)
	{
		if (dubble)
		{
			return size >= min * 2 && size <= max * 2;
		}
		return size >= min && size <= max;
	}

	public bool ValidSize(int size)
	{
		return size >= min && size <= max;
	}

	public override string ToString()
	{
		return max.ToString();
	}
}
