using UnityEngine;

public class VisualizeMesh : MonoBehaviour
{
	public float sphereRadius = 0.05f;

	private MeshFilter mf;

	private Mesh m;

	private void OnDrawGizmosSelected()
	{
		if (!mf)
		{
			mf = GetComponent<MeshFilter>();
		}
		if (!mf)
		{
			return;
		}
		if (!m)
		{
			m = mf.sharedMesh;
		}
		if ((bool)m)
		{
			Vector3[] vertices = m.vertices;
			Vector3[] normals = m.normals;
			Vector4[] tangents = m.tangents;
			Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
			Matrix4x4 transpose = localToWorldMatrix.inverse.transpose;
			for (int i = 0; i < vertices.Length; i++)
			{
				Gizmos.color = Color.green;
				Vector3 vector = localToWorldMatrix.MultiplyPoint3x4(vertices[i]);
				Gizmos.DrawSphere(vector, sphereRadius);
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(vector, vector + transpose.MultiplyVector(normals[i]) * 0.5f);
				Gizmos.color = Color.red;
				Gizmos.DrawLine(vector, vector + localToWorldMatrix.MultiplyVector(new Vector3(tangents[i].x, tangents[i].y, tangents[i].z)) * 0.5f);
			}
		}
	}
}
