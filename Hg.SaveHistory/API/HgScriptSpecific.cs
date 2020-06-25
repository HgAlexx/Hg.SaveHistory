using System;
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
        #region Members

        /// <summary>
        /// Decrypt a file from DOOM Eternal Saves
        /// 
        /// Function taken from https://github.com/GoobyCorp/DOOMSaveManager/blob/master/Crypto.cs
        /// Thank you for the reverse engineering of the save file encryption.
        /// TODO: convert to native .NET when possible and remove dependency
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

        #endregion
    }
}