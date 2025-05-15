using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Infrastructure.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var projectDir = Directory.GetCurrentDirectory();
            var parentDir = Directory.GetParent(projectDir);
            if (parentDir == null)
            {
                throw new DirectoryNotFoundException("Could not find parent directory");
            }

            var logWorkerDir = Path.Combine(parentDir.FullName, "LogWorker");
            if (!Directory.Exists(logWorkerDir))
            {
                throw new DirectoryNotFoundException($"LogWorker directory not found at {logWorkerDir}");
            }

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(logWorkerDir)
                .AddJsonFile("appsettings.Development.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.Development.json");
            }

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseNpgsql(connectionString);

            return new ApplicationDbContext(builder.Options);
        }
    }
}
