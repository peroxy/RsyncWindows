using System;
using System.Net;
using System.Net.Sockets;

namespace RsyncBackup
{
    public class NetworkingHelper
    {
        /// Source: https://stackoverflow.com/a/6803109
        /// <summary>
        /// Gets local user's IP address.
        /// </summary>
        /// <param name="_type"></param>
        /// <returns></returns>
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
