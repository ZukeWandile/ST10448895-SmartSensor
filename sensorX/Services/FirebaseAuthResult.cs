using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sensorX.Services
{
    // Represents the result of a Firebase authentication attempt.
    public class FirebaseAuthResult
    {
        // Gets a value indicating whether the authentication was successful.
        public bool IsSuccess { get; private set; }

        // Gets the Firebase ID token for the authenticated user, if successful
        public string? IdToken { get; private set; }

        // Gets the refresh token used to obtain a new ID token, if successful.
        public string? RefreshToken { get; private set; }

        // Gets the unique user ID (UID) assigned by Firebase, if successful.
        public string? LocalId { get; private set; }

        // Gets the user's email address, if successful.
        public string? Email { get; private set; }

        // Gets the error message if the authentication failed.
        public string? ErrorMessage { get; private set; }

        // Creates a successful result instance using data from the Firebase response.
        public static FirebaseAuthResult Succeeded(FirebaseAuthResponse response) =>
            new FirebaseAuthResult
            {
                IsSuccess = true,
                IdToken = response.IdToken,
                RefreshToken = response.RefreshToken,
                LocalId = response.LocalId,
                Email = response.Email
            };

        // Creates a failed result instance with the specified error message.
        public static FirebaseAuthResult Failed(string errorMessage) =>
            new FirebaseAuthResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
    }
}