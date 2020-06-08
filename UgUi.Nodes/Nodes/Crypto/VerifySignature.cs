using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ujeby.UgUi.Nodes.Nodes.Crypto
{
    [NodeInfo]
	public class VerifySignature : NodeBase
	{
        // TODO NODE VerifySignature

        //private static bool Verify(byte[] data, X509Certificate2 publicKey, byte[] signature)
        //{
        //    if (data == null)
        //    {
        //        throw new ArgumentNullException(“data”);
        //    }
        //    if (publicKey == null)
        //    {
        //        throw new ArgumentNullException(“publicKey”);
        //    }
        //    if (signature == null)
        //    {
        //        throw new ArgumentNullException(“signature”);
        //    }
        //    var provider = (RSACryptoServiceProvider)publicKey.PublicKey.Key;
        //    return provider.VerifyData(data, new SHA1CryptoServiceProvider(), signature);
        //}
    }
}
