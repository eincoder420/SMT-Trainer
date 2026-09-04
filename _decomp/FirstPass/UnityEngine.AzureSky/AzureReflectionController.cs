using UnityEngine.Rendering;

namespace UnityEngine.AzureSky;

[ExecuteInEditMode]
[AddComponentMenu("Azure[Sky]/Azure Reflection Controller")]
public class AzureReflectionController : MonoBehaviour
{
	public ReflectionProbe reflectionProbe;

	public AzureReflectionProbeState state = AzureReflectionProbeState.Off;

	public Transform followTarget;

	public ReflectionProbeRefreshMode refreshMode;

	public ReflectionProbeTimeSlicingMode timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;

	public bool updateAtFirstFrame = true;

	public float refreshInterval = 2f;

	private float m_timeSinceLastProbeUpdate;

	private void Awake()
	{
		if (state == AzureReflectionProbeState.On && refreshMode == ReflectionProbeRefreshMode.ViaScripting && updateAtFirstFrame)
		{
			reflectionProbe.RenderProbe();
		}
	}

	private void Update()
	{
		if (!Application.isPlaying || state != 0)
		{
			return;
		}
		if ((bool)followTarget)
		{
			reflectionProbe.transform.position = followTarget.position;
		}
		if (refreshMode == ReflectionProbeRefreshMode.EveryFrame)
		{
			reflectionProbe.RenderProbe();
		}
		else if (refreshMode == ReflectionProbeRefreshMode.ViaScripting)
		{
			m_timeSinceLastProbeUpdate += Time.deltaTime;
			if (m_timeSinceLastProbeUpdate >= refreshInterval)
			{
				reflectionProbe.RenderProbe();
				m_timeSinceLastProbeUpdate = 0f;
			}
		}
	}

	public void UpdateReflectionProbe()
	{
		reflectionProbe.RenderProbe();
	}
}
