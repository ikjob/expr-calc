using ExprCalc.RestApi.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

namespace ExprCalc.RestApi
{
    public static class RestAPIRegistrationExtensions
    {
        public static void AddRestApiServices(this IServiceCollection serviceCollection, IConfiguration configurationManager)
        {
            var config = new RestApiConfig();
            configurationManager.Bind(RestApiConfig.ConfigurationSectionName, config);

            if (config.CorsAllowAny)
            {
                serviceCollection.AddCors(options =>
                {
                    options.AddDefaultPolicy(policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
                });
            }

            serviceCollection.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                })
                .AddApplicationPart(typeof(RestAPIRegistrationExtensions).Assembly);

            if (config.UseSwagger)
            {
                serviceCollection.AddEndpointsApiExplorer();
                serviceCollection.AddSwaggerGen();
            }
        }


        public static void UseRestApiFeatures(this WebApplication app)
        {
            var config = new RestApiConfig();
            app.Configuration.Bind(RestApiConfig.ConfigurationSectionName, config);

            // Configure the HTTP request pipeline.
            if (config.UseSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseDefaultFiles();
            //app.UseStaticFiles();
            app.UseRouting();

            // Enable Cors
            if (config.CorsAllowAny)
                app.UseCors();

            app.UseAuthorization();

            app.MapControllers();
        }
    }
}
