using System.Security.Cryptography;
using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Crypto
{
	[NodeInfo]
	public class MD5 : HashNode
	{
		public override void Execute()
		{
			Output = new MD5CryptoServiceProvider().ComputeHash(Input);

			base.Execute();
		}
	}
}
