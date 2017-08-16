using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;

namespace XamarinFormsLiveSync.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            //var host = new WebHostBuilder()
            //    .UseUrls("http://*:5000")
            //    .UseKestrel()
            //    .UseContentRoot(Directory.GetCurrentDirectory())
            //    .UseIISIntegration()
            //    .UseStartup<Startup>()
            //    .Build();

            //host.Run();
            XamlParser parser = new XamlParser();
            var r = parser.ExtractAstNodes(SampleXaml);
        }

        static string SampleXaml = @"
          <?xml version=""1.0"" encoding=""utf-8"" ?>
<ContentPage xmlns=""http://xamarin.com/schemas/2014/forms""
             xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
             xmlns:controls=""clr-namespace:SegmentedControl.FormsPlugin.Abstractions;assembly=SegmentedControl.FormsPlugin.Abstractions""
             x:Class=""EdebeDesafioApp.Portable.Views.AvaliacoesPage""
             Title=""Avaliações"">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height=""Auto""/>
            <RowDefinition Height=""*""/>
            <RowDefinition Height=""Auto""/>
            
        </Grid.RowDefinitions>

        <Grid BackgroundColor=""#9E1521"" >
            <controls:SegmentedControl Margin=""6"" x:Name=""segControl"" TintColor=""#E49993""  SelectedSegment=""0"" >
                <controls:SegmentedControl.Children>
                    <controls:SegmentedControlOption Text=""DISPONÍVEIS"" />
                    <controls:SegmentedControlOption Text=""REALIZADAS"" />
                    <controls:SegmentedControlOption Text=""EXPIRADAS"" />
                </controls:SegmentedControl.Children>
            </controls:SegmentedControl>
        </Grid>

        <Label Grid.Row=""1"" Text=""{Binding NenhumItemMsg}"" IsVisible=""{Binding NenhumItem}"" HorizontalOptions=""Center"" Margin=""16""/>

        <ListView Grid.Row=""1"" ItemsSource=""{Binding Avaliacoes}""
                  IsRefreshing=""{Binding IsLoading}""
                  ItemTapped=""Handle_ItemTapped""
                  IsPullToRefreshEnabled=""True""
                  RefreshCommand=""{Binding RefreshCmd}""
                  RowHeight=""200""
                  ItemAppearing=""ListView_ItemAppearing""
            CachingStrategy=""RecycleElement""
            IsVisible=""{Binding TemItens}"">

            <ListView.ItemTemplate>
                <DataTemplate>
                    <ViewCell>
                        <Grid Padding=""12"" RowSpacing=""2"">
                            <Grid.RowDefinitions>
                                <RowDefinition Height=""auto""/>
                                <RowDefinition Height=""auto""/>
                                <RowDefinition Height=""auto""/>
                                <RowDefinition Height=""auto""/>
                                <RowDefinition Height=""auto""/>
                                <RowDefinition Height=""auto""/>
                                <RowDefinition Height=""auto""/>
                                <RowDefinition Height=""auto""/>
                            </Grid.RowDefinitions>

                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width=""auto""/>
                                <ColumnDefinition Width=""*""/>
                            </Grid.ColumnDefinitions>

                            <Image Grid.RowSpan=""3"" Grid.Column=""0"" Source=""{Binding IconeDisciplina}"" HeightRequest=""56"" WidthRequest=""56"" HorizontalOptions=""Start"" VerticalOptions=""Center""/>

                            <Label  Grid.Row=""0"" Grid.Column=""1"" Text=""{Binding NomeTruncado}"" FontSize=""16"" TextColor=""#FFAB00""/>
                            <Label  Grid.Row=""1"" Grid.Column=""1"" Text=""{Binding Criador, StringFormat='Prof.: {0}'}"" FontSize=""12""/>
                            <Label  Grid.Row=""2"" Grid.Column=""1"" Text=""{Binding Descricao}"" FontSize=""12""/>

                            <Label  Grid.Row=""3"" Grid.Column=""0"" Grid.ColumnSpan=""2"" FontSize=""12"" Text=""{Binding DuracaoMin, StringFormat='Duração: {0}min'}""/>
                            <Label  Grid.Row=""4"" Grid.Column=""0"" Grid.ColumnSpan=""2"" FontSize=""12"" Text=""{Binding DescricaoDataInicio, StringFormat='Início: {0}'}""/>
                            <Label  Grid.Row=""5"" Grid.Column=""0"" Grid.ColumnSpan=""2"" FontSize=""12"" Text=""{Binding DescricaoDataFimPrazo, StringFormat='Prazo: {0}'}""/>
                            <Label  Grid.Row=""6"" Grid.Column=""0"" Grid.ColumnSpan=""2"" FontSize=""12"" Text=""{Binding PontuacaoMaxima, StringFormat='Esta prova vale {0} ponto(s)'}""/>

                            <Button IsVisible=""{Binding IsIniciada}"" Grid.Row=""7"" Grid.ColumnSpan=""2"" Text=""Continuar Avaliação""  WidthRequest=""192"" HorizontalOptions=""Start"" CommandParameter=""{Binding}"" Clicked=""ButtonIniciarAvaliacao_Clicked"" BackgroundColor=""#3C9D61"" TextColor=""White""/>
                            <Button IsVisible=""{Binding IsLiberada}"" Grid.Row=""7"" Grid.ColumnSpan=""2"" Text=""Iniciar Avaliação""  WidthRequest=""192"" HorizontalOptions=""Start"" CommandParameter=""{Binding}"" Clicked=""ButtonIniciarAvaliacao_Clicked"" BackgroundColor=""#3C9D61"" TextColor=""White""/>
                            <Label Margin=""0,8,0,0"" IsVisible=""{Binding IsPrazoEsgotado}"" Grid.Row=""7""  Grid.ColumnSpan=""2"" Text=""PRAZO ESGOTADO"" FontSize=""14""/>
                            <Label Margin=""0,8,0,0"" IsVisible=""{Binding IsAguardandoCorrecao}"" Grid.Row=""7""  Grid.ColumnSpan=""2"" Text=""FEITO, AGUARDANDO CORREÇÂO"" FontSize=""14""/>
                            <Label Margin=""0,8,0,0"" IsVisible=""{Binding IsAguardandoLiberacao}"" Grid.Row=""7""  Grid.ColumnSpan=""2"" Text=""AGUARDANDO LIBERAÇÂO"" FontSize=""14""/>

                            <Grid Margin=""0,8,0,0"" Grid.Row=""7""  Grid.ColumnSpan=""2"" IsVisible=""{Binding IsFeitaEPodeVerEspelho}"">
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width=""*""/>
                                    <ColumnDefinition Width=""auto""/>
                                    <ColumnDefinition Width=""auto""/>
                                </Grid.ColumnDefinitions>

                                <Label Text=""VISUALIZAR ESPELHO"" TextColor=""#3C9D61"" FontSize=""14"" VerticalOptions=""Center"">
                                    <Label.GestureRecognizers>
                                        <TapGestureRecognizer CommandParameter=""{Binding}"" Tapped=""VisualizarEspelho_Tapped""/>
                                    </Label.GestureRecognizers>
                                </Label>
                                <Label Grid.Column=""1"" Text=""Nota:"" TextColor=""#4387CB"" FontSize=""14"" VerticalOptions=""Center""/>
                                <Label Grid.Column=""2"" Text=""{Binding PontuacaoRealizada, StringFormat='{0:F2}'}"" TextColor=""#FFAB00"" FontSize=""24"" VerticalOptions=""Center""/>
                                
                            </Grid>

                            <Grid Margin=""0,8,0,0"" Grid.Row=""7""  Grid.ColumnSpan=""2"" IsVisible=""{Binding IsCorrecaoDisponivel}"">
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width=""*""/>
                                    <ColumnDefinition Width=""auto""/>
                                    <ColumnDefinition Width=""auto""/>
                                </Grid.ColumnDefinitions>

                                <Label Text=""VER CORREÇÂO"" TextColor=""#3C9D61"" FontSize=""14"" VerticalOptions=""Center"">
                                    <Label.GestureRecognizers>
                                        <TapGestureRecognizer CommandParameter=""{Binding}"" Tapped=""VerCorrecao_Tapped""/>
                                    </Label.GestureRecognizers>
                                </Label>
                                <Label Grid.Column=""1"" Text=""Nota:"" TextColor=""#4387CB"" FontSize=""14"" VerticalOptions=""Center""/>
                                <Label Grid.Column=""2"" Text=""{Binding PontuacaoRealizada, StringFormat='{0:F2}'}"" TextColor=""#FFAB00"" FontSize=""24"" VerticalOptions=""Center""/>

                            </Grid>
                            <!--<Label Text=""{Binding}"" 
                   Style=""{DynamicResource ListItemTextStyle}"" />-->
                            <!--<Label Text=""{Binding Text}"" 
                   Style=""{DynamicResource ListItemTextStyle}"" />
            <Label Text=""{Binding Detail}"" 
                   Style=""{DynamicResource ListItemDetailTextStyle}""/>-->
                        </Grid>
                    </ViewCell>
                </DataTemplate>
            </ListView.ItemTemplate>

        </ListView>

        <ActivityIndicator Margin=""8"" Grid.Row=""2"" IsRunning=""True""  IsVisible=""{Binding IsLoadingMore}"" Color=""#bbb""/>
       
    </Grid>
</ContentPage>
        ";
    }
}
