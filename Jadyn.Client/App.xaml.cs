using Jadyn.Client.Windows.Utils;
using Jadyn.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Jadyn.Client
{
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }
        public Window MainWindow;

        public App()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var services = new ServiceCollection();

            services.AddDbContext<JadynDbContext>(options =>
            {
                options.UseSqlite($"Data Source = {System.IO.Path.Combine(AppContext.BaseDirectory, "jadyn.db")}");
            });
            services.AddTransient<IFileImporter, FileImporter>();
            ServiceProvider = services.BuildServiceProvider();

            MainWindow = new MainWindow();
            MainWindow.Activate();
        }
    }
}
