using System;

namespace UnityEngine.AzureSky;

[ExecuteInEditMode]
[AddComponentMenu("Azure[Sky]/Weather Zone")]
public sealed class AzureWeatherZone : MonoBehaviour
{
	public float blendDistance;

	public AzureSkyProfile profile;

	private Collider m_tempCollider;

	private void OnEnable()
	{
		m_tempCollider = GetComponent<Collider>();
	}

	private void OnDrawGizmos()
	{
		m_tempCollider = GetComponent<Collider>();
		if (m_tempCollider == null)
		{
			return;
		}
		if (m_tempCollider.enabled)
		{
			Gizmos.color = Color.green;
			Vector3 lossyScale = base.transform.lossyScale;
			Vector3 vector = new Vector3(1f / lossyScale.x, 1f / lossyScale.y, 1f / lossyScale.z);
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, lossyScale);
			Type type = m_tempCollider.GetType();
			if (type == typeof(BoxCollider))
			{
				BoxCollider boxCollider = (BoxCollider)m_tempCollider;
				Gizmos.DrawCube(boxCollider.center, boxCollider.size);
				Gizmos.DrawWireCube(boxCollider.center, boxCollider.size + vector * blendDistance * 4f);
			}
			else if (type == typeof(SphereCollider))
			{
				SphereCollider sphereCollider = (SphereCollider)m_tempCollider;
				Gizmos.DrawSphere(sphereCollider.center, sphereCollider.radius);
				Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius + vector.x * blendDistance * 2f);
			}
			else if (type == typeof(MeshCollider))
			{
				MeshCollider meshCollider = (MeshCollider)m_tempCollider;
				if (!meshCollider.convex)
				{
					meshCollider.convex = true;
				}
				Gizmos.DrawMesh(meshCollider.sharedMesh);
				Gizmos.DrawWireMesh(meshCollider.sharedMesh, Vector3.zero, Quaternion.identity, Vector3.one + vector * blendDistance * 4f);
			}
		}
		m_tempCollider = null;
	}
}
