using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Helpers
{
    public enum RefundErrorType
    {
        /// <summary>
        /// Unknown error
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Payment request can't be found
        /// </summary>
        PaymentRequestNotFound,

        /// <summary>
        /// Invliad refund destination address
        /// </summary>
        InvalidDestinationAddress,

        /// <summary>
        /// No payment transactions exists to be refunded
        /// </summary>
        NoPaymentTransactions,

        /// <summary>
        /// Refund is not allowed in payment request status
        /// </summary>
        NotAllowedInStatus,

        /// <summary>
        /// Multiple payment transactions exists
        /// </summary>
        MultitransactionNotSupported
    }
}
