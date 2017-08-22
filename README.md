# XamarinForms LiveSync (Preview)#

Não perca mais tempo compilando sua aplicação para ver o resultado das suas alterações de interface XAML.

O XamarinFormsLiveSync é uma ferramenta gratuita e de código aberto que te permite visualizar em tempo real as alterações.

Ele funciona de dentro do seu aplicativo, suportando com isso quase tudo que sua aplicação possui, como StaticResources, CustomControls, CustomRenderers, etc.

Essa abordagem traz melhores resultados se comparado com ferramentas similares como Gorilla Player, Xamarin LivePlayer ou o Visual Studio Xaml Preview.

## Como utilizar? ##

### 1) Adicionar as bibliotecas necessárias de acordo com as plataformas ###

Android

- Adicionar o pacote NuGet 'Websockets.Pcl v1.1.9'
- Fazer o download e adicionar referência ao arquivo 'XamarinFormsLiveSync.Core.dll'

iOS

- Adicionar o pacote NuGet 'Websockets Portable v.1.1.9'
- Fazer o download e adicionar referência ao arquivo 'XamarinFormsLiveSync.Core.dll'

### 2) Chamar os códigos de inicialização ###

Adicione o código abaixo de acordo com a plataforma, logo antes da linha que chama o método "LoadApplication"

Android/MainActivity.cs:
 
```
//... Xamarin.Forms.Forms.Init();

//XamarinLivesync
Websockets.Droid.WebsocketConnection.Link();
XamarinFormsLiveSync.Core.XamlLiveSyncServer.Init("http://LOCAL_IP:PORT");

//... LoadApplication(...)
```

iOS/AppDelegate.cs:

```
//... Xamarin.Forms.Forms.Init();

//XamarinLivesync
Websockets.Ios.WebsocketConnection.Link();
XamarinFormsLiveSync.Core.XamlLiveSyncServer.Init("http://LOCAL_IP:PORT");

//... LoadApplication(...)
```

### 3) Iniciar o servidor de sincronização ###

O servidor de sincronização é uma aplicação que roda no terminal que fica observando as mudanças que você faz nos arquivos *.xaml

Ao iniciar o servidor, será necessário informar o caminho completo do projeto (Geralmente o Portable) onde estão localizados os arquivos XAML.

Atenção: Para rodar o servidor você precisará ter o .Net Core Runtime 1.1+ instalado na maquina. Para instalar [clique aqui](https://www.microsoft.com/net/download/core).

Configurando e iniciando o servidor:

- Faça o download do arquivo XamarinFormsLiveSync.Server.zip
- Descompacte o arquivo
- Abra o terminal e navegue até a pasta descompactada
- Rode o comando abaixo informando o caminho para o seu projeto (--path) 
```bash
> dotnet XamarinFormsLiveSync.Server.dll --path "CAMINHO_DO_SEU_PROJETO_COM_OS_XAML"
```

### 4) Após iniciar o servidor, vamos configurar o endereço HTTP ###

- Após iniciar o servidor, será exibido uma mensagem dizendo qual o caminho da pasta que ele está escutando e qual o endereço HTTP gerado. 
- Copie o endereço e subistitua na String "http://LOCAL_IP:PORT" que você colocou nos arquivos Android/MainActivity.cs e iOS/AppDelegate.cs

### 5) Pronto! ###
- Basta rodar o aplicativo e visualizar a modal dizendo que o XamarinLiveSync está conectado.
- Nesse momento, basta acessar a tela que deseja e alterar o arquivo .xaml correspondente.

Boa codificação!

### Suporte ###
Crie um Issue ou me contate pelo email
[wilckerson@gmail.com](mailto:wilckerson@gmail.com)

### TODO ###
- Criar uma pacote Nuget para facilitar o processo de instalação
