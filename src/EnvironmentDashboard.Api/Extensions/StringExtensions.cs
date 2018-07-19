using System;

namespace EnvironmentDashboard.Api.Extensions {
    public static class StringExtensions {
        public static double ToDouble(this string input) {
            double.TryParse(input, out var tmp);
            return tmp;
        }

        public static DateTime ToDateTime(this string input) {
            DateTime.TryParse(input, out var tmp);
            return tmp;
        }
    }
}