using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class InteractionSystemTestGUI : MonoBehaviour
{
	[Tooltip("The object to interact to")]
	public InteractionObject interactionObject;

	[Tooltip("The effectors to interact with")]
	public FullBodyBipedEffector[] effectors;

	private InteractionSystem interactionSystem;

	private void Awake()
	{
		interactionSystem = GetComponent<InteractionSystem>();
	}

	private void OnGUI()
	{
		if (interactionSystem == null)
		{
			return;
		}
		if (GUILayout.Button("Start Interaction With " + interactionObject.name))
		{
			if (effectors.Length == 0)
			{
				Debug.Log("Please select the effectors to interact with.");
			}
			FullBodyBipedEffector[] array = effectors;
			foreach (FullBodyBipedEffector effectorType in array)
			{
				interactionSystem.StartInteraction(effectorType, interactionObject, interrupt: true);
			}
		}
		if (effectors.Length != 0 && interactionSystem.IsPaused(effectors[0]) && GUILayout.Button("Resume Interaction With " + interactionObject.name))
		{
			interactionSystem.ResumeAll();
		}
	}
}
