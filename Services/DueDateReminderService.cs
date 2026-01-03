using eLibrary.Api.Data;
using eLibrary.Api.Services;
using Microsoft.EntityFrameworkCore;

public class DueDateReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DueDateReminderService> _logger;

    public DueDateReminderService(IServiceProvider serviceProvider, ILogger<DueDateReminderService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckDueDatesAsync();
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken); // runs every 24 hours
        }
    }

    private async Task CheckDueDatesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

        var soonDue = await context.Borrows
            .Include(b => b.User)
            .Include(b => b.Book)
            .Where(b => b.DueAt > DateTime.UtcNow &&
                        b.DueAt < DateTime.UtcNow.AddDays(2) && // 2 days left
                        b.ReturnedAt == null)
            .ToListAsync();

        foreach (var borrow in soonDue)
        {
            string subject = $"Reminder: {borrow.Book.Title} is due soon!";
            string body = $"Hello {borrow.User.FullName},\n\n" +
                          $"Your borrowed book \"{borrow.Book.Title}\" is due on {borrow.DueAt:MMMM dd}. " +
                          $"Please return it on time to avoid late fees.\n\n" +
                          "Thank you,\n eLibrary Team";

            await emailService.SendEmailAsync(borrow.User.Email, subject, body);
            _logger.LogInformation($"Reminder sent to {borrow.User.Email} for book {borrow.Book.Title}");
        }
    }
}
