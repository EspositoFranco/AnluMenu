namespace AnluMenu
{
    /// <summary>
    /// Abstraction for settings persistence. The menu package never calls PlayerPrefs
    /// directly — implement this interface to use JSON files, cloud saves, or any
    /// other backend. The default is <see cref="PlayerPrefsStorage"/>.
    /// </summary>
    public interface ISettingsStorage
    {
        /// <summary>Reads a float value, returning <paramref name="defaultValue"/> if the key does not exist.</summary>
        float GetFloat(string key, float defaultValue);

        /// <summary>Writes a float value.</summary>
        void SetFloat(string key, float value);

        /// <summary>Reads an int value, returning <paramref name="defaultValue"/> if the key does not exist.</summary>
        int GetInt(string key, int defaultValue);

        /// <summary>Writes an int value.</summary>
        void SetInt(string key, int value);

        /// <summary>Reads a string value, returning <paramref name="defaultValue"/> if the key does not exist.</summary>
        string GetString(string key, string defaultValue);

        /// <summary>Writes a string value.</summary>
        void SetString(string key, string value);

        /// <summary>Flushes pending writes to disk / network.</summary>
        void Save();

        /// <summary>Returns true if the key exists in the store.</summary>
        bool HasKey(string key);

        /// <summary>Removes a key from the store.</summary>
        void DeleteKey(string key);
    }
}
