using System;

namespace Ujeby.UgUi.Nodes.Arrays
{
	[NodeInfo]
	public class SubArray : NodeBase
	{
		[Input(Order = 0, InputAnchor = true, AnchorOnly = true, DisplayName = "Array")]
		public Array Input { get; set; }

		private int start = 0;
		[Input(Order = 1, InputAnchor = true, Serializable = true)]
		public int Start
		{
			get { return start; }
			set { SetField(ref start, value, nameof(Start)); }
		}

		private int length = 0;
		[Input(Order = 2, InputAnchor = true, Serializable = true)]
		public int Length
		{
			get { return length; }
			set { SetField(ref length, value, nameof(Length)); }
		}

		[Output(AnchorOnly = true, DisplayName = "")]
		public Array Output { get; private set; }

		public override void Execute()
		{
			if (Input == null || Input.Length < Start)
				Output = null;

			else
			{
				var length = System.Math.Min(Length, Input.Length - Start);
				Output = Array.CreateInstance(Input.GetValue(0).GetType(), length);
				for (var i = 0; i < length; i++)
					Output.SetValue(Input.GetValue(i + Start), i);
			}
		}

		public override string[] GetInputs()
		{
			return new string[] 
			{ 
				//Input,
				$"{ nameof(Start) }:{ Start }",
				$"{ nameof(Length) }:{ Length }",
			};
		}

		public override string[] GetOutputs()
		{
			return new string[] 
			{ 
				//Output,
				$"{ nameof(Output) }.{ nameof(Length) }:{ (Output?.Length ?? 0).ToString() }",
			};
		}
	}
}
