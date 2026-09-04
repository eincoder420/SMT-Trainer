using System;
using Crosstales.RTVoice.Model;

namespace Crosstales.RTVoice;

[Serializable]
public class DataStore
{
	public Wrapper wrapper;

	public byte[] Data;

	public DataStore()
	{
	}

	public DataStore(Wrapper wrapper, byte[] data)
	{
		this.wrapper = wrapper;
		Data = data;
	}
}
