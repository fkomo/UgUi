using Ujeby.UgUi.Core;
using Ujeby.UgUi.Operations.Types;

namespace Ujeby.UgUi.Operations.Strings
{
	[NodeInfo]
	public class ToLower : _String
	{
		public override void Execute()
		{
			var newValue = (input ?? string.Empty).ToLower();
			SetField(ref this.value, newValue, nameof(Value));

			base.Execute();
		}
	}
}
