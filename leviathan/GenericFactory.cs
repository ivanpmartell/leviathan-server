using System;
using System.Collections.Generic;

public class GenericFactory<T>
{
	private Dictionary<string, Type> m_states = new Dictionary<string, Type>();

	public void Register<TY>(string name)
	{
		m_states.Add(name, typeof(TY));
	}

	public T Create(string name, params object[] args)
	{
		if (m_states.TryGetValue(name, out var value))
		{
			return (T)Activator.CreateInstance(value, args);
		}
		return default(T);
	}

	public string GetTypeName(T refObject)
	{
		Type type = refObject.GetType();
		foreach (KeyValuePair<string, Type> state in m_states)
		{
			if (state.Value == type)
			{
				return state.Key;
			}
		}
		return null;
	}

	public int GetNrOfRegisteredTypes()
	{
		return m_states.Count;
	}
}
