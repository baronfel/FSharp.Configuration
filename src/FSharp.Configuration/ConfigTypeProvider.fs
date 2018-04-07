module FSharp.Configuration.ConfigTypeProvider

open FSharp.Configuration.Helper
open Microsoft.FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes

[<TypeProvider>]
type FSharpConfigurationProvider(cfg: TypeProviderConfig) as this =
    class
        inherit TypeProviderForNamespaces(cfg)
        let context = new Context(this, cfg)
        let runtimeAssy = 
            let assyName = System.IO.Path.GetFileNameWithoutExtension cfg.RuntimeAssembly
            match this.TargetContext.TryBindSimpleAssemblyNameToTarget assyName with
            | Choice1Of2 assy -> assy
            | Choice2Of2 err -> raise err

        do this.AddNamespace (
            rootNamespace,
            [ AppSettingsTypeProvider.typedAppSettings context runtimeAssy
#if !NETSTANDARD2_0
              ResXProvider.typedResources context runtimeAssy
#endif              
              YamlConfigTypeProvider.typedYamlConfig context runtimeAssy
              IniFileProvider.typedIniFile context runtimeAssy ])
        do this.Disposing.Add (fun _ -> dispose context)
    end

[<TypeProviderAssembly>]
do ()