namespace UnityEngine
{
	internal struct InternalDrawArguments
	{
		public Material material;

		public MaterialPropertyBlock properties;

		public Mesh mesh;

		public Camera camera;

		public int layer;

		public int submeshIndex;

		public Matrix4x4 matrix;
	}
}
