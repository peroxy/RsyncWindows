using Microsoft.Extensions.Configuration;
using Renci.SshNet;
using System;

namespace RsyncBackup
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("RSync backup started.");
                var (host, username, password) = GetSecrets();

                using (var client = new SshClient(host, username, password))
                {
                    client.Connect();
                    using var command = client.CreateCommand("command");
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

        private static (string host, string username, string password) GetSecrets()
        {
            var configuration = new ConfigurationBuilder()
            .AddUserSecrets(typeof(Program).Assembly)
            .Build();

            string host = configuration["BackupHostName"];
            string username = configuration["Username"];
            string password = configuration["Password"];
            
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

            return (host, username, password);
        }
    }
}
