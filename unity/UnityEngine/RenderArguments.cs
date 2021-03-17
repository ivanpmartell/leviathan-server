namespace UnityEngine
{
	internal struct RenderArguments
	{
		public Camera camera;

		public Light[] lights;

		public float meshTreeDistance;

		public float billboardTreeDistance;

		public float crossFadeLength;

		public int maximumMeshTrees;

		public int layer;
	}
}
