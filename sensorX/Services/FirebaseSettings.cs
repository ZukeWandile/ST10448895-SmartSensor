using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sensorX.Services
{

    // Holds configuration settings required to connect and interact with Firebase Authentication.
    public class FirebaseSettings
    {

        // Gets or sets the Web API key provided by Firebase for your project.
        public string ApiKey { get; set; } = string.Empty;

        // Gets or sets the base URL for the Firebase Identity Toolkit REST API.
        public string BaseUrl { get; set; } = "https://identitytoolkit.googleapis.com/v1/accounts:";
        // Gets or sets the unique identifier (ID) of the Firebase project.
        public string ProjectId { get; set; } = string.Empty;

        // Gets or sets the domain used for authentication flows 
        public string AuthDomain { get; set; } = string.Empty;
    }
}