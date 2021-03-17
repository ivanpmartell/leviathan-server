using System;
using System.Collections;

namespace UnityEngine
{
	internal class AttributeHelperEngine
	{
		private static Type[] GetRequiredComponents(Type klass)
		{
			ArrayList arrayList = null;
			while (klass != null && klass != typeof(MonoBehaviour))
			{
				object[] customAttributes = klass.GetCustomAttributes(typeof(RequireComponent), inherit: false);
				for (int i = 0; i < customAttributes.Length; i++)
				{
					RequireComponent requireComponent = (RequireComponent)customAttributes[i];
					if (arrayList == null && customAttributes.Length == 1 && klass.BaseType == typeof(MonoBehaviour))
					{
						return new Type[3] { requireComponent.m_Type0, requireComponent.m_Type1, requireComponent.m_Type2 };
					}
					if (arrayList == null)
					{
						arrayList = new ArrayList();
					}
					if (requireComponent.m_Type0 != null)
					{
						arrayList.Add(requireComponent.m_Type0);
					}
					if (requireComponent.m_Type1 != null)
					{
						arrayList.Add(requireComponent.m_Type1);
					}
					if (requireComponent.m_Type2 != null)
					{
						arrayList.Add(requireComponent.m_Type2);
					}
				}
				klass = klass.BaseType;
			}
			if (arrayList == null)
			{
				return null;
			}
			return (Type[])arrayList.ToArray(typeof(Type));
		}

		private static bool CheckIsEditorScript(Type klass)
		{
			while (klass != null && klass != typeof(MonoBehaviour))
			{
				object[] customAttributes = klass.GetCustomAttributes(typeof(ExecuteInEditMode), inherit: false);
				if (customAttributes.Length != 0)
				{
					return true;
				}
				klass = klass.BaseType;
			}
			return false;
		}
	}
}
