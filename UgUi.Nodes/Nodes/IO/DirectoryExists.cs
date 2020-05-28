using System.IO;
using Ujeby.UgUi.Nodes;
using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.IO
{
	[NodeInfo]
	public class DirectoryExists : Check<string>
	{
		public override void Execute()
		{
			Output = !string.IsNullOrEmpty(Input) && Directory.Exists(Input);
		}
	}
}
