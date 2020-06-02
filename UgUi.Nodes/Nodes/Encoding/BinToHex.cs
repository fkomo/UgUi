
using System;
using System.Linq;
using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Encoding
{
	[NodeInfo]
	public class BinToHex : EncodingNode
	{
		public override void Execute()
		{
			if (InputBin?.Length > 0)
				OutputText = new System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary(InputBin).ToString();
			else
				OutputText = string.Empty;

			if (!string.IsNullOrEmpty(InputText))
				OutputBin = Enumerable.Range(0, InputText.Length)
					 .Where(x => x % 2 == 0)
					 .Select(x => Convert.ToByte(InputText.Substring(x, 2), 16))
					 .ToArray();
			else
				OutputBin = new byte[] { };
		}
	}
}
