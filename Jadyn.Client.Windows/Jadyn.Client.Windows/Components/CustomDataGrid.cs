using Jadyn.Client.Windows.Utils;
using Jadyn.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Windows.Storage.Pickers;

namespace Jadyn.Client.Windows.Components
{
    public class CustomDataGrid<TDbContext, TModel> : UserControl
        where TDbContext : DbContext
        where TModel : BaseModel
    {
        private TDbContext DbContext { get; set; }
        private StackPanel MainContainer { get; set; }
        private Grid MainGrid { get; set; }
        private CommandBar MainCommandBar { get; set; }
        private CustomDataGridSettings Settings { get; set; }
        private int CurrentPage { get; set; }
        private IFileImporter FileImporter { get; set; }

        public CustomDataGrid(TDbContext dbContext, CustomDataGridSettings settings) 
        {
            DbContext = dbContext;
            FileImporter = ((App)(App.Current)).ServiceProvider.GetService<IFileImporter>();
            Render();
        }

        private void Render()
        {
            MainContainer = new StackPanel();
            MainCommandBar = new CommandBar();
            MainGrid = new Grid();

            MainContainer.Children.Add(MainCommandBar);
            MainContainer.Children.Add(MainGrid);
            this.Content = MainContainer;

            RenderControls();
            RenderHeader();
            RenderBody();
        }

        private void RenderControls()
        {
            var importCommand = new AppBarButton()
            {
                Content = new TextBlock()
                {
                    Text = "Import"
                }
            };
            importCommand.Click += ImportCommand_Click;
            MainCommandBar.PrimaryCommands.Add(importCommand);
        }

        private void RenderHeader()
        {
            if (Settings?.Columns == null)
            {
                RenderDefaultHeader();
                return;
            }
        }

        private void RenderBody()
        {
            if (Settings?.Columns == null)
            {
                RenderDefaultBody();
                return;
            }
        }

        private void RenderDefaultHeader()
        {
            var tempProps = typeof(TModel).GetRuntimeProperties();

            MainGrid.RowDefinitions.Add(new RowDefinition()
            {
                Height = GridLength.Auto
            });

            foreach (var prop in tempProps)
            {
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = GridLength.Auto
                });
            }

            foreach (var prop in tempProps.Select((value, i) => new { i, value }))
            {
                StackPanel tempSP = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };
                tempSP.Children.Add(new TextBlock()
                {
                    Text = prop.value.Name,
                    Style = (Style)App.Current.Resources["CustomGridCellHeader"]
                });

                MainGrid.Children.Add(tempSP);
                Grid.SetRow(tempSP, 0);
                Grid.SetColumn(tempSP, prop.i);
            }
        }

        private void RenderDefaultBody()
        {
            var tempProps = typeof(TModel).GetRuntimeProperties();
            var tempModels = DbContext.Set<TModel>()
                .ToList();

            foreach (var item in tempModels.Select((value, i) => new { i, value }))
            {
                MainGrid.RowDefinitions.Add(new RowDefinition()
                {
                    Height = GridLength.Auto
                });

                foreach (var prop in tempProps.Select((value, i) => new { i, value }))
                {
                    StackPanel tempSP = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal
                    };

                    tempSP.Children.Add(new TextBlock()
                    {
                        Text = prop.value.GetValue(item).ToString(),
                        Style = (Style)App.Current.Resources["CustomGridCell"]
                    });
                }
            }

            


        }

        private async void ImportCommand_Click(object sender, RoutedEventArgs e)
        {
            var openPicker = new FileOpenPicker();

            var window = ((App)(App.Current)).MainWindow;

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Add("*");

            var file = await openPicker.PickSingleFileAsync();
            var models = FileImporter.ImportModelsFromFile<TModel>(file);
        }
    }


    public class CustomDataGridSettings
    {
        public int? PageRowCount { get; set; }
        public List<Column>? Columns { get; set; }
    }

    public class Column
    {
        public string Name { get; set; }
        public string Property { get; set; }
        public UserControl Template { get; set; }
    }
}
