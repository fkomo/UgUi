using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Ujeby.UgUi.Nodes.Strings
{
	[NodeInfo]
	public class RemovePunctuation : NodeBase
	{
		[Input(Order = 0, InputAnchor = true, AnchorOnly = true, DisplayName = "")]
		public string Input { get; set; }

		protected string output = string.Empty;
		[Output(AnchorOnly = false, DisplayName = "", ReadOnly = true)]
		public string Output
		{
			get { return output; }
			set { SetField(ref output, value, nameof(Output)); }
		}

		public override void Execute()
		{
			if (string.IsNullOrEmpty(Input))
				Output = string.Empty;

			else
				Output = string
					.Concat(Input.Normalize(NormalizationForm.FormD)
					.Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark))
					.Normalize(NormalizationForm.FormC);
		}

		public override string[] GetInputs()
		{
			return new string[]
			{
				Input
			};
		}

		public override string[] GetOutputs()
		{
			return new string[]
			{
				Output
			};
		}
	}
}
