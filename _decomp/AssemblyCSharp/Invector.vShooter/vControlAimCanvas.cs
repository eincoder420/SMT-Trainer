using System.Collections.Generic;
using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vShooter;

public class vControlAimCanvas : MonoBehaviour
{
	public static vControlAimCanvas instance;

	public RectTransform canvas;

	public List<vAimCanvas> aimCanvasCollection = new List<vAimCanvas>();

	public Camera scopeBackgroundCamera;

	protected vThirdPersonController cc;

	public vAimCanvas currentAimCanvas;

	protected int currentCanvasID;

	protected float scopeCameraTransformWeight;

	private float scopeCameraTargetZoom;

	private float scopeCameraOriginZoom;

	private Vector3 scopeCameraTargetDir;

	private Quaternion scopeCameraOriginRot;

	private Vector3 scopeCameraTargetPos;

	private Vector3 scopeCameraOriginPos;

	public Camera mainCamera;

	public bool isScopeCameraActive
	{
		get
		{
			if ((bool)scopeBackgroundCamera)
			{
				return scopeBackgroundCamera.gameObject.activeInHierarchy;
			}
			return false;
		}
		set
		{
			if ((bool)scopeBackgroundCamera)
			{
				scopeBackgroundCamera.gameObject.SetActive(value);
			}
		}
	}

	public bool isValid
	{
		get
		{
			if (!currentAimCanvas)
			{
				return false;
			}
			return currentAimCanvas.isValid;
		}
		set
		{
			currentAimCanvas.isValid = value;
		}
	}

	public bool isAimActive
	{
		get
		{
			if (!currentAimCanvas)
			{
				return false;
			}
			return currentAimCanvas.isAimActive;
		}
		set
		{
			currentAimCanvas.isAimActive = value;
		}
	}

	public bool isScopeUIActive
	{
		get
		{
			if (!currentAimCanvas)
			{
				return false;
			}
			return currentAimCanvas.isScopeUIActive;
		}
		set
		{
			currentAimCanvas.isScopeUIActive = value;
		}
	}

	public bool useScopeTransition
	{
		get
		{
			if (!currentAimCanvas)
			{
				return false;
			}
			return currentAimCanvas.useScopeTransition;
		}
		set
		{
			currentAimCanvas.useScopeTransition = value;
		}
	}

	protected bool scaleAimWithMovement
	{
		get
		{
			if (!currentAimCanvas)
			{
				return false;
			}
			return currentAimCanvas.scaleAimWithMovement;
		}
	}

	protected float movementSensibility => currentAimCanvas.movementSensibility;

	protected float scaleWithMovement => currentAimCanvas.scaleWithMovement;

	protected float smoothChangeScale => currentAimCanvas.smoothChangeScale;

	protected float smoothTransition => currentAimCanvas.smoothTransition;

	protected RectTransform aimTarget => currentAimCanvas.aimTarget;

	protected RectTransform aimCenter => currentAimCanvas.aimCenter;

	protected Vector2 sizeDeltaTarget => currentAimCanvas.sizeDeltaTarget;

	protected Vector2 sizeDeltaCenter => currentAimCanvas.sizeDeltaCenter;

	protected UnityEvent onEnableAim => currentAimCanvas.onEnableAim;

	protected UnityEvent onDisableAim => currentAimCanvas.onDisableAim;

	protected UnityEvent onCheckvalidAim => currentAimCanvas.onCheckvalidAim;

	protected UnityEvent onCheckInvalidAim => currentAimCanvas.onCheckInvalidAim;

	protected UnityEvent onEnableScopeCamera => currentAimCanvas.onEnableScopeCamera;

	protected UnityEvent onDisableScopeCamera => currentAimCanvas.onDisableScopeCamera;

	protected UnityEvent onEnableScopeUI => currentAimCanvas.onEnableScopeUI;

	protected UnityEvent onDisableScopeUI => currentAimCanvas.onDisableScopeUI;

	public virtual void Init(vThirdPersonController cc)
	{
		if (scopeBackgroundCamera == null)
		{
			scopeBackgroundCamera = GetComponentInChildren<Camera>(includeInactive: true);
		}
		if (scopeBackgroundCamera == null)
		{
			Debug.LogWarning("Could not find Scope Background Camera. Please assign ScopeBackgroundCamera of Control aim canvas", base.gameObject);
		}
		mainCamera = Camera.main;
		instance = this;
		this.cc = cc;
		currentAimCanvas = aimCanvasCollection[currentCanvasID];
		isValid = true;
	}

	public void UpdateScopeCameraTransition()
	{
		if (!scopeBackgroundCamera || !scopeBackgroundCamera.gameObject.activeSelf || !useScopeTransition)
		{
			scopeCameraTransformWeight = 0f;
			return;
		}
		scopeBackgroundCamera.transform.position = Vector3.Lerp(scopeCameraOriginPos, scopeCameraTargetPos, scopeCameraTransformWeight);
		if (scopeCameraTargetDir.magnitude > 0.01f)
		{
			scopeBackgroundCamera.transform.rotation = Quaternion.Lerp(scopeCameraOriginRot, Quaternion.LookRotation(scopeCameraTargetDir), scopeCameraTransformWeight);
		}
		scopeBackgroundCamera.fieldOfView = Mathf.Lerp(scopeCameraOriginZoom, scopeCameraTargetZoom, scopeCameraTransformWeight);
		if (isScopeCameraActive)
		{
			scopeCameraTransformWeight = Mathf.Lerp(scopeCameraTransformWeight, 1.01f, smoothTransition * Time.deltaTime);
		}
		scopeCameraTransformWeight = Mathf.Clamp(scopeCameraTransformWeight, 0f, 1f);
	}

	public void SetAimToCenter(bool validPoint = true)
	{
		if (currentAimCanvas == null)
		{
			return;
		}
		if (validPoint != isValid)
		{
			isValid = validPoint;
			if (isValid)
			{
				onCheckvalidAim.Invoke();
			}
			else
			{
				onCheckInvalidAim.Invoke();
			}
		}
		if ((bool)aimTarget && (bool)aimCenter)
		{
			aimTarget.anchoredPosition = aimCenter.anchoredPosition;
			aimTarget.sizeDelta = sizeDeltaTarget;
		}
	}

	public void SetWordPosition(Vector3 wordPosition, bool validPoint = true)
	{
		if (currentAimCanvas == null)
		{
			return;
		}
		if (validPoint != isValid)
		{
			isValid = validPoint;
			if (isValid)
			{
				onCheckvalidAim.Invoke();
			}
			else
			{
				onCheckInvalidAim.Invoke();
			}
		}
		if (validPoint && (bool)aimTarget && (bool)aimCenter)
		{
			Vector2 vector = mainCamera.WorldToViewportPoint(wordPosition);
			Vector2 anchoredPosition = new Vector2(vector.x * canvas.sizeDelta.x - canvas.sizeDelta.x * 0.5f, vector.y * canvas.sizeDelta.y - canvas.sizeDelta.y * 0.5f);
			if (currentAimCanvas.aimCenterToAimTarget)
			{
				aimCenter.anchoredPosition = anchoredPosition;
			}
			aimTarget.anchoredPosition = anchoredPosition;
			if (scaleAimWithMovement && (cc.input.magnitude > movementSensibility || Input.GetAxis("Mouse X") > movementSensibility || Input.GetAxis("Mouse Y") > movementSensibility))
			{
				aimCenter.sizeDelta = Vector2.Lerp(aimCenter.sizeDelta, sizeDeltaCenter * scaleWithMovement, smoothChangeScale * Time.deltaTime);
				aimTarget.sizeDelta = Vector2.Lerp(aimTarget.sizeDelta, sizeDeltaTarget * scaleWithMovement, smoothChangeScale * Time.deltaTime);
			}
			else
			{
				aimCenter.sizeDelta = Vector2.Lerp(aimCenter.sizeDelta, sizeDeltaCenter * 1f, smoothChangeScale * Time.deltaTime);
				aimTarget.sizeDelta = Vector2.Lerp(aimTarget.sizeDelta, sizeDeltaTarget * 1f, smoothChangeScale * Time.deltaTime);
			}
		}
	}

	public void SetActiveAim(bool value)
	{
		if (!(currentAimCanvas == null) && value != isAimActive)
		{
			isAimActive = value;
			if (value)
			{
				isValid = true;
				onEnableAim.Invoke();
			}
			else
			{
				onDisableAim.Invoke();
			}
		}
	}

	public void SetActiveScopeCamera(bool value, bool useUI = false)
	{
		if (currentAimCanvas == null || !scopeBackgroundCamera || (isScopeCameraActive == value && isScopeUIActive == useUI))
		{
			return;
		}
		mainCamera.enabled = !value;
		isScopeUIActive = useUI;
		if (value)
		{
			if (useScopeTransition)
			{
				scopeBackgroundCamera.transform.position = mainCamera.transform.position;
				scopeBackgroundCamera.transform.rotation = mainCamera.transform.rotation;
				scopeBackgroundCamera.fieldOfView = mainCamera.fieldOfView;
			}
			onEnableScopeCamera.Invoke();
			isScopeCameraActive = true;
			if (value && useUI)
			{
				onEnableScopeUI.Invoke();
				isScopeUIActive = true;
			}
			else
			{
				onDisableScopeUI.Invoke();
				isScopeUIActive = false;
			}
		}
		else
		{
			onDisableScopeCamera.Invoke();
			onDisableScopeUI.Invoke();
			isScopeUIActive = false;
			isScopeCameraActive = false;
			scopeCameraTransformWeight = 0f;
		}
	}

	public void UpdateScopeCamera(Vector3 position, Vector3 lookDirection, float zoom = 60f)
	{
		if (!(currentAimCanvas == null) && (bool)scopeBackgroundCamera)
		{
			float fieldOfView = Mathf.Clamp(60f - zoom, 1f, 179f);
			if (useScopeTransition)
			{
				scopeCameraTargetPos = position;
				scopeCameraTargetDir = lookDirection;
				scopeCameraTargetZoom = fieldOfView;
				scopeCameraOriginPos = mainCamera.transform.position;
				scopeCameraOriginRot = mainCamera.transform.rotation;
				scopeCameraOriginZoom = mainCamera.fieldOfView;
				UpdateScopeCameraTransition();
			}
			else
			{
				scopeBackgroundCamera.fieldOfView = fieldOfView;
				scopeBackgroundCamera.transform.position = position;
				Quaternion rotation = Quaternion.LookRotation(lookDirection);
				scopeBackgroundCamera.transform.rotation = rotation;
			}
		}
	}

	public void SetAimCanvasID(int id)
	{
		if (aimCanvasCollection.Count > 0 && currentCanvasID != id)
		{
			if (currentAimCanvas != null)
			{
				currentAimCanvas.DisableAll();
			}
			if (id < aimCanvasCollection.Count)
			{
				currentAimCanvas = aimCanvasCollection[id];
				currentCanvasID = id;
			}
			else
			{
				currentAimCanvas = aimCanvasCollection[0];
				currentCanvasID = 0;
			}
		}
	}
}
