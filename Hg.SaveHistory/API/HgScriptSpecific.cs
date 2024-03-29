﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace Hg.SaveHistory.API
{
    public static class HgScriptSpecific
    {
        #region Types

        public class SatisfactoryHeaderData
        {
            #region Fields & Properties

            public int BuildVersion;
            public TimeSpan PlayedTime;
            public DateTime SavedAt;
            public int SaveVersion;
            public string SessionName;

            #endregion
        }

        #endregion

        #region Members

        /// <summary>
        ///     Decrypt a file from DOOM Eternal Saves
        ///     Function taken from https://github.com/GoobyCorp/DOOMSaveManager/blob/master/Crypto.cs
        ///     Thank you for the reverse engineering of the save file encryption.
        ///     TODO: convert to native .NET when possible and remove dependency
        /// </summary>
        /// <param name="fileKey">decryption key</param>
        /// <param name="filePath">full path to file</param>
        /// <returns>plain text decrypted data</returns>
        public static string DOOMEternal_Decrypt(string fileKey, string filePath)
        {
            string plaintext;

            try
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                byte[] nonceBytes = new byte[12];
                byte[] cipherBytes = new byte[fileData.Length - 12];

                Buffer.BlockCopy(fileData, 0, nonceBytes, 0, nonceBytes.Length);
                Buffer.BlockCopy(fileData, nonceBytes.Length, cipherBytes, 0, cipherBytes.Length);

                byte[] fileKeyBytes = Encoding.UTF8.GetBytes(fileKey);
                byte[] fileKeyHash = new SHA256Managed().ComputeHash(fileKeyBytes);

                var gcmBlockCipher = new GcmBlockCipher(new AesEngine());
                var parameters = new AeadParameters(new KeyParameter(fileKeyHash, 0, 16), 128, nonceBytes, fileKeyBytes);

                gcmBlockCipher.Init(false, parameters);

                byte[] plainBytes = new byte[gcmBlockCipher.GetOutputSize(cipherBytes.Length)];
                int outputOffset = gcmBlockCipher.ProcessBytes(cipherBytes, 0, cipherBytes.Length, plainBytes, 0);

                gcmBlockCipher.DoFinal(plainBytes, outputOffset);

                plaintext = Encoding.UTF8.GetString(plainBytes);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
                return null;
            }

            return plaintext;
        }

        /// <summary>
        ///     Parse save file
        ///     https://satisfactory.gamepedia.com/Save_files
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>SatisfactoryHeaderData</returns>
        public static SatisfactoryHeaderData Satisfactory_GetHeaderData(string filePath)
        {
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var binaryReader = new BinaryReader(fileStream))
                    {
                        SatisfactoryHeaderData data = new SatisfactoryHeaderData();

                        // same variable to read binary value
                        string value;

                        // header version
                        // ReSharper disable once RedundantAssignment
                        var int32Byte = binaryReader.ReadInt32();
                        // unused
                        // save version
                        int32Byte = binaryReader.ReadInt32();
                        data.SaveVersion = int32Byte;

                        // build version
                        int32Byte = binaryReader.ReadInt32();
                        data.BuildVersion = int32Byte;

                        string ReadString()
                        {
                            // get string size
                            int byteCount = binaryReader.ReadInt32();
                            if (byteCount == 0)
                            {
                                return "";
                            }

                            // ASCII string
                            if (byteCount > 0)
                            {
                                value = new string(binaryReader.ReadChars(byteCount));
                            }
                            else
                            {
                                // UTF16 string
                                byte[] bytes = binaryReader.ReadBytes(2 * -byteCount);
                                value = Encoding.Unicode.GetString(bytes);
                            }

                            return value.TrimEnd('\0');
                        }

                        // map name: size + data
                        // ReSharper disable once RedundantAssignment
                        value = ReadString();
                        // unused

                        // map options: size + data
                        // ReSharper disable once RedundantAssignment
                        value = ReadString();
                        // unused

                        // session name: size + data
                        value = ReadString();
                        data.SessionName = value;

                        // played time
                        int32Byte = binaryReader.ReadInt32();
                        data.PlayedTime = new TimeSpan(0, 0, int32Byte); // as seconds

                        // saved at
                        var int64Byte = binaryReader.ReadInt64();
                        data.SavedAt = new DateTime(int64Byte, DateTimeKind.Utc).ToLocalTime(); // as ticks

                        return data;
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
                return null;
            }
        }

        #endregion
    }
}