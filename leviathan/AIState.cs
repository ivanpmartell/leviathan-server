using System.IO;

public class AIState<OwnerType>
{
	public virtual string GetStatusText()
	{
		return string.Empty;
	}

	public virtual void Enter(OwnerType owner, AIStateMachine<OwnerType> sm)
	{
	}

	public virtual void Exit(OwnerType owner)
	{
	}

	public virtual void Update(OwnerType owner, AIStateMachine<OwnerType> sm, float dt)
	{
	}

	public virtual void Save(BinaryWriter writer)
	{
	}

	public virtual void Load(BinaryReader reader)
	{
	}

	public virtual string DebugString(OwnerType owner)
	{
		return string.Empty;
	}

	public virtual void GetCharageLevel(out float time, out float totalTime)
	{
		time = -1f;
		totalTime = -1f;
	}
}
