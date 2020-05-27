﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace GenderPayGap.Extensions
{
    public static class Networking
    {


        private static IPAddress GetNetworkAddress(this IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
            {
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");
            }

            var broadcastAddress = new byte[ipAdressBytes.Length];
            for (var i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte) (ipAdressBytes[i] & subnetMaskBytes[i]);
            }

            return new IPAddress(broadcastAddress);
        }

        private static bool IsInSameSubnet(this IPAddress address2, IPAddress address, IPAddress subnetMask)
        {
            IPAddress network1 = address.GetNetworkAddress(subnetMask);
            IPAddress network2 = address2.GetNetworkAddress(subnetMask);

            return network1.Equals(network2);
        }

        [DebuggerStepThrough]
        private static bool IsOnLocalSubnet(this string hostName)
        {
            if (string.IsNullOrWhiteSpace(hostName))
            {
                throw new Exception("Missing hostname");
            }

            try
            {
                IPAddress[] IPs = Dns.GetHostAddresses(hostName);
                if (IPs == null || IPs.Length < 1)
                {
                    throw new Exception("Could not resolve host name '" + hostName + "'");
                }

                foreach (IPAddress address in IPs)
                {
                    if (address.IsOnLocalSubnet())
                    {
                        return true;
                    }
                }
            }
            catch { }

            return false;
        }

        public static bool IsTrustedAddress(this string hostName, string[] trustedIPdomains)
        {
            if (string.IsNullOrWhiteSpace(hostName))
            {
                throw new ArgumentNullException(nameof(hostName));
            }

            if (trustedIPdomains == null || trustedIPdomains.Length == 0)
            {
                throw new ArgumentNullException(nameof(trustedIPdomains));
            }

            if (trustedIPdomains.ContainsI(hostName))
            {
                return true;
            }

            try
            {
                IPAddress[] IPs = Dns.GetHostAddresses(hostName);
                if (IPs == null || IPs.Length < 1)
                {
                    throw new Exception("Could not resolve host name '" + hostName + "'");
                }

                if (IPs.Any(address => trustedIPdomains.ContainsI(address.ToString()) || address.IsOnLocalSubnet()))
                {
                    return true;
                }
            }
            catch { }

            return hostName.IsOnLocalSubnet();
        }

        private static bool IsOnLocalSubnet(this IPAddress clientIP)
        {
            if (clientIP.ToString().EqualsI("::1", "127.0.0.1"))
            {
                return true;
            }

            foreach (NetworkInterface networkAdapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkAdapter.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties interfaceProperties = networkAdapter.GetIPProperties();
                    UnicastIPAddressInformationCollection IPsettings = interfaceProperties.UnicastAddresses;
                    foreach (UnicastIPAddressInformation IPsetting in IPsettings)
                    {
                        if (clientIP.Equals(IPsetting.Address))
                        {
                            return true;
                        }

                        if (IPsetting.IPv4Mask == null || IPsetting.IPv4Mask.ToString() == "0.0.0.0")
                        {
                            continue;
                        }

                        if (clientIP.IsInSameSubnet(IPsetting.Address, IPsetting.IPv4Mask))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }


    }
}
