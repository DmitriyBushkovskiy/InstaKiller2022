using Api.Models.Attach;

namespace Api.Models.Post
{
    public class CreatePostRequest
    {
        public string? Description { get; set; } //TODO: добавить валидацию - микимальная м максимальная длина строки
        public List<MetadataModel> Contents { get; set; } = new List<MetadataModel>();  //TODO: добавить валидац
    }
}
