﻿using MailKit.Security;

namespace ITHelpDesk.Models
{
    public class MailKitEmailSenderOptions
    {
        public MailKitEmailSenderOptions()
        {
            Host_SecureSocketOptions = SecureSocketOptions.StartTls;
        }

        public string? Host_Address { get; set; }

        public int Host_Port { get; set; }

        public string? Host_Username { get; set; }

        public string? Host_Password { get; set; }

        public SecureSocketOptions Host_SecureSocketOptions { get; set; }

        public string? Sender_EMail { get; set; }

        public string? Sender_Name { get; set; }
    }
}
