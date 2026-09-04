using UnityEngine;

public class MaterialAlteration : MonoBehaviour
{
	private Renderer rend;

	[Tooltip("Control Make-up Values : RGBA/XYZW : Color(vector3) Glitter Amount(Float)")]
	public Vector4 makeUp1;

	public Vector4 makeUp2;

	public Vector4 makeUp3;

	[Tooltip("Control Make-up channel amounts")]
	public Vector3 colorPower;

	private void Start()
	{
		rend = GetComponent<Renderer>();
		makeUp1 = rend.material.GetVector("_MakeUpColor1GlitterAlpha");
		makeUp2 = rend.material.GetVector("_MakeUpColor2GlitterAlpha");
		makeUp3 = rend.material.GetVector("_MakeUpColor3GlitterAlpha");
		colorPower = rend.material.GetVector("_ColorPowerRGB");
	}

	private void Update()
	{
		rend.material.SetVector("_MakeUpColor1GlitterAlpha", makeUp1);
		rend.material.SetVector("_MakeUpColor2GlitterAlpha", makeUp2);
		rend.material.SetVector("_MakeUpColor3GlitterAlpha", makeUp3);
		rend.material.SetVector("_ColorPowerRGB", colorPower);
	}
}
