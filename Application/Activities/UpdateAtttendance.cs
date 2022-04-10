using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities
{
    public class UpdateAtttendance
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {

            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _context = context;

                _userAccessor = userAccessor;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var activtiy = await _context.Activities
                    .Include(a => a.Attendees)
                    .ThenInclude(u => u.AppUser)
                    .SingleOrDefaultAsync(x =>x.Id == request.Id);

                if(activtiy == null) return null;

                var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername());

                if(user == null) return null;

                var hostUsername = activtiy.Attendees.FirstOrDefault(x => x.IsHost)?.AppUser?.UserName;

                var attendance = activtiy.Attendees.FirstOrDefault(x => x.AppUser.UserName == user.UserName);

                if(attendance != null && hostUsername == user.UserName)
                    activtiy.IsCancelled = !activtiy.IsCancelled;
                
                if(attendance != null & hostUsername != user.UserName)
                    activtiy.Attendees.Remove(attendance);

                if(attendance == null){
                    attendance = new ActivityAttendee{
                        AppUser = user,
                        Activity = activtiy,
                        IsHost = false
                    };

                    activtiy.Attendees.Add(attendance);
                }

                var result = await _context.SaveChangesAsync() > 0;

                return result ? Result<Unit>.Success(Unit.Value) : Result<Unit>.Failure("Problem updating attendance");
            }
        }
    }
}