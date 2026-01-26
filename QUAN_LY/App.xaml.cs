using Microsoft.Extensions.DependencyInjection;
using QUAN_LY.Interfaces;
using QUAN_LY.Model;
using QUAN_LY.Services;
using QUAN_LY.View;
using QUAN_LY.ViewModel;
using System;
using System.Windows;

namespace QUAN_LY
{
    public partial class App : Application
    {
        public static new App Current => (App)Application.Current;
        public static Admin CurrentUser { get; set; }
        public IServiceProvider Services { get; }

        public App()
        {
            Services = ConfigureServices();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            
            services.AddDbContext<BookStoreDbContext>();

            
            services.AddSingleton<IAdminService, AdminServiceSQL>();
            
            services.AddTransient<LoginViewModel>();
            services.AddTransient<AdminViewModel>();
            services.AddTransient<PageModel>(); 

           
            services.AddTransient<LoginWindowView>();
            services.AddTransient<MainWindowView>();

            return services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

           
            var loginWindow = Services.GetRequiredService<LoginWindowView>();
            
            loginWindow.DataContext = Services.GetRequiredService<LoginViewModel>();
            loginWindow.Show();
        }

        public static class Roles
        {
            public const string SuperAdmin = "Super Admin";
            public const string Manager = "Manager";
            public const string SaleStaff = "Sale Staff";
        }
    }
}