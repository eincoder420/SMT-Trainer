using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobileControl : MonoBehaviour, IDragHandler, IEventSystemHandler, IPointerUpHandler, IPointerDownHandler
{
	private bool toqueAnalogic;

	private bool toqueLook;

	private GameObject analogLeft;

	private GameObject lookDirection;

	public GameObject player;

	private GameObject camLook;

	private Image imgAnalog;

	private Image imgLook;

	private Image joystickImgAnalog;

	private Image joystickImgLook;

	private Vector3 inputVectorAnalogic;

	private Vector3 inputVectorLook;

	public float speed = 10f;

	private float straffe;

	private float translation;

	private RectTransform rectTransform;

	private Vector2 xyBase;

	private CharacterController charController;

	private void Awake()
	{
		Application.targetFrameRate = 60;
		analogLeft = base.transform.Find("AnalogLeft").gameObject;
		lookDirection = base.transform.Find("LookDirection").gameObject;
		camLook = player.transform.Find("Camera").gameObject;
		rectTransform = base.transform.GetComponent<RectTransform>();
		imgAnalog = analogLeft.GetComponent<Image>();
		joystickImgAnalog = analogLeft.transform.GetChild(0).GetComponent<Image>();
		imgLook = lookDirection.GetComponent<Image>();
		joystickImgLook = lookDirection.transform.GetChild(0).GetComponent<Image>();
		charController = player.GetComponent<CharacterController>();
		xyBase = new Vector2(camLook.transform.localRotation.eulerAngles.x, player.transform.localRotation.eulerAngles.y);
	}

	private void Update()
	{
		Vector2 vector = new Vector2(inputVectorLook.z, inputVectorLook.x);
		camLook.transform.localRotation = Quaternion.Euler(xyBase.x + vector.x * -60f, 0f, 0f);
		player.transform.rotation = Quaternion.Euler(0f, xyBase.y + vector.y * 180f, 0f);
		Vector2 vector2 = new Vector2(inputVectorAnalogic.z, inputVectorAnalogic.x);
		Vector3 vector3 = player.transform.forward * vector2.x;
		Vector3 vector4 = player.transform.right * vector2.y;
		charController.SimpleMove(Vector3.ClampMagnitude(vector3 + vector4, 1f) * (Input.GetKey(KeyCode.LeftShift) ? (speed * 2f) : speed));
	}

	public virtual void OnDrag(PointerEventData ped)
	{
		bool flag = ped.position.x > (float)(Screen.width / 2);
		Vector2 localPoint;
		if (!flag)
		{
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(imgAnalog.rectTransform, ped.position, ped.pressEventCamera, out localPoint))
			{
				localPoint.x = (localPoint.x - imgAnalog.rectTransform.rect.width / 2f) / imgAnalog.rectTransform.sizeDelta.x;
				localPoint.y = (localPoint.y + imgAnalog.rectTransform.rect.height / 2f) / imgAnalog.rectTransform.sizeDelta.y;
				inputVectorAnalogic = new Vector3(localPoint.x * 2f + 1f, 0f, localPoint.y * 2f - 1f);
				inputVectorAnalogic = ((inputVectorAnalogic.magnitude > 1f) ? inputVectorAnalogic.normalized : inputVectorAnalogic);
				joystickImgAnalog.rectTransform.anchoredPosition = new Vector3(inputVectorAnalogic.x * (imgAnalog.rectTransform.sizeDelta.x / 3f), inputVectorAnalogic.z * (imgAnalog.rectTransform.sizeDelta.y / 3f));
			}
		}
		else if (flag && RectTransformUtility.ScreenPointToLocalPointInRectangle(imgLook.rectTransform, ped.position, ped.pressEventCamera, out localPoint))
		{
			localPoint.x = (localPoint.x - imgLook.rectTransform.rect.width / 2f) / imgLook.rectTransform.sizeDelta.x;
			localPoint.y = (localPoint.y + imgLook.rectTransform.rect.height / 2f) / imgLook.rectTransform.sizeDelta.y;
			inputVectorLook = new Vector3(localPoint.x * 2f + 1f, 0f, localPoint.y * 2f - 1f);
			inputVectorLook = ((inputVectorLook.magnitude > 1f) ? inputVectorLook.normalized : inputVectorLook);
			joystickImgLook.rectTransform.anchoredPosition = new Vector3(inputVectorLook.x * (imgLook.rectTransform.sizeDelta.x / 3f), inputVectorLook.z * (imgLook.rectTransform.sizeDelta.y / 3f));
		}
	}

	public virtual void OnPointerDown(PointerEventData ped)
	{
		bool flag = ped.position.x > (float)(Screen.width / 2);
		if ((!flag || !toqueLook) && (flag || !toqueAnalogic) && RectTransformUtility.ScreenPointToLocalPointInRectangle(base.transform.GetComponent<Image>().rectTransform, ped.position, ped.pressEventCamera, out var localPoint))
		{
			localPoint.x += rectTransform.sizeDelta.x / 2f;
			localPoint.y += rectTransform.sizeDelta.y / 2f;
			localPoint.x *= rectTransform.localScale.x;
			localPoint.y *= rectTransform.localScale.y;
			flag = localPoint.x > (float)(Screen.width / 2);
			if (flag && !toqueLook)
			{
				toqueLook = true;
				xyBase = new Vector2(camLook.transform.localRotation.eulerAngles.x, player.transform.localRotation.eulerAngles.y);
				imgLook.transform.position = localPoint;
				OnDrag(ped);
			}
			else if (!flag && !toqueAnalogic)
			{
				toqueAnalogic = true;
				imgAnalog.transform.position = localPoint;
				OnDrag(ped);
			}
		}
	}

	public virtual void OnPointerUp(PointerEventData ped)
	{
		if (!(ped.position.x > (float)(Screen.width / 2)))
		{
			inputVectorAnalogic = Vector3.zero;
			toqueAnalogic = false;
			joystickImgAnalog.rectTransform.anchoredPosition = Vector3.zero;
		}
		else
		{
			toqueLook = false;
		}
	}
}
