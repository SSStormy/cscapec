﻿using System;
using System.Collections.Generic;
using System.IO;
using CScape.Core.Extensions;
using CScape.Models;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity.Component;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CScape.Core.Json
{
    // TODO : proper player serialization
    public sealed class PlayerJsonDatabase : IDisposable
    {
        [NotNull]
        private readonly PlayerJsonIO _serializer;

        private const string SaveDir = "players/";
        private const string PasswordsDir = "passwd.json";

        private Dictionary<string, string> _pwdLookup;

        private readonly ILogger _log;

        public PlayerJsonDatabase(IServiceProvider services)
        {
            _serializer = new PlayerJsonIO(services);
            _log = services.ThrowOrGet<ILogger>();
            LoadPwdLookup();
        }

        private void LoadPwdLookup()
        {
            Directory.CreateDirectory(SaveDir);

            if (File.Exists(PasswordsDir))
            {
                _pwdLookup = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(PasswordsDir));
            }
            else
            {
                _pwdLookup = new Dictionary<string, string>();
            }
        }

        private void SavePwdLookup()
        {
            File.WriteAllText(PasswordsDir, JsonConvert.SerializeObject(_pwdLookup));
        }

        private string MakeFileDir(string username)
            => Path.Combine(SaveDir, username + ".json");

        public bool IsValidPassword(string username, string password)
        {
            return true;
        }

        public bool PlayerExists(string username)
        {
            return false;
        }

        public void Save([NotNull] IPlayerComponent player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));

            // serialize player
            var data = _serializer.Serialize(player.Parent);
            File.WriteAllText(MakeFileDir(player.Username), data);   
        }

        public void SetPassword(string username, string password)
        {
            _pwdLookup[username] = password;
        }

        [CanBeNull]
        public SerializablePlayerModel Load([NotNull] string username)
        {
            throw new InvalidOperationException();
        }

        public void Dispose()
        {
            return;
        }
    }
}