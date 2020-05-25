using System.IO;
using Ujeby.Common.Tools;
using Ujeby.UgUi.Core;
using Ujeby.UgUi.Operations.Abstract;

namespace Ujeby.UgUi.Operations.IO
{
	[NodeInfo]
	public class TextFile : _File<string>
	{
		public override void Execute()
		{
			try
			{
				Length = 0;
				Read = string.Empty;

				// write
				if (!string.IsNullOrEmpty(Path) && toWrite != null)
					File.WriteAllText(Path, toWrite, System.Text.Encoding.UTF8);

				// read
				if (!string.IsNullOrEmpty(Path) && File.Exists(Path))
					Read = File.ReadAllText(path, System.Text.Encoding.UTF8);

				Length = Read?.Length ?? 0;
			}
			catch (System.Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}
	}
}
