using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System;
using System.Reflection;
using Jadyn.Common.Models;
using System.Linq;
using Windows.UI;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Data;

namespace Jadyn.Client.Components
{
    public class CustomEditForm<TModel> : UserControl
    {
        public TModel Model { get; set; }

        private StackPanel MainContainer { get; set; }
        public Action SubmitCallBack { get; set; }
        public Action CancelCallBack { get; set; }

        public CustomEditForm(TModel model)
        {
            Model = model;
            this.DataContext = Model;
        }

        public void Render()
        {
            MainContainer = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Padding = new Thickness(10, 15, 10, 15)
            };


            this.Content = MainContainer;

            var props = typeof(TModel).GetRuntimeProperties();

            foreach (var prop in props)
            {
                if (!UpdateFormsDictionary.ContainsKey(prop.PropertyType))
                {
                    continue;
                }
                Border tempSPBorder = new Border()
                {
                    BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.White),
                    BorderThickness = new Thickness(0, 0, 0, 1)
                };

                StackPanel tempSP = new StackPanel()
                {
                    Padding = new Thickness(5)
                };

                tempSP.Children.Add(new TextBlock()
                {
                    Text = prop.Name,
                    Margin = new Thickness(0, 0, 0, 5)
                });

                var tempForm = UpdateFormsDictionary[prop.PropertyType].Invoke(prop.GetValue(Model), prop.Name, Model);
                tempSP.Children.Add(tempForm);
                tempSPBorder.Child = tempSP;

                MainContainer.Children.Add(tempSPBorder);
            }

            StackPanel tempSubmitSP = new StackPanel()
            {
                Padding = new Thickness(5),
                Orientation = Orientation.Horizontal
            };

            var submitButton = new Button()
            {
                Content = "Submit",
            };
            submitButton.Click += (s, e) =>
            {
                SubmitCallBack.Invoke();
            };

            tempSubmitSP.Children.Add(submitButton);

            var cancelButton = new Button()
            {
                Content = "Cancel",
            };
            cancelButton.Click += (s, e) =>
            {
                CancelCallBack.Invoke();
            };

            tempSubmitSP.Children.Add(cancelButton);
            MainContainer.Children.Add(tempSubmitSP);
        }


        private Dictionary<Type, Func<object?, string, TModel, UIElement>> UpdateFormsDictionary = new Dictionary<Type, Func<object?, string, TModel, UIElement>>()
        {
            {
                typeof(int),
                (value, name, model) =>
                {
                    var tempInput = new NumberBox()
                    {
                        Value = (int)(value ?? default(int)),
                        SmallChange = 1,
                        Width = 250,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    };
                    var tempBinding = new Microsoft.UI.Xaml.Data.Binding();
                    tempBinding.Source = model;
                    tempBinding.Mode = BindingMode.TwoWay;
                    tempBinding.Path = new PropertyPath(name); 
                    tempInput.SetBinding(NumberBox.ValueProperty, tempBinding);
                    return tempInput;
                }
            },
            {
                typeof(long),
                (value, name, model) =>
                {
                    var tempInput = new NumberBox()
                    {
                        Value = (long)(value ?? default(long)),
                        Width = 250,
                        SmallChange = 1,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    };
                    var tempBinding = new Microsoft.UI.Xaml.Data.Binding();
                    tempBinding.Source = model;
                    tempBinding.Mode = BindingMode.TwoWay;
                    tempBinding.Path = new PropertyPath(name);
                    tempInput.SetBinding(NumberBox.ValueProperty, tempBinding);
                    return tempInput;
                }
            },
            {
                typeof(string),
                (value, name, model) =>
                {
                    var tempInput = new TextBox()
                    {
                        Text = (string)(value ?? string.Empty),
                        Width = 250,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    };
                    var tempBinding = new Microsoft.UI.Xaml.Data.Binding();
                    tempBinding.Source = model;
                    tempBinding.Mode = BindingMode.TwoWay;
                    tempBinding.Path = new PropertyPath(name);
                    tempInput.SetBinding(TextBox.TextProperty, tempBinding);
                    return tempInput;
                }
            },
            //{
            //    typeof(Gender),
            //    (value) =>
            //    {
            //        var items = Enum.GetValues(typeof(Gender)).Cast<Gender>();
            //        return new ComboBox()
            //        {
            //            SelectedValue = (Gender)value,
            //            ItemsSource = items
            //        };
            //    }
            //}
        };
    }
}
