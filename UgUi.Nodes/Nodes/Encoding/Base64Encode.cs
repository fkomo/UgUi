using Ujeby.UgUi.Nodes;
using Ujeby.UgUi.Nodes.Types;

namespace Ujeby.UgUi.Nodes.Encoding
{
	[NodeInfo]
	public class Base64Encode : _String
	{
		public override void Execute()
		{
			var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(input ?? string.Empty);
			var newValue = System.Convert.ToBase64String(plainTextBytes);

			SetField(ref this.value, newValue, nameof(Value));

			base.Execute();
		}
	}
}
