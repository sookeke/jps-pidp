namespace EdtService.HttpClients.Mail;

public interface ISmtpEmailClient
{
    Task SendAsync(Email email);
}
