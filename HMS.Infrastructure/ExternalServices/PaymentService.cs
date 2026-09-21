using HMS.Core.Contracts;
using HMS.Core.Entities.BookingModule;
using HMS.Core.Entities.Enums.BookingEnums;
using HMS.Services.Abstraction;
using HMS.Shared.Messages;
using HMS.Shared.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace HMS.Infrastructure.ExternalServices
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymentService> _logger;
        private readonly IEmailService _emailService;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            HttpClient httpClient,
            ILogger<PaymentService> logger,
            IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
            _emailService = emailService;
        }
        public async Task<GenericResponse<string>> ProcessPaymentAsync(Guid bookingId)
        {
            var bookingRepo = _unitOfWork.GetRepository<BookingEntity, Guid>();

            var booking = await bookingRepo.GetByIdAsync(bookingId, b => b.User);

            if (booking is null)
                return GenericResponse<string>.Error($"Booking with id: {bookingId} was not found.", StatusCodes.Status404NotFound);


            var (orderId, ClientSecret) = await CreateIntentionAsync(booking);

            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(ClientSecret))
                return GenericResponse<string>.Failure($"Failed to create order id or client secret.");

            var publicKey = _configuration["Paymob:PublicKey"];

            if (publicKey is null)
                return GenericResponse<string>.Failure("Failed to catch public key.");

            var paymentUrl = $"https://eg.checkout.paymob.com/?publicKey={publicKey}&clientSecret={ClientSecret}";

            try
            {
                booking.Status = BookingStatus.Paid;
                booking.PaymobOrderId = orderId;
                booking.PaymobPaymentKey = ClientSecret;
                booking.PaidDate = DateTime.UtcNow;

                bookingRepo.Update(booking);
                booking.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                var emailToSent = new Email()
                {
                    Message = "Your booking is paid, We hope you enjoy, Thank you for choosing us.",
                    Subject = "Booking Confirmation",
                    To = booking.User.Email!
                };

                await _emailService.SendEmailAsync(emailToSent);

                return GenericResponse<string>.Success(paymentUrl, "Payment url is ready");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception happened while creating payment url.");

                return GenericResponse<string>.Failure("Unhandled exception happened while creating payment url.");
            }
        }

        #region Helper Methods


        private async Task<(string? orderId, string? ClientSecret)> CreateIntentionAsync(BookingEntity booking)
        {
            var secretKey = _configuration["Paymob:SecretKey"];
            var baseUrl = _configuration["Paymob:BaseUrl"];
            var integrationId = int.Parse(_configuration["Paymob:IntegrationId"]!);

            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/intention/");
            request.Headers.Add("Authorization", $"Token {secretKey}");

            var content = new
            {
                amount = (long)(booking.TotalAmount * 100),
                currency = booking.Currency,
                payment_methods = new List<int>() { integrationId },
                items = Array.Empty<string>(),
                billing_data = new
                {
                    first_name = booking.User.FullName.Split(" ")[0],
                    last_name = booking.User.FullName.Split(" ")[1],
                    email = booking.User.Email,
                }

            };


            var stringContent = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");
            request.Content = stringContent;
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Paymob Intention Error: {Error}", error);
                return (null, null);
            }


            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();

            var clientSecret = jsonResponse.GetProperty("client_secret").ToString();
            var orderId = jsonResponse.GetProperty("id").ToString();

            return (orderId, clientSecret);
        }

        #endregion
    }
}
