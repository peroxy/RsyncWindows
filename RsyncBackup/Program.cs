using Microsoft.Extensions.Configuration;
using Renci.SshNet;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace RsyncBackup


{
    internal class Program
    {

        [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow([In] IntPtr hWnd, [In] int nCmdShow);

        private static void Main(string[] args)
        {
            try
            {
                // minimize console 
                IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
                ShowWindow(handle, 6);

                var configuration = GetConfiguration();
                string localIp = NetworkingHelper.GetLocalIPAddress();

                Colorful.Console.WriteLine("RSync backup started.", Color.Green);
                Colorful.Console.WriteLine($"Backing up {localIp} to {configuration.BackupHostName}", Color.LightBlue);
                Console.WriteLine("-----------------------------------------------------------------------------------------------");
                Console.WriteLine("-----------------------------------------------------------------------------------------------");

                using (var client = new SshClient(configuration.BackupHostName, configuration.Username, configuration.Password))
                {
                    client.ConnectionInfo.Timeout = TimeSpan.FromHours(5);
                    client.Connect();
                    client.CreateCommand("pkill -f rsync").Execute(); //kills all previous rsync processes, if this script has been terminated

                    foreach (var folder in configuration.DeltaCopyFolderAliases)
                    {
                        string commandText = $"rsync -avz --chmod=a=rwX --progress {localIp}::{folder.Alias} '{folder.HostPath}'";
                        Colorful.Console.WriteLine($"Starting rsync for local DeltaCopy virtual directory with alias {folder.Alias} and host path {folder.HostPath}.", Color.Orange);
                        Colorful.Console.WriteLine(commandText, Color.Orange);
                        Console.WriteLine("-----------------------------------------------------------------------------------------------");
                        
                        using var command = client.CreateCommand(commandText);
                        var result = command.BeginExecute();
                        using (var reader = new StreamReader(command.OutputStream, Encoding.UTF8, true, 1024, true))
                        {
                            while (!result.IsCompleted || !reader.EndOfStream)
                            {
                                string line = reader.ReadLine();
                                if (line != null)
                                {
                                    if (!line.Contains('%'))
                                    {
                                        Console.WriteLine(line);
                                    } else
                                    {
                                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                                        Console.Write(line);
                                    }
                                }
                            }
                        }
                        command.EndExecute(result);

                        Colorful.Console.WriteLine($"Finished rsync for local DeltaCopy virtual directory with alias {folder.Alias} and host path {folder.HostPath}.", Color.Green);
                    }

                    client.Disconnect();
                }

                Colorful.Console.WriteLine("Finished successfully, exiting...", Color.Green);
            }
            catch (Exception ex)
            {
                Colorful.Console.WriteLine($"Unexpected exception has occured: {ex.Message}", Color.Red);
                Console.WriteLine(ex);
                Console.WriteLine("Exiting...");
                return;
            }
        }

        private static BackupConfiguration GetConfiguration()
        {
            var configuration = new ConfigurationBuilder()
            .AddUserSecrets(typeof(Program).Assembly)
            .Build();

            var config = configuration.GetSection("BackupConfiguration").Get<BackupConfiguration>();
            if (string.IsNullOrWhiteSpace(config.BackupHostName))
            {
                throw new ArgumentException("Host name cannot be null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(config.Username))
            {
                throw new ArgumentException("Username cannot be null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(config.Password))
            {
                throw new ArgumentException("Password cannot be null or whitespace.");
            }
            if (config.DeltaCopyFolderAliases == null || config.DeltaCopyFolderAliases.Length == 0)
            {
                throw new ArgumentException("DeltaCopy folder aliases have to contain at least 1 folder.");
            }

            return config;
        }
    }
}
