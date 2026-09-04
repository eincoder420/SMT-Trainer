using System;
using System.IO;
using System.Text;
using Crosstales.Common.Util;
using UnityEngine;

namespace Crosstales.Common.Audio;

public abstract class WavMaster
{
	private const int BLOCK_SIZE_16_BIT = 2;

	public static AudioClip ToAudioClip(string filePath, string name = "wav")
	{
		try
		{
			return ToAudioClip(FileHelper.ReadAllBytes(filePath), name);
		}
		catch (Exception ex)
		{
			Debug.LogError("Could not read audio file: " + ex);
		}
		return null;
	}

	public static AudioClip ToAudioClip(Stream stream, string name = "wav")
	{
		try
		{
			return ToAudioClip(stream.CTReadFully(), name);
		}
		catch (Exception ex)
		{
			Debug.LogError("Could not read audio stream: " + ex);
		}
		return null;
	}

	public static AudioClip ToAudioClip(byte[] fileBytes, string name = "wav")
	{
		int num = BitConverter.ToInt32(fileBytes, 16);
		formatCode(BitConverter.ToUInt16(fileBytes, 20));
		ushort num2 = BitConverter.ToUInt16(fileBytes, 22);
		int frequency = BitConverter.ToInt32(fileBytes, 24);
		ushort num3 = BitConverter.ToUInt16(fileBytes, 34);
		int num4 = 20 + num + 4;
		int dataSize = BitConverter.ToInt32(fileBytes, num4);
		float[] array = num3 switch
		{
			8 => convert8BitByteArrayToAudioClipData(fileBytes, num4, dataSize), 
			16 => convert16BitByteArrayToAudioClipData(fileBytes, num4, dataSize), 
			24 => convert24BitByteArrayToAudioClipData(fileBytes, num4, dataSize), 
			32 => convert32BitByteArrayToAudioClipData(fileBytes, num4, dataSize), 
			_ => throw new Exception(num3 + " bit depth is not supported."), 
		};
		AudioClip audioClip = AudioClip.Create(name, array.Length / num2, num2, frequency, stream: false);
		audioClip.SetData(array, 0);
		return audioClip;
	}

	public static byte[] FromAudioClip(AudioClip audioClip)
	{
		return FromAudioClip(audioClip, null, saveAsFile: false);
	}

	public static byte[] FromAudioClip(AudioClip audioClip, string filepath, bool saveAsFile = true)
	{
		using MemoryStream memoryStream = new MemoryStream();
		int fileSize = audioClip.samples * audioClip.channels * 2 + 44;
		writeFileHeader(memoryStream, fileSize);
		writeFileFormat(memoryStream, audioClip.channels, audioClip.frequency, 16);
		writeFileData(memoryStream, audioClip);
		byte[] array = memoryStream.ToArray();
		if (saveAsFile)
		{
			try
			{
				FileHelper.WriteAllBytes(filepath, array);
			}
			catch (Exception ex)
			{
				Debug.LogError("Could not save audio file: " + ex);
			}
		}
		return array;
	}

	public static ushort BitDepth(AudioClip audioClip)
	{
		return Convert.ToUInt16((float)(audioClip.samples * audioClip.channels) * audioClip.length / (float)audioClip.frequency);
	}

	private static float[] convert8BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
	{
		int num = BitConverter.ToInt32(source, headerOffset);
		headerOffset += 4;
		float[] array = new float[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = (float)(int)source[i] / 127f;
		}
		return array;
	}

	private static float[] convert16BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
	{
		int num = BitConverter.ToInt32(source, headerOffset);
		headerOffset += 4;
		int num2 = num / 2;
		float[] array = new float[num2];
		for (int i = 0; i < num2; i++)
		{
			int startIndex = i * 2 + headerOffset;
			array[i] = (float)BitConverter.ToInt16(source, startIndex) / 32767f;
		}
		return array;
	}

	private static float[] convert24BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
	{
		int num = BitConverter.ToInt32(source, headerOffset);
		headerOffset += 4;
		int num2 = num / 3;
		float[] array = new float[num2];
		byte[] array2 = new byte[4];
		for (int i = 0; i < num2; i++)
		{
			int srcOffset = i * 3 + headerOffset;
			Buffer.BlockCopy(source, srcOffset, array2, 1, 3);
			array[i] = (float)BitConverter.ToInt32(array2, 0) / 2.1474836E+09f;
		}
		return array;
	}

	private static float[] convert32BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
	{
		int num = BitConverter.ToInt32(source, headerOffset);
		headerOffset += 4;
		int num2 = num / 4;
		float[] array = new float[num2];
		for (int i = 0; i < num2; i++)
		{
			int startIndex = i * 4 + headerOffset;
			array[i] = (float)BitConverter.ToInt32(source, startIndex) / 2.1474836E+09f;
		}
		return array;
	}

	private static int writeFileHeader(MemoryStream stream, int fileSize)
	{
		byte[] bytes = Encoding.ASCII.GetBytes("RIFF");
		int num = 0 + writeBytesToMemoryStream(stream, bytes);
		int value = fileSize - 8;
		int num2 = num + writeBytesToMemoryStream(stream, BitConverter.GetBytes(value));
		byte[] bytes2 = Encoding.ASCII.GetBytes("WAVE");
		return num2 + writeBytesToMemoryStream(stream, bytes2);
	}

	private static int writeFileFormat(MemoryStream stream, int channels, int sampleRate, ushort bitDepth)
	{
		byte[] bytes = Encoding.ASCII.GetBytes("fmt ");
		int num = 0 + writeBytesToMemoryStream(stream, bytes);
		int value = 16;
		int num2 = num + writeBytesToMemoryStream(stream, BitConverter.GetBytes(value)) + writeBytesToMemoryStream(stream, BitConverter.GetBytes((ushort)1));
		ushort value2 = Convert.ToUInt16(channels);
		int num3 = num2 + writeBytesToMemoryStream(stream, BitConverter.GetBytes(value2)) + writeBytesToMemoryStream(stream, BitConverter.GetBytes(sampleRate));
		int value3 = sampleRate * channels * bytesPerSample(bitDepth);
		int num4 = num3 + writeBytesToMemoryStream(stream, BitConverter.GetBytes(value3));
		ushort value4 = Convert.ToUInt16(channels * bytesPerSample(bitDepth));
		return num4 + writeBytesToMemoryStream(stream, BitConverter.GetBytes(value4)) + writeBytesToMemoryStream(stream, BitConverter.GetBytes(bitDepth));
	}

	private static int writeFileData(MemoryStream stream, AudioClip audioClip)
	{
		float[] data = new float[audioClip.samples * audioClip.channels];
		audioClip.GetData(data, 0);
		byte[] bytes = convertAudioClipDataToInt16ByteArray(data);
		byte[] bytes2 = Encoding.ASCII.GetBytes("data");
		int num = 0 + writeBytesToMemoryStream(stream, bytes2);
		int value = Convert.ToInt32(audioClip.samples * 2 * audioClip.channels);
		return num + writeBytesToMemoryStream(stream, BitConverter.GetBytes(value)) + writeBytesToMemoryStream(stream, bytes);
	}

	private static byte[] convertAudioClipDataToInt16ByteArray(float[] data)
	{
		using MemoryStream memoryStream = new MemoryStream();
		foreach (float num in data)
		{
			memoryStream.Write(BitConverter.GetBytes(Convert.ToInt16(num * 32767f)), 0, 2);
		}
		return memoryStream.ToArray();
	}

	private static int writeBytesToMemoryStream(MemoryStream stream, byte[] bytes)
	{
		int num = bytes.Length;
		stream.Write(bytes, 0, num);
		return num;
	}

	private static int bytesPerSample(ushort bitDepth)
	{
		return bitDepth / 8;
	}

	private static string formatCode(ushort code)
	{
		switch (code)
		{
		case 1:
			return "PCM";
		case 2:
			return "ADPCM";
		case 3:
			return "IEEE";
		case 7:
			return "μ-law";
		case 65534:
			return "WaveFormatExtendable";
		default:
			Debug.LogWarning("Unknown wav code format:" + code);
			return string.Empty;
		}
	}
}
