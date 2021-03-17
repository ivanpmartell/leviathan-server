using System;
using System.Runtime.CompilerServices;
using UnityEngineInternal;

namespace UnityEngine
{
	public class Component : Object
	{
		public Transform transform
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public Rigidbody rigidbody
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public Camera camera
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public Light light
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public Animation animation
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public ConstantForce constantForce
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public Renderer renderer
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public AudioSource audio
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public GUIText guiText
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public NetworkView networkView
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		[Obsolete("Please use guiTexture instead")]
		public GUIElement guiElement
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public GUITexture guiTexture
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public Collider collider
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public HingeJoint hingeJoint
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public ParticleEmitter particleEmitter
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public ParticleSystem particleSystem
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public GameObject gameObject
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		[Obsolete("the active property is deprecated on components. Please use gameObject.active instead. If you meant to enable / disable a single component use enabled instead.")]
		public bool active
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public string tag
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		[TypeInferenceRule(TypeInferenceRules.TypeReferencedByFirstArgument)]
		public extern Component GetComponent(Type type);

		public T GetComponent<T>() where T : Component
		{
			return GetComponent(typeof(T)) as T;
		}

		public Component GetComponent(string type)
		{
			return gameObject.GetComponent(type);
		}

		[TypeInferenceRule(TypeInferenceRules.TypeReferencedByFirstArgument)]
		public Component GetComponentInChildren(Type t)
		{
			return gameObject.GetComponentInChildren(t);
		}

		public T GetComponentInChildren<T>() where T : Component
		{
			return (T)GetComponentInChildren(typeof(T));
		}

		public Component[] GetComponentsInChildren(Type t)
		{
			bool includeInactive = false;
			return GetComponentsInChildren(t, includeInactive);
		}

		public Component[] GetComponentsInChildren(Type t, bool includeInactive)
		{
			return gameObject.GetComponentsInChildren(t, includeInactive);
		}

		public T[] GetComponentsInChildren<T>(bool includeInactive) where T : Component
		{
			return gameObject.GetComponentsInChildren<T>(includeInactive);
		}

		public T[] GetComponentsInChildren<T>() where T : Component
		{
			return GetComponentsInChildren<T>(includeInactive: false);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern Component[] GetComponents(Type type);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern Component[] GetComponentsWithCorrectReturnType(Type type);

		public T[] GetComponents<T>() where T : Component
		{
			return (T[])GetComponentsWithCorrectReturnType(typeof(T));
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern bool CompareTag(string tag);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void SendMessageUpwards(string methodName, object value, SendMessageOptions options);

		public void SendMessageUpwards(string methodName, object value)
		{
			SendMessageOptions options = SendMessageOptions.RequireReceiver;
			SendMessageUpwards(methodName, value, options);
		}

		public void SendMessageUpwards(string methodName)
		{
			SendMessageOptions options = SendMessageOptions.RequireReceiver;
			object value = null;
			SendMessageUpwards(methodName, value, options);
		}

		public void SendMessageUpwards(string methodName, SendMessageOptions options)
		{
			SendMessageUpwards(methodName, null, options);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void SendMessage(string methodName, object value, SendMessageOptions options);

		public void SendMessage(string methodName, object value)
		{
			SendMessageOptions options = SendMessageOptions.RequireReceiver;
			SendMessage(methodName, value, options);
		}

		public void SendMessage(string methodName)
		{
			SendMessageOptions options = SendMessageOptions.RequireReceiver;
			object value = null;
			SendMessage(methodName, value, options);
		}

		public void SendMessage(string methodName, SendMessageOptions options)
		{
			SendMessage(methodName, null, options);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void BroadcastMessage(string methodName, object parameter, SendMessageOptions options);

		public void BroadcastMessage(string methodName, object parameter)
		{
			SendMessageOptions options = SendMessageOptions.RequireReceiver;
			BroadcastMessage(methodName, parameter, options);
		}

		public void BroadcastMessage(string methodName)
		{
			SendMessageOptions options = SendMessageOptions.RequireReceiver;
			object parameter = null;
			BroadcastMessage(methodName, parameter, options);
		}

		public void BroadcastMessage(string methodName, SendMessageOptions options)
		{
			BroadcastMessage(methodName, null, options);
		}
	}
}
