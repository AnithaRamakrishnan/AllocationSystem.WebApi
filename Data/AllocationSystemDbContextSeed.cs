namespace AllocationSystem.WebApi.Data
{
    public class AllocationSystemDbContextSeed
    {
        protected AllocationSystemDbContextSeed(){}
        public static async Task SeedAsync(AllocationSystemDbContext context, ILoggerFactory loggerFactory, int? retry = 0)
        {
            int retryForAvailability = retry.Value;

            try
            {
                await context.SaveChangesAsync();
            }
            catch
            {
                if (retryForAvailability < 10)
                {
                    retryForAvailability++;
                    await SeedAsync(context, loggerFactory, retryForAvailability);
                }
                throw;
            }
        }
    }
}
