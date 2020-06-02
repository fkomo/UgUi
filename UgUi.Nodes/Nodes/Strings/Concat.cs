using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Strings
{
	[NodeInfo]
	public class Concat : BinaryOperator<string>
	{
		protected string format = "{A}{B}";
		[Input(Order = 2, InputAnchor = true, OutputAnchor = true, Serializable = true)]
		public string Format
		{
			get { return format; }
			set { SetField(ref format, value, nameof(Format)); }
		}

		public override void Execute()
		{
			C = Format.Replace("{A}", A ?? string.Empty).Replace("{B}", B ?? string.Empty);

			base.Execute();
		}
	}
}
