namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.BuyerAggregate;

public class Buyer
    : Entity
{
    private readonly List<PaymentMethod> _paymentMethods = new();
    private readonly List<Order> _orders = new();

    protected Buyer()
    {
        
    }

    public Buyer(string identityGuid, string name)
    {
        IdentityGuid = identityGuid ?? throw new ArgumentNullException(nameof(identityGuid));
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public string IdentityGuid { get; private set; }

    public string Name { get; private set; }

    public IReadOnlyCollection<PaymentMethod> PaymentMethods => _paymentMethods.AsReadOnly();

    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    public PaymentMethod VerifyOrAddPaymentMethod(string cardNumber, string securityNumber, string cardHolderName, DateTime expiration,
        int cardTypeId)
    {
        PaymentMethod paymentMethod;
        var existingPayment = PaymentMethods
            .SingleOrDefault(p => p.CardTypeId == cardTypeId
                                  && p.CardNumber == cardNumber
                                  && p.Expiration == expiration);

        if (existingPayment != null)
        {
            paymentMethod = existingPayment;
        }
        else
        {
            var payment = new PaymentMethod(
                cardNumber, 
                securityNumber,
                cardHolderName,
                expiration,
                cardTypeId,
                $"Payment Method on {DateTime.UtcNow}"
            );
            
            _paymentMethods.Add(payment);

            paymentMethod = payment;
        }

        return paymentMethod;
    }
}
