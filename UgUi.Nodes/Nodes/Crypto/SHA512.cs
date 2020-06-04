using System.Security.Cryptography;
using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Crypto
{
	[NodeInfo]
	public class SHA512 : HashNode
	{
		public override void Execute()
		{
			if (Input != null)
				Output = SHA512Managed.Create().ComputeHash(Input);
			else
				Output = null;

			base.Execute();
		}
	}
}
