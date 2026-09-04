namespace UnityEngine.AzureSky;

public class AzureCameraOrbit : MonoBehaviour
{
	private float m_mouseX;

	private float m_mouseY;

	private Vector3 m_startRotation;

	private void Start()
	{
		m_startRotation = base.transform.eulerAngles;
	}

	private void Update()
	{
		if (Input.GetMouseButton(1))
		{
			m_mouseX += Input.GetAxis("Mouse X") * 2.5f;
			m_mouseY -= Input.GetAxis("Mouse Y") * 2.5f;
			base.transform.localRotation = Quaternion.Euler(new Vector3(m_mouseY, m_mouseX, base.transform.localRotation.z) + m_startRotation);
		}
	}
}
