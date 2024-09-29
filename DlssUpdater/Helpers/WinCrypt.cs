using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Pkcs;

namespace CLRSignatures
{
    public static class Wincrypt
    {
        // source type
        public const int CERT_QUERY_OBJECT_FILE = 1;
        // object type
        const int CERT_QUERY_CONTENT_CERT = 1;
        const int CERT_QUERY_CONTENT_CTL = 2;
        const int CERT_QUERY_CONTENT_CRL = 3;
        const int CERT_QUERY_CONTENT_SERIALIZED_STORE = 4;
        const int CERT_QUERY_CONTENT_SERIALIZED_CERT = 5;
        const int CERT_QUERY_CONTENT_SERIALIZED_CTL = 6;
        const int CERT_QUERY_CONTENT_SERIALIZED_CRL = 7;
        const int CERT_QUERY_CONTENT_PKCS7_SIGNED = 8;
        const int CERT_QUERY_CONTENT_PKCS7_UNSIGNED = 9;
        const int CERT_QUERY_CONTENT_PKCS7_SIGNED_EMBED = 10;
        const int CERT_QUERY_CONTENT_PKCS10 = 11;
        const int CERT_QUERY_CONTENT_PFX = 12;
        const int CERT_QUERY_CONTENT_CERT_PAIR = 13;

        const int CERT_QUERY_CONTENT_FLAG_CERT = (1 << CERT_QUERY_CONTENT_CERT);
        const int CERT_QUERY_CONTENT_FLAG_CTL = (1 << CERT_QUERY_CONTENT_CTL);
        const int CERT_QUERY_CONTENT_FLAG_CRL = (1 << CERT_QUERY_CONTENT_CRL);
        const int CERT_QUERY_CONTENT_FLAG_SERIALIZED_STORE = (1 << CERT_QUERY_CONTENT_SERIALIZED_STORE);
        const int CERT_QUERY_CONTENT_FLAG_SERIALIZED_CERT = (1 << CERT_QUERY_CONTENT_SERIALIZED_CERT);
        const int CERT_QUERY_CONTENT_FLAG_SERIALIZED_CTL = (1 << CERT_QUERY_CONTENT_SERIALIZED_CTL);
        const int CERT_QUERY_CONTENT_FLAG_SERIALIZED_CRL = (1 << CERT_QUERY_CONTENT_SERIALIZED_CRL);
        const int CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED = (1 << CERT_QUERY_CONTENT_PKCS7_SIGNED);
        const int CERT_QUERY_CONTENT_FLAG_PKCS7_UNSIGNED = (1 << CERT_QUERY_CONTENT_PKCS7_UNSIGNED);
        const int CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED_EMBED = (1 << CERT_QUERY_CONTENT_PKCS7_SIGNED_EMBED);
        const int CERT_QUERY_CONTENT_FLAG_PKCS10 = (1 << CERT_QUERY_CONTENT_PKCS10);
        const int CERT_QUERY_CONTENT_FLAG_PFX = (1 << CERT_QUERY_CONTENT_PFX);
        const int CERT_QUERY_CONTENT_FLAG_CERT_PAIR = (1 << CERT_QUERY_CONTENT_CERT_PAIR);
        public const int CERT_QUERY_CONTENT_FLAG_ALL =
            CERT_QUERY_CONTENT_FLAG_CERT |
            CERT_QUERY_CONTENT_FLAG_CTL |
            CERT_QUERY_CONTENT_FLAG_CRL |
            CERT_QUERY_CONTENT_FLAG_SERIALIZED_STORE |
            CERT_QUERY_CONTENT_FLAG_SERIALIZED_CERT |
            CERT_QUERY_CONTENT_FLAG_SERIALIZED_CTL |
            CERT_QUERY_CONTENT_FLAG_SERIALIZED_CRL |
            CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED |
            CERT_QUERY_CONTENT_FLAG_PKCS7_UNSIGNED |
            CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED_EMBED |
            CERT_QUERY_CONTENT_FLAG_PKCS10 |
            CERT_QUERY_CONTENT_FLAG_PFX |
            CERT_QUERY_CONTENT_FLAG_CERT_PAIR;

        // format type
        const int CERT_QUERY_FORMAT_BINARY = 1;
        const int CERT_QUERY_FORMAT_BASE64_ENCODED = 2;
        const int CERT_QUERY_FORMAT_ASN_ASCII_HEX_ENCODED = 3;
        const int CERT_QUERY_FORMAT_FLAG_BINARY = 1 << CERT_QUERY_FORMAT_BINARY;
        const int CERT_QUERY_FORMAT_FLAG_BASE64_ENCODED = 1 << CERT_QUERY_FORMAT_BASE64_ENCODED;
        const int CERT_QUERY_FORMAT_FLAG_ASN_ASCII_HEX_ENCODED = 1 << CERT_QUERY_FORMAT_ASN_ASCII_HEX_ENCODED;
        public const int CERT_QUERY_FORMAT_FLAG_ALL =
            CERT_QUERY_FORMAT_FLAG_BINARY |
            CERT_QUERY_FORMAT_FLAG_BASE64_ENCODED |
            CERT_QUERY_FORMAT_FLAG_ASN_ASCII_HEX_ENCODED;

        public const int CMSG_ENCODED_MESSAGE = 29;

        public static bool CheckSignature(string file)
        {
            IntPtr phCertStore = IntPtr.Zero;
            IntPtr phMsg = IntPtr.Zero;
            IntPtr ppvContext = IntPtr.Zero;
            int pdwMsgAndCertEncodingType = 0;
            int pdwContentType = 0;
            int pdwFormatType = 0;
            if (!Crypt32.CryptQueryObject(
                Wincrypt.CERT_QUERY_OBJECT_FILE,
                file,
                Wincrypt.CERT_QUERY_CONTENT_FLAG_ALL,
                Wincrypt.CERT_QUERY_FORMAT_FLAG_ALL,
                0,
                ref pdwMsgAndCertEncodingType,
                ref pdwContentType,
                ref pdwFormatType,
                ref phCertStore,
                ref phMsg,
                ref ppvContext
            ))
            {
                Console.WriteLine((new Win32Exception(Marshal.GetLastWin32Error())).Message);
                return false;
            }
            int pcbData = 0;
            if (!Crypt32.CryptMsgGetParam(phMsg, Wincrypt.CMSG_ENCODED_MESSAGE, 0, null, ref pcbData))
            {
                Console.WriteLine((new Win32Exception(Marshal.GetLastWin32Error())).Message);
                return false;
            }
            byte[] pvData = new byte[pcbData];
            Crypt32.CryptMsgGetParam(phMsg, Wincrypt.CMSG_ENCODED_MESSAGE, 0, pvData, ref pcbData);
            var signedCms = new SignedCms();
            signedCms.Decode(pvData);
            try
            {
                signedCms.CheckSignature(false);
                Console.WriteLine("Signature check passed");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                Crypt32.CryptMsgClose(phMsg);
                Crypt32.CertCloseStore(phCertStore, 0);
            }

            return false;
        }

    }
    public static class Crypt32
    {

        [DllImport("crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CryptQueryObject(
            int dwObjectType,
            [MarshalAs(UnmanagedType.LPWStr)]
            string pvObject,
            int dwExpectedContentTypeFlags,
            int dwExpectedFormatTypeFlags,
            int dwFlags,
            ref int pdwMsgAndCertEncodingType,
            ref int pdwContentType,
            ref int pdwFormatType,
            ref IntPtr phCertStore,
            ref IntPtr phMsg,
            ref IntPtr ppvContext
        );
        [DllImport("crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CryptMsgGetParam(
            IntPtr hCryptMsg,
            int dwParamType,
            int dwIndex,
            byte[] pvData,
            ref int pcbData
        );
        [DllImport("crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CryptMsgClose(
            IntPtr hCryptMsg
        );
        [DllImport("crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CertCloseStore(
            IntPtr hCertStore,
            int dwFlags
        );
    }
}