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
                var (host, username, password, folders) = GetConfiguration();
                string localIp = NetworkingHelper.GetLocalIPAddress();

                using (var client = new SshClient(host, username, password))
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

        private static (string host, string username, string password, string[] folders) GetConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
            .AddUserSecrets(typeof(Program).Assembly)
            .Build();

            string host = configuration["BackupHostName"];
            string username = configuration["Username"];
            string password = configuration["Password"];
            string[] folders = configuration.GetSection("DeltaCopyFolderAliases").GetChildren().Select(x => x.Value).ToArray();

            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("Host name cannot be null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be null or whitespace.");
            }
            if (folders == null || folders.Length == 0)
            {
                throw new ArgumentException("DeltaCopy folder aliases have to contain at least 1 folder.");
            }

            return (host, username, password, folders);
        }
    }
}
