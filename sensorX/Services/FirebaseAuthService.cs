using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace sensorX.Services
{
 
    // Service responsible for handling Firebase Authentication operations via REST API.
    public class FirebaseAuthService
    {
        private readonly FirebaseSettings _settings;

        // Initializes a new instance of the <see cref="FirebaseAuthService"/> class.
        public FirebaseAuthService(FirebaseSettings settings)
        {
            _settings = settings;
        }
        // Reusable HttpClient instance for sending requests to Firebase
        private readonly HttpClient _http = new();


        // Signs in an existing user with their email and password.
        public Task<FirebaseAuthResult> SignInAsync(string email, string password) => AuthenticateAsync("signInWithPassword", email, password);

        // Registers a new user account with an email and password
        public Task<FirebaseAuthResult> SignUpAsync(string email, string password) => AuthenticateAsync("signUp", email, password);

        // Core helper method to handle authentication requests (sign-in or sign-up) with Firebase.
        private async Task<FirebaseAuthResult> AuthenticateAsync(string endpoint, string email, string password)
        {
            var url = $"{_settings.BaseUrl}{endpoint}?key={_settings.ApiKey}";

            var payload = new { email, password, returnSecureToken = true };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                response = await _http.PostAsync(url, content);
            }
            catch (HttpRequestException ex)
            {
                return FirebaseAuthResult.Failed($"Network error: {ex.Message}");
            }

            var body = await response.Content.ReadAsStringAsync();

            // If successful, deserialize the response and return a success result
            if (response.IsSuccessStatusCode)
            {
                var success = JsonSerializer.Deserialize<FirebaseAuthResponse>(body);
                return FirebaseAuthResult.Succeeded(success!);
            }

            // If failed, extract and translate the error code into a user-friendly message
            return FirebaseAuthResult.Failed(TranslateError(ExtractErrorCode(body)));
        }
        // Sends a password reset email to the specified address.
        public async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            var url = $"{_settings.BaseUrl}sendOobCode?key={_settings.ApiKey}";

            var payload = new
            {
                requestType = "PASSWORD_RESET",
                email
            };

            var json = JsonSerializer.Serialize(payload);

            var response = await _http.PostAsync(url,new StringContent(json, Encoding.UTF8, "application/json") );

            return response.IsSuccessStatusCode;
        }
        // Sends an email verification link to the user using their ID token
        public async Task<bool> SendVerificationEmailAsync(string idToken)
        {
            var url = $"{_settings.BaseUrl}sendOobCode?key={_settings.ApiKey}";
            var payload = new { requestType = "VERIFY_EMAIL", idToken };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(url, content);
            return response.IsSuccessStatusCode;
        }

        // Parses the Firebase error JSON response to extract the specific error message
        private static string ExtractErrorCode(string body)
        {
            try
            {
                var errorDoc = JsonSerializer.Deserialize<FirebaseErrorEnvelope>(body);
                return errorDoc?.Error?.Message ?? "UNKNOWN_ERROR";
            }
            catch (JsonException)
            {
                return "UNKNOWN_ERROR";
            }
        }
        // Maps raw Firebase error strings/codes to clean, user-friendly messages
        private static string TranslateError(string code)
        {
            if (code.StartsWith("WEAK_PASSWORD"))
                return "Password should be at least 6 characters.";

            switch (code)
            {
                case "EMAIL_NOT_FOUND":
                    return "No account found with that email address.";
                case "INVALID_PASSWORD":
                case "INVALID_LOGIN_CREDENTIALS":
                    return "Incorrect email or password.";
                case "USER_DISABLED":
                    return "This account has been disabled.";
                case "TOO_MANY_ATTEMPTS_TRY_LATER":
                    return "Too many failed attempts. Please try again later.";
                case "EMAIL_EXISTS":
                    return "An account with that email already exists.";
                case "OPERATION_NOT_ALLOWED":
                    return "Email/password sign-in is not enabled for this project.";
                default:
                    return "Request failed. Please try again.";
            }
        }

        // Helper model representing the root structure of a Firebase error response
        private class FirebaseErrorEnvelope
        {
            [JsonPropertyName("error")]
            public FirebaseErrorDetail? Error { get; set; }
        }

        // Helper model representing the error details within the Firebase error response
        private class FirebaseErrorDetail
        {
            [JsonPropertyName("message")]
            public string Message { get; set; } = "";
        }
    }
}