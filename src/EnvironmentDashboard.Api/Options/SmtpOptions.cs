using System;

namespace EnvironmentDashboard.Api.Options {
    public class SmtpOptions {
        public string Host { get; set; }
        public Int32 Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Sender { get; set; }
    }
}