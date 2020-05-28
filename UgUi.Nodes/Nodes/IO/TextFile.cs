using System.IO;
using System.Linq;
using Ujeby.Common.Tools;
using Ujeby.UgUi.Nodes;
using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.IO
{
	[NodeInfo]
	public class TextFile : _File<string>
	{
		[Output(AnchorOnly = true)]
		public string[] ReadLines { get; protected set; }

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
				{
					Read = File.ReadAllText(path, System.Text.Encoding.UTF8);
					ReadLines = File.ReadAllLines(path, System.Text.Encoding.UTF8); 
				}

				Length = Read?.Length ?? 0;
			}
			catch (System.Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}

		public override string[] GetOutputs()
		{
			return base.GetOutputs().Concat(
				new string[]
				{
					$"{ nameof(ReadLines) }.{ nameof(ReadLines.Length) }:{ ReadLines?.Length.ToString() }",
				}).ToArray();
		}
	}
}
