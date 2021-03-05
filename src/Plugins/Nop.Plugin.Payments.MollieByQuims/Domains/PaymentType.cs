namespace Nop.Plugin.Payments.MollieByQuims.Domain
{
    /// <summary>
    /// Represents payment type enumeration
    /// </summary>
    public enum PaymentType
    {
        /// <summary>
        /// The merchant intends to capture payment immediately after the customer makes a payment.
        /// The Payments API allows you to create payments for your web shop, e-invoicing platform or other application that you need payments for.
        /// </summary>
        Payment

        /// <summary>
        ///For each order in your shop, you can create an Order via the Mollie API. The order will remain valid for a certain amount of time (default 28 days).
        /// </summary>
        //Order
    }
}