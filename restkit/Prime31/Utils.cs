using System;
using System.Collections;
using System.Text;
using UnityEngine;

namespace Prime31
{
	public static class Utils
	{
		private static System.Random _random;

		private static System.Random random
		{
			get
			{
				if (_random == null)
				{
					_random = new System.Random();
				}
				return _random;
			}
		}

		public static string randomString(int size = 38)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < size; i++)
			{
				char value = Convert.ToChar(Convert.ToInt32(Math.Floor(26.0 * random.NextDouble() + 65.0)));
				stringBuilder.Append(value);
			}
			return stringBuilder.ToString();
		}

		public static void logObject(object obj)
		{
			StringBuilder stringBuilder = new StringBuilder();
			addObjectToString(stringBuilder, obj);
			Debug.Log(stringBuilder.ToString());
		}

		private static void addObjectToString(StringBuilder builder, object obj, string indenter = "")
		{
			if (obj is string)
			{
				builder.AppendFormat("{0}{1}\n", indenter, obj);
			}
			else if (obj is IList)
			{
				addIListToString(builder, obj as IList, indenter + "\t");
			}
			else if (obj is IDictionary)
			{
				addIDictionaryToString(builder, obj as IDictionary, indenter);
			}
			else
			{
				builder.AppendFormat("{0}{1}\n", indenter, obj);
			}
		}

		private static void addIDictionaryToString(StringBuilder builder, IDictionary dict, string indenter = "")
		{
			builder.AppendFormat("{0} --------- IDictionary ---------\n", indenter);
			IEnumerator enumerator = dict.Keys.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object current = enumerator.Current;
					if (dict[current] is IList)
					{
						builder.AppendFormat("\n{0} --------- IList ---------\n", indenter);
					}
					builder.AppendFormat("{0}{1}: ", indenter, current);
					addObjectToString(builder, dict[current], indenter);
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
		}

		private static void addIListToString(StringBuilder builder, IList result, string indenter = "")
		{
			builder.Append("\n");
			IEnumerator enumerator = result.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object current = enumerator.Current;
					addObjectToString(builder, current, indenter);
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
		}
	}
}
