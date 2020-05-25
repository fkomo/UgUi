using Ujeby.UgUi.Core;

namespace Ujeby.UgUi.Operations.Strings
{
	[NodeInfo(DisplayName = "Length")]
	public class _Length : NodeOperationBase
	{
		[Input(Order = 0, InputAnchor = true, AnchorOnly = true, DisplayName = "", Serializable = true)]
		public string Input { get; set; }

		protected int value;
		[Output(ReadOnly = true, DisplayName = "")]
		public int Value
		{
			get { return value; }
			set { SetField(ref this.value, value, nameof(Value)); }
		}

		public override void Execute()
		{
			Value = Input.Length;
		}

		public override string[] GetInputs()
		{
			return new string[] { Input };
		}

		public override string[] GetOutputs()
		{
			return new string[] { Value.ToString() };
		}
	}
}
