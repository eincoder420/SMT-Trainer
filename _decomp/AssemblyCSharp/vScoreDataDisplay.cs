using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class vScoreDataDisplay : MonoBehaviour
{
	public Text index;

	public Text score;

	public Text[] hits;

	public void Show(int index, float? score, List<float> hits)
	{
		if ((bool)this.index)
		{
			this.index.text = index.ToString("00");
		}
		if ((bool)this.score)
		{
			this.score.text = (score.HasValue ? score.Value.ToString("00") : "--");
		}
		if (hits != null)
		{
			for (int i = 0; i < this.hits.Length; i++)
			{
				Text text = this.hits[i];
				if (i < hits.Count)
				{
					text.text = hits[i].ToString("00");
					continue;
				}
				break;
			}
		}
		else
		{
			for (int j = 0; j < this.hits.Length; j++)
			{
				this.hits[j].text = "--";
			}
		}
	}
}
