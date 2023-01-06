//using FluentValidation;

//using MediatR;

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Application.Balances.Commands;

//public class QvapayReceived
//{
//    public class Command : IRequest<Data>
//    {
//        public string 
//    }

//    public class Validation : AbstractValidator<Command>
//    {
//        public Validation()
//        {
//            CascadeMode = CascadeMode.Stop;

//            RuleFor(c => c)
//                .Must(i => true)
//                .WithMessage("ERROR");
//        }
//    }

//    public class CommandHandler : IRequestHandler<Command, Data>
//    {
//        public CommandHandler()
//        {

//        }

//        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
//        {
//            throw new NotImplementedException();
//        }
//    }

//    public class Data
//    {

//    }

//} // main class 
