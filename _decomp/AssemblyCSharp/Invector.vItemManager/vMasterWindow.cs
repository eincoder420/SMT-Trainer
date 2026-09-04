using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vItemManager;

[vClassHeader("Master Window", true, "icon_v2", false, "", openClose = false)]
public class vMasterWindow : vMonoBehaviour
{
	[vHelpBox("Window that always opens when this window is activated", vHelpBoxAttribute.MessageType.None)]
	public GameObject mainWindow;

	public bool sequenceWindows;

	[vReadOnly(true)]
	public vMasterWindow parentWindow;

	[vReadOnly(true)]
	public GameObject currentWindow;

	[vReadOnly(true)]
	public List<GameObject> windows;

	public UnityEvent onEnable;

	public UnityEvent onDisable;

	protected virtual void OnDisable()
	{
		GetParentWindow();
		_ = (bool)parentWindow;
		CloseAllMasterWindows();
		onDisable.Invoke();
	}

	protected virtual void GetParentWindow()
	{
		if (!parentWindow)
		{
			parentWindow = base.transform.parent.GetComponentInParent<vMasterWindow>();
		}
	}

	protected virtual void OnEnable()
	{
		GetParentWindow();
		if ((bool)parentWindow)
		{
			parentWindow.SetCurrentWindow(base.gameObject);
		}
		if (windows.Count == 0 && (bool)mainWindow)
		{
			SetCurrentWindow(mainWindow);
		}
		onEnable.Invoke();
	}

	public virtual void RemoveWindow(GameObject window)
	{
		if (!windows.Contains(window) || window == mainWindow)
		{
			return;
		}
		if (!sequenceWindows || currentWindow == window)
		{
			windows.Remove(window);
		}
		currentWindow = null;
		if (sequenceWindows && windows.Count > 0)
		{
			currentWindow = windows[windows.Count - 1];
			if (!currentWindow.activeSelf)
			{
				currentWindow.SetActive(value: true);
			}
		}
		if (windows.Count == 0 && (bool)mainWindow)
		{
			SetCurrentWindow(mainWindow);
		}
	}

	public virtual void SetCurrentWindow(GameObject window)
	{
		if (currentWindow == window)
		{
			if (!currentWindow.activeSelf)
			{
				currentWindow.SetActive(value: true);
			}
			return;
		}
		if (!windows.Contains(window))
		{
			windows.Add(window);
		}
		if (!sequenceWindows && (bool)currentWindow)
		{
			windows.Remove(currentWindow);
			if (currentWindow.activeSelf)
			{
				currentWindow.SetActive(value: false);
			}
		}
		currentWindow = window;
		if (!currentWindow.activeSelf)
		{
			currentWindow.SetActive(value: true);
		}
	}

	public virtual void CloseAllMasterWindows()
	{
		for (int i = 0; i < windows.Count; i++)
		{
			windows[i].SetActive(value: false);
		}
		windows.Clear();
		if ((bool)mainWindow)
		{
			mainWindow.SetActive(value: true);
		}
	}
}
