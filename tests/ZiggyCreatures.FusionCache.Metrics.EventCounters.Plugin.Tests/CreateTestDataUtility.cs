﻿using System.Text.Json;
using Xunit.Abstractions;

namespace ZiggyCreatures.Caching.Fusion.Metrics.EventCounters.Plugin.Tests
{

    public class CreateTestDataUtility
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private JsonSerializerOptions serializeOptions;

        public CreateTestDataUtility(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;

            serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        /// <summary>
        /// Data in MockDomainCertData.json and MockEmailtoIpData.json was generated by Makaroo.com
        /// This test utility correlates the domains in MockDomainCertData.json to the email addresses in
        /// MockEmailToIpData.json.  This data will be used to build example apps with caching and metrics needs.
        /// </summary>
        // [Fact]
        // public void fixupMockEmailToIpData_Json()
        // {
        //     //_testOutputHelper.WriteLine(File.ReadAllText("../../../../../examples/data/MockDomainCertData.json"));
        //     
        //     var domains = JsonSerializer.Deserialize<List<DomainCertData>>(
        //         File.ReadAllText("../../../../../examples/data/MockDomainCertData.json"),
        //         serializeOptions);
        //
        //     var emails = JsonSerializer.Deserialize<List<EmailToIpData>>(
        //         File.ReadAllText("../../../../../examples/data/MockEmailToIpOriginalData.json"),
        //         serializeOptions);
        //
        //     var pattern = "(?<mail>.*@)(?<host>.*)";
        //     
        //
        //     foreach (var emailToIpData in emails)
        //     {
        //         var sourceIndex = emailToIpData.Id;
        //         var sourceEmail = emailToIpData.Email;
        //         var searchIndex = sourceIndex >= 100 ? emailToIpData.Id % 100 + 1 : emailToIpData.Id % 100;
        //         // _testOutputHelper.WriteLine(emailToIpData.Id + " " + searchIndex.ToString());
        //         var searchDomain = domains.Single(d => d.Id == searchIndex).Domain;
        //         var replacement = "${mail}" + searchDomain;
        //         emailToIpData.Email = Regex.Replace(sourceEmail, pattern, replacement);
        //     }
        //
        //     //_testOutputHelper.WriteLine(JsonSerializer.Serialize(emails, serializeOptions));
        //
        //     File.WriteAllText("../../../../../examples/data/MockEmailToIpData.json", JsonSerializer.Serialize(emails, serializeOptions));
        // }
        //
        // [Fact]
        // public void MockDomainCertDataUtil()
        // {
        //     var domains = JsonSerializer.Deserialize<List<DomainCertData>>(
        //         File.ReadAllText("../../../../../examples/data/MockDomainCertOriginalData.json"),
        //         serializeOptions);
        //
        //     var domainGroups = domains.GroupBy(d => d.Domain);
        //     List<DomainCertData> uniqueDomains = new List<DomainCertData>();
        //     int i = 1;
        //
        //     foreach (var group in domainGroups)
        //     {
        //         var item = group.First();
        //         item.Id = i++;
        //         uniqueDomains.Add(group.First());
        //         if (i >= 101) break;
        //     }
        //
        //     // _testOutputHelper.WriteLine(JsonSerializer.Serialize(uniqueDomains));
        //
        //     File.WriteAllText("../../../../../examples/data/MockDomainCertData.json", JsonSerializer.Serialize(uniqueDomains, serializeOptions));
        // }


        // [Fact]
        // public void CreateLoadTestDataUtil()
        // {
        //     var emails = JsonSerializer.Deserialize<List<EmailToIpData>>(
        //         File.ReadAllText("../../../../../examples/data/MockEmailToIpData.json"),
        //         serializeOptions);
        //
        //     var testData = "email\r\n" + string.Join("\r\n", emails.Select(e => e.Email));
        //
        //     // _testOutputHelper.WriteLine(testData);
        //     File.WriteAllText("../../../../../examples/superbenchmarker/EmailAddressData.csv", testData);
        // }


    }
}