using System.Collections;
using System.Collections.Generic;
using Crosstales.RTVoice.Model;
using UnityEngine;

namespace Crosstales.RTVoice.Provider;

public interface IVoiceProvider
{
	string AudioFileExtension { get; }

	AudioType AudioFileType { get; }

	string DefaultVoiceName { get; }

	List<Voice> Voices { get; }

	int MaxTextLength { get; }

	bool isWorkingInEditor { get; }

	bool isWorkingInPlaymode { get; }

	bool isSpeakNativeSupported { get; }

	bool isSpeakSupported { get; }

	bool isPlatformSupported { get; }

	bool isSSMLSupported { get; }

	bool isOnlineService { get; }

	bool hasCoRoutines { get; }

	bool isIL2CPPSupported { get; }

	bool hasVoicesInEditor { get; }

	List<string> Cultures { get; }

	int MaxSimultaneousSpeeches { get; }

	void Silence();

	void Silence(string uid);

	IEnumerator SpeakNative(Wrapper wrapper);

	IEnumerator Speak(Wrapper wrapper);

	IEnumerator Generate(Wrapper wrapper);

	IEnumerator SpeakWithClip(Wrapper wrapper, AudioClip clip);

	void Load(bool forceReload = false);
}
