using UnityEngine;

public class vSendScore : MonoBehaviour
{
	public int displayID;

	public vShooterScore shooterScore;

	public void SendScore(float value)
	{
		if (shooterScore == null)
		{
			shooterScore = Object.FindObjectOfType<vShooterScore>();
		}
		if (shooterScore != null)
		{
			shooterScore.AddScore(new vShooterScore.ScorePoint(displayID, value));
		}
	}
}
