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
      if (api.Side == EnumAppSide.Server) {
        ServerSettings = Config.LoadOrCreateDefault<T>(api, ServerConfigFilename);
      }
      else {
        ClientSettings = Config.LoadOrCreateDefault<U>(api, ClientConfigFilename);
      }
    }

    public override void StartServerSide(ICoreServerAPI api) {
      base.StartServerSide(api);
      ServerChannel = api.Network.RegisterChannel(ChannelName).RegisterMessageType<T>();
      api.Event.PlayerJoin += OnPlayerJoin;
    }

    private void OnPlayerJoin(IServerPlayer player) {
      ServerChannel.SendPacket(ServerSettings, player);
    }

    public override void StartClientSide(ICoreClientAPI api) {
      base.StartClientSide(api);
      ClientChannel = api.Network.RegisterChannel(ChannelName).RegisterMessageType<T>();
      ClientChannel.SetMessageHandler<T>(OnReceivedServerSettings);
    }

    private void OnReceivedServerSettings(T settings) {
      ServerSettings = settings;
      ServerSettingsReceived?.Invoke(ServerSettings);
    }
  }
}
