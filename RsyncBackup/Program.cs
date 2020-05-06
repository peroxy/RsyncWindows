using Microsoft.Extensions.Configuration;
using Renci.SshNet;
using System;
using System.Linq;

namespace RsyncBackup
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("RSync backup started.");
                var configuration = GetConfiguration();
                string localIp = NetworkingHelper.GetLocalIPAddress();

                Console.WriteLine("Backing up ");

                using (var client = new SshClient(configuration.BackupHostName, configuration.Username, configuration.Password))
                {
                    client.Connect();
                    using var command = client.CreateCommand("ls -al");
                    var asyncExecute = command.BeginExecute();
                    command.OutputStream.CopyTo(Console.OpenStandardOutput());
                    command.EndExecute(asyncExecute);
                }

                Console.WriteLine("Finished successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected exception has occured: {ex.Message}");
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
