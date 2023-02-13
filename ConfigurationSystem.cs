using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace RiceConfig {
  public abstract class ConfigurationSystem<T, U> : ModSystem
                                            where T : ServerConfig, new()
                                            where U : ClientConfig, new() {
    public delegate void ServerSettingsDelegate(T serverSettings);
    public event ServerSettingsDelegate ServerSettingsReceived;

    public abstract string ChannelName { get; }

    public T ServerSettings { get; protected set; }
    public abstract string ServerConfigFilename { get; }

    public U ClientSettings { get; protected set; }
    public abstract string ClientConfigFilename { get; }

    private IServerNetworkChannel ServerChannel;
    private IClientNetworkChannel ClientChannel;

    public override bool ShouldLoad(EnumAppSide forSide) {
      return true;
    }

    public override void StartPre(ICoreAPI api) {
      base.StartPre(api);

      switch (api.Side) {
        case EnumAppSide.Server:
          StartPreServer(api as ICoreServerAPI);
          break;
        case EnumAppSide.Client:
          StartPreClient(api as ICoreClientAPI);
          break;
      }
    }

    protected virtual void StartPreServer(ICoreServerAPI sapi) {
      LoadServerConfigFromDisk(sapi);
    }

    protected virtual void LoadServerConfigFromDisk(ICoreServerAPI sapi) {
      ServerSettings = Config.LoadOrCreateDefault<T>(sapi, ServerConfigFilename);
    }

    protected virtual void StartPreClient(ICoreClientAPI capi) {
      LoadClientConfigFromDisk(capi);
    }

    protected virtual void LoadClientConfigFromDisk(ICoreClientAPI capi) {
      ClientSettings = Config.LoadOrCreateDefault<U>(capi, ClientConfigFilename);
    }

    public override void StartServerSide(ICoreServerAPI api) {
      base.StartServerSide(api);
      ServerChannel = api.Network.RegisterChannel(ChannelName).RegisterMessageType<T>();
      api.Event.PlayerJoin += OnPlayerJoin;
    }

    protected virtual void OnPlayerJoin(IServerPlayer player) {
      ServerChannel.SendPacket(ServerSettings, player);
    }

    public override void StartClientSide(ICoreClientAPI api) {
      base.StartClientSide(api);
      ClientChannel = api.Network.RegisterChannel(ChannelName).RegisterMessageType<T>();
      ClientChannel.SetMessageHandler<T>(OnReceivedServerSettings);
    }

    protected virtual void OnReceivedServerSettings(T settings) {
      ServerSettings = settings;
      ServerSettingsReceived?.Invoke(ServerSettings);
    }
  }

  public class PlaceholderClientConfig : ClientConfig { }
  public class PlaceholderServerConfig : ServerConfig { }

  public abstract class ServerOnlyConfigurationSystem<T> : ConfigurationSystem<T, PlaceholderClientConfig> where T : ServerConfig, new() {
    public sealed override string ClientConfigFilename => "";

    protected override void StartPreClient(ICoreClientAPI capi) {
      // do nothing
    }

    protected override void LoadClientConfigFromDisk(ICoreClientAPI capi) {
      // do nothing
    }
  }

  public abstract class ClientOnlyConfigurationSystem<U> : ConfigurationSystem<PlaceholderServerConfig, U> where U : ClientConfig, new() {
    public sealed override string ServerConfigFilename => "";

    public override bool ShouldLoad(EnumAppSide forSide) {
      return forSide == EnumAppSide.Client;
    }

    protected override void StartPreServer(ICoreServerAPI sapi) {
      // do nothing
    }

    protected override void LoadServerConfigFromDisk(ICoreServerAPI sapi) {
      // do nothing
    }

    public override void StartServerSide(ICoreServerAPI api) {
      // do nothing
    }

    protected override void OnPlayerJoin(IServerPlayer player) {
      // do nothing
    }

    public override void StartClientSide(ICoreClientAPI api) {
      // do nothing, configuration loading is handled in #StartPre
    }

    protected override void OnReceivedServerSettings(PlaceholderServerConfig settings) {
      // do nothing
    }
  }
}
