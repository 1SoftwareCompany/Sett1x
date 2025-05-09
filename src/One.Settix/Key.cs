﻿using System;
using System.Text.RegularExpressions;

namespace One.Settix
{
    public class Key
    {
        public Key(string applicationName, string cluster, string machine, string settingKey)
        {
            ApplicationName = applicationName;
            Cluster = cluster;
            Machine = machine;
            SettingKey = settingKey;
        }

        public string Raw { get; private set; }
        public string ApplicationName { get; private set; }
        public string Cluster { get; private set; }
        public string Machine { get; private set; }
        public string SettingKey { get; private set; }

        public Key WithSettingKey(string settingKey)
        {
            if (string.IsNullOrWhiteSpace(settingKey)) throw new ArgumentException($"'{nameof(settingKey)}' cannot be null or whitespace.", nameof(settingKey));

            return new Key(ApplicationName, Cluster, Machine, settingKey);
        }

        public static Key Parse(string rawKey)
        {
            if (string.IsNullOrEmpty(rawKey)) throw new ArgumentNullException(nameof(rawKey));

            var rawKeyPattern = new Regex(@"([^@]+)@@([^\^]+)\^([^~]+)~~(.+)");

            var mappedKey = rawKeyPattern.Match(rawKey);
            if (mappedKey.Success)
            {
                return new Key(
                        applicationName: mappedKey.Groups[1].Value,
                        cluster: mappedKey.Groups[2].Value,
                        machine: mappedKey.Groups[3].Value,
                        settingKey: mappedKey.Groups[4].Value);
            }
            else
            {
                throw new ArgumentException($"Invalid Settix key: {rawKey}", nameof(rawKey));
            }
        }
    }
}
