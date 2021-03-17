using System;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class Graphics
	{
		public static RenderBuffer activeColorBuffer
		{
			get
			{
				GetActiveColorBuffer(out var res);
				return res;
			}
		}

		public static RenderBuffer activeDepthBuffer
		{
			get
			{
				GetActiveDepthBuffer(out var res);
				return res;
			}
		}

		[Obsolete("Use SystemInfo.graphicsDeviceName instead.")]
		public static string deviceName => SystemInfo.graphicsDeviceName;

		[Obsolete("Use SystemInfo.graphicsDeviceVendor instead.")]
		public static string deviceVendor => SystemInfo.graphicsDeviceVendor;

		[Obsolete("Use SystemInfo.graphicsDeviceVersion instead.")]
		public static string deviceVersion => SystemInfo.graphicsDeviceVersion;

		[Obsolete("Use SystemInfo.supportsVertexPrograms instead.")]
		public static bool supportsVertexProgram => SystemInfo.supportsVertexPrograms;

		public static void DrawMesh(Mesh mesh, Vector3 position, Quaternion rotation, Material material, int layer, Camera camera, int submeshIndex)
		{
			MaterialPropertyBlock properties = null;
			DrawMesh(mesh, position, rotation, material, layer, camera, submeshIndex, properties);
		}

		public static void DrawMesh(Mesh mesh, Vector3 position, Quaternion rotation, Material material, int layer, Camera camera)
		{
			MaterialPropertyBlock properties = null;
			int submeshIndex = 0;
			DrawMesh(mesh, position, rotation, material, layer, camera, submeshIndex, properties);
		}

		public static void DrawMesh(Mesh mesh, Vector3 position, Quaternion rotation, Material material, int layer)
		{
			MaterialPropertyBlock properties = null;
			int submeshIndex = 0;
			Camera camera = null;
			DrawMesh(mesh, position, rotation, material, layer, camera, submeshIndex, properties);
		}

		public static void DrawMesh(Mesh mesh, Vector3 position, Quaternion rotation, Material material, int layer, Camera camera, int submeshIndex, MaterialPropertyBlock properties)
		{
			Internal_DrawMeshTRArguments arguments = default(Internal_DrawMeshTRArguments);
			arguments.mesh = mesh;
			arguments.position = position;
			arguments.rotation = rotation;
			arguments.material = material;
			arguments.layer = layer;
			arguments.camera = camera;
			arguments.submeshIndex = submeshIndex;
			arguments.properties = properties;
			Internal_DrawMeshTR(ref arguments, castShadows: true, receiveShadows: true);
		}

		public static void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int layer, Camera camera, int submeshIndex)
		{
			MaterialPropertyBlock properties = null;
			DrawMesh(mesh, matrix, material, layer, camera, submeshIndex, properties);
		}

		public static void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int layer, Camera camera)
		{
			MaterialPropertyBlock properties = null;
			int submeshIndex = 0;
			DrawMesh(mesh, matrix, material, layer, camera, submeshIndex, properties);
		}

		public static void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int layer)
		{
			MaterialPropertyBlock properties = null;
			int submeshIndex = 0;
			Camera camera = null;
			DrawMesh(mesh, matrix, material, layer, camera, submeshIndex, properties);
		}

		public static void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int layer, Camera camera, int submeshIndex, MaterialPropertyBlock properties)
		{
			Internal_DrawMeshMatrixArguments arguments = default(Internal_DrawMeshMatrixArguments);
			arguments.mesh = mesh;
			arguments.matrix = matrix;
			arguments.material = material;
			arguments.layer = layer;
			arguments.camera = camera;
			arguments.submeshIndex = submeshIndex;
			arguments.properties = properties;
			Internal_DrawMeshMatrix(ref arguments, castShadows: true, receiveShadows: true);
		}

		public static void DrawMesh(Mesh mesh, Vector3 position, Quaternion rotation, Material material, int layer, Camera camera, int submeshIndex, MaterialPropertyBlock properties, bool castShadows, bool receiveShadows)
		{
			Internal_DrawMeshTRArguments arguments = default(Internal_DrawMeshTRArguments);
			arguments.mesh = mesh;
			arguments.position = position;
			arguments.rotation = rotation;
			arguments.material = material;
			arguments.layer = layer;
			arguments.camera = camera;
			arguments.submeshIndex = submeshIndex;
			arguments.properties = properties;
			Internal_DrawMeshTR(ref arguments, castShadows, receiveShadows);
		}

		public static void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int layer, Camera camera, int submeshIndex, MaterialPropertyBlock properties, bool castShadows, bool receiveShadows)
		{
			Internal_DrawMeshMatrixArguments arguments = default(Internal_DrawMeshMatrixArguments);
			arguments.mesh = mesh;
			arguments.matrix = matrix;
			arguments.material = material;
			arguments.layer = layer;
			arguments.camera = camera;
			arguments.submeshIndex = submeshIndex;
			arguments.properties = properties;
			Internal_DrawMeshMatrix(ref arguments, castShadows, receiveShadows);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_DrawMeshTR(ref Internal_DrawMeshTRArguments arguments, bool castShadows, bool receiveShadows);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_DrawMeshMatrix(ref Internal_DrawMeshMatrixArguments arguments, bool castShadows, bool receiveShadows);

		public static void DrawMeshNow(Mesh mesh, Vector3 position, Quaternion rotation)
		{
			Internal_DrawMeshNow1(mesh, position, rotation, -1);
		}

		public static void DrawMeshNow(Mesh mesh, Vector3 position, Quaternion rotation, int materialIndex)
		{
			Internal_DrawMeshNow1(mesh, position, rotation, materialIndex);
		}

		public static void DrawMeshNow(Mesh mesh, Matrix4x4 matrix)
		{
			Internal_DrawMeshNow2(mesh, matrix, -1);
		}

		public static void DrawMeshNow(Mesh mesh, Matrix4x4 matrix, int materialIndex)
		{
			Internal_DrawMeshNow2(mesh, matrix, materialIndex);
		}

		private static void Internal_DrawMeshNow1(Mesh mesh, Vector3 position, Quaternion rotation, int materialIndex)
		{
			INTERNAL_CALL_Internal_DrawMeshNow1(mesh, ref position, ref rotation, materialIndex);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_Internal_DrawMeshNow1(Mesh mesh, ref Vector3 position, ref Quaternion rotation, int materialIndex);

		private static void Internal_DrawMeshNow2(Mesh mesh, Matrix4x4 matrix, int materialIndex)
		{
			INTERNAL_CALL_Internal_DrawMeshNow2(mesh, ref matrix, materialIndex);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_Internal_DrawMeshNow2(Mesh mesh, ref Matrix4x4 matrix, int materialIndex);

		[Obsolete("Use Graphics.DrawMeshNow instead.")]
		public static void DrawMesh(Mesh mesh, Vector3 position, Quaternion rotation)
		{
			Internal_DrawMeshNow1(mesh, position, rotation, -1);
		}

		[Obsolete("Use Graphics.DrawMeshNow instead.")]
		public static void DrawMesh(Mesh mesh, Vector3 position, Quaternion rotation, int materialIndex)
		{
			Internal_DrawMeshNow1(mesh, position, rotation, materialIndex);
		}

		[Obsolete("Use Graphics.DrawMeshNow instead.")]
		public static void DrawMesh(Mesh mesh, Matrix4x4 matrix)
		{
			Internal_DrawMeshNow2(mesh, matrix, -1);
		}

		[Obsolete("Use Graphics.DrawMeshNow instead.")]
		public static void DrawMesh(Mesh mesh, Matrix4x4 matrix, int materialIndex)
		{
			Internal_DrawMeshNow2(mesh, matrix, materialIndex);
		}

		public static void DrawTexture(Rect screenRect, Texture texture)
		{
			Material mat = null;
			DrawTexture(screenRect, texture, mat);
		}

		public static void DrawTexture(Rect screenRect, Texture texture, Material mat)
		{
			DrawTexture(screenRect, texture, 0, 0, 0, 0, mat);
		}

		public static void DrawTexture(Rect screenRect, Texture texture, int leftBorder, int rightBorder, int topBorder, int bottomBorder)
		{
			Material mat = null;
			DrawTexture(screenRect, texture, leftBorder, rightBorder, topBorder, bottomBorder, mat);
		}

		public static void DrawTexture(Rect screenRect, Texture texture, int leftBorder, int rightBorder, int topBorder, int bottomBorder, Material mat)
		{
			DrawTexture(screenRect, texture, new Rect(0f, 0f, 1f, 1f), leftBorder, rightBorder, topBorder, bottomBorder, mat);
		}

		public static void DrawTexture(Rect screenRect, Texture texture, Rect sourceRect, int leftBorder, int rightBorder, int topBorder, int bottomBorder)
		{
			Material mat = null;
			DrawTexture(screenRect, texture, sourceRect, leftBorder, rightBorder, topBorder, bottomBorder, mat);
		}

		public static void DrawTexture(Rect screenRect, Texture texture, Rect sourceRect, int leftBorder, int rightBorder, int topBorder, int bottomBorder, Material mat)
		{
			InternalDrawTextureArguments arguments = default(InternalDrawTextureArguments);
			arguments.screenRect = screenRect;
			arguments.texture = texture;
			arguments.sourceRect = sourceRect;
			arguments.leftBorder = leftBorder;
			arguments.rightBorder = rightBorder;
			arguments.topBorder = topBorder;
			arguments.bottomBorder = bottomBorder;
			Color32 color = default(Color32);
			color.r = (color.g = (color.b = (color.a = 128)));
			arguments.color = color;
			arguments.mat = mat;
			DrawTexture(ref arguments);
		}

		public static void DrawTexture(Rect screenRect, Texture texture, Rect sourceRect, int leftBorder, int rightBorder, int topBorder, int bottomBorder, Color color)
		{
			Material mat = null;
			DrawTexture(screenRect, texture, sourceRect, leftBorder, rightBorder, topBorder, bottomBorder, color, mat);
		}

		public static void DrawTexture(Rect screenRect, Texture texture, Rect sourceRect, int leftBorder, int rightBorder, int topBorder, int bottomBorder, Color color, Material mat)
		{
			InternalDrawTextureArguments arguments = default(InternalDrawTextureArguments);
			arguments.screenRect = screenRect;
			arguments.texture = texture;
			arguments.sourceRect = sourceRect;
			arguments.leftBorder = leftBorder;
			arguments.rightBorder = rightBorder;
			arguments.topBorder = topBorder;
			arguments.bottomBorder = bottomBorder;
			arguments.color = color;
			arguments.mat = mat;
			DrawTexture(ref arguments);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal static extern void DrawTexture(ref InternalDrawTextureArguments arguments);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void Blit(Texture source, RenderTexture dest);

		public static void Blit(Texture source, RenderTexture dest, Material mat)
		{
			int pass = -1;
			Blit(source, dest, mat, pass);
		}

		public static void Blit(Texture source, RenderTexture dest, Material mat, int pass)
		{
			Internal_BlitMaterial(source, dest, mat, pass, setRT: true);
		}

		public static void Blit(Texture source, Material mat)
		{
			int pass = -1;
			Blit(source, mat, pass);
		}

		public static void Blit(Texture source, Material mat, int pass)
		{
			Internal_BlitMaterial(source, null, mat, pass, setRT: false);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_BlitMaterial(Texture source, RenderTexture dest, Material mat, int pass, bool setRT);

		public static void BlitMultiTap(Texture source, RenderTexture dest, Material mat, params Vector2[] offsets)
		{
			Internal_BlitMultiTap(source, dest, mat, offsets);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_BlitMultiTap(Texture source, RenderTexture dest, Material mat, Vector2[] offsets);

		public static void SetRenderTarget(RenderTexture rt)
		{
			Internal_SetRT(rt);
		}

		public static void SetRenderTarget(RenderBuffer colorBuffer, RenderBuffer depthBuffer)
		{
			Internal_SetRTBuffer(out colorBuffer, out depthBuffer);
		}

		public static void SetRenderTarget(RenderBuffer[] colorBuffers, RenderBuffer depthBuffer)
		{
			Internal_SetRTBuffers(colorBuffers, out depthBuffer);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_SetRT(RenderTexture rt);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_SetRTBuffer(out RenderBuffer colorBuffer, out RenderBuffer depthBuffer);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_SetRTBuffers(RenderBuffer[] colorBuffers, out RenderBuffer depthBuffer);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void GetActiveColorBuffer(out RenderBuffer res);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void GetActiveDepthBuffer(out RenderBuffer res);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal static extern void SetupVertexLights(Light[] lights);
	}
}
