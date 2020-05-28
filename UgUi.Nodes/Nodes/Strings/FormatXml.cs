using Ujeby.UgUi.Nodes;
using Ujeby.UgUi.Nodes.Types;

namespace Ujeby.UgUi.Nodes.Strings
{
	[NodeInfo]
	public class FormatXml : _String
	{
		public override void Execute()
		{
			var newValue = Ujeby.Common.Tools.Utils.GetFormattedXml(input);

			SetField(ref this.value, newValue, nameof(Value));

			base.Execute();
		}
	}
}
