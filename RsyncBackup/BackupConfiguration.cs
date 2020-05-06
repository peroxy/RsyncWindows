using System;
using System.Collections.Generic;
using System.Text;

namespace RsyncBackup
{
    public class BackupConfiguration
    {
        public string BackupHostName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DeltaCopyFolderAlias[] DeltaCopyFolderAliases { get; set; }
    }

    public class DeltaCopyFolderAlias
    {
        public string Alias { get; set; }
        public string HostPath { get; set; }
    }


}
