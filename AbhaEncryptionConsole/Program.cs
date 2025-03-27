using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

class Program
{
    static readonly string configFilePath = "config.json";

    static void Main()
    {
        StoreUserData();
    }

    // 📌 Store Encrypted Data in config.json
    static void StoreUserData()
    {
        Console.WriteLine("Select an option:");
        Console.WriteLine("1. Enter mobile number");
        Console.WriteLine("2. Enter OTP");
        Console.WriteLine("3. Enter ABHA number");
        Console.WriteLine("4. Enter Aadhar number");
        Console.Write("Choice: ");
        string choice = Console.ReadLine();

        string publicKey = GetPublicKey();

        if (string.IsNullOrEmpty(publicKey))
        {
            Console.WriteLine("❌ Public key not found.");
            return;
        }

        Dictionary<string, string> configData = LoadConfig();

        switch (choice)
        {
            case "1":
                Console.Write("Enter mobile number to encrypt: ");
                configData["ENCRYPTED_MOBILE_NUMBER"] = RSAEncrypt(Console.ReadLine(), publicKey);
                Console.WriteLine("✅ Mobile number encrypted and stored.");
                break;
            case "2":
                Console.Write("Enter OTP to encrypt: ");
                configData["ENCRYPTED_OTP"] = RSAEncrypt(Console.ReadLine(), publicKey);
                Console.WriteLine("✅ OTP encrypted and stored.");
                break;
            case "3":
                Console.Write("Enter ABHA number: ");
                configData["ABHA_NUMBER"] = Console.ReadLine();
                Console.WriteLine("✅ ABHA number stored.");
                break;
            case "4":
                Console.Write("Enter Aadhar number: ");
                configData["AADHAR_NUMBER"] = Console.ReadLine();
                Console.WriteLine("✅ Aadhar number stored.");
                break;
            default:
                Console.WriteLine("❌ Invalid choice.");
                return;
        }

        SaveConfig(configData);
    }

    // 📌 Encrypt Data using RSA
    static string RSAEncrypt(string text, string publicKeyPem)
    {
        using (var rsa = RSA.Create())
        {
            rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKeyPem), out _);
            byte[] encryptedBytes = rsa.Encrypt(Encoding.UTF8.GetBytes(text), RSAEncryptionPadding.OaepSHA256);
            return Convert.ToBase64String(encryptedBytes);
        }
    }

    // 📌 Retrieve Public Key from Environment Variables
    static string GetPublicKey()
    {
        return Environment.GetEnvironmentVariable("publicKey", EnvironmentVariableTarget.User);
    }

    // 📌 Load Data from config.json
    static Dictionary<string, string> LoadConfig()
    {
        if (!File.Exists(configFilePath))
            return new Dictionary<string, string>();

        string json = File.ReadAllText(configFilePath);
        return JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
    }

    // 📌 Save Data to config.json
    static void SaveConfig(Dictionary<string, string> configData)
    {
        string json = JsonConvert.SerializeObject(configData, Formatting.Indented);
        File.WriteAllText(configFilePath, json);
    }
}
