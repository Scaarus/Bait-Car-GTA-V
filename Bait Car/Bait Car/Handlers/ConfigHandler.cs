using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Linq;

namespace Bait_Car.Handlers
{
    /// <summary>
    /// Responsible for loading the options used by the plugin.
    /// </summary>
    public class ConfigHandler
    {
        private const string FilePath = @"Plugins\BaitCar.ini";

        private const string DefaultFile =
            @"// Set any keybind to 'None' to disable that key

[Keys]

// The main key that opens the menu
// Default: F7
OpenMenu=F7

// Shut down the bait car's engine.
// Pressing a second time turns the engine back on.
// Default: K
KillSwitch=K

// Lock the bait car's doors.
// Pressing a second time unlocks the doors.
// Default: L
LockDoors=L

[Controller]

// Valid controller buttons: A, B, X, Y, DPadUp, DPadDown, DPadLeft, DPadRight, Back, Start, RightShoulder, RightThumb, LeftShoulder, LeftThumb, None

// The main key that opens the menu
// Default: None
OpenMenu=None

// Shut down the bait car's engine.
// Pressing a second time turns the engine back on.
// Default: None
KillSwitch=None

// Lock the bait car's doors.
// Pressing a second time unlocks the doors.
// Default: None
LockDoors=None

[Options]

// Minimum time in seconds before someone can take the bait car.
// Default: 30
MinSecondsToWait=30

// Maxiumum time in seconds before someone can take the bait car.
// Default: 120
MaxSecondsToWait=120

// The maximum distance to find a ped who can steal the car.
// A value too small leads to unstolen cars. Too large leads to lag.
// Default: 100
MaxSearchRadius=100

// Set this to true if you want to disable helpful tool tips
// Also disables notification telling you when you are far enough away
// Default: False
Hardcore=False

// Debug mode
// Only enable at developer request
// Default: False
Debug=False";

        private readonly Dictionary<string, string> Options = new Dictionary<string, string>();

        public ConfigHandler()
        {
            LogHandler.Log("Starting config handler.", LogType.Debug);

            // Create the config file if it doesn't exist
            if (!DoesConfigExist())
            {
                LogHandler.Log("Config file does not exist.", LogType.Debug);
                CreateDefaultFile();
            }

            LogHandler.Log("Found config file. Loading options.", LogType.Debug);

            if (!LoadOptions())
            {
                LogHandler.Log("Unable to load options, using defaults.");
            }
            LogHandler.Log("Options loaded!", LogType.Debug);
        }

        /// <summary>
        /// Checks if the config file exists.
        /// </summary>
        /// <returns>True if the file exists.</returns>
        private static bool DoesConfigExist()
        {
            return File.Exists(Path.Combine(Directory.GetCurrentDirectory(),FilePath));
        }

        /// <summary>
        /// Create the default file at FilePath.
        /// Will Overwrite any existing file with that name.
        /// </summary>
        /// <returns>True if the file was created.</returns>
        private static void CreateDefaultFile()
        {
            try
            {
                LogHandler.Log("Creating config file.", LogType.Debug);
                using (var file = new StreamWriter(FilePath, false))
                {
                    file.Write(DefaultFile);
                }
                LogHandler.Log("Config file created successfully.", LogType.Debug);
            }
            catch (Exception e)
            {
                LogHandler.Log("Unable to create default file.", LogType.Error);
                LogHandler.Log(e.Message, LogType.Debug);
            }
        }

        /// <summary>
        /// Load all of the options and store them in the dictionary.
        /// </summary>
        /// <returns>True if the options loaded.</returns>
        private bool LoadOptions()
        {
            Options.Clear();
            try
            {
                using (var sr = new StreamReader(FilePath))
                {
                    string line, section = "";
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();

                        // Skip empty lines
                        if (line.Length == 0) continue;

                        // Skip commented lines
                        if (line.StartsWith("/")) continue;

                        // Found a section
                        if (line.StartsWith("[") && line.Contains("]"))
                        {
                            var index = line.IndexOf(']');
                            section = line.Substring(1, index - 1).Trim();
                            continue;
                        }

                        if (!line.Contains("=")) continue;
                        {
                            var index = line.IndexOf('=');
                            var key = line.Substring(0, index).Trim();
                            var value = line.Substring(index + 1).Trim();

                            Options.Add($"{section}.{key}".ToLower(), value);
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                LogHandler.Log("Unable to load the config file.", LogType.Error);
                LogHandler.Log(e.Message, LogType.Debug);
            }
            return false;
        }

        /// <summary>
        /// Tries to load the key requested.
        /// </summary>
        /// <param name="section">The section the key is part of.</param>
        /// <param name="key">The key to load.</param>
        /// <param name="value">The value of the key.</param>
        /// <returns>True if the key was found.</returns>
        private bool TryGetValue(string section, string key, out string value)
        {
            return Options.TryGetValue($"{section}.{key}".ToLower(), out value);
        }
        
        /// <summary>
        /// Loads a string option.
        /// </summary>
        /// <param name="section">The section the key is part of.</param>
        /// <param name="key">The key to load.</param>
        /// <param name="defaultValue">The value to use if none is found.</param>
        /// <returns>The value for the given key.</returns>
        public string GetValue(string section, string key, string defaultValue = "")
        {
            return !TryGetValue(section, key, out var value) ? defaultValue : value;
        }

        /// <summary>
        /// Loads an integer option.
        /// </summary>
        /// <param name="section">The section the key is part of.</param>
        /// <param name="key">The key to load.</param>
        /// <param name="defaultValue">The value to use if none is found.</param>
        /// <returns>The value for the given key.</returns>
        public int GetInteger(string section, string key, int defaultValue = 0)
        {
            if (!TryGetValue(section, key, out var stringValue))
                return defaultValue;

            return !int.TryParse(stringValue, out var value) ? defaultValue : value;
        }

        /// <summary>
        /// Loads a boolean option.
        /// </summary>
        /// <param name="section">The section the key is part of.</param>
        /// <param name="key">The key to load.</param>
        /// <param name="defaultValue">The value to use if none is found.</param>
        /// <returns>The value for the given key.</returns>
        public bool GetBoolean(string section, string key, bool defaultValue = false)
        {
            if (!TryGetValue(section, key, out var stringValue))
                return defaultValue;

            return (stringValue != "0" && !stringValue.StartsWith("f", true, null));
        }

        /// <summary>
        /// Loads a key.
        /// </summary>
        /// <param name="section">The section the key is part of.</param>
        /// <param name="key">They key to load.</param>
        /// <param name="defaultValue">The value to use if none is found.</param>
        /// <returns>The value for the given key.</returns>
        public Keys GetKey(string section, string key, Keys defaultValue = Keys.None)
        {
            if (!TryGetValue(section, key, out var stringValue))
                return defaultValue;

            return Enum.TryParse(stringValue, out Keys keyValue) ? defaultValue : keyValue;
        }

        /// <summary>
        /// Sets a specific key to the given value.
        /// </summary>
        /// <param name="section">The section the key is part of.</param>
        /// <param name="key">The key to replace.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>True if the value was set successfully.</returns>
        public bool SetValue(string section, string key, string value)
        {
            try
            {
                var lines = new List<string>();

                // Load the current config file
                using (var sr = new StreamReader(FilePath))
                {
                    while (!sr.EndOfStream)
                    {
                        lines.Add(sr.ReadLine());
                    }
                }

                var sectionIndex = lines.FindIndex(f => f == $"[{section}]");
                LogHandler.Log("Section: " + lines[sectionIndex], LogType.Debug);
                var keyIndex = lines.FindIndex(sectionIndex, f => f.StartsWith(key));
                LogHandler.Log("Key: " + lines[keyIndex], LogType.Debug);

                var valueIndex = lines.ElementAt(keyIndex).IndexOf("=", StringComparison.Ordinal) + 1;
                LogHandler.Log("Value: " + lines[keyIndex].Remove(0, valueIndex), LogType.Debug);
                lines[keyIndex] = lines.ElementAt(keyIndex).Remove(valueIndex) + value;

                using (var sw = new StreamWriter(FilePath))
                    foreach (var line in lines)
                        sw.WriteLine(line);

                return true;
            }
            catch (Exception e)
            {
                LogHandler.Log("Unable to load the config file.", LogType.Error);
                LogHandler.Log(e.Message, LogType.Debug);
            }
            return false;
        }
    }
}
