using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BookingGatewayRepository;
using BookingGatewayRepository.Model;
using BookingGatewayService.Exceptions;

namespace BookingGatewayService
{
    public class BookingGateway : IBookingGateway
    {
        public BookingGateway(IDBRepository dbRepository)
        {
            DBRepository = dbRepository;
        }

        private static volatile object _locker = new object();
        public IDBRepository DBRepository { get; set; }

        public void StartBooking()
        {
            if (Monitor.IsEntered(_locker) || !Monitor.TryEnter(_locker))
                throw new BookingInProgressException();
        }

        public void EndBooking()
        {
            if (Monitor.IsEntered(_locker))
                Monitor.Exit(_locker);
            else
                throw new NoStartBookingException();
        }

        public void Booking(string uniqueReference, decimal amount, string transactonTitle, string srcAccountNo, string destAccountNo)
        {
            if (Monitor.IsEntered(_locker))
            {
                var transactionData = new TransactionData
                {
                    Amount = amount,
                    DestAccountNo = destAccountNo,
                    SourceAccountNo = srcAccountNo,
                    UniqueRef = uniqueReference,
                    TransactionTitle = transactonTitle
                };

                DBRepository.SaveBooking(transactionData);
            }
            else
            {
                throw new NoStartBookingException();
            }
        }

        public IList<TransactionStatus> GetBookingStatuses(IList<string> uniqueTransactionRefs)
        {
            //if (Monitor.IsEntered(_locker))
            //    throw new BookingInProgressException();

            if (uniqueTransactionRefs == null || !uniqueTransactionRefs.Any())
            {
                return new List<TransactionStatus>();
            }
            return DBRepository.GetStatuses(uniqueTransactionRefs.ToArray()).ToList();
        }
    }
}