using UnityEngine;
using UnityEngine.AzureSky;

public class AzureOutputExample : MonoBehaviour
{
	public AzureSkyController azureSky;

	public Light myPointLight;

	private void Update()
	{
		myPointLight.intensity = azureSky.GetOutputFloatValue(0) * 2.5f;
		myPointLight.color = azureSky.GetOutputColorValue(1);
	}
}
