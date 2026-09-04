using System;
using System.Collections.Generic;
using Invector;
using UnityEngine;
using UnityEngine.Events;

[vClassHeader("Mult-Toggle Event", true, "icon_v2", false, "", helpBoxText = "Use the method SetToggleOn/Off via Events", openClose = false)]
public class vMultToogleEvent : vMonoBehaviour
{
	[Serializable]
	public class Toogle
	{
		public string name;

		[Header("Current Value of the toogle")]
		public bool value;

		[Header("Validation to compare with value")]
		public bool validation;

		public bool isValid => value.Equals(validation);

		public void ToogleOn()
		{
			value = true;
		}

		public void ToogleOff()
		{
			value = false;
		}
	}

	public List<Toogle> toogles;

	public bool isValid;

	public UnityEvent onValidate;

	public UnityEvent onInvalidate;

	public void Start()
	{
		CheckValidation();
	}

	public void ToogleOn(int index)
	{
		if (toogles.Count > 0 && index < toogles.Count)
		{
			toogles[index].ToogleOn();
			CheckValidation();
		}
	}

	public void ToogleOff(int index)
	{
		if (toogles.Count > 0 && index < toogles.Count)
		{
			toogles[index].ToogleOff();
			CheckValidation();
		}
	}

	public void ToogleOn(string name)
	{
		Toogle toogle = toogles.Find((Toogle t) => t.name.Equals(name));
		if (toogle != null)
		{
			toogle.ToogleOn();
			CheckValidation();
		}
	}

	public void ToogleOff(string name)
	{
		Toogle toogle = toogles.Find((Toogle t) => t.name.Equals(name));
		if (toogle != null)
		{
			toogle.ToogleOff();
			CheckValidation();
		}
	}

	private void CheckValidation()
	{
		bool flag = isValid;
		flag = ((toogles.FindAll((Toogle t) => t.isValid).Count == toogles.Count) ? true : false);
		if (flag != isValid)
		{
			isValid = flag;
			if (isValid)
			{
				onValidate.Invoke();
			}
			else
			{
				onInvalidate.Invoke();
			}
		}
	}
}
