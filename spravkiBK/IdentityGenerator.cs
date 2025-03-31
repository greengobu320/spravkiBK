using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

public static class IdentityGenerator
{
    public static string GenerateBase64IdentityClaims(string userName)
    {
        var claims = new List<Dictionary<string, string>>
        {
            new Dictionary<string, string>
            {
                { "Item1", "EsaipUserSid" },
                { "Item2", "AQUAAAAAAUVAAAAv1vqBpARySKgS3J\\BMFIQA==" } // можно сделать параметром, если нужно
            },
            new Dictionary<string, string>
            {
                { "Item1", "EsaipUserName" },
                { "Item2", userName }
            }
        };

        var identityData = new Dictionary<string, object>
        {
            { "SerializedWindowsIdentityClaims", JsonConvert.SerializeObject(claims) },
            { "SerializedJwtSecurityToken", "" },
            { "IsAuthenticatedWindows", true },
            { "EsaipUserCompositeId", null }
        };

        string json = JsonConvert.SerializeObject(identityData);
        string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        return base64;
    }
}
