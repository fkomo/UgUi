using Newtonsoft.Json;
using Ujeby.UgUi.Core;

namespace Ujeby.UgUi.Operations.Generators
{
	[NodeInfo(DisplayName = "DateTime")]
	public class _DateTime : NodeOperationBase, ISerializableNode
	{
		protected string format = "yyyyMMdd_HHmmssfff";
		[Input(Order = 0, InputAnchor = true, AnchorOnly = false, Serializable = true)]
		public string Format
		{
			get { return format; }
			set { SetField(ref format, value, nameof(Format)); }
		}

		protected string value = "";
		[Output(AnchorOnly = false, ReadOnly = true, DisplayName = "")]
		public string Value
		{
			get { return value; }
			set { SetField(ref this.value, value, nameof(Value)); }
		}

		public override void Execute()
		{
			Value = System.DateTime.Now.ToString(format);
		}

		public override string[] GetInputs()
		{
			return new string[] { Format };
		}

		public override string[] GetOutputs()
		{
			return new string[] { Value };
		}
	}
}
