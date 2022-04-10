using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities
{
    public class List
    {
        public class Query : IRequest<Result<List<ActivityDto>>> { }

        public class Handler : IRequestHandler<Query, Result<List<ActivityDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;

            }

            public async Task<Result<List<ActivityDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var actitivities = await _context.Activities
                    .ProjectTo<ActivityDto>(_mapper.ConfigurationProvider) //Project Load
                    // .Include(a => a.Attendees) <- eager load
                    // .ThenInclude(u => u.AppUser)
                    .ToListAsync(cancellationToken);

                //var actitivitiesToReturn = _mapper.Map<List<ActivityDto>>(actitivities);

                return Result<List<ActivityDto>>.Success(actitivities);
            }
        }


    }
}