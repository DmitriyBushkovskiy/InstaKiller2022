using Api.Models.Attach;
using Api.Models.PostContent;
using Api.Services;
using AutoMapper;
using DAL.Entities;

namespace Api.Mapper.MapperAction
{
    public class AttachMapperAction : IMappingAction<MetadataModel, Attach>
    {
        private LinkGeneratorService _linkGeneratorService;

        public AttachMapperAction(LinkGeneratorService linkGeneratorService)
        {
            _linkGeneratorService = linkGeneratorService;
        }

        public void Process(MetadataModel source, Attach destination, ResolutionContext context)
        {
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "attaches");
            var destFi = new FileInfo(directory);
            if (destFi.Directory != null && !destFi.Directory.Exists)
                destFi.Directory.Create();
            _linkGeneratorService.GetAttachLink(source, destination);
            var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(), source.TempId.ToString()));
            if (tempFi.Exists)
                File.Move(tempFi.FullName, destination.FilePath, true);
        }
    }
}
