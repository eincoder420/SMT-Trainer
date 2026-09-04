using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Invector;
using UnityEngine;

public class vShooterScore : MonoBehaviour
{
	[Serializable]
	public class TargetPointCounter
	{
		[vReadOnly(false)]
		public float currentScore;

		public vScorePointDisplay display;

		public void ShowValue()
		{
			display.ShowValue(currentScore);
		}
	}

	public class ScorePoint
	{
		public int id;

		public float value;

		public ScorePoint(float value)
		{
			this.value = value;
		}

		public ScorePoint(int id, float value)
		{
			this.id = id;
			this.value = value;
		}
	}

	[Serializable]
	public class ScoreDATAList
	{
		public List<ScoreDATA> datas = new List<ScoreDATA>();
	}

	[Serializable]
	public class ScoreDATA
	{
		public float score;

		public List<float> hits = new List<float>();
	}

	[vButton("ShowData", "ShowData", typeof(vShooterScore), false)]
	[vButton("ClearData", "ClearData", typeof(vShooterScore), false)]
	public TargetPointCounter scoreDisplay;

	public TargetPointCounter[] hitCounters;

	public vScoreDataDisplay[] dataDisplays;

	private ScoreDATAList scoreDATAList;

	public void AddScore(ScorePoint score)
	{
		scoreDisplay.currentScore += score.value;
		scoreDisplay.ShowValue();
		if (hitCounters.Length != 0 && score.id < hitCounters.Length)
		{
			TargetPointCounter targetPointCounter = hitCounters[score.id];
			if (targetPointCounter != null)
			{
				targetPointCounter.currentScore += 1f;
				targetPointCounter.ShowValue();
			}
		}
	}

	internal void StartScore()
	{
		scoreDisplay.currentScore = 0f;
		TargetPointCounter[] array = hitCounters;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].currentScore = 0f;
		}
		scoreDATAList = LoadData("ShooterScore");
	}

	internal void FinishScore()
	{
		if (scoreDATAList == null)
		{
			return;
		}
		ScoreDATA scoreDATA = new ScoreDATA();
		scoreDATA.score = scoreDisplay.currentScore;
		scoreDATA.hits = new List<float>();
		TargetPointCounter[] array = hitCounters;
		foreach (TargetPointCounter targetPointCounter in array)
		{
			scoreDATA.hits.Add(targetPointCounter.currentScore);
		}
		if (scoreDATAList.datas.Count < dataDisplays.Length)
		{
			scoreDATAList.datas.Add(scoreDATA);
			scoreDATAList.datas = scoreDATAList.datas.OrderBy((ScoreDATA d) => d.score).Reverse().ToList();
		}
		else
		{
			scoreDATAList.datas.Add(scoreDATA);
			scoreDATAList.datas = scoreDATAList.datas.OrderBy((ScoreDATA d) => d.score).Reverse().ToList();
			int num = scoreDATAList.datas.Count - dataDisplays.Length;
			bool flag = true;
			for (int j = 0; j < num; j++)
			{
				if (scoreDATAList.datas[scoreDATAList.datas.Count - 1].Equals(scoreDATA))
				{
					flag = false;
				}
				scoreDATAList.datas.RemoveAt(scoreDATAList.datas.Count - 1);
			}
		}
		SaveData("ShooterScore");
		ShowData();
	}

	public void ClearData()
	{
		scoreDATAList = new ScoreDATAList();
		SaveData("ShooterScore");
	}

	public void ShowData()
	{
		if (scoreDATAList == null || !Application.isPlaying)
		{
			scoreDATAList = LoadData("ShooterScore");
		}
		for (int i = 0; i < dataDisplays.Length; i++)
		{
			if (scoreDATAList.datas.Count > 0 && i < scoreDATAList.datas.Count)
			{
				ScoreDATA scoreDATA = scoreDATAList.datas[i];
				dataDisplays[i].Show(i + 1, scoreDATA.score, scoreDATA.hits);
			}
			else
			{
				dataDisplays[i].Show(i + 1, null, null);
			}
		}
	}

	public void SaveData(string dataName)
	{
		if (scoreDATAList == null)
		{
			scoreDATAList = new ScoreDATAList();
		}
		string contents = JsonUtility.ToJson(scoreDATAList);
		File.WriteAllText(Application.dataPath + "/" + dataName + ".json", contents);
	}

	public ScoreDATAList LoadData(string dataName)
	{
		string path = Application.dataPath + "/" + dataName + ".json";
		if (!File.Exists(path))
		{
			SaveData("ShooterScore");
		}
		else
		{
			string json = File.ReadAllText(path);
			scoreDATAList = JsonUtility.FromJson<ScoreDATAList>(json);
		}
		return scoreDATAList;
	}
}
