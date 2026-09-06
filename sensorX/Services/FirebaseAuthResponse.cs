using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace sensorX.Services
{
    // The raw data Firebase returns after a successful sign-in OR sign-up call.
    public class FirebaseAuthResponse
    {
        [JsonPropertyName("idToken")]
        public string IdToken { get; set; } = "";

        [JsonPropertyName("email")]
        public string Email { get; set; } = "";

        [JsonPropertyName("localId")]
        public string LocalId { get; set; } = "";

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = "";
    }
}