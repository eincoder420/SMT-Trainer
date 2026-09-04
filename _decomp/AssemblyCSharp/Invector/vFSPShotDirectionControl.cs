using System.Collections.Generic;
using Invector.vShooter;
using UnityEngine;

namespace Invector;

[vClassHeader("FPS Shot Direction Control", true, "icon_v2", false, "", openClose = false)]
public class vFSPShotDirectionControl : vMonoBehaviour
{
	public vShooterWeaponBase shooterWeapon;

	public List<string> shooterWeaponIgnoreTags;

	public LayerMask shooterWeaponHitLayer;

	private vFPSController controller;

	private RaycastHit hitObject;

	private void Start()
	{
		controller = GetComponentInParent<vFPSController>();
		if ((bool)shooterWeapon)
		{
			shooterWeapon.ignoreTags = shooterWeaponIgnoreTags;
			shooterWeapon.hitLayer = shooterWeaponHitLayer;
		}
	}

	public void Shot()
	{
		if ((bool)shooterWeapon)
		{
			if (Physics.Raycast(controller._camera.transform.position, controller._camera.transform.forward, out hitObject, controller._camera.farClipPlane, shooterWeaponHitLayer))
			{
				shooterWeapon.Shoot(hitObject.point, controller.transform);
			}
			else
			{
				shooterWeapon.Shoot(controller._camera.transform.position + controller._camera.transform.forward * controller._camera.farClipPlane, controller.transform);
			}
		}
	}
}
