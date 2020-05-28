using Ujeby.UgUi.Nodes;
using Ujeby.UgUi.Nodes.Types;

namespace Ujeby.UgUi.Nodes.Strings
{
	[NodeInfo]
	public class ToUpper : _String
	{
		public override void Execute()
		{
			var newValue = (input ?? string.Empty).ToUpper();
			SetField(ref this.value, newValue, nameof(Value));

			base.Execute();
		}
	}
}
