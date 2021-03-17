using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class Camera : Behaviour
	{
		public float fov
		{
			get
			{
				return fieldOfView;
			}
			set
			{
				fieldOfView = value;
			}
		}

		public float near
		{
			get
			{
				return nearClipPlane;
			}
			set
			{
				nearClipPlane = value;
			}
		}

		public float far
		{
			get
			{
				return farClipPlane;
			}
			set
			{
				farClipPlane = value;
			}
		}

		public float fieldOfView
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float nearClipPlane
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float farClipPlane
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public RenderingPath renderingPath
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public RenderingPath actualRenderingPath
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public bool hdr
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float orthographicSize
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public bool orthographic
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public TransparencySortMode transparencySortMode
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public bool isOrthoGraphic
		{
			get
			{
				return orthographic;
			}
			set
			{
				orthographic = value;
			}
		}

		public float depth
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float aspect
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public int cullingMask
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Color backgroundColor
		{
			get
			{
				INTERNAL_get_backgroundColor(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_backgroundColor(ref value);
			}
		}

		public Rect rect
		{
			get
			{
				INTERNAL_get_rect(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_rect(ref value);
			}
		}

		public Rect pixelRect
		{
			get
			{
				INTERNAL_get_pixelRect(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_pixelRect(ref value);
			}
		}

		public RenderTexture targetTexture
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float pixelWidth
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public float pixelHeight
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public Matrix4x4 cameraToWorldMatrix
		{
			get
			{
				INTERNAL_get_cameraToWorldMatrix(out var value);
				return value;
			}
		}

		public Matrix4x4 worldToCameraMatrix
		{
			get
			{
				INTERNAL_get_worldToCameraMatrix(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_worldToCameraMatrix(ref value);
			}
		}

		public Matrix4x4 projectionMatrix
		{
			get
			{
				INTERNAL_get_projectionMatrix(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_projectionMatrix(ref value);
			}
		}

		public Vector3 velocity
		{
			get
			{
				INTERNAL_get_velocity(out var value);
				return value;
			}
		}

		public CameraClearFlags clearFlags
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public static Camera main
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public static Camera current
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public static Camera[] allCameras
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public static Camera mainCamera => main;

		public bool useOcclusionCulling
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float[] layerCullDistances
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public bool layerCullSpherical
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public DepthTextureMode depthTextureMode
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
		private extern void INTERNAL_get_backgroundColor(out Color value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_backgroundColor(ref Color value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_rect(out Rect value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_rect(ref Rect value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_pixelRect(out Rect value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_pixelRect(ref Rect value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_cameraToWorldMatrix(out Matrix4x4 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_worldToCameraMatrix(out Matrix4x4 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_worldToCameraMatrix(ref Matrix4x4 value);

		public void ResetWorldToCameraMatrix()
		{
			INTERNAL_CALL_ResetWorldToCameraMatrix(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_ResetWorldToCameraMatrix(Camera self);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_projectionMatrix(out Matrix4x4 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_projectionMatrix(ref Matrix4x4 value);

		public void ResetProjectionMatrix()
		{
			INTERNAL_CALL_ResetProjectionMatrix(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_ResetProjectionMatrix(Camera self);

		public void ResetAspect()
		{
			INTERNAL_CALL_ResetAspect(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_ResetAspect(Camera self);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_velocity(out Vector3 value);

		public Vector3 WorldToScreenPoint(Vector3 position)
		{
			return INTERNAL_CALL_WorldToScreenPoint(this, ref position);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern Vector3 INTERNAL_CALL_WorldToScreenPoint(Camera self, ref Vector3 position);

		public Vector3 WorldToViewportPoint(Vector3 position)
		{
			return INTERNAL_CALL_WorldToViewportPoint(this, ref position);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern Vector3 INTERNAL_CALL_WorldToViewportPoint(Camera self, ref Vector3 position);

		public Vector3 ViewportToWorldPoint(Vector3 position)
		{
			return INTERNAL_CALL_ViewportToWorldPoint(this, ref position);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern Vector3 INTERNAL_CALL_ViewportToWorldPoint(Camera self, ref Vector3 position);

		public Vector3 ScreenToWorldPoint(Vector3 position)
		{
			return INTERNAL_CALL_ScreenToWorldPoint(this, ref position);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern Vector3 INTERNAL_CALL_ScreenToWorldPoint(Camera self, ref Vector3 position);

		public Vector3 ScreenToViewportPoint(Vector3 position)
		{
			return INTERNAL_CALL_ScreenToViewportPoint(this, ref position);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern Vector3 INTERNAL_CALL_ScreenToViewportPoint(Camera self, ref Vector3 position);

		public Vector3 ViewportToScreenPoint(Vector3 position)
		{
			return INTERNAL_CALL_ViewportToScreenPoint(this, ref position);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern Vector3 INTERNAL_CALL_ViewportToScreenPoint(Camera self, ref Vector3 position);

		public Ray ViewportPointToRay(Vector3 position)
		{
			return INTERNAL_CALL_ViewportPointToRay(this, ref position);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern Ray INTERNAL_CALL_ViewportPointToRay(Camera self, ref Vector3 position);

		public Ray ScreenPointToRay(Vector3 position)
		{
			return INTERNAL_CALL_ScreenPointToRay(this, ref position);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern Ray INTERNAL_CALL_ScreenPointToRay(Camera self, ref Vector3 position);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern float GetScreenWidth();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern float GetScreenHeight();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void DoClear();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void Render();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void RenderWithShader(Shader shader, string replacementTag);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void SetReplacementShader(Shader shader, string replacementTag);

		public void ResetReplacementShader()
		{
			INTERNAL_CALL_ResetReplacementShader(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_ResetReplacementShader(Camera self);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void RenderDontRestore();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void SetupCurrent(Camera cur);

		public bool RenderToCubemap(Cubemap cubemap)
		{
			int faceMask = 63;
			return RenderToCubemap(cubemap, faceMask);
		}

		public bool RenderToCubemap(Cubemap cubemap, int faceMask)
		{
			return Internal_RenderToCubemapTexture(cubemap, faceMask);
		}

		public bool RenderToCubemap(RenderTexture cubemap)
		{
			int faceMask = 63;
			return RenderToCubemap(cubemap, faceMask);
		}

		public bool RenderToCubemap(RenderTexture cubemap, int faceMask)
		{
			return Internal_RenderToCubemapRT(cubemap, faceMask);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern bool Internal_RenderToCubemapRT(RenderTexture cubemap, int faceMask);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern bool Internal_RenderToCubemapTexture(Cubemap cubemap, int faceMask);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void CopyFrom(Camera other);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern bool IsFiltered(GameObject go);
	}
}
