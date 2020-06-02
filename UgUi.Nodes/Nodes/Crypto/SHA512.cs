using System.Security.Cryptography;
using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Crypto
{
	[NodeInfo]
	public class SHA512 : HashNode
	{
		public override void Execute()
		{
			Output = SHA512Managed.Create().ComputeHash(Input);

			base.Execute();
		}
	}
}
