# VSLib-Riceconfig
***Not ready for use.***

Consumers (mod developers) define their own `ClientConfig`s and `ServerConfig`s. These `Config` classes consist of many `Setting<T>`s where `T` is a type understood by the Newtonsoft JsonConverter.

A `Setting<T>` requires the consumer to establish a `Default` value for the setting.
Optionally, the consumer can define a `Min` and/or a `Max` value.
In the event that a `Min` or `Max` is defined and the configuration file has been adjusted by a user to a value outside the defined range, the value will be clamped to an acceptable value.

Consumers can also tag their `Setting<T>` properties with a `[SettingDescription("")]` attribute.
The string passed to this attribute will be added after the corresponding field and value in the JSON file as a comment.
The `Default` value and, if present, `Min` and `Max` will be prefixed to the given description to help guide users on what values to fill in for settings.

When a mod is loaded for the first time, a configuration JSON will be created with the given default values.
`ClientConfig` files will be created and read only on the client-side.
`ServerConfig` files will be created and read only on the server-side, but the read values will be sent to clients during the `PlayerJoin` event.
Client-side, code can subscribe to the `ServerSettingsReceived` event to read any needed server settings.
Only those settings marked with a `[ProtoMember]` attribute will be sent over the network to the client and others will remain as their defaults.
