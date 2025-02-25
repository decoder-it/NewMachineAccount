using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        
        Dictionary<string, string> arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        
        for (int i = 0; i < args.Length - 1; i += 2)
        {
            if (args[i].StartsWith("-"))
            {
                arguments[args[i].ToLower()] = args[i + 1];
            }
        }

        
        if (!arguments.TryGetValue("-dc", out string domainController) ||
            !arguments.TryGetValue("-name", out string machineAccount) ||
            !arguments.TryGetValue("-domain", out string domain) ||
            !arguments.TryGetValue("-password", out string machinePasswordStr)) // Mandatory
        {
            Console.WriteLine("Usage: NewMachineAccount.exe v1.0 by @decoder_it\n\t-dc <DomainController>\n\t-name <MachineAccount>r\n\t-domain <Domain>r\n\t-password <MachinePassword>r\n\t[-ou <OU>]r\n\t[-user <Username>]\n\t[-pass <Password>]\n\t[-uac <UAC Flags>]");
            return;
        }

        
        arguments.TryGetValue("-ou", out string ou); // Default to CN=Computers if not provided
        arguments.TryGetValue("-user", out string username);
        arguments.TryGetValue("-pass", out string passwordStr);
        arguments.TryGetValue("-uac", out string uacStr);

        
        SecureString machinePassword = ConvertToSecureString(machinePasswordStr);

        
        int uac = 4096;
        if (!string.IsNullOrEmpty(uacStr) && int.TryParse(uacStr, out int parsedUac))
        {
            uac = parsedUac;
        }

        
        NetworkCredential credential = null;
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(passwordStr))
        {
            SecureString securePassword = ConvertToSecureString(passwordStr);
            credential = new NetworkCredential(username, securePassword);
        }

        
        AddMachineAccount(domain, domainController, machineAccount, machinePassword, credential, ou, uac);
    }

    static SecureString ConvertToSecureString(string password)
    {
        SecureString securePassword = new SecureString();
        foreach (char c in password)
        {
            securePassword.AppendChar(c);
        }
        return securePassword;
    }

    static void AddMachineAccount(string domain, string domainController, string machineAccount, SecureString password, NetworkCredential credential = null, string ou = null, int uac = 4096)
    {
        string samAccount = machineAccount.EndsWith("$") ? machineAccount : machineAccount + "$";
        string distinguishedName;
        try
        {
            
            if (ou == null)
            {
                distinguishedName = $"CN={machineAccount},CN=Computers,DC={domain.Replace(".", ",DC=")}";
            }
            else
            {
                distinguishedName = $"CN={machineAccount},{ou}";
            }

            IntPtr passwordBSTR = Marshal.SecureStringToBSTR(password);
            byte[] passwordBytes = Encoding.Unicode.GetBytes("\"" + Marshal.PtrToStringBSTR(passwordBSTR) + "\"");
            Marshal.ZeroFreeBSTR(passwordBSTR);

            LdapDirectoryIdentifier identifier = new LdapDirectoryIdentifier(domainController, 389);
            LdapConnection connection;

            
            if (credential != null)
            {
                Console.WriteLine("[*] Using alternate Credentials");
                connection = new LdapConnection(identifier, credential);
            }
            else
            {
                connection = new LdapConnection(identifier);
            }
            Console.WriteLine($"[*] Trying to create Machine Account: {distinguishedName}");
            connection.SessionOptions.Sealing = true;
            connection.SessionOptions.Signing = true;

            
            connection.Bind();
AddRequest request = new AddRequest(distinguishedName, "computer");
            request.Attributes.Add(new DirectoryAttribute("SamAccountName", samAccount));
            request.Attributes.Add(new DirectoryAttribute("userAccountControl", uac.ToString()));
            request.Attributes.Add(new DirectoryAttribute("DnsHostName", $"{machineAccount}.{domain}"));
            request.Attributes.Add(new DirectoryAttribute("ServicePrincipalName",
                $"HOST/{machineAccount}.{domain}",
                $"RestrictedKrbHost/{machineAccount}.{domain}",
                $"HOST/{machineAccount}",
                $"RestrictedKrbHost/{machineAccount}"));
            request.Attributes.Add(new DirectoryAttribute("unicodePwd", passwordBytes));
            connection.SendRequest(request);
            Console.WriteLine($"[+] Machine account created successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[-] Error creating Machine Account: {ex.Message}");
        }
    }

}
