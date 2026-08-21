
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

            app.MapGet("/", () => "Sensor -X API Running");

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
