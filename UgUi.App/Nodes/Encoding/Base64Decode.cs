using Ujeby.UgUi.Core;
using Ujeby.UgUi.Operations.Types;

namespace Ujeby.UgUi.Operations.Encoding
{
	[NodeInfo]
	public class Base64Decode : _String
	{
		public override void Execute()
		{
			var base64EncodedBytes = System.Convert.FromBase64String(input ?? string.Empty);
			var newValue = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);

			SetField(ref this.value, newValue, nameof(Value));

			base.Execute();
		}
	}
}
 