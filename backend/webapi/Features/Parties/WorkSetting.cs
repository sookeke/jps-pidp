namespace Pidp.Features.Parties;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Pidp.Data;
using Pidp.Features;
using Pidp.Models;
using Pidp.Models.Lookups;

public class WorkSetting
{
    public class Query : IQuery<Command>
    {
        public int PartyId { get; set; }
    }

    public class Command : ICommand
    {
        public int PartyId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string FacilityName { get; set; } = string.Empty;

        public Address? PhysicalAddress { get; set; }

        public class Address
        {
            public CountryCode CountryCode { get; set; }
            public ProvinceCode ProvinceCode { get; set; }
            public string Street { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string Postal { get; set; } = string.Empty;
        }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            this.RuleFor(x => x.PartyId).NotEmpty();
            this.RuleFor(x => x.PhysicalAddress).SetValidator(new AddressValidator()!);
        }
    }

    public class AddressValidator : AbstractValidator<Command.Address>
    {
        public AddressValidator()
        {
            this.RuleFor(x => x.CountryCode).IsInEnum();
            this.RuleFor(x => x.ProvinceCode).IsInEnum();
            this.RuleFor(x => x.Street).NotEmpty();
            this.RuleFor(x => x.City).NotEmpty();
            this.RuleFor(x => x.Postal).NotEmpty();
        }
    }

    public class QueryHandler : IQueryHandler<Query, Command>
    {
        private readonly PidpDbContext context;
        private readonly IMapper mapper;

        public QueryHandler(PidpDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<Command> HandleAsync(Query query)
        {
            return await this.context.Parties
                .Where(party => party.Id == query.PartyId)
                .ProjectTo<Command>(this.mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }
    }

    public class CommandHandler : ICommandHandler<Command>
    {
        private readonly PidpDbContext context;

        public CommandHandler(PidpDbContext context) => this.context = context;

        public async Task HandleAsync(Command command)
        {
            var party = await this.context.Parties
                .Include(party => party.Facility)
                .SingleOrDefaultAsync(party => party.Id == command.PartyId);

            if (party == null)
            {
                return;
            }

            party.JobTitle = command.JobTitle;

            if (party.Facility == null)
            {
                party.Facility = new Facility
                {
                    PartyId = command.PartyId,
                };
                this.context.Facilities.Add(party.Facility);
            }

            party.Facility.FacilityName = command.FacilityName;

            if (command.PhysicalAddress == null)
            {
                party.Facility.PhysicalAddress = null;
            }
            else
            {
                party.Facility.PhysicalAddress = new FacilityAddress
                {
                    AddressType = AddressType.Physical,
                    CountryCode = command.PhysicalAddress.CountryCode,
                    ProvinceCode = command.PhysicalAddress.ProvinceCode,
                    Street = command.PhysicalAddress.Street,
                    City = command.PhysicalAddress.City,
                    Postal = command.PhysicalAddress.Postal
                };
            }

            await this.context.SaveChangesAsync();
        }
    }
}
