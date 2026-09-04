using UnityEngine;

public class Cinematic_Sam : MonoBehaviour
{
	public PauseMenuScript Interface_Script;

	public AudioSource audio;

	public bool Morning_Comes;

	public AudioClip[] Sound_Queue;

	public AudioClip Orgasm_Sound;

	public int Current_Sound;

	public void Play_Sound(AudioClip clip)
	{
		if (audio.isPlaying)
		{
			audio.Stop();
		}
		audio.PlayOneShot(clip);
	}

	public void Play_Sound_Queue()
	{
		if (audio.isPlaying)
		{
			audio.Stop();
		}
		audio.PlayOneShot(Sound_Queue[Current_Sound]);
		if (Current_Sound < 2)
		{
			Current_Sound++;
		}
		else
		{
			Current_Sound = 0;
		}
	}

	public void Stop_Sound()
	{
		audio.Stop();
	}

	private void Update()
	{
		if (Morning_Comes)
		{
			if (Interface_Script.Loader.timeController.timeline < 6.5f)
			{
				Interface_Script.Loader.timeController.timeline += Time.deltaTime * 5f;
			}
			Interface_Script.data.time = Interface_Script.Loader.timeController.timeline;
		}
	}

	public void Night_Started()
	{
		Interface_Script.Loader.timeController.timeline = 0.1f;
	}

	public void Move_Time_Morning()
	{
		Morning_Comes = true;
	}

	public void Stop_Time_Morning()
	{
		Morning_Comes = false;
	}
}
