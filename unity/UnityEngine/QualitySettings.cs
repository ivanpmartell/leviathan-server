using System;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class QualitySettings : Object
	{
		public static string[] names
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		[Obsolete("Use GetQualityLevel and SetQualityLevel")]
		public static QualityLevel currentLevel
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public static int pixelLightCount
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public static ShadowProjection shadowProjection
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public static int shadowCascades
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public static float shadowDistance
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public static int masterTextureLimit
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public static AnisotropicFiltering anisotropicFiltering
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public static float lodBias
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public static int maximumLODLevel
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public static bool softVegetation
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public static int maxQueuedFrames
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public static int vSyncCount
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public static int antiAliasing
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public static ColorSpace desiredColorSpace
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public static ColorSpace activeColorSpace
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public static BlendWeights blendWeights
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
		public static extern int GetQualityLevel();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void SetQualityLevel(int index, bool applyExpensiveChanges);

		public static void SetQualityLevel(int index)
		{
			bool applyExpensiveChanges = true;
			SetQualityLevel(index, applyExpensiveChanges);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void IncreaseLevel(bool applyExpensiveChanges);

		public static void IncreaseLevel()
		{
			bool applyExpensiveChanges = false;
			IncreaseLevel(applyExpensiveChanges);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void DecreaseLevel(bool applyExpensiveChanges);

		public static void DecreaseLevel()
		{
			bool applyExpensiveChanges = false;
			DecreaseLevel(applyExpensiveChanges);
		}
	}
}
