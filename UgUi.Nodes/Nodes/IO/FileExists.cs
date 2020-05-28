using System.IO;
using Ujeby.UgUi.Nodes;
using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.IO
{
	[NodeInfo]
	public class FileExists : Check<string>
	{
		public override void Execute()
		{
			Output = !string.IsNullOrEmpty(Input) && File.Exists(Input);
		}
	}
}
