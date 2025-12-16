using System;
using System.Configuration;

namespace DescargadorComprobantes
{
    public static class Configuracion
    {
        public static string ApiBaseUrl => ConfigurationManager.AppSettings["ApiBaseUrl"];
        public static string ClientId => ConfigurationManager.AppSettings["ClientId"];
        public static string ClientSecret => ConfigurationManager.AppSettings["ClientSecret"];
        public static string SqlServer => ConfigurationManager.AppSettings["SqlServer"];
        public static string SqlDatabase => ConfigurationManager.AppSettings["SqlDatabase"];
        public static string SqlUsername => ConfigurationManager.AppSettings["SqlUsername"];
        public static string SqlPassword => ConfigurationManager.AppSettings["SqlPassword"];
        public static int DelayBetweenRequests => int.Parse(ConfigurationManager.AppSettings["DelayBetweenRequests"]);
        public static int MaxRetryAttempts => int.Parse(ConfigurationManager.AppSettings["MaxRetryAttempts"]);

        public static string ConnectionString
        {
            get
            {
                return string.Format("Server={0};Database={1};User Id={2};Password={3};Trusted_Connection=false;Encrypt=false;TrustServerCertificate=true;",
                    SqlServer, SqlDatabase, SqlUsername, SqlPassword);
            }
        }
    }
}