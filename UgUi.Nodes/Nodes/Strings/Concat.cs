using Ujeby.UgUi.Nodes;
using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Strings
{
	[NodeInfo]
	public class Concat : BinaryOperator<string>
	{
		protected string format = "{0}{1}";
		[Input(Order = 2, InputAnchor = true, OutputAnchor = true, Serializable = true)]
		public string Format
		{
			get { return format; }
			set { SetField(ref format, value, nameof(Format)); }
		}

		public override void Execute()
		{
			C = Format.Replace("{0}", A ?? string.Empty).Replace("{1}", B ?? string.Empty);

			base.Execute();
		}
	}
}
