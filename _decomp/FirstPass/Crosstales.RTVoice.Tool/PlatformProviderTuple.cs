using System;
using Crosstales.Common.Model.Enum;
using Crosstales.RTVoice.Provider;

namespace Crosstales.RTVoice.Tool;

[Serializable]
public class PlatformProviderTuple
{
	public Platform Platform;

	public BaseCustomVoiceProvider CustomVoiceProvider;
}
