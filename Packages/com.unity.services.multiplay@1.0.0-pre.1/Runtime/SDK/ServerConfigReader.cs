using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Services.Multiplay
{
    internal class ServerConfigReader : IServerConfigReader
    {
        private const string k_ServerJsonFileName = "server.json";

        public ServerConfig LoadServerConfig()
        {
#if UNITY_EDITOR
            return LoadServerConfigInEditor();
#else
            return LoadServerConfigInServer();
#endif
        }

#if UNITY_EDITOR
        public ServerConfig LoadServerConfigInEditor()
        {
            if (GetServerConfigHomeFilePath(out var homeFilePath))
            {
                return LoadServerConfig(homeFilePath);
            }

            if (GetServerConfigLocalFilePath(out var localFilePath))
            {
                return LoadServerConfig(localFilePath);
            }

            if (GetServerConfigSubdirectoryFilePath(out var subdirectoryFilePath))
            {
                return LoadServerConfig(subdirectoryFilePath);
            }

            throw new InvalidOperationException($"Failed to load {nameof(ServerConfig)}. Ensure there is a test server.json file available in the project directory (or a subdirectory), or at $HOME: {homeFilePath}");
        }

        private bool GetServerConfigLocalFilePath(out string serverJsonFilePath)
        {
            serverJsonFilePath = k_ServerJsonFileName;
            return File.Exists(k_ServerJsonFileName);
        }

        private bool GetServerConfigSubdirectoryFilePath(out string serverJsonFilePath)
        {
            serverJsonFilePath = CheckForServerJsonFile(Directory.GetCurrentDirectory());
            return serverJsonFilePath != null;
        }

        private string CheckForServerJsonFile(string root)
        {
            var directories = Directory.GetDirectories(root);
            foreach (var directory in directories)
            {
                var files = Directory.GetFiles(directory);
                foreach (var file in files)
                {
                    var filename = Path.GetFileName(file);
                    if (filename == k_ServerJsonFileName)
                    {
                        return file;
                    }
                }
                var serverJsonFile = CheckForServerJsonFile(directory);
                if (serverJsonFile != null)
                {
                    return serverJsonFile;
                }
            }
            return null;
        }

#else
        private ServerConfig LoadServerConfigInServer()
        {
            if (GetServerConfigHomeFilePath(out var filePath))
            {
                return LoadServerConfig(filePath);
            }
            throw new InvalidOperationException($"Failed to load {nameof(ServerConfig)} from path[{filePath}] as it does not exist!");
        }

#endif

        private bool GetServerConfigHomeFilePath(out string serverJsonFilePath)
        {
            var home = Environment.GetEnvironmentVariable("HOME");
#if UNITY_STANDALONE_WIN
            serverJsonFilePath = $"{home}\\{k_ServerJsonFileName}";
#elif UNITY_STANDALONE_LINUX
            serverJsonFilePath = $"{home}/{k_ServerJsonFileName}";
#elif UNITY_STANDALONE_OSX
            serverJsonFilePath = $"{home}/{k_ServerJsonFileName}";
#else
            serverJsonFilePath = string.Empty;
#endif
            return File.Exists(serverJsonFilePath);
        }

        private ServerConfig LoadServerConfig(string path)
        {
            MultiplayServiceLogging.Verbose($"server.json exists: {path}");
            var serverJsonContents = File.ReadAllText(path);
            MultiplayServiceLogging.Verbose(serverJsonContents);
            return JsonConvert.DeserializeObject<ServerConfig>(serverJsonContents);
        }
    }
}
