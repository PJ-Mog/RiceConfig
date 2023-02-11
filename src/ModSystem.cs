using System;
using System.Collections.Generic;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace RiceConfig {
  public class CompassConfigurationSystem : ModSystem {
    public static readonly string ChannelName = "japanhasrice.riceconfig";
    private IServerNetworkChannel ServerChannel;
    private IClientNetworkChannel ClientChannel;
    public delegate void ServerSettingsDelegate(Dictionary<string, Config> receivedServerConfigs);
    public event ServerSettingsDelegate ServerSettingsReceived;
    public Dictionary<string, Type> ServerConfigMap = new Dictionary<string, Type>();
    public Dictionary<string, Type> ClientConfigMap = new Dictionary<string, Type>();
    public Dictionary<string, Config> LoadedConfigurations = new Dictionary<string, Config>();

    public override bool ShouldLoad(EnumAppSide forSide) {
      return true;
    }

    public override double ExecuteOrder() {
      return double.MaxValue;
    }

    public override void StartPre(ICoreAPI api) {
      base.StartPre(api);
      var loadOrCreateDefaultMethod = typeof(Config).GetMethod(nameof(Config.LoadOrCreateDefault), BindingFlags.Static | BindingFlags.Public);

      if (api.Side == EnumAppSide.Server) {
        foreach (var config in ServerConfigMap) {
          var filename = config.Key;
          var configType = config.Value;
          var loadedConfiguration = (ServerConfig)loadOrCreateDefaultMethod.MakeGenericMethod(configType).Invoke(null, new object[] { filename });

          LoadedConfigurations.Add(filename, loadedConfiguration);
        }
      }
      else {
        foreach (var config in ClientConfigMap) {
          var filename = config.Key;
          var configType = config.Value;
          var loadedConfiguration = (ClientConfig)loadOrCreateDefaultMethod.MakeGenericMethod(configType).Invoke(null, new object[] { filename });

          LoadedConfigurations.Add(filename, loadedConfiguration);
        }
      }
    }

    public override void StartServerSide(ICoreServerAPI api) {
      base.StartServerSide(api);
      api.Event.PlayerJoin += SendServerConfigurationsToClient;

      ServerChannel = api.Network.RegisterChannel(ChannelName).RegisterMessageType(LoadedConfigurations.GetType());
    }

    private void SendServerConfigurationsToClient(IServerPlayer player) {
      ServerChannel.SendPacket(LoadedConfigurations, player);
    }

    public override void StartClientSide(ICoreClientAPI api) {
      base.StartClientSide(api);
      ClientChannel = api.Network.RegisterChannel(ChannelName).RegisterMessageType(LoadedConfigurations.GetType());
      ClientChannel.SetMessageHandler<Dictionary<string, Config>>(OnClientReceivedServerConfigs);
    }

    private void OnClientReceivedServerConfigs(Dictionary<string, Config> configs) {
      foreach (var config in configs) {
        LoadedConfigurations.Add(config.Key, config.Value);
      }
      ServerSettingsReceived?.Invoke(LoadedConfigurations);
    }

    public void RegisterClientConfiguration<T>(string filename) where T : ClientConfig {
      ClientConfigMap.Add(filename, typeof(T));
    }

    public void RegisterServerConfiguration<T>(string filename) where T : ServerConfig {
      ServerConfigMap.Add(filename, typeof(T));
    }
  }
}
