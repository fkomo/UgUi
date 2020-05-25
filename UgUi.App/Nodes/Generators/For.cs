using Ujeby.UgUi.Core;

namespace Ujeby.UgUi.Operations.Generators
{
	[NodeInfo]
	public class For : NodeOperationBase
	{
		protected int from = 0;
		[Input(Order = 0, InputAnchor = true, Serializable = true)]
		public int From
		{
			get { return from; }
			set 
			{
				SetField(ref from, value, nameof(From));
				Value = from; 
			}
		}

		protected int to = 0;
		[Input(Order = 1, InputAnchor = true, Serializable = true)]
		public int To
		{
			get { return to; }
			set
			{
				SetField(ref to, value, nameof(To));
				Value = from;
			}
		}

		protected int value = 0;
		[Output(AnchorOnly = false, ReadOnly = true, DisplayName = "")]
		public int Value
		{
			get { return value; }
			set { SetField(ref this.value, value, nameof(Value)); }
		}

		public override void Execute()
		{
			if (To > From && Value < To)
				Value++;
		}

		public override string[] GetInputs()
		{
			return new string[]
			{
				From.ToString(),
				To.ToString(),
			};
		}

		public override string[] GetOutputs()
		{
			return new string[] { Value.ToString() };
		}
	}
}
