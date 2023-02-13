# VSLib-Riceconfig
A library to simplify handling of settings/configurations for Vintage Story mods.

## Required references

* `VintagestoryAPI.dll`
* `Newtonsoft.Json.dll`
* `protobuf-net.dll`

## Use

Consumers (mod developers) each define their own `ClientConfig` and/or `ServerConfig` classes.
Consumers then create a concrete configuration system class, inheriting from `ConfigurationSystem<T,U>` and supplying their Client and Server Config classes.
Optionally, consumers can inherit from `ServerOnlyConfigurationSystem<T>` or `ClientOnlyConfigurationSystem<U>` if their configurations are limited to a single side.

Configuration classes consist of many `Setting<T>`s where `T` is a type understood by the Newtonsoft JsonConverter.
A `Setting<T>` requires consumers to establish a `Default` value for the setting.
Optionally, consumers can define a `Min` and/or a `Max` value.
In the event that a `Min` or `Max` is defined and the configuration file has been adjusted by a user to a value outside the defined range, the value will be clamped to an acceptable value.

Consumers may also define a `Description` for their settings.
The provided string will be added after the corresponding field and value in the JSON file as a comment.
The `Default` value and, if present, `Min` and `Max` will be prefixed to the given description to help guide users on what values to fill in for settings.

When a mod is loaded for the first time, a configuration JSON will be created with the given default values.
`ClientConfig` files will be created and read only on the client-side.
`ServerConfig` files will be created and read only on the server-side, and the values read from disk will be sent to clients during the `PlayerJoin` event.
Client-side, consumers can subscribe to the `ServerSettingsReceived` event to read any needed server settings.
Only those settings marked with a `[ProtoMember]` attribute will be sent over the network to the client and others will remain as their defaults.
