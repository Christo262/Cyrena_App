using Cyrena.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace Cyrena.Serialization.Tests
{
    public class KernelContentSerializationTests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void SerializeSimple()
        {
            var message = new ChatMessageContent(AuthorRole.User, "This is some text content");
            var entity = new ChatMessageContentEntity(message, Ulid.NewUlid());
            var json = JsonSerializer.Serialize(entity);
            var copied_entity = JsonSerializer.Deserialize<ChatMessageContentEntity>(json);
            Assert.That(copied_entity, Is.Not.Null);
            var copied_message = copied_entity.AsChatMessage();
            Assert.That(message.Content, Is.EqualTo(copied_message.Content));
            Assert.That(entity.Id, Is.EqualTo(copied_entity.Id));
            Assert.That(entity.IterationId, Is.EqualTo(copied_entity.IterationId));
        }

        [Test]
        public void SerializeSaveAs()
        {
            var message = new ChatMessageContent(AuthorRole.User, "This is some text content");

            var metadata = new Dictionary<string, object?>();
            metadata.Add("name", "TestFile.txt");
            metadata.Add("fileId", "a-test-file");
#pragma warning disable SKEXP0110
            var reference = new FileReferenceContent("test-file-id")
            {
                MimeType = "text/plain"
            };
            metadata.Add("save-as", reference);
#pragma warning restore SKEXP0110
            var file = new TextContent("Hello World", metadata: metadata);
            message.Items.Add(file);

            var entity = new ChatMessageContentEntity(message, Ulid.NewUlid());
            Assert.That(entity.Items.Select(x => x.Item), Has.Member(reference));
            var json = JsonSerializer.Serialize(entity);
            var copied_entity = JsonSerializer.Deserialize<ChatMessageContentEntity>(json);
            Assert.That(copied_entity, Is.Not.Null);
            var copied_message = copied_entity.AsChatMessage();
            Assert.That(message.Content, Is.EqualTo(copied_message.Content));
            Assert.That(entity.Id, Is.EqualTo(copied_entity.Id));
            Assert.That(entity.IterationId, Is.EqualTo(copied_entity.IterationId));
#pragma warning disable SKEXP0110
            var copied_reference = copied_message.Items.First(x => x is FileReferenceContent);
            Assert.That(copied_message.Items, Has.Member(copied_reference));
            Assert.That(reference.FileId, Is.EqualTo(((FileReferenceContent)copied_reference).FileId));
#pragma warning restore SKEXP0110
        }
    }
}
