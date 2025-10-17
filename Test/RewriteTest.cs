using System.Xml;

namespace Test
{
    public class RewriteTest
    {
        [Fact]
        public async Task TestRewrite()
        {
            // Arrange
            var rewriter = new DacDeploySkip.XmlRewriter();

            // Act
            await rewriter.RewriteXmlMetadataAsync("model.xml");

            // Assert
            var xmlDoc = new XmlDocument();
            xmlDoc.Load("model.xml");

            var nodes = xmlDoc.GetElementsByTagName("Metadata");

            foreach (XmlNode node in nodes)
            {
                if (node.Attributes != null 
                    && node.Attributes.Count == 2
                    && node.Attributes[0].Name == "Name"
                    && node.Attributes[0].Value == "FileName")
                {
                    var originalValue = node.Attributes[1].Value;
                    Assert.True(!originalValue.Contains(Path.DirectorySeparatorChar));
                }
            }
        }
    }
}