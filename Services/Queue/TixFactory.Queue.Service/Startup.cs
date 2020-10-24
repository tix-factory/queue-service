using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TixFactory.ApplicationAuthorization;
using TixFactory.Configuration;
using TixFactory.Configuration.Client;
using TixFactory.Http.Client;
using TixFactory.Logging;
using TixFactory.Logging.Client;

namespace TixFactory.Queue.Service
{
	public class Startup : TixFactory.Http.Service.Startup
	{
		public const string ApiKeyHeaderName = "Tix-Factory-Api-Key";
		private const string _ApplicationApiKeyEnvironmentVariableName = "ApplicationApiKey";
		private readonly IQueueOperations _QueueOperations;
		private readonly IApiKeyParser _ApiKeyParser;
		private readonly IApplicationAuthorizationsAccessor _ApplicationAuthorizationsAccessor;

		public Startup()
			: base(CreateLogger())
		{
			var applicationKey = GetApplicationKey();
			var applicationAuthorizationUrl = new Uri("http://applicationauthorization.services.tixfactory.systems");
			var configurationServiceUrl = new Uri("http://applicationconfiguration.services.tixfactory.systems");

			var applicationSettingsProvider = new ApplicationSettingsValueSource(configurationServiceUrl, Logger, applicationKey);
			var settingsInitializer = new SettingsInitializer(applicationSettingsProvider);
			var settings = settingsInitializer.CreateFromInterface<ISettings>();

			_QueueOperations = new QueueOperations(Logger, settings);
			_ApiKeyParser = new ApiKeyHeaderParser(ApiKeyHeaderName);
			_ApplicationAuthorizationsAccessor = new ApplicationAuthorizationsAccessor(Logger, applicationAuthorizationUrl, applicationKey);
		}

		public override void ConfigureServices(IServiceCollection services)
		{
			services.AddTransient(s => _QueueOperations);

			base.ConfigureServices(services);
		}

		protected override void ConfigureMvc(MvcOptions options)
		{
			options.Filters.Add(new ValidateApiKeyAttribute(_ApiKeyParser, _ApplicationAuthorizationsAccessor));

			base.ConfigureMvc(options);
		}

		private static ILogger CreateLogger()
		{
			var httpClient = new HttpClient();
			var consoleLogger = new ConsoleLogger();
			return new NetworkLogger(httpClient, consoleLogger, "TFQS1.TixFactory.Queue.Service", "monitoring.tixfactory.systems");
		}

		private ISetting<Guid> GetApplicationKey()
		{
			var rawApiKey = Environment.GetEnvironmentVariable(_ApplicationApiKeyEnvironmentVariableName);
			if (!Guid.TryParse(rawApiKey, out var apiKey))
			{
				Logger.Warn($"\"{_ApplicationApiKeyEnvironmentVariableName}\" (environment variable) could not be parsed to Guid. Application will likely fail all authorizations.\n\tValue: \"{rawApiKey}\"");
			}

			return new Setting<Guid>(apiKey);
		}
	}
}
