using Invector.IK;
using Invector.vShooter;
using UnityEngine;

namespace Invector;

public interface vIShooterIKController
{
	GameObject gameObject { get; }

	vIKSolver LeftIK { get; }

	vIKSolver RightIK { get; }

	vWeaponIKAdjustList WeaponIKAdjustList { get; set; }

	vWeaponIKAdjust CurrentWeaponIK { get; }

	IKAdjust CurrentIKAdjust { get; }

	bool LockAiming { get; set; }

	bool LockHipFireAiming { get; set; }

	vShooterWeapon CurrentActiveWeapon { get; }

	bool EditingIKGlobalOffset { get; set; }

	bool IsAiming { get; }

	bool IsCrouching { get; set; }

	bool IsLeftWeapon { get; }

	Vector3 AimPosition { get; }

	string CurrentIKAdjustState { get; }

	string CurrentIKAdjustStateWithTag { get; }

	bool IsUsingCustomIKAdjust { get; }

	bool IsIgnoreIK { get; }

	bool IsSupportHandIKEnabled { get; }

	string CustomIKAdjustState { get; }

	event IKUpdateEvent onStartUpdateIK;

	event IKUpdateEvent onFinishUpdateIK;

	void UpdateWeaponIK();

	void SetCustomIKAdjustState(string value);

	void ResetCustomIKAdjustState();
}
