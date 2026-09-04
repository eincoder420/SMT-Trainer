using UnityEngine;

public class Subparam_Window : MonoBehaviour
{
	public Transform[] Sub_parameter;

	public bool Progress_Params;

	public Transform[] Hidden_parameter;

	public Subparam_Window[] Secondary_parameter;

	private Edit_Base edit_base;

	public bool params_open;

	public bool Tatoo_Param;

	private void Start()
	{
		for (int i = 0; i < Sub_parameter.Length; i++)
		{
			Sub_parameter[i].gameObject.SetActive(value: false);
		}
		if (Hidden_parameter.Length != 0)
		{
			for (int j = 0; j < Hidden_parameter.Length; j++)
			{
				Hidden_parameter[j].gameObject.SetActive(value: false);
			}
		}
	}

	public void Unblock_Param(int id)
	{
		if (!edit_base)
		{
			edit_base = Object.FindObjectOfType<Edit_Base>();
		}
		if (edit_base.data.Blocked_Param[id])
		{
			edit_base.data.Blocked_Param[id] = false;
			if (!params_open)
			{
				Switch_Subparam();
			}
			if (id > 0)
			{
				Sub_parameter[id].GetComponent<Subparam_Window>().Switch_Subparam();
			}
			if ((bool)edit_base.inventory)
			{
				edit_base.inventory.Inventory_Menu = true;
				edit_base.inventory.Check_Inventory_Open();
			}
		}
	}

	public void Unblock_Quiet(int id)
	{
		if (!edit_base)
		{
			edit_base = Object.FindObjectOfType<Edit_Base>();
		}
		edit_base.data.Blocked_Param[id] = false;
	}

	public void Switch_Subparam()
	{
		if (!edit_base)
		{
			edit_base = Object.FindObjectOfType<Edit_Base>();
		}
		for (int i = 0; i < edit_base.Sub_Params.Length; i++)
		{
			if (edit_base.Sub_Params[i].params_open && edit_base.Sub_Params[i] != this)
			{
				edit_base.Sub_Params[i].Switch_Subparam();
			}
		}
		params_open = !params_open;
		for (int j = 0; j < Sub_parameter.Length; j++)
		{
			if (!Progress_Params)
			{
				Sub_parameter[j].gameObject.SetActive(params_open);
			}
			else if (params_open)
			{
				Sub_parameter[j].gameObject.SetActive(!edit_base.data.Blocked_Param[j]);
			}
			else
			{
				Sub_parameter[j].gameObject.SetActive(value: false);
			}
		}
		if (params_open)
		{
			edit_base.Play_Subparam_Sound();
		}
		if (Hidden_parameter.Length != 0)
		{
			for (int k = 0; k < Hidden_parameter.Length; k++)
			{
				Hidden_parameter[k].gameObject.SetActive(value: false);
			}
		}
		if (Tatoo_Param)
		{
			edit_base.Turn_Tatoo_Mode();
		}
		else if (edit_base.print_params.Tatoo_Mode)
		{
			edit_base.Turn_Tatoo_Mode();
		}
	}

	public void Switch_Secondary_Param()
	{
		params_open = !params_open;
		for (int i = 0; i < Secondary_parameter.Length; i++)
		{
			if (Secondary_parameter[i].params_open)
			{
				Secondary_parameter[i].Close_Subparams();
			}
		}
		for (int j = 0; j < Sub_parameter.Length; j++)
		{
			Sub_parameter[j].gameObject.SetActive(params_open);
		}
	}

	public void Close_Subparams()
	{
		for (int i = 0; i < Sub_parameter.Length; i++)
		{
			Sub_parameter[i].gameObject.SetActive(value: false);
		}
		params_open = false;
	}
}
