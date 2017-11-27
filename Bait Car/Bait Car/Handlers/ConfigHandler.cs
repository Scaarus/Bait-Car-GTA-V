using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using Rage;

namespace Bait_Car.Handlers
{
    /// <summary>
    /// Responsible for loading the options used by the plugin.
    /// </summary>
    public class ConfigHandler
    {
        private const string FilePath = @"Plugins\Bait Car.ini";

        private const string DefaultFile =
            @"// Set any keybind to 'None' to disable that key
// Set a modifier key by using +
// Eg. Shift+Y

[Keys]

// The main key that opens the menu
// Default: F7
OpenMenu=F7

// Shut down the bait car's engine.
// Pressing a second time turns the engine back on.
// Default: K
KillSwitch=K

// Warp the bait car closer
// Default: Shift+Y
WarpCar=Shift+Y

[Controller]

// Valid controller buttons: A, B, X, Y, DPadUp, DPadDown, DPadLeft, DPadRight, Back, Start, RightShoulder, RightThumb, LeftShoulder, LeftThumb, None

// The main key that opens the menu
// Default: None
OpenMenu=None

// Shut down the bait car's engine.
// Pressing a second time turns the engine back on.
// Default: None
KillSwitch=None

// Warp the bait car closer
// Default: None
WarpCar=None

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

        private readonly Dictionary<string, string> _options = new Dictionary<string, string>();

        public ConfigHandler()
        {
            LogHandler.Log("Starting config handler.", LogType.Debug);

            // Create the config file if it doesn't exist
            if (!DoesConfigExist())
            {
                LogHandler.Log("Config file does not exist.", LogType.Debug);
                CreateDefaultConfig();
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
        public static void CreateDefaultConfig()
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
            _options.Clear();
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

                            _options.Add($"{section}.{key}".ToLower(), value);
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
            return _options.TryGetValue($"{section}.{key}".ToLower(), out value);
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
        public Keys[] GetKey(string section, string key, Keys defaultValue = Keys.None)
        {
            if (!TryGetValue(section, key, out var stringValue))
                return new[] { defaultValue };

            // Check for + to see if there is a modifier
            if (stringValue.Contains("+") && !stringValue.EndsWith("+"))
            {
                    // Split the string and get our modifier and key value
                Enum.TryParse(stringValue.Split('+').First(), out Keys modifier);
                Enum.TryParse(stringValue.Split('+').Last(), out Keys keyValue);
                return new[] { modifier, keyValue };
            }
            else
                // Return our key if one is found or the default key
                return new[] { Enum.TryParse(stringValue, out Keys keyValue) ? keyValue : defaultValue };
        }

        /// <summary>
        /// Loads a key.
        /// </summary>
        /// <param name="section">The section the key is part of.</param>
        /// <param name="key">They key to load.</param>
        /// <param name="defaultValue">The value to use if none is found.</param>
        /// <returns>The value for the given key.</returns>
        public Keys[] GetKey(string section, string key, Keys[] defaultValue = null)
        {
            if (!TryGetValue(section, key, out var stringValue))
                return defaultValue;

            // Check for + to see if there is a modifier
            if (stringValue.Contains("+") && !stringValue.EndsWith("+"))
            {
                // Split the string and get our modifier and key value
                Enum.TryParse(stringValue.Split('+').First(), out Keys modifier);
                Enum.TryParse(stringValue.Split('+').Last(), out Keys keyValue);
                return new[] { modifier, keyValue };
            }
            else
                // Return our key if one is found or the default key
                return Enum.TryParse(stringValue, out Keys keyValue) ? new[] { keyValue } : defaultValue;
        }

        /// <summary>
        /// Loads a button.
        /// </summary>
        /// <param name="section">The section to load from.</param>
        /// <param name="key">The key to load.</param>
        /// <param name="defaultValue">The value to use if no key is found.</param>
        /// <returns>The requested key or the default.</returns>
        public ControllerButtons GetButton(string section, string key,
            ControllerButtons defaultValue = ControllerButtons.None)
        {
            if (!TryGetValue(section, key, out var stringValue))
                return defaultValue;

            return Enum.TryParse(stringValue, out ControllerButtons buttonValue) ? buttonValue : defaultValue;
        }

        /// <summary>
        /// Given a set of keys and values, save all changes to the ini file.
        /// </summary>
        /// <param name="values">{"Section.Option", Value}</param>
        /// <returns>If the save was successful.</returns>
        public bool SetValues(Dictionary<string, string> values)
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

                // Update the lines to use the new values
                foreach (var i in values)
                {
                    var section = i.Key.Split('.').First();
                    var key = i.Key.Split('.').Last();

                    var sectionIndex = lines.FindIndex(f => f == $"[{section}]");
                    var keyIndex = lines.FindIndex(sectionIndex, f => f.StartsWith(key));
                    var valueIndex = lines.ElementAt(keyIndex).IndexOf("=", StringComparison.Ordinal) + 1;
                    lines[keyIndex] = lines.ElementAt(keyIndex).Remove(valueIndex) + i.Value;
                    _options[$"{section}.{key}"] = i.Value;
                }

                // Write the new values to the file
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
