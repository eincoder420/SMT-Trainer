using System.Text;

namespace Crosstales.Common.Util;

public class CTProcessStartInfo
{
	public bool UseThread { get; set; }

	public bool UseCmdExecute { get; set; }

	public string FileName { get; set; }

	public string Arguments { get; set; }

	public bool CreateNoWindow { get; set; }

	public string WorkingDirectory { get; set; }

	public bool RedirectStandardOutput { get; set; }

	public bool RedirectStandardError { get; set; }

	public Encoding StandardOutputEncoding { get; set; }

	public Encoding StandardErrorEncoding { get; set; }

	public bool UseShellExecute { get; set; }

	public CTProcessStartInfo()
	{
		StandardErrorEncoding = (StandardOutputEncoding = Encoding.UTF8);
		UseThread = true;
	}
}
