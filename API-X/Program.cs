namespace API_X
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            var app = builder.Build();
            app.UseSwagger();
            app.UseSwaggerUI();

            // Basic root route just to confirm the API is up if visited in a browser
            app.MapGet("/", () => "Sensor -X API Running");

            // Existing scanner status endpoint,,used by the WPF app to check "is API online"
            app.MapGet("/Scanner", () =>
            {
                return Results.Ok(new
                {
                    Status = "Online",
                    Time = DateTime.Now
                });
            });

            app.Run();
        }
    }
}