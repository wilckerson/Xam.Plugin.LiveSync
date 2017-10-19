# XamarinForms LiveSync (Preview)

Não perca mais tempo compilando sua aplicação para ver o resultado das suas alterações de interface XAML.

O XamarinFormsLiveSync é uma ferramenta gratuita e de código aberto que te permite visualizar em tempo real as alterações sempre que você salva um arquivo XAML.

Ele funciona de dentro do seu aplicativo, suportando com isso quase tudo que sua aplicação possui, como StaticResources, CustomControls, CustomRenderers, etc.

Essa abordagem traz melhores resultados se comparado com ferramentas similares como Gorilla Player, Xamarin LivePlayer ou o Visual Studio Xaml Preview.

## NuGet
* [Xam.Plugin.LiveSync](https://www.nuget.org/packages/Xam.Plugin.LiveSync) [![NuGet](https://img.shields.io/nuget/v/Xam.Plugin.LiveSync.svg?label=NuGet)](https://www.nuget.org/packages/Xam.Plugin.LiveSync)

## Como utilizar? ##

Basta fazer da mesma forma que a maioria dos plugins para Xamarin Forms.

### 1) Adicionar o pacote NuGet no projeto Portable e nas plataformas Android (Preview) e iOS (Em breve) ###
```
Install-Package Xam.Plugin.LiveSync
```

### 2) Chamar os códigos de inicialização ###

Adicione o código abaixo de acordo com a plataforma. 

### ATENÇÂO! 
- Os códigos abaixo PRECISAM ESTAR APÓS a da linha que chama o método "LoadApplication", caso contrário uma Exception será disparada.
- É necessário ter o Dot NetCore 2.0 instalado na maquina (https://www.microsoft.com/net/download/core)
- Esse plugin só é util durante o processo de desenvolvimento, não esqueça de comentar as linhas de inicialização antes de gerar a versão Release.

Android/MainActivity.cs:
 
```
//... Xamarin.Forms.Forms.Init();
//... LoadApplication(...)

Xam.Plugin.LiveSync.Droid.LiveSync.Init();
```

iOS/AppDelegate.cs:

```
//... Xamarin.Forms.Forms.Init();
//... LoadApplication(...)

Xam.Plugin.LiveSync.iOS.LiveSync.Init();
```

### 3) Pronto! ###
- Basta rodar o aplicativo e visualizar a modal dizendo que o Xamarin Forms LiveSync está conectado.
- Nesse momento, basta acessar a tela que deseja e alterar o arquivo .xaml correspondente.

Boa codificação!

### Suporte ###
Crie um Issue ou me contate pelo email
[wilckerson@gmail.com](mailto:wilckerson@gmail.com)

### Problemas comuns ###

- Caso esteja disparando uma exception desconhecida no metodo de inicialização, verifique se o metodo Init do plugin está sendo chamado APÓS o metodo LoadApplication.
- Caso você inicie o aplicativo e não veja a modal sizendo que o LiveSync está conectado, basta dar um Rebuild no projeto Portable e depois executar normalmente.