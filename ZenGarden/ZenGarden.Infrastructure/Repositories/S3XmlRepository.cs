using System.Net;
using System.Xml.Linq;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.DataProtection.Repositories;

namespace ZenGarden.Infrastructure.Repositories;

public class S3XmlRepository(AmazonS3Client s3Client, string bucketName) : IXmlRepository
{
    private const string DataProtectionKeyFile = "dataprotection-keys.xml";

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        try
        {
            var request = new GetObjectRequest { BucketName = bucketName, Key = DataProtectionKeyFile };
            using var response = s3Client.GetObjectAsync(request).Result;
            using var reader = new StreamReader(response.ResponseStream);
            var xmlContent = reader.ReadToEnd();
            return XElement.Parse(xmlContent).Elements().ToList();
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Array.Empty<XElement>();
        }
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        var xml = new XElement("keys", GetAllElements().Append(element));
        var xmlString = xml.ToString();

        var putRequest = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = DataProtectionKeyFile,
            ContentBody = xmlString,
            ContentType = "application/xml"
        };

        s3Client.PutObjectAsync(putRequest).Wait();
    }
}