using UnityEngine;

namespace FCG;

public class TrafficLights2 : MonoBehaviour
{
	private float countTime;

	private int step;

	private int status;

	public TrafficLight trafficLight_N;

	public TrafficLight trafficLight_S;

	public TrafficLight trafficLight_E;

	public TrafficLight trafficLight_W;

	private void Start()
	{
		countTime = 0f;
		step = 0;
		status = ((Random.Range(1, 8) < 4) ? 13 : 31);
		EnabledObjects(status);
		InvokeRepeating("TrafficLightTurn", Random.Range(0, 4), 1f);
	}

	private void TrafficLightTurn()
	{
		countTime += 1f;
		if (step == 0)
		{
			if (countTime > 16f)
			{
				countTime = 0f;
				step = 1;
				if (status == 13)
				{
					status = 12;
				}
				else if (status == 31)
				{
					status = 21;
				}
				EnabledObjects(status);
			}
		}
		else if (step == 1)
		{
			if (countTime >= 5f)
			{
				countTime = 0f;
				step = 2;
				if (status == 12)
				{
					status = 41;
				}
				else if (status == 21)
				{
					status = 14;
				}
				EnabledObjects(44);
			}
		}
		else if (step == 2 && countTime >= 7f)
		{
			countTime = 0f;
			step = 0;
			if (status == 14)
			{
				status = 13;
			}
			else if (status == 41)
			{
				status = 31;
			}
			EnabledObjects(status);
		}
	}

	private void EnabledObjects(int st)
	{
		if ((bool)trafficLight_N)
		{
			trafficLight_N.SetStatus(st.ToString().Substring(0, 1));
		}
		if ((bool)trafficLight_S)
		{
			trafficLight_S.SetStatus(st.ToString().Substring(0, 1));
		}
		if ((bool)trafficLight_E)
		{
			trafficLight_E.SetStatus(st.ToString().Substring(1, 1));
		}
		if ((bool)trafficLight_W)
		{
			trafficLight_W.SetStatus(st.ToString().Substring(1, 1));
		}
	}
}
