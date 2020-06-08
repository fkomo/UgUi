using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ujeby.UgUi.Nodes.Nodes.Crypto
{
    [NodeInfo]
	public class Sign : NodeBase
	{
        // TODO NODE Sign

        //private static byte[] Sign(byte[] data, X509Certificate2 privateKey)
        //{
        //    if (data == null)
        //    {
        //        throw new ArgumentNullException(“data”);
        //    }
        //    if (privateKey == null)
        //    {
        //        throw new ArgumentNullException(“privateKey”);
        //    }
        //    if (!privateKey.HasPrivateKey)
        //    {
        //        throw new ArgumentException(“invalid certificate”, “privateKey”);
        //    }
        //    var provider = (RSACryptoServiceProvider)privateKey.PrivateKey;
        //    return provider.SignData(data, new SHA1CryptoServiceProvider());
        //}
    }
}
