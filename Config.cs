using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using ProtoBuf;
using RiceConfig.Extensions;
using Vintagestory.API.Common;

namespace RiceConfig {
  [ProtoContract]
  public class Setting<T> {
    private string _description;
    public string Description {
      get { return _description; }
      set {
        List<string> restrictions = new List<string>(3);
        restrictions.Add("Default: " + Default);
        if (_minSet) { restrictions.Add("Min: " + Min); }
        if (_maxSet) { restrictions.Add("Max: " + Max); }
        _description = " [" + String.Join(", ", restrictions) + "] " + value + " ";
      }
    }

    private bool _valueSet = false;
    private T _value;
    private bool ShouldSerializeValue() => true;
    [ProtoMember(1)]
    public T Value {
      get { return _valueSet ? _value : Default; }
      set {
        if (_minSet) {
          var newValue = typeof(Math).GetTypeInfo().GetMethod("Max", new Type[] { typeof(T), typeof(T) })?.Invoke(null, new object[] { _min, value });
          if (newValue != null) { value = (T)newValue; }
        }
        if (_maxSet) {
          var newValue = typeof(Math).GetTypeInfo().GetMethod("Min", new Type[] { typeof(T), typeof(T) })?.Invoke(null, new object[] { _max, value });
          if (newValue != null) { value = (T)newValue; }
        }
        _valueSet = true;
        _value = value;
      }
    }

    public T Default { get; set; }

    private bool _minSet = false;
    private T _min;
    public T Min {
      get { return _min; }
      set {
        _minSet = true;
        _min = value;
      }
    }

    private bool _maxSet = false;
    private T _max;
    public T Max {
      get { return _max; }
      set {
        _maxSet = true;
        _max = value;
      }
    }
  }

  public class SettingConverter<T> : JsonConverter {
    public override bool CanConvert(Type objectType) {
      return objectType == typeof(Setting<T>);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
      var existingSetting = (Setting<T>)existingValue;
      existingSetting.Value = (T)Convert.ChangeType(reader.Value, typeof(T));
      return existingSetting;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
      Setting<T> setting = value as Setting<T>;
      var path = writer.Path;
      serializer.Serialize(writer, setting.Value, setting.Value.GetType());

      writer.WriteWhitespace("  ");
      writer.WriteComment(setting.Description);
    }
  }

  public abstract class Config {
    public static T LoadOrCreateDefault<T>(ICoreAPI api, string filename) where T : Config, new() {
      T config = TryLoadModConfig<T>(api, filename);

      if (config == null) {
        api.Logger.ModNotification("Unable to load valid config file. Generating {0} with defaults.", filename);
        config = new T();
      }

      config.Save(api, filename);
      return config;
    }

    // Throws exception if the config file exists, but had parsing errors.
    // Returns null if no config file exists.
    public static T TryLoadModConfig<T>(ICoreAPI api, string filename) where T : Config {
      try {
        return api.LoadModConfig<T>(filename);
      }
      catch (JsonReaderException e) {
        api.Logger.ModError("Unable to parse configuration file, {0}. Correct syntax errors and retry, or delete. {1}", filename, e);
        throw;
      }
      catch (Exception e) {
        api.Logger.ModError("I don't know what happened. Delete {0} in the mod config folder and try again. {1}", filename, e);
        throw;
      }
    }

    public void Save(ICoreAPI api, string filename) {
      api.StoreModConfig(this, filename);
    }
  }

  public abstract class ServerConfig : Config { }

  public abstract class ClientConfig : Config { }
}
