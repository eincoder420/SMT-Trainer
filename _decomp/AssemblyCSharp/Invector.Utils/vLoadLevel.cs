using Invector.vCharacterController;
using UnityEngine;

namespace Invector.Utils;

[vClassHeader("Load Level", true, "icon_v2", false, "", openClose = false)]
public class vLoadLevel : vMonoBehaviour
{
	[Tooltip("Write the name of the level you want to load")]
	public string levelToLoad;

	[Tooltip("Assign here the spawnPoint name of the scene that you will load")]
	public string spawnPointName;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			vThirdPersonInput component = other.transform.gameObject.GetComponent<vThirdPersonInput>();
			LoadLevelHelper.LoadScene(levelToLoad, spawnPointName, component);
		}
	}
}
