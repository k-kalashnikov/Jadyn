using Jadyn.Client.Windows.Utils;
using Jadyn.Common.Models;
using Jadyn.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Windows.Storage.Pickers;
using Windows.UI;

namespace Jadyn.Client.Windows.Components
{
    public class CustomDataGrid<TDbContext, TModel> : UserControl
        where TDbContext : DbContext
        where TModel : BaseModel
    {
        public int CountPage { get; set; }
        private TDbContext DbContext { get; set; }
        private StackPanel MainContainer { get; set; }
        private Grid MainGrid { get; set; }
        private CommandBar MainCommandBar { get; set; }
        private CommandBar PaginationControlBar { get; set; }
        private AppBarButton LeftButton { get; set; }
        private AppBarButton RightButton { get; set; }
        private CustomDataGridSettings Settings { get; set; }
        private int CurrentPage { get; set; } = 0;
        private int TotalCount { get; set; } = 0;
        private IFileImporter FileImporter { get; set; }
        private IList<TModel> Items { get; set; }

        public CustomDataGrid(CustomDataGridSettings settings)
        {
            FileImporter = ((App)(App.Current)).ServiceProvider.GetService<IFileImporter>();
            DbContext = ((App)(App.Current)).ServiceProvider.GetService<TDbContext>();
            CountPage = settings?.PageRowCount ?? 10;
            Render();
        }

        private void Render()
        {
            MainContainer = new StackPanel();
            MainCommandBar = new CommandBar();
            PaginationControlBar = new CommandBar();
            MainGrid = new Grid();

            MainContainer.Children.Add(MainCommandBar);
            MainContainer.Children.Add(MainGrid);
            MainContainer.Children.Add(PaginationControlBar);

            this.Content = MainContainer;

            try
            {
                UpdateData();
                RenderControls();
                RenderHeader();
                RenderBody();
                RenderFooter();
            }
            catch (Exception ex) 
            {
                ContentDialog dialog = new ContentDialog();
                dialog.XamlRoot = this.XamlRoot;
                dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                dialog.Title = "Exception";
                dialog.PrimaryButtonText = "Ok";
                dialog.DefaultButton = ContentDialogButton.Primary;
                dialog.Content = new TextBlock()
                {
                    Text = $"Error => {ex.Message}"
                };

                var result = dialog.ShowAsync()
                    .GetResults();

            }

        }

        private void RenderFooter()
        {
            LeftButton = new AppBarButton()
            {
                Content = new TextBlock()
                {
                    Text = "Prev"
                }
            };
            LeftButton.Click += LeftPageButton_Click;

            RightButton = new AppBarButton()
            {
                Content = new TextBlock()
                {
                    Text = "Next"
                }
            };

            RightButton.Click += RightPageButton_Click;

            PaginationControlBar.PrimaryCommands.Add(LeftButton);
            PaginationControlBar.PrimaryCommands.Add(RightButton);
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

            var refreshCommand = new AppBarButton()
            {
                Content = new TextBlock()
                {
                    Text = "Refresh"
                }
            };
            refreshCommand.Click += RefreshCommand_Click;
            MainCommandBar.PrimaryCommands.Add(importCommand);
            MainCommandBar.PrimaryCommands.Add(refreshCommand);
        }

        private void RefreshCommand_Click(object sender, RoutedEventArgs e)
        {
            MainGrid.Children.Clear();
            RenderHeader();
            RenderBody();
        }

        private void RenderHeader()
        {
            if (Settings?.Columns == null)
            {
                RenderDefaultHeader();
                return;
            }
            //TODO add render with custom colums
        }

        private void RenderBody()
        {
            if (Settings?.Columns == null)
            {
                RenderDefaultBody();
                return;
            }
            //TODO add render with custom template and columns
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

            MainGrid.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = GridLength.Auto
            });

            foreach (var prop in tempProps.Select((value, i) => new { i, value }))
            {
                Border tempSPBorder = new Border()
                {
                    BorderThickness = new Thickness(0, 0, 1, 1),
                    BorderBrush = new SolidColorBrush(Colors.White)
                };
                StackPanel tempSP = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };
                tempSP.Children.Add(new TextBlock()
                {
                    Text = prop.value.Name,
                    Style = (Style)App.Current.Resources["CustomGridCellHeader"]
                });

                tempSPBorder.Child = tempSP;
                MainGrid.Children.Add(tempSPBorder);
                Grid.SetRow(tempSPBorder, 0);
                Grid.SetColumn(tempSPBorder, prop.i);
            }
        }

        private void RenderDefaultBody()
        {
            var tempProps = typeof(TModel).GetRuntimeProperties();

            foreach (var item in Items.Select((value, i) => new { i, value }))
            {
                MainGrid.RowDefinitions.Add(new RowDefinition()
                {
                    Height = GridLength.Auto
                });

                foreach (var prop in tempProps.Select((value, i) => new { i, value }))
                {
                    Border tempSPBorder = new Border()
                    {
                        BorderThickness = new Thickness(0, 0, 1, 1),
                        BorderBrush = new SolidColorBrush(Colors.White)
                    };
                    StackPanel tempSP = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal
                    };

                    tempSP.Children.Add(new TextBlock()
                    {
                        Text = prop.value.GetValue(item.value)?.ToString() ?? string.Empty,
                        Style = (Style)App.Current.Resources["CustomGridCell"]
                    });
                    tempSPBorder.Child = tempSP;
                    MainGrid.Children.Add(tempSPBorder);
                    Grid.SetColumn(tempSPBorder, prop.i);
                    Grid.SetRow(tempSPBorder, item.i + 1);
                }
                Border tempActionSPBorder = new Border()
                {
                    BorderThickness = new Thickness(0, 0, 1, 1),
                    BorderBrush = new SolidColorBrush(Colors.White)
                };
                StackPanel tempActionSP = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };
                CommandBar tempActionBar = new CommandBar();

                AppBarButton editButton = new AppBarButton()
                {
                    Content = new TextBlock()
                    {
                        Text = "Edit"
                    }
                };
                editButton.Click += EditButton_Click;

                AppBarButton deleteButton = new AppBarButton()
                {
                    Content = new TextBlock()
                    {
                        Text = "Delete"
                    }
                };
                deleteButton.Click += DeleteButton_Click;
                tempActionBar.PrimaryCommands.Add(editButton);
                tempActionBar.PrimaryCommands.Add(new AppBarSeparator());
                tempActionBar.PrimaryCommands.Add(deleteButton);

                tempActionSP.Children.Add(tempActionBar);
                tempActionSPBorder.Child = tempActionSP;
                
                MainGrid.Children.Add(tempActionSPBorder);
                Grid.SetColumn(tempActionSPBorder, tempProps.Count());
                Grid.SetRow(tempActionSPBorder, item.i + 1);
            }




        }

        private void UpdateData()
        {
            TotalCount = DbContext.Set<TModel>()
                .Count();

            Items = DbContext.Set<TModel>()
                .Skip(CurrentPage * CountPage)
                .Take(CountPage)
                .ToList();
        }
        
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            UIElement currentElement = (AppBarButton)sender;

            // Итерируемся по родительским элементам
            while (currentElement != null)
            {
                if (currentElement is Border)
                {
                    // Нашли Border, возвращаем его
                    break;
                }

                // Переходим к следующему родительскому элементу
                currentElement = VisualTreeHelper.GetParent(currentElement) as UIElement;
            }

            if (!MainGrid.Children.Contains(currentElement))
            {
                return;
            }

            var itemIndex = Grid.GetRow((Border)currentElement);
            var model = Items[itemIndex - 1];
            DbContext.Remove(model);
            DbContext.SaveChanges();
            MainGrid.Children.Clear();
            UpdateData();
            RenderHeader();
            RenderBody();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RightPageButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage += 1;
            if ((CurrentPage * CountPage) > TotalCount)
            {
                CurrentPage -= 1;
                RightButton.IsEnabled = false;
                return;
            }
            LeftButton.IsEnabled = true;
            MainGrid.Children.Clear();
            UpdateData();
            RenderHeader();
            RenderBody();
        }

        private void LeftPageButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage -= 1;
            if (CurrentPage < 0)
            {
                CurrentPage = 0;
                LeftButton.IsEnabled = false;
                return;
            }
            RightButton.IsEnabled = true;
            MainGrid.Children.Clear();
            UpdateData();
            RenderHeader();
            RenderBody();
        }

        private async void ImportCommand_Click(object sender, RoutedEventArgs e)
        {
            var tempStackPanel = new StackPanel();

            try
            {
                var openPicker = new FileOpenPicker();

                var window = ((App)(App.Current)).MainWindow;

                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

                WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

                openPicker.ViewMode = PickerViewMode.Thumbnail;
                openPicker.FileTypeFilter.Add("*");

                var file = await openPicker.PickSingleFileAsync();

                tempStackPanel.Orientation = Orientation.Vertical;
                tempStackPanel.Padding = new Thickness(10, 15, 10, 15);

                var tempProgressLabel = new TextBlock();
                tempProgressLabel.Text = $"Start import...";

                tempStackPanel.Children.Add(tempProgressLabel);

                var tempProgressBar = new ProgressBar()
                {
                    Maximum = 100,
                    Width = 450
                };

                tempStackPanel.Children.Add(tempProgressBar);

                MainContainer.Children.Insert(0, tempStackPanel);

                //var tempProgressDialog = new ContentDialog();
                //tempProgressDialog.XamlRoot = this.XamlRoot;
                //tempProgressDialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                //tempProgressDialog.Title = "Import progress";


                //tempProgressDialog.Content = tempStackPanel;

                //tempProgressDialog.ShowAsync();

                var models = await FileImporter.ImportModelsFromFileAsync<TModel>(file, (value) =>
                {
                    tempProgressBar.Value = value;
                    tempProgressLabel.Text = $"Progress: {value}%";
                    if (value == 100)
                    {
                        MainContainer.Children.Remove(tempStackPanel);
                    }
                });

                await DbContext.Set<TModel>()
                    .AddRangeAsync(models);

                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MainContainer.Children.Remove(tempStackPanel);
                ContentDialog dialog = new ContentDialog();
                dialog.XamlRoot = this.XamlRoot;
                dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                dialog.Title = "Exception";
                dialog.PrimaryButtonText = "Ok";
                dialog.DefaultButton = ContentDialogButton.Primary;
                dialog.Content = new TextBlock()
                {
                    Text = $"Error => {ex.Message}"
                };

                var result = await dialog.ShowAsync();

            }

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
