# NewMachineAccount

## Overview
**NewMachineAccount.exe is  a simple standalone exe  tool for creating new machine accounts with custom password within a specified domain.

## Usage
```
NewMachineAccount.exe v1.0 by @decoder_it
    -dc <DomainController>
    -name <MachineAccount>
    -domain <Domain>
    -password <MachinePassword>
    [-ou <OU>]
    [-user <Username>]
    [-pass <Password>]
    [-uac <UAC Flags>]
```

## Parameters
- `-dc <DomainController>`: Specifies the domain controller to use.
- `-name <MachineAccount>`: Name of the machine account to be created.
- `-domain <Domain>`: The domain in which the machine account will be created.
- `-password <MachinePassword>`: Password for the new machine account.
- `[-ou <OU>]` (Optional): Organizational Unit (OU) where the account should be placed.
- `[-user <Username>]` (Optional): Username for authentication.
- `[-pass <Password>]` (Optional): Password for authentication.
- `[-uac <UAC Flags>]` (Optional): User Account Control (UAC) flags.

## Example Usage
```
NewMachineAccount.exe -dc DC01 -name NewMachine -domain example.com -password StrongPass123!
```

```
NewMachineAccount.exe -dc DC02 -name TestMachine -domain test.com -password Passw0rd! -ou "OU=Computers,DC=test,DC=com" -user admin -pass AdminPass
```

## Notes
- Ensure you have the necessary permissions to create machine accounts in the specified domain.
- If you want to use explict credentials, use the `-user` and `-pass` options.
- The `-uac` flag allows setting specific UAC configurations.

## Author
**@decoder_it**

