using UnityEngine;

namespace Crosstales.RTVoice.Demo.Util;

[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_demo_1_1_util_1_1_material_changer.html")]
[RequireComponent(typeof(Renderer))]
public class MaterialChanger : MonoBehaviour
{
	public AudioSource Source;

	public Material ActiveMaterial;

	private Material inactiveMaterial;

	private Renderer myRenderer;

	private void Start()
	{
		myRenderer = GetComponent<Renderer>();
		inactiveMaterial = myRenderer.material;
	}

	private void Update()
	{
		myRenderer.material = (Source.CTHasActiveClip() ? ActiveMaterial : inactiveMaterial);
	}
}
