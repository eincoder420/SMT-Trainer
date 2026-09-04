namespace Invector.vItemManager;

[vClassHeader("Equipment", true, "icon_v2", false, "", openClose = false, helpBoxText = "Use this component if you also use the ItemManager in your Character")]
public class vEquipment : vMonoBehaviour
{
	public OnHandleItemEvent onEquip;

	public OnHandleItemEvent onUnequip;

	public vItem referenceItem;

	public EquipPoint equipPoint { get; set; }

	public virtual void OnDestroy()
	{
	}

	public virtual void OnEquip(vItem item)
	{
		referenceItem = item;
		onEquip.Invoke(item);
	}

	public virtual void OnUnequip(vItem item)
	{
		onUnequip.Invoke(item);
	}
}
