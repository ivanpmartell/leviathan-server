#define DEBUG
using System.Collections.Generic;
using System.IO;

public class AIStateMachine<OwnerType>
{
	private GenericFactory<AIState<OwnerType>> m_stateFactory;

	private Stack<AIState<OwnerType>> m_stateStack = new Stack<AIState<OwnerType>>();

	private OwnerType m_owner;

	public AIStateMachine(OwnerType owner, GenericFactory<AIState<OwnerType>> stateFactory)
	{
		m_stateFactory = stateFactory;
		m_owner = owner;
	}

	public AIState<OwnerType> GetActiveState()
	{
		if (m_stateStack.Count > 0)
		{
			return m_stateStack.Peek();
		}
		return null;
	}

	public AIState<OwnerType> ChangeState(string stateName)
	{
		AIState<OwnerType> aIState = m_stateFactory.Create(stateName);
		if (aIState == null)
		{
			PLog.LogError("Missing ai state " + stateName);
			return null;
		}
		ChangeState(aIState);
		return aIState;
	}

	public void ChangeState(AIState<OwnerType> newTopState)
	{
		if (m_stateStack.Count > 0)
		{
			AIState<OwnerType> aIState = m_stateStack.Pop();
			aIState.Exit(m_owner);
		}
		m_stateStack.Push(newTopState);
		newTopState.Enter(m_owner, this);
	}

	public void PushState(string stateName)
	{
		AIState<OwnerType> aIState = m_stateFactory.Create(stateName);
		if (aIState == null)
		{
			PLog.LogError("Missing ai state " + stateName);
		}
		else
		{
			PushState(aIState);
		}
	}

	public void PushState(AIState<OwnerType> newTopState)
	{
		if (m_stateStack.Count > 0)
		{
			m_stateStack.Peek()?.Exit(m_owner);
		}
		m_stateStack.Push(newTopState);
		newTopState.Enter(m_owner, this);
	}

	public void PopChildStates(AIState<OwnerType> parent)
	{
		while (m_stateStack.Count > 0 && m_stateStack.Peek() != parent)
		{
			PopState();
		}
	}

	public void PopState()
	{
		if (m_stateStack.Count != 0)
		{
			AIState<OwnerType> aIState = m_stateStack.Pop();
			aIState.Exit(m_owner);
			if (m_stateStack.Count > 0)
			{
				AIState<OwnerType> aIState2 = m_stateStack.Peek();
				aIState2.Enter(m_owner, this);
			}
			if (m_stateStack.Count == 0)
			{
				PLog.LogWarning("Warning, statemachine is empty");
			}
		}
	}

	public void Update(float dt)
	{
		if (m_stateStack.Count > 0)
		{
			m_stateStack.Peek()?.Update(m_owner, this, dt);
		}
	}

	public void Save(BinaryWriter writer)
	{
		writer.Write((byte)m_stateStack.Count);
		AIState<OwnerType>[] array = m_stateStack.ToArray();
		for (int num = array.Length - 1; num >= 0; num--)
		{
			AIState<OwnerType> aIState = array[num];
			string typeName = m_stateFactory.GetTypeName(aIState);
			writer.Write(typeName);
			aIState.Save(writer);
		}
	}

	public void Load(BinaryReader reader)
	{
		int num = reader.ReadByte();
		m_stateStack.Clear();
		for (int i = 0; i < num; i++)
		{
			string name = reader.ReadString();
			AIState<OwnerType> aIState = m_stateFactory.Create(name);
			DebugUtils.Assert(aIState != null);
			aIState.Load(reader);
			m_stateStack.Push(aIState);
		}
	}

	public override string ToString()
	{
		string text = "AIState:\n";
		AIState<OwnerType>[] array = m_stateStack.ToArray();
		foreach (AIState<OwnerType> aIState in array)
		{
			string typeName = m_stateFactory.GetTypeName(aIState);
			string text2 = text;
			text = text2 + "   " + typeName + ": " + aIState.DebugString(m_owner) + "\n";
		}
		return text;
	}
}
