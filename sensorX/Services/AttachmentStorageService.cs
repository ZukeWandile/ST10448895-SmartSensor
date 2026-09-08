using System;
using System.IO;
using sensorX.Models;

namespace sensorX.Services
{
    // Service responsible for saving and managing sensor file attachments locally
    public class AttachmentStorageService
    {
        private readonly string _rootFolder;

        // Constructor sets up the base directory where attachments will be stored
        public AttachmentStorageService()
        {
            // Set the root folder to the user's Local Application Data folder under "SensorX/Attachments"
            _rootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SensorX", "Attachments");

            // Ensure the root directory exists
            Directory.CreateDirectory(_rootFolder);
        }

        // Saves a file attachment associated with a specific sensor (identified by its MAC address)
        public SensorAttachment SaveAttachment(string sourceFilePath, string macAddress)
        {
            // Format the MAC address to be safe for folder naming (replace colons with dashes)
            var safeKey = macAddress.Replace(":", "-");

            // Create a dedicated folder for this specific sensor
            var sensorFolder = Path.Combine(_rootFolder, safeKey);
            Directory.CreateDirectory(sensorFolder);

            // Get the original file name and generate a unique file name to prevent naming collisions
            var fileName = Path.GetFileName(sourceFilePath);
            var uniqueName = $"{Guid.NewGuid():N}_{fileName}";
            var destPath = Path.Combine(sensorFolder, uniqueName);

            // Copy the file from the source path to the new destination path
            File.Copy(sourceFilePath, destPath, overwrite: false);

            // Return a metadata object representing the saved attachment
            return new SensorAttachment
            {
                FileName = fileName,
                StoredPath = destPath,
                FileType = Path.GetExtension(fileName).TrimStart('.').ToUpperInvariant(), // like "TXT" or "JPG"
                UploadedAt = DateTime.Now
            };
        }
    }
}