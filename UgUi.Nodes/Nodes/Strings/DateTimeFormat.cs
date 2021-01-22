
using System;

namespace Ujeby.UgUi.Nodes.Strings
{
	[NodeInfo(DisplayName = "DateTime.Format")]
	public class _DateTimeFormat : NodeBase
	{
		[Input(Order = 0, InputAnchor = true, AnchorOnly = true, DisplayName = "")]
		public DateTime Value { get; set; }

		protected string format = "yyyyMMdd_HHmmssfff";
		[Input(Order = 1, InputAnchor = true, AnchorOnly = false, Serializable = true)]
		public string Format
		{
			get { return format; }
			set { SetField(ref format, value, nameof(Format)); }
		}

		[Output(AnchorOnly = true)]
		public string Formatted { get; protected set; }

		public override void Execute()
		{
			Formatted = Value.ToString(format);
		}

		public override string[] GetInputs()
		{
			return new string[] 
			{
				$"{ nameof(Format) }:{ Format }"
			};
		}

		public override string[] GetOutputs()
		{
			return new string[] { Formatted };
		}
	}
}
