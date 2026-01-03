using eLibrary.Api.Data;
using eLibrary.Api.Services;
using eLibrary.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---------------------- SERVICES ----------------------
builder.Services.AddControllers();
builder.Services.AddScoped<EmailService>();
builder.Services.AddHostedService<DueDateReminderService>();
// ✅ Register HttpClient so OnlineSearchController can call external APIs
builder.Services.AddHttpClient();
// Database
//builder.Services.AddDbContext<AppDbContext>(options =>
  //  options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// ✅ JWT Authentication & Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>

{
options.TokenValidationParameters = new TokenValidationParameters
{
ValidateIssuer = true,
ValidateAudience = true,
ValidateLifetime = true,
ValidateIssuerSigningKey = true,
ValidIssuer = builder.Configuration["Jwt:Issuer"],
ValidAudience = builder.Configuration["Jwt:Audience"],
IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
    )
};
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

// ✅ Swagger with JWT Bearer Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
c.SwaggerDoc("v1", new OpenApiInfo { Title = "eLibrary API", Version = "v1" });

c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
In = ParameterLocation.Header,
Description = "Enter 'Bearer {token}'",
Name = "Authorization",
Type = SecuritySchemeType.Http,
Scheme = "Bearer",
BearerFormat = "JWT"
});

c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
options.AddPolicy("AllowFrontend", policy =>
{
policy.WithOrigins("http://localhost:3000")
      .AllowAnyHeader()
      .AllowAnyMethod();
});
});

// ---------------------- BUILD APP ----------------------
var app = builder.Build();

// ---------------------- MIDDLEWARE ----------------------
if (app.Environment.IsDevelopment())
{
app.UseSwagger();
app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

app.UseAuthentication();   
app.UseAuthorization();

app.MapControllers();

// ---------------------- SEEDING ----------------------
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (context.Users.Count() == 0)
    {
        context.Users.Add(new User
        {
            Username = "ahmadd",
            Email = "ahmads@example.com",
            FullName = "ahmad yousef",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("ahmad12345!-"),
            Role = "Admin",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        context.SaveChanges();
    }

    if (!context.Genres.Any())
    {
        var genres = new[]
        {
            new Genre { Name = "Fiction" },
            new Genre { Name = "Non-Fiction" },
            new Genre { Name = "Science" },
            new Genre { Name = "History" },
            new Genre { Name = "Mystery" }
        };
        context.Genres.AddRange(genres);
        context.SaveChanges();
    }

    if (!context.Books.Any())
    {
        var fiction = context.Genres.First(g => g.Name == "Fiction").Id;
        var nonfiction = context.Genres.First(g => g.Name == "Non-Fiction").Id;
        var science = context.Genres.First(g => g.Name == "Science").Id;
        var history = context.Genres.First(g => g.Name == "History").Id;
        var mystery = context.Genres.First(g => g.Name == "Mystery").Id;

        var books = new[]
        {
            new Book { Title="Clean Code", Author="Robert C. Martin", GenreId=science, TotalCopies=5, AvailableCopies=5 },
            new Book { Title="The Pragmatic Programmer", Author="Andrew Hunt", GenreId=science, TotalCopies=3, AvailableCopies=3 },
            new Book { Title="Sapiens", Author="Yuval Noah Harari", GenreId=nonfiction, TotalCopies=4, AvailableCopies=4 },
            new Book { Title="1984", Author="George Orwell", GenreId=fiction, TotalCopies=6, AvailableCopies=6 },
            new Book { Title="The Da Vinci Code", Author="Dan Brown", GenreId=mystery, TotalCopies=2, AvailableCopies=2 },
            new Book { Title="Brave New World", Author="Aldous Huxley", GenreId=fiction, TotalCopies=5, AvailableCopies=5 },
            new Book { Title="The Great Gatsby", Author="F. Scott Fitzgerald", GenreId=fiction, TotalCopies=6, AvailableCopies=6 },
            new Book { Title="Fahrenheit 451", Author="Ray Bradbury", GenreId=fiction, TotalCopies=5, AvailableCopies=5 },
            new Book { Title="To Kill a Mockingbird", Author="Harper Lee", GenreId=fiction, TotalCopies=5, AvailableCopies=5 },
            new Book { Title="The Catcher in the Rye", Author="J.D. Salinger", GenreId=fiction, TotalCopies=5, AvailableCopies=5 },
            new Book { Title="Pride and Prejudice", Author="Jane Austen", GenreId=fiction, TotalCopies=5, AvailableCopies=5 },
            new Book { Title="War and Peace", Author="Leo Tolstoy", GenreId=fiction, TotalCopies=3, AvailableCopies=3 },
            new Book { Title="The Hobbit", Author="J.R.R. Tolkien", GenreId=fiction, TotalCopies=6, AvailableCopies=6 },
            new Book { Title="The Lord of the Rings", Author="J.R.R. Tolkien", GenreId=fiction, TotalCopies=5, AvailableCopies=5 },
            new Book { Title="The Martian", Author="Andy Weir", GenreId=science, TotalCopies=5, AvailableCopies=5 },
            new Book { Title="A Brief History of Time", Author="Stephen Hawking", GenreId=science, TotalCopies=5, AvailableCopies=5 },
            new Book { Title="Cosmos", Author="Carl Sagan", GenreId=science, TotalCopies=5, AvailableCopies=5 },
            new Book { Title="The Selfish Gene", Author="Richard Dawkins", GenreId=science, TotalCopies=4, AvailableCopies=4 },
            new Book { Title="Homo Deus", Author="Yuval Noah Harari", GenreId=nonfiction, TotalCopies=5, AvailableCopies=5 },
            new Book { Title="21 Lessons for the 21st Century", Author="Yuval Noah Harari", GenreId=nonfiction, TotalCopies=5, AvailableCopies=5 },
            new Book { Title="Guns, Germs, and Steel", Author="Jared Diamond", GenreId=history, TotalCopies=4, AvailableCopies=4 },
            new Book { Title="The Silk Roads", Author="Peter Frankopan", GenreId=history, TotalCopies=4, AvailableCopies=4 },
            new Book { Title="The Wright Brothers", Author="David McCullough", GenreId=history, TotalCopies=3, AvailableCopies=3 },
            new Book { Title="Team of Rivals", Author="Doris Kearns Goodwin", GenreId=history, TotalCopies=3, AvailableCopies=3 },
            new Book { Title="The Hound of the Baskervilles", Author="Arthur Conan Doyle", GenreId=mystery, TotalCopies=5, AvailableCopies=5 },
            new Book { Title="Murder on the Orient Express", Author="Agatha Christie", GenreId=mystery, TotalCopies=5, AvailableCopies=5 },
            new Book { Title="The Girl with the Dragon Tattoo", Author="Stieg Larsson", GenreId=mystery, TotalCopies=5, AvailableCopies=5 },
            new Book { Title="Gone Girl", Author="Gillian Flynn", GenreId=mystery, TotalCopies=5, AvailableCopies=5 },
            new Book { Title="Big Little Lies", Author="Liane Moriarty", GenreId=mystery, TotalCopies=5, AvailableCopies=5 }
        };

        context.Books.AddRange(books);
        context.SaveChanges();
    }
}


app.Run();
