using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.Core.Services
{
    public class TreeService(ITreeRepository treeRepository, IUnitOfWork unitOfWork, IMapper mapper) : ITreeService
    {
        public async Task<List<TreeDto>> GetAllTreeAsync()
        {
            var items = await treeRepository.GetAllAsync();
            return mapper.Map<List<TreeDto>>(items);
        }
    }
}
