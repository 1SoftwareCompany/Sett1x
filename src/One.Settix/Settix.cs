using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using One.Settix.Box;

namespace One.Settix
{
    public class Settix
    {
        readonly IConfigurationRepository cfgRepo;
        readonly ISettixContext context;

        public Settix(ISettixContext context, IConfigurationRepository configurationRepository)
        {
            this.context = context;
            this.cfgRepo = configurationRepository;
        }

        public Settix(ISettixFactory factory) : this(factory.GetContext(), factory.GetConfiguration()) { }

        public ISettixContext ApplicationContext { get { return context; } }

        public Task<string> GetAsync(string settingKey)
        {
            return GetAsync(settingKey, context);
        }

        public async Task<string> GetAsync(string settingKey, ISettixContext applicationContext)
        {
            if (string.IsNullOrEmpty(settingKey)) throw new ArgumentNullException(nameof(settingKey));
            if (ReferenceEquals(null, applicationContext)) throw new ArgumentNullException(nameof(applicationContext));

            var sanitizedKey = settingKey.ToLower();
            string keyForMachine = NameBuilder.GetSettingName(applicationContext.ApplicationName, applicationContext.Cluster, applicationContext.Machine, sanitizedKey);

            bool exitst = await cfgRepo.ExistsAsync(keyForMachine).ConfigureAwait(false);
            if (exitst)
            {
                return await cfgRepo.GetAsync(keyForMachine).ConfigureAwait(false);
            }
            else
            {
                string keyForCluster = NameBuilder.GetSettingName(applicationContext.ApplicationName, applicationContext.Cluster, Machine.NotSpecified, sanitizedKey);
                return await cfgRepo.GetAsync(keyForCluster).ConfigureAwait(false);
            }
        }

        public Task<T> GetAsync<T>(string settingKey)
        {
            return GetAsync<T>(settingKey, context);
        }

        public async Task<T> GetAsync<T>(string settingKey, ISettixContext context)
        {
            string value = await GetAsync(settingKey, context).ConfigureAwait(false);
            if (value == null)
                return default(T);

            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter.IsValid(value))
            {
                T converted = (T)converter.ConvertFrom(value);
                return converted;
            }
            else
            {
                var result = JsonSerializer.Deserialize<T>(value);
                return result;
            }
        }

        public IEnumerable<DeployedSetting> GetAll(ISettixContext context)
        {
            try
            {
                IEnumerable<DeployedSetting> allKeys = cfgRepo.GetAll(context);

                IEnumerable<DeployedSetting> clusterKeys = from setting in allKeys
                                                           where setting.Key.Cluster.Equals(context.Cluster, StringComparison.OrdinalIgnoreCase) &&
                                                                 setting.Key.Machine.Equals(Box.Machine.NotSpecified, StringComparison.OrdinalIgnoreCase) &&
                                                                 setting.Key.ApplicationName.Equals(context.ApplicationName, StringComparison.OrdinalIgnoreCase)
                                                           select setting;

                IEnumerable<DeployedSetting> machineKeys = from setting in allKeys
                                                           where setting.Key.Cluster.Equals(context.Cluster, StringComparison.OrdinalIgnoreCase) &&
                                                                 setting.Key.Machine.Equals(context.Machine, StringComparison.OrdinalIgnoreCase) &&
                                                                 setting.Key.ApplicationName.Equals(context.ApplicationName, StringComparison.OrdinalIgnoreCase)
                                                           select setting;

                var merged = clusterKeys.Select(item => machineKeys.SingleOrDefault(x => x.Key.SettingKey.Equals(item.Key.SettingKey, StringComparison.OrdinalIgnoreCase)) ?? item);
                var result = new List<DeployedSetting>();
                foreach (var item in merged)
                {
                    var reader = new Utf8JsonReader(new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(item.Value)));
                    var parsed = false;
                    JsonDocument document = default;

                    try
                    {
                        parsed = JsonDocument.TryParseValue(ref reader, out document);
                    }
                    catch (Exception) { }

                    if (parsed)
                    {
                        var parsedSettings = ParseNestedSetting(document.RootElement, item.Key);
                        result.AddRange(parsedSettings);
                        document?.Dispose();
                    }
                    else
                        result.Add(item);
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Settix configuration error: {ex.Message}");
                return Enumerable.Empty<DeployedSetting>();
            }

            static List<DeployedSetting> ParseNestedSetting(JsonElement element, Key key, int? index = null)
            {
                var result = new List<DeployedSetting>();

                if (element.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in element.EnumerateObject())
                    {
                        var settingKey = $"{key.SettingKey}:{prop.Name}";
                        if (index.HasValue)
                            settingKey = $"{key.SettingKey}:{index.Value}:{prop.Name}";

                        var newKey = new Key(key.ApplicationName, key.Cluster, key.Machine, settingKey);
                        var nested = ParseNestedSetting(prop.Value, newKey);
                        result.AddRange(nested);
                    }
                }
                else if (element.ValueKind == JsonValueKind.Array)
                {
                    var i = 0;
                    foreach (var arrayItem in element.EnumerateArray())
                    {
                        var newKey = key;
                        if (index.HasValue)
                            newKey = key.WithSettingKey($"{key.SettingKey}:{index.Value}");

                        var nested = ParseNestedSetting(arrayItem, newKey, i++);
                        result.AddRange(nested);
                    }
                }
                else
                {
                    var newKey = key;
                    if (index.HasValue)
                        newKey = key.WithSettingKey($"{key.SettingKey}:{index.Value}");

                    var value = element.ToString();
                    if (string.IsNullOrEmpty(value))
                    {
                        Console.WriteLine($"Missing Settix setting value for key {newKey.SettingKey}");
                    }
                    else
                    {
                        result.Add(new DeployedSetting(newKey, value));
                    }
                }

                return result;
            }
        }

        public Task SetAsync(string settingKey, string value)
        {
            return SetAsync(settingKey, value, context);
        }

        public Task SetAsync(string settingKey, string value, ISettixContext context)
        {
            var settingName = NameBuilder.GetSettingName(context.ApplicationName, context.Cluster, context.Machine, settingKey);
            return cfgRepo.SetAsync(settingName, value);
        }

        public Task DeleteAsync(string settingKey)
        {
            return DeleteAsync(settingKey, context);
        }

        public Task DeleteAsync(string settingKey, ISettixContext context)
        {
            var settingName = NameBuilder.GetSettingName(context.ApplicationName, context.Cluster, context.Machine, settingKey);
            return cfgRepo.DeleteAsync(settingName);
        }
    }
}
