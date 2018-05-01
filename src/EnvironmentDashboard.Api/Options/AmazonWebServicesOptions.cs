using System;
using Amazon;

namespace EnvironmentDashboard.Api.Options {
    public class AmazonWebServicesOptions {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string BucketName { get; set; }
        public RegionEndpoint Region { get; set; }
    }
}