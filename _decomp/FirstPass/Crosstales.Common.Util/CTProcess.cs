using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using UnityEngine;

namespace Crosstales.Common.Util;

public class CTProcess : IDisposable
{
	private uint _exitCode = 123456u;

	private CTProcessStartInfo _startInfo = new CTProcessStartInfo();

	private static readonly FieldInfo[] EVENT_FIELDS = typeof(DataReceivedEventArgs).GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic);

	private IntPtr _threadHandle = IntPtr.Zero;

	private const uint INFINITE = uint.MaxValue;

	private const uint CREATE_NO_WINDOW = 134217728u;

	public IntPtr Handle { get; private set; }

	public int Id { get; private set; }

	public CTProcessStartInfo StartInfo
	{
		get
		{
			return _startInfo;
		}
		set
		{
			if (value != null)
			{
				_startInfo = value;
			}
		}
	}

	public bool HasExited { get; private set; }

	public uint ExitCode => _exitCode;

	public DateTime StartTime { get; private set; }

	public DateTime ExitTime { get; private set; }

	public StreamReader StandardOutput { get; private set; }

	public StreamReader StandardError { get; private set; }

	public bool isBusy { get; private set; }

	public event EventHandler Exited;

	public event DataReceivedEventHandler OutputDataReceived;

	public event DataReceivedEventHandler ErrorDataReceived;

	private void onExited()
	{
		if (BaseConstants.DEV_DEBUG)
		{
			UnityEngine.Debug.Log($"onExited: {ExitCode}");
		}
		this.Exited?.Invoke(this, EventArgs.Empty);
	}

	public void BeginOutputReadLine()
	{
		new Thread(watchStdOut).Start();
	}

	public void BeginErrorReadLine()
	{
		new Thread(watchStdErr).Start();
	}

	public void Start(CTProcessStartInfo info)
	{
		if (info != null)
		{
			StartInfo = info;
		}
		Start();
	}

	public void Start()
	{
		cleanup();
		isBusy = true;
		HasExited = false;
		if (StartInfo.UseThread)
		{
			new Thread(createProcess).Start();
			Thread.Sleep(200);
		}
		else
		{
			createProcess();
		}
	}

	public void Kill()
	{
		if (Handle != IntPtr.Zero)
		{
			uint exitCode = 99999u;
			NativeMethods.TerminateProcess(Handle, ref exitCode);
			Dispose();
		}
	}

	public void WaitForExit(int milliseconds = 0)
	{
		if (milliseconds > 0)
		{
			NativeMethods.WaitForSingleObject(Handle, (uint)milliseconds);
		}
		else
		{
			NativeMethods.WaitForSingleObject(Handle, uint.MaxValue);
		}
	}

	public void Dispose()
	{
		if (BaseConstants.DEV_DEBUG)
		{
			UnityEngine.Debug.LogWarning("Dispose called!");
		}
		if (Handle != IntPtr.Zero)
		{
			NativeMethods.CloseHandle(Handle);
		}
		if (_threadHandle != IntPtr.Zero)
		{
			NativeMethods.CloseHandle(_threadHandle);
		}
		Handle = IntPtr.Zero;
		_threadHandle = IntPtr.Zero;
		Id = 0;
		isBusy = false;
		HasExited = true;
		StandardOutput?.Dispose();
		StandardError?.Dispose();
	}

	private void createProcess()
	{
		StartTime = DateTime.Now;
		string text = StartInfo.FileName;
		string text2 = StartInfo.Arguments;
		if (BaseConstants.DEV_DEBUG)
		{
			UnityEngine.Debug.Log($"createProcess: {StartTime}");
		}
		NativeMethods.STARTUPINFOEX lpStartupInfo = default(NativeMethods.STARTUPINFOEX);
		try
		{
			if ((StartInfo.RedirectStandardOutput || StartInfo.RedirectStandardError || StartInfo.UseCmdExecute) && !StartInfo.FileName.CTContains("cmd"))
			{
				text = BaseConstants.CMD_WINDOWS_PATH;
				text2 = "/c call \"" + StartInfo.FileName + "\" " + StartInfo.Arguments;
			}
			if (StartInfo.RedirectStandardOutput)
			{
				string tempFile = FileHelper.TempFile;
				text2 = text2 + " > \"" + tempFile + "\"";
				if (BaseConstants.DEV_DEBUG)
				{
					UnityEngine.Debug.Log("tempStdFile: " + tempFile);
				}
				StandardOutput = new StreamReader(new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), StartInfo.StandardOutputEncoding);
			}
			else
			{
				StandardOutput = new StreamReader(new MemoryStream(), StartInfo.StandardOutputEncoding);
			}
			if (StartInfo.RedirectStandardError)
			{
				string tempFile2 = FileHelper.TempFile;
				text2 = text2 + " 2> \"" + tempFile2 + "\"";
				if (BaseConstants.DEV_DEBUG)
				{
					UnityEngine.Debug.Log("tempErrFile: " + tempFile2);
				}
				StandardError = new StreamReader(new FileStream(tempFile2, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), StartInfo.StandardOutputEncoding);
			}
			else
			{
				StandardError = new StreamReader(new MemoryStream(), StartInfo.StandardOutputEncoding);
			}
			NativeMethods.SECURITY_ATTRIBUTES lpProcessAttributes = default(NativeMethods.SECURITY_ATTRIBUTES);
			NativeMethods.SECURITY_ATTRIBUTES lpThreadAttributes = default(NativeMethods.SECURITY_ATTRIBUTES);
			lpProcessAttributes.nLength = Marshal.SizeOf(lpProcessAttributes);
			lpThreadAttributes.nLength = Marshal.SizeOf(lpThreadAttributes);
			if (BaseConstants.DEV_DEBUG)
			{
				UnityEngine.Debug.Log("application: " + text + Environment.NewLine + "arguments: " + text2);
			}
			if (NativeMethods.CreateProcess(text, " " + text2, ref lpProcessAttributes, ref lpThreadAttributes, bInheritHandles: true, StartInfo.CreateNoWindow ? 134217728u : 0u, IntPtr.Zero, StartInfo.WorkingDirectory, ref lpStartupInfo, out var lpProcessInformation))
			{
				Handle = lpProcessInformation.hProcess;
				_threadHandle = lpProcessInformation.hThread;
				Id = lpProcessInformation.dwProcessId;
				WaitForExit();
				return;
			}
			UnityEngine.Debug.LogError($"Could not start process: '{StartInfo.FileName}'{Environment.NewLine}Arguments: '{StartInfo.Arguments}'{Environment.NewLine}Working dir: '{StartInfo.WorkingDirectory}'{Environment.NewLine}Last error: {NativeMethods.GetLastError()}");
		}
		catch (Exception arg)
		{
			UnityEngine.Debug.LogError($"Process threw an error: {arg}");
			Dispose();
		}
		finally
		{
			Thread.Sleep(200);
			NativeMethods.GetExitCodeProcess(Handle, ref _exitCode);
			ExitTime = DateTime.Now;
			if (Handle != IntPtr.Zero)
			{
				NativeMethods.CloseHandle(Handle);
			}
			if (_threadHandle != IntPtr.Zero)
			{
				NativeMethods.CloseHandle(_threadHandle);
			}
			Handle = IntPtr.Zero;
			_threadHandle = IntPtr.Zero;
			Id = 0;
			if (!HasExited)
			{
				onExited();
			}
			isBusy = false;
			HasExited = true;
		}
	}

	private void cleanup()
	{
		Kill();
		Dispose();
	}

	private void watchStdOut()
	{
		using StreamReader streamReader = StandardOutput;
		while (!streamReader.EndOfStream)
		{
			string text = streamReader.ReadLine();
			if (BaseConstants.DEV_DEBUG)
			{
				UnityEngine.Debug.Log("watchStdOut: " + text);
			}
			this.OutputDataReceived?.Invoke(this, createMockDataReceivedEventArgs(text));
		}
	}

	private void watchStdErr()
	{
		using StreamReader streamReader = StandardError;
		while (!streamReader.EndOfStream)
		{
			string text = streamReader.ReadLine();
			if (BaseConstants.DEV_DEBUG)
			{
				UnityEngine.Debug.Log("watchStdErr: " + text);
			}
			this.ErrorDataReceived?.Invoke(this, createMockDataReceivedEventArgs(text));
		}
	}

	private static DataReceivedEventArgs createMockDataReceivedEventArgs(string data)
	{
		if (string.IsNullOrEmpty(data))
		{
			throw new ArgumentException("Data is null or empty.", "data");
		}
		DataReceivedEventArgs dataReceivedEventArgs = (DataReceivedEventArgs)FormatterServices.GetUninitializedObject(typeof(DataReceivedEventArgs));
		if (EVENT_FIELDS.Length != 0)
		{
			EVENT_FIELDS[0].SetValue(dataReceivedEventArgs, data);
		}
		else
		{
			UnityEngine.Debug.LogError("Could not create 'DataReceivedEventArgs'!");
		}
		return dataReceivedEventArgs;
	}
}
