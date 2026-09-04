namespace Invector.vShooter;

public static class IKLocomotionOptionsHelper
{
	public static vShooterWeapon.IKLocomotionOptions Copy(this vShooterWeapon.IKLocomotionOptions options)
	{
		return new vShooterWeapon.IKLocomotionOptions
		{
			use = options.use,
			useOnIdle = options.useOnIdle,
			useOnWalk = options.useOnWalk,
			useOnRun = options.useOnRun,
			useOnSprint = options.useOnSprint
		};
	}
}
