namespace Invector;

public interface vIHealthController : vIDamageReceiver
{
	OnDead onDead { get; }

	float currentHealth { get; }

	int MaxHealth { get; }

	bool isDead { get; set; }

	void AddHealth(int value);

	void ChangeHealth(int value);

	void ChangeMaxHealth(int value);

	void ResetHealth(float health);

	void ResetHealth();
}
