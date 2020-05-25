using System.IO;
using Ujeby.Common.Tools;
using Ujeby.UgUi.Core;
using Ujeby.UgUi.Operations.Abstract;

namespace Ujeby.UgUi.Operations.IO
{
	[NodeInfo]
	public class BinaryFile : _File<byte[]>
	{
		public override void Execute()
		{
			try
			{
				Length = 0;
				Read = new byte[] { };

				// write
				if (!string.IsNullOrEmpty(Path) && toWrite != null)
					File.WriteAllBytes(Path, toWrite);

				// read
				if (!string.IsNullOrEmpty(Path) && File.Exists(Path))
					Read = File.ReadAllBytes(path);

				Length = Read?.Length ?? 0;
			}
			catch (System.Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}
	}
}
