namespace UnityEngine.AzureSky;

[ImageEffectAllowedInSceneView]
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Azure[Sky]/Azure Fog Scattering")]
public class AzureFogScattering : MonoBehaviour
{
	public Material fogScatteringMaterial;

	private Camera m_camera;

	private Transform m_cameraTransform;

	private Vector3[] m_frustumCorners = new Vector3[4];

	private Rect m_rect = new Rect(0f, 0f, 1f, 1f);

	private Matrix4x4 m_frustumCornersArray;

	private static readonly int FrustumCorners = Shader.PropertyToID("_FrustumCorners");

	private void Start()
	{
		m_camera = GetComponent<Camera>();
		m_cameraTransform = m_camera.transform;
	}

	[ImageEffectOpaque]
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		m_camera.depthTextureMode |= DepthTextureMode.Depth;
		if (fogScatteringMaterial == null)
		{
			Graphics.Blit(source, destination);
			return;
		}
		m_camera.CalculateFrustumCorners(m_rect, m_camera.farClipPlane, m_camera.stereoActiveEye, m_frustumCorners);
		m_frustumCornersArray = Matrix4x4.identity;
		m_frustumCornersArray.SetRow(0, m_cameraTransform.TransformVector(m_frustumCorners[0]));
		m_frustumCornersArray.SetRow(2, m_cameraTransform.TransformVector(m_frustumCorners[1]));
		m_frustumCornersArray.SetRow(3, m_cameraTransform.TransformVector(m_frustumCorners[2]));
		m_frustumCornersArray.SetRow(1, m_cameraTransform.TransformVector(m_frustumCorners[3]));
		fogScatteringMaterial.SetMatrix(FrustumCorners, m_frustumCornersArray);
		Graphics.Blit(source, destination, fogScatteringMaterial, 0);
	}
}
