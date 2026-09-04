using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace Crosstales.Common.Util;

internal static class NativeMethods
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct STARTUPINFOEX
	{
		public STARTUPINFO StartupInfo;

		public IntPtr lpAttributeList;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct STARTUPINFO
	{
		public int cb;

		public string lpReserved;

		public string lpDesktop;

		public string lpTitle;

		public int dwX;

		public int dwY;

		public int dwXSize;

		public int dwYSize;

		public int dwXCountChars;

		public int dwYCountChars;

		public int dwFillAttribute;

		public int dwFlags;

		public short wShowWindow;

		public short cbReserved2;

		public IntPtr lpReserved2;

		public IntPtr hStdInput;

		public IntPtr hStdOutput;

		public IntPtr hStdError;
	}

	internal struct PROCESS_INFORMATION
	{
		public IntPtr hProcess;

		public IntPtr hThread;

		public int dwProcessId;

		public int dwThreadId;
	}

	internal struct SECURITY_ATTRIBUTES
	{
		public int nLength;

		public IntPtr lpSecurityDescriptor;

		public int bInheritHandle;
	}

	[DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes, ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFOEX lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

	[DllImport("Kernel32.dll", SetLastError = true)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	internal static extern bool CloseHandle(IntPtr hObject);

	[DllImport("Kernel32.dll", SetLastError = true)]
	internal static extern bool GetExitCodeProcess(IntPtr process, ref uint exitCode);

	[DllImport("Kernel32.dll", SetLastError = true)]
	internal static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);

	[DllImport("Kernel32.dll", SetLastError = true)]
	internal static extern bool TerminateProcess(IntPtr hProcess, ref uint exitCode);

	[DllImport("Kernel32.dll")]
	internal static extern uint GetLastError();
}
