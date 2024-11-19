using lexicon_Garage2.Data;

namespace lexicon_Garage2.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static async Task<IApplicationBuilder> SeedDataAsync(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<lexicon_Garage2Context>();

                try
                {
                    await SeedData.Init(context, services);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return app;
        }
    }
}
