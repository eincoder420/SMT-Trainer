using System;
using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Invector.vItemManager;

public class vEquipItemToArea : MonoBehaviour
{
	[Serializable]
	public class AreaToEquip
	{
		[Tooltip("Target EquipArea to Equip")]
		public vEquipArea area;

		[Tooltip("Target EquipSlot of the EquipArea to Equip")]
		public int slotIndex;

		[Tooltip("Optional button to equip on press")]
		public Button optionalButton;

		[Tooltip("Input to equip on press")]
		public GenericInput input = new GenericInput("Alpha 1", "B", "B");

		internal void CheckInput(vItem item, UnityEvent onEquip)
		{
			if (input.GetButtonDown())
			{
				Equip(item);
				onEquip?.Invoke();
			}
		}

		internal void Equip(vItem item)
		{
			if ((bool)area)
			{
				area.AddItemToEquipSlot(slotIndex, item);
			}
		}
	}

	public vItemWindowDisplay itemWindow;

	public AreaToEquip[] areasToEquip;

	public GenericInput cancelInput = new GenericInput("Escape", "B", "B");

	public UnityEvent onEquip;

	public UnityEvent onCancel;

	protected UnityAction onEquipAction;

	private void Start()
	{
		onEquipAction = delegate
		{
			onEquip.Invoke();
		};
		for (int i = 0; i < areasToEquip.Length; i++)
		{
			if ((bool)areasToEquip[i].optionalButton)
			{
				AreaToEquip areaToEquip = areasToEquip[i];
				areasToEquip[i].optionalButton.onClick.AddListener(delegate
				{
					Equip(areaToEquip);
				});
			}
		}
	}

	protected virtual void Update()
	{
		if ((bool)itemWindow && (bool)itemWindow.currentSelectedSlot && (bool)itemWindow.currentSelectedSlot.item)
		{
			for (int i = 0; i < areasToEquip.Length; i++)
			{
				areasToEquip[i].CheckInput(itemWindow.currentSelectedSlot.item, onEquip);
			}
		}
		if (cancelInput.GetButtonDown())
		{
			onCancel.Invoke();
		}
	}

	protected virtual void Equip(AreaToEquip areaToEquip)
	{
		if ((bool)itemWindow && (bool)itemWindow.currentSelectedSlot && (bool)itemWindow.currentSelectedSlot.item)
		{
			areaToEquip.Equip(itemWindow.currentSelectedSlot.item);
			onEquip.Invoke();
		}
	}

	public virtual void Equip(int index)
	{
		if (index < areasToEquip.Length)
		{
			AreaToEquip areaToEquip = areasToEquip[index];
			Equip(areaToEquip);
		}
	}
}
