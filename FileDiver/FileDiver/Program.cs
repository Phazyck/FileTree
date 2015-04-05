using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileDiver
{
    public class Program
    {
        private static int? _limit;
        private static bool _showSystem;
        private static bool _showHidden;
        private static bool _ignoreFiles;
        private static bool _fullPath;
        private static bool _prune;

        public static void Main(string[] args)
        {
            _showSystem = false;

            for (var i = 0; i < args.Length; ++i)
            {
                var option = args[i];

                switch (option)
                {
                    case "-a" :
                        _showHidden = true;
                        break;
                    case "-d" :
                        _ignoreFiles = true;
                        break;
                    case "-f" :
                        _fullPath = true;
                        break;
                    case "-L" :
                        int level;
                        if (int.TryParse(args[++i], out level))
                        {
                            _limit = level;
                        }
                        else
                        {
                            Fail();
                            return;
                        }
                        break;
                    case "--prune":
                        _prune = true;
                        break;
                    default:
                        Fail();
                        return;
                }
            }

            var info = new DirectoryInfo(Directory.GetCurrentDirectory());
            WriteInfo("", info, 0, true);
        }

        private static void Fail()
        {
            Console.WriteLine("Unknown Argument");
        }

        private static void WriteInfo(string prefix, FileSystemInfo info, int level, bool last)
        {
            if (_limit.HasValue && level > _limit.Value) return;

            var name = _fullPath ? info.FullName : info.Name;

            Console.WriteLine(level == 0 ? "." : prefix + (last ? "└── " : "├── ") + name);

            var directoryInfo = info as DirectoryInfo;

            if (directoryInfo == null) return;

            var entries = GetInfos(directoryInfo).ToArray();
            var lastEntry = entries.LastOrDefault();

            if(level > 0) prefix += last ? "    " : "│   ";

            foreach (var entry in entries)
                WriteInfo(prefix, entry, level + 1, entry.Equals(lastEntry));
         }

        private static IEnumerable<FileSystemInfo> GetInfos(DirectoryInfo directoryInfo)
        {

            return
                from info 
                    in directoryInfo.GetFileSystemInfos() 
                    where _showSystem || !info.Attributes.HasFlag(FileAttributes.System)
                    where !_ignoreFiles || !(info is FileInfo)
                    where _showHidden || !info.Attributes.HasFlag(FileAttributes.Hidden)
                    where !(info is DirectoryInfo) || !_prune || GetInfos((DirectoryInfo) info).Any()
                select info;

        }

    }
}
